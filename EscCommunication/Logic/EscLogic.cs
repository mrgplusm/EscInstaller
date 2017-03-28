#region

using System;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;

#endregion

namespace EscInstaller.EscCommunication.Logic
{
    public class EscLogic
    {
        protected readonly MainUnitModel Main;

        protected internal EscLogic(MainUnitModel main)
        {
            Main = main;
        }

        public async void GetBoseVersion()
        {
            var q = new GetBoseVersion(Main.Id);
            CommunicationViewModel.AddData(q);
            await q.WaitAsync();

            Main.BoseVersion = q.BoseVersion;
            OnBoseVersionReceived();
        }

        public event EventHandler BoseVersionReceived;

        protected virtual void OnBoseVersionReceived()
        {            
            BoseVersionReceived?.Invoke(this, EventArgs.Empty);
        }
    }
}