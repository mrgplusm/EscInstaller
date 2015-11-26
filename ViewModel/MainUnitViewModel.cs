#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Common;
using Common.Commodules;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.EscCommunication.Logic;
using EscInstaller.ViewModel.Matrix;
using EscInstaller.ViewModel.OverView;
using EscInstaller.ViewModel.Settings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

#endregion

namespace EscInstaller.ViewModel
{
    public sealed class MainUnitViewModel : ViewModelBase, ITabControl, IEquatable<MainUnitViewModel>
    {
        private readonly MainViewModel _main;
        private BusyDownloading _busyDownloading;
        private ConnectType _connectType;
        private ObservableCollection<DiagramData> _diagramObjects;
        private BlLink _link;
        private double _monitorSlider;
        private double _progress;
        private DiagramData _selectedObject;
        public Action DelayChanged;
        public Action ExtinputUpdate;
        private IntervalSettingsViewModel v;
        private double _canvasSize;

        public MainUnitViewModel(MainUnitModel mainUnit, MainViewModel main)
        {
            main.Communication.ConnectionModeChanged += Communication_ConnectionModeChanged;
            DataModel = mainUnit;
            _main = main;
            main.SystemChanged += UpdateName;

            VuMeter = new VuMeter(this);
            AlarmMessages = new AlarmMessagesViewModel(this);            

            UpdateConnectionMode();
            CardsUpdated += (sender, args) => RaisePropertyChanged(() => CanvasSize);
        }

#if DEBUG
        /// <summary>
        ///     Design instance
        /// </summary>
        public MainUnitViewModel()
        {
            LibraryData.CreateEmptySystem();
            DataModel = LibraryData.GetMainUnit(0);
            _main = new MainViewModel();
        }
#endif
        internal EepromDataHandler EepromHandler { get; private set; }
        internal VuMeter VuMeter { get; private set; }
        public AlarmMessagesViewModel AlarmMessages { get; private set; }


        public double CanvasSize => 400d + DataModel.ExpansionCards * 200d;

        public ConnectionViewModel Connection
        {
            get { return _main.Communication.Connections.FirstOrDefault(d => d.UnitId == Id); }
        }

        /// <summary>
        ///     used to temporary store timestamp when verifing
        /// </summary>
        public long Timestamp { get; set; }

        public string Name
        {
            get { return DataModel.Name; }
            set
            {
                DataModel.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public MainUnitModel DataModel { get; }

        public double MonitorSlider
        {
            get { return _monitorSlider; }
            set
            {
                _monitorSlider = value;
                OnMonitorSliderUpdated();
            }
        }

        public DiagramData SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                RaisePropertyChanged(() => SelectedObject);
            }
        }

        public List<FlowModel> Flows
        {
            get
            {
                if (DataModel.Cards == null || DataModel.Cards.Count < 1) return new List<FlowModel>();
                return DataModel.Cards.SelectMany(f => f.Flows).OrderBy(f => f.Id).ToList();
            }
        }

        public ConnectType ConnectType
        {
            get { return _connectType; }
            private set
            {
                _connectType = value;
                RaisePropertyChanged(() => ConnectType);
            }
        }

        public List<SpeakerDataModel> SpeakerDataModels
        {
            get { return DataModel.SpeakerDataModels; }
        }

        public List<byte> DspCopy
        {
            get { return DataModel.DspCopy; }
        }

        public bool E2PromUpdateVisible
        {
            get
            {
                bool b;
                return bool.TryParse(LibraryData.Settings["EepromUpdateEnabled"], out b) && b;
            }
        }

        public ICommand AddNewCard
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        if (DataModel.ExpansionCards > 1)
                            return;
                        DataModel.ExpansionCards++;

                        DiagramObjects.OfType<BlLink>().First().Cards(DataModel.ExpansionCards);

                        foreach (var n in DiagramObjects.OfType<BlExtInput>())
                        {
                            n.SetYLocation();
                        }
                        foreach (var n in DiagramObjects.OfType<BlInputPeq>())
                        {
                            n.SetYLocation();
                        }


                        DiagramObjects.OfType<BlEmergency>().First().SetYLocation();

