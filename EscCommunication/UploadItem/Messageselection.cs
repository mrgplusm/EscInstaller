#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

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