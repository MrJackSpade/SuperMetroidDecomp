using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>The pointer and block appearances of one lookup eye door stock expected.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Words">The block appearances.</param>
internal readonly record struct LookupEyeDoorStockExpected(ushort Pointer, RoomBlockAppearance[] Words);
