#region

using System.Threading.Tasks;
using Common;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication.downloadItems;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class PresetNames : EepromToDownload
    {
        public PresetNames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Preset Names";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.PresetNames;

        protected override void Done()
        {
            var ps = new UpdatePresetNames(Main);
            ps.SetInOutputNames();
        }
    }
}