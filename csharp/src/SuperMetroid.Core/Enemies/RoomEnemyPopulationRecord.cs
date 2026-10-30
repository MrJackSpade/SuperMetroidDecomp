namespace SuperMetroid.Core.Enemies;

/// <summary>One literal 16-byte bank-$A1 room-population record.</summary>
/// <param name="DefinitionId">Record +$00 bank-$A0 enemy header pointer; population list terminator $FFFF is not a spawn record.</param>
/// <param name="Position">Record +$02/+$04 initial room position in whole pixels, retaining both native sixteen-bit words.</param>
/// <param name="SpawnSettings">What the entry tells its species about how to start, decoded by the import (#1471).</param>
/// <param name="Properties">The enemy's properties governing interaction, processing and visibility.</param>
/// <param name="ExtraProperties">The enemy's additional properties, including extended-spritemap selection.</param>
/// <param name="Parameter1">First actor-specific spawn parameter, 0 through 65535; its units and meaning belong to the selected enemy AI.</param>
/// <param name="Parameter2">Second actor-specific spawn parameter, 0 through 65535; its units and meaning belong to the selected enemy AI.</param>
public readonly record struct RoomEnemyPopulationRecord(
    EnemyDefinitionId DefinitionId,
    Position16<RoomPixels> Position,
    EnemySpawnSettings SpawnSettings,
    EnemyPropertyWord Properties,
    EnemyExtraPropertyWord ExtraProperties,
    int Parameter1,
    int Parameter2)
{
    /// <summary>What the entry tells its species about how to start.</summary>
    public EnemySpawnSettings SpawnSettings { get; init; } = Ensure.NotNull(SpawnSettings);

    /// <summary>The entry's settings as the <typeparamref name="T"/> its species reads.</summary>
    /// <exception cref="InvalidDataException">The entry carries other settings.</exception>
    internal T SpawnSettingsOf<T>() where T : EnemySpawnSettings =>
        SpawnSettings as T ?? throw new InvalidDataException(
            $"Enemy {DefinitionId}'s population entry carries {SpawnSettings}, not {typeof(T).Name}.");

    /// <summary>First actor-specific spawn parameter, 0 through 65535.</summary>
    [PackedFields("population parameter word whose meaning each species decodes at spawn")]
    public int Parameter1 { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter1);

    /// <summary>Second actor-specific spawn parameter, 0 through 65535.</summary>
    public int Parameter2 { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter2);

    /// <summary>Parameter 1 as the two one-byte settings a species that packs two settings into it reads.</summary>
    public PopulationParameterSettings Parameter1Settings => PopulationParameterSettings.Of(Parameter1);

    /// <summary>Parameter 2 as the two one-byte settings a species that packs two settings into it reads.</summary>
    public PopulationParameterSettings Parameter2Settings => PopulationParameterSettings.Of(Parameter2);
}
