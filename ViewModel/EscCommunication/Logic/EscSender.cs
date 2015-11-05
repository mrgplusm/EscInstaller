#region

using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    public sealed class EscSender
    {
        private readonly MainUnitModel _main;

        public EscSender(MainUnitModel main)
        {
            _main = main;
        }

        /// <summary>
        ///     List of input deviation settings which aren't set to 0
        /// </summary>
        /// <returns></returns>
        //todo: implement in batch
        public IEnumerable<IDispatchData> InputDeviations()
        {
            var flows = _main.Cards.OfType<CardModel>().SelectMany(o => o.Flows).ToList();
            foreach (var result in flows)
            {
                if (result.DeviationHp != 0)
                    yield return new SetDeviation(result.Id, result.DeviationHp, false);
                if (result.DeviationLp != 0)
                    yield return new SetDeviation(result.Id, result.DeviationLp, true);
            }
        }
    }


    public class DownloadProgress
    {
        public int Total { get; set; }
        public int Progress { get; set; }
    }

    public class DownloadEeprom : DownloadProgress
    {
        public E2PromArea Area { get; set; }
    }
}