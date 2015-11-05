#region

using EscInstaller.ViewModel.OverView;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.Matrix
{
    public class RowHeaderViewModel : ViewModelBase
    {
        public RowHeaderViewModel(BlOutput flowModel)
        {
            DataModel = flowModel;
        }

        public BlOutput DataModel { get; }

        public string HeaderName
        {
            get
            {
                if (IsInDesignMode)
                {
                    return "DesignName";
                }

                return string.IsNullOrWhiteSpace(DataModel.NameOfOutput)
                    ? "Output " + (DataModel.Id + 1)
                    : DataModel.NameOfOutput;
            }
        }
    }
}