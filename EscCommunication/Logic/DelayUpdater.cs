#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class DelayUpdater : EscLogic
    {
        private volatile int _delayPackages;

        protected internal DelayUpdater(MainUnitModel main) : base(main)
        {
        }

        public async Task SetDelaySettings(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            _delayPackages = 0;
            var q = new[]
            {
                new SetDelay(1, Main.DelayMilliseconds1, Main.Id),
                new SetDelay(2, Main.DelayMilliseconds2, Main.Id)
            };
            
            foreach (var setDelay in q)
            {
                if (token.IsCancellationRequested) return;
                CommunicationViewModel.AddData(setDelay);
                await setDelay.WaitAsync();

                iProgress.Report(new DownloadProgress {Total = 2, Progress = ++_delayPackages});
            }            
        }
    }
}