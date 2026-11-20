namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Which part of a Magdollite a placement is (#1472, #1519, #2023): its head, its rising body or its tracking overlay,
/// each its own settings (#2474), so only the head's carry how it rises; each builds its own part (#2554).
/// </summary>
internal abstract record MagdolliteSpawnSettings : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);

    /// <summary>Builds the part these settings name in <paramref name="place"/>.</summary>
    /// <param name="species">The Magdollite species.</param>
    /// <param name="place">The part's place.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="setup">How the part comes to be set up.</param>
    internal abstract IEnemyEntity Create(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup);
}
