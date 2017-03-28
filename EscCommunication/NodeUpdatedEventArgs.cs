using System;

namespace EscInstaller.EscCommunication
{
    public class NodeUpdatedEventArgs : EventArgs
    {
        public bool NewValue { get; set; }
        public IDownloadNode Node { get; set; }
    }
}