#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.UploadItem
{
    internal class GainSliders : ItemtoDownload
    {
        public GainSliders(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "Gain Sliders"; }
        }

        public override Task Function
        {
            get
            {
                var p = new SliderUpdater(Main.DataModel);
                return p.SetSliders(ProgressFactory());
            }
        }
    }
}