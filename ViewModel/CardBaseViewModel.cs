using System.Collections.Generic;
using Common.Model;
using EscInstaller.ViewModel.Settings;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel
{
    public abstract class CardBaseViewModel : ViewModelBase
    {
        private readonly CardBaseModel _card;
        

        protected CardBaseViewModel(CardBaseModel card)
        {
            _card = card;
        }        

        public MainUnitViewModel MainUnitViewModel { get; protected set; }        


        /// <summary>
        ///     0,1,2,3 - 3 = extension card
        /// </summary>
        public int Id
        {
            get { return _card.Id; }
        }
    }
}