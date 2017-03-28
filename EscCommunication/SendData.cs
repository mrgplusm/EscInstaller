#region

using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.EscCommunication.UploadItem;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication
{
    public class SendData : Downloader
    {
        public SendData(MainUnitViewModel main) : base(main)
        {
            PopulateItems();
            foreach (var itemtoDownload in DataChilds)
            {
                AttachHandler((ItemtoDownload)
                    itemtoDownload);
            }
        }

        protected void PopulateItems()
        {
            DataChilds.Add(new Auxlinks(Main));
            DataChilds.Add(new Delaysettings(Main));
            DataChilds.Add(new InputSensitivity(Main));
            DataChilds.Add(new Linelinks(Main));

            DataChilds.Add(new MatrixSelection(Main));
            DataChilds.Add(new Messageselection(Main));
            DataChilds.Add(new Peqpresetnames(Main));
            DataChilds.Add(new InOutNames(Main));

            DataChilds.Add(new PeqData(Main));
            DataChilds.Add(new GainSliders(Main));
            DataChilds.Add(new ToneControl(Main));
        }
    }
}