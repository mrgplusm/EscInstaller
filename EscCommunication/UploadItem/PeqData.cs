#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

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