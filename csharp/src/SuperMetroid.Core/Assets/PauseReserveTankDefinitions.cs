using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Reserve-strip visual identities and import-only cartridge bindings.</summary>
public static class PauseReserveTankDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>$82:C1D6 supplies six origins, including the trailing cap position.</summary>
    public const int AnchorCount = 6;
    /// <summary>$82:C1D6..C1E2: six eight-pixel columns starting at24, at Y96 minus the draw bias.</summary>
    public static MapLabelPoint StockAnchor(int index)
    {
        if ((uint)index >= AnchorCount) throw new IndexOutOfRangeException();
        return new(24 + index * 8, 95);
    }
    public const int XPositions = PauseReserveTankRomData.XPositions;
    /// <summary>$82:C1E2 supplies Y plus one; import applies the draw routine's decrement.</summary>
    public const int YPosition = PauseReserveTankRomData.YPosition;
    /// <summary>$82:B3D9 has two identical eight-entry partial-fill tables.</summary>
    public const int PartialMaps = PauseReserveTankRomData.PartialMaps;
    /// <summary>$82:B3FC uses OBJ palette three; $82:B433 keeps that palette even as its unused timer advances.</summary>
    public const ushort PaletteBits = PauseReserveTankRomData.PaletteBits;
    /// <summary>$82:B305 full tank; $82:B396 trailing cap; $82:B37D empty tank and $82:B3D9 seven fill levels.</summary>
    public static IEnumerable<(string Name, ushort Id)> Frames()
    {
        yield return ("Full", PauseReserveTankRomData.FullMap);
        yield return ("EndCap", PauseReserveTankRomData.EndCapMap);
        yield return ("Empty", PauseReserveTankRomData.EmptyMap);
        for (int fill = 1; fill <= 7; fill++) yield return ($"Fill{fill}", (ushort)(PauseReserveTankRomData.EmptyMap + fill));
    }
    /// <summary>Reserve spritemaps $82:C35B/C369/C3D9..C410 are one stationary small sprite.</summary>
    /// <remarks>Partial levels1..6 advance through sheet tiles47..4C; level7 uses
    /// the full tile4E. Tile4D is empty, and4F is the end cap. Palette is caller-owned.</remarks>
    internal static int StockTile(ushort identity) => identity switch
    {
        PauseReserveTankRomData.FullMap or PauseReserveTankRomData.EmptyMap + 7 => 0x4e,
        PauseReserveTankRomData.EndCapMap => 0x4f,
        PauseReserveTankRomData.EmptyMap => 0x4d,
        > PauseReserveTankRomData.EmptyMap and < PauseReserveTankRomData.EmptyMap + 7 => 0x46 + identity - PauseReserveTankRomData.EmptyMap,
        _ => throw new InvalidDataException($"Unknown reserve visual {identity:X4}."),
    };

    internal static SpriteVisualPart StockPart(ushort identity)
    {
        int tile = StockTile(identity);
        return new() { OffsetX = 0, OffsetY = 0, Size = 8, Priority = 3, Palette = null,
            FlipX = false, FlipY = false, TileColumn = tile % MapSpriteFormat.TileColumns, TileRow = tile / MapSpriteFormat.TileColumns };
    }
    /// <summary>$82:B3D9 maps each eighth-step index to $20..$27; both native tables agree.</summary>
    public static ushort PartialMap(int index) => (uint)index < 16
        ? (ushort)(PauseReserveTankRomData.EmptyMap + index % 8) : throw new ArgumentOutOfRangeException(nameof(index));
}
