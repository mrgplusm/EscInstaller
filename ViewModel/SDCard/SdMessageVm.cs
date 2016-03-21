#region

using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Common.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NAudio.Wave;
using File = TagLib.File;

#endregion

namespace EscInstaller.ViewModel.SDCard
{
    public enum MessageType
    {
        Abcd,
        Alarm,
        Preannounce
    }


    public class SdMessageViewModel : ViewModelBase, IEquatable<SdMessageViewModel>, IDragable
    {
        private readonly string[] _nonf =
        {
            "Not Found!", "Wrong format", "Corrupt file", "Not an mp3 file",
            "Filename is empty"
        };

        private bool _initialized;
        private Mp3FileReader _reader;
        private File _v;
        private WaveOut _waveOut; // or WaveOutEvent()
        public Action RemoveThis;

        public SdMessageViewModel(SdCardMessageModel model)
        {
            DataModel = model;
        }

        public SdCardMessageModel DataModel { get; }

        public string Location
        {
            get { return DataModel.Location; }
        }

        public bool ParsedCorrect
        {
            get { return _v != null && !_v.PossiblyCorrupt; }
        }

        public string TrackLength
        {
            get
            {
                string error;
                if (InitializeForPlay(out error))
                    return (_v == null) ? string.Empty : _v.Properties.Duration.ToString();
                return "0";
            }
        }

        public string LongFileName
        {
            get
            {
                //return user filename if any
                if (!string.IsNullOrWhiteSpace(DataModel.LongFileName))
                    return DataModel.LongFileName;
                //if no filename is specified by user, try to use filename instead
                if (string.IsNullOrWhiteSpace(DataModel.Location)) return string.Empty;
                try
                {
                    return Path.GetFileNameWithoutExtension(DataModel.Location);
                }
                catch (ArgumentException ae)
                {
                    Console.WriteLine(ae.Message);
                }
                //neither of the above worked out, this file is not put on sdcard by cardmanager
                //this is used as a selection creteria by sdcard manager whether to display this object
                return string.Empty;
            }
            set
            {
                DataModel.LongFileName = value;
                RaisePropertyChanged(() => LongFileName);
            }
        }

        public ICommand PlayTrack
        {
            get
            {
                return new RelayCommand(Playtrack,
                    () => _waveOut == null || ((_waveOut != null && _waveOut.PlaybackState != PlaybackState.Playing)));
            }
        }

        public ICommand StopTrack
        {
            get
            {
                return new RelayCommand(Stoptrack, () => _waveOut != null
                                                         && _reader != null &&
                                                         _waveOut.PlaybackState == PlaybackState.Playing);
            }
        }

        public Type DataType
        {
            get { return typeof (SdMessageViewModel); }
        }

        public void Remove()
        {
            if (RemoveThis != null)
                RemoveThis();
        }

        public bool Equals(SdMessageViewModel other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Location == Location
                   && other.LongFileName == LongFileName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DataModel != null ? DataModel.GetHashCode() : 0)*23;
            }
        }

        public override string ToString()
        {
            return LongFileName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (GetType() != obj.GetType())
                return false;

            return Equals(obj as SdMessageViewModel);
        }

        /// <summary>
        ///     Call this method to play track. Returns false if not playable
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool InitializeForPlay(out string error)
        {
            if (_initialized)
            {
                error = string.Empty;
                return true;
            }
            if (string.IsNullOrWhiteSpace(DataModel.Location))
            {
                error = _nonf[4];
                return false;
            }

            if (!System.IO.File.Exists(DataModel.Location))
            {
                error = "file does not exist";
                return false;
            }

            try
            {
                _v = File.Create(DataModel.Location);
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            try
            {
                _waveOut = new WaveOut();
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            try
            {
                _reader = new Mp3FileReader(DataModel.Location);
                _waveOut.Init(_reader);
                //_waveOut.Play();
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            if (_v == null)
            {
                error = _nonf[0];
                return false;
            }

            if (_v.PossiblyCorrupt)
            {
                error = _nonf[2];
                return false; //
            }

            if (_v.MimeType != "taglib/mp3")
            {
                error = _nonf[3];
                return false;
            }


            error = "Succesfully initiated player for file";
            _initialized = true;
            return true;
        }

        private void Playtrack()
        {
            string error;
            if (!InitializeForPlay(out error))
            {
                MessageBox.Show(string.Format(SdMessageCard.ErrorPlayMessage, error), SdMessageCard.ErrorPlayTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_waveOut.PlaybackState != PlaybackState.Playing)
                _waveOut.Play();
        }

        private void Stoptrack()
        {
            string error;
            if (!InitializeForPlay(out error)) return;
            if (_waveOut.PlaybackState != PlaybackState.Stopped)
                _waveOut.Stop();
        }
    }
}