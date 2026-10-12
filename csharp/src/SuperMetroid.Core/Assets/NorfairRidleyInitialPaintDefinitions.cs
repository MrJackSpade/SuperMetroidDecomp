using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

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
internal sealed class NorfairRidleyInitialPaintDefinitions
{
    /// <summary>Stores armor.</summary>
    private readonly CeresDoorEscapePaintDefinitions armor;
    /// <summary>Stores organ.</summary>
    private readonly CeresBabyPaintDefinitions organ;
    /// <summary>Stores clear blue.</summary>
    private readonly int clearBlue;
    /// <summary>Stores edits.</summary>
    private readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Initializes a new norfair ridley initial paint definitions instance.</summary>
    internal NorfairRidleyInitialPaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        Ensure.LengthEqual(colors, 32);
        clearBlue = colors[0].Blue;
        if (colors[0] != ClearSlot) edits.Add(0, colors[0]);
        if (colors[16] != ClearSlot) edits.Add(16, colors[16]);
        armor = new(colors.Slice(1, 15));
        organ = new(colors.Slice(17, 15));
    }

    /// <summary>Gets clear slot.</summary>
    private Bgr555 ClearSlot => new(0, 0, clearBlue);

    /// <summary>Color at.</summary>
    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 32) throw new IndexOutOfRangeException();
        if (edits.TryGetValue(index, out Bgr555 edited)) return edited;
        if (index is 0 or 16) return ClearSlot;
        return index < 16 ? armor.ColorAt(index - 1) : organ.Resolve(0, index - 17);
    }
}
