#region

using System.Threading.Tasks;
using Common;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class SpeakerRedundancy : EepromToDownload
    {
        public SpeakerRedundancy(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Redundancy Data";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.SpeakerRedundancy;

        protected override void Done()
        {
            var r = new EepromRedundancyData(Main);
            r.SetRedundancyData();
        }
    }
}