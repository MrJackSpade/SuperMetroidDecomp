using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Arena reveal $A6:A50B-A6AE, ending at the fourteen room-theme9 palette7
/// colors also present in decompressed $C2:B5E4. Native $A6:A4D6-A4EA uniformly
/// copies the selected row. Every channel uses the existing fifteen-interval
/// biased fade; no intermediate sample is independently stored.
/// </summary>
internal sealed class NorfairRidleyRevealPaintDefinitions
{
    /// <summary>Fifteen fade rows precede the full arena reveal palette.</summary>
    private const int Rows = 15;
    /// <summary>Each reveal row copies fourteen room-theme colors.</summary>
    private const int Colors = 14;
    /// <summary>Maximum value of an SNES five-bit color channel.</summary>
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
    /// <summary>Independently supplied row colors that differ from the scaled material endpoints.</summary>
    internal readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Captures only palette cells that differ from the calculated fifteen-step reveal.</summary>
    internal NorfairRidleyRevealPaintDefinitions(Bgr555[][] rows)
    {
        for (int row = 0; row < Rows; row++)
            for (int color = 0; color < Colors; color++)
                if (rows[row][color] != Calculate(row, color)) edits.Add(row * Colors + color, rows[row][color]);
    }

    /// <summary>Resolves an independently edited reveal cell or scales its stock material endpoint.</summary>
    internal Bgr555 ColorAt(int row, int color)
    {
        ValidateRow(row);
        if ((uint)color >= Colors) throw new IndexOutOfRangeException();
        return edits.TryGetValue(row * Colors + color, out Bgr555 edited) ? edited : Calculate(row, color);
    }

    /// <summary>Restricts lookup to the fifteen native reveal phases.</summary>
    internal static void ValidateRow(int row)
    {
        if ((uint)row >= Rows) throw new IndexOutOfRangeException();
    }

    /// <summary>Applies the shared biased fade to one selected arena material endpoint.</summary>
    private static Bgr555 Calculate(int row, int color) => CeresRidleyFadeColorDefinitions.Scale(Endpoint(color), row + 1);

    /// <summary>
    /// Immutable stock material operation for $A6:A693-A6AE and theme9 slots113..126.
    /// Panel shade12 reuses masonry middle; cavern shade13 reuses cavern middle;
    /// slot14 is a copied white target (RGB5 maximum), not a visibility exemption.
    /// RoomStaticPalette's separate occurrence remains independently accounted.
    /// </summary>
    internal static Bgr555 Endpoint(int color)
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

    /// <summary>Calculates the selected red-blue steps of the masonry and pillar ramp.</summary>
    private static Bgr555 Masonry(int shade) => Pack((3 - shade) * MasonryRedStep, 0,
        TrimBlue + (2 - shade) * MasonryBlueStep);
    /// <summary>Calculates cavern-edge red steps and the one-unit blue decline.</summary>
    private static Bgr555 Cavern(int shade) => Pack(CavernHighlightRed - CavernRedStep * shade, 0,
        CavernHighlightBlue - shade);
    /// <summary>Clamps calculated red and blue material channels to RGB5 bounds.</summary>
    private static Bgr555 Pack(int red, int green, int blue) =>
        new(Math.Clamp(red, 0, Maximum), green, Math.Clamp(blue, 0, Maximum));
}
