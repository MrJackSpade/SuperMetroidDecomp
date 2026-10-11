using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Reserve-strip visual identities and import-only cartridge bindings.</summary>
public static class PauseReserveTankDefinitions
{
    /// <summary>Supported schema version for the editable reserve-strip anchors, palette, and visual frames.</summary>
    public const int Version = 1;
    /// <summary>JSON asset filename for the pause equipment screen's reserve-tank strip presentation.</summary>
    public const string FileName = "pause-reserve-tanks.json";
    /// <summary>$82:C1D6 supplies six origins, including the trailing cap position.</summary>
    public const int AnchorCount = 6;
    /// <summary>$82:C1D6..C1E2: six eight-pixel columns starting at24, at Y96 minus the draw bias.</summary>
    /// <param name="index">Zero-based tank/cap anchor, 0..5, not a reserve-energy quantity.</param>
    /// <returns>The stock screen-pixel origin before editable anchor deviations are applied.</returns>
    /// <exception cref="IndexOutOfRangeException">The anchor is outside the six-entry strip.</exception>
    public static MapLabelPoint StockAnchor(int index)
    {
        if ((uint)index >= AnchorCount) throw new IndexOutOfRangeException();
        return new(24 + index * 8, 95);
    }
    /// <summary>$82:C1D6, EquipmentScreen_ReserveTank_Xpositions: import-only pointer to the six native horizontal origins; runtime uses the compiled stock anchors and editable deviations.</summary>
    public const int XPositions = PauseReserveTankRomData.XPositions;
    /// <summary>$82:C1E2 supplies Y plus one; import applies the draw routine's decrement.</summary>
    public const int YPosition = PauseReserveTankRomData.YPosition;
    /// <summary>$82:B3D9 has two identical eight-entry partial-fill tables.</summary>
    public const int PartialMaps = PauseReserveTankRomData.PartialMaps;
    /// <summary>$82:B3FC uses OBJ palette three; $82:B433 keeps that palette even as its unused timer advances.</summary>
    public const ushort PaletteBits = PauseReserveTankRomData.PaletteBits;
    /// <summary>$82:B305 full tank; $82:B396 trailing cap; $82:B37D empty tank and $82:B3D9 seven fill levels.</summary>
    /// <returns>Ten stable frame-name/native-spritemap pairs in full, end-cap, empty, then Fill1..Fill7 order; caller-owned reserve quantities select the frame.</returns>
    public static IEnumerable<(string Name, PauseReserveTankVisual Id)> Frames()
    {
        yield return ("Full", PauseReserveTankVisual.Full);
        yield return ("EndCap", PauseReserveTankVisual.EndCap);
        yield return ("Empty", PauseReserveTankVisual.Empty);
        for (int fill = 1; fill <= 7; fill++) yield return ($"Fill{fill}", FillLevel(fill));
    }

    /// <summary>The partial-tank spritemap identity for a fill-step count.</summary>
    /// <param name="steps">Fill steps, 0 (empty) through 7.</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is outside the eight fill levels.</exception>
    internal static PauseReserveTankVisual FillLevel(int steps) => steps switch
    {
        0 => PauseReserveTankVisual.Empty,
        1 => PauseReserveTankVisual.Fill1,
        2 => PauseReserveTankVisual.Fill2,
        3 => PauseReserveTankVisual.Fill3,
        4 => PauseReserveTankVisual.Fill4,
        5 => PauseReserveTankVisual.Fill5,
        6 => PauseReserveTankVisual.Fill6,
        7 => PauseReserveTankVisual.Fill7,
        _ => throw new ArgumentOutOfRangeException(nameof(steps), steps, "A reserve tank has eight fill levels."),
    };
    /// <summary>Reserve spritemaps $82:C35B/C369/C3D9..C410 are one stationary small sprite.</summary>
    /// <remarks>Partial levels1..6 advance through sheet tiles47..4C; level7 uses
    /// the full tile4E. Tile4D is empty, and4F is the end cap. Palette is caller-owned.</remarks>
    internal static int StockTile(PauseReserveTankVisual identity) => identity switch
    {
        PauseReserveTankVisual.Full or PauseReserveTankVisual.Fill7 => 0x4e,
        PauseReserveTankVisual.EndCap => 0x4f,
        PauseReserveTankVisual.Empty => 0x4d,
        PauseReserveTankVisual.Fill1 or PauseReserveTankVisual.Fill2 or PauseReserveTankVisual.Fill3 or
            PauseReserveTankVisual.Fill4 or PauseReserveTankVisual.Fill5 or PauseReserveTankVisual.Fill6 =>
            0x46 + (identity - PauseReserveTankVisual.Empty),
        _ => throw new InvalidOperationException($"Undefined PauseReserveTankVisual {identity}."),
    };

    internal static SpriteVisualPart StockPart(PauseReserveTankVisual identity)
    {
        int tile = StockTile(identity);
        return new() { OffsetX = 0, OffsetY = 0, Size = 8, Priority = 3, Palette = null,
            FlipX = false, FlipY = false, TileColumn = tile % MapSpriteFormat.TileColumns, TileRow = tile / MapSpriteFormat.TileColumns };
    }
    /// <summary>$82:B3D9 maps each of eight fill-step indices to $20..$27; both native tables agree.</summary>
    /// <param name="index">Entry index, 0..15, covering both identical eight-entry native tables.</param>
    /// <returns>The Empty-through-Fill7 spritemap identity selected by index modulo eight; gameplay computes the index using fourteen energy per step.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the sixteen native entries.</exception>
    public static PauseReserveTankVisual PartialMap(int index) => (uint)index < 16
        ? FillLevel(index % 8) : throw new ArgumentOutOfRangeException(nameof(index));
}
