using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class PeqData : ItemtoDownload
    {
        public PeqData(MainUnitViewModel main) : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Peq Data+ red"; }
        }

        public override Task Function
        {
            get
            {
                var p = new PeqUpload(Main.DataModel);
                return p.SetSpeakerPresetData(ProgressFactory());
            }
        }
    }
}