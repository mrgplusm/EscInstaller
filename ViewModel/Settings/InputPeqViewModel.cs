using EscFileHandler.Model;
using McuCommunication;
using McuCommunication.Commodules;

namespace EscInstaller.ViewModel.Settings
{
    public sealed class InputPeqViewModel : PeqBaseViewModel
    {
        private SpeakerDataViewModel _speaker;

        public override SpeakerDataViewModel CurrentSpeaker
        {
            get
            {
                return _speaker ??
                       (_speaker = new SpeakerDataViewModel(Speaker));
            }
        }

        private SpeakerDataModel Speaker
        {
            get
            {
                if (InputPeqId == 2)
                    return
                        CurrenttMainUnit.InputPeq1 ?? (CurrenttMainUnit.InputPeq1 =
                            new SpeakerDataModel() { SpeakerPeqType = SpeakerPeqType.BiquadsMic });

                return CurrenttMainUnit.InputPeq2 ?? (CurrenttMainUnit.InputPeq2 =
                            new SpeakerDataModel() { SpeakerPeqType = SpeakerPeqType.BiquadsMic });
            }
        }

        public int InputPeqId
        {
            get { return (Id - ConnStatMethods.StartCountFrom) % 5; }
        }

        public override string DisplayId
        {
            get { return ((Id - ConnStatMethods.StartCountFrom) % 5 -1).ToString("N0"); }
        }
    }
}