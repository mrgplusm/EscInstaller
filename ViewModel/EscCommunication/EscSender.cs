using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings;
using EscInstaller.ViewModel.Settings.Peq;

namespace EscInstaller.ViewModel.EscCommunication
{
    public class EscSender
    {
        private readonly MainUnitViewModel _main;

        public EscSender(MainUnitViewModel main)
        {
            _main = main;
            _flows = _main.DataModel.Cards.OfType<CardModel>().SelectMany(o => o.Flows).ToList();
        }


        private readonly List<FlowModel> _flows;



        /// <summary>
        /// List of gain sliders (input, output, pagegain)
        /// </summary>        
        /// <returns></returns>
        private IEnumerable<SetGainSlider> GainSliders()
        {

            //send flow data
            foreach (var result in _main.DataModel.Cards.SelectMany(o => o.Flows)
                .Where(result => result.Id < GenericMethods.StartCountFrom))
            {
                yield return new SetGainSlider(result.Id, result.InputSlider, SliderType.Input);
                yield return new SetGainSlider(result.Id, result.PageGain, SliderType.Page);
                yield return new SetGainSlider(result.Id, result.OutputGain, SliderType.Output);
            }
        }

        /// <summary>
        /// tonecontrol blocks (flow 0-4, card 0)
        /// </summary>        
        /// <returns>packages for sending tone control</returns>
        public async void SetToneControls()
        {
            _toneControlPackages = 0;
            var list = new List<SetToneControl>(


            _main.DataModel.Cards.First().Flows
                .Select(flow => new SetToneControl(DspCoefficients.GetToneControl(flow.Bass, flow.Treble), flow.Id)));

            CommunicationViewModel.AddData(list);

            foreach (var setToneControl in list)
            {

                await setToneControl.WaitAsync();
                OnToneControlSend(new DownloadProgressEventArgs()
                {
                    Total = 4,
                    Progress = ++_toneControlPackages
                });
            }
        }

        private volatile int _toneControlPackages;

        public event EventHandler<DownloadProgressEventArgs> ToneControlSend;

        protected virtual void OnToneControlSend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = ToneControlSend;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// List of in and output names
        /// </summary>        
        /// <returns></returns>
        public async void SetInAndOutputNames()
        {
            _inOutputNamesPackages = 0;

            var list = new List<NameUpdate>(
                _flows.Select(result => new[]
                {
                    new NameUpdate(result.Id, result.NameOfInput, NameType.Input),
                    new NameUpdate(result.Id, result.NameOfOutput, NameType.Output),
                }).SelectMany(io => io));

            CommunicationViewModel.AddData(list);

            foreach (var update in list)
            {
                await update.WaitAsync();
                OnInOutputNamesSend(new DownloadProgressEventArgs()
                {
                    Progress = ++_inOutputNamesPackages,
                    Total = 24
                });
            }
        }

        private volatile int _inOutputNamesPackages;

        public event EventHandler<DownloadProgressEventArgs> InOutputNamesSend;

        protected virtual void OnInOutputNamesSend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = InOutputNamesSend;
            if (handler != null) handler(this, e);
        }

        public async void PeqNames()
        {
            _peqDataCount = 0;
            var list = _main.SpeakerDataModels.Where(t => t.SpeakerPeqType != SpeakerPeqType.BiquadsMic)
                .Select(n => new PresetNameUpdate(_main.Id, n.SpeakerName, n.Id)).ToList();

            CommunicationViewModel.AddData(list);

            foreach (var nameUpdate in list)
            {
                await nameUpdate.WaitAsync();
                OnPeqNamesSend(new DownloadProgressEventArgs() { Progress = ++_peqNamesCount, Total = 15 });
            }
        }

        private volatile int _peqNamesCount;
        public event EventHandler<DownloadProgressEventArgs> PeqNamesSend;

