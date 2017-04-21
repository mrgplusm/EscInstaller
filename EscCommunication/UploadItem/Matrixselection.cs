#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class SetMatrix : DownloadData
    {
        public SetMatrix(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Set matrix selection";

        protected override Task Function
        {
            get
            {
                var p = new MatrixSender(Main.DataModel);
                return p.SetMatrixSelections(Reporting, Cancellation.Token);
            }
        }
    }
}