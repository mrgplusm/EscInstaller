using System.Web.UI.WebControls.Expressions;
using EscInstaller.View;

namespace EscInstaller.ViewModel.Matrix
{
    public class McuSelector : MatrixRangeSelector
    {
        private readonly MainUnitViewModel _main;
        private readonly PanelViewModel _panel;


        public McuSelector(MainUnitViewModel main, PanelViewModel panel)
            : base(main.Id)
        {
            _main = main;
            _panel = panel;
            panel.McuChanged += PanelOnMcuChanged;
        }

        public MainUnitViewModel MainUnitViewModel
        {
            get { return _main; }
        }

        private void PanelOnMcuChanged(object sender, McuChangedEventArgs rangeChangedEventArgs)
        {            
            base.IsSelected = rangeChangedEventArgs.NewMcu.Equals(_main);                       
        }


        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                _panel.OnMcuChanged(new McuChangedEventArgs() { NewMcu = _main });
            }
        }

        public override string DisplayValue
        {
            get
            {
                return string.Format("{2}: {0} - {1}",
                    (Id * 12 + 1),
                    (Id * 12 + 12),
                    Id == 0 ? "M" : "S" + Id);
            }
        }
    }
}