#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Commodules;
using Common.Model;
using EscInstaller.EscCommunication.Logic;
using EscInstaller.ViewModel;

#endregion

namespace EscInstaller.EscCommunication.UploadItem
{
    internal class PeqData : DownloadData
    {
        public PeqData(MainUnitViewModel main) : base(main)
        {
            var d = new PeqUpload(main.DataModel);

            foreach (var key in d.PresetOffset.Keys)
            {
                AddChild(new PresetSpeakers(main, key));
            }                       
        }
        
        public override string Value => "Peq Data";        
    }
    
    internal class PresetSpeakers : DownloadData
    {
        private readonly SpeakerPeqType _type;
        public override string Value => $"Presets {_type}";

        public PresetSpeakers(MainUnitViewModel main, SpeakerPeqType type) : base(main)
        {
            _type = type;
            var p = new PeqUpload(main.DataModel);
            foreach (var q in p.PresetModels(type).Select((model, id) => new {model, id}))
            {
                AddChild(new PresetSpeaker(main, q.model, q.id));
       //         AddChild(new PresetRedundancy(main, q.model, q.id));
            }
        }
    }

    internal class PresetSpeaker : DownloadData
    {
        private readonly SpeakerDataModel _model;
        private readonly int _flowId;
        public override string Value => $"Filter {_flowId}";

        public PresetSpeaker(MainUnitViewModel main, SpeakerDataModel model, int flowId) : base(main)
        {
            _model = model;
            _flowId = flowId;
        }

        protected override Task Function
        {
            get
            {
                var p = new PeqUpload(Main.DataModel);
                return p.SetData(ProgressFactory(), Cancellation.Token, p.DspData(_model).ToArray());
            }
        }
    }

    //internal class PresetRedundancy : DownloadData
    //{
    //    private readonly SpeakerDataModel _model;
    //    private readonly int _flowId;
    //    public override string Value => $"Redundancy {_flowId}";

    //    public PresetRedundancy(MainUnitViewModel main, SpeakerDataModel model, int flowId) : base(main)
    //    {
    //        _model = model;
    //        _flowId = flowId;
    //    }

    //    protected override Task Function
    //    {
    //        get
    //        {
    //            var p = new PeqUpload(Main.DataModel);
    //            return p.SetData(ProgressFactory(), Cancellation.Token, p.RedundancyData(_model).ToArray());
    //        }
    //    }
    //}
}