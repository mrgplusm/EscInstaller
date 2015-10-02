using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings
{
    public class PeakingLogic : PeqDataLogic
    {
        private readonly PeqDataModel _peqDataModel;

        public PeakingLogic(PeqDataModel peqDataModel) : base(peqDataModel)
        {
            _peqDataModel = peqDataModel;
        }

        public override IEnumerable<IDispatchData> GetParamData(SpeakerLogic currentSpeaker, int flowId)
        {
            var sosparam = DspCoefficients.GetBiquadSos(_peqDataModel.Boost, _peqDataModel.Frequency,
             _peqDataModel.BandWidth, true, DspCoefficients.Fs);
            
             yield return currentSpeaker.GetSosParamPackage(sosparam, flowId, GetBiquad(0));
             yield return currentSpeaker.GetRedundancyData(flowId, GetBiquad(0));            
        }
    }


    public class PeqDataLogic
    {
        private readonly PeqDataModel _peqDataModel;

        public PeqDataLogic(PeqDataModel peqDataModel)
        {
            _peqDataModel = peqDataModel;
            if (_peqDataModel.Biquads == null) _peqDataModel.Biquads = new List<int>();

        }

        public List<int> Biquads
        {
            get { return _peqDataModel.Biquads; }
        }

        /// <summary>
        /// send param
        /// </summary>
        /// <param name="p">Peq data view model </param>
        /// <param name="currentSpeaker"></param>
        /// <param name="flowId"></param>        
        public virtual IEnumerable<IDispatchData> GetParamData(SpeakerLogic currentSpeaker, int flowId)
        {
            var ret = new List<IDispatchData>();

            ret.AddRange(PassFilterData(currentSpeaker, flowId));
            return ret;
        }
        
        /// <summary>
        /// Sends a multi biquad passband filter
        /// there should be enough biquads assigned to this filter
        /// </summary>                
        private IEnumerable<PeqParam> GetPassFilter(SpeakerLogic s, int flowId)
        {
            if (_peqDataModel.FilterType == FilterType.Peaking)
                throw new ArgumentException("GetPassFilter() cannot create peaking filter");

            return DspCoefficients.GetXoverSOS(_peqDataModel.Frequency, _peqDataModel.Order,
                _peqDataModel.FilterType, DspCoefficients.Fs).Select((source, index) =>
                    s.GetSosParamPackage(source, flowId, GetBiquad(index)));
        }


        protected int GetBiquad(int n)
        {
            if (Biquads.Count >= n) return Biquads[n];
            Debug.WriteLine("Not enough biquads for filter!");
            return 0;
        }

        public IEnumerable<IDispatchData> PassFilterData(SpeakerLogic currentSpeaker, int flowId)
        {
            foreach (var filter in GetPassFilter(currentSpeaker, flowId))
            {
                yield return filter;
            }

            //set redundany data block
            yield return currentSpeaker.GetRedundancyData(flowId, GetBiquad(0));

            //set redundancy data empty for other biquads
            foreach (var biquad in Biquads.Skip(1))
            {
                yield return currentSpeaker.GetRedundancyData(flowId, biquad);
                //clear biquad data that was used and isn't used anymore
                yield return currentSpeaker.GetSosParamPackage(SOS.Empty(), flowId, biquad);
            }                        
        }
    }
}