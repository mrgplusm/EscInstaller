#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Common;
using Common.Model;
using EscInstaller.Properties;
using EscInstaller.ViewModel.Settings.Peq;
using Microsoft.Win32;

#endregion

namespace EscInstaller.ImportSpeakers
{
    public static class SpeakerMethods
    {
        public const string SpeakerFileName = @"\SpeakerLib.xml";

        public static readonly Dictionary<string, FilterType> FTypes = new Dictionary<string, FilterType>
        {
            {"PEQ", FilterType.Peaking},
            {"HighShelf", FilterType.HighShelf},
            {"LowShelf", FilterType.LowShelf},
            {"Notch", FilterType.Notch},
            {"HighPass", FilterType.ButterworthHp},
            {"LowPass", FilterType.ButterworthLp}
        };

        // private static readonly SimpleAes Crypto = new SimpleAes();

        public static readonly string UserDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                                @"\ESCController";

        public static readonly string MasterFile = AppDomain.CurrentDomain.BaseDirectory + SpeakerFileName;
        public static readonly List<SpeakerDataModel> SystemLibrary = new List<SpeakerDataModel>();
        public static readonly List<SpeakerDataModel> UserLibrary = new List<SpeakerDataModel>();
        private static ObservableCollection<SpeakerDataViewModel> _library;

        static SpeakerMethods()
        {
           
        }

        public static void Initialize()
        {
            CreateSystemLibrary();
            CreateUserLibrary();
        }

        public static ObservableCollection<SpeakerDataViewModel> Library
        {
            get
            {
                if (_library != null) return _library;

                _library = new ObservableCollection<SpeakerDataViewModel>(
                    SystemLibrary.Select(tt => new SpeakerDataViewModel(tt))
                        .Concat(UserLibrary.Select(sl => new SpeakerDataViewModel(sl))));

                return _library;
            }
        }

        public static bool Import(SpeakerDataViewModel speaker)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Peq files (*.peq)|*.peq|Seq files (*.seq)|*.seq",
                DefaultExt = "Peq files (*.peq)|*.peq|Seq files (*.seq)|*.seq"
            };

            if (!string.IsNullOrWhiteSpace(Settings.Default.RecentLocationImport))
                dlg.InitialDirectory = Settings.Default.RecentLocationImport;

            var q = dlg.ShowDialog();
            if (!q.HasValue || !q.Value || string.IsNullOrWhiteSpace(dlg.FileName)) return false;

            Settings.Default.RecentLocationImport = dlg.FileName;
            Settings.Default.Save();

            SpeakerDataModel sp = null;

