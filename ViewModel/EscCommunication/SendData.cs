using System.Linq;

namespace EscInstaller.ViewModel.EscCommunication
{
    public class SendData : Downloader
    {
        public SendData(MainUnitViewModel main) : base(main)
        {
            PopulateItems();
            foreach (var itemtoDownload in ItemstoDownload)
            {
                AttachHandler(itemtoDownload);
            }
        }

        protected void PopulateItems()
        {
            var toneControl = new ItemtoDownload()
            {
                ItemName = "Tone Control",
                Function = () => Main.Sender.SetToneControls(),
            };

            Main.Sender.ToneControlSend += toneControl.OnCompleted;
            ItemstoDownload.Add(toneControl);

            var inoutputnames = new ItemtoDownload()
            {
                ItemName = "In/output names",
                Function = () => Main.Sender.SetInAndOutputNames(),
            };

            Main.Sender.InOutputNamesSend += inoutputnames.OnCompleted;
            ItemstoDownload.Add(inoutputnames);

            var links = new ItemtoDownload()
            {
                ItemName = "Line links",
                Function = () => Main.Sender.SetLinkDemuxers(),
            };

            Main.Sender.LinkDemuxSend += links.OnCompleted;
            ItemstoDownload.Add(links);

            var delay = new ItemtoDownload()
            {
                ItemName = "Delay settings",
                Function = () => Main.Sender.SetDelaySettings(),
            };

            Main.Sender.DelaySend += delay.OnCompleted;
            ItemstoDownload.Add(delay);

            var emergencySliders = new ItemtoDownload()
            {
                ItemName = "emergencySliders",
                Function = () => Main.Sender.SetSliders(),
            };

            Main.Sender.EmergencySlidersSend += emergencySliders.OnCompleted;
            ItemstoDownload.Add(emergencySliders);

            var inputSensitivity = new ItemtoDownload()
            {
                ItemName = "Input Sensitivity",
                Function = () => Main.Sender.SetInputSensitivity(),
            };
            Main.Sender.InputSensitivitySend += inputSensitivity.OnCompleted;
            ItemstoDownload.Add(inputSensitivity);

            var auxLinks = new ItemtoDownload()
            {
                ItemName = "Aux link",
                Function = () => Main.Sender.SetAuxLink(),
            };
            Main.Sender.AuxLinks += auxLinks.OnCompleted;
            ItemstoDownload.Add(auxLinks);

            var peqNames = new ItemtoDownload()
            {
                ItemName = "Peq preset names",
                Function = () => Main.Sender.PeqNames(),
            };
            Main.Sender.PeqNamesSend += peqNames.OnCompleted;
            ItemstoDownload.Add(peqNames);

            var presets = new ItemtoDownload()
            {
                ItemName = "Peq data + redundancy",
                Function = () => Main.Sender.SetSpeakerPresetData(Main.Sender.GetTotalPresetData().ToList()),
            };
            Main.Sender.SpeakerPeqDownloaded += presets.OnCompleted;
            ItemstoDownload.Add(presets);            

            
            var matrix = new ItemtoDownload()
            {
                ItemName = "Matrix selection",
                Function = () => Main.Sender.SetMatrixSelections(),
            };
            Main.Sender.MatrixSendComplete += matrix.OnCompleted;
            ItemstoDownload.Add(matrix);
            
            var messages = new ItemtoDownload()
            {
                ItemName = "Message selection",
                Function = () => Main.Sender.SetMessageData(),
            };
            Main.Sender.MessageSelectionSend += messages.OnCompleted;
            ItemstoDownload.Add(messages);


        }        
    }
}
