#region

using System.Collections.ObjectModel;
using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication
{
    public class ReceiveData : Downloader
    {
#if DEBUG
        public ReceiveData()
            : base(new MainUnitViewModel())
        {
            DataChilds = new ObservableCollection<IDownloadableItem>();
            for (var i = 0; i < 2; i++)
            {
                DataChilds.Add(new Dsp(Main)); 
            }
        }
#endif

        public ReceiveData(MainUnitViewModel main)
            : base(main)
        {
            PopulateItems();
            foreach (var itemtoDownload in DataChilds)
            {
                AttachHandler((ItemtoDownload)itemtoDownload);
            }
        }

        protected void PopulateItems()
        {
            DataChilds.Add(new Dsp(Main));
            DataChilds.Add(new SpeakerRedundancy(Main));

            DataChilds.Add(new InstalledPanels(Main));
            DataChilds.Add(new CalibrationData(Main));
            DataChilds.Add(new PresetNames(Main));
            DataChilds.Add(new MatrixSelection(Main));
            DataChilds.Add(new Sensitivity(Main));
            DataChilds.Add(new Hardware(Main));

            if (Main.Id != 0) return;
            DataChilds.Add(new SdMessages(Main));
            DataChilds.Add(new SdSelection(Main));
        }
    }
}