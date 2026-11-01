using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Initial Ridley sprite targets. The first material is the escape
/// door's grey-blue paint; the second is the Baby/container initial material.
/// Each occurrence owns its supplied channel edits. The two copied clear-slot
/// blue14 targets specify the exact copied metadata, shared
/// across the two clear slots. Norfair Ridley's initialization uniformly copies these observable
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
    /// <summary>Supplied clear-slot colors that differ from the copied metadata.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits;

    /// <summary>Keeps the two materials, the copied clear-slot blue and the independent clear-slot edits.</summary>
    /// <param name="armor">The escape-door armor material of the first fifteen targets.</param>
    /// <param name="organ">The Baby/container material of the second fifteen targets.</param>
    /// <param name="clearBlue">The copied clear-slot blue.</param>
    /// <param name="edits">The clear-slot colors the import found differing from the copied metadata.</param>
    internal NorfairRidleyInitialPaint(CeresDoorEscapePaint armor, CeresBabyPaint organ, int clearBlue,
        Dictionary<ColourCell, RgbColor> edits)
    {
        this.armor = armor;
        this.organ = organ;
        this.clearBlue = clearBlue;
        this.edits = edits;
    }

    /// <summary>Reconstructs the clear-slot value from the source's copied blue channel.</summary>
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
