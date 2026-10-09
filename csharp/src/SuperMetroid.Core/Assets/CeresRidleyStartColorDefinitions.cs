namespace SuperMetroid.Core.Assets;

/// <summary>
/// Additional OBJ palettes at $A6:E16F-E1AE, copied together by $A6:A245-A24E.
/// The first palette repeats the reviewed normal door/container material at
/// $A6:F4EC. The second repeats the Baby initial colors at $A6:E1F1; their paint
/// inputs remain owned by the separately reviewed CeresBabyPaintDefinitions.
/// Both transparent entries retain the selected copied clear-black target value;
/// transparency itself does not require RGB zero. Independent supplied edits,
/// including edits to either occurrence of the Baby colors, remain separate.
/// </summary>
internal sealed class CeresRidleyStartColorDefinitions
{
    /// <summary>Number of RGB5 words in each of the two copied OBJ palettes.</summary>
    private const int PaletteSize = 16;

    /// <summary>Reviewed normal door/container colors used to calculate the first copied palette.</summary>
    private readonly CeresDoorNormalPaintDefinitions door;

    /// <summary>Separately owned Baby colors used to calculate the second copied palette.</summary>
    private readonly CeresBabyPaintDefinitions baby;

    /// <summary>Supplied color words retained only where they differ from the calculated defaults.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds the two palette definitions, retaining any supplied per-entry differences as independent edits.</summary>
    /// <param name="colors">The 32 selected RGB5 words for the copied door and Baby palettes.</param>
    /// <param name="initialBaby">Baby palette source used to calculate the second copied palette.</param>
    internal CeresRidleyStartColorDefinitions(ReadOnlySpan<ushort> colors, CeresBabyPaintDefinitions initialBaby)
    {
        if (colors.Length != 2 * PaletteSize)
            throw new ArgumentException("Ceres additional palettes require 32 colors and fifteen Baby colors.");
        door = new(colors.Slice(1, PaletteSize - 1));
        baby = initialBaby;
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) edits.Add(color, colors[color]);
    }

    /// <summary>Returns an authored replacement or the calculated color for one entry in the combined palettes.</summary>
    /// <param name="color">Index from 0 through 31 across the door palette followed by the Baby palette.</param>
    /// <returns>The selected RGB5 word, preserving each palette's independently supplied entries.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the two 16-color palettes.</exception>
    internal ushort Resolve(int color)
    {
        if ((uint)color >= 2 * PaletteSize) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(color, out ushort edited) ? edited : Calculate(color);
    }

    /// <summary>Calculates the unchanged palette word from the reviewed door and Baby source definitions.</summary>
    /// <param name="color">Index in the combined two-palette sequence.</param>
    /// <returns>Clear black at each palette's transparent entry, otherwise the corresponding source color.</returns>
    private ushort Calculate(int color) => color switch
    {
        0 or PaletteSize => 0,
        < PaletteSize => door.ColorAt(color - 1),
        _ => baby.Resolve(0, color - PaletteSize - 1),
    };
}
