namespace SuperMetroid.Core.Game;

/// <summary>Yard's native eight-direction crawling sign selection.</summary>
public static class YardVelocityDefinitions
{

    /// <summary>Equivalent to native XOR $FFFF then increment; negation wraps at sixteen bits.</summary>
    public static (ushort X, ushort Y) Apply(ushort magnitude, ushort direction)
    {
        ushort negative = unchecked((ushort)-magnitude);
        return direction switch
        {
            0 or 4 => (negative, negative),
            1 or 6 => (negative, magnitude),
            2 or 5 => (magnitude, negative),
            3 or 7 => (magnitude, magnitude),
            _ => throw new InvalidDataException($"Yard direction ${direction:X4} exceeds the eight native sign records."),
        };
    }
}
