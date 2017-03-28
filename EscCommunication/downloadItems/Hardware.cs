#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class Hardware : DownloadData
    {
        public Hardware(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Hardware";

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