        protected virtual void OnPeqNamesSend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = PeqNamesSend;
            if (handler != null) handler(this, e);
        }

        private volatile int _linkDemuxPackages;
        public async void SetLinkDemuxers()
        {
            _linkDemuxPackages = 0;
            var q = _flows.Skip(1).Select(result => new SetLinkDemux(result.Id, result.Path)).ToArray();
            CommunicationViewModel.AddData(q);

            foreach (var setLinkDemux in q)
            {
                await setLinkDemux.WaitAsync();

                OnLinkDemuxSend(new DownloadProgressEventArgs()
                {
                    Progress = ++_linkDemuxPackages,
                    Total = 11
                });
            }
        }

        public event EventHandler<DownloadProgressEventArgs> LinkDemuxSend;

        protected virtual void OnLinkDemuxSend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = LinkDemuxSend;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// List of input deviation settings which aren't set to 0
        /// </summary>        
        /// <returns></returns>
        //todo: implement in batch
        public IEnumerable<IDispatchData> InputDeviations()
        {
            foreach (var result in _flows)
            {
                if (result.DeviationHp != 0)
                    yield return new SetDeviation(result.Id, result.DeviationHp, false);
                if (result.DeviationLp != 0)
                    yield return new SetDeviation(result.Id, result.DeviationLp, true);
            }
        }

        private volatile int _delayPackages;
        public async void SetDelaySettings()
        {
            _delayPackages = 0;
            var q = new[] 
            { 
                new SetDelay(1, _main.DataModel.DelayMilliseconds1, _main.Id),
                new SetDelay(2, _main.DataModel.DelayMilliseconds2, _main.Id)
            };

            CommunicationViewModel.AddData(q);

            foreach (var dispatchData in q)
            {
                await dispatchData.WaitAsync();

                OnDelaySend(new DownloadProgressEventArgs { Total = 2, Progress = ++_delayPackages });
            }
            //todo: chain delays
        }

        public event EventHandler<DownloadProgressEventArgs> DelaySend;

        protected virtual void OnDelaySend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = DelaySend;
            if (handler != null) handler(this, e);
        }


        public async void SetInputSensitivity()
        {
            //sensitivity
            _inputSensitivityPackages = 0;
            bool b;
            if (!Boolean.TryParse(LibraryData.Settings["InputsensitivityIsEnabled"], out b) || !b) return;
            if (_main.DataModel.InputSensitivity == null) return;

            var lst =
                _main.DataModel.InputSensitivity.Select((x, y) => new SetInputSensitivity(_main.Id * 12 + y, x)).ToArray();

            CommunicationViewModel.AddData(lst);

            foreach (var sensitivity in lst)
            {

                await sensitivity.WaitAsync();
                OnInputSensitivitySend(new DownloadProgressEventArgs
                    {
                        Total = 12,
                        Progress = ++_inputSensitivityPackages
                    });
            }

        }

        private volatile int _inputSensitivityPackages;

        public event EventHandler<DownloadProgressEventArgs> InputSensitivitySend;

        protected virtual void OnInputSensitivitySend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = InputSensitivitySend;
            if (handler != null) handler(this, e);
        }


        public async void SetMatrixSelections()
        {
            var matrixBlocks = 0;

            var list = Enumerable.Range(0, 18).Select(i =>

             new RoutingTable(Enumerable.Range(i * 12, 12).ToArray(), _main.Id,
                    LibraryData.FuturamaSys.MatrixSelection)).ToArray();

            CommunicationViewModel.AddData(list);

            foreach (var table in list)
            {
                await table.WaitAsync();

                Interlocked.Increment(ref matrixBlocks);
                OnMatrixSendComplete(new DownloadProgressEventArgs()
                {
                    Progress = ++matrixBlocks,
                    Total = 18
                });
            }

        }



        public event EventHandler<DownloadProgressEventArgs> MatrixSendComplete;

        protected virtual void OnMatrixSendComplete(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = MatrixSendComplete;
            if (handler != null) handler(this, e);
        }


        public async void SetAuxLink()
        {
            _auxLinks = 0;
            var t = _main.DataModel.Cards.OfType<CardModel>().Select(result => AuxLinkOption.SetAuxLink(_main.Id, result)).ToArray();

            CommunicationViewModel.AddData(t);

            foreach (var dispatchData in t)
            {
                await dispatchData.WaitAsync();

                OnAuxLinks(new DownloadProgressEventArgs { Progress = ++_auxLinks, Total = 3 });

            }
        }

        private volatile int _auxLinks;

        public event EventHandler<DownloadProgressEventArgs> AuxLinks;

        protected virtual void OnAuxLinks(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = AuxLinks;
            if (handler != null) handler(this, e);
        }

        private volatile int _emergencySliderPackages;
        public async void SetSliders()
        {
            _emergencySliderPackages = 0;
            var r = _main.DataModel.Cards.OfType<ExtensionCardModel>().First()
                .Flows.Select(flow => new SetGainSlider(flow.Id, (int)flow.InputSlider, SliderType.Input)).Concat(GainSliders()).ToArray();
            CommunicationViewModel.AddData(r);

            var total = r.Length;

            foreach (var setGainSlider in r)
            {
                await setGainSlider.WaitAsync();
                OnEmergencySlidersSend(new DownloadProgressEventArgs() { Progress = ++_emergencySliderPackages, Total = total });

            }
        }

        public event EventHandler<DownloadProgressEventArgs> EmergencySlidersSend;

        protected virtual void OnEmergencySlidersSend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = EmergencySlidersSend;
            if (handler != null) handler(this, e);
        }     

        /// <summary>
        /// new in 10.7 add timestamp to verify design.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDispatchData> TimeStamp()
        {
            long timestamp = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;

            var data = BitConverter.GetBytes(timestamp);
            _main.DataModel.TimeStampWrittenToEsc = timestamp;

            yield return new SetE2PromExt(_main.Id, data, McuDat.DesignTimestamp);
        }


        /// <summary>
        /// Message selection
        /// </summary>
        /// <returns></returns>
        public async void SetMessageData()
        {
            var z = new SetMessageSelection(
                LibraryData.FuturamaSys.Messages,
                LibraryData.FuturamaSys.PreannAlrm1,
                LibraryData.FuturamaSys.PreannAlrm2,
                LibraryData.FuturamaSys.PreannFp,
                LibraryData.FuturamaSys.PreannEvac,
                LibraryData.FuturamaSys.PreannFds,
                LibraryData.FuturamaSys.PreannExt,
                _main.Id);

            CommunicationViewModel.AddData(z);

            await z.WaitAsync();
            OnMessageSelectionSend(new DownloadProgressEventArgs() { Progress = 1, Total = 1 });
        }

        public event EventHandler<DownloadProgressEventArgs> MessageSelectionSend;

        protected virtual void OnMessageSelectionSend(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = MessageSelectionSend;
            if (handler != null) handler(this, e);
        }

        private IEnumerable<IDispatchData> GetPresetData(int flowOffset, SpeakerPeqType type)
        {
            return _main.SpeakerDataModels.Where(s => s.SpeakerPeqType == type)
                .Select((speakerDataModel, flow) => new SpeakerLogicForFlow(speakerDataModel, flow + flowOffset))
                .SelectMany(sp => sp.TotalSpeakerData());
        }        

        /// <summary>
        /// //aux &&
        /// redundancy data &&
        /// peq data &&
        /// EQpreset names
        /// </summary>        
        public IEnumerable<IDispatchData> GetTotalPresetData()
        {
            var flowOffsets = new Dictionary<SpeakerPeqType, int>
            {
                {SpeakerPeqType.BiquadsPreset, _main.Id*12},
                {SpeakerPeqType.BiquadsAux, _main.Id * 12},
                {SpeakerPeqType.BiquadsMic, 2 + _main.Id * 5 + GenericMethods.StartCountFrom},
            };

            return flowOffsets.SelectMany(offset => GetPresetData(offset.Value, offset.Key));
        }

        public async void SetSpeakerPresetData(List<IDispatchData> dataToSend)
        {
            var total = dataToSend.Count;
            foreach (var data in dataToSend)
            {
                CommunicationViewModel.AddData(data);
                await data.WaitAsync();
                OnSpeakerPeqDownloaded(new DownloadProgressEventArgs()
                {
                    Progress = ++_peqDataCount,
                    Total = total
                });
            }
        }

        private volatile int _peqDataCount;

        public event EventHandler<DownloadProgressEventArgs> SpeakerPeqDownloaded;

        protected virtual void OnSpeakerPeqDownloaded(DownloadProgressEventArgs e)
        {
            EventHandler<DownloadProgressEventArgs> handler = SpeakerPeqDownloaded;
            if (handler != null) handler(this, e);
        }
    }

    public class DownloadProgressEventArgs : EventArgs
    {
        public int Total { get; set; }
        public int Progress { get; set; }
    }

    public class DownloadEepromEventArgs : DownloadProgressEventArgs
    {
        public E2PromArea Area { get; set; }
    }
}