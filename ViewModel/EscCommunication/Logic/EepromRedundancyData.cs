#region

using System.Linq;
using EscInstaller.ViewModel.Settings.Peq;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class EepromRedundancyData : EepromDataHandler
    {
        public EepromRedundancyData(MainUnitViewModel main)
            : base(main)
        {
        }

        public void SetRedundancyData()
        {
            foreach (var logic in Main.DataModel.SpeakerDataModels.Select(speaker => new SpeakerLogic(speaker)))
            {
                logic.ParseRedundancyData(Main.DataModel.DspCopy);
            }
            Main.OnRedundancyUpdated();
        }
    }
}