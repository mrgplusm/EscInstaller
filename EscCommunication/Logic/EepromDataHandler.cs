using EscInstaller.ViewModel;

namespace EscInstaller.EscCommunication.Logic
{
    /// <summary>
    ///     Class is used to apply all eeprom values to currently open designfile
    /// </summary>
    internal abstract class EepromDataHandler
    {
        protected readonly MainUnitViewModel Main;

        protected EepromDataHandler(MainUnitViewModel main)
        {
            Main = main;
        }
    }
}