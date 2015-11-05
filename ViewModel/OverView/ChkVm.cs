#region

using System.Linq;
using Common.Model;
using GalaSoft.MvvmLight;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    internal sealed class LspLeftHeader : ChkVm
    {
        private readonly FlowModel _flow;
        private BindablePoint _location;

        public LspLeftHeader(int line, CardModel card, MainUnitModel main)
            : base(0, line, card, main)
        {
            _flow = card.Flows.Skip(line).FirstOrDefault();
        }

        public string HeaderValue
        {
            get { return (FlowId + 1).ToString("N0"); }
        }

        public bool IsPresent
        {
            get
            {
                if (_flow.AttachedChannels == null || _flow == null) return false;
                return _flow.AttachedChannels.Length > 3 && _flow.AttachedChannels.Any(t => t);
            }
        }

        public override BindablePoint Location
        {
            get { return _location ?? (_location = new BindablePoint() {X = 0, Y = Line*BlSpMatrix.NodeSize}); }
        }
    }

    internal sealed class LspNode : ChkVm
    {
        private readonly FlowModel _flow;
        private readonly int _line;
        private readonly int _loudspeaker;

        public LspNode(int loudspeaker, int line, CardModel card, MainUnitModel main)
            : base(loudspeaker, line, card, main)
        {
            _loudspeaker = loudspeaker;
            _line = line;
            _flow = card.Flows.Skip(Loudspeaker).FirstOrDefault();
        }

        /// <summary>
        ///     Determines a speaker is attached
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
            get { return (_line + _flow.Id + 1).ToString("N0"); }
        }
    }

    public abstract class ChkVm : ViewModelBase
    {
        protected readonly CardModel Card;
        protected readonly int Line;
        protected readonly int Loudspeaker;
        protected readonly MainUnitModel Main;
        private BindablePoint _location;

        protected ChkVm(int loudspeaker, int line, CardModel card, MainUnitModel main)
        {
            Loudspeaker = loudspeaker;
            Line = line;
            Card = card;
            Main = main;
        }

        protected int FlowId
        {
            get { return Main.Id*12 + Card.Id*4 + Line; }
        }

        public virtual BindablePoint Location
        {
            get
            {
                return _location ??
                       (_location =
                           new BindablePoint()
                           {
                               X = Line*BlSpMatrix.NodeSize + BlSpMatrix.NodeSize,
                               Y = Loudspeaker*BlSpMatrix.NodeSize
                           });
            }
        }
    }
}