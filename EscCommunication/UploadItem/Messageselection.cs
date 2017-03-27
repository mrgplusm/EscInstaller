#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Messageselection : ItemtoDownload
    {
        public Messageselection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Message selection";

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