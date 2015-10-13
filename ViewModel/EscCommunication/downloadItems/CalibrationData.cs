using System.Threading.Tasks;
using Common;

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class CalibrationData : EepromToDownload
    {
        public CalibrationData(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Calibration Data"; }
        }

        public override Task Function
        {
            get { return DownloaderFactory().GetEeprom(ProgressFactory()); }
        }

        protected override E2PromArea Area
        {
            get { return E2PromArea.KreisInstall; }
        }

        protected override void Done()
        {
            Main.OnKreisUpdated();
        }
    }
}