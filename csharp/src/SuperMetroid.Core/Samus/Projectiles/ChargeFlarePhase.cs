namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A picture of a charge-flare component's program: its place among the component's pictures, counted from the first.
/// The draw shows the picture of that place for the component and Samus's facing.
/// </summary>
/// <param name="Index">The place among the component's pictures, from zero.</param>
public readonly record struct ChargeFlarePhase(int Index);
