using SuperMetroid.Core.Animation;

using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour family as its presentation document lays it out: consecutive timed records from
/// <paramref name="FirstRecord"/>, each an animation frame of <paramref name="ColorsPerFrame"/> written colours.
/// </summary>
/// <param name="FirstRecord">The record shown as the family's first frame.</param>
/// <param name="FrameCount">The family's frames.</param>
/// <param name="ColorsPerFrame">The colours each frame writes.</param>
public readonly record struct PaletteFxColorFrames(int FirstRecord, int FrameCount, int ColorsPerFrame)
{
    /// <summary>
    /// The frames <paramref name="program"/> shows: the consecutive records of its segment's frames and, where its list runs
    /// on into the list authored next (<see cref="PaletteFxListLayout.RunOnIntoNext"/>), of each list it runs on into.
    /// </summary>
    /// <param name="program">The segment whose first frame is the family's first frame.</param>
    /// <param name="colorsPerFrame">The colours each frame writes.</param>
    public static PaletteFxColorFrames Of(PaletteFxSegment program, int colorsPerFrame)
    {
        int first = -1, count = 0;
        PaletteFxSegment[] authored = Enum.GetValues<PaletteFxSegment>();
        for (PaletteFxSegment running = program; ; running = authored[Array.IndexOf(authored, running) + 1])
        {
            int frame = 0;
            foreach (AnimationBeat<int, PaletteFxCue> beat in PaletteFxSegments.All.Beats(running))
            {
                if (beat is not AnimationBeat<int, PaletteFxCue>.Frame)
                    continue;
                int record = PaletteFxRecord.Of(new PaletteFxFrame(running, frame++)).Record;
                if (first < 0)
                {
                    first = record;
                    count = 1;
                }
                else if (record == first + count)
                    count++;
                else
                    throw new InvalidOperationException($"Palette-FX segment {program} shows {new PaletteFxFrame(running, frame - 1)} out of order.");
            }
            if (!PaletteFxListLayout.RunOnIntoNext.Contains(running))
                return Found(program, first, count, colorsPerFrame);
        }
    }

    /// <summary>The frames found, rejecting a program that shows nothing.</summary>
    private static PaletteFxColorFrames Found(PaletteFxSegment program, int first, int count, int colorsPerFrame) =>
        first < 0
            ? throw new InvalidOperationException($"Palette-FX program {program} shows no record.")
            : new(first, count, colorsPerFrame);

    /// <summary>The record shown as frame <paramref name="frame"/>.</summary>
    public PaletteFxRecord Frame(int frame) => (uint)frame < FrameCount
        ? new(FirstRecord + frame)
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>Colour <paramref name="color"/> of frame <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => (uint)color < ColorsPerFrame
        ? new(Frame(frame), color)
        : throw new ArgumentOutOfRangeException(nameof(color));
}
