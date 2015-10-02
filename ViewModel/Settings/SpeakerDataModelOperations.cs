using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings
{
    public class SpeakerLogic
    {
        private readonly SpeakerDataModel _model;


        public SpeakerLogic(SpeakerDataModel model)
        {
            _model = model;
            if (model.PEQ == null) model.PEQ = new List<PeqDataModel>();
            var isOk = CheckBiquadIntegraty();
            if (!isOk)
            {
                ResetAvailableBq();
                
            }
        }

        //check for each filter and speaker if amount of biquads is correct
        public bool CheckBiquadIntegraty()
        {
            if (_model.AvailableBiquads == null)
                return false;
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

        public SpeakerDataModel DataModel
        {
            get { return _model; }
        }


        /// <summary>
        ///     Send whole configuration
        ///     todo:Implement dbcorrect for PEQ (DSP data)
        /// </summary>
        public IEnumerable<IDispatchData> GetPresetData(int presetId)
        {
            var usedBiquads = (from q in (Enumerable.Range(0, (int)_model.SpeakerPeqType).ToArray()) where !DataModel.AvailableBiquads.Contains(q) select q).ToArray();

            var ret = new List<IDispatchData>();

            ResetAvailableBq();

            if (DataModel.PEQ.Count > (int)_model.SpeakerPeqType)
                throw new IndexOutOfRangeException("This speaker utilizes too many biquads and cannot be send");

            var allP = AllParamData(presetId);
            ret.AddRange(allP);

            var unused1 = DataModel.AvailableBiquads.Except(usedBiquads);            
            var clearbq = ClearBiquadData(unused1, presetId);
          
            ret.AddRange(clearbq);

            return ret;
        }


        private IEnumerable<IDispatchData> AllParamData(int flowId)
        {
            var ret = new List<IDispatchData>();
            var speakerBq = new Stack<PeqDataLogic>(PeqOperations);


            //send all 
            while (DataModel.AvailableBiquads.Count > 0)
            {
                if (speakerBq.Count < 1) break;

                var qdata = speakerBq.Pop();

                ret.AddRange(qdata.GetParamData(this, flowId));

            }
            return ret;
        }


        public PeqParam GetSosParamPackage(SOS sos, int flowId, int biquad)
        {
            return new SafeLoadParam(sos, flowId, biquad, DataModel.SpeakerPeqType);
            //: new PeqParam(sos, flowId, biquad, DataModel.SpeakerPeqType);

        }
        
        public void ParseRedundancyData(List<byte> dspCopy)
        {            
            for (var redundancyPosition = 0; redundancyPosition < (int)DataModel.SpeakerPeqType; redundancyPosition++)
            {
                var position = EepromPosition(dspCopy, redundancyPosition);
                SetPeqData(position);                
            }
        }

        private byte[] EepromPosition(List<byte> dspCopy, int redpos )
        {
            int skipto;
            try
            {
                skipto = EqDataFiles.RedundancyAddress(redpos, DataModel.Id, DataModel.SpeakerPeqType);
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
                DataModel.PEQ.Add(pdm);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Raw eeprom data could not be parsed for peq");
                return;
            }

            foreach (var dspBiquad in pdm.Biquads)
            {
                DataModel.AvailableBiquads.Remove(dspBiquad);
            }
        }

        /// <summary>
        /// The biquads are not rembered als "used" and can be claimed again
        /// </summary>
        /// <param name="biquads"></param>
        public void FreeupBiquads(IEnumerable<int> biquads)
        {
            foreach (var biquad in biquads)
            {
                DataModel.AvailableBiquads.Add(biquad);
            }
        }

        /// <summary>
        /// Clears all biquads and redundancy data for this flow
        /// </summary>
        /// <param name="flowId"></param>
        public List<IDispatchData> ClearBiquadData(int flowId)
        {
            var ret = new List<IDispatchData>();

            for (int biquad = 0; biquad < (int)DataModel.SpeakerPeqType; biquad++)
            {
                ret.Add(GetSosParamPackage(SOS.Empty(), flowId, biquad));
                ret.Add(RedundancyData(flowId, biquad));
            }
            
            return ret;
        }

        public SetE2PromExt RedundancyData(int flowId, int bq)
        {            
            //var redundancydata = (pdo == null) ? new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 } : pdo.GetRedundancyData();
            var mcuId = GenericMethods.GetMainunitIdForFlowId(flowId);
            try
            {
                var redAddress = EqDataFiles.RedundancyAddress(bq, DataModel.Id, DataModel.SpeakerPeqType);
                return new SetE2PromExt(mcuId,  redundancydata, redAddress);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Redundancy address not valid {0}", a);
                return null;

            }
        }

        public void ResetAvailableBq()
        {
            if (DataModel.AvailableBiquads == null) DataModel.AvailableBiquads = new HashSet<int>();
            var all = (Enumerable.Range(0, (int)_model.SpeakerPeqType));
            foreach (var i in all) { DataModel.AvailableBiquads.Add(i); }
        }

        public IEnumerable<PeqDataLogic> PeqOperations
        {
            get { return _model.PEQ.Select(n => new PeqDataLogic(n)); }
        }

        public PresetNameUpdate PresetNameFactory(int flowId)
        {
            //set the name of choosen preset
            if (_model.SpeakerPeqType == SpeakerPeqType.BiquadsMic) return null;
            var mcuId = GenericMethods.GetMainunitIdForFlowId(flowId);
            return new PresetNameUpdate(mcuId, _model.SpeakerName, _model.Id);
        }
    }
}