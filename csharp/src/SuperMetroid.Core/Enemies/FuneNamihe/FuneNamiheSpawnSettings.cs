namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>What a Fune's and a Namihe's spawn settings both say (#2476): the way it faces and how fast its fireballs fly.</summary>
/// <param name="Facing">The way it faces, which selects its animations (#2277).</param>
/// <param name="LaunchSpeed">The speed, in pixels per update, at which its fireballs fly away from it.</param>
internal abstract record FuneNamiheSpawnSettings(HorizontalDirection Facing, SignedEightEight LaunchSpeed) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
