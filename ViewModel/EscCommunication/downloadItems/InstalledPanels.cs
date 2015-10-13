using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class InstalledPanels : EepromToDownload
    {
        public InstalledPanels(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Installed Panels"; }
        }

        public override Task Function
        {
            get { return DownloaderFactory().GetEeprom(ProgressFactory()); }
        }

        protected override E2PromArea Area
        {
            get { return E2PromArea.InstalledPanels; }
        }

        protected override void Done()
        {
            var ph = new AttachedPanelUpdater(Main);
            ph.Update();
        }
    }
}