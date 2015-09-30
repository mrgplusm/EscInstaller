using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using McuCommunication.Commodules;
using Microsoft.Practices.Unity;

namespace EscInstaller.ViewModel.Settings
{
    public class
        LinkViewModel : SettingsBaseViewModel
    {
        [InjectionConstructor]
        public LinkViewModel()
        {
            Messenger.Default.Register<int>(this, "CardLines", s =>
                {
                    if (s == Id || s == Id - 1 || s == Id + 1)
                    {
                        RaisePropertyChanged(() => Path);
                        RaisePropertyChanged(() => AfterDelayVisible);
                        RaisePropertyChanged(() => ToPreviousVisible);
                    }
                });
        }


        //public override string ViewTitle
        //{
        //    get { return Language._linkBlockTitle + " " + (Id + 1) + " " + CurrentFlow.NameOfOutput; }
        //}

        public LinkTo Path
        {
            get { return CurrentFlow.Path; }
        }


        public bool Straight
        {
            get { return (Path == LinkTo.No); }
            set
            {
                if (!value) return;
                CurrentFlow.Path = LinkTo.No;
                AddData(new SetLinkDemux(Id, LinkTo.No));

                Messenger.Default.Send(Id, "CardLines");
                
                
            }
        }

        public bool BeforeLink
        {
            get { return CurrentFlow.Path == LinkTo.Previous; }
            set
            {
                if (!value) return;
                CurrentFlow.Path = LinkTo.Previous;
                AddData(new SetLinkDemux(Id, LinkTo.Previous));

                Messenger.Default.Send(Id, "CardLines");
            }
        }

        public bool AfterDelay
        {
            get { return CurrentFlow.Path == LinkTo.PreviousWithDelay; }
            set
            {
                if (!value) return;
                CurrentFlow.Path = LinkTo.PreviousWithDelay;
                AddData(new SetLinkDemux(Id, LinkTo.PreviousWithDelay));

                Messenger.Default.Send(Id, "CardLines");
            }
        }

        public Visibility ToPreviousVisible
        {
            get { return (CurrentFlow.Id%12) > 0 ? Visibility.Visible : Visibility.Hidden; }
        }

        public Visibility AfterDelayVisible
        {
            get
            {
                return (((CurrentFlow.Id%12) > 1 && (CurrentFlow.Id%12) < 4) && !FirstDelayUseAll())
                           ? Visibility.Visible
                           : Visibility.Hidden;
            }
        }

        /// <summary>
        ///     check if flow 3 and flow 2 has used all the delay
        /// </summary>
        /// <returns> true is first delay use all memory </returns>
        private bool FirstDelayUseAll()
        {
            if ((CurrentFlow.Id%12) != 3) return false;

            return CurrentCard.Flows.First(i => i.Id%12 == 1).DelayMilliseconds > SetDelay.MaxSingleDelay;
        }
    }
}