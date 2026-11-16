
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The wall a space pirate clings to and the direction it climbs.</summary>
/// <param name="OnRightWall">Whether the pirate clings to a wall on its right rather than its left.</param>
/// <param name="Direction">Whether it climbs up or down the wall.</param>
internal readonly record struct WallSpacePirateClimb(bool OnRightWall, WallSpacePirateClimbDirection Direction);
