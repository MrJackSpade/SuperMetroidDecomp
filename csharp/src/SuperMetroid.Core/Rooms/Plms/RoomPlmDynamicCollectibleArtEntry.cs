namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable pixels and tile-palette selectors for one permanent-item kind.</summary>
/// <param name="Kind">Dynamically uploaded permanent collectible kind with native header-table index 4..20; the four preceding energy/ammunition pickups use separate fixed artwork.</param>
/// <param name="Tiles">Caller-owned $0100-byte array of eight consecutive native four-bit planar characters, representing the item's two visual frames.</param>
/// <param name="PaletteOffsets">Caller-owned eight-byte array of BG palette selectors 0..7, one per uploaded character; these are not palette colors or CGRAM byte offsets.</param>
public sealed record RoomPlmDynamicCollectibleArtEntry(
    InWorldCollectibleKind Kind, byte[] Tiles, byte[] PaletteOffsets);
