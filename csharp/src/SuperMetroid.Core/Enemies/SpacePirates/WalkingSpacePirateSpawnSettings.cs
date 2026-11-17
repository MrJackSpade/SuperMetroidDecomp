namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a walking space pirate patrols (#1472, #1519, #2278).</summary>
/// <param name="Facing">The way it faces and walks first.</param>
/// <param name="LaserSpeed">How fast its lasers fly.</param>
/// <param name="PatrolReach">The pixels it patrols either side of its placement.</param>
internal sealed record WalkingSpacePirateSpawnSettings(HorizontalDirection Facing, PirateLaserSpeed LaserSpeed, int PatrolReach) : EnemySpawnSettings
{
    /// <summary>
    /// Whether the pirate flinches from Samus's shots: a pirate placed with the slow laser also looks for her shots after
    /// its normal AI, as the game's placement names both with one choice.
    /// </summary>
    internal bool FlinchesFromShots => LaserSpeed == PirateLaserSpeed.Slow;
}
