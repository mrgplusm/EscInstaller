using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class PresetNames : EepromToDownload
    {
        public PresetNames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Preset Names"; }
        }

        public override Task Function
        {
            get { return DownloaderFactory().GetEeprom(ProgressFactory()); }
        }

        protected override E2PromArea Area
        {
            get { return E2PromArea.PresetNames; }
        }

        protected override void Done()
        {
            var ps = new UpdatePresetNames(Main);
            ps.SetInOutputNames();
        }
    }
}