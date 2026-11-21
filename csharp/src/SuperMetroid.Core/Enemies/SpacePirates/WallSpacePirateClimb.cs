
namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The wall a space pirate clings to and the direction it climbs.</summary>
/// <param name="Wall">The side the wall it clings to lies on.</param>
/// <param name="Direction">The way it climbs.</param>
internal readonly partial record struct WallSpacePirateClimb(HorizontalDirection Wall, WallSpacePirateClimbDirection Direction);
