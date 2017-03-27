#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class PeqData : ItemtoDownload
    {
        public PeqData(MainUnitViewModel main) : base(main)
        {
        }

        public override string ItemName => "Peq Data+ red";

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