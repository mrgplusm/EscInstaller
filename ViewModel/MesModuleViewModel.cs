using System;
using System.Windows.Media;
using Common.Model;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel
{
    public class MesModuleViewModel : ViewModelBase
    {
        private readonly Action<int, int, int, bool> _changed;
        private readonly MesModuleModel _mesModule;


        public MesModuleViewModel(MesModuleModel model, Action<int, int, int, bool> changed)
        {
            _changed = changed;
            _mesModule = model;
        }


        public bool[] IsEnabled
        {
            get { return _mesModule.IsEnabled ?? (_mesModule.IsEnabled = new bool[3]); }
        }


        public bool IsEnabled1
        {
            get { return IsEnabled[0]; }
            set
            {
                if (IsEnabled[0] == value) return;
                IsEnabled[0] = value;
                RaisePropertyChanged(() => IsEnabled1);
                RaisePropertyChanged(() => Color2);
                RaisePropertyChanged(() => Color3);
                if (_changed != null)
                    _changed.Invoke(0, ColumnIndex, RowIndex, value);
            }
        }

        public bool IsEnabled2
        {
            get { return IsEnabled[1]; }
            set
            {
                if (IsEnabled2 == value) return;

                IsEnabled[1] = value;
                RaisePropertyChanged(() => IsEnabled2);
                RaisePropertyChanged(() => Color1);
                RaisePropertyChanged(() => Color3);
                if (_changed != null)
                    _changed.Invoke(1, ColumnIndex, RowIndex, value);
            }
        }

        public bool IsEnabled3
        {
            get { return IsEnabled[2]; }
            set
            {
                if (IsEnabled[2] == value) return;
                IsEnabled[2] = value;
                RaisePropertyChanged(() => IsEnabled3);
                RaisePropertyChanged(() => Color2);
                RaisePropertyChanged(() => Color1);
                if (_changed != null)
                    _changed.Invoke(2, ColumnIndex, RowIndex, value);
            }
        }

        public int RowIndex
        {
            get { return (_mesModule.ZoneId/12); }
        }

        public int ColumnIndex
        {
            get { return _mesModule.ZoneId%12; }
        }

        public int Name
        {
            get { return _mesModule.ZoneId + 1; }
        }


        public Brush Color1
        {
            get { return IsEnabled2 || IsEnabled3 ? Brushes.LightCoral : Brushes.White; }
        }

        public Brush Color2
        {
            get { return IsEnabled1 || IsEnabled3 ? Brushes.LightCoral : Brushes.White; }
        }

        public Brush Color3
        {
            get { return IsEnabled1 || IsEnabled2 ? Brushes.LightCoral : Brushes.White; }
        }

        public void ChangeWithHandlers(int id, bool value)
        {
            switch (id)
            {
                case 0:
                    IsEnabled1 = value;
                    return;
                case 1:
                    IsEnabled2 = value;
                    return;
                case 2:
                    IsEnabled3 = value;
                    return;
            }
        }
    }
}