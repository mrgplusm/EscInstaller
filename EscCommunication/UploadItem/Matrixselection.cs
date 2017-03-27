#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class Matrixselection : ItemtoDownload
    {
        public Matrixselection(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Matrix selection";

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