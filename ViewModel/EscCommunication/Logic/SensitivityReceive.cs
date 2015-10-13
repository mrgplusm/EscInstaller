using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class SensitivityReceive
    {
        private readonly MainUnitModel _main;
        private readonly int _mcuId;


        public SensitivityReceive(int mcuId, MainUnitModel main)
        {
            _mcuId = mcuId;
            _main = main;
        }

        /// <summary>
        ///     get inputsensitivity for each of the 12 channels
        /// </summary>
        public async Task GetSensitivityValues(IProgress<DownloadProgress> downloadProgress)
        {
            var data = new GetInputSensitivity(_mcuId);

            CommunicationViewModel.AddData(data);
            await data.WaitAsync();

            _main.InputSensitivity = data.Flows.ToList();
            downloadProgress.Report(new DownloadProgress() {Progress = 1, Total = 1});
        }
    }
}