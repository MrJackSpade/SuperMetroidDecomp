namespace SuperMetroid.Core.Game;

/// <summary>Native Kraid second-phase position-dependent target/timer distributions.</summary>
public static class KraidMovementChoices
{
    /// <summary>$A7:BA89 position $D0, the leftmost second-phase destination.</summary>
    private const ushort Leftmost = 0x00d0;
    /// <summary>$A7:BA7D position $F0, the left destination with a distinct long pause.</summary>
    private const ushort Left = 0x00f0;
    /// <summary>$A7:BA8D position $140, the middle destination.</summary>
    private const ushort Middle = 0x0140;
    /// <summary>$A7:BA81 position $160, initial position and default movement-policy row.</summary>
    private const ushort Starting = 0x0160;
    /// <summary>$A7:BA91 position $170, the usual half-probability destination.</summary>
    private const ushort Right = 0x0170;
    /// <summary>$A7:BA85 position $180, the rightmost destination.</summary>
    private const ushort Rightmost = 0x0180;

    /// <summary>
    /// $A7:BA7D position/pointer records and BA95..BB0C target/timer pairs.
    /// Unlisted positions select the 0160 row. RNG choices four through seven
    /// all select its final record, preserving the native half-probability choice.
    /// </summary>
    /// <remarks>
    /// Position and weighted branch select named movement destinations, not a sampled
    /// numerical curve. The pause policy distinguishes the first two branches and the
    /// third branch at the leftmost, middle and right positions. Independently checked
    /// against all native pairs and both complete selector domains for #1165.
    /// </remarks>
    public static (ushort TargetX, ushort ThinkTimer) Select(ushort position, ushort random)
    {
        int choice = Math.Min((random & 0x1c) >> 2, 4);
        ushort target = choice switch
        {
            0 => position == Left ? Rightmost : Left,
            1 => position is Rightmost or Leftmost or Middle ? Starting : Rightmost,
            2 => position is Leftmost or Middle ? Rightmost : position == Right ? Right : Leftmost,
            3 => position == Left ? Right : position is Middle or Right ? Leftmost : Middle,
            _ => position == Right ? Middle : Right,
        };
        ushort timer = choice switch
        {
            0 => position == Left ? (ushort)344 : (ushort)256,
            1 => 344,
            2 when position is Leftmost or Middle or Right => 344,
            _ => 44,
        };
        return (target, timer);
    }
}
