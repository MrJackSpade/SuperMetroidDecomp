namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The eight palette selector bytes and 256 raw 4bpp character bytes consumed by
/// bank-$84 instruction <c>$8764</c> for one permanent-item kind.
/// </summary>
public sealed record RoomPlmDynamicCollectibleGraphic(
    InWorldCollectibleKind Kind,
    ushort GraphicsPointer,
    ReadOnlyMemory<byte> PaletteOffsets,
    ReadOnlyMemory<byte> Tiles);

/// <summary>
/// Cartridge definitions shared by the exposed, Chozo-orb, and shot-block
/// presentations of the 17 dynamically uploaded permanent-item kinds.
/// </summary>
internal static partial class RoomPlmDynamicCollectibleGraphicsDefinitions
{
    internal const int FirstKind = (int)InWorldCollectibleKind.Bombs;
    internal const int GraphicCount = RoomPlmHeaders.PermanentCollectibleKindCount - FirstKind;

    internal static IEnumerable<RoomPlmDynamicCollectibleGraphic> All
    {
        get
        {
            for (int kind = FirstKind; kind < FirstKind + GraphicCount; kind++)
                yield return Get((InWorldCollectibleKind)kind);
        }
    }

    internal static RoomPlmDynamicCollectibleGraphic Get(InWorldCollectibleKind kind)
    {
        int index = (int)kind - FirstKind;
        if ((uint)index >= GraphicCount)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic graphics upload.");
        var source = Sources[index];
        byte[] palettes = new byte[8];
        for (int tile = 0; tile < palettes.Length; tile++)
            palettes[tile] = PaletteOffset(kind, tile);
        return new(kind, source.GraphicsPointer, palettes, Convert.FromHexString(source.TilesHex));
    }

    /// <summary>
    /// Eight upload palette selectors are two row-major 2x2 frames. Beam icons
    /// select their accent palette in the top-right tile of each frame; X-ray
    /// uses its top row with different palettes between the two frames.
    /// </summary>
    internal static byte PaletteOffset(InWorldCollectibleKind kind, int tile)
    {
        if ((uint)((int)kind - FirstKind) >= GraphicCount)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic graphics upload.");
        if ((uint)tile >= 8)
            throw new ArgumentOutOfRangeException(nameof(tile));
        if (kind == InWorldCollectibleKind.XrayScope)
            return (byte)(tile % 4 < 2 ? (tile < 4 ? 1 : 3) : 0);
        if (tile % 4 != 1) return 0;
        return kind switch
        {
            InWorldCollectibleKind.IceBeam => 3,
            InWorldCollectibleKind.WaveBeam => 2,
            InWorldCollectibleKind.PlasmaBeam => 1,
            _ => 0,
        };
    }
}