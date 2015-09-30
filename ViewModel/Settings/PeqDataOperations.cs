using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

namespace EscInstaller.ViewModel.Settings
{
    public class PeqDataLogic
    {
        private readonly PeqDataModel _peqDataModel;

        public PeqDataLogic(PeqDataModel peqDataModel)
        {
            _peqDataModel = peqDataModel;
            if (_peqDataModel.DspBiquads == null) _peqDataModel.DspBiquads = new HashSet<int>();
        }


        public HashSet<int> DspBiquads
        {
            get { return _peqDataModel.DspBiquads; }
            set { _peqDataModel.DspBiquads = value; }
        }

        public const int PeqRedundancyCount = 8;

        /// <summary>
        /// 6  byte array containing:
        /// 01 Frequency    2 (512 steps) 10^step*.01        
        /// 2 Ordr & ftype & IsEnabled
        ///      1 (1 - 6, 0 = disabled & enum)
        /// 3  Bandwith     1 (.1 - 6.5)
        /// 4  Boost        1 (-15 - +15)        
        /// 5  Gain         1 (-15 - +15)
        /// 6& 7 biquads used (max 4)
        /// 
        /// </summary>
        /// <returns></returns>
        public static byte[] RedundancyData(PeqDataModel p)
        {
            if (p == null) return new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var s = new byte[PeqRedundancyCount];
            if (p.Frequency > 20000)
                p.Frequency = 20000;
            var freq = (ushort)Math.Round(Math.Log10(p.Frequency) * FreqDevider, 0);
            var freqBytes = BitConverter.GetBytes(freq);
            s[0] = freqBytes[0];
            s[1] = freqBytes[1];
            s[2] = (byte)((p.Order) | ((byte)p.FilterType << 3) | ((byte)(p.IsEnabled ? 1 : 0) << 7));
            s[3] = (byte)Math.Round(p.BandWidth * 36);
            s[4] = (byte)Math.Round(p.Boost * 5 + 0x80, 0);
            s[5] = (byte)Math.Round(p.Gain * 5 + 0x80, 0);

            var arrDspBq = p.DspBiquads.Select(q => (byte)q).ToList();
            while (arrDspBq.Count < 4) { arrDspBq.Add(15); }
            s[6] = (byte)((arrDspBq[0] & 0x0f) | (arrDspBq[1] << 4) & 0xf0);
            s[7] = (byte)((arrDspBq[2] & 0x0f) | (arrDspBq[3] << 4) & 0xf0);

            return s;
        }


        public byte[] GetRedundancyData()
        {
            return RedundancyData(_peqDataModel);
        }

        /// <summary>
        /// Frequency is stored in 2 bytes, 65536. log(10) 20k =~ 4.31. 
        /// </summary>
        private const int FreqDevider = 12205;
        
        /// <summary>
        /// Resolves redundant peq blocks
        /// </summary>
        /// <param name="input">Array representing the redundancy data</param>
        /// <returns>PeqDatamodel with all settings except used biquads</returns>
        public bool ResolveData(byte[] input)
        {
            if ((input == null) || (input.Length != PeqRedundancyCount))
                return false;


            double b = BitConverter.ToUInt16(new[] { input[0], input[1] }, 0);
            _peqDataModel.Frequency = Math.Pow(10, b / FreqDevider);
            _peqDataModel.Order = input[2] & 0x07;
            _peqDataModel.FilterType = (FilterType)(input[2] >> 3 & 0x0F);
            _peqDataModel.IsEnabled = (input[2] >> 7 & 0x01) > 0;
            _peqDataModel.BandWidth = input[3] / 36.0;
            _peqDataModel.Boost = (input[4] - 0x80) / 5.0;
            _peqDataModel.Gain = (input[5] - 0x80) / 5.0;

            var dspBq = new List<int>
            {
                (byte) (input[6] & 0x0F),
                (byte) ((input[6] & 0xF0) >> 4),
                (byte) (input[7] & 0x0F),
                (byte) ((input[7] & 0xF0) >> 4)
            };
            _peqDataModel.DspBiquads = new HashSet<int>(dspBq.Where(q => q != 15));

            if (_peqDataModel.Frequency < 10 || _peqDataModel.Frequency > 20000 || _peqDataModel.Order < 1 ||
                _peqDataModel.Order > 6 ||
                (int)_peqDataModel.FilterType < 0 || (int)_peqDataModel.FilterType > 7 ||
                _peqDataModel.BandWidth < .1 || _peqDataModel.BandWidth > 7 || _peqDataModel.Boost < -15 ||
                _peqDataModel.Boost > 15 || _peqDataModel.Gain < -15 ||
                _peqDataModel.Gain > 15)
                return false;
            return true;
        }


