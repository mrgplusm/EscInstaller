#region

using System.Linq;
using Common;
using Common.Commodules;
using Common.Converters;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class DspDataUpdater : EepromDataHandler
    {
        public DspDataUpdater(MainUnitViewModel main) : base(main)
        {
        }

        public void SetDspValues()
        {
            SetToneControl();
            SetAuxLinks();

            //todo: chain delays
            Main.DataModel.DelayMilliseconds1 = GetDelay.DelayValue(Main.DspCopy, 1);
            Main.DataModel.DelayMilliseconds2 = GetDelay.DelayValue(Main.DspCopy, 2);
            GetExternalInputSliders();
            ApplySliderValues();
            Main.OnDspMirrorUpdated();
        }

        private void ApplySliderValues()
        {
            foreach (var result in Main.DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                SetSliders(result);
            }

            foreach (var c in Main.DataModel.Cards.OfType<CardModel>())
            {
                c.AuxGainSlider = GetGainSlider.DbValue(Main.DspCopy, SliderType.Auxiliary, Main.Id*12 + c.Id*4);
            }
        }

        private void GetExternalInputSliders()
        {
            //only get monitor slider for the first card as it is per mainunit
            Main.MonitorSlider =
                GetGainSlider.DbValue(Main.DspCopy, SliderType.Monitoring, 0);
            var cardFlows = Main.DataModel.Cards.OfType<ExtensionCardModel>().First().Flows.OrderBy(i => i.Id).ToArray();

            foreach (var flow in cardFlows)
                flow.InputSlider = GetGainSlider.DbValue(Main.DspCopy, SliderType.Input, flow.Id);
        }

        private void SetAuxLinks()
        {
            foreach (var card in Main.DataModel.Cards.OfType<CardModel>())
            {
                card.LinkedChannel = AuxLink.GetAuxLink(card.Id, Main.DspCopy);
            }
        }

        private void SetToneControl()
        {
            foreach (var flow in Main.DataModel.Cards.First().Flows)
            {
                var tb = DspCoefficients.BassTreble(Main.DspCopy, flow.Id);
                flow.Bass = tb.Item1;
                flow.Treble = tb.Item2;
            }
        }

        private void SetSliders(FlowModel flow)
        {
            if (flow.Id > GenericMethods.StartCountFrom)
            {
                flow.InputSlider = GetGainSlider.DbValue(Main.DspCopy, SliderType.Input,
                    (flow.Id - GenericMethods.StartCountFrom)%5);
            }

            //set pilotTone frequencies
            var pilot = Main.DspCopy.Skip(0xF113 + 72*flow.Id + 60).First();
            flow.PilotFrequency = PilotToneConverter.PilotFreqDict.ContainsKey(pilot)
                ? PilotToneConverter.PilotFreqDict[pilot]
                : 0;

            flow.InputSlider = GetGainSlider.DbValue(Main.DspCopy, SliderType.Input, flow.Id);
            flow.PageGain = GetGainSlider.DbValue(Main.DspCopy, SliderType.Page, flow.Id);
            flow.OutputGain = GetGainSlider.DbValue(Main.DspCopy, SliderType.Output, flow.Id);


            //demux link
            flow.Path = DspCoefficients.Link(Main.DspCopy, flow.Id);
        }
    }
}