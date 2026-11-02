using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using static SuperMetroid.Core.Bosses.Ridley.Artwork.CeresRidleyStartColorDefinitions;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Additional sprite palettes, copied together by Ceres Ridley's initialization.
/// The first palette repeats the reviewed normal door/container material. The
/// second repeats the Baby initial colors; their paint
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
    internal readonly Dictionary<ColourCell, RgbColor> edits;

    /// <summary>Keeps the two palettes' source materials and the colors that differ from them.</summary>
    /// <param name="door">The normal door/container material of the first palette.</param>
    /// <param name="baby">The Baby material of the second palette.</param>
    /// <param name="edits">The supplied colors the import found differing from the materials.</param>
    internal CeresRidleyStartColors(CeresDoorNormalPaint door, CeresBabyPaint baby, Dictionary<ColourCell, RgbColor> edits)
    {
        this.door = door;
        this.baby = baby;
        this.edits = edits;
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
