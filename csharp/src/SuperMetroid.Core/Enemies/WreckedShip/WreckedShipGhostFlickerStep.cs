namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One step of a Wrecked Ship ghost's flicker as it appears.</summary>
/// <param name="Duration">The updates the step lasts before the next one starts.</param>
/// <param name="Visibility">What starting the step does to the ghost's visibility.</param>
public readonly record struct WreckedShipGhostFlickerStep(int Duration, WreckedShipGhostFlickerVisibility Visibility);
