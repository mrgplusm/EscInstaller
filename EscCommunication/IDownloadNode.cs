using System;
using System.Collections.ObjectModel;
using EscInstaller.EscCommunication.Logic;

namespace EscInstaller.EscCommunication
{
    public interface IDownloadNode
    {
        bool IsCompleted { get; set; }
        event EventHandler<NodeUpdatedEventArgs> Completed; 
        string Value { get; }
        bool IsChecked { get; set; }
        event EventHandler<NodeUpdatedEventArgs> Checked;        
        ObservableCollection<IDownloadNode> DataChilds { get; } 
        double ProgressBar { get; }        
    }
}