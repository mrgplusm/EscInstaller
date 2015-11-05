#region

using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Controls;

#endregion

namespace EscInstaller.ViewModel
{
    public class RecentFilesLogic
    {
        public const int MaxRecentFiles = 5;
        private readonly MainViewModel _main;

        public RecentFilesLogic(MainViewModel main)
        {
            _main = main;

            InitiateStorageCollection();

            //initiate if empty
            _main.RecentFiles.Clear();


            RemoveFirstWhenExceedCound();
            var last = Properties.Settings.Default.RecentFiles.Cast<string>().Reverse();

            foreach (var mi in last.Select(MenuItemRecent))
            {
                _main.RecentFiles.Add(mi);
            }
        }

        private static void InitiateStorageCollection()
        {
            if (Properties.Settings.Default.RecentFiles == null)
                Properties.Settings.Default.RecentFiles = new StringCollection();
        }

        public void AddFile(string file)
        {
            if (Properties.Settings.Default.RecentFiles.Contains(file))
            {
                RemoveFirstWhenExceedCound();
                return;
            }
            Properties.Settings.Default.RecentFiles.Add(file);

            RemoveFirstWhenExceedCound();

            _main.RecentFiles.Insert(0, MenuItemRecent(file));
        }

        private static void RemoveFirstWhenExceedCound()
        {
            while (Properties.Settings.Default.RecentFiles.Count > MaxRecentFiles)
            {
                Properties.Settings.Default.RecentFiles.RemoveAt(0);
            }
        }

        private MenuItem MenuItemRecent(string item)
        {
            var mi = new MenuItem
            {
                Command = _main.OpenFile,
                CommandParameter = item,
                Header = Path.GetFileNameWithoutExtension(item)
            };

            return mi;
        }
    }
}