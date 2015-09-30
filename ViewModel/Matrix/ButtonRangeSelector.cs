using EscInstaller.View;

namespace EscInstaller.ViewModel.Matrix
{
    public class ButtonRangeSelector : MatrixRangeSelector
    {
        private readonly PanelViewModel _panel;

        public ButtonRangeSelector(int id, PanelViewModel panel)
            : base(id)
        {
            _panel = panel;
            panel.ButtonChanged += panel_ButtonChanged;
        }

        void panel_ButtonChanged(object sender, RangeChangedEventArgs e)
        {                        
            base.IsSelected = e.NewId == Id;
            Update();
        }


        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {                
                base.IsSelected = value;                
                _panel.OnButtonChanged(new RangeChangedEventArgs() { NewId = Id });                
            }
        }

        public override string DisplayValue
        {
            get
            {
                if (Id == 17)
                    return Panel._matrixButtonFDS;
                if (Id == 16)
                    return string.Format(Panel._matrixButtonPanelAD);
                return string.Format("P{0}: {1} - {2}", Id + 1, (Id * 12 + 1), (Id * 12 + 12));
            }
        }
    }
}