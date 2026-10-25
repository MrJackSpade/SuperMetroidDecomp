namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Yard's native eight-direction crawling sign selection.</summary>
public static class YardVelocityDefinitions
{

    /// <summary>Equivalent to native XOR $FFFF then increment; negation wraps at sixteen bits.</summary>
    public static EightEightVector<RoomPixels> Apply(int magnitude, int direction)
    {
        var positive = new SnesSignedEightEight(checked((ushort)magnitude));
        var negative = new SnesSignedEightEight(unchecked((ushort)-magnitude));
        return direction switch
        {
            0 or 4 => new(negative, negative),
            1 or 6 => new(negative, positive),
            2 or 5 => new(positive, negative),
            3 or 7 => new(positive, positive),
            _ => throw new InvalidDataException($"Yard direction ${direction:X4} exceeds the eight native sign records."),
        };
    }
}
