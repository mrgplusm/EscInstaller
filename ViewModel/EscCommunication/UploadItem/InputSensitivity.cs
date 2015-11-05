#region

using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class InputSensitivity : ItemtoDownload
    {
        public InputSensitivity(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Input Sensitivity"; }
        }

        public override Task Function
        {
            get
            {
                var p = new InputSensitivityUpdater(Main.DataModel);
                return p.SetInputSensitivity(ProgressFactory());
            }
        }
    }
}