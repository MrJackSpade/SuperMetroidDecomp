using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Normal Ceres door paint at $A6:F4EE-F50B. Palette slots are one-based here.
/// Shared hue channels and the three-shade gold ramp calculate; the twenty
/// paint-channel seeds and selected hue/ramp policies specify categorical material paint.
/// Slots1..8 paint visible bevels/light/shadow. Slots9..15 are separately preserved
/// copied target paints, not claimed to be visible door pixels. The uniform native
/// copy at $A6:F704-F726 supplies no rule for choosing different material colors.
/// Supplied channel changes remain independent, including changes to shared seeds.
/// </summary>
internal sealed class CeresDoorNormalPaintDefinitions
{
    private const int MaximumChannel = (1 << 5) - 1;
    private const int HighlightBlueTint = 14;

    internal CeresDoorWarmTargetPaintDefinitions WarmTargets { get; }
    private readonly Dictionary<int, int> paint = [];
    private readonly Dictionary<int, Bgr555> edits = [];

    internal CeresDoorNormalPaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        if (colors.Length != 15) throw new ArgumentException("Normal door paint requires fifteen colors.", nameof(colors));
        WarmTargets = new(colors.Slice(8, 6));
        for (int slot = 1; slot <= colors.Length; slot++)
            foreach (ColorChannel channel in Enum.GetValues<ColorChannel>())
                if (StoresPaint(slot, channel))
                    paint.Add(slot * 3 + (int)channel, colors[slot - 1][channel]);
        for (int slot = 1; slot <= colors.Length; slot++)
            if (Calculate(slot) != colors[slot - 1]) edits.Add(slot, colors[slot - 1]);
    }

    private static bool StoresPaint(int slot, ColorChannel channel) => channel switch
    {
        ColorChannel.Red => false,
        ColorChannel.Green => slot is 1 or 2 or 3 or 5 or 6 or 7,
        ColorChannel.Blue => slot is 3 or 4 or 5 or 6 or 7 or 8 or 15,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    private int Seed(int slot, ColorChannel channel) => paint[slot * 3 + (int)channel];

    internal Bgr555 ColorAt(int index)
    {
        if ((uint)index >= 15) throw new IndexOutOfRangeException();
        int slot = index + 1;
        return edits.TryGetValue(slot, out Bgr555 color) ? color : Calculate(slot);
    }

    private Bgr555 Calculate(int slot)
    {
        if (slot is >= 9 and <= 14) return WarmTargets.ColorAt(slot - 9);
        int green = slot == 15 ? MaximumChannel : slot == 4 ? 0 : slot == 8 ? Seed(7, ColorChannel.Green) : Seed(slot, ColorChannel.Green);
        int blue = slot is 1 or 2 ? Math.Min(MaximumChannel, green + HighlightBlueTint) : Seed(slot, ColorChannel.Blue);
        return new(0, green, blue);
    }
}