using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Commodules;
using Common.Converters;
using Common.Model;
using EscInstaller.ViewModel.Settings;

namespace EscInstaller.ViewModel.EscCommunication
{
    /// <summary>
    /// Class is used to apply all eeprom values to currently open designfile
    /// </summary>
    internal class EepromDataHandler
    {
        private readonly MainUnitViewModel _main;

        public EepromDataHandler(MainUnitViewModel main)
        {
            _main = main;
        }

        public void ApplyEepromDataOnDesign(object sender, DownloadEepromEventArgs e)
        {
            if (e.Progress < e.Total) return;

            switch (e.Area)
            {
                case E2PromArea.SpeakerRedundancy:
                    SetRedundancyData();
                    break;
                case E2PromArea.DspMirror:
                    ApplyDspValues();
                    break;
                case E2PromArea.KreisInstall:
                    SetKreisData();
                    break;
                case E2PromArea.RoutingTable:
                    SetRoutingTableValues();
                    break;
                case E2PromArea.PresetNames:
                    SetInOutputNames();
                    break;
                case E2PromArea.InstalledPanels:
                    InstalledPanelsHandler();
                    break;
            }
        }

        public event EventHandler KreisUpdated;

        protected virtual void OnKreisUpdated()
        {
            EventHandler handler = KreisUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void SetKreisData()
        {
            OnKreisUpdated();            
        }

        private void ApplyDspValues()
        {
            SetToneControl();
            SetAuxLinks();
            
            //todo: chain delays
            _main.DataModel.DelayMilliseconds1 = GetDelay.DelayValue(_main.DspCopy, 1);
            _main.DataModel.DelayMilliseconds2 = GetDelay.DelayValue(_main.DspCopy, 2);
            GetExternalInputSliders();
            ApplySliderValues();
            OnDspMirrorUpdated();
        }

        public event EventHandler DspMirrorUpdated;

        protected virtual void OnDspMirrorUpdated()
        {
            EventHandler handler = DspMirrorUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void ApplySliderValues()
        {

            foreach (var result in _main.DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                SetSliders(result);
            }

            foreach (var c in _main.DataModel.Cards.OfType<CardModel>())
            {
                c.AuxGainSlider = GetGainSlider.DbValue(_main.DspCopy, SliderType.Auxiliary, _main.Id * 12 + c.Id * 4);
            }

        }
        
        public event EventHandler EepromDataUpdated;

        protected virtual void OnEepromDataUpdated()
        {
            EventHandler handler = EepromDataUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        private void SetRedundancyData()
        {
            foreach (var speaker in _main.DataModel.SpeakerDataModels)
            {
                SetRedundantData(speaker, _main.DataModel.DspCopy);
            }
            OnSpeakerRedundancyDataUpdated();
        }

        public event EventHandler SpeakerRedundancyDataUpdated;

        protected virtual void OnSpeakerRedundancyDataUpdated()
        {
            EventHandler handler = SpeakerRedundancyDataUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        /// <summary>
        /// Resolve all redundand data for one speaker
        /// 
        /// </summary>
        /// <param name="speaker"></param>
        /// <param name="dspCopy"></param>
        public void SetRedundantData(SpeakerDataModel speaker, List<byte> dspCopy)
        {
            speaker.ParseRedundancyData(dspCopy);                       
        }        

        /// <summary>
        /// Put the dsp cache into readable names 
        /// </summary>        
        private void SetInOutputNames()
        {
            var names = Names(_main.DspCopy).ToArray();
            var i = 0;
            foreach (var flow in _main.DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                flow.NameOfInput = names[i].Trim();
                flow.NameOfOutput = names[i + 12].Trim();

                i++;
            }
            i = 0;
            foreach (var speakerDataModel in _main.SpeakerDataModels.Where(s=> s.SpeakerPeqType != SpeakerPeqType.BiquadsMic))
            {
                speakerDataModel.SpeakerName = names[i++ + 24].Trim();
            }
            OnPresetNamesUpdated();
        }

        public event EventHandler PresetNamesUpdated;

        protected virtual void OnPresetNamesUpdated()
        {
            EventHandler handler = PresetNamesUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///   Reconstruct the system names by splitting byte array
        /// </summary>
        /// <param name="data"> 16 dividable byte bytestream (624) </param>
        /// <returns> 00-11 input names (12) 12-23 output names (12) 24-39 preset names (15) </returns>
        private static IEnumerable<string> Names(IEnumerable<byte> data)
        {
            var namesArray = data.Skip(0xA000).Take(16 * 39).ToArray();
            for (var i = 0; i < namesArray.Length; i += 16)
            {
                yield return Encoding.Default.GetString(namesArray.Skip(i).Take(13)
                    .Select(n => (n < 32 || n > 126) ? (byte)0x20 : n ).ToArray());
            }
        }

        private void SetSliders(FlowModel flow)
        {
            if (flow.Id > GenericMethods.StartCountFrom)
            {
                flow.InputSlider = GetGainSlider.DbValue(_main.DspCopy, SliderType.Input,
                    (flow.Id - GenericMethods.StartCountFrom) % 5);
            }

            //set pilotTone frequencies
            var pilot = _main.DspCopy.Skip(0xF113 + 72 * flow.Id + 60).First();
            flow.PilotFrequency = PilotToneConverter.PilotFreqDict.ContainsKey(pilot)
                ? PilotToneConverter.PilotFreqDict[pilot]
                : 0;

            flow.InputSlider = GetGainSlider.DbValue(_main.DspCopy, SliderType.Input, flow.Id);
            flow.PageGain = GetGainSlider.DbValue(_main.DspCopy, SliderType.Page, flow.Id);
            flow.OutputGain = GetGainSlider.DbValue(_main.DspCopy, SliderType.Output, flow.Id);



            //demux link
            flow.Path = DspCoefficients.Link(_main.DspCopy, flow.Id);
        }

        private void SetToneControl()
        {
            foreach (var flow in _main.DataModel.Cards.First().Flows)
            {
                var tb = DspCoefficients.BassTreble(_main.DspCopy, flow.Id);
                flow.Bass = tb.Item1;
                flow.Treble = tb.Item2;
            }
        }

        private void SetAuxLinks()
        {
            foreach (var card in _main.DataModel.Cards.OfType<CardModel>())
            {
                card.LinkedChannel = AuxLink.GetAuxLink(card.Id, _main.DspCopy);
            }
        }

        private void GetExternalInputSliders()
        {
            //only get monitor slider for the first card as it is per mainunit
            _main.MonitorSlider =
                GetGainSlider.DbValue(_main.DspCopy, SliderType.Monitoring, 0);
            var cardFlows = _main.DataModel.Cards.OfType<ExtensionCardModel>().First().Flows.OrderBy(i => i.Id).ToArray();

            foreach (var flow in cardFlows)
                flow.InputSlider = GetGainSlider.DbValue(_main.DspCopy, SliderType.Input, flow.Id);
        }


        /// <summary>
        /// 4.3 routing table receive.
        /// For some reason, each button pair of 4 bytes has two bytes extra, 
        /// set to ff whose are not specified in the protocol.
        /// 
        /// BroadcastType
        /// Button Id,
        /// Flow Id        
        /// </summary>
        private void SetRoutingTableValues()
        {
            if (!LibraryData.SystemIsOpen) return;

            var theData = _main.DspCopy.Skip(McuDat.RoutingTable).Take(0x528).ToArray();

            for (var button = 0; button < 216; button++)
            {
                //each button consumes 6 bytes
                var alarm1 = new BitArray(new[] { theData[button * 6 + 1], theData[button * 6] });
                var alarm2 = new BitArray(new[] { theData[button * 6 + 3], theData[button * 6 + 2] });

                for (var zone = 0; zone < 12; zone++)
                {
                    var q = new MatrixCell

                    {
                        BroadcastMessage = alarm1[zone] ? BroadCastMessage.Alarm1
                                         : alarm2[zone] ? BroadCastMessage.Alarm2 : BroadCastMessage.None,
                        ButtonId = button,
                        FlowId = zone + (_main.Id * 12)
                    };


                    LibraryData.FuturamaSys.MatrixSelection.Remove(q);
                    LibraryData.FuturamaSys.MatrixSelection.Add(q);
                }
            }
            OnRoutingTableUpdated();
        }

        public event EventHandler RoutingTableUpdated;

        protected virtual void OnRoutingTableUpdated()
        {
            EventHandler handler = RoutingTableUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        ///Start address of the first struct:   0xE800
        ///INT8U   u8_installedFP[32];                               
        ///INT8U   u8_installedEP[32];
        ///INT8U   u8_installedFDS[32];
        /// </summary>        
        private void InstalledPanelsHandler()
        {
            var types = new[]
            {
                new BitArray(_main.DspCopy.Skip(McuDat.InstalledPanels).Take(32).Reverse().ToArray()), //fp
                new BitArray(_main.DspCopy.Skip(McuDat.InstalledPanels).Skip(32).Take(32).Reverse().ToArray()), //ep
                new BitArray(_main.DspCopy.Skip(McuDat.InstalledPanels).Skip(64).Take(32).Reverse().ToArray()) //fds
            };

            for (var type = 0; type < 3; type++)
            {
                for (var id = 0; id < 32; id++)
                {
                    var pm = _main.DataModel.AttachedPanelsBus2.FirstOrDefault(se => se.Id == id && se.PanelType == (PanelType)type);
                    if (pm == null) continue;
                    pm.IsInstalled = types[type][id];
                }
            }
        }
    }
}