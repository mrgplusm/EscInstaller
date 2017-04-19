#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;

using Common;

using Common.Commodules;
using Common.Model;
using EscInstaller.ImportSpeakers;
using EscInstaller.View;
using EscInstaller.ViewModel.Connection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

#endregion

namespace EscInstaller.ViewModel.Settings.Peq
{
    public enum SpeakerValue
    {
        Name,
        IsCustom,
        Id,
        Color
    }

    public class SpeakerDataViewModel : ViewModelBase
    {
        private static ObservableCollection<SpeakerPeqType> _speakerPeqTypes;

        private readonly Dictionary<PeqField, Action<BiquadsChangedEventArgs, PeqDataViewModel>> _actionForField =
            new Dictionary<PeqField, Action<BiquadsChangedEventArgs, PeqDataViewModel>>();

        private readonly int? _flowId;
        private RelayCommand _addNewParam;
        private bool _isEnabled;
        private LineGraph _masterLine;

        /// <summary>
        ///     Parameters for equalizer controlled by user
        /// </summary>
        private ObservableCollection<PeqDataViewModel> _peqDataViewModels;

        private ObservableCollection<IPlotterElement> _plotter;
#if DEBUG
        /// <summary>
        ///     Design constructor
        /// </summary>
        public SpeakerDataViewModel()
        {
            DataModel = new SpeakerDataModel
            {
                PEQ = new List<PeqDataModel>() { new PeqDataModel(), new PeqDataModel() }
            };
        }
#endif

        public SpeakerDataViewModel(SpeakerDataModel speakerData, int? flowId = null)
        {
            DataModel = speakerData;
            _flowId = flowId;

            if (speakerData.PEQ != null)
                DbMagnitude = Fourier(speakerData.PEQ).Max();

            PopulateActionField();
        }

        public SpeakerPeqType SpeakerPeqType
        {
            get { return DataModel.SpeakerPeqType; }
        }

        public ObservableCollection<SpeakerPeqType> SpeakerPeqTypes => _speakerPeqTypes ??
                                                                       (_speakerPeqTypes =
                                                                           new ObservableCollection<SpeakerPeqType>(Enum.GetValues(typeof(SpeakerPeqType))
                                                                               .Cast<SpeakerPeqType>()));

