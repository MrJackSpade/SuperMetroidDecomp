using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour family as its presentation document lays it out: consecutive timed records from
/// <paramref name="FirstRecord"/>, each a row of <paramref name="ColorsPerFrame"/> written colours.
/// </summary>
/// <param name="FirstRecord">The record shown as the family's first row.</param>
/// <param name="FrameCount">The family's rows.</param>
/// <param name="ColorsPerFrame">The colours each row writes.</param>
public readonly record struct PaletteFxColorRows(int FirstRecord, int FrameCount, int ColorsPerFrame)
{
    /// <summary>
    /// The rows <paramref name="program"/> shows, read from its steps: the consecutive records it shows, through cues,
    /// counters and counted repeats and on into each program it falls into, until it jumps, holds or stops.
    /// </summary>
    /// <param name="program">The program whose first show is the family's first row.</param>
    /// <param name="colorsPerFrame">The colours each row writes.</param>
    public static PaletteFxColorRows Of(PaletteFxProgram program, int colorsPerFrame)
    {
        int first = -1, count = 0;
        PaletteFxProgram? current = program;
        while (current is { } running)
        {
            current = null;
            foreach (AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step in PaletteFxAnimations.Programs.Steps(running))
            {
                switch (step.Kind)
                {
                    case AnimationProgramStepKind.Show when first < 0:
                        first = step.Frame.Record;
                        count = 1;
                        break;
                    case AnimationProgramStepKind.Show when step.Frame.Record == first + count:
                        count++;
                        break;
                    case AnimationProgramStepKind.Show:
                        throw new InvalidOperationException($"Palette-FX program {program} shows record {step.Frame.Record} out of order.");
                    case AnimationProgramStepKind.FallInto:
                        current = step.Target;
                        break;
                    case AnimationProgramStepKind.Cue:
                    case AnimationProgramStepKind.Wait:
                    case AnimationProgramStepKind.SetCounter:
                    case AnimationProgramStepKind.RepeatWhileCounting:
                        break;
                    case AnimationProgramStepKind.GoTo:
                    case AnimationProgramStepKind.Hold:
                    case AnimationProgramStepKind.Stop:
                        return Rows(program, first, count, colorsPerFrame);
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(AnimationProgramStepKind)} {step.Kind}.");
                }
            }
        }
        return Rows(program, first, count, colorsPerFrame);
    }

    /// <summary>The rows found, rejecting a program that shows nothing.</summary>
    private static PaletteFxColorRows Rows(PaletteFxProgram program, int first, int count, int colorsPerFrame) =>
        first < 0
            ? throw new InvalidOperationException($"Palette-FX program {program} shows no record.")
            : new(first, count, colorsPerFrame);

    /// <summary>The record shown as row <paramref name="frame"/>.</summary>
    public PaletteFxFrame Frame(int frame) => (uint)frame < FrameCount
        ? new(FirstRecord + frame)
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>Colour <paramref name="color"/> of row <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => (uint)color < ColorsPerFrame
        ? new(Frame(frame), color)
        : throw new ArgumentOutOfRangeException(nameof(color));

    /// <summary>The row and colour of <paramref name="key"/> when it belongs to this family.</summary>
    public bool TryLocate(PaletteFxColorKey key, out int frame, out int color)
    {
        frame = key.Frame.Record - FirstRecord;
        color = key.Color;
        if ((uint)frame < FrameCount && (uint)color < ColorsPerFrame)
            return true;
        frame = color = 0;
        return false;
    }
}
