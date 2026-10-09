namespace SuperMetroid.Core.Assets;

/// <summary>
/// Arena reveal $A6:A50B-A6AE, ending at the fourteen room-theme9 palette7
/// colors also present in decompressed $C2:B5E4. Native $A6:A4D6-A4EA uniformly
/// copies the selected row. Every channel uses the existing fifteen-interval
/// biased fade; no intermediate sample is independently stored.
/// </summary>
internal sealed class NorfairRidleyRevealPaintDefinitions
{
    /// <summary>Number of ordered arena-reveal palette rows, each representing one fade intensity.</summary>
    private const int Rows = 15;
    /// <summary>Number of theme-9 palette colors replaced by each reveal row.</summary>
    private const int Colors = 14;
    /// <summary>Maximum component value representable by a five-bit RGB channel.</summary>
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
    /// <summary>Authored row/color values that differ from the calculated fifteen-step fade.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Retains only supplied palette entries that differ from their calculated reveal colors.</summary>
    /// <param name="rows">Fifteen chronological rows of fourteen packed RGB5 colors each.</param>
    internal NorfairRidleyRevealPaintDefinitions(ushort[][] rows)
    {
        for (int row = 0; row < Rows; row++)
            for (int color = 0; color < Colors; color++)
                if (rows[row][color] != Calculate(row, color)) edits.Add(row * Colors + color, rows[row][color]);
    }

    /// <summary>Returns the authored override or calculated fade color for one row and palette position.</summary>
    /// <param name="row">Zero-based reveal row, from the first visible step through the final fade step.</param>
    /// <param name="color">Zero-based color among the fourteen replaced palette entries.</param>
    /// <returns>The selected packed RGB5 color.</returns>
    /// <exception cref="IndexOutOfRangeException">Either index is outside its reveal table.</exception>
    internal ushort ColorAt(int row, int color)
    {
        ValidateRow(row);
        if ((uint)color >= Colors) throw new IndexOutOfRangeException();
        return edits.TryGetValue(row * Colors + color, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Rejects a reveal-row index outside the fifteen authored fade steps.</summary>
    /// <param name="row">Zero-based row to validate.</param>
    /// <exception cref="IndexOutOfRangeException">The row is outside the reveal table.</exception>
    internal static void ValidateRow(int row)
    {
        if ((uint)row >= Rows) throw new IndexOutOfRangeException();
    }

    /// <summary>Scales a stock material endpoint by the intensity assigned to the requested reveal row.</summary>
    /// <param name="row">Zero-based reveal step; its one-based value selects the fade intensity.</param>
    /// <param name="color">Zero-based palette entry whose stock endpoint is scaled.</param>
    /// <returns>The packed RGB5 color for that step.</returns>
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

    /// <summary>Composes a masonry/pillar shade from its red step and shared blue tint.</summary>
    /// <param name="shade">Zero-based shade from the brightest to the darkest material level.</param>
    /// <returns>The packed RGB5 masonry color.</returns>
    private static ushort Masonry(int shade) => Pack((3 - shade) * MasonryRedStep, 0,
        TrimBlue + (2 - shade) * MasonryBlueStep);
    /// <summary>Composes a cavern shade by reducing red and blue from the selected edge highlight.</summary>
    /// <param name="shade">Zero-based shade progression from the edge highlight.</param>
    /// <returns>The packed RGB5 cavern color, with channels clipped to the RGB5 range.</returns>
    private static ushort Cavern(int shade) => Pack(CavernHighlightRed - CavernRedStep * shade, 0,
        CavernHighlightBlue - shade);
    /// <summary>Packs red, green, and blue component values into the SNES RGB5 word layout.</summary>
    /// <param name="red">Red channel value, clipped to five bits.</param>
    /// <param name="green">Green channel value, placed in bits five through nine.</param>
    /// <param name="blue">Blue channel value, clipped to five bits.</param>
    /// <returns>The packed SNES color word.</returns>
    private static ushort Pack(int red, int green, int blue) => (ushort)(Math.Clamp(red, 0, Maximum)
        | green << 5 | Math.Clamp(blue, 0, Maximum) << 10);
}
