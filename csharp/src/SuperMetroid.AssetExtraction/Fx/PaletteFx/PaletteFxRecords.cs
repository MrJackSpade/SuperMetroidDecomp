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
internal static class PaletteFxRecords
{
    /// <summary>The frame that shows each record, by the record's place: the frame of the animation whose run holds it.</summary>
    private static readonly ColourEffectFrame[] Frames = Number();

    /// <summary>The record each frame shows.</summary>
    private static readonly FrozenDictionary<ColourEffectFrame, PaletteFxRecord> ByFrame = Index();

    /// <summary>The frame that shows <paramref name="record"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No animation shows the record.</exception>
    internal static ColourEffectFrame FrameOf(PaletteFxRecord record) => (uint)record.Record < (uint)Frames.Length
        ? Frames[record.Record]
        : throw new ArgumentOutOfRangeException(nameof(record), record, $"The colour effects' animations show {Frames.Length} records.");

    /// <summary>The record <paramref name="frame"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not one its animation shows.</exception>
    internal static PaletteFxRecord Of(ColourEffectFrame frame) => ByFrame.TryGetValue(frame, out PaletteFxRecord record)
        ? record
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "The frame is not one its animation shows.");

    /// <summary>The number of frames <paramref name="animation"/> shows.</summary>
    internal static int FrameCount(ColourEffectAnimation animation) =>
        ColourEffectAnimations.Steps.Beats(animation).Count(beat => beat is AnimationBeat<int, ColourEffectCue>.Frame);

    /// <summary>
    /// Each run's frames in the runs' order, each run's in the order its animation shows them; every animation's frames are
    /// numbered from zero in the order its steps show them, and each lies in exactly one run unless it shows another
    /// animation's records.
    /// </summary>
    /// <exception cref="InvalidDataException">An animation numbers its frames out of order, or the runs miss or repeat a frame.</exception>
    private static ColourEffectFrame[] Number()
    {
        foreach (ColourEffectAnimation animation in Enum.GetValues<ColourEffectAnimation>())
        {
            int next = 0;
            foreach (AnimationBeat<int, ColourEffectCue> beat in ColourEffectAnimations.Steps.Beats(animation))
            {
                if (beat is not AnimationBeat<int, ColourEffectCue>.Frame frame)
                    continue;
                if (frame.Shown != next)
                    throw new InvalidDataException($"Colour effect animation {animation} shows frame {frame.Shown} where its frame {next} belongs.");
                next++;
            }
        }
        var frames = new List<ColourEffectFrame>();
        foreach (ColourEffectRecordRun run in ColourEffectRecordOrder.Runs)
        {
            for (int frame = run.FirstFrame; frame < run.FirstFrame + run.FrameCount; frame++)
                frames.Add(new ColourEffectFrame(run.Animation, frame));
        }
        if (frames.Distinct().Count() != frames.Count)
            throw new InvalidDataException("The colour effects' record runs hold a frame twice.");
        foreach (ColourEffectAnimation animation in Enum.GetValues<ColourEffectAnimation>())
        {
            if (ColourEffectRecordOrder.ShowsRecordsOf.ContainsKey(animation))
                continue;
            for (int frame = 0; frame < FrameCount(animation); frame++)
            {
                if (!frames.Contains(new ColourEffectFrame(animation, frame)))
                    throw new InvalidDataException($"No record run holds frame {frame} of colour effect animation {animation}.");
            }
        }
        return [.. frames];
    }

    /// <summary>
    /// The record each frame shows, from <see cref="Frames"/>: an animation that shows another's records shows, at each of
    /// its frames, the record of that animation's frame of the same number.
    /// </summary>
    /// <exception cref="InvalidDataException">An animation shows another's records but shows another number of frames.</exception>
    private static FrozenDictionary<ColourEffectFrame, PaletteFxRecord> Index()
    {
        var byFrame = new Dictionary<ColourEffectFrame, PaletteFxRecord>(Frames.Length);
        for (int record = 0; record < Frames.Length; record++)
            byFrame.Add(Frames[record], new PaletteFxRecord(record));
        foreach ((ColourEffectAnimation animation, ColourEffectAnimation shown) in ColourEffectRecordOrder.ShowsRecordsOf)
        {
            if (FrameCount(animation) != FrameCount(shown))
                throw new InvalidDataException($"Colour effect animation {animation} shows {shown}'s records but not its number of frames.");
            for (int frame = 0; frame < FrameCount(animation); frame++)
                byFrame.Add(new ColourEffectFrame(animation, frame), byFrame[new ColourEffectFrame(shown, frame)]);
        }
        return byFrame.ToFrozenDictionary();
    }
}
