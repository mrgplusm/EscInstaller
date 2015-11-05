#region

using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class FilterBase
    {
        public const int PeqRedundancyCount = 8;

        /// <summary>
        ///     Frequency is stored in 2 bytes, 65536. log(10) 20k =~ 4.31.
        /// </summary>
        private const int FreqDevider = 12205;

        public readonly PeqDataModel PEQDataModel;

        public FilterBase(PeqDataModel model)
        {
            PEQDataModel = model;
        }

        /// <summary>
        ///     6  byte array containing:
        ///     01 Frequency    2 (512 steps) 10^step*.01
        ///     2 Ordr & ftype & IsEnabled
        ///     1 (1 - 6, 0 = disabled & enum)
        ///     3  Bandwith     1 (.1 - 6.5)
        ///     4  Boost        1 (-15 - +15)
        ///     5  Gain         1 (-15 - +15)
        ///     6& 7 biquads used (max 4)
        /// </summary>
        /// <returns></returns>
        public byte[] RedundancyToBytes()
        {
            if (PEQDataModel == null) return new byte[] {0, 0, 0, 0, 0, 0, 0, 0};
            var s = new byte[PeqRedundancyCount];
            if (PEQDataModel.Frequency > 20000)
                PEQDataModel.Frequency = 20000;
            var freq = (ushort) Math.Round(Math.Log10(PEQDataModel.Frequency)*FreqDevider, 0);
            var freqBytes = BitConverter.GetBytes(freq);
            s[0] = freqBytes[0];
            s[1] = freqBytes[1];
            s[2] =
                (byte)
                    ((PEQDataModel.Order) | ((byte) PEQDataModel.FilterType << 3) |
                     ((byte) (PEQDataModel.IsEnabled ? 1 : 0) << 7));
            s[3] = (byte) Math.Round(PEQDataModel.BandWidth*36);
            s[4] = (byte) Math.Round(PEQDataModel.Boost*5 + 0x80, 0);
            s[5] = (byte) Math.Round(PEQDataModel.Gain*5 + 0x80, 0);

            var arrDspBq = PEQDataModel.Biquads.Select(q => (byte) q).ToList();
            while (arrDspBq.Count < 4)
            {
                arrDspBq.Add(15);
            }
            s[6] = (byte) ((arrDspBq[0] & 0x0f) | (arrDspBq[1] << 4) & 0xf0);
            s[7] = (byte) ((arrDspBq[2] & 0x0f) | (arrDspBq[3] << 4) & 0xf0);

            return s;
        }

        /// <summary>
        ///     Resolves redundant peq blocks
        /// </summary>
        /// <param name="input">Array representing the redundancy data</param>
        /// <returns>PeqDatamodel with all settings except used biquads</returns>
        public void Parse(byte[] input)
        {
            if ((input == null) || (input.Length != PeqRedundancyCount))
                throw new ArgumentException("no data found in this peq data");

            double b = BitConverter.ToUInt16(new[] {input[0], input[1]}, 0);

            var peqDataModel = new PeqDataModel
            {
                Frequency = Math.Pow(10, b/FreqDevider),
                Order = input[2] & 0x07,
                FilterType = (FilterType) (input[2] >> 3 & 0x0F),
                IsEnabled = (input[2] >> 7 & 0x01) > 0,
                BandWidth = input[3]/36.0,
                Boost = (input[4] - 0x80)/5.0,
                Gain = (input[5] - 0x80)/5.0
            };

            var dspBq = new List<int>
            {
                (byte) (input[6] & 0x0F),
                (byte) ((input[6] & 0xF0) >> 4),
                (byte) (input[7] & 0x0F),
                (byte) ((input[7] & 0xF0) >> 4)
            };

            peqDataModel.Biquads = new List<int>(dspBq.Where(q => q != 15));

            if (peqDataModel.Frequency < 10 || peqDataModel.Frequency > 20000 || peqDataModel.Order < 1 ||
                peqDataModel.Order > 6 ||
                (int) peqDataModel.FilterType < 0 || (int) peqDataModel.FilterType > 7 ||
                peqDataModel.BandWidth < .1 || peqDataModel.BandWidth > 7 || peqDataModel.Boost < -15 ||
                peqDataModel.Boost > 15 || peqDataModel.Gain < -15 ||
                peqDataModel.Gain > 15)
                throw new ArgumentException("the eq data found is not applicable");

            PEQDataModel.BandWidth = peqDataModel.BandWidth;
            PEQDataModel.Biquads = peqDataModel.Biquads;
            PEQDataModel.Boost = peqDataModel.Boost;
            PEQDataModel.FilterType = peqDataModel.FilterType;
            PEQDataModel.Frequency = peqDataModel.Frequency;
            PEQDataModel.Gain = peqDataModel.Gain;
            PEQDataModel.IsEnabled = peqDataModel.IsEnabled;
            PEQDataModel.Order = peqDataModel.Order;
        }
    }
}