using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Native Kraid second-phase position-dependent target/timer distributions.</summary>
public static class KraidMovementChoices
{
    /// <summary>A named second-phase destination: one of Kraid's X positions in room pixels.</summary>
    /// <param name="X">The destination's X position in room pixels.</param>
    private sealed record Destination(int X)
    {
        /// <summary>The leftmost second-phase destination.</summary>
        internal static readonly Destination Leftmost = new(208);
        /// <summary>The left destination with a distinct long pause.</summary>
        internal static readonly Destination Left = new(240);
        /// <summary>The middle destination.</summary>
        internal static readonly Destination Middle = new(320);
        /// <summary>The initial position and default movement-policy row.</summary>
        internal static readonly Destination Starting = new(352);
        /// <summary>The usual half-probability destination.</summary>
        internal static readonly Destination Right = new(368);
        /// <summary>The rightmost destination.</summary>
        internal static readonly Destination Rightmost = new(384);

        /// <summary>The position records the movement-policy search runs over, in search order.</summary>
        internal static readonly ImmutableArray<Destination> Records = [Leftmost, Left, Middle, Starting, Right, Rightmost];
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
        // The $A7:BA7D record search ends on the starting row for every unlisted position.
        Destination row = Destination.Records.FirstOrDefault(record => record.X == position) ?? Destination.Starting;
        int choice = Math.Min((random & 28) >> 2, 4);
        Destination target = choice switch
        {
            0 => row == Destination.Left ? Destination.Rightmost : Destination.Left,
            1 => row == Destination.Rightmost || row == Destination.Leftmost || row == Destination.Middle
                ? Destination.Starting : Destination.Rightmost,
            2 => row == Destination.Leftmost || row == Destination.Middle ? Destination.Rightmost
                : row == Destination.Right ? Destination.Right : Destination.Leftmost,
            3 => row == Destination.Left ? Destination.Right
                : row == Destination.Middle || row == Destination.Right ? Destination.Leftmost : Destination.Middle,
            _ => row == Destination.Right ? Destination.Middle : Destination.Right,
        };
        ushort timer = choice switch
        {
            0 => row == Destination.Left ? (ushort)344 : (ushort)256,
            1 => 344,
            2 when row == Destination.Leftmost || row == Destination.Middle || row == Destination.Right => 344,
            _ => 44,
        };
        return new KraidMovementChoice(target.X, timer);
    }
}
