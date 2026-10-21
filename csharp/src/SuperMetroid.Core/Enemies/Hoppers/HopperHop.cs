
namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The surface a hopper stands on and the direction it hops.</summary>
/// <param name="Surface">The surface.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct HopperHop(HopperSurface Surface, HopperHopDirection Direction);
