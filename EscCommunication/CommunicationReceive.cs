#region

using System.Collections.ObjectModel;
using System.Linq;
using Common;

#endregion

namespace EscInstaller.ViewModel.EscCommunication
{
    public class CommunicationReceive : CommunicationBase
    {
        private ObservableCollection<Downloader> _escs;

        public CommunicationReceive(MainViewModel main)
            : base(main)
        {
        }

        public override ObservableCollection<Downloader> Escs
        {
            get
            {
                if (_escs != null) return _escs;
                _escs = new ObservableCollection<Downloader>();
                foreach (var q in Main.TabCollection.OfType<MainUnitViewModel>()
                    .Where(d => d.ConnectType != ConnectType.None).Select(esc => new ReceiveData(esc)))
                {
                    AttachHandlers(q);
                    _escs.Add(q);
                }

                return _escs;
            }
        }
    }
}