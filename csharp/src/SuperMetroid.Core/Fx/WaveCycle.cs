namespace SuperMetroid.Core.Fx;

/// <summary>
/// Where a looping liquid wave stands in its cycle (#1987): the phase it shows and the updates it has shown it. The wave
/// holds each phase for its phase's length of updates, then shows the next, returning to the cycle's start after its last
/// phase.
/// </summary>
/// <remarks>
/// The wave keeps the updates it has shown its phase, not a countdown started with the phase's length (#2561): each update
/// compares them with the length its owner passes for that update, which follows the room's liquid as it stands then (a
/// lava or acid haze's length is its direction's), so a liquid that changes in the middle of a phase moves the phase on
/// at the new length, as the game's haze step does. A countdown or an <see cref="IntervalClock"/> fixes the length when
/// the phase starts and would not.
/// </remarks>
/// <param name="Phase">The phase of the cycle the wave shows.</param>
/// <param name="UpdatesShown">The updates the wave has shown its phase, zero before its first update.</param>
internal readonly record struct WaveCycle(int Phase, int UpdatesShown)
{
    /// <summary>A wave that starts at <paramref name="phase"/> and shows it for a whole phase from its first update.</summary>
    internal static WaveCycle StartingAt(int phase) => new(phase, 0);

    /// <summary>
    /// The wave one update on: it shows its phase for one more update, or, once it has shown it for
    /// <paramref name="phaseLength"/> updates, shows the next of the cycle's <paramref name="phases"/> for its first.
    /// </summary>
    /// <param name="phaseLength">The updates the wave shows each phase, at least one.</param>
    /// <param name="phases">The phases in one cycle, at least one.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6464:Wait stepped by hand",
        Justification = "The wave compares the updates it has shown its phase with the length its owner passes on each update, which follows the room's liquid as it stands then; a countdown or IntervalClock fixes the length when the phase starts and would move a haze whose liquid changes mid-phase on at the old length.")]
    internal WaveCycle Advanced(int phaseLength, int phases) =>
        UpdatesShown < Ensure.BetweenInclusive(1, int.MaxValue, phaseLength)
            ? this with { UpdatesShown = UpdatesShown + 1 }
            : new((Phase + 1) % Ensure.BetweenInclusive(1, int.MaxValue, phases), 1);
}
