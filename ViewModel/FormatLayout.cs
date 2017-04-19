using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EscInstaller.ViewModel.OverView;

namespace EscInstaller.ViewModel
{


    public class FormatLayout
    {
        private readonly List<DiagramData> _objects;
        private readonly BlLink _link;

        public FormatLayout(List<DiagramData> objects, BlLink link)
        {
            _objects = objects;
            _link = link;



        }

        public void DoFormat()
        {
            LinesCardModel();
        }

        private void AddToneControlLines()
        {
            var t = _objects.OfType<BlInputName>().Join(_objects.OfType<BlToneControl>(), s => s.Id, q => q.Id, (b, c) =>
            new LineViewModel(b, c) { LineType = LineType.PublicAddress }).ToArray();

            _objects.AddRange(t);
        }

        private void LinesCardModel()
        {
            AddToneControlLines();

            var tone = _objects.OfType<BlToneControl>().OrderBy(n => n.Id).ToArray();
            var delay = _objects.OfType<BlDelay>().OrderBy(i => i.Id).ToArray();

            if (_link != null)
            {
                if (tone.Length > 0)
                    _objects.AddRange(
                        tone.Select(q => new LineViewModel(q, _link) { Id = q.Id }).OrderBy(n => n.Id).ToArray());
                _objects.AddRange(
                    delay
                        .Select(q => new LinkLineVm(_link, q, _link, q.Id))
                        .ToArray());
                _objects.AddRange(_objects.OfType<BlSpeakerPeq>()
                    .Where(i => i.Id % 12 == 1 || i.Id % 12 == 2)
                    .OrderBy(i => i.Id)
                    .ToArray()
                    .Join(delay, peq => peq.Id, blDelay => blDelay.Id, (peq, blDelay) =>
                        new LineViewModel(peq, blDelay) { LineType = LineType.Emergency, Id = blDelay.Id }));
            }

            var input = _objects.OfType<BlInputName>().OrderBy(n => n.Id).Where(i => i.Id % 12 > 3).ToArray();
            if (input.Length > 0 && _link != null)
                _objects.AddRange(input.Select(q => new LineViewModel(q, _link) { LineType = LineType.PublicAddress }));

            var outputs = _objects.OfType<BlAmplifier>()
                .Join(_objects.OfType<BlOutput>(), s => s.Id, q => q.Id,
                    (amplifier, blOutput) => new LineViewModel(amplifier, blOutput)).ToArray();

            _objects.AddRange(outputs);

            _objects.AddRange(
                _objects.OfType<BlMonitor>()
                    .Join(_objects.OfType<BlSpeakerPeq>(), s => s.Id, q => q.Id, (b, c) => new LineViewModel(b, c))
                    .ToArray());

            var speakerMatix = _objects.OfType<BlAmplifier>()
                .Join(_objects.OfType<BlSpMatrix>(), s => s.Id % 12 / 4, q => q.Id, (b, c) => new LineViewModel(b, c))
                .ToArray();
            _objects.AddRange(speakerMatix);

            AddLineLinks();

            AddSpeakerLines();



            _objects.AddRange(_objects.OfType<BlSpeakerPeq>().Where(q => q.Id % 12 != 1 && q.Id % 12 != 2)
                .Select(sp => new LinkLineVm(_link, sp, _link, sp.Id)).ToArray());
        }

        private void AddLineLinks()
        {
            if (_link != null)
                _objects.AddRange(
                    _objects.OfType<BlAuxSpeakerPeq>()
                        .OrderBy(q => q.Id)
                        .Select(n => new LineViewModel(n, _link) { Id = n.Id })
                        .ToArray());

            _objects.AddRange(
                _objects.OfType<BlAuxSpeakerPeq>().OrderBy(n => n.Id)
                    .Join(_objects.OfType<BlAuxiliary>(), peq => peq.Id, auxiliary => auxiliary.Id,
                        (peq, auxiliary) => new LineViewModel(peq, auxiliary))
                    .ToArray());

            _objects.AddRange(
                _objects.OfType<BlMonitor>()
                    .Join(_objects.OfType<BlOutput>(), s => s.Id, q => q.Id, (b, c) => new LineViewModel(b, c))
                    .ToArray());
        }

        private void AddSpeakerLines()
        {
            var spMatrix = _objects.OfType<BlSpMatrix>().OrderBy(q => q.Id).ToArray();

            var speakers = new List<BlSpeaker>[3];

            speakers[0] = _objects.OfType<BlSpeaker>().Where(i => i.Id % 12 < 4).ToList();
            speakers[1] = _objects.OfType<BlSpeaker>().Where(i => i.Id % 12 > 3 && i.Id % 12 < 8).ToList();
            speakers[2] = _objects.OfType<BlSpeaker>().Where(i => i.Id % 12 > 7).ToList();

            if (spMatrix.Length > 0)
                for (var i = 0; i < 3; i++)
                {
                    if (speakers[i].Count > 0)
                        _objects.AddRange(speakers[i].Select(n => new LineViewModel(n, spMatrix[0])));
                }
        }


    }
}