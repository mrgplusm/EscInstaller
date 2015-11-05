#region

using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class Delaysettings : ItemtoDownload
    {
        public Delaysettings(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Delay settings"; }
        }

        public override Task Function
        {
            get
            {
                var p = new DelayUpdater(Main.DataModel);
                return p.SetDelaySettings(ProgressFactory());
            }
        }
    }
}