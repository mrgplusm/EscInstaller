#region

using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    public class InOutNames : ItemtoDownload
    {
        public InOutNames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Send in- output names"; }
        }

        public override Task Function
        {
            get
            {
                var s = new NameUpdater(Main.DataModel);
                return s.SetInAndOutputNames(ProgressFactory());
            }
        }
    }
}