            try
            {
                var ext = dlg.FileName.Split('.').Last();

                switch (ext)
                {
                    case "seq":
                    {
                        var t = FileManagement.OpenEqFile<Speaker>(dlg.FileName);
                        sp = ParseParametricEq(t);
                    }
                        break;
                    case "peq":
                    {
                        var t = FileManagement.OpenEqFile<ParametricEQ>(dlg.FileName);
                        sp = ParseParametricEq(t);
                    }
                        break;
                }
            }
            catch (Exception e)
            {
#if !DEBUG
                MessageBox.Show(e.Message, "Could not open file", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                return false;
            }

            if (sp == null) return false;

            MessageBoxResult res;
            if (speaker.Biquads < 1) res = MessageBoxResult.OK;
            else if (sp.PEQ.RequiredBiquads() + speaker.RequiredBiquads <=
                     (int) speaker.SpeakerPeqType)
            {
                res =
                    MessageBox.Show("Would you like to replace the old configuration? (Press no to add to existing)",
                        "Import",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }
            else
            {
                res =
                    MessageBox.Show("Would you like to replace the old configuration?",
                        "Import",
                        MessageBoxButton.OKCancel, MessageBoxImage.Question);
            }

            switch (res)
            {
                case MessageBoxResult.Yes:
                case MessageBoxResult.OK:
                    speaker.Clear();
                    speaker.SendMyName();
                    speaker.AddRange(sp.PEQ);
                    break;
                case MessageBoxResult.No:
                    speaker.AddRange(sp.PEQ);
                    break;
            }

            speaker.CopySpeakerName(8.RandomString());

            speaker.RedrawMasterLine();
            return true;
        }

        public static SpeakerDataModel ParseParametricEq(Speaker speaker)
        {
            var ret = new SpeakerDataModel();
            if (!speaker.Bandpass.Any() || !speaker.PEQ.Any() || !speaker.PEQ[0].Band.Any() ||
                speaker.PEQ[0].Band.Count() > 9)
                throw new FileFormatException("The file has no valid data");

            ret.PEQ = new List<PeqDataModel>();
            if (speaker.Bandpass[0].HighPass != null)
                ret.PEQ.Add(Create(speaker.Bandpass[0].HighPass[0], true));

            if (speaker.Bandpass[0].LowPass != null)
                ret.PEQ.Add(Create(speaker.Bandpass[0].LowPass[0], false));

            foreach (var band in speaker.PEQ[0].Band)
            {
                ret.PEQ.Add(Create(band));
            }

            //set id's:
            for (var i = 0; i < ret.PEQ.Count; i++) ret.PEQ[i].Id = i;
            return ret;
        }

        public static SpeakerDataModel ParseParametricEq(ParametricEQ speaker)
        {
            var ret = new SpeakerDataModel {PEQ = new List<PeqDataModel>()};
            for (var i = 1;; i++)
            {
                var t = Create(speaker.Property.Where(s => EndNumber(s.name, i))
                    .ToDictionary(k => k.name.Substring(0, k.name.Length - 1).ToLower(), k => k.value));
                if (t == null) break;
                ret.PEQ.Add(t);
            }
            return ret;
        }

        private static PeqDataModel Create(Bandpass bandfileter, bool isHighpass)
        {
            return new PeqDataModel
            {
                IsEnabled = !bandfileter.ByPass,
                Order = GetOrder(bandfileter.filterType),
                FilterType = isHighpass ? FilterType.ButterworthHp : FilterType.ButterworthLp,
                Frequency = bandfileter.freq
            };
        }

        private static PeqDataModel Create(SpeakerPEQBand speakerPeqBand)
        {
            return new PeqDataModel
            {
                IsEnabled = !speakerPeqBand.ByPass,
                FilterType = speakerPeqBand.FilterType,
                Frequency = speakerPeqBand.freq,
                BandWidth = VerifyBandWidth(speakerPeqBand.width, speakerPeqBand.FilterType),
                Boost = VerifyBoost(speakerPeqBand.gain)
            };
        }

        private static int GetOrder(string bandFilter)
        {
            var r = new Regex("[0-9]{1,2}");
            return int.Parse(r.Match(bandFilter).Value, CultureInfo.InvariantCulture)/3;
        }

        private static PeqDataModel Create(IDictionary<string, string> speakerPeqBand)
        {
            if (speakerPeqBand == null || speakerPeqBand.Count < 6) return null;
            return new PeqDataModel
            {
                IsEnabled = !bool.Parse(speakerPeqBand["bypass"]),
                FilterType = FTypes[speakerPeqBand["type"]],
                Frequency = double.Parse(speakerPeqBand["freq"], CultureInfo.InvariantCulture),
                BandWidth =
                    VerifyBandWidth(double.Parse(speakerPeqBand["qbw"], CultureInfo.InvariantCulture).Bw(),
                        FTypes[speakerPeqBand["type"]]),
                Boost = VerifyBoost(double.Parse(speakerPeqBand["gain"], CultureInfo.InvariantCulture)),
                Order = GetOrder(speakerPeqBand["cut"])
            };
        }

        private static bool EndNumber(string input, int n)
        {
            int o;
            return int.TryParse(input.Last() + "", out o) && o == n;
        }

        private static double VerifyBandWidth(double bw, FilterType filter)
        {
            if (filter == FilterType.HighShelf || filter == FilterType.LowShelf) return (1.0);
            if (bw < .1) return .1;
            return bw > 14 ? 14 : bw;
        }

        private static double VerifyBoost(double boost)
        {
            if (boost > 15) return 15;
            if (boost < -15) return -15;
            return boost;
        }

        /// <summary>
        ///     Reorder id's of speakerEq library
        /// </summary>
        public static void ReorderIds()
        {
            var id = 0;
            foreach (var q in Library)
            {
                q.Id = id++;
            }
        }

        private static void CreateSystemLibrary()
        {
            if (SystemLibrary.Count > 0) return;
            try
            {
                var masterSpeakers = FileManagement.OpenSpeakerLib(MasterFile);
                SystemLibrary.AddRange(masterSpeakers.Distinct());
            }
            catch (Exception e)
            {
                Debug.WriteLine("Master speaker library access" + e);
            }
        }

        public static void SaveSpeakerlib()
        {
            if (!Library.Any(s => s.IsCustom)) return;
            try
            {
                FileManagement.SaveCustomSpeakers(Library.Where(q => q.IsCustom).Select(n => n.DataModel),
                    UserDir + SpeakerFileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Couldn't save library file" + e);
            }
        }

        private static void CreateUserLibrary()
        {
            //add user created speakers to library
            if (UserLibrary.Count > 0) return;
            try
            {
                //check if file exists, create otherwise
                if (!Directory.Exists(UserDir))
                {
                    Directory.CreateDirectory(UserDir);
                }
                if (!File.Exists(UserDir + SpeakerFileName))
                    FileManagement.SaveCustomSpeakers(new List<SpeakerDataModel>(),
                        UserDir + SpeakerFileName);


                var userSpeakers = FileManagement.OpenSpeakerLib(UserDir + SpeakerFileName);
                UserLibrary.AddRange(userSpeakers.Distinct());
            }
            catch (Exception e)
            {
                Debug.WriteLine("User speaker library access " + e);
            }
        }
    }
}