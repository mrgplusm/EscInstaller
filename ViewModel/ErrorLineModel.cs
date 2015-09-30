using System;
using System.Runtime.Serialization;

namespace Futurama.ViewModel
{
    [Serializable]
    [DataContract]
    internal class ErrorLineModel : IEquatable<ErrorLineModel>
    {
        [DataMember]
        internal int EscUnit { get; set; }

        [DataMember]
        internal byte[] EscData { get; set; }

        [DataMember]
        internal int Id { get; set; }

        [DataMember]
        internal DateTime Date { get; set; }

        [DataMember]
        internal DeviceError Device { get; set; }

        [DataMember]
        internal string Detail { get; set; }

        [DataMember]
        internal ErrorStatuses Status { get; set; }

        [DataMember]
        internal bool EmailSend { get; set; }

        public bool Equals(ErrorLineModel other)
        {
            return Device.Equals(other.Device) && Detail.Equals(other.Detail);
        }
    }
}