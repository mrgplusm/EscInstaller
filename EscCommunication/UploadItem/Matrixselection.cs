#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Matrixselection : DownloadData
    {
        public Matrixselection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Matrix selection";

        public override Task Function
        {
            get
            {
                var p = new MatrixSender(Main.DataModel);
                return p.SetMatrixSelections(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}