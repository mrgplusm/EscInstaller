#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Commodules;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.downloadItems
{
    public class EepromDownloader
    {
        private readonly E2PromArea _area;
        private readonly List<byte> _dspCopy;
        private readonly Dictionary<E2PromArea, int> _eepromReceivePackages = new Dictionary<E2PromArea, int>();
        private readonly int _mcuid;

        public EepromDownloader(List<byte> dspCopy, int mcuid, E2PromArea area)
        {
            _dspCopy = dspCopy;
            _mcuid = mcuid;
            _area = area;

            InitializeEepromReceivePackages();
        }

        private void InitializeEepromReceivePackages()
        {
            foreach (var package in Enum.GetValues(typeof (E2PromArea)).Cast<E2PromArea>())
            {
                _eepromReceivePackages.Add(package, 0);
            }
        }

        public async Task GetEeprom(IProgress<DownloadProgress> iProgress, CancellationToken token)
        {
            _eepromReceivePackages[_area] = 0;
            var ar = McuDat.EepromAreaFactory(_area);

            var count = ar.Size/McuDat.BufferSize + 1;
           
            for (var i = ar.Location; i < ar.Location + ar.Size + McuDat.BufferSize; i += McuDat.BufferSize)
            {
                var s = new GetE2PromExt(_mcuid, McuDat.BufferSize, i);
                CommunicationViewModel.AddData(s);
                if (token.IsCancellationRequested) return;
                await s.WaitAsync();

                UpdateEepromData(s);

                iProgress.Report(new DownloadEeprom()
                {
                    Progress = ++_eepromReceivePackages[_area],
                    Total = count,
                    Area = _area
                });
            }
        }

        private void UpdateEepromData(GetE2PromExt data)
        {
            var arr = data.ReceivedEpromValues.ToArray();
            for (var n = 0; n < arr.Length; n++)
            {
                _dspCopy[n + data.StartAddress] = arr[n];
            }
        }
    }
}