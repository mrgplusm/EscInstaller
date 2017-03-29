#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class SdMessages : DownloadData
    {
        public SdMessages(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Sd messages";

        protected override Task Function
        {
            get
            {
                var s = new SdMessageReceive(Main.DataModel);
                return s.GetSdCardMessages(ProgressFactory(), Cancellation.Token);
            }
        }

        protected override void Done()
        {
            Main.OnSdCardMessagesReceived();
        }
    }
}