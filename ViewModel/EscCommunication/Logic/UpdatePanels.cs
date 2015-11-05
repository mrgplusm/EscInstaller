#region

using System.Collections;
using System.Linq;
using Common;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class AttachedPanelUpdater : EepromDataHandler
    {
        public AttachedPanelUpdater(MainUnitViewModel main) : base(main)
        {
        }

        /// <summary>
        ///     Start address of the first struct:   0xE800
        ///     INT8U   u8_installedFP[32];
        ///     INT8U   u8_installedEP[32];
        ///     INT8U   u8_installedFDS[32];
        /// </summary>
        public void Update()
        {
            var types = new[]
            {
                new BitArray(Main.DspCopy.Skip(McuDat.InstalledPanels).Take(32).Reverse().ToArray()), //fp
                new BitArray(Main.DspCopy.Skip(McuDat.InstalledPanels).Skip(32).Take(32).Reverse().ToArray()), //ep
                new BitArray(Main.DspCopy.Skip(McuDat.InstalledPanels).Skip(64).Take(32).Reverse().ToArray()) //fds
            };

            for (var type = 0; type < 3; type++)
            {
                for (var id = 0; id < 32; id++)
                {
                    var pm =
                        Main.DataModel.AttachedPanelsBus2.FirstOrDefault(
                            se => se.Id == id && se.PanelType == (PanelType) type);
                    if (pm == null) continue;
                    pm.IsInstalled = types[type][id];
                }
            }
        }
    }
}