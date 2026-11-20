namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Whether a gunship bottom placement is the entrance pad (#1472).</summary>
/// <param name="Part">Which part of the gunship it is.</param>
internal sealed record GunshipSpawnSettings(GunshipBottomPiece Part) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
