#region

using System;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public class SliderValue : ViewModelBase, IEquatable<SliderValue>
    {
        private readonly BlExtInput _extInput;
        private readonly FlowModel _flow;
        private bool _useVu;

        public SliderValue(FlowModel flow, BlExtInput extInput)
        {
            _flow = flow;
            _extInput = extInput;
        }

        public double Value
        {
            get { return _flow.InputSlider; }
            set
            {
                _flow.InputSlider = value;
                RaisePropertyChanged(() => Value);
                _extInput.OnValueUpdate();
                CommunicationViewModel.AddData(new SetGainSlider(_flow.Id, (int) value, SliderType.Input));
            }
        }

        public string Header
        {
            get { return BlExtInput.Names[(_flow.Id - GenericMethods.StartCountFrom)%5]; }
        }

        public bool UseVu
        {
            get { return _useVu; }
            set
            {
                _useVu = value;
                RaisePropertyChanged(() => UseVu);
                OnUseVuUpdated();
            }
        }

        public int Id
        {
            get { return _flow.Id; }
        }

        public bool Equals(SliderValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_flow.Id, other._flow.Id);
        }

        public void UpdateValue()
        {
            RaisePropertyChanged(() => Value);
        }

        public event EventHandler UseVuUpdated;

        protected virtual void OnUseVuUpdated()
        {
            var handler = UseVuUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void UpdateUseVu(bool value)
        {
            _useVu = value;
            RaisePropertyChanged(() => UseVu);
        }

        public override int GetHashCode()
        {
            return (_flow != null ? _flow.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SliderValue);
        }
    }
}