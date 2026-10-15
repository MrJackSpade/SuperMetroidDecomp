namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Reusable translation of <c>$82:DA02/$82:DA4A/$82:DAA6</c>'s current-to-target CGRAM
/// interpolation. Death, doors, maps, and several bosses all call this same cartridge math.
/// </summary>
internal sealed class CartridgePaletteTransition
{
    /// <summary>Copied 256-color destination used across the fade.</summary>
    private readonly Bgr555[] target;
    /// <summary>Native divisor controlling how far each interpolation call moves.</summary>
    private readonly int denominator;
    /// <summary>Shared WRAM fade counter advanced and reset by this transition.</summary>
    private readonly GradualColorChangeCounter numerator;

    /// <summary>Captures the destination palette and binds the native shared fade counter.</summary>
    /// <param name="target">All 256 destination CGRAM colors in native order; copied into the transition's target buffer.</param>
    /// <param name="denominator">Native interpolation divisor, one through 65534; transition number denominator plus one installs the exact target.</param>
    /// <param name="numerator">The shared <c>$7E:C400</c> transition number this fade advances.</param>
    public CartridgePaletteTransition(ReadOnlySpan<Bgr555> target, int denominator, GradualColorChangeCounter numerator)
    {
        Ensure.NotNull(numerator);
        Ensure.LengthEqual(target, CgramPalettes.ColorCount);
        if (denominator is <= 0 or > (ushort.MaxValue - 1))
            throw new ArgumentOutOfRangeException(nameof(denominator));
        this.target = target.ToArray();
        this.denominator = denominator;
        this.numerator = numerator;
    }

    /// <summary>
    /// Advances one native call. The first call (transition number zero) is intentionally a
    /// no-op, number denominator+1 reaches the exact target, and the following call returns
    /// complete while resetting the native counter.
    /// </summary>
    public bool Step(SnesCgram cgram, ushort paletteMask = ushort.MaxValue)
    {
        Ensure.NotNull(cgram);
        int transitionNumber = numerator.Numerator;
        if (transitionNumber > denominator + 1)
        {
            numerator.Numerator = 0;
            return true;
        }

        var palettes = new CgramPalettes(cgram);
        for (int row = 0; row < CgramPalettes.RowCount; row++)
        {
            // Each mask bit selects one 16-colour row, as the native row loop does.
            if ((paletteMask & (1 << row)) == 0)
                continue;
            CgramPalette palette = palettes.Row(row);
            for (int index = 0; index < palette.Count; index++)
            {
                Bgr555 current = palette[index];
                Bgr555 goal = target[row * palette.Count + index];
                if (current != goal)
                    palette.Write(index, CalculateColor(transitionNumber, current, goal));
            }
        }
        numerator.Numerator = unchecked((ushort)(transitionNumber + 1));
        return false;
    }

    /// <summary>Updates the native target buffer without resetting fade progress or current colors.</summary>
    internal void SetTargetColor(int index, Bgr555 color) => target[index] = color;

    /// <summary>Applies native fixed-point interpolation independently to each RGB5 channel.</summary>
    private Bgr555 CalculateColor(int step, Bgr555 current, Bgr555 destination) =>
        current.Zip(destination, (_, from, to) => CalculateComponent(step, from, to));

    /// <summary>Moves one channel toward its target using the remaining-step divisor and 8.8 truncation.</summary>
    private int CalculateComponent(int step, int current, int destination)
    {
        if (step == 0)
            return current;
        int zeroBasedStep = step - 1;
        if (zeroBasedStep == denominator)
            return destination;

        int fixedDelta = Math.Abs(destination - current) * 0x100 /
                         (denominator - zeroBasedStep);
        if (destination < current)
            fixedDelta = -fixedDelta;
        return SnesEightEight.WholeOf(SnesEightEight.FromWhole(current) + fixedDelta);
    }
}
