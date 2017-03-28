#region

using System.Threading;
using System.Threading.Tasks;
using Common;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class InstalledPanels : EepromToDownload
    {
        public InstalledPanels(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Installed Panels";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory(), Cancellation.Token);

        protected override E2PromArea Area => E2PromArea.InstalledPanels;

        protected override void Done()
        {
            var ph = new AttachedPanelUpdater(Main);
            ph.Update();
        }
    }
}