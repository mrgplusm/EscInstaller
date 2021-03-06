﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.Model;
using EscInstaller.ViewModel.Connection;
using EscInstaller.ViewModel.EscCommunication;
using EscInstaller.ViewModel.EscCommunication.Logic;
using EscInstaller.ViewModel.Settings;

using EscInstaller.ViewModel.OverView;
using Common;
using Common.Commodules;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using CommunicationViewModel = EscInstaller.ViewModel.Connection.CommunicationViewModel;
using ITabControl = EscInstaller.ViewModel.Connection.ITabControl;


namespace EscInstaller.ViewModel
{
    public sealed class MainUnitViewModel : ViewModelBase, ITabControl, IEquatable<MainUnitViewModel>
    {
        internal EepromDataHandler EepromHandler { get; private set; }
        internal VuMeter VuMeter { get; private set; }
        public AlarmMessagesViewModel AlarmMessages { get; private set; }

        public MainUnitViewModel(MainUnitModel mainUnit, MainViewModel main)
        {
            main.Communication.ConnectionModeChanged += Communication_ConnectionModeChanged;
            _mainUnit = mainUnit;
            _main = main;
            main.SystemChanged += UpdateName;                       
            
            VuMeter = new VuMeter(this);
            AlarmMessages = new AlarmMessagesViewModel(this);                       

            UpdateConnectionMode();
        }

        /// <summary>
        /// Connection might allready be initialized
        /// </summary>
        private void UpdateConnectionMode()
        {
            ConnectType = (Connection != null && Connection.UnitId == Id) ? Connection.ConnectType : ConnectType.None;
            _main.Communication.SetTriggers();
        }

        public ConnectionViewModel Connection
        {
            get { return _main.Communication.Connections.FirstOrDefault(d => d.UnitId == Id); }
        }

        void Communication_ConnectionModeChanged(object sender, ConnectionModeChangedEventArgs e)
        {
            if (e.UnitId == Id)
            {
                ConnectType = e.Mode == ConnectMode.None ? ConnectType.None : e.Type;
            }
        }

