namespace EscInstaller.ViewModel.Matrix
{
    public class McuSelector : MatrixRangeSelector
    {
        private readonly PanelViewModel _panel;

        public McuSelector(MainUnitViewModel main, PanelViewModel panel)
            : base(main.Id)
        {
            MainUnitViewModel = main;
            _panel = panel;
            panel.McuChanged += PanelOnMcuChanged;
        }

        public MainUnitViewModel MainUnitViewModel { get; }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                _panel.OnMcuChanged(new McuChangedEventArgs() {NewMcu = MainUnitViewModel});
            }
        }

        public override string DisplayValue => string.Format("{2}: {0} - {1}",
            (Id*12 + 1),
            (Id*12 + 12),
            Id == 0 ? "M" : "S" + Id);

        private void PanelOnMcuChanged(object sender, McuChangedEventArgs rangeChangedEventArgs)
        {
            base.IsSelected = rangeChangedEventArgs.NewMcu.Equals(MainUnitViewModel);
        }
    }
}