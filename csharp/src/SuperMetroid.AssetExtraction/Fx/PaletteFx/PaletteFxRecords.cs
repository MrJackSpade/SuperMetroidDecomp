using System.Collections.Frozen;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The import's map between the colour effects' frames and the cartridge's palette-effect records (#1598): the records
/// lie in the cartridge in the order the programs, in their authored order, show their frames, so the program set itself
/// numbers them. The palette-FX program checks compare each record's place with its cartridge address.
/// </summary>
internal static class PaletteFxRecords
{
    /// <summary>The frame that shows each record, by the record's place.</summary>
    private static readonly PaletteFxFrame[] Frames = Number();

    /// <summary>The record each frame shows.</summary>
    private static readonly FrozenDictionary<PaletteFxFrame, PaletteFxRecord> ByFrame = Index();

    /// <summary>The frame that shows <paramref name="record"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No program shows the record.</exception>
    internal static PaletteFxFrame FrameOf(PaletteFxRecord record) => (uint)record.Record < (uint)Frames.Length
        ? Frames[record.Record]
        : throw new ArgumentOutOfRangeException(nameof(record), record, $"The colour effects' programs show {Frames.Length} records.");

    /// <summary>The record <paramref name="frame"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not one its program shows.</exception>
    internal static PaletteFxRecord Of(PaletteFxFrame frame) => ByFrame.TryGetValue(frame, out PaletteFxRecord record)
        ? record
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The frame is not one its program shows.");

    /// <summary>
    /// Each program's frames in the programs' authored order, each program's in the order it shows them; each program
    /// numbers its own frames from zero in that order.
    /// </summary>
    /// <exception cref="InvalidDataException">A program numbers its frames out of order.</exception>
    private static PaletteFxFrame[] Number()
    {
        var frames = new List<PaletteFxFrame>();
        foreach (PaletteFxSegment program in Enum.GetValues<PaletteFxSegment>())
        {
            int next = 0;
            foreach (AnimationBeat<int, PaletteFxCue> beat in PaletteFxSegments.All.Beats(program))
            {
                if (beat is not AnimationBeat<int, PaletteFxCue>.Frame frame)
                    continue;
                if (frame.Shown != next)
                    throw new InvalidDataException($"Colour effect segment {program} shows frame {frame.Shown} where its frame {next} belongs.");
                frames.Add(new PaletteFxFrame(program, next));
                next++;
            }
        }
        return [.. frames];
    }

    /// <summary>The record each frame shows, from <see cref="Frames"/>.</summary>
    private static FrozenDictionary<PaletteFxFrame, PaletteFxRecord> Index()
    {
        var byFrame = new Dictionary<PaletteFxFrame, PaletteFxRecord>(Frames.Length);
        for (int record = 0; record < Frames.Length; record++)
            byFrame.Add(Frames[record], new PaletteFxRecord(record));
        return byFrame.ToFrozenDictionary();
    }
}
