using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Model;
using EscInstaller.ViewModel.Settings;

namespace EscInstaller.ViewModel
{
    public sealed class CardViewModel : CardBaseViewModel
    {
        private readonly CardModel _card;

        public CardViewModel(CardBaseModel card, MainUnitViewModel mainUnitViewModel)
            : base(card)
        {
            MainUnitViewModel = mainUnitViewModel;
            _card = (CardModel) card;

            //if (card.Flows == null || card.Flows.Count != 4) card.Flows = GenFlows(mainUnitViewModel.DataModel, Id);
            //FlowsB = card.Flows.Select(flowModel => new FlowViewModel(flowModel, this)).ToList();

            //if (BlockUnits == null)
            //{
            //    BlockUnits = new List<OverViewData>();
            //    foreach (var t in FlowsB)
            //    {
            //        BlockUnits.AddRange(t.GetOverView);
            //    }
            //}

            //BlockUnits.Add(new BlAuxiliary(this));
        }

        public int LinkedChannel
        {
            get { return _card.LinkedChannel; }
        }

        public bool HasBackup
        {
            get
            {
                if (AttachedBackupAmps.Count < 3) return false;
                //either card has its own backup, or first card has backup.                
                return false; //todo: fix this
                
                //return AttachedBackupAmps[Id] || ((CardViewModel) MainUnitViewModel.Cards[0]).AttachedBackupAmps[Id];
            }
        }

        public BitArray AttachedBackupAmps
        {
            get { return _card.AttachedBackupAmps ?? (_card.AttachedBackupAmps = new BitArray(6)); }
            set { _card.AttachedBackupAmps = value; }
        }

        public bool EightOhms
        {
            get { return _card.EightOhms; }
            set { _card.EightOhms = value; }
        }

        public int AuxSpeaker
        {
            get { return _card.AuxSpeaker; }
            set { _card.AuxSpeaker = value; }
        }

        private static SortedSet<FlowModel> GenFlows(MainUnitModel model, int cardId)
        {
            return
                new SortedSet<FlowModel>(
                    Enumerable.Range(model.Id*12 + cardId*4, 4).Select(n => new FlowModel {Id = n}));
        }
    }
}