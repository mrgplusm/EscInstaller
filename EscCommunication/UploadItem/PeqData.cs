#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class PeqData : DownloadData
    {
        public PeqData(MainUnitViewModel main) : base(main)
        {
        }

        public override string Value => "Peq Data+ red";

        protected override Task Function
        {
            get
            {
                var p = new PeqUpload(Main.DataModel);
                return p.SetSpeakerPresetData(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}