#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class PeqUpload : EscLogic
    {
        private volatile int _peqDataCount;

        public PeqUpload(MainUnitModel main) : base(main)
        {
            PresetOffset = new Dictionary<SpeakerPeqType, int>()
            {
                {SpeakerPeqType.BiquadsPreset, Main.Id*12},
                {SpeakerPeqType.BiquadsAux, Main.Id*12},
                {SpeakerPeqType.BiquadsMic, 2 + Main.Id*5 + GenericMethods.StartCountFrom}
            };
        }                
        
        public Dictionary<SpeakerPeqType, int> PresetOffset;

        /// <summary>
        /// list of list of speakerdata + redundancy
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<SpeakerDataModel> PresetModels(SpeakerPeqType type)
        {            
            return Main.SpeakerDataModels.Where(sq => sq.SpeakerPeqType == type);
        }

        public IEnumerable<IDispatchData> DspData(SpeakerDataModel model)
        {
            return new SpeakerLogicForFlow(model, model.Id + PresetOffset[model.SpeakerPeqType]).TotalSpeakerData();
        }

        //public IEnumerable<IDispatchData> RedundancyData(SpeakerDataModel model)
        //{
        //    return new SpeakerLogicForFlow(model, model.Id + PresetOffset[model.SpeakerPeqType]).RedundancyData();
        //}

        public async Task SetData(IProgress<DownloadProgress> iProgress, CancellationToken token, IList<IDispatchData> packages)
        {
            
            var total = packages.Count;
            foreach (var data in packages)
            {
                if (token.IsCancellationRequested) return;
                CommunicationViewModel.AddData(data);
                await data.WaitAsync();
                iProgress.Report(new DownloadProgress()
                {
                    Progress = ++_peqDataCount,
                    Total = total
                });
            }
        }
    }
}