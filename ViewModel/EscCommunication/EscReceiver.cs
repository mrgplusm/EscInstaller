#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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
            OnSdCardPositionsReceived();
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


        /// <summary>
        /// skip 0 and 1(16khz) start at position 2
        /// </summary>
        private const int SdFirstMessageToDownload = 2;

        /// <summary>
        /// clear stored messages
        /// </summary>
        private static void ClearMessages()
        {
            if (LibraryData.FuturamaSys == null) return;

            foreach (var b in LibraryData.FuturamaSys
                .SdFilesB
                .Concat(LibraryData.FuturamaSys.SdFilesA)
                .Where(qq => qq.Position != 0xff))
            {
                b.Name = null;
            }
        }

        private async Task<int[]> GetSdMessageCount()
        {
            var q = new SdCardMessageCount();
            CommunicationViewModel.AddData(q);

            await q.WaitAsync();

            _messageCount = q.CountA + q.CountB + 1 + (SdCardMessageName.TrackShift * 2);
            return new[] { q.CountA, q.CountB };
        }

        public async void GetSdCardMessages()
        {
            ClearMessages();
            var lists = await GetSdMessageCount();

            foreach (var list in lists.Select((count, n) => new { count, n }))
            {
                for (var i = SdFirstMessageToDownload; i < list.count; i++)
                {
                    var s = new SdMessageNameLoader(list.n, list.count);
                    await s.GetMessageName();
                    OnSdCardMessagesReceived(new DownloadProgressEventArgs() { Progress = ++_sdMessagePackages, Total = _messageCount });
                }
            }

            OnSdCardMessagesReceived(new DownloadProgressEventArgs
            {
                Progress = ++_sdMessagePackages,
                Total = _messageCount,
            });
        }

        private volatile int _messageCount;
        private volatile int _sdMessagePackages;
    }
}