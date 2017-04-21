#region

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class HardwareReceive : EscLogic
    {
        public HardwareReceive(MainUnitModel main)
            : base(main)
        {
        }

        /// <summary>
        ///     Set the downloaded hardware settigns in the given mainunit
        /// </summary>
        public async Task GetHardware(IProgress<DownloadProgress> iProgress)
        {
            var data = new GetInstallTree(Main.Id);
            CommunicationViewModel.AddData(data);

            await data.WaitAsync();

            //set amount of expension cards            
            Main.ExpansionCards = Main.InputSensitivity.Skip(4)
                .TakeWhile(sense => sense == InputSens.High || sense == InputSens.Low || sense == InputSens.None)
                .Count() >> 2;

            SetBackupConfig(data);

            SetChannels(data);

            iProgress.Report(new DownloadProgress() { Progress = 1, Total = 1 });
        }

        private void SetChannels(GetInstallTree data)
        {
            var sp = data.GetChannels().ToList();

            //loop through cards and channels;
            var id = 0;
            foreach (var result in Main.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                result.HasAmplifier = sp[id].Item1 || sp[id].Item2 || sp[id].Item3 || sp[id].Item4;
                result.AmplifierOperationMode = result.HasAmplifier ? sp[id].Item6 : AmplifierOperationMode.Unknown;
                result.AttachedChannels = new[] { sp[id].Item1, sp[id].Item2, sp[id].Item3, sp[id].Item4 };
                id++;
            }
        }

        private void SetBackupConfig(GetInstallTree data)
        {
            //set backupamps
            Main.BackupAmp = data.BackupConfig.ToList();            
        }
    }


}