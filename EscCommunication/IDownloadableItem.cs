using System;
using System.Collections.ObjectModel;

namespace EscInstaller.EscCommunication
{
    public interface IDownloadableItem
    {
        bool Completed { get; set; }
        event EventHandler HasCompleted; 
        string Value { get; }
        bool Checked { get; set; }
        ObservableCollection<IDownloadableItem> DataChilds { get; } 
    }
}