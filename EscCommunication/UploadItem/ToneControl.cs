#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class ToneControl : DownloadData
    {
        public ToneControl(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Tone Control";

        protected override Task Function
        {
            get
            {
                var p = new ToneControlUpdater(Main.DataModel);
                return p.SetToneControls(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}