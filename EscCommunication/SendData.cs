#region

using System.Threading.Tasks;
using EscInstaller.EscCommunication.downloadItems;
using EscInstaller.EscCommunication.UploadItem;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication
{
    public class Upload : DownloadNode
    {
        private readonly MainUnitViewModel _main;
        private string _value1;

        public Upload(MainUnitViewModel main)
        {
            _main = main;
            
            _value1 = main.DisplayValue;
        }

        public override string Value
        {
            get { return _value1; }
            set { _value1 = value; }
        }             
    }
}