using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using Common.Model;
using EscInstaller.View;
using GalaSoft.MvvmLight;

namespace EscInstaller.ViewModel.OverView
{



    internal sealed class LspLeftHeader : ChkVm
    {
        private FlowModel _flow;

        public LspLeftHeader(int line, CardModel card, MainUnitModel main)
            : base(0, line, card, main)
        {
            _flow = card.Flows.Skip(line).FirstOrDefault();
        }

        public String HeaderValue
        {
            get
            {
                return (FlowId + 1).ToString("N0");
            }
        }

        public bool IsPresent
        {
            get
            {
                if(_flow.AttachedChannels == null || _flow == null ) return false;
                return _flow.AttachedChannels.Length > 3 && _flow.AttachedChannels.Any(t=> t);
            }
        }

        private BindablePoint _location;

        public override BindablePoint Location
        {
            get { return _location ?? (_location = new BindablePoint() { X = 0, Y = Line * BlSpMatrix.NodeSize }); }
        }
    }

    internal sealed class LspNode : ChkVm
    {
        private readonly int _loudspeaker;
        private readonly int _line;
        private readonly FlowModel _flow;

        public LspNode(int loudspeaker, int line, CardModel card, MainUnitModel main)
            : base(loudspeaker, line, card, main)
        {
            _loudspeaker = loudspeaker;
            _line = line;
            _flow = card.Flows.Skip(Loudspeaker).FirstOrDefault();
        }

        /// <summary>
        /// Determines a speaker is attached
        /// </summary>
        public bool IsPresent
        {
            get
            {
                if (_flow.AttachedChannels == null || _flow.AttachedChannels.Length < 4) return false;
                return _flow.AttachedChannels[_line];
            }
        }

        public string NodeValue
        {
            get { return (_line + _flow.Id +1).ToString("N0"); }
        }
    }

    public abstract class ChkVm : ViewModelBase
    {
        protected readonly int Loudspeaker;
        protected readonly int Line;
        protected readonly CardModel Card;
        protected readonly MainUnitModel Main;

        protected ChkVm(int loudspeaker, int line, CardModel card, MainUnitModel main)
        {
            Loudspeaker = loudspeaker;
            Line = line;
            Card = card;
            Main = main;
        }

        protected int FlowId
        {
            get { return Main.Id * 12 + Card.Id * 4 + Line; }
        }

        private BindablePoint _location;

        public virtual BindablePoint Location
        {
            get { return _location ?? (_location = new BindablePoint() { X = Line * BlSpMatrix.NodeSize + BlSpMatrix.NodeSize, Y = Loudspeaker * BlSpMatrix.NodeSize }); }
        }
    }
}