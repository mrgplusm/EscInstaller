using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Common.Model;
using EscInstaller.ImportSpeakers;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Common;
using Common.Commodules;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace EscInstaller.ViewModel.Settings
{
    public enum SpeakerValue
    {
        Name,
        IsCustom,
        Id,
        Color,
    }

    public class SpeakerDataViewModel : ViewModelBase, IDragable
    {
        private readonly SpeakerDataModel _speakerData;
        private readonly int? _flowId;


        private bool _isEnabled;
        private LineGraph _masterLine;

        /// <summary>
        ///     Parameters for equalizer controlled by user
        /// </summary>
        private ObservableCollection<PeqDataViewModel> _peqDataViewModels;

        private double _dbMagnitude;


#if DEBUG
        /// <summary>
        /// Design constructor
        /// </summary>
        public SpeakerDataViewModel()
        {
            _speakerData = new SpeakerDataModel();
            _speakerData.PEQ = new List<PeqDataModel>() { new PeqDataModel(), new PeqDataModel() };
        }
#endif

        public SpeakerDataViewModel(SpeakerDataModel speakerData, int? flowId = null)
        {
            _speakerData = speakerData;
            _flowId = flowId;



            //set magniture directly in case speaker loaded
            if (speakerData.PEQ != null)
                DbMagnitude = Fourier(speakerData.PEQ).Max();

            _spo = new Lazy<SpeakerLogic>(() => new SpeakerLogic(_speakerData));


        }

        private readonly Lazy<SpeakerLogic> _spo;

        public SpeakerPeqType SpeakerPeqType
        {
            get { return DataModel.SpeakerPeqType; }
            //set
            //{
            //    DataModel.SpeakerPeqType = value;
            //    var q = new SpeakerLogic(_speakerData);
            //    _spo.
            //}
        }


        private static ObservableCollection<SpeakerPeqType> _speakerPeqTypes;
        public ObservableCollection<SpeakerPeqType> SpeakerPeqTypes
        {
            get
            {
                return _speakerPeqTypes ?? (_speakerPeqTypes = new ObservableCollection<SpeakerPeqType>(Enum.GetValues(typeof(SpeakerPeqType))
                    .Cast<SpeakerPeqType>()));
            }
        }






        public SpeakerDataModel DataModel
        {
            get { return _speakerData; }
        }


        /// <summary>
        ///     Loads data in current window
        /// </summary>
        public void Load(SpeakerDataViewModel librarySpeaker)
        {
            if (librarySpeaker.RequiredBiquads >
                ((int)DataModel.SpeakerPeqType))
            {
                MessageBox.Show(string.Format(
                    SpeakerLibrary.NotEnoughSpaceWarning, librarySpeaker.RequiredBiquads,
                    (int)DataModel.SpeakerPeqType),
                    SpeakerLibrary.NotEnoughSpaceWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning,
                    MessageBoxResult.OK);
                return;
            }
            Clear();
            SendMyName();
            AddRange(librarySpeaker.DataModel.PEQ.ToArray());
            CopySpeakerName(librarySpeaker.SpeakerName);
            RaisePropertyChanged(() => InLibrary);
            //copy the data to the model
            if (_flowId.HasValue)
            {
                foreach (var pd in _spo.Value.GetPresetData(_flowId.Value))
                {
                    CommunicationViewModel.AddData(pd);
                }
            }

            OnSpeakerNameChanged();
        }


        public LineGraph MasterLine
        {
            get
            {
                var fourier = Fourier(PeqDataModels).ToArray();
                DbMagnitude = fourier.Max(p => p);

                return _masterLine ?? (_masterLine = new LineGraph
                    {
                        StrokeThickness = 2,
                        Stroke = Brushes.DarkBlue,
                        DataSource = GenLine(fourier),
                        ZIndex = 30,
                    });
            }
        }

        /// <summary>
        /// prevents the user from editing a speaker with the same name. Only when a flowId is specified
        /// </summary>
        public bool InLibrary
        {
            get { return _flowId.HasValue && IsLibraryDuplicate(SpeakerName); }
        }

        private static bool IsLibraryDuplicate(string name)
        {
            if ((string.IsNullOrWhiteSpace(name)) || (name.Trim().Length < 1)) return false;
            return SpeakerMethods.Library.Any(y => !y.IsCustom && y.SpeakerName == name);
        }

        public void RemovePeqParam(PeqDataViewModel s)
        {
            RemoveVm(s);
            OnRemoveParam(s);
            RedrawMasterLine();
            if (!_flowId.HasValue) return;

            CommunicationViewModel.AddData(_spo.Value.GetClearBiquadData(s.PeqDataModel.DspBiquads, _flowId.Value));
        }

        public void RefreshBiquads()
        {
            DetachGraphHandlers();
            _peqDataViewModels = null;

            RaisePropertyChanged(() => PeqDataViewModels);
            AttachGraphHandlers();

            RedrawMasterLine();
        }


        private RelayCommand _addNewParam;

        public ICommand AddNewParam
        {
            get
            {
                if (IsInDesignMode) return null;
                return _addNewParam ?? (_addNewParam =

                    new RelayCommand(() =>
                    {
                        var vm = NewParam();
                        if (vm == null) return;

                        PlotterChildren.Add(vm.LineData);
                        PlotterChildren.Add(vm.DraggablePoint);
                        SetModelHandlers(vm);

                        RedrawMasterLine();

                        if (!_flowId.HasValue) return;
                        try
                        {
                            var q = new PeqDataLogic(vm.PeqDataModel);

                            IEnumerable<IDispatchData> data = q.GetParamData(_spo.Value, _flowId.Value).ToList();
                            CommunicationViewModel.AddData(data);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Filter upload", MessageBoxButton.OK, MessageBoxImage.Error,
                                MessageBoxResult.OK);
                        }
                    }, () => _spo.Value.DataModel.AvailableBiquads != null &&
                             PeqDataModels.RequiredBiquads() < (int)SpeakerPeqType));
            }
        }

        /// <summary>
        /// This function is called when a biquad parameter has been changed
        /// </summary>
        /// <param name="model1"></param>
        /// <param name="pf"></param>
        protected virtual void OnChangeBiquads(PeqDataViewModel model1, PeqField pf)
        {
            IsCustom = true;
            RedrawMasterLine();

            if (pf == PeqField.FilterType &&
                !model1.HasBandwidth())
            {
                RemoveBandwith(model1);
            }

            if (!_flowId.HasValue) return;
            var op = new PeqDataLogic(model1.PeqDataModel);

            if (!model1.IsEnabled)
            {
                CommunicationViewModel.AddData(_spo.Value.GetClearBiquadData(model1.PeqDataModel.DspBiquads, _flowId.Value));
                return;
            }

            try
            {
                IEnumerable<IDispatchData> data = op.GetParamData(_spo.Value, _flowId.Value).ToList();
                CommunicationViewModel.AddData(data);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Filter upload", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
        }

        private void SetModelHandlers(PeqDataViewModel model1)
        {
            model1.BiquadsChanged = OnChangeBiquads;

            model1.DraggablePoint.MouseRightButtonDown += (sender, args) =>
            {
                if (!model1.HasBandwidth()) return;
                if (PlotterChildren.Contains(model1.BandWidthPoint) ||
                    PlotterChildren.Contains(model1.BandwidthArrow)) return;

                if (model1.BandWidthPoint == null) return;
                PlotterChildren.Add(model1.BandWidthPoint);

                if (model1.BandwidthArrow == null) return;
                PlotterChildren.Add(model1.BandwidthArrow);
            };

            model1.BandWidthPoint.MouseRightButtonDown += (q, i) => RemoveBandwith(model1);
            model1.RemoveThisParam += () => RemovePeqParam(model1);
        }



        private void RemoveBandwith(PeqDataViewModel model1)
        {
            PlotterChildren.Remove(model1.BandWidthPoint);
            PlotterChildren.Remove(model1.BandwidthArrow);
        }

        public ICommand ClearParams
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBox.Show(SpeakerLibrary.DeleteAllParamsVerify, SpeakerLibrary.DeleteAllParamsVerifyTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Cancel)
                        return;

                    if (_flowId.HasValue)
                    {
                        var z = _spo.Value.PresetNameFactory(_flowId.Value);
                        if (z != null)
                            CommunicationViewModel.AddData(z);
                    }
                    foreach (var s in PeqDataViewModels)
                    {
                        RemoveVm(s);
                    }
                    Clear();
                    SendMyName();
                    RedrawMasterLine();
                }

                    );
            }
        }

        private void RemoveVm(PeqDataViewModel s)
        {
            PlotterChildren.Remove(s.DraggablePoint);
            PlotterChildren.Remove(s.LineData);
            RemoveBandwith(s);
        }

        protected void AttachGraphHandlers()
        {
            foreach (var model in PeqDataViewModels)
            {
                if (model.LineData != null)
                    PlotterChildren.Add(model.LineData);
                if (model.DraggablePoint != null)
                    PlotterChildren.Add(model.DraggablePoint);
                SetModelHandlers(model);
            }
            PlotterChildren.Add(MasterLine);
        }

        public void DetachGraphHandlers()
        {
            while (PlotterChildren.Count > 0)
            {
                PlotterChildren.RemoveAt(0);
            }
        }




        private ObservableCollection<IPlotterElement> _plotter;
        public ObservableCollection<IPlotterElement> PlotterChildren
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new ObservableCollection<IPlotterElement>();
                }
                if (_plotter == null)
                {
                    _plotter = new ObservableCollection<IPlotterElement>();
                    AttachGraphHandlers();
                }
                return _plotter;
            }
        }

        public ObservableCollection<PeqDataViewModel> PeqDataViewModels
        {
            get
            {
                if (_peqDataViewModels == null)
                {
                    _peqDataViewModels = new ObservableCollection<PeqDataViewModel>();

                    foreach (var z in PeqDataModels.Select(n => new PeqDataViewModel(n, this)))
                    {
                        _peqDataViewModels.Add(z);
                    }

                    _peqDataViewModels.CollectionChanged += (sender, args) => RaisePropertyChanged(() => Biquads);
                    AttachRemoveDelegate();

                    ReorderIds(_peqDataViewModels);

                }
                return _peqDataViewModels;
            }
        }

        private void AttachRemoveDelegate()
        {
            foreach (var peq in _peqDataViewModels)
            {
                var peq1 = peq;
                peq.RemoveThisParam += () => RemovePeqParam(peq1);
            }
        }

        public int RequiredBiquads
        {
            get { return PeqDataModels.RequiredBiquads(); }
        }

        public List<PeqDataModel> PeqDataModels
        {
            get { return _speakerData.PEQ ?? (_speakerData.PEQ = new List<PeqDataModel>()); }
        }

        public bool IsCustom
        {
            get { return _speakerData.IsCustom; }
            set { _speakerData.IsCustom = value; }
        }

        public Action SpeakerNameChanged;

        public string SpeakerName
        {
            get { return DisplayValue(_speakerData); }
            set
            {
                SetSpeakerName(value);
            }
        }

        private void SetSpeakerName(string value)
        {
            var q = (SpeakerMethods.Library.FirstOrDefault(y => !y.IsCustom && y.SpeakerName == value));
            if (q != null && !q.Equals(this))
                MessageBox.Show(
                    SpeakerLibrary.NameExists,
                    SpeakerLibrary.NameExistsTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                _speakerData.SpeakerName = value.Truncate(13);
                SendMyName();
                RaisePropertyChanged(() => InLibrary);
            }
            RaisePropertyChanged(() => SpeakerName);
            OnSpeakerNameChanged();

        }

        public void OnSpeakerNameChanged()
        {
            if (SpeakerNameChanged != null)
                SpeakerNameChanged();
        }

        public string DisplayId
        {
            get { return (Id + 1).ToString(CultureInfo.InvariantCulture); }
        }

        public int Id
        {
            get { return _speakerData.Id; }
            set
            {
                if (value == _speakerData.Id) return;

                _speakerData.Id = value;
                RaisePropertyChanged(() => Id);
                RaisePropertyChanged(() => DisplayId);
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => _isEnabled);
            }
        }

        public Brush Color
        {
            get { return _speakerData.IsCustom ? Brushes.Brown : Brushes.Black; }
        }


        public Action<PeqDataViewModel> ParamRemoved { get; set; }

        /// <summary>
        /// Amount of biquads used by this speaker
        /// </summary>
        public int Biquads
        {
            get { return PeqDataModels.Sum(p => (p.Order + 1) >> 1); }
        }

        public string FilterCountText
        {
            get { return string.Format("Max {0} second order filters", (int)SpeakerPeqType); }
        }

        public PeqDataViewModel NewParam()
        {
            if (RequiredBiquads <=
                (int)(DataModel.SpeakerPeqType))
            {
                var dm = new PeqDataModel
                    {
                        Frequency = 1000,
                        FilterType = FilterType.Peaking,
                        IsEnabled = true,
                        BandWidth = 1,
                        Boost = 0,
                        Order = 2,
                        Id = PeqDataModels.Count
                    };

                //vms might not be initiated, add viewmodel before model as initialisation wraps all available models
                var vm = new PeqDataViewModel(dm, this);
                PeqDataViewModels.Add(vm);

                PeqDataModels.Add(dm);

                return vm;
            }
            throw new Exception("not enough biquads");
        }

        private void AddNew(PeqDataModel data)
        {
            var dm = new PeqDataModel
                {
                    Id = PeqDataModels.Count,
                    FilterType = data.FilterType,
                    IsEnabled = data.IsEnabled,
                    BandWidth = data.BandWidth,
                    Boost = data.Boost,
                    Frequency = data.Frequency,
                    Order = data.Order,
                    Gain = data.Gain,
                };
            //dm.Parse(data);

            PeqDataModels.Add(dm);
            var vm = new PeqDataViewModel(dm, this);
            vm.RemoveThisParam += () => RemovePeqParam(vm);
            PeqDataViewModels.Add(vm);
        }
        /// <summary>
        /// Add new biquads to this speaker (e.g. for import)
        /// </summary>
        /// <param name="peqdata"></param>
        public void AddRange(IList<PeqDataModel> peqdata)
        {
            if (peqdata == null) return;
            if (RequiredBiquads + peqdata.RequiredBiquads() <=
                (int)DataModel.SpeakerPeqType)
            {
                var id = PeqDataModels.Count;
                foreach (var peqDataModel in peqdata)
                {
                    peqDataModel.Id = id;
                    AddNew(peqDataModel);
                }
                RaisePropertyChanged(() => PeqDataModels);


                DetachGraphHandlers();
                AttachGraphHandlers();
                RedrawMasterLine();
                return;
            }

            MessageBox.Show("Not enough space left", "Does not fit", MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }

        private static CompositeDataSource GenLine(IEnumerable<double> fourier)
        {
            var xDs = DspCoefficients.XList.AsXDataSource();
            //_dbCorrect = -t.Max();

            return fourier.AsYDataSource().Join(xDs);
        }

        private static IEnumerable<double> Fourier(IEnumerable<PeqDataModel> data)
        {
            var xs = DspCoefficients.XList;
            return (from d in xs
                    let cfilter = new Complex(1, 0)
                    select data.Where(s => s.IsEnabled).Aggregate(cfilter,
                                                                  (current, peqData) =>
                                                                  current *
                                                                  DspCoefficients.FilterFuncs[peqData.FilterType](d,
                                                                                                                  peqData))
                        into cfilterx
                        select cfilterx.DbMagnitude());
        }

        public Action DbMagnitudeChanged;

        public double DbMagnitude
        {
            get { return _dbMagnitude; }
            set
            {
                _dbMagnitude = value;
                if (DbMagnitudeChanged != null)
                    DbMagnitudeChanged.Invoke();
            }
        }


        public void RedrawMasterLine()
        {
            if (_masterLine == null) return;
            var fourier = Fourier(PeqDataModels).ToArray();
            _masterLine.DataSource = GenLine(fourier);
            DbMagnitude = fourier.Max(p => p);
        }

        public static string DisplayValue(SpeakerDataModel model, bool isPreset = false)
        {
            if (string.IsNullOrWhiteSpace(model.SpeakerName)) return string.Empty;
            return model.SpeakerName.Truncate(13);
        }

        public void CopySpeakerName(string speakername)
        {
            _speakerData.SpeakerName = speakername;
            RaisePropertyChanged(() => SpeakerName);
        }


        public void OnRemoveParam(PeqDataViewModel s)
        {
            if (ParamRemoved != null)
                ParamRemoved.Invoke(s);
            PeqDataModels.Remove(s.PeqDataModel);
            PeqDataViewModels.Remove(s);
            ReorderIds(PeqDataViewModels);
        }

        private static void ReorderIds(IEnumerable<PeqDataViewModel> peqDataViewModels)
        {
            var i = 0;
            foreach (var model in peqDataViewModels)
            {
                model.Index = i++;
            }
        }

        public void Clear()
        {
            PeqDataModels.Clear();
            PeqDataViewModels.Clear();
            SetSpeakerName(string.Empty);
        }

        public void SendMyName()
        {
            if (!_flowId.HasValue) return;
            var package = _spo.Value.PresetNameFactory(_flowId.Value);
            if (package != null)
                CommunicationViewModel.AddData(package);
        }

        public Type DataType
        {
            get { return typeof(SpeakerDataViewModel); }
        }

        public Action<SpeakerDataViewModel> RemoveThis;

        public void Remove()
        {
            if (RemoveThis != null)
                RemoveThis(this);
        }
    }
}