                        var lst =
                            GenDiagramObjects(DataModel.Cards.First(i => i.Id == DataModel.ExpansionCards),
                                DiagramObjects).ToArray();
                        foreach (var n in lst)
                        {
                            DiagramObjects.Add(n);
                        }
                        OnCardsUpdated(new MainUnitUpdatedEventArgs() {MainUnit = DataModel});
                    },
                    () => DataModel.ExpansionCards < 2);
            }
        } 

        

        public ICommand RemoveLastCard
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DataModel.ExpansionCards--;
                    DiagramObjects.OfType<BlLink>().First().Cards(DataModel.ExpansionCards);

                    foreach (var n in DiagramObjects.OfType<BlExtInput>())
                    {
                        n.SetYLocation();
                    }
                    foreach (var n in DiagramObjects.OfType<BlInputPeq>())
                    {
                        n.SetYLocation();
                    }

                    DiagramObjects.OfType<BlEmergency>().First().SetYLocation();

                    var removelist = new List<DiagramData>();
                    removelist.AddRange(
                        DiagramObjects.OfType<BlInputName>().Where(s => s.Id%12/4 > DataModel.ExpansionCards));
                    removelist.AddRange(
                        DiagramObjects.OfType<BlSpeakerPeq>().Where(s => s.Id%12/4 > DataModel.ExpansionCards));
                    removelist.AddRange(
                        DiagramObjects.OfType<BlMonitor>().Where(s => s.Id%12/4 > DataModel.ExpansionCards));
                    removelist.AddRange(
                        DiagramObjects.OfType<BlOutput>().Where(s => s.Id%12/4 > DataModel.ExpansionCards));
                    removelist.AddRange(
                        DiagramObjects.OfType<BlSpeaker>().Where(s => s.Id%12/4 > DataModel.ExpansionCards));
                    removelist.AddRange(
                        DiagramObjects.OfType<BlAmplifier>().Where(s => s.Id%12/4 > DataModel.ExpansionCards));

                    removelist.AddRange(
                        DiagramObjects.OfType<BlAuxSpeakerPeq>().Where(s => s.Id > DataModel.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlAuxiliary>().Where(s => s.Id > DataModel.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlBackupAmp>().Where(s => s.Id > DataModel.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlSpMatrix>().Where(s => s.Id > DataModel.ExpansionCards));


                    foreach (var d in removelist.OfType<SnapDiagramData>())
                    {
                        d.RemoveChildren();
                    }

                    foreach (var diagramData in removelist)
                    {
                        DiagramObjects.Remove(diagramData);
                    }
                    OnCardsUpdated(new MainUnitUpdatedEventArgs() {MainUnit = DataModel});
                }, (() => DataModel.ExpansionCards > 0));
            }
        }

        public string DisplayValue
        {
            get { return (Id < 1) ? Main._mainMaster : string.Format(Main._mainSlave, Id); }
        }

        public ObservableCollection<DiagramData> DiagramObjects
        {
            get
            {
                return _diagramObjects ?? (_diagramObjects = new ObservableCollection<DiagramData>(GenDiagramObjects()));
            }
        }

        public ICommand ButtonFinished
        {
            get { return new RelayCommand(() => { BusyDownloading = BusyDownloading.No; }); }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }

        public BusyDownloading BusyDownloading
        {
            get
            {
                if (IsInDesignMode)
                    return BusyDownloading.No;
                return _busyDownloading;
            }
            set
            {
                _busyDownloading = value;
                RaisePropertyChanged(() => BusyDownloading);
            }
        }

        public bool Equals(MainUnitViewModel other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public int Id
        {
            get { return DataModel.Id; }
            set
            {
                DataModel.Id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        /// <summary>
        ///     Connection might allready be initialized
        /// </summary>
        private void UpdateConnectionMode()
        {
            ConnectType = (Connection != null && Connection.UnitId == Id) ? Connection.ConnectType : ConnectType.None;
            _main.Communication.SetTriggers();
        }

        private void Communication_ConnectionModeChanged(object sender, ConnectionModeChangedEventArgs e)
        {
            if (e.UnitId == Id)
            {
                ConnectType = e.Mode == ConnectMode.None ? ConnectType.None : e.Type;
            }
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public void UpdateHardware()
        {
            UpdateGraphics();
            UpdateLineLinks();
            OnCardsUpdated(new MainUnitUpdatedEventArgs() {MainUnit = DataModel});
        }

        private void UpdateName(object sender, SystemChangedEventArgs e)
        {
            RaisePropertyChanged(() => Name);
        }

        

        

        public event EventHandler MonitorSliderUpdated;

        private void OnMonitorSliderUpdated()
        {
            var handler = MonitorSliderUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void OpenMeasurementSettings()
        {
            SelectedObject = (v ?? (v = new IntervalSettingsViewModel(this)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as MainUnitViewModel);
        }

        public async Task<bool> TimestampIsEqual()
        {
            //no timestamp has ever been written, so we dont know
            if (DataModel.TimeStampWrittenToEsc == 0) return false;

            var z = new GetE2PromExt(Id, 8, McuDat.DesignTimestamp);
            var sem = new AutoResetEvent(false);

            CommunicationViewModel.AddData(z);
            await z.WaitAsync();

            Timestamp = BitConverter.ToInt64(z.ReceivedEpromValues.ToArray(), 0);
            var ret = Timestamp == DataModel.TimeStampWrittenToEsc;
            sem.Set();


            z.Error += (p, q) => sem.Set();

            CommunicationViewModel.AddData(z);

            sem.WaitOne(2000);

            return ret;
        }

        public void ItemsControlClick()
        {
            SelectedObject = null;
        }

        public void UpdateGraphics()
        {
            _diagramObjects = null;
            RaisePropertyChanged(() => DiagramObjects);
        }

        private IEnumerable<DiagramData> GenDiagramObjects()
        {
            var link = new BlLink(this);
            var ret = new List<DiagramData> {link};
            _link = link;

            var cards =
                DataModel.Cards.Take(DataModel.ExpansionCards + 1)
                    .Concat(DataModel.Cards.OfType<ExtensionCardModel>());

            ret.AddRange(cards.SelectMany(model => GenDiagramObjects(model, ret.ToList())));

            return ret;
        }

        private IEnumerable<DiagramData> GenDiagramObjects(CardBaseModel card, IEnumerable<DiagramData> currentObjects)
        {
            var lst = new List<DiagramData>();

            foreach (var flow in card.Flows)
            {
                lst.AddRange(GenFlow(flow, card as CardModel));
            }

            if (card.GetType() == typeof (CardModel))
            {
                var l = new List<SnapDiagramData>
                {
                    new BlBackupAmp(this, (CardModel) card),
                    new BlAuxiliary(this, (CardModel) card),
                    new BlSpMatrix(this, (CardModel) card),
                    new BlAuxSpeakerPeq(this, (CardModel) card)
                };

                foreach (var snapDiagramData in l)
                {
                    snapDiagramData.SetYLocation();
                }

                lst.AddRange(l);
                LinesCardModel(lst, _link);
            }
            else
            {
                var emergency = new BlEmergency(this);
                emergency.SetYLocation();
                lst.Add(emergency);
                LinesExtCardModel(lst, _link, emergency);
            }
            foreach (var n in lst.OfType<LineViewModel>())
            {
                var n1 = n;
                n.OnEndPointRemoved += () => DiagramObjects.Remove(n1);
            }

            return lst;
        }

        /// <summary>
        ///     Put lines of the emergency panels on screen
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="link"></param>
        /// <param name="emergency"></param>
        private static void LinesExtCardModel(List<DiagramData> objects, BlLink link, BlEmergency emergency)
        {
            if (emergency != null && link != null)
                objects.Add(new LineViewModel(link, emergency, 30) {LineType = LineType.Special});

            var extinp = objects.OfType<BlExtInput>().ToArray();
            if (extinp.Length > 2)
                objects.AddRange(new[] {0, 1, 4}.Select(n => new LineViewModel(extinp[n], emergency)));

            var inpPeq =
                objects.OfType<BlInputPeq>()
                    .Join(extinp, q => q.Id, j => j.Id, (peq, extInput) => new {peq, extInput})
                    .ToArray();
            objects.AddRange(inpPeq.Select(n => new LineViewModel(n.peq, n.extInput)));

            objects.AddRange(objects.OfType<BlInputPeq>().Select(n => new LineViewModel(n, emergency)).ToArray());
        }

        /// <summary>
        ///     add lines to diagram
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="link"></param>
        private static void LinesCardModel(List<DiagramData> objects, BlLink link)
        {
            var t = objects.OfType<BlInputName>().Join(objects.OfType<BlToneControl>(), s => s.Id, q => q.Id, (b, c) =>
                new LineViewModel(b, c) {LineType = LineType.PublicAddress}).ToArray();

            objects.AddRange(t);

            var tone = objects.OfType<BlToneControl>().OrderBy(n => n.Id).ToArray();
            var delay = objects.OfType<BlDelay>().OrderBy(i => i.Id).ToArray();

            if (link != null)
            {
                if (tone.Length > 0)
                    objects.AddRange(
                        tone.Select(q => new LineViewModel(q, link) {Id = q.Id}).OrderBy(n => n.Id).ToArray());
                objects.AddRange(
                    delay
                        .Select(q => new LinkLineVm(link, q, link, q.Id))
                        .ToArray());
                objects.AddRange(objects.OfType<BlSpeakerPeq>()
                    .Where(i => i.Id%12 == 1 || i.Id%12 == 2)
                    .OrderBy(i => i.Id)
                    .ToArray()
                    .Join(delay, peq => peq.Id, blDelay => blDelay.Id, (peq, blDelay) =>
                        new LineViewModel(peq, blDelay) {LineType = LineType.Emergency, Id = blDelay.Id}));
            }

            var input = objects.OfType<BlInputName>().OrderBy(n => n.Id).Where(i => i.Id%12 > 3).ToArray();
            if (input.Length > 0 && link != null)
                objects.AddRange(input.Select(q => new LineViewModel(q, link) {LineType = LineType.PublicAddress}));

            var outputs = objects.OfType<BlAmplifier>()
                .Join(objects.OfType<BlOutput>(), s => s.Id, q => q.Id,
                    (amplifier, blOutput) => new LineViewModel(amplifier, blOutput)).ToArray();

            objects.AddRange(outputs);

            objects.AddRange(
                objects.OfType<BlMonitor>()
                    .Join(objects.OfType<BlSpeakerPeq>(), s => s.Id, q => q.Id, (b, c) => new LineViewModel(b, c))
                    .ToArray());

            var speakerMatix = objects.OfType<BlAmplifier>()
                .Join(objects.OfType<BlSpMatrix>(), s => s.Id%12/4, q => q.Id, (b, c) => new LineViewModel(b, c))
                .ToArray();
            objects.AddRange(speakerMatix);

            if (link != null)
                objects.AddRange(
                    objects.OfType<BlAuxSpeakerPeq>()
                        .OrderBy(q => q.Id)
                        .Select(n => new LineViewModel(n, link) {Id = n.Id})
                        .ToArray());

            objects.AddRange(
                objects.OfType<BlAuxSpeakerPeq>().OrderBy(n => n.Id)
                    .Join(objects.OfType<BlAuxiliary>(), peq => peq.Id, auxiliary => auxiliary.Id,
                        (peq, auxiliary) => new LineViewModel(peq, auxiliary))
                    .ToArray());

            objects.AddRange(
                objects.OfType<BlMonitor>()
                    .Join(objects.OfType<BlOutput>(), s => s.Id, q => q.Id, (b, c) => new LineViewModel(b, c))
                    .ToArray());

            var spMatrix = objects.OfType<BlSpMatrix>().OrderBy(q => q.Id).ToArray();

            var speakers = new List<BlSpeaker>[3];

            speakers[0] = objects.OfType<BlSpeaker>().Where(i => i.Id%12 < 4).ToList();
            speakers[1] = objects.OfType<BlSpeaker>().Where(i => i.Id%12 > 3 && i.Id%12 < 8).ToList();
            speakers[2] = objects.OfType<BlSpeaker>().Where(i => i.Id%12 > 7).ToList();

            var backup = objects.OfType<BlBackupAmp>().FirstOrDefault();

            if (backup != null)
            {
                BlAmplifier prevamp = null;
                foreach (var amp in objects.OfType<BlAmplifier>().ToArray())
                {
                    if (prevamp != null)
                        objects.Add(new LineViewModel(prevamp, amp));
                    prevamp = amp;
                }
                objects.Add(new LineViewModel(prevamp, backup));
            }

            if (spMatrix.Length > 0)
                for (var i = 0; i < 3; i++)
                {
                    if (speakers[i].Count > 0)
                        objects.AddRange(speakers[i].Select(n => new LineViewModel(n, spMatrix[0])));
                }

            objects.AddRange(objects.OfType<BlSpeakerPeq>().Where(q => q.Id%12 != 1 && q.Id%12 != 2)
                .Select(sp => new LinkLineVm(link, sp, link, sp.Id)).ToArray());
        }

        public void UpdateLineLinks()
        {
            foreach (var result in DataModel.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
            {
                UpdateLineLink(result);
            }
        }

        public void UpdateLineLink(FlowModel flow)
        {
            var q = DiagramObjects.OfType<LinkLineVm>().FirstOrDefault(i => i.Id == flow.Id);
            if (q == null) return;

            SnapDiagramData z = null;

            if (flow.Path == LinkTo.Previous && (flow.Id == 2 || flow.Id == 3) || flow.Path == LinkTo.PreviousWithDelay)
            {
                z = DiagramObjects.OfType<BlDelay>().FirstOrDefault(i => i.Id == flow.Id - 1);
            }
            else if (flow.Path == LinkTo.No || flow.Id%12 < 4)
            {
                z = DiagramObjects.OfType<BlLink>().FirstOrDefault();
            }
            else if (flow.Path == LinkTo.Previous)
            {
                z = DiagramObjects.OfType<BlSpeakerPeq>().FirstOrDefault(i => i.Id == flow.Id - 1);
            }

            if (z != null)
                q.ChangePath(flow.Path, z);
        }

        private IEnumerable<DiagramData> GenFlow(FlowModel flow, CardModel card)
        {
            var lst = new List<SnapDiagramData>();

            if (flow.Id < GenericMethods.StartCountFrom)
            {
                lst.Add(new BlInputName(flow, this));
                lst.Add(new BlSpeakerPeq(flow, this));
                lst.Add(new BlMonitor(flow, this, card));
                lst.Add(new BlOutput(flow, this));

                lst.Add(new BlSpeaker(flow, this, 0));
                lst.Add(new BlSpeaker(flow, this, 1));
                lst.Add(new BlAmplifier(flow));
            }
            if (flow.Id >= GenericMethods.StartCountFrom)
            {
                lst.Add(new BlExtInput(flow, this, _main));
            }

            if (flow.Id%12 < 4 && flow.Id < GenericMethods.StartCountFrom)
            {
                lst.Add(new BlToneControl(flow));
                if (flow.Id%12 < 3 && flow.Id%12 > 0)
                    lst.Add(new BlDelay(this, flow));
            }

            if ((flow.Id%5 == 2 || flow.Id%5 == 3) &&
                flow.Id >= GenericMethods.StartCountFrom)
            {
                lst.Add(new BlInputPeq(flow, this));
            }

            if (flow.Id < GenericMethods.StartCountFrom && flow.Id%12%4 == 0)
            {
            }
            foreach (var snapDiagramData in lst)
            {
                snapDiagramData.SetYLocation();
            }

            return lst;
        }

        public event EventHandler<MainUnitUpdatedEventArgs> CardsUpdated;

        private void OnCardsUpdated(MainUnitUpdatedEventArgs e)
        {
            var handler = CardsUpdated;
            if (handler != null) handler(this, e);
        }

        public event EventHandler SdCardMessagesReceived;

        public void OnSdCardMessagesReceived()
        {
            var handler = SdCardMessagesReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler SdCardPositionsReceived;

        public void OnSdCardPositionsReceived()
        {
            var handler = SdCardPositionsReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler KreisUpdated;

        public void OnKreisUpdated()
        {
            KreisUpdated?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RedundancyUpdated;

        public void OnRedundancyUpdated()
        {
            var handler = RedundancyUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler RoutingTableUpdated;

        public void OnRoutingTableUpdated()
        {
            var handler = RoutingTableUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler PresetNamesUpdated;

        public void OnPresetNamesUpdated()
        {
            var handler = PresetNamesUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler DspMirrorUpdated;

        public void OnDspMirrorUpdated()
        {
            var handler = DspMirrorUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }        
    }

    public class MainUnitUpdatedEventArgs : EventArgs
    {
        public MainUnitModel MainUnit { get; set; }
    }
}