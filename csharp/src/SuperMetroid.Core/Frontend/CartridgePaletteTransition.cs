using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Reusable translation of <c>$82:DA02/$82:DA4A/$82:DAA6</c>'s current-to-target CGRAM
/// interpolation. Death, doors, maps, and several bosses all call this same cartridge math.
/// </summary>
internal sealed class CartridgePaletteTransition
{
    private readonly ushort[] target;
    private readonly int denominator;
    private int transitionNumber;

    public CartridgePaletteTransition(ReadOnlySpan<ushort> target, int denominator)
    {
        if (target.Length != SnesCgram.ColorCount)
            throw new ArgumentException("A global palette transition requires 256 target colors.", nameof(target));
        if (denominator <= 0 || denominator > ushort.MaxValue - 1)
            throw new ArgumentOutOfRangeException(nameof(denominator));
        this.target = target.ToArray();
        this.denominator = denominator;
    }

    /// <summary>
    /// Advances one native call. The first call (transition number zero) is intentionally a
    /// no-op, number denominator+1 reaches the exact target, and the following call returns
    /// complete while resetting the native counter.
    /// </summary>
    public bool Step(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (transitionNumber > denominator + 1)
        {
            transitionNumber = 0;
            return true;
        }

        for (int color = 0; color < SnesCgram.ColorCount; color++)
        {
            ushort current = cgram.Colors[color];
            if (current != target[color])
                cgram.SetColor(color, CalculateColor(transitionNumber, current, target[color]));
        }
        transitionNumber++;
        return false;
    }

    /// <summary>Updates the native target buffer without resetting fade progress or current colors.</summary>
    internal void SetTargetColor(int index, ushort color) => target[index] = color;

    private ushort CalculateColor(int step, ushort current, ushort destination)
    {
        int red = CalculateComponent(step, current & 0x1f, destination & 0x1f);
        int green = CalculateComponent(step, (current >> 5) & 0x1f, (destination >> 5) & 0x1f);
        int blue = CalculateComponent(step, (current >> 10) & 0x1f, (destination >> 10) & 0x1f);
        return unchecked((ushort)(red | (green << 5) | (blue << 10)));
    }

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
        return ((current << 8) + fixedDelta) >> 8;
    }
}
