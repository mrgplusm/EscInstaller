using System;
using System.Collections.Generic;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings.Peq
{
    public abstract class FilterLogicBase
    {
        protected readonly PeqDataModel PEQDataModel;
        private readonly SpeakerDataModel _model;
        protected readonly int FlowId;

        protected FilterLogicBase(PeqDataModel peqDataModel, SpeakerDataModel model, int flowId)
        {
            PEQDataModel = peqDataModel;
            _model = model;
            FlowId = flowId;

            if (PEQDataModel.Biquads == null) PEQDataModel.Biquads = new List<int>();
        }

        public List<int> Biquads
        {
            get { return PEQDataModel.Biquads; }
        }

        /// <summary>
        /// Sends a multi biquad passband filter
        /// there should be enough biquads assigned to this filter
        /// </summary>                
        public abstract IEnumerable<PeqParam> GetParamData();

        protected PeqParam GetSosParamPackage(SOS sos, int biquad)
        {
            return new SafeLoadParam(sos, FlowId, biquad, _model.SpeakerPeqType);
        }

        protected int GetBiquad(int n)
        {
            if (Biquads.Count >= n) return Biquads[n];
            throw new ArgumentException("Not enough biquads for filter!");
        }

        private int GetRedundancyAddress(int biquad)
        {
            return EqDataFiles.RedundancyAddress(GetBiquad(biquad), _model.Id, _model.SpeakerPeqType);
        }

        public IEnumerable<SetE2PromExt> RedundancyData()
        {
            var mcuId = GenericMethods.GetMainunitIdForFlowId(FlowId);

            var redAddress = GetRedundancyAddress(0);

            var data = EqDataFiles.RedundancyData(PEQDataModel);

            yield return new SetE2PromExt(mcuId, data, redAddress);

            //if second biquad previously also contained redundancy data remove it
            for (int i = 1; i < PEQDataModel.Biquads.Count; i++)
            {
                yield return new SetE2PromExt(mcuId, EqDataFiles.RedundancyData(null), GetRedundancyAddress(i));
            }
        }
    }
}