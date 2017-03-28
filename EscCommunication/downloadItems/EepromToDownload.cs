#region

using Common;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public abstract class EepromToDownload : DownloadData
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