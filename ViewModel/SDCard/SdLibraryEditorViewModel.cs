using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Common;
using Common.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace EscInstaller.ViewModel.SDCard
{
    public class SdLibraryEditorViewModel : ViewModelBase
    {


        public SdLibraryEditorViewModel()        
        {           
            _sdworker.DoWork += SdworkerDoWork;
            _sdworker.ProgressChanged += SdworkerProgressChanged;
            _sdworker.RunWorkerCompleted += WorkCompleted;

            MessagesVms = new ObservableCollection<SdCardVm>()
            {
                new SdCardVm(0),
                new SdCardVm(1),
                new SdCardVm(3),
            };
            
        }

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; }
        }

        public ICommand FileManager
        {
            get
            {
                return new RelayCommand<SdCardVm>((sdvm) =>
                {

                    var dlg = new OpenFileDialog
                    {
                        Filter = "mp3 files (*.mp3)|*.mp3",
                        DefaultExt = "mp3 files (*.mp3)|*.mp3",
                        Multiselect = true,
                        CheckPathExists = true,
                        CheckFileExists = true,

                    };

                    dlg.ShowDialog();
                    sdvm.AddMessages(dlg.FileNames);

                });
            }
        }

        public ObservableCollection<SdCardVm> MessagesVms { get; private set; }

        private static readonly string PreDefinedDir = AppDomain.CurrentDomain.BaseDirectory + "SdPredefined";

        /// <summary>
        /// enumerates the file in the included predefined mp3s directory (excluding 16khz)
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> SdCardPredefines()
        {
            return Directory.EnumerateFiles(PreDefinedDir);
        }

        private const long ExpectedSize = 2048L * 1024 * 1024;

        public void Resetdrives()
        {
            var drives = new List<DriveInfo>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.DriveType != DriveType.Removable)

                        continue;
                    if (drive.TotalSize <= ExpectedSize)
                    {
                        drives.Add(drive);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            SystemDrives.Clear();
            foreach (var driveInfo in (drives))
            {
                SystemDrives.Add(driveInfo);
            }
            Drives.MoveCurrentToFirst();
        }

        private ObservableCollection<SdMessageViewModel> _preDefinedMessages;
        public ObservableCollection<SdMessageViewModel> PreDefinedMessages
        {

            get
            {
                return _preDefinedMessages ?? (_preDefinedMessages = new ObservableCollection<SdMessageViewModel>
                    (SdCardPredefines().Where(f =>
                    {
                        var extension = Path.GetExtension(f);
                        return extension != null && extension.Equals(".mp3") && !f.ToLower().Contains("16khz");
                    }).Select((n, q) => new SdMessageViewModel(new SdCardMessageModel { Location = n, }))));
            }
        }


        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Error != null)
            {
                MessageBox.Show(runWorkerCompletedEventArgs.Error.Message,
                                SdMessageCard.ErrorOccured,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else if (runWorkerCompletedEventArgs.Cancelled)
            {
                MessageBox.Show(SdMessageCard.Canceld,
                                SdMessageCard.CanceldTitle,
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show(SdMessageCard.WriteCompleted,
                                SdMessageCard.WriteCompletedTitle,
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ProgressValue = 0;

        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                RaisePropertyChanged(() => ProgressValue);
            }
        }

        private void SdworkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;
        }

        private void SdworkerDoWork(object sender, DoWorkEventArgs e)
        {
            var sdmessagecardvm = e.Argument as SdCardVm;
            if (sdmessagecardvm == null) return;
            var copiedFiles = new List<string>();
            //create random temp directory
            var dir = Path.GetTempPath() + 8.RandomString() + "\\";
            Directory.CreateDirectory(dir);
            _sdworker.ReportProgress(1);

            //copy file to random temp dir
            var tmpworker = 0; ;

            var messages = (sdmessagecardvm.Card == 0
                ? LibraryData.FuturamaSys.MessagesCardA
                : LibraryData.FuturamaSys.MessagesCardB).Where(q => !string.IsNullOrWhiteSpace(q.Location)).ToArray();

            foreach (var sdMessageViewModel in messages)
            {
                var destPath = (dir + Path.GetFileName(sdMessageViewModel.Location)).NextAvailableFilename();
                File.Copy(sdMessageViewModel.Location, destPath, true);
                copiedFiles.Add(destPath);
                tmpworker += 10 / messages.Length;
                _sdworker.ReportProgress(tmpworker);
                if (!_sdworker.CancellationPending) continue;
                e.Cancel = true;
                return;
            }
            _sdworker.ReportProgress(10);

            //format sd card
            var startInfo = new ProcessStartInfo
            {
                FileName = "format.com",
                Arguments = SystemDrives[Drives.CurrentPosition].Name.Remove(2) + "/fs:fat /v:Entero /q " ,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };

            var p = Process.Start(startInfo);
            var processInputStream = p.StandardInput;
            processInputStream.Write("\r\n");

            _sdworker.ReportProgress(20);
            //those lines are not interesting in case an exception is thrown.
            p.StandardOutput.ReadLine();
            p.StandardOutput.ReadLine();

            var s = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if((SystemDrives[Drives.CurrentPosition].VolumeLabel.ToLower() != "entero") || (SystemDrives[Drives.CurrentPosition].DriveFormat != "FAT" 
                || SystemDrives[Drives.CurrentPosition].AvailableFreeSpace < SystemDrives[Drives.CurrentPosition].TotalFreeSpace ))
            {
                throw new IOException(s);
            }
            
            
            //if (!s.Contains("complete"))

            //copy files to sdcard
            tmpworker = 20;

            //first copy 16khz
            var pilot = SdCardPredefines().FirstOrDefault(qq => qq.ToLower().Contains("16khz"));
            if (pilot == null) throw new FileNotFoundException("Couldn't find 16khz file");

            File.Copy(pilot, SystemDrives[Drives.CurrentPosition].Name + Path.GetFileName(pilot));

            foreach (var file in copiedFiles)
            {
                File.Move(file, SystemDrives[Drives.CurrentPosition].Name + Path.GetFileName(file));
                tmpworker += 70 / messages.Length;
                _sdworker.ReportProgress(tmpworker);
                if (!_sdworker.CancellationPending) continue;
                e.Cancel = true;
                return;
            }
            _sdworker.ReportProgress(90);

            //remove temp dir
            Directory.Delete(dir);
            _sdworker.ReportProgress(100);
        }

        public ICommand CancelWrite
        {
            get { return new RelayCommand(() => _sdworker.CancelAsync(), () => !_sdworker.CancellationPending && _sdworker.IsBusy); }
        }


        private readonly BackgroundWorker _sdworker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };

        private ICollectionView _drives;
        private int _progressValue;

        private ObservableCollection<DriveInfo> _systemdrives;
        private int _selectedTabIndex = LibraryData.SystemIsOpen ? 0 : 2;

       
        public ICollectionView Drives
        {
            get { return _drives ?? (_drives = CollectionViewSource.GetDefaultView(SystemDrives)); }
        }

        public ObservableCollection<DriveInfo> SystemDrives
        {
            get
            {
                if (_systemdrives != null) return _systemdrives;
                _systemdrives = new ObservableCollection<DriveInfo>();
                Resetdrives();
                return _systemdrives;
            }
        }

        public ICommand WriteToSdCard
        {
            get
            {
                return new RelayCommand<SdCardVm>(s =>
                {
                    if (DriveInfo.GetDrives().All(d => d.Name != SystemDrives[Drives.CurrentPosition].Name))
                    {
                        Resetdrives();

                        return;
                    }

                    //synchronise matrix messages (only if system file opened)
                    //SynchroniseWithMatrix();
                    _sdworker.RunWorkerAsync(s);

                }, (s) => SystemDrives.Any() && !_sdworker.IsBusy);
            }
        }


    }
}
