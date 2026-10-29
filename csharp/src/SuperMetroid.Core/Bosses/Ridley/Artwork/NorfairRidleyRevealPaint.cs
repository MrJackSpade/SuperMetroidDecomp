using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.Ridley.Artwork.NorfairRidleyRevealPaintDefinitions;
namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Arena reveal $A6:A50B-A6AE, ending at the fourteen room-theme9 palette7
/// colors also present in decompressed $C2:B5E4. Native $A6:A4D6-A4EA uniformly
/// copies the selected step. Every channel uses the existing fifteen-interval
/// biased fade; no intermediate sample is independently stored.
/// </summary>
internal sealed class NorfairRidleyRevealPaint
{

    /// <summary>Independently supplied step colors that differ from the scaled material endpoints.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Captures only palette cells that differ from the calculated fifteen-step reveal.</summary>
    internal NorfairRidleyRevealPaint(RgbColor[][] steps)
    {
        for (int step = 0; step < Steps; step++)
            for (int color = 0; color < Colors; color++)
                if (steps[step][color] != Calculate(step, color)) edits.Add(new ColourCell(step, color), steps[step][color]);
    }

    /// <summary>Resolves an independently edited reveal cell or scales its stock material endpoint.</summary>
    internal RgbColor ColorAt(int step, int color)
    {
        ValidateStep(step);
        if ((uint)color >= Colors) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(step, color), out RgbColor edited) ? edited : Calculate(step, color);
    }

    /// <summary>Restricts lookup to the fifteen native reveal phases.</summary>
    internal static void ValidateStep(int step)
    {
        if ((uint)step >= Steps) throw new ArgumentOutOfRangeException(nameof(step));
    }

    /// <summary>Applies the shared biased fade to one selected arena material endpoint.</summary>
    private static RgbColor Calculate(int step, int color) => CeresRidleyFade.Scale(Endpoint(color), step + 1);

    /// <summary>
    /// Immutable stock material operation for $A6:A693-A6AE and theme9 slots113..126.
    /// Panel shade12 reuses masonry middle; cavern shade13 reuses cavern middle;
    /// slot14 is a copied white target (RGB5 maximum), not a visibility exemption.
    /// RoomStaticPalette's separate occurrence remains independently accounted.
    /// </summary>
    internal static RgbColor Endpoint(int color)
    {
        if ((uint)color >= Colors) throw new ArgumentOutOfRangeException(nameof(color));
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
    private static RgbColor Masonry(int shade) => Pack((3 - shade) * MasonryRedStep, 0,
        TrimBlue + (2 - shade) * MasonryBlueStep);
    /// <summary>Calculates cavern-edge red steps and the one-unit blue decline.</summary>
    private static RgbColor Cavern(int shade) => Pack(CavernHighlightRed - CavernRedStep * shade, 0,
        CavernHighlightBlue - shade);
    /// <summary>Clamps calculated red and blue material channels to RGB5 bounds.</summary>
    private static RgbColor Pack(int red, int green, int blue) =>
        new(Math.Clamp(red, 0, Maximum), green, Math.Clamp(blue, 0, Maximum));
}
