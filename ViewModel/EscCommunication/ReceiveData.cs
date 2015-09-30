using System.Collections.ObjectModel;
using Common;

namespace EscInstaller.ViewModel.EscCommunication
{
    public class ReceiveData : Downloader
    {
#if DEBUG
        public ReceiveData()
            : base(new MainUnitViewModel())
        {
            ItemstoDownload = new ObservableCollection<ItemtoDownload>();
            for (int i = 0; i < 10; i++)
            {
                ItemstoDownload.Add(new ItemtoDownload() { ItemName = "TEST " + i });
            }

        }
#endif

        public ReceiveData(MainUnitViewModel main)
            : base(main)
        {
            PopulateItems();
            foreach (var itemtoDownload in ItemstoDownload)
            {
                AttachHandler(itemtoDownload);
            }
        }

        protected void PopulateItems()
        {

            var dsp = new ItemToEeprom()
            {
                ItemName = "DSP Mirror",
                Function = () => Main.Receiver.GetEeprom(E2PromArea.DspMirror),
                Area = E2PromArea.DspMirror
            };
            Main.Receiver.EepromReceived += dsp.OnCompleted;
            ItemstoDownload.Add(dsp);

            var peq = new ItemToEeprom()
            {
                ItemName = "Peq Data",
                Function = () => Main.Receiver.GetEeprom(E2PromArea.SpeakerRedundancy),
                Area = E2PromArea.SpeakerRedundancy
            };
            Main.Receiver.EepromReceived += peq.OnCompleted;
            ItemstoDownload.Add(peq);

            var sens = new ItemtoDownload()
            {
                ItemName = "Sensitivity data",
                Function = Main.Receiver.GetSensitivityValues
            };
            Main.Receiver.SensitivityDownloaded += sens.OnCompleted;
            ItemstoDownload.Add(sens);

            var matrix = new ItemToEeprom()
            {
                ItemName = "Matrix selection",
                Function = () => Main.Receiver.GetEeprom(E2PromArea.RoutingTable),
                Area = E2PromArea.RoutingTable
            };
            Main.Receiver.EepromReceived += matrix.OnCompleted;
            ItemstoDownload.Add(matrix);

            var installTree = new ItemtoDownload()
            {
                ItemName = "Install tree",
                Function = Main.Receiver.GetHardware
            };
            Main.Receiver.HardwareReceived += installTree.OnCompleted;
            ItemstoDownload.Add(installTree);


            var panels = new ItemToEeprom()
            {
                ItemName = "Installed Panels",
                Function = () => Main.Receiver.GetEeprom(E2PromArea.InstalledPanels),
                Area = E2PromArea.InstalledPanels
            };
            Main.Receiver.EepromReceived += panels.OnCompleted;
            ItemstoDownload.Add(panels);


            var calibration = new ItemToEeprom()
            {
                ItemName = "Calibration data",
                Function = () => Main.Receiver.GetEeprom(E2PromArea.KreisInstall),
                Area = E2PromArea.KreisInstall
            };
            Main.Receiver.EepromReceived += calibration.OnCompleted;
            ItemstoDownload.Add(calibration);


            var names = new ItemToEeprom()
            {
                ItemName = "Preset names",
                Function = () => Main.Receiver.GetEeprom(E2PromArea.PresetNames),
                Area = E2PromArea.PresetNames
            };
            Main.Receiver.EepromReceived += names.OnCompleted;
            ItemstoDownload.Add(names);

            if (Main.Id != 0) return;
            var sdMessages = new ItemtoDownload()
            {
                ItemName = "Sd message names",
                Function = Main.Receiver.GetSdCardMessages
            };
            Main.Receiver.SdCardMessagesReceived += sdMessages.OnCompleted;

            ItemstoDownload.Add(sdMessages);

            var messageSelection = new ItemtoDownload()
            {
                ItemName = "Message Selection",
                Function = Main.Receiver.GetButtonProgramming
            };
            Main.Receiver.SdCardPositionsReceived += messageSelection.OnCompleted;
            ItemstoDownload.Add(messageSelection);
        }



    }
}