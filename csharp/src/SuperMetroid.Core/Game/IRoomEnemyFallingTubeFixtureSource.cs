namespace SuperMetroid.Core.Game;

/// <summary>
/// Explicit constructed placement for Mother Brain's falling tubes. Gameplay uses
/// the compiled retail record; a verifier supplies a record directly instead of
/// asking Core to decode cartridge-bank bytes from an untyped bus.
/// </summary>
internal interface IRoomEnemyFallingTubeFixtureSource
{
    /// <summary>Supplies a constructed falling-tube population record for an authored fixture bus.</summary>
    /// <param name="pointer">Native population-record pointer selected by the Mother Brain cutscene.</param>
    /// <returns>The eight-word population record associated with the pointer.</returns>
    RoomEnemyPopulationRecord ReadFallingTubePopulation(ushort pointer);
}
