#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class SliderUpdater : EscLogic
    {
        private volatile int _emergencySliderPackages;

        protected internal SliderUpdater(MainUnitModel main) : base(main)
        {
        }

        /// <summary>
        ///     List of gain sliders (input, output, pagegain)
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SetGainSlider> GainSliders()
        {
            //send flow data
            foreach (var result in Main.Cards.SelectMany(o => o.Flows)
                .Where(result => result.Id < GenericMethods.StartCountFrom))
            {
                yield return new SetGainSlider(result.Id, result.InputSlider, SliderType.Input);
                yield return new SetGainSlider(result.Id, result.PageGain, SliderType.Page);
                yield return new SetGainSlider(result.Id, result.OutputGain, SliderType.Output);
            }
        }

        public async Task SetSliders(IProgress<DownloadProgress> iProgress)
        {
            _emergencySliderPackages = 0;
            var r = Main.Cards.OfType<ExtensionCardModel>().First()
                .Flows.Select(flow => new SetGainSlider(flow.Id, (int) flow.InputSlider, SliderType.Input))
                .Concat(GainSliders())
                .ToArray();
            CommunicationViewModel.AddData(r);

            var total = r.Length;

            foreach (var setGainSlider in r)
            {
                await setGainSlider.WaitAsync();
                iProgress.Report(new DownloadProgress() {Progress = ++_emergencySliderPackages, Total = total});
            }
        }
    }
}