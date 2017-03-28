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
        //protected readonly MainUnitViewModel Main;
        private bool _isChecked = true;
        private bool _escDownloadIsCompleted;

        protected DownloadNode()
        {
            //  Main = main;
            DataChilds = new ObservableCollection<IDownloadNode>();
            //_value = Main.DisplayValue;
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
            get { return _escDownloadIsCompleted; }
            set
            {
                _escDownloadIsCompleted = value;
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
                node.Cancellation.Cancel();
                ResetNodes(node.DataChilds);
            }
        }

        protected void StopDownload(IList<IDownloadNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (var node in nodes.Where(n => n.IsChecked).Cast<DownloadNode>())
            {
                node.Cancellation.Cancel();
                node.Cancellation.Dispose();
                node.Cancellation = new CancellationTokenSource();
                StopDownload(node.DataChilds);
            }
        }

        protected async void StartDownload(IList<IDownloadNode> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;
            foreach (var node in nodes.Where(n => n.IsChecked).Cast<DownloadNode>())
            {
                await Task.Run(() => node.Function, Cancellation.Token);
                StartDownload(node.DataChilds);
            }
            Report(new DownloadProgress() { Progress = 1, Total = 1 });
        }

        private double _progressBar;
        private string _value = string.Empty;

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
        public virtual Task Function { get { return Task.Run(() => { }); } }

        public CancellationTokenSource Cancellation = new CancellationTokenSource();

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

        protected void AttachHandlers(IDownloadNode esc)
        {
            esc.Completed += CompletedEventReceived;
            esc.Checked += CheckedEventReceived;
        }

        protected void RemoveHandlers(IDownloadNode esc)
        {
            esc.Completed -= CompletedEventReceived;
            esc.Checked -= CheckedEventReceived;
        }

        private void CompletedEventReceived(object sender, NodeUpdatedEventArgs eventArgs)
        {
            var newValue = DataChilds.All(n => n.IsCompleted);

            if (IsCompleted == newValue) return;
            IsCompleted = newValue;
            if (eventArgs.Node == this) return;
            OnCompleted(new NodeUpdatedEventArgs() { NewValue = IsCompleted, Node = eventArgs.Node });
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