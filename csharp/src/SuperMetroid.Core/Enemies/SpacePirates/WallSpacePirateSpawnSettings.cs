namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a wall space pirate jumps (#1472, #1519, #2278, #2453).</summary>
/// <param name="Wall">The wall it starts on.</param>
/// <param name="LaserSpeed">How fast its lasers fly.</param>
/// <param name="JumpSpeed">How fast its jumps between its walls turn.</param>
/// <param name="JumpSpan">The pixels between its walls.</param>
internal sealed record WallSpacePirateSpawnSettings(
    HorizontalDirection Wall, PirateLaserSpeed LaserSpeed, WallSpacePirateJumpSpeed JumpSpeed, int JumpSpan) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
