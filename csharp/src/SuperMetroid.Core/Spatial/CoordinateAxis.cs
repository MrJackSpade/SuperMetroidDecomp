namespace SuperMetroid.Core.Spatial;

/// <summary>
/// One axis of the wrapping coordinate family: the per-axis form of the wrap <see cref="Position16{TSpace}"/> and
/// <see cref="Offset16{TSpace}"/> own, for code that computes a single coordinate before it reaches a position.
/// </summary>
/// <remarks>
/// Room, scroll, screen and grid coordinates wrap at sixteen bits per axis, and the game's behaviour depends on it
/// (<c>docs/spatial-types.md</c>): a coordinate computed past the top or left of the space continues from the far end,
/// so a probe above a room reads as a large coordinate that the later bounds checks reject; the distance between two
/// coordinates is their wrapped difference read as signed, so an actor that has wrapped past the room's edge still lies
/// a small distance away; and a coordinate stored above the origin reads as a negative offset in the sign tests that
/// clamp cameras and despawn actors. Every axis computation that the game wraps states it here, once.
/// </remarks>
[EncodingOwner("sixteen-bit wrapping coordinate axis")]
public static class CoordinateAxis
{
    /// <summary>
    /// The coordinate a computation lands on: a result past either end of the sixteen-bit space continues from the
    /// other end, 0 through 65535.
    /// </summary>
    /// <param name="coordinate">The computed coordinate, which may lie outside the space.</param>
    public static int Wrap(int coordinate) => unchecked((ushort)coordinate);

    /// <summary>A coordinate computed in thirty-two-bit unsigned arithmetic, wrapped onto the axis, 0 through 65535.</summary>
    /// <param name="coordinate">The computed coordinate, which may lie past either end of the axis.</param>
    public static int Wrap(uint coordinate) => unchecked((ushort)coordinate);

    /// <summary>
    /// The signed distance from <paramref name="from"/> to <paramref name="to"/> across the wrapping space,
    /// -32768 through 32767: the coordinates' difference wrapped to sixteen bits and read as signed.
    /// </summary>
    /// <param name="to">The coordinate measured to.</param>
    /// <param name="from">The coordinate measured from.</param>
    public static int Difference(int to, int from) => unchecked((short)(to - from));

    /// <summary>
    /// A coordinate read as a signed offset from the space's origin, -32768 through 32767: a coordinate stored above
    /// or left of the origin has wrapped to a large word and reads negative.
    /// </summary>
    /// <param name="coordinate">The coordinate or computed displacement.</param>
    public static int AsSigned(int coordinate) => unchecked((short)coordinate);

    /// <summary>A coordinate computed in thirty-two-bit unsigned arithmetic, read as a signed position on the axis.</summary>
    /// <param name="coordinate">The computed coordinate; only its low sixteen bits are read.</param>
    public static int AsSigned(uint coordinate) => unchecked((short)coordinate);

    /// <summary>
    /// The distance between two coordinates regardless of direction, 0 through 32768: the magnitude of their wrapped
    /// signed <see cref="Difference"/>. The farthest wrapped difference, -32768, keeps its magnitude 32768.
    /// </summary>
    /// <param name="to">The coordinate measured to.</param>
    /// <param name="from">The coordinate measured from.</param>
    public static int Distance(int to, int from) => Math.Abs(Difference(to, from));
}
