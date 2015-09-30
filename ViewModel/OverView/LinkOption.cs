using System.IO;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.OverView
{
    public class LinkOption : ViewModelBase
    {
        private readonly FlowModel _flow;
        private readonly MainUnitViewModel _main;
        private readonly BlLink _linkViewModel;


        public LinkOption(FlowModel flow, MainUnitViewModel main, BlLink linkViewModel)
        {
            _flow = flow;
            _main = main;
            _linkViewModel = linkViewModel;
        }

        public int LinkId
        {
            get { return _flow.Id % 12 + 1; }
        }

        public FlowModel Flow
        {
            get { return _flow; }
        }

        public int Path
        {
            get
            {
                return (int)_flow.Path;
            }
            set
            {
                _flow.Path = (LinkTo)value;
                _main.UpdateLineLink(_flow);
                RaisePropertyChanged(() => Path);
                _linkViewModel.OnLinkChanged(new LinkChangedEventArgs { Flow = Flow });
                CommunicationViewModel.AddData(new SetLinkDemux(_flow.Id, LinkTo.Previous));
            }
        }

        public LinkTo LinkPath
        {
            get { return _flow.Path; }
        }

        public bool IsDelayLinkEnabled
        {
            get { return _flow.Id % 12 > 1 && _flow.Id % 12 < 4; }
        }


    }
}