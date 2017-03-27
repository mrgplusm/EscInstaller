#region

using System.Collections.ObjectModel;
using System.Linq;
using Common;
using EscInstaller.ViewModel;
using EscInstaller.ViewModel.EscCommunication;

#endregion

namespace EscInstaller.EscCommunication
{
    public class CommunicationSend : CommunicationBase
    {
        private ObservableCollection<Downloader> _escs;

        public CommunicationSend(MainViewModel main)
            : base(main)
        {
        }

        public override ObservableCollection<Downloader> Escs
        {
            get
            {
                if (_escs != null) return _escs;
                _escs = new ObservableCollection<Downloader>();
                foreach (var q in Main.TabCollection.OfType<MainUnitViewModel>().ToList()
                    .Where(d => d.ConnectType != ConnectType.None).Select(esc => new SendData(esc)))
                {
                    AttachHandlers(q);
                    _escs.Add(q);
                }

                return _escs;
            }
        }
    }
}