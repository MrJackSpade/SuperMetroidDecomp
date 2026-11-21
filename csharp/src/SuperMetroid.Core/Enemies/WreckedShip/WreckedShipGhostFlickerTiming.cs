namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The timing of one step of a Wrecked Ship ghost's flicker as it appears (#2451).</summary>
/// <param name="Duration">The updates the step lasts before the next one starts.</param>
/// <param name="Visibility">What starting the step does to the ghost's visibility.</param>
public readonly partial record struct WreckedShipGhostFlickerTiming(int Duration, WreckedShipGhostFlickerVisibility Visibility);
