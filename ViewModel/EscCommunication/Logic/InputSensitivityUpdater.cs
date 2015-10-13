using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public class InputSensitivityUpdater : EscLogic
    {
        private volatile int _inputSensitivityPackages;

        protected internal InputSensitivityUpdater(MainUnitModel main) : base(main)
        {
        }

        public async Task SetInputSensitivity(IProgress<DownloadProgress> iProgress)
        {
            //sensitivity
            _inputSensitivityPackages = 0;
            bool b;
            if (!Boolean.TryParse(LibraryData.Settings["InputsensitivityIsEnabled"], out b) || !b) return;
            if (Main.InputSensitivity == null) return;

            var lst =
                Main.InputSensitivity.Select((x, y) => new SetInputSensitivity(Main.Id*12 + y, x)).ToArray();

            CommunicationViewModel.AddData(lst);

            foreach (var sensitivity in lst)
            {
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