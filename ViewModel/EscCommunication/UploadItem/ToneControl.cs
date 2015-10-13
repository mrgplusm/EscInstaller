using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class ToneControl : ItemtoDownload
    {
        public ToneControl(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Tone Control"; }
        }

        public override Task Function
        {
            get
            {
                var p = new ToneControlUpdater(Main.DataModel);
                return p.SetToneControls(ProgressFactory());
            }
        }
    }
}