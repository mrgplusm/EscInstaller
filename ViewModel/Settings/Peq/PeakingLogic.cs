#region

using System.Collections.Generic;
using Common;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class PeakingLogic : FilterLogicBase
    {
        public PeakingLogic(PeqDataModel peqDataModel, SpeakerDataModel model, int flowId)
            : base(peqDataModel, model, flowId)
        {
        }

        public override IEnumerable<PeqParam> GetParamData()
        {
            var c = DspCoefficients.GetBiquadSos(PEQDataModel.Boost, PEQDataModel.Frequency,
                PEQDataModel.BandWidth, true, DspCoefficients.Fs);
            yield return GetSosParamPackage(c, GetBiquad(0));
        }
    }
}