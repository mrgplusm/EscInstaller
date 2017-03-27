#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Peqpresetnames : ItemtoDownload
    {
        public Peqpresetnames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Peq preset names";

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