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
    private readonly CeresDoorEscapePaintDefinitions armor;
    private readonly CeresBabyPaintDefinitions organ;
    private readonly int clearBlue;
    private readonly Dictionary<int, ushort> edits = [];

    internal NorfairRidleyInitialPaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 32) throw new ArgumentException("Norfair Ridley initial paint requires thirty-two colors.", nameof(colors));
        clearBlue = colors[0] >> 10;
        if (colors[0] != clearBlue << 10) edits.Add(0, colors[0]);
        if (colors[16] != clearBlue << 10) edits.Add(16, colors[16]);
        armor = new(colors.Slice(1, 15));
        organ = new(colors.Slice(17, 15));
    }

    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 32) throw new IndexOutOfRangeException();
        if (edits.TryGetValue(index, out ushort edited)) return edited;
        if (index is 0 or 16) return (ushort)(clearBlue << 10);
        return index < 16 ? armor.ColorAt(index - 1) : organ.Resolve(0, index - 17);
    }
}
