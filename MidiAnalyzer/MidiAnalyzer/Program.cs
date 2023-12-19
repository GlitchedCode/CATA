namespace MidiAnalyzer;

using System.Collections.Generic;

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

using Simulation;

using SimState = Simulation.Container.Array<Simulation.State>;

class Program
{

    static List<SimState> MidiFileToSimStates(MidiFile file, int track, int channel, int dtMul = 1)
    {
        var timeDivision = (TicksPerQuarterNoteTimeDivision)file.TimeDivision;
        Console.WriteLine($"tpqn: {timeDivision.TicksPerQuarterNote}");

        var offState = new State(1, 0);
        var onState = new State(1, 1);

        List<SimState> sequence = new();
        var state = new SimState(128, offState);

        foreach (var trackChunk in file.Chunks.OfType<TrackChunk>())
        {
            foreach (var ev in trackChunk.Events.OfType<NoteEvent>().Where((e) => e.Channel == channel))
            {
                Console.WriteLine($"note event at delta {ev.DeltaTime}");
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

        }

        return sequence;
    }

    static void Main(string[] args)
    {
        var midiFile = MidiFile.Read("test.mid");

        SimState.PrintMany(MidiFileToSimStates(midiFile, 0, 4, 2), ".O");

        midiFile.Write("out.mid", overwriteFile: true);
    }
}
