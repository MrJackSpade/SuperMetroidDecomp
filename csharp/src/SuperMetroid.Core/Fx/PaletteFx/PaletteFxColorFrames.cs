using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

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
    /// The frames <paramref name="program"/> shows, read from its steps: the consecutive records it shows, through cues,
    /// counters and counted repeats and on into each program it falls into, until it jumps, holds or stops.
    /// </summary>
    /// <param name="program">The program whose first show is the family's first frame.</param>
    /// <param name="colorsPerFrame">The colours each frame writes.</param>
    public static PaletteFxColorFrames Of(PaletteFxProgram program, int colorsPerFrame)
    {
        int first = -1, count = 0;
        PaletteFxProgram? current = program;
        while (current is { } running)
        {
            current = null;
            foreach (AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step in PaletteFxAnimations.Programs.Steps(running))
            {
                switch (step)
                {
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show when first < 0:
                        first = show.Frame.Record;
                        count = 1;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show when show.Frame.Record == first + count:
                        count++;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep show:
                        throw new InvalidOperationException($"Palette-FX program {program} shows record {show.Frame.Record} out of order.");
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallIntoStep fallInto:
                        current = fallInto.Target;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.WaitStep or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.SetCounterStep or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCountingStep:
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoToStep or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.HoldStep or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.StopStep:
                        return Found(program, first, count, colorsPerFrame);
                    default:
                        throw new InvalidOperationException($"Undefined animation program step {step}.");
                }
            }
        }
        return Found(program, first, count, colorsPerFrame);
    }

    /// <summary>The frames found, rejecting a program that shows nothing.</summary>
    private static PaletteFxColorFrames Found(PaletteFxProgram program, int first, int count, int colorsPerFrame) =>
        first < 0
            ? throw new InvalidOperationException($"Palette-FX program {program} shows no record.")
            : new(first, count, colorsPerFrame);

    /// <summary>The record shown as frame <paramref name="frame"/>.</summary>
    public PaletteFxFrame Frame(int frame) => (uint)frame < FrameCount
        ? new(FirstRecord + frame)
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>Colour <paramref name="color"/> of frame <paramref name="frame"/>.</summary>
    public PaletteFxColorKey Color(int frame, int color) => (uint)color < ColorsPerFrame
        ? new(Frame(frame), color)
        : throw new ArgumentOutOfRangeException(nameof(color));

    /// <summary>The frame and colour of <paramref name="key"/> when it belongs to this family.</summary>
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
