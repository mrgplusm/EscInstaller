#region

using System;
using System.IO;
using System.Linq;
using Common;
using Common.Model;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.SDCard
{
    /// <summary>
    ///     Used to display messages in matrix
    /// </summary>
    public class SdFileVM : ViewModelBase, IEquatable<SdFileVM>
    {
        private readonly int _card;
        private readonly bool _isForEmergency;
        private readonly SdFileModel _model;

        /// <summary>
        /// </summary>
        /// <param name="model">datamodel</param>
        /// <param name="card">0=cardA 1=CardB. Used to fetch mp3 file names if any</param>
        /// <param name="isForEmergency">used to indicate first message as no message, matrix otherwise</param>
        public SdFileVM(SdFileModel model, int card, bool isForEmergency = false)
        {
            _isForEmergency = isForEmergency;
            _model = model;
            _card = card;
        }

        public string Name
        {
            get
            {
                if (Position == 0xff)
                    return _isForEmergency ? "No message" : "Matrix";
                if (LibraryData.SystemIsOpen)
                {
                    var list = (_card == 0)
                        ? LibraryData.FuturamaSys.MessagesCardA
                        : LibraryData.FuturamaSys.MessagesCardB;
                    if (_model.Position < list.Count)
                    {
                        var model = list[_model.Position];
                        foreach (
                            var s in
                                new[] {model.LongFileName, Path.GetFileNameWithoutExtension(model.Location)}.SkipWhile(
                                    string.IsNullOrWhiteSpace)) return s;
                    }
                }
                return !string.IsNullOrWhiteSpace(_model.Name) ? _model.Name : (_model.Position + 1).ToString("N0");
            }
        }

        public int Position
        {
            get { return _model.Position; }
        }

        public bool IsOnCard
        {
            get { return !string.IsNullOrWhiteSpace(_model.Name); }
        }

        public bool Equals(SdFileVM other)
        {
            if (ReferenceEquals(this, other)) return true;
            return !ReferenceEquals(other, null) && _model.Equals(other._model);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_model != null ? _model.GetHashCode() : 0)*397) ^ _card;
            }
        }

        public void UpdateName()
        {
            RaisePropertyChanged(() => Name);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            return !ReferenceEquals(other, null) && Equals(other as SdFileVM);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}