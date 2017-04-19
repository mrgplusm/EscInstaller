using System;
using Common.Model;

namespace EscInstaller.ViewModel
{
    public class MainUnitUpdatedEventArgs : EventArgs
    {
        public MainUnitModel MainUnit { get; set; }
    }
}