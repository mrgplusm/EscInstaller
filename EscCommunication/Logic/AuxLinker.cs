#region

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class AuxLinker : EscLogic
    {
        private volatile int _auxLinks;

        protected internal AuxLinker(MainUnitModel main)
            : base(main)
        {
        }

        public async Task SetAuxLink(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            _auxLinks = 0;
            var t =
                Main.Cards.OfType<CardModel>()
                    .Select(result => AuxLinkOption.SetAuxLink(Main.Id, result))
                    .ToArray();            

            foreach (var auxLink in t)
            {
                CommunicationViewModel.AddData(auxLink);
                if (token.IsCancellationRequested) return;
                await auxLink.WaitAsync();
                iProgress.Report(new DownloadProgress {Progress = ++_auxLinks, Total = 3});
            }
        }
    }
}