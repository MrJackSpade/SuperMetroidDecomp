namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One entry of a room's enemy population (#1472): which enemy spawns, where, with which engine properties, and the
/// settings its species starts from.
/// </summary>
/// <param name="DefinitionId">The enemy that spawns.</param>
/// <param name="Position">The initial room position in whole pixels, each coordinate wrapping at sixteen bits.</param>
/// <param name="Properties">The enemy's properties governing interaction, processing and visibility.</param>
/// <param name="ExtraProperties">The enemy's additional properties, including extended-spritemap selection.</param>
/// <param name="SpawnSettings">What the entry tells its species about how to start, decoded by the import (#1471, #1472).</param>
public readonly record struct RoomEnemyPopulationRecord(
    EnemyDefinitionId DefinitionId,
    Position16<RoomPixels> Position,
    EnemyProperties Properties,
    EnemyExtraProperties ExtraProperties,
    EnemySpawnSettings SpawnSettings)
{
    /// <summary>What the entry tells its species about how to start.</summary>
    public EnemySpawnSettings SpawnSettings { get; init; } = Ensure.NotNull(SpawnSettings);

    /// <summary>The entry's settings as the <typeparamref name="T"/> its species reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettingsOf<T>() where T : EnemySpawnSettings =>
        SpawnSettings as T ?? throw new InvalidDataException(
            $"Enemy {DefinitionId}'s population entry carries {SpawnSettings}, not {typeof(T).Name}.");
}
