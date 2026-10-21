
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The wall a space pirate clings to and the direction it climbs.</summary>
/// <param name="OnRightWall">The on right wall.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct WallSpacePirateClimb(bool OnRightWall, WallSpacePirateClimbDirection Direction);
