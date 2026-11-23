namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' creation of an enemy built the same way wherever it is set up (#2554), from the settings its population
/// entry gives it.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
/// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
/// <param name="settings">The settings the occupant's population entry gives its species.</param>
/// <param name="place">The place, already holding the occupant and its live values.</param>
/// <param name="common">The shared participation the entity composes.</param>
/// <returns>The entity with the setup and main AI it runs.</returns>
internal delegate EnemyOccupant EnemySpawnCreation<in TSettings>(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place,
    EnemyCommonParticipation common) where TSettings : EnemySpawnSettings;
