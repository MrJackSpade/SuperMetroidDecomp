namespace SuperMetroid.Core.Game;

/// <summary>
/// Explicit constructed placement for Mother Brain's falling tubes. Gameplay uses
/// the compiled retail record; a verifier supplies a record directly instead of
/// asking Core to decode cartridge-bank bytes from an untyped bus.
/// </summary>
internal interface IRoomEnemyFallingTubeFixtureSource
{
    /// <summary>Reads falling tube population.</summary>
    RoomEnemyPopulationRecord ReadFallingTubePopulation(ushort pointer);
}
