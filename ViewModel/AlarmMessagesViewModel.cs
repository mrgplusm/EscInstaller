using System.Collections.ObjectModel;
using System.Linq;
using Common;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.EscCommunication;
using EscInstaller.ViewModel.Matrix;
using EscInstaller.ViewModel.SDCard;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel
{
    public class AlarmMessagesViewModel : ViewModelBase
    {
        private readonly MainUnitViewModel _main;

        public AlarmMessagesViewModel(MainUnitViewModel main)
        {
            _main = main;
            InitializeMessageList();
            _main.Receiver.SdCardMessagesReceived += Receiver_SdCardMessagesReceived;
            _main.Receiver.SdCardPositionsReceived += Receiver_SdCardPositionsReceived;
        }

        void Receiver_SdCardPositionsReceived(object sender, System.EventArgs e)
        {
            RaisePropertyChanged(()=> PreannAlrm1);
            RaisePropertyChanged(() => PreannAlrm2);
            RaisePropertyChanged(() => PreannEvac);
            RaisePropertyChanged(() => PreannExt);
            RaisePropertyChanged(() => PreannFds);
            RaisePropertyChanged(() => PreannFp);
        }

        void Receiver_SdCardMessagesReceived(object sender, DownloadProgressEventArgs e)
        {
            if(e.Progress < e.Total) return;
            foreach (var messageSelectViewModel in MesA.Concat(MewWithNoMessage))
            {
                messageSelectViewModel.UpdateName();
            }
        }

        private void InitializeMessageList()
        {
            Messages = new ObservableCollection<MessageSelectViewModel>(Enumerable.Range(0, 3)
                .Select(a => new MessageSelectViewModel(_main, a)));
        }

        private static int MessageForNumber(int number, bool isAlarmMessage = false)
        {
            if (number == 0xff)
                return 0;
            return number - (isAlarmMessage ? 2 : 1);
        }

        private static int NumberForMessage(int number, bool isAlarmMessage = false)
        {
            if (number == 0)
                return 0xff;
            return number + (isAlarmMessage ? 2 : 1);
        }


        public int PreannAlrm1
        {
            get
            {
                return MessageForNumber(LibraryData.FuturamaSys.PreannAlrm1, true);
            }
            set
            {
                if (value == -1) return;
                LibraryData.FuturamaSys.PreannAlrm1 = NumberForMessage(value, true);
                SendMessages();
            }
        }

        public int PreannAlrm2
        {
            get
            {
                return MessageForNumber(LibraryData.FuturamaSys.PreannAlrm2, true);
            }
            set
            {
                if (value == -1) return;
                LibraryData.FuturamaSys.PreannAlrm2 = NumberForMessage(value, true);
                SendMessages();
            }
        }


        public int PreannFp
        {
            get { return MessageForNumber(LibraryData.FuturamaSys.PreannFp); }
            set
            {
                if (value == -1) return;
                LibraryData.FuturamaSys.PreannFp = NumberForMessage(value);
                SendMessages();
            }
        }

        public int PreannEvac
        {
            get { return MessageForNumber(LibraryData.FuturamaSys.PreannEvac); }
            set
            {
                if (value == -1) return;
                LibraryData.FuturamaSys.PreannEvac = NumberForMessage(value);
                SendMessages();
            }
        }

        public int PreannFds
        {
            get
            {

                return MessageForNumber(LibraryData.FuturamaSys.PreannFds);
            }
            set
            {
                if (value == -1) return;
                LibraryData.FuturamaSys.PreannFds = NumberForMessage(value);
                SendMessages();
            }
        }



        public int PreannExt
        {
            get
            {

                return MessageForNumber(LibraryData.FuturamaSys.PreannExt);
            }
            set
            {
                if (value == -1) return;
                LibraryData.FuturamaSys.PreannExt = NumberForMessage(value);
                SendMessages();
            }
        }

        public ObservableCollection<MessageSelectViewModel> Messages { get; private set; }


        private ObservableCollection<SdFileVM> _mesA;
        public ObservableCollection<SdFileVM> MesA
        {
            get
            {
                return _mesA ??
                       (_mesA =
                           new ObservableCollection<SdFileVM>(
                               LibraryData.FuturamaSys.SdFilesA.Where(n => n.Position < 0xff).Select(n => new SdFileVM(n, 0))));
            }
        }

        private ObservableCollection<SdFileVM> _mesWithNoMessage;
        public ObservableCollection<SdFileVM> MewWithNoMessage
        {
            get
            {
                return _mesWithNoMessage ??
                       (_mesWithNoMessage =
                           new ObservableCollection<SdFileVM>(
                               LibraryData.FuturamaSys.SdFilesA.OrderByDescending(t => t.Position == 0xff).ThenBy(t => t.Position)
                               .Select(n => new SdFileVM(n, 0, true))));
            }
        }


        public void SendMessages()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Messages == null || LibraryData.FuturamaSys.Messages.Count < 3) return;
            _main.Sender.SetMessageData();
        }
    }
}