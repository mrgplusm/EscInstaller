#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.ViewModel.EscCommunication
{
    internal class EscReceiver
    {
        protected readonly MainUnitViewModel Main;

        protected internal EscReceiver(MainUnitViewModel main)
        {

            Main = main;
            InitializeEepromReceivePackages();
        }

        private void InitializeEepromReceivePackages()
        {
            foreach (var package in Enum.GetValues(typeof(E2PromArea)).Cast<E2PromArea>())
            {
                _eepromReceivePackages.Add(package, 0);
            }
        }

        /// <summary>
        ///   Set the downloaded hardware settigns in the given mainunit
        /// </summary>
        public async void GetHardware()
        {
            var data = new GetInstallTree(Main.Id);
            CommunicationViewModel.AddData(data);

            await data.WaitAsync();

            var sq = data.GetBackupConfig().ToList();
            var sp = data.GetChannels().ToList();

            //set amount of expension cards            
            Main.DataModel.ExpansionCards = Main.DataModel.InputSensitivity.Skip(4)
                .TakeWhile(sense => sense == InputSens.High || sense == InputSens.Low || sense == InputSens.None)
                .Count() >> 2;

            //set backupamps
            var n = 0;
            foreach (var cardBaseModel in Main.DataModel.Cards.OfType<CardModel>().OrderBy(z => z.Id))
            {
                cardBaseModel.AttachedBackupAmps =
                    new BitArray(new[] { sq[n].Item1, sq[n].Item2, sq[n].Item3, sq[n].Item4, sq[n].Item5, sq[n].Item6 });
                n++;
            }

            //loop through cards and channels;
            var id = 0;
            foreach (var result in Main.DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                result.HasAmplifier = sp[id].Item1 || sp[id].Item2 || sp[id].Item3 || sp[id].Item4;
                result.AmplifierOperationMode = result.HasAmplifier ? sp[id].Item6 : AmplifierOperationMode.Unknown;
                result.AttachedChannels = new[] { sp[id].Item1, sp[id].Item2, sp[id].Item3, sp[id].Item4 };
                id++;
            }
            OnHardwareReceived();
        }



        public event EventHandler HardwareReceived;

        protected virtual void OnHardwareReceived()
        {
            EventHandler handler = HardwareReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// get inputsensitivity for each of the 12 channels
        /// </summary>        
        public async void GetSensitivityValues()
        {
            var data = new GetInputSensitivity(Main.Id);

            CommunicationViewModel.AddData(data);
            await data.WaitAsync();

            Main.DataModel.InputSensitivity = data.Flows.ToList();
            OnSensitivityDownloaded();

        }

        public event EventHandler SensitivityDownloaded;

        protected virtual void OnSensitivityDownloaded()
        {
            EventHandler handler = SensitivityDownloaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public async void GetBoseVersion()
        {
            var q = new GetBoseVersion(Main.Id);
            CommunicationViewModel.AddData(q);
            await q.WaitAsync();

            Main.DataModel.BoseVersion = q.BoseVersion;
            OnBoseVersionReceived();
        }

        public event EventHandler BoseVersionReceived;

        protected virtual void OnBoseVersionReceived()
        {
            EventHandler handler = BoseVersionReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public async void GetSdCardMessages()
        {
            var q = new SdCardMessageCount();
            CommunicationViewModel.AddData(q);

            await q.WaitAsync();

            _sdMessagePackages = 0;
            if (LibraryData.FuturamaSys == null) return;
            //clear stored messages
            foreach (var b in LibraryData.FuturamaSys
                .SdFilesB
                .Concat(LibraryData.FuturamaSys.SdFilesA)
                .Where(qq => qq.Position != 0xff))
            {
                b.Name = null;
                //b.AvailableOnSdCard = false;
            }

            //skip 0 and 1(16khz) start at position 2
            for (var i = 2; i < q.CountA; i++)
            {
                GetMessageName(0, i);
            }

            for (var i = 2; i < q.CountB; i++)
            {
                GetMessageName(1, i);
            }
            _messageCount = q.CountA + q.CountB + 1 + TrackShiftCardA + TrackShiftCardB;

            Application.Current.Dispatcher.Invoke(() =>
                OnSdCardMessagesReceived(new DownloadProgressEventArgs
                {
                    Progress = ++_sdMessagePackages,
                    Total = _messageCount,
                }));
        }

        private volatile int _messageCount;
        private volatile int _sdMessagePackages;

        /// <summary>
        /// Occurs when all message names are received from main unit
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> SdCardMessagesReceived;

        protected virtual void OnSdCardMessagesReceived(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = SdCardMessagesReceived;
            if (handler != null) handler(this, e);

        }

        public async void GetButtonProgramming()
        {
            //get button programming first unit
            if (Main.Id != 0) return;

            var s = new GetMessageSelection(Main.Id);

            CommunicationViewModel.AddData(s);

            await s.WaitAsync();

            LibraryData.FuturamaSys.PreannAlrm1 = s.AlarmTrack;
            LibraryData.FuturamaSys.PreannAlrm2 = s.AlertTrack;

            LibraryData.FuturamaSys.PreannEvac = s.PreannounceEP;
            LibraryData.FuturamaSys.PreannExt = s.PreannounceExtAudio;
            LibraryData.FuturamaSys.PreannFds = s.PreannounceFDS;
            LibraryData.FuturamaSys.PreannFp = s.PreannounceFP;

            LibraryData.FuturamaSys.Messages = s.MessageSelectModels;
            Application.Current.Dispatcher.Invoke(OnSdCardPositionsReceived);
        }

        public event EventHandler SdCardPositionsReceived;

        protected virtual void OnSdCardPositionsReceived()
        {
            EventHandler handler = SdCardPositionsReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public async void GetEeprom(E2PromArea area)
        {
            _eepromReceivePackages[area] = 0;
            var ar = McuDat.EepromAreaFactory(area);

            if (Main.DataModel.DspCopy == null)
            {
                Main.DataModel.DspCopy = new List<byte>(Enumerable.Range(0, 131072).Select(n => (byte)0));
            }
            var count = ar.Size / McuDat.BufferSize + 1;

            for (var i = ar.Location; i < ar.Location + ar.Size + McuDat.BufferSize; i += McuDat.BufferSize)
            {
                var s = new GetE2PromExt(Main.Id, McuDat.BufferSize, i);
                CommunicationViewModel.AddData(s);

                await s.WaitAsync();

                UpdateEepromData(s);

                OnEepromReceived(new DownloadEepromEventArgs()
                {
                    Progress = ++_eepromReceivePackages[area],
                    Total = count,
                    Area = area
                });
            }
        }

        private void UpdateEepromData(GetE2PromExt data)
        {
            var arr = data.ReceivedEpromValues.ToArray();
            for (var n = 0; n < arr.Length; n++)
            {
                Main.DspCopy[n + data.StartAddress] = arr[n];
            }
        }

        private readonly Dictionary<E2PromArea, int> _eepromReceivePackages = new Dictionary<E2PromArea, int>();


        public event EventHandler<DownloadEepromEventArgs> EepromReceived;

        protected virtual void OnEepromReceived(DownloadEepromEventArgs e)
        {
            EventHandler<DownloadEepromEventArgs> handler = EepromReceived;
            if (handler != null) handler(this, e);
        }

        const int TrackShiftCardA = -2;
        const int TrackShiftCardB = -2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card">Either 0 or 1</param>
        /// <param name="track">message id to receive</param>
        private async void GetMessageName(int card, int track)
        {
            if (LibraryData.FuturamaSys == null || LibraryData.FuturamaSys.SdFilesB == null ||
                LibraryData.FuturamaSys.SdFilesA == null
                || LibraryData.FuturamaSys.SdFilesA.Count < 0xff
                || LibraryData.FuturamaSys.SdFilesB.Count < 0xff)
            {
                Debug.WriteLine("Couldn't store message names, futuramasys or messagesA/B null");
                return;
            }


            //esc sometimes gives erroneously wrong response. If name contains 16khz, redownload a few times until succes.
            try
            {
                for (var n = 0; n < 3; n++)
                {
                    var download = new SdCardMessageName(card, track);
                    CommunicationViewModel.AddData(download);
                    await download.WaitAsync();

                    if (download.TrackName.ToLower().Contains("16khz"))
                    {
                        Debug.WriteLine("Warning: Track {0} contains 16khz! Re-downloading..", download.TrackNumber);
                        continue;
                    }
                    if (download.CardNumber == 0)
                    {
                        var f = LibraryData.FuturamaSys.SdFilesA[download.TrackNumber + TrackShiftCardA];
                        f.Name = download.TrackName;
                        //f.AvailableOnSdCard = true;
                        return;
                    }
                    else
                    {
                        var f = LibraryData.FuturamaSys.SdFilesB[download.TrackNumber + TrackShiftCardB];
                        f.Name = download.TrackName;
                        //f.AvailableOnSdCard = true;
                        return;
                    }
                }
            }
            finally
            {
                OnSdCardMessagesReceived(new DownloadProgressEventArgs() { Progress = ++_sdMessagePackages, Total = _messageCount });

            }
        }
    }
}