namespace Futurama.ViewModel.Settings
{
    public class BlBackupAmp : UnitData
    {
        public BlBackupAmp(LibraryData sys, CardViewModel card)
            : base(sys, card)
        {
        }

        protected override SetName SetName
        {
            get { return SetName.BackupAmp; }
        }

        public override string SettingName
        {
            get { return Main.BackupAmpBlock; }
        }

        public override string DisplaySetting
        {
            get { return string.Empty; }
        }
    }
}