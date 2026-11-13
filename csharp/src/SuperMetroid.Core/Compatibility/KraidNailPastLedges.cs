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
[CompatibilityService("a fingernail higher than Kraid's four contour ledges is pushed back at the record that follows them")]
public sealed class KraidNailPastLedges
{
    /// <summary>The offset from Kraid's body a nail past the ledges is pushed back at, as a wrapped room coordinate.</summary>
    private readonly int _leftOffset;

    /// <summary>The bound a nail past the ledges must lie below to be pushed at that offset, as a wrapped room coordinate.</summary>
    private readonly int _bound;

    /// <summary>Keeps the decoded record that follows the ledges.</summary>
    /// <param name="leftOffset">The offset from Kraid's body's X, -32768 through 32767.</param>
    /// <param name="bound">The bound on the nail's height relative to the body, -32768 through 32767.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value lies outside -32768 through 32767.</exception>
    public KraidNailPastLedges(int leftOffset, int bound)
    {
        _leftOffset = CoordinateAxis.Wrap(Ensure.BetweenInclusive(short.MinValue, short.MaxValue, leftOffset));
        _bound = CoordinateAxis.Wrap(Ensure.BetweenInclusive(short.MinValue, short.MaxValue, bound));
    }

    /// <summary>
    /// The offset from Kraid's body's X at which a nail <paramref name="relativeY"/> pixels below the body, higher than
    /// every ledge, is pushed back, as a wrapped room coordinate.
    /// </summary>
    /// <param name="relativeY">The nail's Y minus the body's Y, as a wrapped room coordinate.</param>
    /// <exception cref="ArgumentOutOfRangeException">The nail lies at or above the record's bound.</exception>
    public int LeftOffset(int relativeY)
    {
        if (CoordinateAxis.Difference(_bound, relativeY) < 0)
            return _leftOffset;

        // Rejected, not read: past this bound the walk would read one more record. A nail never gets there. Terrain
        // reflects a nail inside Kraid's room, two screens tall, and Kraid's body stays in the same room, so a nail lies
        // fewer than 512 pixels above the body, and the cartridge's bound lies more than 20000 pixels above it.
        throw new ArgumentOutOfRangeException(nameof(relativeY), relativeY,
            "A Kraid fingernail lies beyond the contour record that follows the ledges, which no nail in Kraid's room can reach.");
    }
}
