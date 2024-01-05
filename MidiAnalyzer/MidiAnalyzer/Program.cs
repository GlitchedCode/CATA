namespace MidiAnalyzer;

using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Simulation;

using SimState = Simulation.Container.Array<Simulation.State>;

class Program
{
    static void ReplaceMidiEvents(MidiFile file, int track, int channel, int dtMul, List<SimState.View> states)
    {
        int getNoteLength(int idx, int offset)
        {
            var ret = 0;
            for(int i = offset; i < states.Count; i++)
            {
                if(states[i].Get(idx).Value == 0) break;
                ret++;
            }
            return ret;
        }

        var tempoMap = file.GetTempoMap();
        var timeDivision = (TicksPerQuarterNoteTimeDivision)file.TimeDivision;
        var delta = (timeDivision.TicksPerQuarterNote / dtMul);

        var trackChunk = file.Chunks.OfType<TrackChunk>().Skip(track).First();

        var noteManager = trackChunk.ManageNotes();
        noteManager.Objects.RemoveAll(n => n.Channel == channel);
        var previous = new SimState(128, new State(2)).GetView();
        for(int offset = 0; offset < states.Count; offset++)
        {
            var state = states[offset];
            for (int idx = 0; idx < 128; idx++)
                if (previous.Get(idx).Value == 0 && state.Get(idx).Value == 1)
                {
                    var turnDuration = getNoteLength(idx, offset);
                    var timeDuration =  delta * turnDuration;
                    var timeOffset = delta * offset;
                    var note = new Note(new SevenBitNumber((byte)idx), timeDuration, timeOffset);
                    noteManager.Objects.Add(note);
                }
            previous = state;
        }

        noteManager.SaveChanges();
    }

    static List<SimState> MidiFileToSimStates(MidiFile file, int track, int channel, int dtMul = 1)
    {
        var timeDivision = (TicksPerQuarterNoteTimeDivision)file.TimeDivision;
        //Console.WriteLine($"tpqn: {timeDivision.TicksPerQuarterNote}");

        var offState = new State(1, 0);
        var onState = new State(1, 1);

        List<SimState> sequence = new();
        var state = new SimState(128, offState);

        var trackChunk = file.Chunks.OfType<TrackChunk>().Skip(track).First();

        foreach (var ev in trackChunk.Events.OfType<NoteEvent>().Where((e) => e.Channel == channel))
        {
            //Console.WriteLine($"note event at delta {ev.DeltaTime}");
            long eighthNotes = ev.DeltaTime * dtMul / timeDivision.TicksPerQuarterNote;
            for (long i = 0; i < eighthNotes; i++)
            {
                sequence.Add(state);
                state = (SimState)state.Clone();
            }
            var note = ev.NoteNumber;

            State cellState;
            switch (ev.EventType)
            {
                case MidiEventType.NoteOn:
                    cellState = onState;
                    break;
                case MidiEventType.NoteOff:
                    cellState = offState;
                    break;
                default:
                    cellState = state.Get(note);
                    break;
            }
            state.Set(note, cellState);
        }
        sequence.Add(state);

        return sequence;
    }

    static void MidiAnalyze()
    {
        var midiFile = MidiFile.Read("test.mid");

        var simStates = MidiFileToSimStates(midiFile, 3, 4, 2);

        Console.WriteLine("original");
        SimState.PrintMany(simStates.Select(s => s.GetView()), ".O");

        var analyzerParams = new Analyzer1D.Params();
        var model = new Model1D(128, 80);
        var simView = new List<SimState.View>();
        foreach (var s in simStates) simView.Add(s.GetView());

        Console.WriteLine("Analyzing...");
        var series = Analyzer1D.TimeSeries(simView.ToArray(), analyzerParams);
        var rule = new CyclicRule(series);
        // var rule = Analyzer1D.SingleRule(simView.ToArray(), analyzerParams);

        Console.WriteLine("Simulating...");
        var state = simStates.First(s =>
        {
            var ret = false;
            for (int i = 0; i < s.CellCount; i++) ret |= s.Get(i).Value != 0;
            return ret;
        });
        model.ResetState(state.GetView());

        //model.ResetHistory(simStates.Take(20).Select(s => s.GetView()));
        //model.Randomize();
        simView.Clear();
        simView.Add(model.GetCurrentStateView());

        model.Rule = rule;
        for (int i = 0; i < 200; i++)
        {
            model.Advance();
            rule.Advance();
            simView.Add(model.GetCurrentStateView());
        }


        Console.WriteLine("recreated");
        SimState.PrintMany(simView, ".O");

        // foreach(var config in rule.EnumerateConfigurations())
        // {
        //     Console.WriteLine(String.Join(" ", config.Select((s) => s.Value)));
        //     Console.WriteLine(String.Join(" ", rule.Distribution(config)));
        // }

        ReplaceMidiEvents(midiFile, 3, 4, 2, simView);
        midiFile.Write("out.mid", overwriteFile: true);
    }
    
    static void Main(string[] args)
    {
        MidiAnalyze();
    }
}
