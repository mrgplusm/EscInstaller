using System;
using System.Collections.ObjectModel;
using System.Linq;
using Common;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;
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
            _main.SdCardPositionsReceived += Receiver_SdCardMessagesReceived;
            _main.SdCardPositionsReceived += Receiver_SdCardPositionsReceived;
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

        void Receiver_SdCardMessagesReceived(object sender, EventArgs eventArgs)
        {
            
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



        public SdFileVM PreannAlrm1
        {
            get { return MesA.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.PreannAlrm1); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.PreannAlrm1 = value.Position;

                SendMessages();
            }
        }

        public SdFileVM PreannAlrm2
        {
            get { return MesA.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.PreannAlrm2); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.PreannAlrm2 = value.Position;

                SendMessages();
            }
        }

        public SdFileVM PreannFp
        {
            get { return MewWithNoMessage.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.PreannFp); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.PreannFp = value.Position;

                SendMessages();
            }
        }

        public SdFileVM PreannEvac
        {
            get { return MewWithNoMessage.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.PreannEvac); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.PreannEvac = value.Position;

                SendMessages();
            }
        }

        public SdFileVM PreannFds
        {
            get { return MewWithNoMessage.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.PreannFds); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.PreannFds = value.Position;

                SendMessages();
            }
        }

        public SdFileVM PreannExt
        {
            get { return MewWithNoMessage.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.PreannExt); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.PreannExt = value.Position;

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


        public async void SendMessages()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Messages == null || LibraryData.FuturamaSys.Messages.Count < 3) return;
            var q = new MessageSelector(_main.DataModel);
            await q.SetMessageData(new Progress<DownloadProgress>());            
        }
    }
}