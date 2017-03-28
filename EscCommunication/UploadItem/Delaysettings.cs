#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Delaysettings : ItemtoDownload
    {
        public Delaysettings(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Delay settings";

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