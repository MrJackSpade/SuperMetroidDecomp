namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// One step of a Wrecked Ship ghost's flicker as it appears (#2451), in the order the ghost takes them: each lasts its
/// updates before the next starts, and starting it may show the ghost. The flicker ends when the last step does.
/// </summary>
public readonly record struct WreckedShipGhostFlickerStep
{
    /// <summary>The step at <paramref name="step"/> of the flicker.</summary>
    private WreckedShipGhostFlickerStep(int step) => Step = step;

    /// <summary>The step's place in the flicker, from zero.</summary>
    private int Step { get; }

    /// <summary>Every step of the flicker, in the order the ghost takes them.</summary>
    public static IReadOnlyList<WreckedShipGhostFlickerStep> All { get; } =
        [.. Enumerable.Range(0, WreckedShipGhostAppearanceDefinitions.FlickerTimings.Count).Select(step => new WreckedShipGhostFlickerStep(step))];

    /// <summary>The flicker's first step, which each appearance and each visible spell starts the flicker at.</summary>
    public static WreckedShipGhostFlickerStep First => All[0];

    /// <summary>The updates the step lasts before the next one starts.</summary>
    public int Duration => WreckedShipGhostAppearanceDefinitions.FlickerTimings[Step].Duration;

    /// <summary>What starting the step does to the ghost's visibility.</summary>
    public WreckedShipGhostFlickerVisibility Visibility => WreckedShipGhostAppearanceDefinitions.FlickerTimings[Step].Visibility;

    /// <summary>The step after this one; none after the last, whose end ends the flicker.</summary>
    public WreckedShipGhostFlickerStep? Next => Step + 1 < All.Count ? All[Step + 1] : null;
}
