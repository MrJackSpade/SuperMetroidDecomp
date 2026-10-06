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
    private const int PaletteSize = 16;
    private readonly CeresDoorNormalPaintDefinitions door;
    private readonly CeresBabyPaintDefinitions baby;
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresRidleyStartColorDefinitions(ReadOnlySpan<ushort> colors, CeresBabyPaintDefinitions initialBaby)
    {
        if (colors.Length != 2 * PaletteSize)
            throw new ArgumentException("Ceres additional palettes require 32 colors and fifteen Baby colors.");
        door = new(colors.Slice(1, PaletteSize - 1));
        baby = initialBaby;
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) edits.Add(color, colors[color]);
    }

    internal ushort Resolve(int color)
    {
        if ((uint)color >= 2 * PaletteSize) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(color, out ushort edited) ? edited : Calculate(color);
    }

    private ushort Calculate(int color) => color switch
    {
        0 or PaletteSize => 0,
        < PaletteSize => door.ColorAt(color - 1),
        _ => baby.Resolve(0, color - PaletteSize - 1),
    };
}
