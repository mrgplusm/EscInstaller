#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class Hardware : ItemtoDownload
    {
        public Hardware(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Hardware";

        public override Task Function
        {
            get
            {
                var s = new HardwareReceive(Main.DataModel);
                return s.GetHardware(ProgressFactory());
            }
        }

        protected override void Done()
        {
            Main.UpdateHardware();
        }
    }
}