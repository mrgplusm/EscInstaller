using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class SpeakerLogic
    {
        private readonly SpeakerDataModel _model;
        private readonly int _flowId;

        public SpeakerLogic(SpeakerDataModel model, int flowId)
        {
            _model = model;
            _flowId = flowId;
            if (model.PEQ == null) model.PEQ = new List<PeqDataModel>();
        }

        /// <summary>
        /// Compare stored biquads with actual filters for older sw versions
        /// </summary>
        /// <returns></returns>
        public void UpdateIntegraty()
        {
            if (CheckBiquadIntegraty()) return;

            ResetAvailableBq();
            _model.PEQ = new List<PeqDataModel>();
        }

        private bool CheckBiquadIntegraty()
        {
            //_model.AvailableBiquads = GenHashset();
            foreach (var peqDataModel in _model.PEQ)
            {
                int req = new[] { peqDataModel }.RequiredBiquads();
                if (peqDataModel.Biquads == null) return false;
                if (peqDataModel.Biquads.Count != req)
                    return false;
                //if biquad is in model, it cannot be in another model or available
                if (peqDataModel.Biquads.Any(_model.AvailableBiquads.Contains))
                    return false;
            }

            return ((int)_model.SpeakerPeqType - _model.PEQ.RequiredBiquads()) == _model.AvailableBiquads.Count;
        }

        public IEnumerable<SetE2PromExt> RedundancyData()
        {
            return _model.PEQ.Select(LogicFactory).SelectMany(logic => logic.RedundancyData());
        }

        public IEnumerable<PeqParam> DspData()
        {
            return _model.PEQ.Select(LogicFactory).SelectMany(logic => logic.GetParamData());
        }

        public IEnumerable<PeqParam> DspData(PeqDataModel data)
        {
            return LogicFactory(data).GetParamData();
        }

        public IEnumerable<PeqParam> GetEmptyDspData(IEnumerable<int> biquads)
        {
            return biquads.Select(bq => new PeqParam(SOS.Empty(), _flowId, bq, _model.SpeakerPeqType));
        }

        public IEnumerable<SetE2PromExt> GetEmtptyRedundancyData(IEnumerable<int> biquads)
        {
            return
                biquads.Select(
                    bq =>
                        new SetE2PromExt(GenericMethods.GetMainunitIdForFlowId(_flowId),
                            EqDataFiles.RedundancyData(null),
                            EqDataFiles.RedundancyAddress(bq, _model.Id, _model.SpeakerPeqType)));
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

            foreach (var peqParam in GetEmptyDspData(_model.AvailableBiquads))
            {
                yield return peqParam;
            }

            foreach (var setE2PromExt in GetEmtptyRedundancyData(_model.AvailableBiquads))
            {
                yield return setE2PromExt;
            }
        }

        private FilterLogicBase LogicFactory(PeqDataModel model)
        {
            switch (model.FilterType)
            {
                case FilterType.Peaking:
                    return new PeakingLogic(model, _model, _flowId);
                default:
                    return new CrossoverLogic(model, _model, _flowId);
            }
        }

        public void ParseRedundancyData(List<byte> dspCopy)
        {
            for (var redundancyPosition = 0; redundancyPosition < (int)_model.SpeakerPeqType; redundancyPosition++)
            {
                var position = EepromPosition(dspCopy, redundancyPosition);
                SetPeqData(position);
            }
        }

        private byte[] EepromPosition(IEnumerable<byte> dspCopy, int redpos)
        {
            int skipto;
            try
            {
                skipto = EqDataFiles.RedundancyAddress(redpos, _model.Id, _model.SpeakerPeqType);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Redundancy address not valid {0}", a); return null;
            }
            return dspCopy.Skip(skipto).Take(EqDataFiles.PeqRedundancyCount).ToArray();
        }

        private void SetPeqData(byte[] rawData)
        {
            PeqDataModel pdm;

            try
            {
                pdm = EqDataFiles.Parse(rawData);
                _model.PEQ.Add(pdm);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Raw eeprom data could not be parsed for peq " + a);
                return;
            }

            foreach (var dspBiquad in pdm.Biquads)
            {
                _model.AvailableBiquads.Remove(dspBiquad);
            }
        }

        private void ResetAvailableBq()
        {
            if (_model.AvailableBiquads == null) _model.AvailableBiquads = new HashSet<int>();
            var all = (Enumerable.Range(0, (int)_model.SpeakerPeqType));
            foreach (var i in all) { _model.AvailableBiquads.Add(i); }
        }

        public PresetNameUpdate PresetNameFactory()
        {
            //set the name of choosen preset
            if (_model.SpeakerPeqType == SpeakerPeqType.BiquadsMic) return null;
            var mcuId = GenericMethods.GetMainunitIdForFlowId(_flowId);
            return new PresetNameUpdate(mcuId, _model.SpeakerName, _model.Id);
        }
    }
}