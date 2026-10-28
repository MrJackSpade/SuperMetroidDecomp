using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The eight palette selector bytes and 256 raw 4bpp character bytes consumed by
/// bank-$84 instruction <c>$8764</c> for one permanent-item kind.
/// </summary>
public sealed record RoomPlmDynamicCollectibleGraphic(
    InWorldCollectibleKind Kind,
    ReadOnlyMemory<byte> PaletteOffsets,
    ReadOnlyMemory<byte> Tiles);
