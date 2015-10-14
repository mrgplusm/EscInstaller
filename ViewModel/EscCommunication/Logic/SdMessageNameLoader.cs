using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class SdMessageNameLoader
    {
        private readonly int _card;

        private readonly List<SdFileModel>[] _sdFileModels =
        {
            LibraryData.FuturamaSys.SdFilesA,
            LibraryData.FuturamaSys.SdFilesB
        };

        private readonly int _track;

        /// <param name="card">Either 0 or 1</param>
        /// <param name="track">message id to receive</param>
        public SdMessageNameLoader(int card, int track)
        {
            _card = card;
            _track = track;
        }

        private static bool MessageCanBeDownloaded()
        {
            if (LibraryData.FuturamaSys != null && LibraryData.FuturamaSys.SdFilesB != null &&
                LibraryData.FuturamaSys.SdFilesA != null && LibraryData.FuturamaSys.SdFilesA.Count >= 0xff &&
                LibraryData.FuturamaSys.SdFilesB.Count >= 0xff) return true;
            Debug.WriteLine("Couldn't store message names, futuramasys or messagesA/B null");
            return false;
        }


        public async Task GetMessageName()
        {
            if (!MessageCanBeDownloaded()) return;

            for (var n = 0; n < 3; n++)
            {
                var download = CardNameFactory(_card, _track);

                await download.WaitAsync();

                if (!CheckName(download))
                    continue;

                _sdFileModels[download.CardNumber][download.TrackNumber -4].Name =
                    download.TrackName;
                break;
            }
        }

        /// <summary>
        ///     esc sometimes gives erroneously wrong response. If name contains 16khz, redownload a few times until succes.
        /// </summary>
        /// <param name="download"></param>
        /// <returns>true if name is not 16khz</returns>
        private static bool CheckName(SdCardMessageName download)
        {
            if (!download.TrackName.ToLower().Contains("16khz")) return true;
            Debug.WriteLine("Warning: Track {0} contains 16khz! Re-downloading..", download.TrackNumber);
            return false;
        }

        private static SdCardMessageName CardNameFactory(int card, int track)
        {
            var download = new SdCardMessageName(card, track);
            CommunicationViewModel.AddData(download);
            return download;
        }
    }
}