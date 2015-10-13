using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public class Sensitivity : ItemtoDownload
    {
        public Sensitivity(MainUnitViewModel main) : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Sensitivity values"; }
        }

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