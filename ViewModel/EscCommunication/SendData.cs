#region

using EscInstaller.ViewModel.EscCommunication.downloadItems;
using EscInstaller.ViewModel.EscCommunication.UploadItem;

#endregion

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
            ItemstoDownload.Add(new Auxlinks(Main));
            ItemstoDownload.Add(new Delaysettings(Main));
            ItemstoDownload.Add(new InputSensitivity(Main));
            ItemstoDownload.Add(new Linelinks(Main));

            ItemstoDownload.Add(new MatrixSelection(Main));
            ItemstoDownload.Add(new Messageselection(Main));
            ItemstoDownload.Add(new Peqpresetnames(Main));
            ItemstoDownload.Add(new InOutNames(Main));

            ItemstoDownload.Add(new PeqData(Main));
            ItemstoDownload.Add(new GainSliders(Main));
            ItemstoDownload.Add(new ToneControl(Main));
        }
    }
}