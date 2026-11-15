namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One statue fragment the breaking Bomb Torizo hand spawns at its block.</summary>
/// <param name="Fragment">The piece that falls, which selects its animation, place and fall.</param>
/// <param name="RoomObjectBlock">The block the hand occupies, which the fragment's placement is relative to.</param>
public readonly record struct BombTorizoStatueProjectileRequest(
    BombTorizoStatueFragment Fragment,
    Position16<RoomBlocks> RoomObjectBlock);
