namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Signs a Yard's crawling speed for its direction.</summary>
public static class YardCrawlVelocity
{
    /// <summary>Equivalent to native XOR $FFFF then increment; negation wraps at sixteen bits.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is outside the eight sign records.</exception>
    public static EightEightVector<RoomPixels> Apply(int magnitude, int direction)
    {
        if ((uint)direction >= (uint)YardVelocityDefinitions.XSigns.Length)
            throw new InvalidDataException($"Yard direction ${direction:X4} exceeds the eight native sign records.");
        var positive = new SignedEightEight(checked((ushort)magnitude));
        var negative = new SignedEightEight(unchecked((ushort)-magnitude));
        return new(YardVelocityDefinitions.XSigns[direction] < 0 ? negative : positive,
            YardVelocityDefinitions.YSigns[direction] < 0 ? negative : positive);
    }
}
