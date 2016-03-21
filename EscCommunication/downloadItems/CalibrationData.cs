#region

using System.Threading.Tasks;
using Common;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class CalibrationData : EepromToDownload
    {
        public CalibrationData(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Calibration Data";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.KreisInstall;

        protected override void Done()
        {
            Main.OnKreisUpdated();
        }
    }
}