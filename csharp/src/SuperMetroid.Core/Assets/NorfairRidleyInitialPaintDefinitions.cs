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
    /// <summary>Resolves the initial armor material for the first fifteen color slots.</summary>
    private readonly CeresDoorEscapePaintDefinitions armor;

    /// <summary>Resolves the initial Baby/container material for the final fifteen color slots.</summary>
    private readonly CeresBabyPaintDefinitions organ;

    /// <summary>Stores the shared high color bits used to reconstruct the two copied clear-slot targets.</summary>
    private readonly int clearBlue;

    /// <summary>Holds supplied clear-slot values that differ from the shared copied target.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds resolvers for the two initial materials and preserves supplied clear-slot overrides.</summary>
    /// <param name="colors">The thirty-two initial palette values, split between armor, clear slots, and organ.</param>
    /// <exception cref="ArgumentException">The span does not contain exactly thirty-two colors.</exception>
    internal NorfairRidleyInitialPaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 32) throw new ArgumentException("Norfair Ridley initial paint requires thirty-two colors.", nameof(colors));
        clearBlue = colors[0] >> 10;
        if (colors[0] != clearBlue << 10) edits.Add(0, colors[0]);
        if (colors[16] != clearBlue << 10) edits.Add(16, colors[16]);
        armor = new(colors.Slice(1, 15));
        organ = new(colors.Slice(17, 15));
    }

    /// <summary>Resolves an initial palette value, including copied clear-slot targets and explicit overrides.</summary>
    /// <param name="index">Zero-based slot in the thirty-two-color initial palette.</param>
    /// <returns>The resolved color at the requested slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the initial palette.</exception>
    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 32) throw new IndexOutOfRangeException();
        if (edits.TryGetValue(index, out ushort edited)) return edited;
        if (index is 0 or 16) return (ushort)(clearBlue << 10);
        return index < 16 ? armor.ColorAt(index - 1) : organ.Resolve(0, index - 17);
    }
}
