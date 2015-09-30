using System;
using Common.Model;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Common.Commodules;

namespace EscInstaller.ViewModel.Settings
{
    public class AuxLinkOption : ViewModelBase
    {
        private readonly FlowModel _flow;
        private readonly CardModel _card;
        private readonly MainUnitViewModel _main;
        private readonly BlLink _link;

        public AuxLinkOption(FlowModel flow, CardModel card, MainUnitViewModel main, BlLink link)
        {
            _card = card;
            _main = main;
            _link = link;
            _flow = flow;

            Messenger.Default.Register<int>(this, "InpName", i =>
                {
                    if (i != _flow.Id) return;
                    RaisePropertyChanged(() => Content);
                });
            link.LinkChanged += LinkOnLinkChanged;
        }

        private void LinkOnLinkChanged(object sender, LinkChangedEventArgs linkChangedEventArgs)
        {
            if (linkChangedEventArgs.Flow != null && linkChangedEventArgs.Flow.Equals(_flow))
            {
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public bool IsChecked
        {
            get
            {
                return _card.LinkedChannel == _flow.Id % 12 % 4;
            }
            set
            {
                if (value)
                {
                    _card.LinkedChannel = _flow.Id % 12 % 4;
                    CommunicationViewModel.AddData(SetAuxLink(_main.Id, _card));
                }
                RaisePropertyChanged(() => IsChecked);
            }
        }

        public static AuxLink SetAuxLink(int destination, CardModel card)
        {
            return new AuxLink(destination, card.Id, card.LinkedChannel);            
        }


        public string Content
        {
            get
            {
                return string.IsNullOrWhiteSpace(_flow.NameOfOutput)
                    ? Auxiliary._auxCh14 + " " + (1 + _flow.Id)
                    : (1 + _flow.Id) + ": " + _flow.NameOfOutput;
            }
        }

        public bool IsEnabled
        {
            get { return _flow.Path == LinkTo.No; }
        }
    }
}