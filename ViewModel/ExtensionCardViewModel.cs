#region

using Common.Model;

#endregion

namespace EscInstaller.ViewModel
{
    public sealed class ExtensionCardViewModel : CardBaseViewModel
    {
        public ExtensionCardViewModel(ExtensionCardModel card, MainUnitViewModel mainUnitViewModel)
            : base(card)
        {
            MainUnitViewModel = mainUnitViewModel;
            //DrawLines();
            //if (card.Flows == null || card.Flows.Count != 5) card.Flows = GenFlows();
            //FlowsB = card.Flows.Select(flowModel => new FlowViewModel(flowModel, this)).ToList();

            //if (BlockUnits != null) return;
            //BlockUnits = new List<OverViewData>();
            //foreach (var t in FlowsB)
            //{
            //    BlockUnits.AddRange(t.GetOverView);
            //}
        }
    }
}