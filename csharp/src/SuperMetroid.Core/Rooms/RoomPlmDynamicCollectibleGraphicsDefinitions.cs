namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The eight palette selector bytes and 256 raw 4bpp character bytes consumed by
/// bank-$84 instruction <c>$8764</c> for one permanent-item kind.
/// </summary>
internal sealed record RoomPlmDynamicCollectibleGraphic(
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

    private static readonly RoomPlmDynamicCollectibleGraphic[] Graphics;

    static RoomPlmDynamicCollectibleGraphicsDefinitions() => Graphics = Parse();

    internal static ReadOnlySpan<RoomPlmDynamicCollectibleGraphic> All => Graphics;

    internal static RoomPlmDynamicCollectibleGraphic Get(InWorldCollectibleKind kind)
    {
        int index = (int)kind - FirstKind;
        if ((uint)index >= Graphics.Length)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic graphics upload.");
        return Graphics[index];
    }

    private static RoomPlmDynamicCollectibleGraphic[] Parse()
    {
        if (Sources.Length != GraphicCount)
            throw new InvalidDataException(
                $"Compiled permanent-item graphics count is {Sources.Length}, expected {GraphicCount}.");
        var result = new RoomPlmDynamicCollectibleGraphic[GraphicCount];
        var pointers = new HashSet<ushort>();
        for (int index = 0; index < result.Length; index++)
        {
            var source = Sources[index];
            if (source.Kind != FirstKind + index ||
                source.GraphicsPointer < 0x8000 ||
                !pointers.Add(source.GraphicsPointer))
                throw new InvalidDataException(
                    $"Compiled permanent-item graphics entry {index} has an invalid kind or pointer.");
            byte[] palettes = Convert.FromHexString(source.PaletteHex);
            byte[] tiles = Convert.FromHexString(source.TilesHex);
            if (palettes.Length != 8 || tiles.Length != 0x100 ||
                palettes.Any(offset => offset > 7))
                throw new InvalidDataException(
                    $"Compiled permanent-item graphics entry {index} has invalid payload sizes or palettes.");
            result[index] = new RoomPlmDynamicCollectibleGraphic(
                (InWorldCollectibleKind)source.Kind,
                source.GraphicsPointer, palettes, tiles);
        }
        return result;
    }
}
