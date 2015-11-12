#region

using System.Collections;
using System.Linq;
using Common;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class UpdateRoutingTable : EepromDataHandler
    {
        public UpdateRoutingTable(MainUnitViewModel main) : base(main)
        {
        }

        /// <summary>
        ///     4.3 routing table receive.
        ///     For some reason, each button pair of 4 bytes has two bytes extra,
        ///     set to ff whose are not specified in the protocol.
        ///     BroadcastType
        ///     Button Id,
        ///     Flow Id
        /// </summary>
        public void SetRoutingTableValues()
        {
            if (!LibraryData.SystemIsOpen) return;

            var theData = Main.DspCopy.Skip(McuDat.RoutingTable).Take(0x528).ToArray();

            for (var button = 0; button < 216; button++)
            {
                //each button consumes 6 bytes
                var alarm1 = new BitArray(new[] {theData[button*6 + 1], theData[button*6]});
                var alarm2 = new BitArray(new[] {theData[button*6 + 3], theData[button*6 + 2]});

                for (var zone = 0; zone < 12; zone++)
                {
                    var q = new MatrixCell(button, zone + (Main.Id*12));
                    
                    LibraryData.FuturamaSys.Selection[q] = alarm1[zone]
                        ? BroadCastMessage.Alarm1
                        : alarm2[zone] ? BroadCastMessage.Alarm2 : BroadCastMessage.None;
                }
            }
            Main.OnRoutingTableUpdated();
        }

        
    }
}