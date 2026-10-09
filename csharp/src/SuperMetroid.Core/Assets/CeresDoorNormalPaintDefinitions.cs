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
    /// <summary>Largest channel value representable by the five-bit SNES color components.</summary>
    private const int MaximumChannel = (1 << 5) - 1;

    /// <summary>Blue-channel increase applied to the first two bevel highlights, capped at the five-bit channel maximum.</summary>
    private const int HighlightBlueTint = 14;

    /// <summary>Separate policy for the copied warm target colors in palette slots nine through fourteen.</summary>
    private readonly CeresDoorWarmTargetPaintDefinitions warm;

    internal CeresDoorWarmTargetPaintDefinitions WarmTargets => warm;

    /// <summary>Five-bit paint seeds keyed by one-based palette slot and color-channel index.</summary>
    private readonly Dictionary<int, int> paint = [];

    /// <summary>Original slot colors retained when the calculated categorical paint differs from the supplied native color.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds channel seeds for categorical paint and preserves source colors that the shared paint rules cannot reproduce.</summary>
    /// <param name="colors">The fifteen normal-door palette colors in slot order, encoded as SNES BGR555 values.</param>
    /// <exception cref="ArgumentException">The palette does not contain exactly fifteen colors.</exception>
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

    /// <summary>Identifies which slot/channel combinations have independently stored paint seeds instead of shared or calculated components.</summary>
    /// <param name="slot">One-based palette slot.</param>
    /// <param name="channel">Component index, where zero is red, one is green, and two is blue.</param>
    /// <returns>True when the component is recorded as a seed in the paint map.</returns>
    private static bool StoresPaint(int slot, int channel) => channel switch
    {
        0 => false,
        1 => slot is 1 or 2 or 3 or 5 or 6 or 7,
        2 => slot is 3 or 4 or 5 or 6 or 7 or 8 or 15,
        _ => false,
    };

    /// <summary>Returns the stored five-bit component used as the paint seed for a slot.</summary>
    /// <param name="slot">One-based palette slot.</param>
    /// <param name="channel">Component index in the slot's paint data.</param>
    /// <returns>The seeded channel value.</returns>
    private int Seed(int slot, int channel) => paint[slot * 3 + channel];

    /// <summary>Returns the normal-door color for a zero-based palette index, using preserved source paint or the categorical calculation as appropriate.</summary>
    /// <param name="index">Zero-based index into the fifteen supplied palette slots.</param>
    /// <returns>The BGR555 color for the requested slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the fifteen-slot palette.</exception>
    internal ushort ColorAt(int index)
    {
        if ((uint)index >= 15) throw new IndexOutOfRangeException();
        int slot = index + 1;
        return edits.TryGetValue(slot, out ushort color) ? color : Calculate(slot);
    }

    /// <summary>Calculates the color for a one-based slot from its stored channel seeds, bevel highlight policy, and warm-target palette.</summary>
    /// <param name="slot">One-based normal-door palette slot.</param>
    /// <returns>The calculated BGR555 color before any preserved source-color override is applied.</returns>
    private ushort Calculate(int slot)
    {
        if (slot is >= 9 and <= 14) return warm.ColorAt(slot - 9);
        int green = slot == 15 ? MaximumChannel : slot == 4 ? 0 : slot == 8 ? Seed(7, 1) : Seed(slot, 1);
        int blue = slot is 1 or 2 ? Math.Min(MaximumChannel, green + HighlightBlueTint) : Seed(slot, 2);
        return (ushort)(green << 5 | blue << 10);
    }
}
