// MidiAnalyzer — analyzes a MIDI file as a 1D cellular automaton.
//
// Usage: MidiAnalyzer [--input <file.mid>] [--output <out.mid>]
//                     [--track N] [--channel N] [--dt-mul N]
//                     [--mode full|chroma] [--sim-steps N]
//
// Modes:
//   full   128 cells, one per MIDI note 0–127 (default, original behaviour)
//   chroma  12 cells, one per chromatic pitch class (C=0 … B=11)
//           Maps each note to note%12; any octave of the pitch activates the cell.
//           Reduces config space from 2^17 to 2^5 at default radius → much less ambiguous rules.

namespace MidiAnalyzer;

using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Simulation;
using Analysis;
using Visualization;

using SimState = Simulation.Container.Array<Simulation.State>;

class MidiAnalyzerProgram
{
    // ── MIDI ↔ SimState conversion ──────────────────────────────────────────

    static List<SimState> MidiToSimStates(MidiFile file, int track, int channel, int dtMul)
    {
        var timeDivision = (TicksPerQuarterNoteTimeDivision)file.TimeDivision;
        var offState = new State(1, 0);
        var onState  = new State(1, 1);

        var sequence = new List<SimState>();
        var state = new SimState(128, offState);

        var trackChunk = file.Chunks.OfType<TrackChunk>().Skip(track).First();
        foreach (var ev in trackChunk.Events.OfType<NoteEvent>().Where(e => e.Channel == channel))
        {
            long eighthNotes = ev.DeltaTime * dtMul / timeDivision.TicksPerQuarterNote;
            for (long i = 0; i < eighthNotes; i++)
            {
                sequence.Add(state);
                state = (SimState)state.Clone();
            }

            var cellState = ev.EventType switch
            {
                MidiEventType.NoteOn  => onState,
                MidiEventType.NoteOff => offState,
                _                     => state.Get(ev.NoteNumber)
            };
            state.Set(ev.NoteNumber, cellState);
        }
        sequence.Add(state);
        return sequence;
    }

    // Reduce 128-cell note representation to 12 chromatic pitch classes.
    static List<SimState> ToChromaStates(List<SimState> fullStates)
    {
        return fullStates.Select(s => {
            var chroma = new SimState(12, new State(1, 0));
            for (int note = 0; note < 128; note++)
                if (s.Get(note).Value == 1)
                    chroma.Set(note % 12, new State(1, 1));
            return chroma;
        }).ToList();
    }

    static void ReplaceMidiEvents(MidiFile file, int track, int channel,
                                   int dtMul, List<SimState> states)
    {
        int getNoteLength(int idx, int offset)
        {
            int len = 0;
            for (int i = offset; i < states.Count; i++)
            {
                if (states[i].Get(idx).Value == 0) break;
                len++;
            }
            return len;
        }

        var timeDivision = (TicksPerQuarterNoteTimeDivision)file.TimeDivision;
        var delta = timeDivision.TicksPerQuarterNote / dtMul;

        var trackChunk = file.Chunks.OfType<TrackChunk>().Skip(track).First();
        var noteManager = trackChunk.ManageNotes();
        noteManager.Objects.RemoveAll(n => n.Channel == channel);

        var previous = new SimState(128, new State(2));
        for (int offset = 0; offset < states.Count; offset++)
        {
            var state = states[offset];
            // For chroma mode (12 cells), expand back to 128 using middle octave (C4=60)
            int cells = state.CellCount;
            for (int idx = 0; idx < cells; idx++)
            {
                int midiNote = cells == 12 ? idx + 60 : idx; // chroma → middle octave
                if (previous.Get(midiNote % cells == midiNote ? midiNote : idx).Value == 0
                    && state.Get(idx).Value == 1)
                {
                    var dur  = getNoteLength(idx, offset);
                    var note = new Note(new SevenBitNumber((byte)midiNote),
                                       delta * dur, delta * offset);
                    noteManager.Objects.Add(note);
                }
            }
            // rebuild "previous" for next iteration at full 128-cell resolution
            var prev128 = new SimState(128, new State(2));
            for (int idx = 0; idx < cells; idx++)
                prev128.Set(cells == 12 ? idx + 60 : idx, state.Get(idx));
            previous = prev128;
        }
        noteManager.SaveChanges();
    }

    // ── Metrics ──────────────────────────────────────────────────────────────

    static int CountNoteOnEvents(List<SimState> states)
    {
        int count = 0;
        for (int t = 1; t < states.Count; t++)
            for (int c = 0; c < states[t].CellCount; c++)
                if (states[t - 1].Get(c).Value == 0 && states[t].Get(c).Value == 1)
                    count++;
        return count;
    }

    static int CountActiveCells(List<SimState> states)
    {
        var active = new System.Collections.Generic.HashSet<int>();
        foreach (var s in states)
            for (int c = 0; c < s.CellCount; c++)
                if (s.Get(c).Value == 1) active.Add(c);
        return active.Count;
    }

    // ── Main analysis ─────────────────────────────────────────────────────────

