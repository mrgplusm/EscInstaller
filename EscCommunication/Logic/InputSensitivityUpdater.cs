#region

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class InputSensitivityUpdater : EscLogic
    {
        private volatile int _inputSensitivityPackages;

        protected internal InputSensitivityUpdater(MainUnitModel main) : base(main)
        {
        }

        public async Task SetInputSensitivity(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            //sensitivity
            _inputSensitivityPackages = 0;
            bool b;
            if (!bool.TryParse(LibraryData.Settings["InputsensitivityIsEnabled"], out b) || !b) return;
            if (Main.InputSensitivity == null) return;

            var lst =
                Main.InputSensitivity.Select((x, y) => new SetInputSensitivity(Main.Id*12 + y, x)).ToArray();

            foreach (var setInputSensitivity in lst)
            {
                if (token.IsCancellationRequested) return;
                CommunicationViewModel.AddData(setInputSensitivity);
            }            

            foreach (var sensitivity in lst)
            {
                if (token.IsCancellationRequested) return;
                await sensitivity.WaitAsync();
                iProgress.Report(new DownloadProgress
                {
                    Total = 12,
                    Progress = ++_inputSensitivityPackages
                });
            }
        }
    }
}