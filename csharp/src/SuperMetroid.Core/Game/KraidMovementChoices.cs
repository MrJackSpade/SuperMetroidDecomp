namespace SuperMetroid.Core.Game;

/// <summary>Native Kraid second-phase position-dependent target/timer distributions.</summary>
public static class KraidMovementChoices
{
    /// <summary>The named second-phase destinations, valued by X position.</summary>
    private enum Destination : ushort
    {
        /// <summary>$A7:BA89 position $D0, the leftmost second-phase destination.</summary>
        Leftmost = 0x00d0,
        /// <summary>$A7:BA7D position $F0, the left destination with a distinct long pause.</summary>
        Left = 0x00f0,
        /// <summary>$A7:BA8D position $140, the middle destination.</summary>
        Middle = 0x0140,
        /// <summary>$A7:BA81 position $160, initial position and default movement-policy row.</summary>
        Starting = 0x0160,
        /// <summary>$A7:BA91 position $170, the usual half-probability destination.</summary>
        Right = 0x0170,
        /// <summary>$A7:BA85 position $180, the rightmost destination.</summary>
        Rightmost = 0x0180,
    }

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
        // The $A7:BA7D record search ends on the $0160 row for every unlisted position.
        Destination row = Enum.IsDefined((Destination)position) ? (Destination)position : Destination.Starting;
        int choice = Math.Min((random & 0x1c) >> 2, 4);
        Destination target = choice switch
        {
            0 => row == Destination.Left ? Destination.Rightmost : Destination.Left,
            1 => row is Destination.Rightmost or Destination.Leftmost or Destination.Middle
                ? Destination.Starting : Destination.Rightmost,
            2 => row is Destination.Leftmost or Destination.Middle ? Destination.Rightmost
                : row == Destination.Right ? Destination.Right : Destination.Leftmost,
            3 => row == Destination.Left ? Destination.Right
                : row is Destination.Middle or Destination.Right ? Destination.Leftmost : Destination.Middle,
            _ => row == Destination.Right ? Destination.Middle : Destination.Right,
        };
        ushort timer = choice switch
        {
            0 => row == Destination.Left ? (ushort)344 : (ushort)256,
            1 => 344,
            2 when row is Destination.Leftmost or Destination.Middle or Destination.Right => 344,
            _ => 44,
        };
        return ((ushort)target, timer);
    }
}
