using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class CrossoverLogic : FilterLogicBase
    {
        public CrossoverLogic(PeqDataModel peqDataModel, SpeakerDataModel model, int flowId)
            : base(peqDataModel, model, flowId)
        {

        }

        /// <summary>
        /// Sends a multi biquad passband filter
        /// there should be enough biquads assigned to this filter
        /// </summary>                
        public override IEnumerable<PeqParam> GetParamData()
        {
            return DspCoefficients.GetXoverSOS(PEQDataModel.Frequency, PEQDataModel.Order,
                PEQDataModel.FilterType, DspCoefficients.Fs).Select((source, index) =>
                    GetSosParamPackage(source, GetBiquad(index)));
        }
    }
}