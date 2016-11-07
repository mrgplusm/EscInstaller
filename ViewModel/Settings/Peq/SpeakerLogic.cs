#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class SpeakerLogic
    {
        protected readonly SpeakerDataModel Model;
        //private readonly int _flowId;

        public SpeakerLogic(SpeakerDataModel model)
        {
            Model = model;

            if (model.PEQ == null) model.PEQ = new List<PeqDataModel>();

            UpdateIntegraty();
        }

        /// <summary>
        ///     start: 34816 end: 39935
        ///     Define redundancy addresses in eeprom
        ///     size: 8 bytes redundancy data * biquads (bytes)
        ///     tot:  size * channels (bytes)
        ///     chnn  bqs   size  tot   address range
        ///     preset  12    14    112   1344  34816   36159
        ///     Aux     3     7     56    168   36160   36327
        ///     mic     2     5     40    80    36328   36408
        /// </summary>
        /// which speaker:
        /// preset: 0-11, aux 12-14, mic 15-16
        /// <returns></returns>
        public ushort RedundancyAddress(int biquad)
        {
            switch (Model.SpeakerPeqType)
            {
                case SpeakerPeqType.BiquadsMic:
                    if (biquad > 4)
                        throw new ArgumentException("Mic does not have biquad" + biquad);
                    if (Model.Id > 16 || Model.Id < 15)
                        throw new ArgumentException("Mic does not have positon" + Model.Id);
                    return
                        (ushort)
                            (McuDat.MicRedundancy + biquad * FilterBase.PeqRedundancyCount +
                             (Model.Id - 15) * 40);

                case SpeakerPeqType.BiquadsAux:
                    if (biquad > 6)
                        throw new ArgumentException("Aux does not have biquad" + biquad);
                    if (Model.Id > 14 || Model.Id < 12)
                        throw new ArgumentException("Aux does not have positon" + Model.Id);
                    return (ushort)(McuDat.AuxRedundancy + biquad * FilterBase.PeqRedundancyCount + (Model.Id - 12) * 56);
                case SpeakerPeqType.BiquadsPreset:
                    if (biquad > 13)
                        throw new ArgumentException("Preset does not have biquad" + biquad);
                    if (Model.Id > 11 || Model.Id < 0)
                        throw new ArgumentException("Preset does not have positon" + Model.Id);
                    return (ushort)(McuDat.PresetRedundancy + biquad * FilterBase.PeqRedundancyCount + Model.Id * 112);
                default:
                    throw new ArgumentException("SpeakerPeqType does not exist");
            }
        }

        /// <summary>
        ///     Updates used biquads field
        ///     Library speakers do not have stored biquad values per filter
        ///     Compare stored biquads with actual filters for older sw versions
        /// </summary>
        /// <returns></returns>
        public void UpdateIntegraty()
        {
            if (CheckBiquadIntegraty()) return;

            AssignBqsToFilters();
        }

        private void AssignBqsToFilters()
        {
            if (Model.PEQ.RequiredBiquads() > (int)Model.SpeakerPeqType)
                throw new Exception("This configuration uses too many biquads");
            
            foreach (var peqDataModel in Model.PEQ)
            {
                
                AssignBiquads(peqDataModel);
            }
        }

        private bool CheckBiquadIntegraty()
        {
            foreach (var peqDataModel in Model.PEQ)
            {
                if (peqDataModel.Biquads == null)
                {
                    peqDataModel.Biquads = new List<int>();
                    return false;
                }

                var req = new[] { peqDataModel }.RequiredBiquads();

                if (peqDataModel.Biquads.Count != req)
                    return false;
             
            }
            return (int)Model.SpeakerPeqType >= Model.PEQ.RequiredBiquads();
        }

        public void ParseRedundancyData(List<byte> dspCopy)
        {
            Model.PEQ.Clear();
            for (var redundancyPosition = 0; redundancyPosition < (int)Model.SpeakerPeqType; redundancyPosition++)
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
                skipto = RedundancyAddress(redpos);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Redundancy address not valid {0}", a);
                return null;
            }
            return dspCopy.Skip(skipto).Take(FilterBase.PeqRedundancyCount).ToArray();
        }

        private void SetPeqData(byte[] rawData)
        {
            if (rawData == null || rawData.All(s => s == 0))
                return;
            try
            {
                var pdm = new PeqDataModel();
                var fb = new FilterBase(pdm);
                fb.Parse(rawData);

                if (Model.PEQ.Contains(pdm, new BiquadsEqual()))
                {
                    Debug.WriteLine("skip biquad, biquads already in use");
                    return;
                }
                Model.PEQ.Add(pdm);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Raw eeprom data could not be parsed for peq " + a);
            }
        }

        sealed class BiquadsEqual : EqualityComparer<PeqDataModel>
        {
            public override bool Equals(PeqDataModel x, PeqDataModel y)
            {
                if (x.Biquads == null || y.Biquads == null)
                    return false;
                return x.Biquads.SequenceEqual(y.Biquads);
            }

            public override int GetHashCode(PeqDataModel obj)
            {
                return obj?.Biquads?.Sum(s => s) ?? 0;
            }
        }

        public IEnumerable<int> UsedBiquads()
        {
            return Model.PEQ.Where(s => s.Biquads != null).SelectMany(n => n.Biquads).ToArray();
        }

        public Stack<int> AvailableBiquads()
        {
            return new Stack<int>(Enumerable.Range(0, (int)Model.SpeakerPeqType).Except(UsedBiquads()));
        }

        public void AssignBiquads(PeqDataModel dm)
        {
            var r = new[] { dm }.RequiredBiquads();
            var availablebq = AvailableBiquads();

            if (dm.Biquads != null)
                foreach (var biquad in dm.Biquads)
                {
                    availablebq.Push(biquad);
                }

            dm.Biquads = new List<int>();

            for (var i = 0; i < r; i++)
            {
                dm.Biquads.Add(availablebq.Pop());
            }
        }
    }
}