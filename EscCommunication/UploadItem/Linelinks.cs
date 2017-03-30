#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Linelinks : DownloadData
    {
        public Linelinks(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Line links";

        protected override Task Function
        {
            get
            {
                var p = new LineLinkUpdater(Main.DataModel);
                return p.SetLinkDemuxers(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}