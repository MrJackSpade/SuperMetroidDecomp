namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One physical fragment emitted when Bomb Torizo's statue hand breaks.</summary>
internal readonly record struct BombTorizoStatueFragmentDefinition(
    TorizoProjectileProgram Program,
    Offset16<RoomPixels> Offset,
    ushort YVelocity,
    ushort Acceleration);
