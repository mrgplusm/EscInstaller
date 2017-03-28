#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Peqpresetnames : DownloadData
    {
        public Peqpresetnames(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Peq preset names";

        public override Task Function
        {
            get
            {
                var p = new NameUpdater(Main.DataModel);
                return p.SetPeqNames(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}