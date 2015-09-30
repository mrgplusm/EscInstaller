using System;
using System.Collections.Generic;


namespace EscInstaller.ViewModel.OverView
{
    public abstract class SnapDiagramData : DiagramData
    {
        public const int RowHeight = 35;
        public const int UnitHeight = 30;
        public const int SnapshotHeight = 17;
        public const int SnapshotWidth = 41;
        public const int Distance = 10;
        public const int InnerSpace = 20;

        public abstract string SettingName { get; }

        protected SnapDiagramData()
        {
            //Location.ValueChanged += RecalculateSnaps;
        }
        
        protected void RecalculateSnaps()
        {
            foreach (var snapshot in Snapshots)
            {
                snapshot.Calculate();
            }}

        public abstract void SetYLocation();

        public void RemoveChildren()
        {
            if (OnRemoveChildren != null)
                OnRemoveChildren();
        }

        /// <summary>
        /// Specifies the flow id for this channel
        /// </summary>
        public abstract int Id { get; }


        public Action OnRemoveChildren;


        private List<SnapShot> _snapShots;

        public virtual List<SnapShot> Snapshots
        {
            get
            {
                return _snapShots ?? (_snapShots = new List<SnapShot>
                {
                    new SnapShot(this) {Offset = {X = 0, Y = SnapshotHeight}, RowId = 0},
                    new SnapShot(this) {Offset = {X = Size.X, Y = SnapshotHeight}, RowId = 1},
                });
            }
        }
    }
}
