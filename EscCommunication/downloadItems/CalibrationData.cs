#region

using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class CalibrationData : EepromToDownload
    {
        public CalibrationData(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Calibration Data";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.KreisInstall;

        protected override void Done()
        {
            Main.OnKreisUpdated();
        }
    }
}