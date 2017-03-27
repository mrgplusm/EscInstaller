#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class GainSliders : ItemtoDownload
    {
        public GainSliders(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName => "Gain Sliders";

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