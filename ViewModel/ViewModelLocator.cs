#region

using EscInstaller.ViewModel.SDCard;

#endregion

namespace EscInstaller.ViewModel
{
    public class ViewModelLocator
    {
        private static MainViewModel _main;

        public static SdLibraryEditorViewModel SdLibraryEditor
        {
            get { return new SdLibraryEditorViewModel(); }
        }

        public static MainViewModel Main
        {
            get { return _main ?? (_main = new MainViewModel()); }
        }
    }
}