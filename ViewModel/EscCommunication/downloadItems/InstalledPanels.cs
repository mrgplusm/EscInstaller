#region

using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class InstalledPanels : EepromToDownload
    {
        public InstalledPanels(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Installed Panels";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.InstalledPanels;

        protected override void Done()
        {
            var ph = new AttachedPanelUpdater(Main);
            ph.Update();
        }
    }
}