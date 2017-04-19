using System.Linq;
using EscInstaller.ViewModel.OverView;

namespace EscInstaller.ViewModel
{
    internal class BackupModules
    {
        private readonly MainUnitViewModel _main;

        public BackupModules(MainUnitViewModel main)
        {
            _main = main;
            _main.BackupConfigChanged += (sender, args) => UpdateBackupLines();
        }

        private void UpdateBackupLines()
        {
            RemoveBackupAmplifierLines();
            AddBackupAmplifierLines();
        }

        private void RemoveBackupAmplifierLines()
        {
            _main.DiagramObjects.RemoveAll(s =>
            {
                var g = s as LineViewModel;
                if (g == null) return false;

                if (g.First is BlAmplifier) return true;
                if (g.Second is BlAmplifier) return true;
                if (g.Second is BlBackupAmp) return true;
                return false;
            });
        }

        private void AddBackupAmplifierLines()
        {
            var backup = _main.DiagramObjects.OfType<BlBackupAmp>().FirstOrDefault();

            if (backup == null) return;

            BlAmplifier prevamp = null;

            foreach (var amp in _main.DiagramObjects.OfType<BlAmplifier>().ToArray())
            {
                if (prevamp != null)
                    _main.DiagramObjects.Add(new LineViewModel(prevamp, amp));
                prevamp = amp;
            }
            _main.DiagramObjects.Add(new LineViewModel(prevamp, backup));
        }
    }
}