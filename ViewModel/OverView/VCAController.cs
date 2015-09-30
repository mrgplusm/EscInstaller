using Common.Model;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.OverView
{
    public class VCAController : ViewModelBase
    {
        private readonly FlowModel _flow;


        public VCAController(FlowModel flow)
        {
            _flow = flow;
        }

        public bool IsAvailable
        {
            get
            {
#if DEBUG
                return true;
#endif
                return _flow.VCAControllerInstalled;
            }
            set
            {
                _flow.VCAControllerInstalled = value;
                RaisePropertyChanged(() => IsAvailable);
            }
        }
    }
}