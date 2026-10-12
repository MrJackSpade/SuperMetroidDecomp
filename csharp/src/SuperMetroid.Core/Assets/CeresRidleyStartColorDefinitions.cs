using SuperMetroid.Core.Hardware;

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
    /// <summary>Defines palette size.</summary>
    private const int PaletteSize = 16;
    /// <summary>Stores door.</summary>
    private readonly CeresDoorNormalPaintDefinitions door;
    /// <summary>Stores baby.</summary>
    private readonly CeresBabyPaintDefinitions baby;
    /// <summary>Stores edits.</summary>
    private readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Initializes a new ceres ridley start color definitions instance.</summary>
    internal CeresRidleyStartColorDefinitions(ReadOnlySpan<Bgr555> colors, CeresBabyPaintDefinitions initialBaby)
    {
        if (colors.Length != 2 * PaletteSize)
            throw new ArgumentException("Ceres additional palettes require 32 colors and fifteen Baby colors.");
        door = new(colors.Slice(1, PaletteSize - 1));
        baby = initialBaby;
        for (int color = 0; color < colors.Length; color++)
            if (Calculate(color) != colors[color]) edits.Add(color, colors[color]);
    }

    /// <summary>Resolve.</summary>
    internal Bgr555 Resolve(int color)
    {
        if ((uint)color >= 2 * PaletteSize) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(color, out Bgr555 edited) ? edited : Calculate(color);
    }

    /// <summary>Calculate.</summary>
    private Bgr555 Calculate(int color) => color switch
    {
        0 or PaletteSize => Bgr555.Black,
        < PaletteSize => door.ColorAt(color - 1),
        _ => baby.Resolve(0, color - PaletteSize - 1),
    };
}
