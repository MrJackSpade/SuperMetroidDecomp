namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Native Kraid second-phase position-dependent target/timer distributions.</summary>
public static class KraidMovementChoices
{
    /// <summary>The named second-phase destinations: Kraid's X positions in room pixels.</summary>
    private static class Destination
    {
        /// <summary>The leftmost second-phase destination.</summary>
        internal const ushort Leftmost = 208;
        /// <summary>The left destination with a distinct long pause.</summary>
        internal const ushort Left = 240;
        /// <summary>The middle destination.</summary>
        internal const ushort Middle = 320;
        /// <summary>The initial position and default movement-policy row.</summary>
        internal const ushort Starting = 352;
        /// <summary>The usual half-probability destination.</summary>
        internal const ushort Right = 368;
        /// <summary>The rightmost destination.</summary>
        internal const ushort Rightmost = 384;
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
    public static KraidMovementChoice Select(ushort position, int random)
    {
        // The $A7:BA7D record search ends on the $0160 row for every unlisted position.
        ushort row = position is Destination.Leftmost or Destination.Left or Destination.Middle or Destination.Starting or
            Destination.Right or Destination.Rightmost
            ? position
            : Destination.Starting;
        int choice = Math.Min((random & 28) >> 2, 4);
        ushort target = choice switch
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
        return new KraidMovementChoice(target, timer);
    }
}
