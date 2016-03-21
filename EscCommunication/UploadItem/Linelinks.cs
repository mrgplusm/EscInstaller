#region

using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class Linelinks : ItemtoDownload
    {
        public Linelinks(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Line links"; }
        }

        public override Task Function
        {
            get
            {
                var p = new LineLinkUpdater(Main.DataModel);
                return p.SetLinkDemuxers(ProgressFactory());
            }
        }
    }
}