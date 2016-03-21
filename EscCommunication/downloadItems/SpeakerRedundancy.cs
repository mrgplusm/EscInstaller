#region

using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class SpeakerRedundancy : EepromToDownload
    {
        public SpeakerRedundancy(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Redundancy Data"; }
        }

        public override Task Function
        {
            get { return DownloaderFactory().GetEeprom(ProgressFactory()); }
        }

        protected override E2PromArea Area
        {
            get { return E2PromArea.SpeakerRedundancy; }
        }

        protected override void Done()
        {
            var r = new EepromRedundancyData(Main);
            r.SetRedundancyData();
        }
    }
}