namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Signs a Yard's crawling speed for its direction.</summary>
public static class YardCrawlVelocity
{
    /// <summary>Equivalent to native XOR $FFFF then increment; negation wraps at sixteen bits.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is outside the eight sign records.</exception>
    public static EightEightVector<RoomPixels> Apply(int magnitude, int direction)
    {
        if ((uint)direction >= (uint)YardVelocityDefinitions.Headings.Length)
            throw new InvalidDataException($"Yard direction ${direction:X4} exceeds the eight native sign records.");
        var positive = new SignedEightEight(checked((ushort)magnitude));
        var negative = new SignedEightEight(unchecked((ushort)-magnitude));
        YardCrawlHeading heading = YardVelocityDefinitions.Headings[direction];
        return new(heading.Horizontal == HorizontalDirection.Left ? negative : positive,
            heading.Vertical == VerticalDirection.Up ? negative : positive);
    }
}
