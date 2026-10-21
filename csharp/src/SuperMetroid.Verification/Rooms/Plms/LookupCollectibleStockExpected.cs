namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The pointer and visual word of one lookup collectible stock expected.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="VisualWord">The visual word.</param>
internal readonly record struct LookupCollectibleStockExpected(ushort Pointer, int VisualWord);
