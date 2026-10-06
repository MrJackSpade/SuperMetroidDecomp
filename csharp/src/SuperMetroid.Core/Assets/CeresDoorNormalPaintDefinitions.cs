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

    private readonly CeresDoorWarmTargetPaintDefinitions warm;
    internal CeresDoorWarmTargetPaintDefinitions WarmTargets => warm;
    private readonly Dictionary<int, int> paint = [];
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresDoorNormalPaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 15) throw new ArgumentException("Normal door paint requires fifteen colors.", nameof(colors));
        warm = new(colors.Slice(8, 6));
        for (int slot = 1; slot <= colors.Length; slot++)
            for (int channel = 0; channel < 3; channel++)
                if (StoresPaint(slot, channel))
                    paint.Add(slot * 3 + channel, (colors[slot - 1] >> (5 * channel)) & MaximumChannel);
        for (int slot = 1; slot <= colors.Length; slot++)
            if (Calculate(slot) != colors[slot - 1]) edits.Add(slot, colors[slot - 1]);
    }

    private static bool StoresPaint(int slot, int channel) => channel switch
    {
        0 => false,
        1 => slot is 1 or 2 or 3 or 5 or 6 or 7,
        2 => slot is 3 or 4 or 5 or 6 or 7 or 8 or 15,
        _ => false,
    };

    private int Seed(int slot, int channel) => paint[slot * 3 + channel];

    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 15) throw new IndexOutOfRangeException();
        int slot = index + 1;
        return edits.TryGetValue(slot, out ushort color) ? color : Calculate(slot);
    }

    private ushort Calculate(int slot)
    {
        if (slot is >= 9 and <= 14) return warm.ColorAt(slot - 9);
        int green = slot == 15 ? MaximumChannel : slot == 4 ? 0 : slot == 8 ? Seed(7, 1) : Seed(slot, 1);
        int blue = slot is 1 or 2 ? Math.Min(MaximumChannel, green + HighlightBlueTint) : Seed(slot, 2);
        return (ushort)(green << 5 | blue << 10);
    }
}