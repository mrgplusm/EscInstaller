#region

using System.Collections.Generic;
using Common;
using Common.Model;

#endregion

namespace EscInstaller.ViewModel.Settings.Peq
{
    public class PeakingFilter : PeqDataViewModel
    {
        public PeakingFilter(PeqDataModel peq)
            : base(peq)
        {
        }

        public override IEnumerable<SOS> FilterData
        {
            get
            {
                return new[]
                {
                    DspCoefficients.GetBiquadSos(PeqDataModel.Gain, PeqDataModel.Frequency.W(),
                        PeqDataModel.BandWidth, false, DspCoefficients.Fs)
                };
            }
        }

        public override FilterType FilterType
        {
            get { return FilterType.Peaking; }
            set { }
        }
    }
}