    static void Main(string[] args)
    {
        string inputFile  = Path.Combine(AppContext.BaseDirectory, "test.mid");
        string outputFile = "out.mid";
        string outputDir  = ".";
        int track   = 3;
        int channel = 4;
        int dtMul   = 2;
        int simSteps = 200;
        string mode = "full";

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input"      when i + 1 < args.Length: inputFile  = args[++i]; break;
                case "--output"     when i + 1 < args.Length: outputFile = args[++i]; break;
                case "--output-dir" when i + 1 < args.Length: outputDir  = args[++i]; break;
                case "--track"      when i + 1 < args.Length: track      = int.Parse(args[++i]); break;
                case "--channel"    when i + 1 < args.Length: channel    = int.Parse(args[++i]); break;
                case "--dt-mul"     when i + 1 < args.Length: dtMul      = int.Parse(args[++i]); break;
                case "--sim-steps"  when i + 1 < args.Length: simSteps   = int.Parse(args[++i]); break;
                case "--mode"       when i + 1 < args.Length: mode       = args[++i]; break;
            }
        }

        Directory.CreateDirectory(outputDir);

        var midiFile  = MidiFile.Read(inputFile);
        var fullStates = MidiToSimStates(midiFile, track, channel, dtMul);

        List<SimState> simStates = mode == "chroma"
            ? ToChromaStates(fullStates)
            : fullStates;

        int cells = simStates[0].CellCount;
        Console.WriteLine($"=== MidiAnalyzer ===");
        Console.WriteLine($"Mode:    {mode}  ({cells} cells)");
        Console.WriteLine($"Frames:  {simStates.Count} time steps from MIDI");

        // ── Analysis ─────────────────────────────────────────────────────────

        var analyzerParams = new Analyzer1D.Params
        {
            StatesCount      = 2,
            StartingRadius   = mode == "chroma" ? (uint)2 : (uint)8,
            MaxRadius        = mode == "chroma" ? (uint)4 : (uint)16,
            VarianceThreshold = 1.0,
            LookBackAmount   = 1,
        };

        Console.WriteLine("\nAnalyzing...");
        var series = Analyzer1D.TimeSeries(simStates.ToArray(), analyzerParams);

        double varMin  = series.Min(r => r.AverageVariance());
        double varMean = series.Average(r => r.AverageVariance());
        double varMax  = series.Max(r => r.AverageVariance());
        int activeOrig = CountActiveCells(simStates);
        int noteOnOrig = CountNoteOnEvents(simStates);

        Console.WriteLine($"\n=== Analysis ===");
        Console.WriteLine($"Rules extracted:  {series.Length}");
        Console.WriteLine($"Rule variance:    min={varMin:F2}  mean={varMean:F2}  max={varMax:F2}");
        Console.WriteLine($"Active cells:     {activeOrig} / {cells} in original");
        Console.WriteLine($"Note-on events:   {noteOnOrig} in original");

        // ── Self-similarity matrix ────────────────────────────────────────────

        Console.WriteLine("\nComputing self-similarity matrix...");
        int n = series.Length;
        float[][] similarity = new float[n][];
        for (int i = 0; i < n; i++)
        {
            similarity[i] = new float[n];
            for (int j = 0; j < n; j++)
                similarity[i][j] = (float)series[i].AverageDifference(series[j]);
        }
        string simPath = Path.Combine(outputDir, "midi_similarity");
        ChartHelper.SaveHeatmap(similarity, simPath);
        Console.WriteLine($"Self-similarity matrix saved to {simPath}.png");

        // ── Simulation ───────────────────────────────────────────────────────

        Console.WriteLine("\nSimulating...");
        var space = new SimState(cells, new State(1, 0));
        var model = new Model<SimState>(space, 80);
        var rule  = new CyclicRule(series);

        // Start from the first non-empty frame
        var startState = simStates.First(s => {
            for (int c = 0; c < s.CellCount; c++) if (s.Get(c).Value != 0) return true;
            return false;
        });
        model.ResetState(startState);
        model.Rule = rule;

        var simView = new List<SimState> { model.CurrentState };
        for (int i = 0; i < simSteps; i++)
        {
            model.Advance();  // Rule.Advance() is called internally — do NOT call rule.Advance() again
            simView.Add(model.CurrentState);
        }

        int activeGen  = CountActiveCells(simView);
        int noteOnGen  = CountNoteOnEvents(simView);
        Console.WriteLine($"\n=== Simulation ===");
        Console.WriteLine($"Active cells:   {activeGen} / {cells} in generated");
        Console.WriteLine($"Note-on events: {noteOnGen} in generated ({(noteOnOrig > 0 ? (100.0 * noteOnGen / noteOnOrig):0):F0}% of original)");

        // ── Write output MIDI ─────────────────────────────────────────────────

        ReplaceMidiEvents(midiFile, track, channel, dtMul, simView);
        midiFile.Write(outputFile, overwriteFile: true);
        Console.WriteLine($"\nOutput written to {outputFile}");
    }
}
