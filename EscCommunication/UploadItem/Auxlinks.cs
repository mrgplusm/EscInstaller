#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Auxlinks : ItemtoDownload
    {
        public Auxlinks(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Auxlinks";

        public override Task Function
        {
            get
            {
                var p = new AuxLinker(Main.DataModel);
                return p.SetAuxLink(ProgressFactory());
            }
        }
    }
}