        /// <summary>
        /// send param
        /// </summary>
        /// <param name="p">Peq data view model </param>
        /// <param name="currentSpeaker"></param>
        /// <param name="flowId"></param>        
        public IEnumerable<IDispatchData> GetParamData(SpeakerLogic currentSpeaker, int flowId)
        {
            var ret = new List<IDispatchData>();

            if (_peqDataModel.FilterType == FilterType.ButterworthLp || _peqDataModel.FilterType == FilterType.BesselHp
                || _peqDataModel.FilterType == FilterType.BesselLp || _peqDataModel.FilterType == FilterType.ButterworthHp
                || _peqDataModel.FilterType == FilterType.LinkWitzHp || _peqDataModel.FilterType == FilterType.LinkWitzLp)
            {
                ret.AddRange(PassFilterData(currentSpeaker, flowId));
                return ret;
            }
            
            if (_peqDataModel.FilterType != FilterType.Peaking) //todo:implement high/low shelf and notch.
                return ret;

            ret.AddRange(PeakingParam(currentSpeaker, flowId));

            return ret;
        }

        private IEnumerable<IDispatchData> PeakingParam(SpeakerLogic currentSpeaker, int flowId)
        {
            var ret = new List<IDispatchData>();

            var bqToRemove = DspBiquads.Skip(1).ToArray();
            DspBiquads.RemoveWhere(bqToRemove.Contains);

            var sosparam = DspCoefficients.GetBiquadSos(_peqDataModel.Boost, _peqDataModel.Frequency,
                _peqDataModel.BandWidth, true, DspCoefficients.Fs);


            if (!DspBiquads.Any())
                //use current bq
                DspBiquads.Add(currentSpeaker.DataModel.AvailableBiquads.HashPop());
            else
                //take bq from speaker to be sure
                currentSpeaker.DataModel.AvailableBiquads.Remove(DspBiquads.First());

            ret.Add(currentSpeaker.GetSosParamPackage(sosparam, flowId, DspBiquads.First()));
            ret.Add(currentSpeaker.GetRedundancyData(flowId, DspBiquads.First(), this));

            return ret;
        }


        /// <summary>
        /// Sends a multi biquad passband filter
        /// </summary>
        /// <param name="s"></param>
        /// <param name="p"></param>
        /// <param name="flowId"></param>
        /// <returns></returns>
        public List<PeqParam> GetPassFilter(SpeakerLogic s, int flowId)
        {
            if (_peqDataModel.FilterType == FilterType.Peaking)
                throw new ArgumentException("GetPassFilter() cannot create peaking filter");

            var ret = new List<PeqParam>();
            //var redundancybq = 0;
            foreach (var t in DspCoefficients.GetXoverSOS(_peqDataModel.Frequency, _peqDataModel.Order,
                _peqDataModel.FilterType, DspCoefficients.Fs))
            {
                if (s.DataModel.AvailableBiquads.Count < 1)
                    throw new ArgumentException("no available biquads left");

                int bq;
                try
                {
                    bq = s.DataModel.AvailableBiquads.HashPop();
                }
                catch (InvalidOperationException ioe)
                {
                    throw new InvalidOperationException("GetPassFilter() " + ioe.Message);
                }

                ret.Add(s.GetSosParamPackage(t, flowId, bq));

                _peqDataModel.DspBiquads.Add(bq);
            }

            return ret;

            //return redundancybq;  
        }


        public List<IDispatchData> PassFilterData(SpeakerLogic currentSpeaker, int flowId)
        {
            var oldDspBiquads = DspBiquads.ToArray();
            
            foreach(var b in oldDspBiquads) {
                currentSpeaker.DataModel.AvailableBiquads.Add(b);
                DspBiquads.Remove(b);
            }
            
            var ret = new List<IDispatchData>();
            var passfilterData = GetPassFilter(currentSpeaker, flowId).ToArray();

            ret.AddRange(passfilterData);
            //set redundany data block
            ret.Add(currentSpeaker.GetRedundancyData(flowId, passfilterData.First().Biquad, this));

            //set redundancy data empty for other biquads
            ret.AddRange(passfilterData.Skip(1).Select(peqParam =>
                currentSpeaker.GetRedundancyData(flowId, peqParam.Biquad)));

            //add biquads to speaker that are in use
            //foreach (var vp in passfilterData) { DspBiquads.Add(vp.Biquad); }

            var unusedBq = (from q in oldDspBiquads where !DspBiquads.Contains(q) select q).ToArray();

            //clear biquad data that was used and isn't used anymore
            ret.AddRange(currentSpeaker.GetClearBiquadData(unusedBq, flowId));

            return ret;
        }


    }
}