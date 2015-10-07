using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Common.Model;

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
        /// Updates used biquads field
        /// Library speakers do not have stored biquad values per filter
        /// Compare stored biquads with actual filters for older sw versions
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
            var bq = GetBiquadStack();

            foreach (var peqDataModel in Model.PEQ)
            {
                var req = new[] { peqDataModel }.RequiredBiquads();
                peqDataModel.Biquads = new List<int>();
                for (int i = 0; i < req; i++)
                {
                    peqDataModel.Biquads.Add(bq.Pop());
                }
            }
        }

        private Stack<int> GetBiquadStack()
        {
            return new Stack<int>(Enumerable.Range(0, (int)Model.SpeakerPeqType));
        }

        private bool CheckBiquadIntegraty()
        {
            //_model.AvailableBiquads = GenHashset();
            foreach (var peqDataModel in Model.PEQ)
            {
                if (peqDataModel.Biquads == null)
                {
                    peqDataModel.Biquads = new List<int>();
                    return false;
                }

                int req = new[] { peqDataModel }.RequiredBiquads();
                if (peqDataModel.Biquads.Count != req)
                    return false;
                //if biquad is in model, it cannot be in another model or available
                //if (peqDataModel.Biquads.Any(Model.AvailableBiquads.Contains))
                //    return false;
            }
            return (int)Model.SpeakerPeqType > Model.PEQ.RequiredBiquads();
            //return ((int)Model.SpeakerPeqType - Model.PEQ.RequiredBiquads()) == Model.AvailableBiquads.Count;
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
                skipto = EqDataFiles.RedundancyAddress(redpos, Model.Id, Model.SpeakerPeqType);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Redundancy address not valid {0}", a); return null;
            }
            return dspCopy.Skip(skipto).Take(EqDataFiles.PeqRedundancyCount).ToArray();
        }

        private void SetPeqData(byte[] rawData)
        {
            if (rawData.All(s => s == 0))
                return;
            try
            {
                PeqDataModel pdm = EqDataFiles.Parse(rawData);
                Model.PEQ.Add(pdm);
            }
            catch (ArgumentException a)
            {
                Debug.WriteLine("Raw eeprom data could not be parsed for peq " + a);
                return;
            }
        }

        //private void ResetAvailableBq()
        //{
        //    if (Model.AvailableBiquads == null) Model.AvailableBiquads = new HashSet<int>();
        //    var all = (Enumerable.Range(0, (int)Model.SpeakerPeqType));
        //    foreach (var i in all) { Model.AvailableBiquads.Add(i); }
        //}

        public IEnumerable<int> UsedBiquads()
        {
            return Model.PEQ.SelectMany(n => n.Biquads);
        }

        public Stack<int> AvailableBiquads()
        {
            return new Stack<int>(Enumerable.Range(0, (int)Model.SpeakerPeqType).Except(UsedBiquads()));
        }

        public void AssignBiquads(PeqDataModel dm)
        {
            var r = new[] { dm }.RequiredBiquads();
            var availablebq = AvailableBiquads();

            foreach (var biquad in dm.Biquads)
            {
                availablebq.Push(biquad);
            }

            dm.Biquads = new List<int>();

            for (int i = 0; i < r; i++)
            {
                dm.Biquads.Add(availablebq.Pop());
            }
        }

    }
}