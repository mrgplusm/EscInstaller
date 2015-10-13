using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class Hardware : ItemtoDownload
    {
        public Hardware(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Hardware"; }
        }

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