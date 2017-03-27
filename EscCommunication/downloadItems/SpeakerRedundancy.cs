#region

using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class SpeakerRedundancy : EepromToDownload
    {
        public SpeakerRedundancy(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Redundancy Data";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.SpeakerRedundancy;

        protected override void Done()
        {
            var r = new EepromRedundancyData(Main);
            r.SetRedundancyData();
        }
    }
}