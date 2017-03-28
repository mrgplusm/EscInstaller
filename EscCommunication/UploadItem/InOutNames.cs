#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    public class InOutNames : DownloadData
    {
        public InOutNames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Send in- output names";

        public override Task Function
        {
            get
            {
                var s = new NameUpdater(Main.DataModel);
                return s.SetInAndOutputNames(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}