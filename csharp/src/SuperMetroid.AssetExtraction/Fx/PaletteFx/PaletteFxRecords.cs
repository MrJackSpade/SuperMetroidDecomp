using System.Collections.Frozen;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The import's map between the colour effects' frames and the cartridge's palette-effect records (#1598, #1739): the
/// records lie in the cartridge in runs of consecutive frames of one animation (<see cref="ColourEffectRecordOrder"/>), so
/// those runs number them; an animation that shows another's frames shows its records. The palette-FX program checks
/// compare each record's place with its cartridge address.
/// </summary>
internal static partial class PaletteFxRecords
{
    /// <summary>
    /// Each record by its place in the cartridge's order: the frame of the animation whose run holds it, and the layout its
    /// run writes its colours in.
    /// </summary>
    private static readonly RunRecord[] Records = Number();

    /// <summary>The record each frame shows.</summary>
    private static readonly FrozenDictionary<ColourEffectFrame, PaletteFxRecord> ByFrame = Index();

    /// <summary>The frame that shows <paramref name="record"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No animation shows the record.</exception>
    internal static ColourEffectFrame FrameOf(PaletteFxRecord record) => Placed(record).Frame;

    /// <summary>How <paramref name="record"/> writes its colours: its run's layout.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No animation shows the record.</exception>
    internal static IReadOnlyList<PaletteFxRecordPart> Layout(PaletteFxRecord record) => Placed(record).Layout;

    /// <summary>The run record at <paramref name="record"/>'s place.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No animation shows the record.</exception>
    private static RunRecord Placed(PaletteFxRecord record) => (uint)record.Record < (uint)Records.Length
        ? Records[record.Record]
        : throw new ArgumentOutOfRangeException(nameof(record), record, $"The colour effects' animations show {Records.Length} records.");

    /// <summary>The record <paramref name="frame"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not one its animation shows.</exception>
    internal static PaletteFxRecord Of(ColourEffectFrame frame) => ByFrame.TryGetValue(frame, out PaletteFxRecord record)
        ? record
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The frame is not one its animation shows.");

    /// <summary>The number of frames <paramref name="effect"/> shows.</summary>
    internal static int FrameCount(ColourEffectId effect) =>
        ColourEffectShownFrames.Steps(effect).Count(beat => beat is AnimationBeat<int, ColourEffectCue>.Frame);

    /// <summary>
    /// Each run's frames in the runs' order, each run's in the order its animation shows them; every animation's frames are
    /// numbered from zero in the order its steps show them, and each lies in exactly one run unless it shows another
    /// animation's records.
    /// </summary>
    /// <exception cref="InvalidDataException">An animation numbers its frames out of order, or the runs miss or repeat a frame.</exception>
    private static RunRecord[] Number()
    {
        foreach (ColourEffectId animation in Enum.GetValues<ColourEffectId>())
        {
            int next = 0;
            foreach (AnimationBeat<int, ColourEffectCue> beat in ColourEffectShownFrames.Steps(animation))
            {
                if (beat is not AnimationBeat<int, ColourEffectCue>.Frame frame)
                    continue;
                if (frame.Shown != next)
                    throw new InvalidDataException($"Colour effect animation {animation} shows frame {frame.Shown} where its frame {next} belongs.");
                next++;
            }
        }
        var records = new List<RunRecord>();
        foreach (ColourEffectRecordRun run in ColourEffectRecordOrder.Runs)
        {
            for (int frame = run.FirstFrame; frame < run.FirstFrame + run.FrameCount; frame++)
                records.Add(new RunRecord(new ColourEffectFrame(run.Effect, frame), run.Layout));
        }
        HashSet<ColourEffectFrame> frames = [.. records.Select(record => record.Frame)];
        if (frames.Count != records.Count)
            throw new InvalidDataException("The colour effects' record runs hold a frame twice.");
        foreach (ColourEffectId animation in Enum.GetValues<ColourEffectId>())
        {
            if (ColourEffectRecordOrder.ShowsRecordsOf.ContainsKey(animation))
                continue;
            for (int frame = 0; frame < FrameCount(animation); frame++)
            {
                if (!frames.Contains(new ColourEffectFrame(animation, frame)))
                    throw new InvalidDataException($"No record run holds frame {frame} of colour effect animation {animation}.");
            }
        }
        return [.. records];
    }

    /// <summary>
    /// The record each frame shows, from <see cref="Records"/>: an animation that shows another's records shows, at each of
    /// its frames, the record of that animation's frame of the same number.
    /// </summary>
    /// <exception cref="InvalidDataException">An animation shows another's records but shows another number of frames.</exception>
    private static FrozenDictionary<ColourEffectFrame, PaletteFxRecord> Index()
    {
        var byFrame = new Dictionary<ColourEffectFrame, PaletteFxRecord>(Records.Length);
        for (int record = 0; record < Records.Length; record++)
            byFrame.Add(Records[record].Frame, new PaletteFxRecord(record));
        foreach ((ColourEffectId animation, ColourEffectId shown) in ColourEffectRecordOrder.ShowsRecordsOf)
        {
            if (FrameCount(animation) != FrameCount(shown))
                throw new InvalidDataException($"Colour effect animation {animation} shows {shown}'s records but not its number of frames.");
            for (int frame = 0; frame < FrameCount(animation); frame++)
                byFrame.Add(new ColourEffectFrame(animation, frame), byFrame[new ColourEffectFrame(shown, frame)]);
        }
        return byFrame.ToFrozenDictionary();
    }

    /// <summary>One record of a run: the frame that shows it and the layout its run writes its colours in.</summary>
    /// <param name="Frame">The frame of the animation whose run holds the record.</param>
    /// <param name="Layout">How the record writes its colours.</param>
    private readonly partial record struct RunRecord(ColourEffectFrame Frame, IReadOnlyList<PaletteFxRecordPart> Layout);
}
