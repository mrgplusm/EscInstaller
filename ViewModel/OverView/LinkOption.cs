#region

using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class LinkOption : ViewModelBase
    {
        private readonly BlLink _linkViewModel;
        private readonly MainUnitViewModel _main;

        public LinkOption(FlowModel flow, MainUnitViewModel main, BlLink linkViewModel)
        {
            Flow = flow;
            _main = main;
            _linkViewModel = linkViewModel;
        }

        public int LinkId
        {
            get { return Flow.Id%12 + 1; }
        }

        public FlowModel Flow { get; }

        public int Path
        {
            get { return (int) Flow.Path; }
            set
            {
                Flow.Path = (LinkTo) value;
                _main.UpdateLineLink(Flow);
                RaisePropertyChanged(() => Path);
                _linkViewModel.OnLinkChanged(new LinkChangedEventArgs {Flow = Flow});
                CommunicationViewModel.AddData(new SetLinkDemux(Flow.Id, LinkTo.Previous));
            }
        }

        public LinkTo LinkPath
        {
            get { return Flow.Path; }
        }

        public bool IsDelayLinkEnabled
        {
            get { return Flow.Id%12 > 1 && Flow.Id%12 < 4; }
        }
    }
}