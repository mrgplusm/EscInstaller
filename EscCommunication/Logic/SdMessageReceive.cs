#region

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class SdMessageReceive : EscLogic
    {
        /// <summary>
        ///     skip 0 and 1(16khz) start at position 2
        /// </summary>
        private const int SdFirstMessageToDownload = 2;

        private volatile int _messageCount;
        private volatile int _sdMessagePackages;

        protected internal SdMessageReceive(MainUnitModel main)
            : base(main)
        {
        }

        /// <summary>
        ///     clear stored messages
        /// </summary>
        private static void ClearMessages()
        {
            if (LibraryData.FuturamaSys == null) return;

            foreach (var b in LibraryData.FuturamaSys
                .SdFilesB
                .Concat(LibraryData.FuturamaSys.SdFilesA)
                .Where(qq => qq.Position != 0xff))
            {
                b.Name = null;
            }
        }

        private async Task<int[]> GetSdMessageCount()
        {
            var q = new SdCardMessageCount();
            CommunicationViewModel.AddData(q);

            await q.WaitAsync();

            _messageCount = q.CountA + q.CountB + 1 + (SdCardMessageName.TrackShift*2);
            return new[] {q.CountA, q.CountB};
        }

        public async Task GetSdCardMessages(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            ClearMessages();
            var lists = await GetSdMessageCount();

            foreach (var list in lists.Select((count, n) => new {count, n}))
            {
                for (var i = SdFirstMessageToDownload; i < list.count; i++)
                {
                    if (token.IsCancellationRequested) return;
                    var s = new SdMessageNameLoader(list.n, i);
                    await s.GetMessageName();
                    iProgress.Report(new DownloadProgress() {Progress = ++_sdMessagePackages, Total = _messageCount});
                }
            }

            iProgress.Report(new DownloadProgress
            {
                Progress = ++_sdMessagePackages,
                Total = _messageCount
            });
        }
    }
}