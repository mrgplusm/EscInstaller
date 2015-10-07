using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class SpeakerLogicForFlow : SpeakerLogic
    {
        private readonly int _flowId;

        public SpeakerLogicForFlow(SpeakerDataModel model, int flowId)
            : base(model)
        {
            _flowId = flowId;
        }

        public IEnumerable<PeqParam> GetEmptyDspData(IEnumerable<int> biquads)
        {
            return biquads.Select(bq => new PeqParam(SOS.Empty(), _flowId, bq, Model.SpeakerPeqType));
        }

        public IEnumerable<SetE2PromExt> GetEmtptyRedundancyData(IEnumerable<int> biquads)
        {
            return
                biquads.Select(
                    bq =>
                        new SetE2PromExt(GenericMethods.GetMainunitIdForFlowId(_flowId),
                            EqDataFiles.RedundancyData(null),
                            EqDataFiles.RedundancyAddress(bq, Model.Id, Model.SpeakerPeqType)));
        }

        public IEnumerable<SetE2PromExt> RedundancyData()
        {
            return Model.PEQ.Select(LogicFactory).SelectMany(logic => logic.RedundancyData());
        }

        /// <summary>
        /// Get total dsp values for every biquad
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PeqParam> DspData()
        {
            return Model.PEQ.Select(LogicFactory).SelectMany(logic => logic.GetParamData());
        }

        /// <summary>
        /// Get dsp data for specific parameter
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IEnumerable<PeqParam> DspData(PeqDataModel data)
        {
            return LogicFactory(data).GetParamData();
        }

        /// <summary>
        /// Get redundancy data for specific parameter
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IEnumerable<SetE2PromExt> Redundancy(PeqDataModel data)
        {
            return LogicFactory(data).RedundancyData();
        }

        public PresetNameUpdate PresetNameFactory()
        {
            //set the name of choosen preset
            if (Model.SpeakerPeqType == SpeakerPeqType.BiquadsMic) return null;
            var mcuId = GenericMethods.GetMainunitIdForFlowId(_flowId);
            return new PresetNameUpdate(mcuId, Model.SpeakerName, Model.Id);
        }

        /// <summary>
        /// Dsp Data
        /// Redundancy data
        /// Empty dsp data for empty biquads
        /// Empty redundancy data for empty biquads
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDispatchData> TotalSpeakerData()
        {
            foreach (var peqParam in DspData())
            {
                yield return peqParam;
            }

            foreach (var setE2PromExt in RedundancyData())
            {
                yield return setE2PromExt;
            }

            foreach (var peqParam in GetEmptyDspData(AvailableBiquads()))
            {
                yield return peqParam;
            }

            foreach (var setE2PromExt in GetEmtptyRedundancyData(AvailableBiquads()))
            {
                yield return setE2PromExt;
            }
        }

        private FilterLogicBase LogicFactory(PeqDataModel model)
        {
            switch (model.FilterType)
            {
                case FilterType.Peaking:
                    return new PeakingLogic(model, Model, _flowId);
                default:
                    return new CrossoverLogic(model, Model, _flowId);
            }
        }


    }
}