#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EscInstaller.EscCommunication.Logic;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.EscCommunication
{
    public class DownloadNode : ViewModelBase, IProgress<DownloadProgress>, IDownloadNode
    {
        private bool _isChecked = true;
        private bool _isCompleted;

        protected DownloadNode()
        {
            DataChilds = new ObservableCollection<IDownloadNode>();
            _cancellation = new CancellationTokenSource();
        }

        

        public virtual string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged(() => Value);
            }
        }

        /// <summary>
        ///     Download all items of this esc
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnChecked(new NodeUpdatedEventArgs() { Node = this, NewValue = value });
                foreach (var child in DataChilds) child.IsChecked = value;
                RaisePropertyChanged(() => IsChecked);
            }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;
                RaisePropertyChanged(() => IsCompleted);
            }
        }

        public void AddChild(IDownloadNode node)
        {
            AttachHandlers(node);
            DataChilds.Add(node);
        }

        public void RemoveChild(IDownloadNode node)
        {
            RemoveHandlers(node);
            DataChilds.Remove(node);
        }

        public void ClearChilds()
        {
            foreach (var child in DataChilds.ToArray())
            {
                RemoveChild(child);
            }
        }

        public ObservableCollection<IDownloadNode> DataChilds { get; protected set; }

        protected void ResetNodes(IList<IDownloadNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (var node in nodes.Cast<DownloadNode>())
            {
                node.Reset();
                ResetNodes(node.DataChilds);
            }
        }

        protected void Cancel(IList<IDownloadNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (var node in nodes.Cast<DownloadNode>())
            {
                node.Cancellation.Cancel();
                Cancel(node.DataChilds);
            }
        }

        protected async void StartDownload(IList<IDownloadNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (var node in nodes.Cast<DownloadNode>())
            {
                node.Cancellation.Dispose();
                node._cancellation = new CancellationTokenSource();
            }
            foreach (var node in nodes.Cast<DownloadNode>())
            {
                if(node.IsChecked)
                    await node.Function;
                StartDownload(node.DataChilds);
            }
        }

        private double _progressBar;
        private string _value = string.Empty;
        private CancellationTokenSource _cancellation;

        /// <summary>
        ///     Indicates progress from 0 - 100;
        /// </summary>
        public double ProgressBar
        {
            get { return _progressBar; }
            private set
            {
                _progressBar = value;
                RaisePropertyChanged(() => ProgressBar);
            }
        }

        protected static bool TraverseCompleted(IList<IDownloadNode> childs)
        {
            if (childs == null || !childs.Any()) return true;
            return childs.All(n => (!n.IsChecked || n.IsCompleted) && TraverseCompleted(n.DataChilds));
        }

        public event EventHandler<NodeUpdatedEventArgs> Completed;
        public event EventHandler<NodeUpdatedEventArgs> Checked;

        public void Report(DownloadProgress e)
        {
            if (e.Progress >= e.Total - .01)
            {
                IsCompleted = true;
                ProgressBar = 100;
                OnCompleted(new NodeUpdatedEventArgs() { NewValue = true, Node = this });
            }
            else
            {
                ProgressBar = (double)e.Progress / e.Total * 100;
            }
        }

        /// <summary>
        ///     to execute when this item is selected
        /// </summary>
        protected virtual Task Function { get { return Task.Run(() => { }); } }

        protected CancellationTokenSource Cancellation
        {
            get { return _cancellation; }
            set { _cancellation = value; }
        }

        protected Progress<DownloadProgress> ProgressFactory()
        {
            return new Progress<DownloadProgress>(Report);
        }

        protected virtual void Done() { }

        public void Reset()
        {
            ProgressBar = 0;
            IsCompleted = false;
        }

        public virtual Progress<DownloadProgress> Reporting => ProgressFactory();

        protected void AttachHandlers(IDownloadNode node)
        {
            node.Completed += CompletedEventReceived;
            node.Checked += CheckedEventReceived;
            ((DownloadNode)node).Reporting.ProgressChanged += ChildProgressChanged;
        }

        private void ChildProgressChanged(object sender, DownloadProgress downloadProgress)
        {
            var total = 100 * DataChilds.Count;
            var progress = (int)DataChilds.Sum(s => s.ProgressBar);
            ((IProgress<DownloadProgress>)Reporting).Report(new DownloadProgress() { Progress = progress, Total = total });
        }

        protected void RemoveHandlers(IDownloadNode node)
        {
            node.Completed -= CompletedEventReceived;
            node.Checked -= CheckedEventReceived;
            //node.Reporting.ProgressChanged -= ChildProgressChanged;
        }

        private void CompletedEventReceived(object sender, NodeUpdatedEventArgs eventArgs)
        {
            var newValue = DataChilds.All(n => !n.IsChecked || n.IsCompleted);

       
            if (eventArgs.Node == this) return;
            //todo: propagate event to button communication window .
            OnCompleted(new NodeUpdatedEventArgs() { NewValue = IsCompleted, Node = eventArgs.Node });

            if (IsCompleted == newValue) return;
            IsCompleted = newValue;
        }

        private void CheckedEventReceived(object sender, NodeUpdatedEventArgs eventArgs)
        {
            var newValue = DataChilds.All(n => n.IsChecked);

            if (IsChecked == newValue) return;
            _isChecked = newValue;
            RaisePropertyChanged(() => IsChecked);
            if (eventArgs.Node == this) return;
            OnChecked(new NodeUpdatedEventArgs() { NewValue = IsCompleted, Node = eventArgs.Node });
        }

        protected virtual void OnCompleted(NodeUpdatedEventArgs e)
        {
            Completed?.Invoke(this, e);
        }

        protected virtual void OnChecked(NodeUpdatedEventArgs e)
        {
            Checked?.Invoke(this, e);
        }
    }
}