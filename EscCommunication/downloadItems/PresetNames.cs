#region

using System.Threading.Tasks;
using Common;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class PresetNames : EepromToDownload
    {
        public PresetNames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Preset Names";

        protected override Task Function => DownloaderFactory().GetEeprom(ProgressFactory(), Cancellation.Token);

        protected override E2PromArea Area => E2PromArea.PresetNames;

        protected override void Done()
        {
            var ps = new UpdatePresetNames(Main);
            ps.SetInOutputNames();
        }
    }
}