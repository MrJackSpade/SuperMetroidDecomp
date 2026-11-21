namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One physical fragment emitted when Bomb Torizo's statue hand breaks: its animation and place; every fragment falls alike
/// (<see cref="BombTorizoStatueFragmentDefinitions.InitialFallSpeed"/>, <see cref="BombTorizoStatueFragmentDefinitions.Acceleration"/>).
/// </summary>
internal readonly record struct BombTorizoStatueFragmentDefinition(
    TorizoProjectileSegment Animation,
    Offset16<RoomPixels> Offset);
