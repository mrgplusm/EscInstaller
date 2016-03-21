#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public class PeqUpload : EscLogic
    {
        private volatile int _peqDataCount;

        public PeqUpload(MainUnitModel main) : base(main)
        {
        }

        /// <summary>
        ///     //aux &&
        ///     redundancy data &&
        ///     peq data &&
        ///     EQpreset names
        /// </summary>
        private IEnumerable<IDispatchData> GetTotalPresetData()
        {
            var flowOffsets = new Dictionary<SpeakerPeqType, int>
            {
                {SpeakerPeqType.BiquadsPreset, Main.Id*12},
                {SpeakerPeqType.BiquadsAux, Main.Id*12},
                {SpeakerPeqType.BiquadsMic, 2 + Main.Id*5 + GenericMethods.StartCountFrom}
            };

            return flowOffsets.SelectMany(offset => GetPresetData(offset.Value, offset.Key));
        }

        private IEnumerable<IDispatchData> GetPresetData(int flowOffset, SpeakerPeqType type)
        {
            return Main.SpeakerDataModels.Where(s => s.SpeakerPeqType == type)
                .Select((speakerDataModel, flow) => new SpeakerLogicForFlow(speakerDataModel, flow + flowOffset))
                .SelectMany(sp => sp.TotalSpeakerData());
        }

        public async Task SetSpeakerPresetData(IProgress<DownloadProgress> iProgress)
        {
            var dataToSend = GetTotalPresetData().ToArray();
            var total = dataToSend.Length;
            foreach (var data in dataToSend)
            {
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