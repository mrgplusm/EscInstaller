#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

#endregion

namespace EscInstaller.ViewModel.SDCard
{
    public class SdCardVm : ViewModelBase, IDropable
    {
        public static List<SdCardMessageModel> Unsavedmessages = new List<SdCardMessageModel>();
        private bool _allMandatorySelected;
        private ObservableCollection<SdMessageViewModel> _messages;

        /// <summary>
        ///     1 == CardA
        ///     2 == CardB
        ///     3 == no project open
        /// </summary>
        /// <param name="card"></param>
        public SdCardVm(int card)
        {
            Card = card;
        }

        public ObservableCollection<SdMessageViewModel> CardMessages { get; set; }

        public bool AllMandatorySelected
        {
            get { return _allMandatorySelected; }
            set
            {
                _allMandatorySelected = value;
                RaisePropertyChanged(() => AllMandatorySelected);
            }
        }

        public string Header
        {
            get { return Card == 0 ? "Card A" : Card == 1 ? "Card B" : "Not saved"; }
        }

        public ICommand Remove
        {
            get { return new RelayCommand<SdMessageViewModel>(s => s.Remove()); }
        }

        public ICommand AddPredefined
        {
            get { return new RelayCommand<SdMessageViewModel>(message => AddMessages(new[] {message.LongFileName})); }
        }

        public bool IsVisible
        {
            get { return (Card < 2 && LibraryData.SystemIsOpen) || (!LibraryData.SystemIsOpen && Card > 1); }
        }

        public int Card { get; }

        private List<SdCardMessageModel> ControlMessages
        {
            get
            {
                if (Card == 0)
                    return LibraryData.FuturamaSys.MessagesCardA;
                if (Card == 1)
                    return LibraryData.FuturamaSys.MessagesCardB;

                return Unsavedmessages;
            }
        }

        public ObservableCollection<SdMessageViewModel> Messages
        {
            get
            {
                if (_messages != null) return _messages;

                _messages =
                    new ObservableCollection<SdMessageViewModel>(ControlMessages.Select(n => new SdMessageViewModel(n)));

                foreach (var q in _messages)
                {
                    q.RemoveThis = () =>
                    {
                        ControlMessages.Remove(q.DataModel);
                        Messages.Remove(q);
                    };
                }

                return _messages;
            }
        }

        public Type DataType
        {
            get { return typeof (SdMessageViewModel); }
        }

        public void Drop(object data, int index = -1)
        {
            var d = data as SdMessageViewModel;
            if (d == null) return;

            InsertIntoList(d.DataModel, index);
        }

        /// <summary>
        ///     Add a range of messages to the list
        /// </summary>
        /// <param name="paths">Path and filename of message to add</param>
        public void AddMessages(IEnumerable<string> paths)
        {
            //var list = _card == 0 ? ProjectData.MessagesA : ProjectData.MessagesB;
            var errorText = new StringBuilder();

            foreach (var path in paths)
            {
                var z = path.ToLower();

                if (z.ToLower().Contains("16khz"))
                {
                    errorText.AppendLine(SdMessageCard.Error16KhzIsAllreadyAdded);
                    continue;
                }
                if (!z.EndsWith(".mp3"))
                {
                    errorText.AppendLine(SdMessageCard.ErrorAddFileMp3Extension);
                    continue;
                }

                var model = new SdCardMessageModel {Location = path};


                InsertIntoList(model, -1);
            }

            if (errorText.Length > 0)
            {
                Application.Current.Dispatcher.Invoke(
                    new Action(() => MessageBox.Show(errorText.ToString(), SdMessageCard.ErrorAddFileTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error)));
            }
        }

        private void InsertIntoList(SdCardMessageModel message, int index)
        {
            if (index > 250)
            {
                Debug.WriteLine("sd message index out of range, skipped");
                return;
            }

            var insertAt = (index == -1) ? Messages.Count : index;

            var newmodel = new SdCardMessageModel()
            {
                Location = message.Location,
                LongFileName = message.LongFileName
            };

            if (insertAt > ControlMessages.Count) insertAt = ControlMessages.Count;
            ControlMessages.Insert(insertAt, newmodel);

            var newmes = new SdMessageViewModel(newmodel);
            newmes.RemoveThis = () =>
            {
                Messages.Remove(newmes);
                ControlMessages.Remove(newmes.DataModel);
            };
            Messages.Insert(insertAt, newmes);
        }
    }
}