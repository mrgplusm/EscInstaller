#region

using System.Threading;
using System.Threading.Tasks;
using Common;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class MatrixSelection : EepromToDownload
    {
        public MatrixSelection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Matrix selection";

        protected override Task Function => DownloaderFactory().GetEeprom(ProgressFactory(), Cancellation.Token);

        protected override E2PromArea Area => E2PromArea.RoutingTable;

        protected override void Done()
        {
            var rt = new UpdateRoutingTable(Main);
            rt.SetRoutingTableValues();
        }
    }
}