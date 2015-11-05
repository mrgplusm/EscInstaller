#region

using System.Threading.Tasks;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class Matrixselection : ItemtoDownload
    {
        public Matrixselection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Matrix selection"; }
        }

        public override Task Function
        {
            get
            {
                var p = new MatrixSender(Main.DataModel);
                return p.SetMatrixSelections(ProgressFactory());
            }
        }
    }
}