#region

using System.Threading.Tasks;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class SdMessages : ItemtoDownload
    {
        public SdMessages(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Sd messages";

        public override Task Function
        {
            get
            {
                var s = new SdMessageReceive(Main.DataModel);
                return s.GetSdCardMessages(ProgressFactory());
            }
        }

        protected override void Done()
        {
            Main.OnSdCardMessagesReceived();
        }
    }
}