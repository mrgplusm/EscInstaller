#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class Peqpresetnames : ItemtoDownload
    {
        public Peqpresetnames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Peq preset names"; }
        }

        public override Task Function
        {
            get
            {
                var p = new NameUpdater(Main.DataModel);
                return p.SetPeqNames(ProgressFactory());
            }
        }
    }
}