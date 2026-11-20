namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a walking space pirate patrols (#1472, #1519, #2278, #2453).</summary>
/// <param name="Facing">The way it faces and walks first.</param>
/// <param name="LaserSpeed">How fast its lasers fly.</param>
/// <param name="ShotWatch">Whether it looks out for Samus's shots and flinches from them.</param>
/// <param name="PatrolReach">The pixels it patrols either side of its placement.</param>
internal sealed record WalkingSpacePirateSpawnSettings(
    HorizontalDirection Facing, PirateLaserSpeed LaserSpeed, WalkingSpacePirateShotWatch ShotWatch, int PatrolReach) : EnemySpawnSettings;
