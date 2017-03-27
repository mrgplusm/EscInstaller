#region

using System.Threading.Tasks;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class Sensitivity : ItemtoDownload
    {
        public Sensitivity(MainUnitViewModel main) : base(main)
        {
        }

        public override string ItemName => "Sensitivity values";

        public override Task Function
        {
            get
            {
                var s = new SensitivityReceive(Main.Id, Main.DataModel);
                return s.GetSensitivityValues(ProgressFactory());
            }
        }
    }
}