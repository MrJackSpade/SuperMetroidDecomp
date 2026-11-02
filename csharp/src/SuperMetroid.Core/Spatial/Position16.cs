namespace SuperMetroid.Core.Spatial;

/// <summary>An unsigned sixteen-bit position whose axes wrap at the native word boundary.</summary>
/// <remarks>
/// This type is the one owner of the room-coordinate wrap: room, scroll and grid positions wrap at sixteen bits per
/// axis, and the game's behaviour at room edges, in door transitions and in the reproduced glitches depends on it
/// (<c>docs/spatial-types.md</c>). Its translation operators and <see cref="Wrapped"/> are the only places that wrap
/// is computed for a whole position; <see cref="CoordinateAxis"/> states it for one axis.
/// </remarks>
/// <typeparam name="TSpace">The coordinate space; positions in different spaces never convert implicitly.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y words.")]
[EncodingOwner("sixteen-bit wrapping coordinate")]
public readonly record struct Position16<TSpace>
    where TSpace : ISpatialSpace
{
    /// <summary>The position at two coordinates that must already be sixteen-bit words, 0 through 65535.</summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    public Position16(int x, int y)
    {
        X = checked((ushort)x);
        Y = checked((ushort)y);
    }

    /// <summary>The horizontal coordinate, 0 through 65535.</summary>
    [IntegerQuantity("horizontal coordinate")]
    public ushort X { get; init; }

    /// <summary>The vertical coordinate, 0 through 65535.</summary>
    [IntegerQuantity("vertical coordinate")]
    public ushort Y { get; init; }

    /// <summary>The origin.</summary>
    public static Position16<TSpace> Zero => default;

    /// <summary>The position at two coordinates that must already be sixteen-bit words, 0 through 65535.</summary>
    public static Position16<TSpace> Of(int x, int y) => new(x, y);

    /// <summary>
    /// The position a computation lands on: each computed coordinate past either end of the space continues from the
    /// other end (<see cref="CoordinateAxis.Wrap"/>).
    /// </summary>
    public static Position16<TSpace> Wrapped(int x, int y) => new(CoordinateAxis.Wrap(x), CoordinateAxis.Wrap(y));

    /// <summary>Replaces the X axis with a coordinate that must already be a sixteen-bit word.</summary>
    public Position16<TSpace> WithX(int x) => this with { X = checked((ushort)x) };

    /// <summary>Replaces the Y axis with a coordinate that must already be a sixteen-bit word.</summary>
    public Position16<TSpace> WithY(int y) => this with { Y = checked((ushort)y) };

    /// <summary>Translates by a signed offset, wrapping each axis at sixteen bits.</summary>
    public static Position16<TSpace> operator +(Position16<TSpace> position, Offset16<TSpace> offset) =>
        Wrapped(position.X + offset.X, position.Y + offset.Y);

    /// <summary>Translates by the negated offset, wrapping each axis at sixteen bits.</summary>
    public static Position16<TSpace> operator -(Position16<TSpace> position, Offset16<TSpace> offset) =>
        Wrapped(position.X - offset.X, position.Y - offset.Y);

    /// <summary>The wrapped signed delta from <paramref name="origin"/> to <paramref name="position"/>.</summary>
    public static Offset16<TSpace> operator -(Position16<TSpace> position, Position16<TSpace> origin) =>
        new(CoordinateAxis.Difference(position.X, origin.X), CoordinateAxis.Difference(position.Y, origin.Y));
}
