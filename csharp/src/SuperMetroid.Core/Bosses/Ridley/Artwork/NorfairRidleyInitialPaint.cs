using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Initial Ridley OBJ targets at $A6:E1CF-E20E. The first material is the escape
/// door's grey-blue paint; the second is the Baby/container initial material.
/// Each occurrence owns its supplied channel edits. The two copied clear-slot
/// blue14 targets specify the exact copied metadata at $A6:E1CF/E1EF, shared
/// across the two clear slots. $A6:A1C7-A1D0 uniformly copies these observable
/// targets; transparency does not choose their value. Replacing that datum would
/// invent different source content. Independent supplied clear-slot edits survive.
/// Arena reveal colors are independent of this initial material operation.
/// </summary>
internal sealed class NorfairRidleyInitialPaint
{
    /// <summary>First 15 non-clear targets, resolved through the escape-door armor paint rules.</summary>
    internal readonly CeresDoorEscapePaint armor;
    /// <summary>Second 15 non-clear targets, resolved through the Baby/container paint rules.</summary>
    internal readonly CeresBabyPaint organ;
    /// <summary>Copied blue channel shared by the two clear-slot source records.</summary>
    private readonly int clearBlue;
    /// <summary>Supplied clear-slot colors that differ from the copied native metadata.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Splits 32 supplied targets into two materials and preserves independent clear-slot edits.</summary>
    internal NorfairRidleyInitialPaint(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 32);
        clearBlue = colors[0].Blue;
        if (colors[0] != ClearSlot) edits.Add(new ColourCell(0, 0), colors[0]);
        if (colors[16] != ClearSlot) edits.Add(new ColourCell(0, 16), colors[16]);
        armor = new(colors.Slice(1, 15));
        organ = new(colors.Slice(17, 15));
    }

    /// <summary>Reconstructs the native clear-slot value from the source's copied blue channel.</summary>
    private RgbColor ClearSlot => new(0, 0, clearBlue);

    /// <summary>Uses an edited clear slot when supplied; otherwise resolves the matching armor or organ material.</summary>
    internal RgbColor ColorAt(int index)
    {
        if ((uint)index >= 32) throw new ArgumentOutOfRangeException(nameof(index));
        if (edits.TryGetValue(new ColourCell(0, index), out RgbColor edited)) return edited;
        if (index is 0 or 16) return ClearSlot;
        return index < 16 ? armor.ColorAt(index - 1) : organ.Resolve(CeresBabyShade.Resting, index - 17);
    }
}