        public SpeakerDataModel DataModel { get; }

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
                    ZIndex = 30
                });
            }
        }

        /// <summary>
        ///     prevents the user from editing a speaker with the same name. Only when a flowId is specified
        /// </summary>
        public bool InLibrary => _flowId.HasValue && IsLibraryDuplicate(SpeakerName);

        public ICommand AddNewParam
        {
            get
            {
                if (IsInDesignMode) return null;
                return _addNewParam ?? (_addNewParam =
                    new RelayCommand(AddParam, () => PeqDataModels.RequiredBiquads() < (int)SpeakerPeqType));
            }
        }

        public ICommand ClearParams
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBox.Show(SpeakerLibrary.DeleteAllParamsVerify, SpeakerLibrary.DeleteAllParamsVerifyTitle,
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) ==
                        MessageBoxResult.Cancel)
                        return;
                    ClearSpeaker();
                });
            }
        }

        public ObservableCollection<IPlotterElement> PlotterChildren
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new ObservableCollection<IPlotterElement>();
                }
                if (_plotter != null) return _plotter;
                _plotter = new ObservableCollection<IPlotterElement>();
                AttachGraphHandlers();
                return _plotter;
            }
        }

        public ObservableCollection<PeqDataViewModel> PeqDataViewModels
        {
            get
            {
                if (_peqDataViewModels != null) return _peqDataViewModels;
                _peqDataViewModels = new ObservableCollection<PeqDataViewModel>();

                foreach (var z in PeqDataModels.Select(n => new PeqDataViewModel(n)))
                {
                    _peqDataViewModels.Add(z);
                }

                _peqDataViewModels.CollectionChanged += (sender, args) => RaisePropertyChanged(() => Biquads);
                AttachRemoveDelegate();

                ReorderIds(_peqDataViewModels);
                return _peqDataViewModels;
            }
        }

        public int RequiredBiquads => PeqDataModels.RequiredBiquads();

        public List<PeqDataModel> PeqDataModels => DataModel.PEQ ?? (DataModel.PEQ = new List<PeqDataModel>());

        public bool IsCustom
        {
            get { return DataModel.IsCustom; }
            set { DataModel.IsCustom = value; }
        }

        public string SpeakerName
        {
            get { return DisplayValue(DataModel); }
            set { SetSpeakerName(value); }
        }

        public string DisplayId => (Id + 1).ToString(CultureInfo.InvariantCulture);

        public int Id
        {
            get { return DataModel.Id; }
            set
            {
                if (value == DataModel.Id) return;

                DataModel.Id = value;
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

        public Brush Color => DataModel.IsCustom ? Brushes.Brown : Brushes.Black;

        /// <summary>
        ///     Amount of biquads used by this speaker
        /// </summary>
        public int Biquads
        {
            get { return PeqDataModels.Sum(p => (p.Order + 1) >> 1); }
        }

        public string FilterCountText => $"Max {(int)SpeakerPeqType} second order filters";

        public double DbMagnitude { get; set; }

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
            PerformChanges(librarySpeaker);
        }

        private void PerformChanges(SpeakerDataViewModel librarySpeaker)
        {
            Clear();
            AddRange(librarySpeaker.DataModel.PEQ.ToArray());
            CopySpeakerName(librarySpeaker.SpeakerName);
            LoadProperties();
        }

        private void LoadProperties()
        {
            SendMyName();
            RaisePropertyChanged(() => InLibrary);

            //copy the data to the model
            if (_flowId.HasValue)
            {
                var sl = new SpeakerLogicForFlow(DataModel, _flowId.Value);
                sl.UpdateIntegraty();
                try
                {
                    var data = sl.TotalSpeakerData().ToArray();
                    foreach (var dispatchData in data)
                    {
                        CommunicationViewModel.AddData(dispatchData);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            OnSpeakerNameChanged();
        }

        private static bool IsLibraryDuplicate(string name)
        {
            if ((string.IsNullOrWhiteSpace(name)) || (name.Trim().Length < 1)) return false;
            return SpeakerMethods.Library.Any(y => !y.IsCustom && y.SpeakerName == name);
        }

        private void RemovePeqParam(object sender, EventArgs eventArgs)
        {
            var s = (PeqDataViewModel)sender;
            RemoveVm(s);
            OnRemoveParam(s);
            RedrawMasterLine();
            if (!_flowId.HasValue) return;

            var sl = new SpeakerLogicForFlow(DataModel, _flowId.Value);
            var emptyDspdata = sl.GetEmptyDspData(s.PeqDataModel.Biquads);
            var emptyRedundancy = sl.GetEmtptyRedundancyData(s.PeqDataModel.Biquads);

            foreach (var peqParam in emptyDspdata)
            {
                CommunicationViewModel.AddData(peqParam);
            }

            foreach (var setE2PromExt in emptyRedundancy)
            {
                CommunicationViewModel.AddData(setE2PromExt);
            }
        }

        public void RefreshBiquads()
        {
            DetachGraphHandlers();
            _peqDataViewModels = null;

            RaisePropertyChanged(() => PeqDataViewModels);
            AttachGraphHandlers();

            RedrawMasterLine();
        }

        private void AddParam()
        {
            var vm = NewParam();
            if (vm == null) return;

            PlotterChildren.Add(vm.LineData);
            PlotterChildren.Add(vm.DraggablePoint);
            SetModelHandlers(vm);

            RedrawMasterLine();

            SendParamData(vm.PeqDataModel);
        }

        private void SendParamData(PeqDataModel dm)
        {
            if (!_flowId.HasValue) return;
            try
            {
                var sl = new SpeakerLogicForFlow(DataModel, _flowId.Value);

                var data = sl.DspData(dm);
                var redundancy = sl.RedundancyData();

                foreach (var setE2PromExt in redundancy)
                {
                    CommunicationViewModel.AddData(setE2PromExt);
                }
                foreach (var peqParam in data)
                {
                    CommunicationViewModel.AddData(peqParam);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Filter upload", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);
            }
        }

        private void OrderChanged(BiquadsChangedEventArgs bf, PeqDataViewModel vm)
        {
            //check if this change fits in current filter:
            //value has to be smaller than current value; 
            //the order is odd. In this case half of a biquad is used already;
            //There is space left.
            var oldValue = vm.Order;
            vm.SetOrder(bf.RequestOrder);
            if (DataModel.PEQ.RequiredBiquads() <= (int)DataModel.SpeakerPeqType)
            {
                vm.SetOrder(bf.RequestOrder);
                vm.UpdateFilterType();
                UpdateBiquads(vm);
                SendParamData(vm.PeqDataModel);
            }
            else
            {
                vm.SetOrder(oldValue);
                bf.Cancel = true;
            }
        }

        private void UpdateBiquads(PeqDataViewModel vm)
        {
            if (!_flowId.HasValue) return;
            var usedBiquads = new List<int>(vm.PeqDataModel.Biquads);
            var sl = new SpeakerLogicForFlow(DataModel, _flowId.Value);
            sl.AssignBiquads(vm.PeqDataModel);
            var toClear = usedBiquads.Except(vm.PeqDataModel.Biquads).ToArray();

            foreach (var peqParam in sl.GetEmptyDspData(toClear))
            {
                CommunicationViewModel.AddData(peqParam);
            }

            foreach (var e2PromExt in sl.GetEmtptyRedundancyData(toClear))
            {
                CommunicationViewModel.AddData(e2PromExt);
            }
        }

        private void TypeChanged(BiquadsChangedEventArgs bf, PeqDataViewModel vm)
        {
            if ((vm.FilterType != FilterType.LinkWitzHp && vm.FilterType != FilterType.LinkWitzLp && vm.Order < 3) &&
                (bf.RequestFilterType == FilterType.LinkWitzHp || bf.RequestFilterType == FilterType.LinkWitzLp) &&
                (DataModel.PEQ.RequiredBiquads() >= (int)DataModel.SpeakerPeqType))
            {
                bf.Cancel = true;
                vm.ResetType();
                return;
            }

            vm.SetType(bf.RequestFilterType);
            vm.SetOrder(2);
            vm.UpdateFilterType();
            UpdateBiquads(vm);
            SendParamData(vm.PeqDataModel);
        }

        private void FilterChanged(BiquadsChangedEventArgs bf, PeqDataViewModel vm)
        {
            SendParamData(vm.PeqDataModel);
        }

        private void PopulateActionField()
        {
            _actionForField.Add(PeqField.Order, OrderChanged);
            _actionForField.Add(PeqField.FilterType, TypeChanged);

            _actionForField.Add(PeqField.Bandwidth, FilterChanged);
            _actionForField.Add(PeqField.Boost, FilterChanged);
            _actionForField.Add(PeqField.Frequency, FilterChanged);
            _actionForField.Add(PeqField.IsEnabled, FilterChanged);
        }

        /// <summary>
        ///     This function is called when a biquad parameter has been changed
        /// </summary>
        protected virtual void OnChangeBiquads(object sender, BiquadsChangedEventArgs pf)
        {
            var model1 = (PeqDataViewModel)sender;
            IsCustom = true;

            if (pf.PeqField == PeqField.FilterType &&
                !model1.HasBandwidth())
            {
                RemoveBandwith(model1);
            }

            _actionForField[pf.PeqField](pf, model1);
            RedrawMasterLine();
        }

        private void SetModelHandlers(PeqDataViewModel model1)
        {
            model1.BiquadsChanged += OnChangeBiquads;

            model1.DraggablePoint.MouseRightButtonDown += (sender, args) =>
            {
                if (!model1.HasBandwidth()) return;
                if (PlotterChildren.Contains(model1.BandWidthPoint) ||
                    PlotterChildren.Contains(model1.BandwidthArrow))
                    return;

                if (model1.BandWidthPoint == null) return;
                PlotterChildren.Add(model1.BandWidthPoint);

                if (model1.BandwidthArrow == null) return;
                PlotterChildren.Add(model1.BandwidthArrow);
            };

            model1.BandWidthPoint.MouseRightButtonDown += (q, i) => RemoveBandwith(model1);
            model1.RemoveThisParam += RemovePeqParam;
        }

        private void RemoveBandwith(PeqDataViewModel model1)
        {
            PlotterChildren.Remove(model1.BandWidthPoint);
            PlotterChildren.Remove(model1.BandwidthArrow);
        }

        private void ClearSpeaker()
        {
            foreach (var s in PeqDataViewModels.ToArray())
            {
                RemovePeqParam(s, EventArgs.Empty);
            }

            SetSpeakerName(string.Empty);
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

        private void AttachRemoveDelegate()
        {
            foreach (var peq in _peqDataViewModels)
            {
                peq.RemoveThisParam += RemovePeqParam;
            }
        }

        public event EventHandler SpeakerNameChanged;

        protected virtual void OnSpeakerNameChanged()
        {
            SpeakerNameChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateSpeakerName()
        {
            RaisePropertyChanged(() => SpeakerName);
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
                DataModel.SpeakerName = value.Truncate(13);
                SendMyName();
                RaisePropertyChanged(() => InLibrary);
            }
            RaisePropertyChanged(() => SpeakerName);
            OnSpeakerNameChanged();
        }

        public PeqDataViewModel NewParam()
        {
            if (RequiredBiquads > (int)(DataModel.SpeakerPeqType)) throw new Exception("not enough biquads");

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
            var vm = new PeqDataViewModel(dm);
            PeqDataViewModels.Add(vm);

            PeqDataModels.Add(dm);
            var sl = new SpeakerLogic(DataModel);
            sl.AssignBiquads(dm);

            return vm;
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
                Gain = data.Gain
            };
            //dm.Parse(data);

            PeqDataModels.Add(dm);
            var vm = new PeqDataViewModel(dm);
            vm.RemoveThisParam += RemovePeqParam;
            PeqDataViewModels.Add(vm);
        }

        /// <summary>
        ///     Add new biquads to this speaker (e.g. for import)
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

        public void RedrawMasterLine()
        {
            if (_masterLine == null) return;
            var fourier = Fourier(PeqDataModels).ToArray();
            _masterLine.DataSource = GenLine(fourier);
            DbMagnitude = fourier.Max(p => p);
        }

        public static string DisplayValue(SpeakerDataModel model, bool isPreset = false)
        {
            return string.IsNullOrWhiteSpace(model.SpeakerName) ? string.Empty : model.SpeakerName.Truncate(13);
        }

        public void CopySpeakerName(string speakername)
        {
            DataModel.SpeakerName = speakername;
            RaisePropertyChanged(() => SpeakerName);
        }

        public void OnRemoveParam(PeqDataViewModel s)
        {
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
        }

        public void SendMyName()
        {
            if (!_flowId.HasValue) return;
            var d = new SpeakerLogicForFlow(DataModel, _flowId.Value);
            var package = d.PresetNameFactory();
            CommunicationViewModel.AddData(package);
        }
    }
}