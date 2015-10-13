using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class Auxlinks : ItemtoDownload
    {
        public Auxlinks(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Auxlinks"; }
        }

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