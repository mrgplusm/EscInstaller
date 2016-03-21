#region

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Commodules;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.EscCommunication.Logic
{
    internal class UpdatePresetNames : EepromDataHandler
    {
        public UpdatePresetNames(MainUnitViewModel main) : base(main)
        {
        }

        /// <summary>
        ///     Put the dsp cache into readable names
        /// </summary>
        public void SetInOutputNames()
        {
            var names = Names(Main.DspCopy).ToArray();
            var i = 0;
            foreach (var flow in Main.DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                flow.NameOfInput = names[i].Trim();
                flow.NameOfOutput = names[i + 12].Trim();

                i++;
            }
            i = 0;
            foreach (
                var speakerDataModel in Main.SpeakerDataModels.Where(s => s.SpeakerPeqType != SpeakerPeqType.BiquadsMic)
                )
            {
                speakerDataModel.SpeakerName = names[i++ + 24].Trim();
            }
            Main.OnPresetNamesUpdated();
        }

        /// <summary>
        ///     Reconstruct the system names by splitting byte array
        /// </summary>
        /// <param name="data"> 16 dividable byte bytestream (624) </param>
        /// <returns> 00-11 input names (12) 12-23 output names (12) 24-39 preset names (15) </returns>
        private static IEnumerable<string> Names(IEnumerable<byte> data)
        {
            var namesArray = data.Skip(0xA000).Take(16*39).ToArray();
            for (var i = 0; i < namesArray.Length; i += 16)
            {
                yield return Encoding.Default.GetString(namesArray.Skip(i).Take(13)
                    .Select(n => (n < 32 || n > 126) ? (byte) 0x20 : n).ToArray());
            }
        }
    }
}