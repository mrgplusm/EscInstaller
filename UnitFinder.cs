#region

using System;
using System.Diagnostics;
using System.Linq;
using Common;
using Common.Model;

#endregion

namespace EscInstaller
{
    public static class UnitFinder
    {
        public static bool TryGetFlowFromId(int id, FuturamaSysModel model, out FlowModel flow)
        {
            if (model == null)
            {
                Debug.WriteLine("No model ");
                flow = null;
                return false;
            }
            if (model.MainUnits == null)
            {
                Debug.WriteLine("No mainUnits ");
                flow = null;
                return false;
            }
            var mainu = model.MainUnits.FirstOrDefault(c => c.Id == GenericMethods.GetMainunitIdForFlowId(id));
            if (mainu == null)
            {
                Debug.WriteLine("No mainunit for Id");
                flow = null;
                return false;
            }
            var card = mainu.Cards.FirstOrDefault(c => c.Id == GetCardIdFromFlowId(id));
            if (card == null)
            {
                Debug.WriteLine("no card for Id");
                flow = null;
                return false;
            }
            if (card.Flows == null)
            {
                Debug.WriteLine("no flow for Id");
                flow = null;
                return false;
            }

            flow = card.Flows.FirstOrDefault(s => s.Id == id);
            return true;
        }

        public static int GetCardIdFromFlowId(int id)
        {
            if (id >= GenericMethods.StartCountFrom) return 3;
            var flowId = (int) Math.Floor((id%12)/4.0);
            return flowId;
        }
    }
}