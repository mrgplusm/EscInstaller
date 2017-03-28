#region

using Common;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class DownloadEeprom : DownloadProgress
    {
        public E2PromArea Area { get; set; }
    }
}