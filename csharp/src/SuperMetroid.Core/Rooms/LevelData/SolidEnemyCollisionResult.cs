namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Observable outputs of one bank-$A0 solid-enemy collision probe.</summary>
/// <param name="Collided">True when the rounded future box overlaps a qualifying body and the current directional gap is not negative.</param>
/// <param name="Distance">Unsigned whole-pixel current-edge gap on collision, or the caller's requested whole distance on failure; no fractional distance is returned.</param>
/// <param name="EnemyIndex">Selected native enemy-slot byte index on collision, or null on failure.</param>
/// <param name="WasTouching">True for a successful zero-gap contact, which clears Samus's Y subposition; false for positive gaps and failed probes.</param>
public readonly record struct SolidEnemyCollisionResult(
    bool Collided,
    ushort Distance,
    ushort? EnemyIndex,
    bool WasTouching);
