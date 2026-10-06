namespace SuperMetroid.Core.Assets;

/// <summary>
/// Arena reveal $A6:A50B-A6AE, ending at the fourteen room-theme9 palette7
/// colors also present in decompressed $C2:B5E4. Native $A6:A4D6-A4EA uniformly
/// copies the selected row. Every channel uses the existing fifteen-interval
/// biased fade; no intermediate sample is independently stored.
/// </summary>
internal sealed class NorfairRidleyRevealPaintDefinitions
{
    private const int Rows = 15;
    private const int Colors = 14;
    private const int Maximum = (1 << 5) - 1;

    /// <summary>
    /// $A6:A693-A698: warm trim's selected red22/blue1 and green12/6/2 shades.
    /// Slots2/3 mark small trim highlights in $B9:BBA5; slot1 is the same
    /// material's copied bright target, without a visible-use claim for that map.
    /// </summary>
    private const int TrimRed = 22, TrimBlue = 1,
        TrimHighlightGreen = 12, TrimMiddleGreen = 6, TrimShadowGreen = 2;

    /// <summary>
    /// $A6:A699-A6A0: three masonry/pillar shades and black, with red step4
    /// and blue step2 above the shared blue1 dark tint. These selected paint
    /// steps describe the native dithered material, not physical illumination.
    /// </summary>
    private const int MasonryRedStep = 4, MasonryBlueStep = 2;

    /// <summary>
    /// $A6:A6A1-A6A8: cavern-edge highlight red14/blue2, red shade step5 and
    /// one-unit blue decline, clipped to black. Replacing the selected material
    /// levels or grouping would invent different categorical artwork content.
    /// </summary>
    private const int CavernHighlightRed = 14, CavernRedStep = 5, CavernHighlightBlue = 2;
    private readonly Dictionary<int, ushort> edits = [];

    internal NorfairRidleyRevealPaintDefinitions(ushort[][] rows)
    {
        for (int row = 0; row < Rows; row++)
            for (int color = 0; color < Colors; color++)
                if (rows[row][color] != Calculate(row, color)) edits.Add(row * Colors + color, rows[row][color]);
    }

    internal ushort ColorAt(int row, int color)
    {
        ValidateRow(row);
        if ((uint)color >= Colors) throw new IndexOutOfRangeException();
        return edits.TryGetValue(row * Colors + color, out ushort edited) ? edited : Calculate(row, color);
    }

    internal static void ValidateRow(int row)
    {
        if ((uint)row >= Rows) throw new IndexOutOfRangeException();
    }

    private static ushort Calculate(int row, int color) => CeresRidleyFadeColorDefinitions.Scale(Endpoint(color), row + 1);

    /// <summary>
    /// Immutable stock material operation for $A6:A693-A6AE and theme9 slots113..126.
    /// Panel shade12 reuses masonry middle; cavern shade13 reuses cavern middle;
    /// slot14 is a copied white target (RGB5 maximum), not a visibility exemption.
    /// RoomStaticPalette's separate occurrence remains independently accounted.
    /// </summary>
    internal static ushort Endpoint(int color)
    {
        if ((uint)color >= Colors) throw new IndexOutOfRangeException();
        if (color < 3)
        {
            int green = color switch { 0 => TrimHighlightGreen, 1 => TrimMiddleGreen, _ => TrimShadowGreen };
            return Pack(TrimRed, green, TrimBlue);
        }
        if (color < 7) return Masonry(color - 3);
        if (color < 11) return Cavern(color - 7);
        if (color == 11) return Masonry(1);
        if (color == 12) return Cavern(1);
        return Pack(Maximum, Maximum, Maximum);
    }

    private static ushort Masonry(int shade) => Pack((3 - shade) * MasonryRedStep, 0,
        TrimBlue + (2 - shade) * MasonryBlueStep);
    private static ushort Cavern(int shade) => Pack(CavernHighlightRed - CavernRedStep * shade, 0,
        CavernHighlightBlue - shade);
    private static ushort Pack(int red, int green, int blue) => (ushort)(Math.Clamp(red, 0, Maximum)
        | green << 5 | Math.Clamp(blue, 0, Maximum) << 10);
}
