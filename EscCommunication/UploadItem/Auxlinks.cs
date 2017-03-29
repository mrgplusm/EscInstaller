#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Auxlinks : DownloadData
    {
        public Auxlinks(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Auxlinks";

        protected override Task Function
        {
            get
            {
                var p = new AuxLinker(Main.DataModel);
                return p.SetAuxLink(ProgressFactory());
            }
        }
    }
}