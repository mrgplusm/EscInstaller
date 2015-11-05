#region

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Model;
using EscInstaller.ImportSpeakers;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.Settings.Peq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

#endregion

namespace EscInstaller.ViewModel
{
    public class LibraryEditorViewModel : ViewModelBase, IDropable, ITabControl
    {
        public ICommand Importbutton
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var z = new SpeakerDataViewModel(new SpeakerDataModel());
                    if (!SpeakerMethods.Import(z)) return;
                    SpeakerMethods.Library.Add(z);
                });
            }
        }

        public ICommand SaveLibrary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var dlg = new SaveFileDialog
                    {
                        DefaultExt = ".xml",
                        Filter = "Speaker library xml (.xml)|*.xml",
                        InitialDirectory = GetDefaultPath(),
                        AddExtension = true
                    };

                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RecentLocationSpeakersMaster))
                        dlg.InitialDirectory = Properties.Settings.Default.RecentLocationSpeakersMaster;

                    var result = dlg.ShowDialog();

                    if (!result.HasValue || !result.Value || string.IsNullOrWhiteSpace(dlg.FileName)) return;
                    Properties.Settings.Default.RecentLocationSpeakersMaster = dlg.FileName;
                    Properties.Settings.Default.Save();


                    SpeakerMethods.ReorderIds();
                    if (FileManagement.SaveCustomSpeakers(SpeakerMethods.Library.Select(n => n.DataModel).ToList(),
                        dlg.FileName))
                        MessageBox.Show("Library saved", "Save", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                });
            }
        }

        public ICommand DeleteCommand
        {
            get { return new RelayCommand<SpeakerDataViewModel>(p => SpeakerMethods.Library.Remove(p)); }
        }

        public ICommand AddNewSpeaker
        {
            get
            {
                return
                    new RelayCommand(() => SpeakerMethods.Library.Add(new SpeakerDataViewModel(new SpeakerDataModel())));
            }
        }

        public Type DataType
        {
            get { return typeof (SpeakerDataViewModel); }
        }

        public void Drop(object data, int index = -1)
        {
            var item = data as SpeakerDataViewModel;
            if (item != null)
            {
                SpeakerMethods.Library.Insert(index, item);
            }
        }

        public int Id
        {
            get { return 100; }
        }

        private static string GetDefaultPath()
        {
            return File.Exists(Properties.Settings.Default.RecentLocationSpeakersMaster)
                ? Properties.Settings.Default.RecentLocationSpeakersMaster
                : FileManagement.DefaultPath;
        }
    }
}