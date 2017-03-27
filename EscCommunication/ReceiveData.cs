#region

using System.Collections.ObjectModel;
using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.downloadItems;

#endregion

namespace EscInstaller.EscCommunication
{
    public class ReceiveData : Downloader
    {
#if DEBUG
        public ReceiveData()
            : base(new MainUnitViewModel())
        {
            ItemstoDownload = new ObservableCollection<ItemtoDownload>();
            for (var i = 0; i < 2; i++)
            {
                //ItemstoDownload.Add(new DspDownload()); { ItemName = "TEST " + i });
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
            ItemstoDownload.Add(new Dsp(Main));
            ItemstoDownload.Add(new SpeakerRedundancy(Main));

            ItemstoDownload.Add(new InstalledPanels(Main));
            ItemstoDownload.Add(new CalibrationData(Main));
            ItemstoDownload.Add(new PresetNames(Main));
            ItemstoDownload.Add(new MatrixSelection(Main));
            ItemstoDownload.Add(new Sensitivity(Main));
            ItemstoDownload.Add(new Hardware(Main));

            if (Main.Id != 0) return;
            ItemstoDownload.Add(new SdMessages(Main));
            ItemstoDownload.Add(new SdSelection(Main));
        }
    }
}