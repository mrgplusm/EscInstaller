using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Windows;
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
            AddLines();
        }

        private void RemoveBackupAmplifierLines()
        {
            _main.DiagramObjects.RemoveAll(s =>
            {
                var g = s as LineViewModel;
                if (g == null) return false;

                if (g.First is BlAmplifier && g.Second is BlAmplifier) return true;
                return g.First is BlAmplifier && g.Second is BlBackupAmp;
            });
        }


        private void AddLines()
        {
            var backup = _main.DiagramObjects.OfType<BlBackupAmp>().OrderBy(i => i.Id).ToArray();
            if (backup.Count(s => s.Visibility == Visibility.Visible) < 1) return;

            var amps = _main.DiagramObjects.OfType<BlAmplifier>().OrderBy(i => i.Id).ToArray();

            AddAmplifierLines(amps);
            AddBackupAmpLines(amps, backup);
        }

        void AddBackupAmpLines(IReadOnlyList<BlAmplifier> amps, IReadOnlyList<BlBackupAmp> backup)
        {
            //3 seperate backups
            if (backup[0].Visibility == Visibility.Visible)
            {
                _main.DiagramObjects.Add(new LineViewModel(amps[3], backup[0]));
            }
            else
            {
                _main.DiagramObjects.Add(new LineViewModel(amps[3], amps[4]));
                if (backup[1].Visibility == Visibility.Visible)
                {
                    _main.DiagramObjects.Add(new LineViewModel(amps[7], backup[1]));
                }
                else
                {
                    _main.DiagramObjects.Add(new LineViewModel(amps[7], amps[8]));
                }
            }
            _main.DiagramObjects.Add(new LineViewModel(amps[11], backup[2]));
        }

        void AddAmplifierLines(IEnumerable<BlAmplifier> amps)
        {
            BlAmplifier prevamp = null;
            foreach (var amp in amps)
            {
                if (prevamp != null)
                {
                    if (amp.Id % 4 != 0)
                        _main.DiagramObjects.Add(new LineViewModel(amp, prevamp));
                }
                prevamp = amp;
            }
        }
    }
}