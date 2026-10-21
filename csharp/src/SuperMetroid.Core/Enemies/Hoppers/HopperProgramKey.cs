
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The variant, surface and phase that select a hopper's program.</summary>
/// <param name="Variant">The variant.</param>
/// <param name="Surface">The surface.</param>
/// <param name="Phase">The phase.</param>
internal readonly record struct HopperProgramKey(int Variant, HopperSurface Surface, HopperPhase Phase);
