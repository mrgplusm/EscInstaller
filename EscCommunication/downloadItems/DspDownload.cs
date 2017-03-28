#region

using System.Threading.Tasks;
using Common;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class Dsp : EepromToDownload
    {
        public Dsp(MainUnitViewModel main)
            : base(main)
        {
        }

        public override string Value => "DSP Mirror";

        public override Task Function => DownloaderFactory().GetEeprom(ProgressFactory());

        protected override E2PromArea Area => E2PromArea.DspMirror;

        protected override void Done()
        {
            var dsp = new DspDataUpdater(Main);
            dsp.SetDspValues();
        }
    }
}