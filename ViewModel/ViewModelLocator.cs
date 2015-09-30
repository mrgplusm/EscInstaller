using EscInstaller.ViewModel.SDCard;


namespace EscInstaller.ViewModel
{

    public class ViewModelLocator
    {
                
        
        public static SdLibraryEditorViewModel SdLibraryEditor
        {
            get { return new SdLibraryEditorViewModel(); }
        }


        private static MainViewModel _main;

        public static MainViewModel Main
        {
            get { return _main ?? (_main = new MainViewModel()); }
        }

        //public static BatteryCalcViewModel BatteryCalcViewModel
        //{
        //    get { return BootStrapper.Container.Resolve<BatteryCalcViewModel>(); }
        //}        

        }
}