        public bool Equals(MainUnitViewModel other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        private readonly MainUnitModel _mainUnit;


#if DEBUG
        /// <summary>
        /// Design instance
        /// </summary>
        public MainUnitViewModel()
        {
            LibraryData.CreateEmptySystem();
            _mainUnit = LibraryData.GetMainUnit(0);
            _main = new MainViewModel();


        }
#endif



        public void UpdateHardware()
        {
            UpdateGraphics();
            UpdateLineLinks();
            OnCardsUpdated(new MainUnitUpdatedEventArgs() { MainUnit = DataModel });
        }

        /// <summary>
        /// used to temporary store timestamp when verifing
        /// </summary>
        public long Timestamp { get; set; }

        private void UpdateName(object sender, SystemChangedEventArgs e)
        {
            RaisePropertyChanged(() => Name);
        }

        public string Name
        {
            get { return _mainUnit.Name; }
            set
            {
                _mainUnit.Name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public event EventHandler PanelSelectionChanged;

        internal void OnPanelSelectionChanged()
        {
            EventHandler handler = PanelSelectionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        private readonly MainViewModel _main;

        public MainUnitModel DataModel
        {
            get { return _mainUnit; }
        }

        public event EventHandler MonitorSliderUpdated;

        private void OnMonitorSliderUpdated()
        {
            EventHandler handler = MonitorSliderUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public double MonitorSlider
        {
            get { return _monitorSlider; }
            set
            {
                _monitorSlider = value;
                OnMonitorSliderUpdated();
            }
        }

        private IntervalSettingsViewModel v;
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


        private DiagramData _selectedObject;
        public DiagramData SelectedObject
        {
            get
            {
                return _selectedObject;
            }
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

        public List<SpeakerDataModel> SpeakerDataModels
        {
            get
            {
                return _mainUnit.SpeakerDataModels;
            }
        }

        public List<byte> DspCopy
        {
            get
            {
                return _mainUnit.DspCopy;
            }
        }

        public void ItemsControlClick()
        {
            SelectedObject = null;
        }

        public Action DelayChanged;
        public Action ExtinputUpdate;

        public bool E2PromUpdateVisible
        {
            get
            {
                bool b;
                return bool.TryParse(LibraryData.Settings["EepromUpdateEnabled"], out b) && b;
            }
        }

        public int Id
        {
            get { return _mainUnit.Id; }
            set
            {
                _mainUnit.Id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        public ICommand AddNewCard
        {
            get
            {
                return new RelayCommand(
                    () =>
                    {
                        if (_mainUnit.ExpansionCards > 1)
                            return;
                        _mainUnit.ExpansionCards++;

                        DiagramObjects.OfType<BlLink>().First().Cards(_mainUnit.ExpansionCards);

                        foreach (var n in DiagramObjects.OfType<BlExtInput>()) { n.SetYLocation(); }
                        foreach (var n in DiagramObjects.OfType<BlInputPeq>()) { n.SetYLocation(); }


                        DiagramObjects.OfType<BlEmergency>().First().SetYLocation();

                        var lst = GenDiagramObjects(_mainUnit.Cards.First(i => i.Id == _mainUnit.ExpansionCards), DiagramObjects).ToArray();
                        foreach (var n in lst) { DiagramObjects.Add(n); }
                        OnCardsUpdated(new MainUnitUpdatedEventArgs() { MainUnit = _mainUnit, });

                    },
                    () => _mainUnit.ExpansionCards < 2);
            }
        }

        public ICommand RemoveLastCard
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _mainUnit.ExpansionCards--;
                    DiagramObjects.OfType<BlLink>().First().Cards(_mainUnit.ExpansionCards);

                    foreach (var n in DiagramObjects.OfType<BlExtInput>()) { n.SetYLocation(); }
                    foreach (var n in DiagramObjects.OfType<BlInputPeq>()) { n.SetYLocation(); }

                    DiagramObjects.OfType<BlEmergency>().First().SetYLocation();

                    var removelist = new List<DiagramData>();
                    removelist.AddRange(DiagramObjects.OfType<BlInputName>().Where(s => s.Id % 12 / 4 > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlSpeakerPeq>().Where(s => s.Id % 12 / 4 > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlMonitor>().Where(s => s.Id % 12 / 4 > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlOutput>().Where(s => s.Id % 12 / 4 > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlSpeaker>().Where(s => s.Id % 12 / 4 > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlAmplifier>().Where(s => s.Id % 12 / 4 > _mainUnit.ExpansionCards));

                    removelist.AddRange(DiagramObjects.OfType<BlAuxSpeakerPeq>().Where(s => s.Id > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlAuxiliary>().Where(s => s.Id > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlBackupAmp>().Where(s => s.Id > _mainUnit.ExpansionCards));
                    removelist.AddRange(DiagramObjects.OfType<BlSpMatrix>().Where(s => s.Id > _mainUnit.ExpansionCards));


                    foreach (var d in removelist.OfType<SnapDiagramData>()) { d.RemoveChildren(); }

                    foreach (var diagramData in removelist)
                    {
                        DiagramObjects.Remove(diagramData);
                    }
                    OnCardsUpdated(new MainUnitUpdatedEventArgs() { MainUnit = _mainUnit });

                }, (() => _mainUnit.ExpansionCards > 0));
            }
        }

        public void UpdateGraphics()
        {
            _diagramObjects = null;
            RaisePropertyChanged(() => DiagramObjects);
        }


        public string DisplayValue
        {
            get
            {
                return (Id < 1) ? Main._mainMaster : string.Format(Main._mainSlave, Id);
            }
        }

        private ObservableCollection<DiagramData> _diagramObjects;
        public ObservableCollection<DiagramData> DiagramObjects
        {
            get
            {
                return _diagramObjects ?? (_diagramObjects = new ObservableCollection<DiagramData>(GenDiagramObjects()));
            }
        }

        private BlLink _link;

        private IEnumerable<DiagramData> GenDiagramObjects()
        {
            var link = new BlLink(this);
            var ret = new List<DiagramData> { link };
            _link = link;

            var cards =
                _mainUnit.Cards.Take(_mainUnit.ExpansionCards + 1)
                    .Concat(_mainUnit.Cards.OfType<ExtensionCardModel>());

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

            if (card.GetType() == typeof(CardModel))
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
        /// Put lines of the emergency panels on screen
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="link"></param>
        /// <param name="emergency"></param>
        private static void LinesExtCardModel(List<DiagramData> objects, BlLink link, BlEmergency emergency)
        {
            if (emergency != null && link != null)
                objects.Add(new LineViewModel(link, emergency, 30) { LineType = LineType.Special });

            var extinp = objects.OfType<BlExtInput>().ToArray();
            if (extinp.Length > 2)
                objects.AddRange(new[] { 0, 1, 4 }.Select(n => new LineViewModel(extinp[n], emergency)));

            var inpPeq = objects.OfType<BlInputPeq>().Join(extinp, q => q.Id, j => j.Id, (peq, extInput) => new { peq, extInput }).ToArray();
            objects.AddRange(inpPeq.Select(n => new LineViewModel(n.peq, n.extInput)));

            objects.AddRange(objects.OfType<BlInputPeq>().Select(n => new LineViewModel(n, emergency)).ToArray());
        }

        /// <summary>
        /// add lines to diagram
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="link"></param>
        private static void LinesCardModel(List<DiagramData> objects, BlLink link)
        {



            var t = objects.OfType<BlInputName>().Join(objects.OfType<BlToneControl>(), s => s.Id, q => q.Id, (b, c) =>
                new LineViewModel(b, c) { LineType = LineType.PublicAddress }).ToArray();

            objects.AddRange(t);

            var tone = objects.OfType<BlToneControl>().OrderBy(n => n.Id).ToArray();
            var delay = objects.OfType<BlDelay>().OrderBy(i => i.Id).ToArray();

            if (link != null)
            {
                if (tone.Length > 0)
                    objects.AddRange(
                        tone.Select(q => new LineViewModel(q, link) { Id = q.Id }).OrderBy(n => n.Id).ToArray());
                objects.AddRange(
                    delay
                        .Select(q => new LinkLineVm(link, q, link, q.Id))
                        .ToArray());
                objects.AddRange(objects.OfType<BlSpeakerPeq>().Where(i => i.Id % 12 == 1 || i.Id % 12 == 2).OrderBy(i => i.Id).ToArray()
                    .Join(delay, peq => peq.Id, blDelay => blDelay.Id, (peq, blDelay) =>
                        new LineViewModel(peq, blDelay) { LineType = LineType.Emergency, Id = blDelay.Id }));
            }

            var input = objects.OfType<BlInputName>().OrderBy(n => n.Id).Where(i => i.Id % 12 > 3).ToArray();
            if (input.Length > 0 && link != null)
                objects.AddRange(input.Select(q => new LineViewModel(q, link) { LineType = LineType.PublicAddress }));

            var outputs = objects.OfType<BlAmplifier>()
                .Join(objects.OfType<BlOutput>(), s => s.Id, q => q.Id,
                    (amplifier, blOutput) => new LineViewModel(amplifier, blOutput)).ToArray();

            objects.AddRange(outputs);

            objects.AddRange(
                objects.OfType<BlMonitor>()
                    .Join(objects.OfType<BlSpeakerPeq>(), s => s.Id, q => q.Id, (b, c) => new LineViewModel(b, c))
                    .ToArray());

            var speakerMatix = objects.OfType<BlAmplifier>()
                .Join(objects.OfType<BlSpMatrix>(), s => s.Id % 12 / 4, q => q.Id, (b, c) => new LineViewModel(b, c))
                .ToArray();
            objects.AddRange(speakerMatix);

            if (link != null)
                objects.AddRange(objects.OfType<BlAuxSpeakerPeq>().OrderBy(q => q.Id).Select(n => new LineViewModel(n, link) { Id = n.Id }).ToArray());

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

            speakers[0] = objects.OfType<BlSpeaker>().Where(i => i.Id % 12 < 4).ToList();
            speakers[1] = objects.OfType<BlSpeaker>().Where(i => i.Id % 12 > 3 && i.Id % 12 < 8).ToList();
            speakers[2] = objects.OfType<BlSpeaker>().Where(i => i.Id % 12 > 7).ToList();

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

            objects.AddRange(objects.OfType<BlSpeakerPeq>().Where(q => q.Id % 12 != 1 && q.Id % 12 != 2)
                .Select(sp => new LinkLineVm(link, sp, link, sp.Id)).ToArray());
        }

        public void UpdateLineLinks()
        {
            foreach (var result in _mainUnit.Cards.OfType<CardModel>().SelectMany(f => f.Flows))
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
            else if (flow.Path == LinkTo.No || flow.Id % 12 < 4)
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

            if (flow.Id % 12 < 4 && flow.Id < GenericMethods.StartCountFrom)
            {
                lst.Add(new BlToneControl(flow));
                if (flow.Id % 12 < 3 && flow.Id % 12 > 0)
                    lst.Add(new BlDelay(this, flow));
            }

            if ((flow.Id % 5 == 2 || flow.Id % 5 == 3) &&
                flow.Id >= GenericMethods.StartCountFrom)
            {
                lst.Add(new BlInputPeq(flow, this));
            }

            if (flow.Id < GenericMethods.StartCountFrom && flow.Id % 12 % 4 == 0)
            {

            }
            foreach (var snapDiagramData in lst)
            {
                snapDiagramData.SetYLocation();
            }

            return lst;
        }

        public ICommand ButtonFinished
        {
            get
            {
                return new RelayCommand(() =>
                {
                    BusyDownloading = BusyDownloading.No;
                });
            }
        }

        private double _progress;
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }

        private BusyDownloading _busyDownloading;
        private double _monitorSlider;
        private ConnectType _connectType;

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

        public event EventHandler<MainUnitUpdatedEventArgs> CardsUpdated;

        private void OnCardsUpdated(MainUnitUpdatedEventArgs e)
        {
            EventHandler<MainUnitUpdatedEventArgs> handler = CardsUpdated;
            if (handler != null) handler(this, e);
        }

        public event EventHandler SdCardMessagesReceived;

        public void OnSdCardMessagesReceived()
        {
            EventHandler handler = SdCardMessagesReceived;
            if (handler != null) handler(this, EventArgs.Empty);
            
        }

        public event EventHandler SdCardPositionsReceived;

        public void OnSdCardPositionsReceived()
        {
            EventHandler handler = SdCardPositionsReceived;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler KreisUpdated;

        public void OnKreisUpdated()
        {
            EventHandler handler = KreisUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }


        public event EventHandler RedundancyUpdated;

        public void OnRedundancyUpdated()
        {
            EventHandler handler = RedundancyUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler RoutingTableUpdated;

        public void OnRoutingTableUpdated()
        {
            EventHandler handler = RoutingTableUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler PresetNamesUpdated;

        public void OnPresetNamesUpdated()
        {
            EventHandler handler = PresetNamesUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler DspMirrorUpdated;

        public void OnDspMirrorUpdated()
        {
            EventHandler handler = DspMirrorUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }

    public class MainUnitUpdatedEventArgs : EventArgs
    {
        public MainUnitModel MainUnit { get; set; }
    }
}