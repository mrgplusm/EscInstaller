using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class Messageselection : ItemtoDownload
    {
        public Messageselection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Message selection"; }
        }

        public override Task Function
        {
            get
            {
                var p = new MessageSelector(Main.DataModel);
                return p.SetMessageData(ProgressFactory());
            }
        }
    }
}