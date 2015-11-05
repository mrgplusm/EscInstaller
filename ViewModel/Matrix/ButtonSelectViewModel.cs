#region

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    public class ButtonSelectViewModel : ViewModelBase
    {
        private readonly FuturamaSysModel _futuramaSys;

        /// <summary>
        ///     0 fire, 1 evac, 2 fds
        /// </summary>
        private readonly int _id;

        private readonly Action _z;

        public ButtonSelectViewModel(int id, FuturamaSysModel futuramaSys, Action z)
        {
            _id = id;
            _futuramaSys = futuramaSys;
            _z = z;
        }

        private static IEnumerable<string> Names
        {
            get
            {
                yield return Panel._matrixGroupFire;
                yield return Panel._matrixGroupEvac;
                yield return Panel._matrixGroupFDS;
                yield return Panel._matrixGroupAnnouncements;
                yield return Panel._matrixGroupAlarmAlert;
            }
        }

        public string HeaderName
        {
            get { return Names.Skip(_id).First(); }
        }
    }
}