using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

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
                foreach (var peqDataModel in model.PEQ)
                {
                    peqDataModel.DspBiquads = new HashSet<int>();
                }
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
                if (peqDataModel.DspBiquads == null) return false;
                if (peqDataModel.DspBiquads.Count != req)
                    return false;
                //if biquad is in model, it cannot be in another model or available
                if (peqDataModel.DspBiquads.Any(_model.AvailableBiquads.Contains))
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
            //var unused = (from q in usedBiquads where DataModel.AvailableBiquads.Contains(q) select q).ToArray();
            var clearbq = GetClearBiquadData(unused1, presetId);
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
            return SafeLoadEnabled
                              ? new SafeLoadParam(sos, flowId, biquad, DataModel.SpeakerPeqType)
                              : new PeqParam(sos, flowId, biquad, DataModel.SpeakerPeqType);

        }

        public void ParseRedundancyData(List<byte> dspCopy)
        {
            DataModel.PEQ.Clear();
            for (var redundancyPosition = 0; redundancyPosition < (int)DataModel.SpeakerPeqType; redundancyPosition++)
            {
                int skipto;
                try
                {
                    skipto = RedundancyAddress(redundancyPosition, DataModel.Id);
                }
                catch (ArgumentException a)
                {
                    Debug.WriteLine("Redundancy address not valid {0}", a);
                    return;
                }

                var rawData = dspCopy.Skip(skipto).Take(PeqDataLogic.PeqRedundancyCount).ToArray();
                var pdm = new PeqDataModel();
                var peqR = new PeqDataLogic(pdm);
                var resolved = peqR.ResolveData(rawData);

                if (!resolved) continue;

                DataModel.PEQ.Add(pdm);

                foreach (var dspBiquad in pdm.DspBiquads)
                {
                    DataModel.AvailableBiquads.Remove(dspBiquad);
                }
            }
        }


        public static readonly bool SafeLoadEnabled;
        public static readonly bool SetEepromEnabled;

        static SpeakerLogic()
        {
            bool b;
            SafeLoadEnabled = bool.TryParse(LibraryData.Settings["SafeloadEnabled"], out b) && b;
            SetEepromEnabled = bool.TryParse(LibraryData.Settings["SetEepromEnabled"], out b) && b;
        }



        /// <summary>
        /// Clears biquad positions in DSP and adds their positions to the "free positions" - list
        /// </summary>
        public IEnumerable<IDispatchData> GetClearBiquadData(IEnumerable<int> biquads, int flowId)
        {
            var ret = new List<IDispatchData>();
            foreach (var biquad in biquads.ToArray())
            {
                ret.Add(GetSosParamPackage(SOS.Empty(), flowId, biquad));
                DataModel.AvailableBiquads.Add(biquad);
                ret.Add(GetRedundancyData(flowId, biquad));

            }
            return ret;
        }

        /// <summary>
        /// Clears all biquads and redundancy data for this flow
        /// </summary>

        /// <param name="flowId"></param>
        public List<IDispatchData> GetClearBiquadData(int flowId)
        {
            var ret = new List<IDispatchData>();

            for (int biquad = 0; biquad < (int)DataModel.SpeakerPeqType; biquad++)
            {
                ret.Add(GetSosParamPackage(SOS.Empty(), flowId, biquad));
                ret.Add(GetRedundancyData(flowId, biquad));
            }
            ResetAvailableBq();
            return ret;
        }

        public SetE2PromExt GetRedundancyData(int flowId, int bq, PeqDataLogic pdo = null)
        {
            if (!SetEepromEnabled) return null;

            var redundancydata = (pdo == null) ? new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 } : pdo.GetRedundancyData();
            var mcuId = GenericMethods.GetMainunitIdForFlowId(flowId);
            try
            {
                var redAddress = RedundancyAddress(bq, DataModel.Id);
                return new SetE2PromExt(mcuId, redundancydata, redAddress);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Redundancy address not valid {0}", a);
                return null;

            }
        }

        /// <summary>           
        /// start: 34816 end: 39935
        /// Define redundancy addresses in eeprom
        /// size: 8 bytes redundancy data * biquads (bytes)
        /// tot:  size * channels (bytes)
        ///         chnn  bqs   size  tot   address range
        /// preset  12    14    112   1344  34816   36159
        /// Aux     3     7     56    168   36160   36327    
        /// mic     2     5     40    80    36328   36408
        /// </summary>
        /// <param name="biquad"></param>                
        /// <param name="speakerId">
        /// which speaker:
        /// preset: 0-11, aux 12-14, mic 15-16        
        /// </param>
        /// <returns></returns>
        public ushort RedundancyAddress(int biquad, int speakerId)
        {
            switch (DataModel.SpeakerPeqType)
            {
                case SpeakerPeqType.BiquadsMic:
                    if (biquad > 4)
                        throw new ArgumentException("Mic does not have biquad" + biquad);
                    if (speakerId > 16 || speakerId < 15)
                        throw new ArgumentException("Mic does not have positon" + speakerId);
                    return
                        (ushort)
                            (McuDat.MicRedundancy + biquad * PeqDataLogic.PeqRedundancyCount +
                             (speakerId - 15) * 40);

                case SpeakerPeqType.BiquadsAux:
                    if (biquad > 6)
                        throw new ArgumentException("Aux does not have biquad" + biquad);
                    if (speakerId > 14 || speakerId < 12)
                        throw new ArgumentException("Aux does not have positon" + speakerId);
                    return (ushort)(McuDat.AuxRedundancy + biquad * PeqDataLogic.PeqRedundancyCount + (speakerId - 12) * 56);
                case SpeakerPeqType.BiquadsPreset:
                    if (biquad > 13)
                        throw new ArgumentException("Preset does not have biquad" + biquad);
                    if (speakerId > 11 || speakerId < 0)
                        throw new ArgumentException("Preset does not have positon" + speakerId);
                    return (ushort)(McuDat.PresetRedundancy + biquad * PeqDataLogic.PeqRedundancyCount + speakerId * 112);
                default:
                    throw new ArgumentException("SpeakerPeqType does not exist");
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