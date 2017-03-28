#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class GainSliders : DownloadData
    {
        public GainSliders(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "Gain Sliders";

        public override Task Function
        {
            get
            {
                var p = new SliderUpdater(Main.DataModel);
                return p.SetSliders(ProgressFactory(), Cancellation.Token);
            }
        }
    }
}