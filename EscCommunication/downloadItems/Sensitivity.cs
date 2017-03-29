#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class Sensitivity : DownloadData
    {
        public Sensitivity(MainUnitViewModel main) : base(main)
        {
        }

        public override string Value => "Sensitivity values";

        protected override Task Function
        {
            get
            {
                var s = new SensitivityReceive(Main.Id, Main.DataModel);
                return s.GetSensitivityValues(ProgressFactory());
            }
        }
    }
}