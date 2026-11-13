namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One physical fragment emitted when Bomb Torizo's statue hand breaks.</summary>
internal readonly record struct BombTorizoStatueFragmentDefinition(
    TorizoProjectileSegment Animation,
    Offset16<RoomPixels> Offset,
    int YVelocity,
    SignedEightEight Acceleration);
