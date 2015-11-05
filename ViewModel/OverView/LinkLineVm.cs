#region

using System.Windows;
using System.Windows.Media;
using Common.Commodules;

#endregion

namespace EscInstaller.ViewModel.OverView
{
    public sealed class LinkLineVm : LineViewModel
    {
        private StreamGeometry _geometry1;
        private LinkTo _linkTo;

        public LinkLineVm(SnapDiagramData first, SnapDiagramData second, SnapDiagramData InitialLink, int flowId,
            int? rowFirst = null, int? rowSecond = null)
            : base(first, second, rowFirst, rowSecond)
        {
            Id = flowId;
            LineType = LineType.LinkLine;


            ChangePath(LinkTo.No, InitialLink);

            SetGeometry();
        }

        public StreamGeometry Geometry
        {
            get { return _geometry1; }
            set
            {
                _geometry1 = value;
                RaisePropertyChanged(() => Geometry);
            }
        }

        //need to define line flow path
        public LinkTo LinkTo
        {
            get { return _linkTo; }
            set
            {
                _linkTo = value;
                RaisePropertyChanged(() => LinkTo);
                SetGeometry();
            }
        }

        public void ChangePath(LinkTo Path, SnapDiagramData linkTo)
        {
            if (Id%12 == 0)
            {
                LinkTo = LinkTo.No;
                return;
            }

            First = linkTo;

            if (Path == LinkTo.No)
            {
                RowIdFirst = null;
            }

            if (Path == LinkTo.Previous)
            {
                RowIdFirst = null;
                if (Id%12 == 2)
                {
                    RowIdSecond = null;
                }
                else
                {
                    RowIdFirst = Id%12 - 1;
                }
            }
            if (Path == LinkTo.PreviousWithDelay)
            {
                RowIdFirst = (Id%12 == 2) ? 1 : (int?) null;
                RowIdSecond = 0;
            }

            LinkTo = Path;
        }

        private void SetGeometry()
        {
            var geomtery = new StreamGeometry();

            using (var ctx = geomtery.Open())
            {
                ctx.BeginFigure(Start.Value, false, false);

                switch (LinkTo)
                {
                    case LinkTo.No:

                        break;
                    case LinkTo.Previous:
                        switch (Id%12)
                        {
                            case 1:
                                ctx.LineTo(new Point(End.Value.X - 5, Start.Value.Y), true, false);
                                ctx.LineTo(new Point(End.Value.X - 5, End.Value.Y), true, false);
                                break;
                            case 2:
                            case 3:
                                ctx.LineTo(new Point(Start.Value.X - 5, Start.Value.Y), true, false);
                                ctx.LineTo(new Point(Start.Value.X - 5, End.Value.Y), true, false);
                                break;

                            default:
                                ctx.LineTo(new Point(Start.Value.X - 20, Start.Value.Y), true, false);
                                ctx.LineTo(new Point(End.Value.X - 20, End.Value.Y), true, false);
                                break;
                        }
                        break;
                    case LinkTo.PreviousWithDelay:
                        if (Id%12 == 2)
                        {
                            ctx.LineTo(new Point(Start.Value.X + 5, Start.Value.Y), true, false);
                            ctx.LineTo(new Point(Start.Value.X + 5, End.Value.Y - 20), true, false);
                            ctx.LineTo(new Point(End.Value.X - 5, End.Value.Y - 20), true, false);
                            ctx.LineTo(new Point(End.Value.X - 5, End.Value.Y), true, false);
                        }
                        else
                        {
                            ctx.LineTo(new Point(Start.Value.X + 5, Start.Value.Y), true, false);
                            ctx.LineTo(new Point(Start.Value.X + 5, End.Value.Y), true, false);
                        }
                        break;
                }
                ctx.LineTo(End.Value, true, false);
            }

            geomtery.Freeze();

            Geometry = geomtery;
        }
    }
}