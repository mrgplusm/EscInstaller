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

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public class ToneControlUpdater : EscLogic
    {
        private volatile int _toneControlPackages;

        protected internal ToneControlUpdater(MainUnitModel main) : base(main)
        {
        }

        /// <summary>
        ///     tonecontrol blocks (flow 0-4, card 0)
        /// </summary>
        /// <returns>packages for sending tone control</returns>
        public async Task SetToneControls(IProgress<DownloadProgress> iProgress)
        {
            _toneControlPackages = 0;
            var list = new List<SetToneControl>(
                Main.Cards.First().Flows
                    .Select(flow => new SetToneControl(DspCoefficients.GetToneControl(flow.Bass, flow.Treble), flow.Id)));

            CommunicationViewModel.AddData(list);

            foreach (var setToneControl in list)
            {
                await setToneControl.WaitAsync();
                iProgress.Report(new DownloadProgress()
                {
                    Total = 4,
                    Progress = ++_toneControlPackages
                });
            }
        }
    }
}