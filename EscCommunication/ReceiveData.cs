#region

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication
{
    public class DownloadData : DownloadNode
    {
        private string _value1;
        protected MainUnitViewModel Main { get;  }
#if DEBUG
        public DownloadData()
            : base()
        {
            DataChilds = new ObservableCollection<IDownloadNode>();
            for (var i = 0; i < 2; i++)
            {
                DataChilds.Add(new Dsp(new MainUnitViewModel())); 
            }
        }
#endif

        public DownloadData(MainUnitViewModel main)
        {
            Main = main;         
            _value1 = main.DisplayValue;
        }

        public override string Value
        {
            get { return _value1; }
            set { _value1 = value; }
        }        

    }
}