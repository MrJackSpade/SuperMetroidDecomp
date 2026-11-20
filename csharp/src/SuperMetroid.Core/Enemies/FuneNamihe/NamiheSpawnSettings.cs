namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>How a Namihe faces, wakes and spits (#1472, #1519, #2063).</summary>
/// <param name="Facing">The way it faces.</param>
/// <param name="LaunchSpeed">The speed, in pixels per update, at which its fireballs fly away from it.</param>
/// <param name="YProximity">The vertical distance in pixels Samus must come within to wake it.</param>
internal sealed record NamiheSpawnSettings(HorizontalDirection Facing, SignedEightEight LaunchSpeed, int YProximity) : FuneNamiheSpawnSettings(Facing, LaunchSpeed)
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
