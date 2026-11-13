namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A Samus pose's animation (#1473): runs of frames, each frame a picture held for its delay or the cue
/// that ends its run. Samus's animation frame is the number of the step it is on; frame numbers are the pose's artwork
/// frame numbers, so a number between two runs names no step, and reaching one fails.
/// </summary>
internal sealed class SamusPoseAnimation
{
    /// <summary>The step at each frame number, or none for a number between two runs.</summary>
    private readonly SamusPoseAnimationStep?[] steps;

    /// <summary>Lays out <paramref name="sequences"/>, each starting after the previous one ends.</summary>
    /// <exception cref="ArgumentException">A run starts before the previous one ends, or there is none.</exception>
    internal SamusPoseAnimation(IReadOnlyList<SamusAnimationSequence> sequences)
    {
        Ensure.NotNull(sequences);
        if (sequences.Count == 0)
            throw new ArgumentException("An animation program holds at least one run of frames.", nameof(sequences));
        SamusAnimationSequence last = sequences[^1];
        steps = new SamusPoseAnimationStep?[last.FirstFrame + last.Delays.Count + 1];
        int nextFree = 0;
        foreach (SamusAnimationSequence sequence in sequences)
        {
            if (sequence.FirstFrame < nextFree)
                throw new ArgumentException($"A run starting at frame {sequence.FirstFrame} overlaps the run before it.", nameof(sequences));
            for (int index = 0; index < sequence.Delays.Count; index++)
                steps[sequence.FirstFrame + index] = new SamusAnimationDelay(sequence.Delays[index]);
            nextFree = sequence.FirstFrame + sequence.Delays.Count;
            steps[nextFree++] = sequence.End;
        }
    }

    /// <summary>How many frame numbers the animation spans, its last step's included.</summary>
    internal int FrameCount => steps.Length;

    /// <summary>The step at frame <paramref name="frame"/>, or null when no step of the animation has that number.</summary>
    internal SamusPoseAnimationStep? TryStepAt(int frame) => (uint)frame < (uint)steps.Length ? steps[frame] : null;
}
