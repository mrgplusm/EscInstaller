using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EscInstaller.ImportSpeakers;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight.Command;

using EscInstaller.ViewModel.OverView;


namespace EscInstaller.ViewModel.Settings
{
    public abstract class PeqBaseViewModel : SnapDiagramData
    {


        protected readonly MainUnitViewModel Main;


        protected PeqBaseViewModel(MainUnitViewModel main)
        {
            Main = main;

            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        }

        public abstract SpeakerDataViewModel CurrentSpeaker { get; }



        public ICommand LoadButton
        {
            get { return new RelayCommand<SpeakerDataViewModel>(CurrentSpeaker.Load); }
        }

        public ICommand Remove
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBox.Show("Are you sure to remove this speaker?",
                                        "Remove Speaker",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)

                        SpeakerMethods.Library.Remove(LibrarySpeaker);
                }, () =>
                       LibrarySpeaker != null &&
                       LibrarySpeaker.IsCustom
                    );
            }
        }

        public ICommand Save
        {
            get { return new RelayCommand(SavePeq); }
        }

        private SpeakerDataViewModel _librarySpeaker;

        /// <summary>
        ///     currently selected speaker
        /// </summary>
        public SpeakerDataViewModel LibrarySpeaker
        {
            get { return _librarySpeaker ?? (_librarySpeaker = SpeakerMethods.Library.FirstOrDefault()); }
            set
            {
                _librarySpeaker = value;
                RaisePropertyChanged(() => LibrarySpeaker);
            }
        }

        private void SavePeq()
        {

            if (SpeakerMethods.Library.Any(i => i.SpeakerName == CurrentSpeaker.SpeakerName))
            {
                MessageBox.Show(SpeakerLibrary.SpeakerExistsMessage, SpeakerLibrary.SpeakerExistsTitle,
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var s = CurrentSpeaker.DataModel.Clone();
            SpeakerMethods.Library.Add(new SpeakerDataViewModel(s) { IsCustom = true });
            MessageBox.Show(SpeakerLibrary.SavedSucces, SpeakerLibrary.SavedSuccesTitle, MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        #region Nested type: CurrentSpeakerChange

        #endregion

        protected abstract int PresetId { get; }


        public ICommand ImportButton
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SpeakerMethods.Import(CurrentSpeaker);
                    var t = new SpeakerLogic(CurrentSpeaker.DataModel);
                    CommunicationViewModel.AddData(t.GetPresetData(Id));
                    CommunicationViewModel.AddData(t.PresetNameFactory(Id));
                });
            }
        }

        public abstract string DisplaySetting { get; }


    }
}