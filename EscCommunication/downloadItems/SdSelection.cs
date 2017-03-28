#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class SdSelection : DownloadData
    {
        public SdSelection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Message Selection";

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