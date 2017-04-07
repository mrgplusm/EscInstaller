#region

using Common;
using EscInstaller.EscCommunication;
using EscInstaller.ViewModel.SDCard;

#endregion

namespace EscInstaller.ViewModel
{
    public class ViewModelLocator
    {
        private static MainViewModel _main;
        private static Communication _communication;

        public static SdLibraryEditorViewModel SdLibraryEdior
        {
            get { return new SdLibraryEditorViewModel(); }
        }

        public static MainViewModel Main => _main ?? (_main = new MainViewModel());

        public static Communication Communication => _communication ?? (_communication = new Communication());
    }
}