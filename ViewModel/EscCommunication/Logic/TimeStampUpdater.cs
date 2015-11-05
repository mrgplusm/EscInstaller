#region

using System;
using System.Collections.Generic;
using Common;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class TimeStampUpdater : EscLogic
    {
        protected internal TimeStampUpdater(MainUnitModel main) : base(main)
        {
        }

        /// <summary>
        ///     new in 10.7 add timestamp to verify design.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDispatchData> TimeStamp()
        {
            var timestamp = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;

            var data = BitConverter.GetBytes(timestamp);
            Main.TimeStampWrittenToEsc = timestamp;

            yield return new SetE2PromExt(Main.Id, data, McuDat.DesignTimestamp);
        }
    }
}