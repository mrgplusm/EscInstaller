#region

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Common;
using EscInstaller.View;
using EscInstaller.ViewModel.EscCommunication.Logic;
using EscInstaller.ViewModel.SDCard;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    /// <summary>
    ///     Used to represent the one of the three ABCD panel selections
    /// </summary>
    public class MessageSelectViewModel : ViewModelBase
    {
        //public const int SelectionB = Common.Commodules.MessageSelection.SelectionB;

        private readonly string[] _headerkeys = {"_matrixGroupFire", "_matrixGroupEvac", "_matrixGroupFDS"};
        private readonly int _id;
        private readonly MainUnitViewModel _main;
        private ObservableCollection<SdFileVM> _mesA;
        private ObservableCollection<SdFileVM> _mesB;

        public MessageSelectViewModel(MainUnitViewModel main, int id)
        {
            _main = main;
            _id = id;
            main.SdCardMessagesReceived += Receiver_SdCardMessagesReceived;
            main.SdCardPositionsReceived += ReceiverOnSdCardPositionsReceived;
        }

        public string HeaderKey
        {
            get
            {
                if (_id > 2) throw new Exception("No message select pane for id " + _id);
                return Panel.ResourceManager.GetString(_headerkeys[_id]);
            }
        }

        public ObservableCollection<SdFileVM> MesA
        {
            get
            {
                return _mesA ??
                       (_mesA =
                           new ObservableCollection<SdFileVM>(
                               LibraryData.FuturamaSys.SdFilesA.OrderByDescending(i => i.Position == 0xff)
                                   .ThenBy(i => i.Position)
                                   .Select(n => new SdFileVM(n, 0))));
            }
        }

        public ObservableCollection<SdFileVM> MesB
        {
            get
            {
                return _mesB ??
                       (_mesB =
                           new ObservableCollection<SdFileVM>(
                               LibraryData.FuturamaSys.SdFilesB.Where(n => n.Position < 0xff)
                                   .Select(n => new SdFileVM(n, 1))));
            }
        }

        public ICommand ResetSelection
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //set all to 0;
                    if (LibraryData.FuturamaSys.Messages == null)
                        return;
                    foreach (var t in LibraryData.FuturamaSys.Messages)
                    {
                        t.ButtonA1 = 0xff;
                        t.ButtonA2 = 0xff;
                        t.ButtonB2 = 0xff;
                        t.ButtonB2 = 0xff;
                        t.ButtonC1 = 0xff;
                        t.ButtonC2 = 0xff;
                        t.ButtonD1 = 0xff;
                        t.ButtonD2 = 0xff;
                    }
                });
            }
        }

        public SdFileVM ButtonA1
        {
            get { return MesA.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonA1); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonA1 = value.Position;
                OnSelectionChanged(_id*4 + 0);

                SendMessages();
            }
        }

        public SdFileVM ButtonA2
        {
            get { return MesB.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonA2); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonA2 = value.Position;
                SendMessages();
            }
        }

        public SdFileVM ButtonB1
        {
            get { return MesA.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonB1); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonB1 = value.Position;

                OnSelectionChanged(_id*4 + 1);
                SendMessages();
            }
        }

        public SdFileVM ButtonB2
        {
            get { return MesB.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonB2); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonB2 = value.Position;

                SendMessages();
            }
        }

        public SdFileVM ButtonC1
        {
            get { return MesA.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonC1); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonC1 = value.Position;
                OnSelectionChanged(_id*4 + 2);
                SendMessages();
            }
        }

        public SdFileVM ButtonC2
        {
            get { return MesB.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonC2); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonC2 = value.Position;
                SendMessages();
            }
        }

        public SdFileVM ButtonD1
        {
            get { return MesA.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonD1); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonD1 = value.Position;
                OnSelectionChanged(_id*4 + 3);
                SendMessages();
            }
        }

        public SdFileVM ButtonD2
        {
            get { return MesB.FirstOrDefault(p => p.Position == LibraryData.FuturamaSys.Messages[_id].ButtonD2); }
            set
            {
                if (value == null) return;
                LibraryData.FuturamaSys.Messages[_id].ButtonD2 = value.Position;

                SendMessages();
            }
        }

        private void ReceiverOnSdCardPositionsReceived(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(() => ButtonA1);
            RaisePropertyChanged(() => ButtonA2);
            RaisePropertyChanged(() => ButtonB1);
            RaisePropertyChanged(() => ButtonB2);
            RaisePropertyChanged(() => ButtonC1);
            RaisePropertyChanged(() => ButtonC2);
            RaisePropertyChanged(() => ButtonD1);
            RaisePropertyChanged(() => ButtonD2);
        }

        private void Receiver_SdCardMessagesReceived(object sender, EventArgs eventArgs)
        {
            foreach (var sdFileVM in MesA.Concat(MesB))
            {
                sdFileVM.UpdateName();
            }
        }

        /// <summary>
        ///     Occurs when user selects one of the messages
        /// </summary>
        public event EventHandler<int> SelectionChanged;

        protected virtual void OnSelectionChanged(int e)
        {
            var handler = SelectionChanged;
            if (handler != null) handler(this, e);
        }

        public async void SendMessages()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (LibraryData.FuturamaSys.Messages == null || LibraryData.FuturamaSys.Messages.Count < 3) return;
            var q = new MessageSelector(_main.DataModel);
            await q.SetMessageData(new Progress<DownloadProgress>());
        }
    }

    public class MessageSelectionChangedEventArgs
    {
        public int ButtonId;
    }
}