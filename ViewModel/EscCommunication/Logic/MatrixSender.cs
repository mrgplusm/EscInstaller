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

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public class MatrixSender : EscLogic
    {
        protected internal MatrixSender(MainUnitModel main) : base(main)
        {
        }

        public async Task SetMatrixSelections(IProgress<DownloadProgress> iProgress)
        {
            var matrixBlocks = 0;

            var list = Enumerable.Range(0, 18).Select(i =>
                new RoutingTable(Enumerable.Range(i*12, 12).ToArray(), Main.Id,
                    LibraryData.FuturamaSys.MatrixSelection)).ToArray();

            CommunicationViewModel.AddData(list);

            foreach (var table in list)
            {
                await table.WaitAsync();

                Interlocked.Increment(ref matrixBlocks);
                iProgress.Report(new DownloadProgress()
                {
                    Progress = ++matrixBlocks,
                    Total = 18
                });
            }
        }
    }
}