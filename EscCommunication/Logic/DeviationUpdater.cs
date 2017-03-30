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
    public class LineLinkUpdater : EscLogic
    {
        private readonly List<FlowModel> _flows;
        private volatile int _linkDemuxPackages;

        protected internal LineLinkUpdater(MainUnitModel main)
            : base(main)
        {
            _flows = Main.Cards.OfType<CardModel>().SelectMany(o => o.Flows).ToList();
        }

        public async Task SetLinkDemuxers(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            _linkDemuxPackages = 0;
            var q = _flows.Skip(1).Select(result => new SetLinkDemux(result.Id, result.Path)).ToArray();

            foreach (var setLinkDemux in q)
            {
                if (token.IsCancellationRequested) return;
                CommunicationViewModel.AddData(setLinkDemux);
            }
            

            foreach (var setLinkDemux in q)
            {
                if (token.IsCancellationRequested) return;
                await setLinkDemux.WaitAsync();
                iProgress.Report(new DownloadProgress()
                {
                    Progress = ++_linkDemuxPackages,
                    Total = 11
                });
            }
        }
    }
}