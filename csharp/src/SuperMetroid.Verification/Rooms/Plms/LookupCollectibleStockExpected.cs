using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The pointer and visual word of one lookup collectible stock expected.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Appearance">The block appearance.</param>
internal readonly record struct LookupCollectibleStockExpected(ushort Pointer, RoomBlockAppearance Appearance);
