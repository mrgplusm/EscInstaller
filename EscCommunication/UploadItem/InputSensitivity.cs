#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class InputSensitivity : ItemtoDownload
    {
        public InputSensitivity(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Input Sensitivity";

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