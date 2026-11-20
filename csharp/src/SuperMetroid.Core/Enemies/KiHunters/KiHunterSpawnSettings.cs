namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>Whether a Ki-Hunter starts without its wings (#1472).</summary>
/// <param name="Wings">Whether it starts with wings; a wingless one's wing placement starts deleted.</param>
internal sealed record KiHunterSpawnSettings(KiHunterWings Wings) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);
}
