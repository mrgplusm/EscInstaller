#region

using System.Globalization;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Markup;
using Common;
using EscInstaller.ImportSpeakers;
using EscInstaller.ViewModel;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

#endregion

namespace EscInstaller
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static Mutex _m;
        private Timer _saveFileTimer;

        private App()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof (FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            SaveBackupFileInit();
            //#if !DEBUG
            //          if (IsSingleInstance())
            //        {
            OpenRegKey();


            return;
            //      }
            MessageBox.Show("The installer/monitor software is already running", "Allready open",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
            //#endif

            //DispatcherHelper.Initialize();
        }

        private void SaveBackupFileInit()
        {
            _saveFileTimer = new Timer {AutoReset = true, Enabled = true, Interval = 300000};
            _saveFileTimer.Elapsed += SaveFileTimerElapsed;
        }

        private void SaveFileTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (LibraryData.SystemIsOpen)
            {
                _saveFileTimer.Stop();
                var q = new SystemFileBackup();
                q.Save();
                _saveFileTimer.Start();
            }
            SpeakerMethods.SaveSpeakerlib();
        }

        private static bool IsSingleInstance()
        {
            try
            {
                // Try to open existing mutex.
                Mutex.OpenExisting("futuramaInstaller");
            }
            catch
            {
                // If exception occurred, there is no such mutex.
                _m = new Mutex(true, "futuramaInstaller");

                // Only one instance.
                return true;
            }
            // More than one instance.
            return false;
        }

        private static void OpenRegKey()
        {
            var z = Registry.CurrentUser.OpenSubKey("Software", true);

            // Add one more sub key
            if (z == null) return;
            if (z.GetValue("Esc Emergency Installer") == null)
            {
                LibraryData.RegisteryKeys = z.CreateSubKey(LibraryData.AppRegKeyName);
            }
        }
    }
}