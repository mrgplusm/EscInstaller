using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class SdSelection : ItemtoDownload
    {
        public SdSelection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Message Selection"; }
        }

        public override Task Function
        {
            get
            {
                var s = new MessageSelector(Main.DataModel);
                return s.GetButtonProgramming(ProgressFactory());
            }
        }

        protected override void Done()
        {
            Main.OnSdCardPositionsReceived();
        }
    }
}