using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using Futurama.Model;
using Futurama.Repository;
using Futurama.View;
using GalaSoft.MvvmLight;
using Microsoft.Practices.Unity;
using UpdateControls.XAML;

namespace Futurama.ViewModel
{
    public class OutputNameViewModel : SettingsBaseViewModel<OutputNameModel>
    {
        [InjectionConstructor]
        public OutputNameViewModel(IDataStore data) : base(data)
        {
        }

        public OutputNameViewModel(OutputNameModel data) : base(data)
        {
        }

        public override string SettingName
        {
            get
            {
                return "Output " + (Id + 1) + " Gain";
            }
        }

        public override string DisplaySetting
        {
            get { return string.Empty; }
        }

        public override SetName Name
        {
            get { return SetName.OutputName; }
        }

        public override Color ClBlock
        {
            get
            {
                return Colors.Black;
            }
        }

        public override ICommand EditSettings
        {
            get
            {
                return MakeCommand.Do(OpenWindow<OutputNameView>);
            }
        }
    }
}
