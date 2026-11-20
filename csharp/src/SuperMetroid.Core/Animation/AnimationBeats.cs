namespace SuperMetroid.Core.Animation;

/// <summary>
/// Beats built from what a run of them shares, so a shared duration or a run played back the other way is stated once
/// (#2557): a catalog names its frames and the duration once and its segments reference the run.
/// </summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal static class AnimationBeats<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Each of <paramref name="frames"/> in turn, every one shown for <paramref name="duration"/> updates.</summary>
    /// <param name="duration">The updates each frame shows for.</param>
    /// <param name="frames">The frames, in the order they show.</param>
    internal static AnimationBeat<TFrame, TEvent>[] Steady(int duration, params ReadOnlySpan<TFrame> frames)
    {
        var beats = new AnimationBeat<TFrame, TEvent>[frames.Length];
        for (int index = 0; index < frames.Length; index++)
            beats[index] = new AnimationBeat<TFrame, TEvent>.Frame(frames[index], duration);
        return beats;
    }

    /// <summary><paramref name="beats"/> played the other way round: the last beat first.</summary>
    /// <param name="beats">The beats, in the order they play forwards.</param>
    internal static AnimationBeat<TFrame, TEvent>[] Backwards(IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats) =>
        [.. Enumerable.Reverse(Ensure.NotNull(beats))];
}
