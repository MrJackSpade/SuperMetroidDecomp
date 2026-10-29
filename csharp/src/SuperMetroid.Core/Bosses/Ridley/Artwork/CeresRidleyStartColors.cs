using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using static SuperMetroid.Core.Bosses.Ridley.Artwork.CeresRidleyStartColorDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Additional OBJ palettes at $A6:E16F-E1AE, copied together by $A6:A245-A24E.
/// The first palette repeats the reviewed normal door/container material at
/// $A6:F4EC. The second repeats the Baby initial colors at $A6:E1F1; their paint
/// inputs remain owned by the separately reviewed CeresBabyPaint.
/// Both transparent entries retain the selected copied clear-black target value;
/// transparency itself does not require RGB zero. Independent supplied edits,
/// including edits to either occurrence of the Baby colors, remain separate.
/// </summary>
internal sealed class CeresRidleyStartColors
{
    /// <summary>Normal door/container material used by the first palette's non-clear colors.</summary>
    private readonly CeresDoorNormalPaint door;
    /// <summary>Previously validated Baby material reused by the second palette.</summary>
    private readonly CeresBabyPaint baby;
    /// <summary>Independent supplied changes to either palette, including transparent entries.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Separates the two palettes, preserving colors that differ from calculated source materials.</summary>
    internal CeresRidleyStartColors(ReadOnlySpan<RgbColor> colors, CeresBabyPaint initialBaby)
    {
        if (colors.Length != 2 * PaletteSize)
            throw new ArgumentException("Ceres additional palettes require 32 colors and fifteen Baby colors.");
        door = new(colors.Slice(1, PaletteSize - 1));
        baby = initialBaby;
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) edits.Add(new ColourCell(0, color), colors[color]);
    }

    /// <summary>Uses a supplied edit when present, otherwise reconstructing the selected source color.</summary>
    internal RgbColor Resolve(int color)
    {
        if ((uint)color >= 2 * PaletteSize) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : Calculate(color);
    }

    /// <summary>Calculates copied clear-black entries, door paint, or Baby paint for an unedited index.</summary>
    private RgbColor Calculate(int color) => color switch
    {
        0 or PaletteSize => RgbColor.Black,
        < PaletteSize => door.ColorAt(color - 1),
        _ => baby.Resolve(CeresBabyShade.Resting, color - PaletteSize - 1),
    };
}
