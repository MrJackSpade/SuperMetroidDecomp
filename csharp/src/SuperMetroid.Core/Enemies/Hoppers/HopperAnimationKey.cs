namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The kind, surface and phase that select a hopper's program.</summary>
/// <param name="Kind">The kind of hopper.</param>
/// <param name="Surface">The surface.</param>
/// <param name="Phase">The phase.</param>
internal readonly record struct HopperAnimationKey(HopperKind Kind, HopperSurface Surface, HopperPhase Phase);
