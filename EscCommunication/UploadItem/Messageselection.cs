#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Messageselection : DownloadData
    {
        public Messageselection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Message selection";

        protected override Task Function
        {
            get
            {
                var p = new MessageSelector(Main.DataModel);
                return p.SetMessageData(ProgressFactory());
            }
        }
    }
}