namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a wall space pirate jumps (#1472, #1519, #2278).</summary>
/// <param name="Wall">The wall it starts on.</param>
/// <param name="LaserSpeed">How fast its lasers fly.</param>
/// <param name="JumpSpan">The pixels between its walls.</param>
internal sealed record WallSpacePirateSpawnSettings(HorizontalDirection Wall, PirateLaserSpeed LaserSpeed, int JumpSpan) : EnemySpawnSettings
{
    /// <summary>
    /// Whether the pirate's jumps between its walls turn slowly: a pirate placed with the slow laser also jumps slowly, as
    /// the game's placement names both with one choice.
    /// </summary>
    internal bool JumpsSlowly => LaserSpeed == PirateLaserSpeed.Slow;
}
