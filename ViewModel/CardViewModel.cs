#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel
{
    public sealed class CardViewModel : CardBaseViewModel
    {
        private readonly CardModel _card;

        public CardViewModel(CardBaseModel card, MainUnitViewModel mainUnitViewModel)
            : base(card)
        {
            MainUnitViewModel = mainUnitViewModel;
            _card = (CardModel)card;
        }

        public int LinkedChannel => _card.LinkedChannel;

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
    }
}