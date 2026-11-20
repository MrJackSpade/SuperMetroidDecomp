namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// One step of a Wrecked Ship ghost's flicker as it appears (#2451), in the order the ghost takes them: each carries how
/// long it lasts before the next starts, what starting it does to the ghost's visibility, and the step after it (#2512).
/// The flicker ends when the last step does.
/// </summary>
public sealed class WreckedShipGhostFlickerStep
{
    /// <summary>The step of <paramref name="timing"/>, followed by <paramref name="next"/>.</summary>
    private WreckedShipGhostFlickerStep(WreckedShipGhostFlickerTiming timing, WreckedShipGhostFlickerStep? next)
    {
        Duration = timing.Duration;
        Visibility = timing.Visibility;
        Next = next;
    }

    /// <summary>
    /// The flicker's first step, which each appearance starts the flicker at; the steps after it follow
    /// <see cref="WreckedShipGhostAppearanceDefinitions.FlickerTimings"/> in order.
    /// </summary>
    public static WreckedShipGhostFlickerStep First { get; } = Schedule(WreckedShipGhostAppearanceDefinitions.FlickerTimings);

    /// <summary>The updates the step lasts before the next one starts.</summary>
    public int Duration { get; }

    /// <summary>What starting the step does to the ghost's visibility.</summary>
    public WreckedShipGhostFlickerVisibility Visibility { get; }

    /// <summary>The step after this one; none after the last, whose end ends the flicker.</summary>
    public WreckedShipGhostFlickerStep? Next { get; }

    /// <summary>The first of the steps of <paramref name="timings"/>, each followed by the next.</summary>
    /// <exception cref="InvalidDataException">The flicker has no step.</exception>
    private static WreckedShipGhostFlickerStep Schedule(IReadOnlyList<WreckedShipGhostFlickerTiming> timings)
    {
        if (timings.Count == 0)
            throw new InvalidDataException("The ghost's flicker has no step.");
        var first = new WreckedShipGhostFlickerStep(timings[^1], null);
        for (int step = timings.Count - 2; step >= 0; step--)
            first = new(timings[step], first);
        return first;
    }
}
