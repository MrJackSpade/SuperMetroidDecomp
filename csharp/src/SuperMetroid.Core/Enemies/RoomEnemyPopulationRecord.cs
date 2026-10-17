namespace SuperMetroid.Core.Enemies;

/// <summary>One literal 16-byte bank-$A1 room-population record.</summary>
/// <param name="DefinitionPointer">Record +$00 bank-$A0 enemy header pointer; population list terminator $FFFF is not a spawn record.</param>
/// <param name="Position">Record +$02/+$04 initial room position in whole pixels, retaining both native sixteen-bit words.</param>
/// <param name="InitializationParameter">Actor-specific initialization value, 0 through 65535, initially copied to the slot's instruction-list field.</param>
/// <param name="Properties">The enemy's properties governing interaction, processing and visibility.</param>
/// <param name="ExtraProperties">The enemy's additional properties, including extended-spritemap selection.</param>
/// <param name="Parameter1">First actor-specific spawn parameter, 0 through 65535; its units and meaning belong to the selected enemy AI.</param>
/// <param name="Parameter2">Second actor-specific spawn parameter, 0 through 65535; its units and meaning belong to the selected enemy AI.</param>
public readonly record struct RoomEnemyPopulationRecord(
    EnemyDefinitionId DefinitionPointer,
    Position16<RoomPixels> Position,
    int InitializationParameter,
    EnemyPropertyWord Properties,
    EnemyExtraPropertyWord ExtraProperties,
    int Parameter1,
    int Parameter2)
{
    /// <summary>Actor-specific initialization value, 0 through 65535.</summary>
    public int InitializationParameter { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, InitializationParameter);

    /// <summary>First actor-specific spawn parameter, 0 through 65535.</summary>
    public int Parameter1 { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter1);

    /// <summary>Second actor-specific spawn parameter, 0 through 65535.</summary>
    public int Parameter2 { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Parameter2);
}
