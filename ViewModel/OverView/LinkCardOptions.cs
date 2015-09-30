using System;
using System.Collections.ObjectModel;
using System.Linq;
using Common.Model;
using EscInstaller.ViewModel.Settings;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.OverView
{
    public class LinkCardOption : ViewModelBase
    {
        private readonly CardModel _card;
        private readonly MainUnitViewModel _main;
        private readonly BlLink _link;

        public LinkCardOption(CardModel card, MainUnitViewModel main, BlLink link)
        {
            _card = card;
            _main = main;
            _link = link;
            _main.CardsUpdated += (sender, args) => RaisePropertyChanged(() => IsVisible);
        }

        public string DisplayId
        {
            get { return (_card.Id + 1).ToString("N0"); }
        }

        

        private ObservableCollection<AuxLinkOption> _auxLinkOptions;
        public ObservableCollection<AuxLinkOption> AuxLinkOptions
        {
            get
            {
                return _auxLinkOptions ?? (_auxLinkOptions = new ObservableCollection<AuxLinkOption>(_card.Flows
                    .Select(g => new AuxLinkOption(g, _card, _main, _link))));
            }
        }

        public bool IsVisible
        {
            get { return _main.DataModel.ExpansionCards >= _card.Id; }
        }
    }

    public class LinkChangedEventArgs : EventArgs
    {
        public FlowModel Flow { get; set; }
    }
}