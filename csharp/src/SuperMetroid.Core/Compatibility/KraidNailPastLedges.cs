namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where Kraid's body pushes back a fingernail that flies higher than his four contour ledges reach (#1500). A nail is
/// pushed back by the ledge level with it: the contour walk tests each ledge's bound against the nail's height above
/// Kraid's body and takes the first ledge whose bound lies above the nail. A nail 128 or more pixels above the body passes
/// all four ledges, and the game goes on to read the record that follows them, so such a nail is pushed back
/// at that record's offset, which lies far beyond Kraid's right side, as long as it lies below that record's bound. The
/// import pipeline decodes the record's offset and bound once from the player's game data.
/// </summary>
/// <remarks>
/// Consulted by a flying nail's update, after its terrain moves and only when no wall reflected it, each time the nail
/// lies above every ledge. Evidence: the original contour walk tests the words of its offset and bound table until a
/// bound lies above the nail, with no count; the nail contour check decodes the record after the ledges from the
/// cartridge and compares it with this service. Nails thrown upward fly through the upper screen of Kraid's room, more
/// than 128 pixels above his body, so the record is read in play.
/// </remarks>
public sealed partial class KraidNailPastLedges
{

    /// <summary>
    /// How far above Kraid's body a nail can fly: terrain reflects a nail inside Kraid's room, two screens tall, and his
    /// body stays in the same room, so a nail lies fewer than this many pixels above it.
    /// </summary>
    private const int NailReachAboveBody = 512;

    /// <summary>
    /// Keeps the decoded record that follows the ledges, whose bound lies above every height a nail reaches, so a nail
    /// past the ledges is always pushed back at its offset: the walk never reads the record after it.
    /// </summary>
    /// <param name="leftOffset">The offset from Kraid's body's X, -32768 through 32767.</param>
    /// <param name="bound">The bound on the nail's height relative to the body, -32768 through 32767.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value lies outside -32768 through 32767.</exception>
    /// <exception cref="InvalidDataException">The bound lies within a nail's reach above Kraid's body.</exception>
    public KraidNailPastLedges(int leftOffset, int bound)
    {
        LeftOffset = CoordinateAxis.Wrap(Ensure.BetweenInclusive(short.MinValue, short.MaxValue, leftOffset));
        int wrappedBound = CoordinateAxis.Wrap(Ensure.BetweenInclusive(short.MinValue, short.MaxValue, bound));
        if (CoordinateAxis.Difference(wrappedBound, -NailReachAboveBody) > 0)
        {
            throw new InvalidDataException(
                $"The contour record after Kraid's ledges bounds nails at {bound}, within the {NailReachAboveBody} pixels a nail reaches above his body.");
        }
    }

    /// <summary>
    /// The offset from Kraid's body's X at which a nail higher than every ledge is pushed back, as a wrapped room
    /// coordinate: every such nail lies below the record's bound.
    /// </summary>
    public int LeftOffset { get; }
}
