using System.Globalization;
using Common.Model;
using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.Matrix
{
    public class RowHeaderViewModel : ViewModelBase
    {
        private readonly BlOutput _blOutput;


        public RowHeaderViewModel(BlOutput flowModel)
        {
            _blOutput = flowModel;
        }

        public BlOutput DataModel
        {
            get { return _blOutput; }
        }
        
        public string HeaderName
        {
            get
            {
                if (IsInDesignMode)
                {
                    return "DesignName";
                }

                return string.IsNullOrWhiteSpace(_blOutput.NameOfOutput)
                    ? "Output " + (_blOutput.Id + 1)
                    : _blOutput.NameOfOutput;                
            }
        }
    }
}