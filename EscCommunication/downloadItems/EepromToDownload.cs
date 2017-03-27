#region

using Common;
using EscInstaller.EscCommunication.downloadItems;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.downloadItems
{
    public abstract class EepromToDownload : ItemtoDownload
    {
        protected EepromToDownload(MainUnitViewModel main) : base(main)
        {
        }

        protected abstract E2PromArea Area { get; }

        protected EepromDownloader DownloaderFactory()
        {
            return new EepromDownloader(Main.DspCopy, Main.Id, Area);
        }
    }
}