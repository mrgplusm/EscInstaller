#region

using System.Threading.Tasks;
using Common;
using EscInstaller.ViewModel.EscCommunication.downloadItems;
using EscInstaller.ViewModel.EscCommunication.Logic;

#endregion

namespace EscInstaller.ViewModel.EscCommunication
{
    public class Dsp : EepromToDownload
    {
        public Dsp(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string ItemName
        {
            get { return "DSP Mirror"; }
        }

        public override Task Function
        {
            get { return DownloaderFactory().GetEeprom(ProgressFactory()); }
        }

        protected override E2PromArea Area
        {
            get { return E2PromArea.DspMirror; }
        }

        protected override void Done()
        {
            var dsp = new DspDataUpdater(Main);
            dsp.SetDspValues();
        }
    }
}