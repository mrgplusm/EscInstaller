#region

using System;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public class MessageSelector : EscLogic
    {
        protected internal MessageSelector(MainUnitModel main)
            : base(main)
        {
        }

        /// <summary>
        ///     Get message selection
        /// </summary>
        /// <param name="iProgress"></param>
        /// <returns></returns>
        public async Task GetButtonProgramming(IProgress<DownloadProgress> iProgress)
        {
            //get button programming first unit
            if (Main.Id != 0) return;

            var s = new GetMessageSelection(Main.Id);

            CommunicationViewModel.AddData(s);

            await s.WaitAsync();

            LibraryData.FuturamaSys.PreannAlrm1 = s.AlarmTrack;
            LibraryData.FuturamaSys.PreannAlrm2 = s.AlertTrack;

            LibraryData.FuturamaSys.PreannEvac = s.PreannounceEP;
            LibraryData.FuturamaSys.PreannExt = s.PreannounceExtAudio;
            LibraryData.FuturamaSys.PreannFds = s.PreannounceFDS;
            LibraryData.FuturamaSys.PreannFp = s.PreannounceFP;

            LibraryData.FuturamaSys.Messages = s.MessageSelectModels;
            iProgress.Report(new DownloadProgress() {Progress = 1, Total = 1});
        }

        /// <summary>
        ///     Send Message selection
        /// </summary>
        /// <returns></returns>
        public async Task SetMessageData(IProgress<DownloadProgress> iProgress)
        {
            var z = new SetMessageSelection(
                LibraryData.FuturamaSys.Messages,
                LibraryData.FuturamaSys.PreannAlrm1,
                LibraryData.FuturamaSys.PreannAlrm2,
                LibraryData.FuturamaSys.PreannFp,
                LibraryData.FuturamaSys.PreannEvac,
                LibraryData.FuturamaSys.PreannFds,
                LibraryData.FuturamaSys.PreannExt,
                Main.Id);

            CommunicationViewModel.AddData(z);

            await z.WaitAsync();
            iProgress.Report(new DownloadProgress() {Progress = 1, Total = 1});
        }
    }
}