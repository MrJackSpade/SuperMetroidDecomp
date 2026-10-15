namespace SuperMetroid.Core.Enemies;

/// <summary>One literal 16-byte bank-$A1 room-population record.</summary>
/// <param name="DefinitionPointer">Record +$00 bank-$A0 enemy header pointer; population list terminator $FFFF is not a spawn record.</param>
/// <param name="Position">Record +$02/+$04 initial room position in whole pixels, retaining both native sixteen-bit words.</param>
/// <param name="InitializationParameter">Record +$06 actor-specific initialization word, initially copied to the slot's instruction-list field.</param>
/// <param name="Properties">Record +$08 primary native enemy properties governing interaction, processing, and visibility.</param>
/// <param name="ExtraProperties">Record +$0A additional native properties, including extended-spritemap selection.</param>
/// <param name="Parameter1">Record +$0C first actor-specific spawn parameter; units and meaning belong to the selected enemy AI.</param>
/// <param name="Parameter2">Record +$0E second actor-specific spawn parameter; units and meaning belong to the selected enemy AI.</param>
public readonly record struct RoomEnemyPopulationRecord(
    EnemyDefinitionId DefinitionPointer,
    Position16<RoomPixels> Position,
    ushort InitializationParameter,
    ushort Properties,
    ushort ExtraProperties,
    ushort Parameter1,
    ushort Parameter2);
