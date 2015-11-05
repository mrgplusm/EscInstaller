#region

using System;
using System.Collections.Generic;
using Common;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.Settings.Peq
{
    public abstract class FilterLogicBase : FilterBase
    {
        private readonly SpeakerDataModel _model;
        protected readonly int FlowId;

        protected FilterLogicBase(PeqDataModel peqDataModel, SpeakerDataModel model, int flowId)
            : base(peqDataModel)
        {
            _model = model;
            FlowId = flowId;

            if (PEQDataModel.Biquads == null)
                throw new Exception("No biquads are defined for this filter");
            if (new[] {PEQDataModel}.RequiredBiquads() > PEQDataModel.Biquads.Count)
                throw new Exception("Not enough biquads are defined for this filter");
        }

        /// <summary>
        ///     Sends a multi biquad passband filter
        ///     there should be enough biquads assigned to this filter
        /// </summary>
        public abstract IEnumerable<PeqParam> GetParamData();

        protected PeqParam GetSosParamPackage(SOS sos, int biquad)
        {
            return new SafeLoadParam(sos, FlowId, biquad, _model.SpeakerPeqType);
        }

        protected int GetBiquad(int n)
        {
            if (PEQDataModel.Biquads.Count > n) return PEQDataModel.Biquads[n];
            throw new ArgumentException("Not enough biquads defined for filter!");
        }

        private int GetRedundancyAddress(int biquad)
        {
            var z = new SpeakerLogic(_model);
            return z.RedundancyAddress(GetBiquad(biquad));
        }

        public virtual IEnumerable<SetE2PromExt> RedundancyData()
        {
            var mcuId = GenericMethods.GetMainunitIdForFlowId(FlowId);

            var redAddress = GetRedundancyAddress(0);

            var data = RedundancyToBytes();

            yield return new SetE2PromExt(mcuId, data, redAddress);

            //if biquad consists out of more biquads remove their redundancy data
            for (var i = 1; i < PEQDataModel.Biquads.Count; i++)
            {
                yield return new SetE2PromExt(mcuId, RedundancyToBytes(), GetRedundancyAddress(i));
            }
        }
    }
}