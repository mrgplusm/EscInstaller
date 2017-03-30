#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class NameUpdater : EscLogic
    {
        private readonly List<FlowModel> _flows;
        private volatile int _inOutputNamesPackages;
        private volatile int _peqNamesCount;

        protected internal NameUpdater(MainUnitModel main) : base(main)
        {
            _flows = Main.Cards.OfType<CardModel>().SelectMany(o => o.Flows).ToList();
        }

        /// <summary>
        ///     List of in and output names
        /// </summary>
        /// <returns></returns>
        public async Task SetInAndOutputNames(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            _inOutputNamesPackages = 0;

            var list = new List<NameUpdate>(
                _flows.Select(result => new[]
                {
                    new NameUpdate(result.Id, result.NameOfInput, NameType.Input),
                    new NameUpdate(result.Id, result.NameOfOutput, NameType.Output)
                }).SelectMany(io => io));
            foreach (var nameUpdate in list)
            {
                if (token.IsCancellationRequested) return;
                CommunicationViewModel.AddData(nameUpdate);
            }            

            foreach (var update in list)
            {
                if (token.IsCancellationRequested) return;
                await update.WaitAsync();
                iProgress.Report(new DownloadProgress()
                {
                    Progress = ++_inOutputNamesPackages,
                    Total = 24
                });
            }
        }

        public async Task SetPeqNames(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            var list = Main.SpeakerDataModels.Where(t => t.SpeakerPeqType != SpeakerPeqType.BiquadsMic)
                .Select(n => new PresetNameUpdate(Main.Id, n.SpeakerName, n.Id)).ToList();
                       

            foreach (var nameUpdate in list)
            {
                if (token.IsCancellationRequested) return;
                CommunicationViewModel.AddData(nameUpdate);
                await nameUpdate.WaitAsync();
                iProgress.Report(new DownloadProgress() {Progress = ++_peqNamesCount, Total = 15});
            }
        }
    }
}