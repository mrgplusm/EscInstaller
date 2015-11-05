#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public class AuxLinker : EscLogic
    {
        private volatile int _auxLinks;

        protected internal AuxLinker(MainUnitModel main)
            : base(main)
        {
        }

        public async Task SetAuxLink(IProgress<DownloadProgress> iProgress)
        {
            _auxLinks = 0;
            var t =
                Main.Cards.OfType<CardModel>()
                    .Select(result => AuxLinkOption.SetAuxLink(Main.Id, result))
                    .ToArray();

            CommunicationViewModel.AddData(t);

            foreach (var dispatchData in t)
            {
                await dispatchData.WaitAsync();

                iProgress.Report(new DownloadProgress {Progress = ++_auxLinks, Total = 3});
            }
        }
    }
}