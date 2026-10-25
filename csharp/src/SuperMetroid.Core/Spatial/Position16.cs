namespace SuperMetroid.Core.Spatial;

/// <summary>An unsigned sixteen-bit position whose axes wrap at the native word boundary.</summary>
/// <remarks>
/// This type is the one owner of the room-coordinate wrap: room, scroll and grid positions wrap at sixteen bits per
/// axis, and the game's behaviour at room edges, in door transitions and in the reproduced glitches depends on it
/// (<c>docs/spatial-types.md</c>). Its translation operators are the only place that wrap is computed.
/// </remarks>
/// <typeparam name="TSpace">The coordinate space; positions in different spaces never convert implicitly.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y words.")]
[SnesEncodingOwner]
public readonly record struct Position16<TSpace>([property: IntegerQuantity("horizontal coordinate")] ushort X, [property: IntegerQuantity("vertical coordinate")] ushort Y)
    where TSpace : ISpatialSpace
{
    /// <summary>The origin.</summary>
    public static Position16<TSpace> Zero => default;

    /// <summary>Replaces the X axis with a coordinate that must already be a sixteen-bit word.</summary>
    public Position16<TSpace> WithX(int x) => this with { X = checked((ushort)x) };

    /// <summary>Replaces the Y axis with a coordinate that must already be a sixteen-bit word.</summary>
    public Position16<TSpace> WithY(int y) => this with { Y = checked((ushort)y) };

    /// <summary>Translates by a signed offset, wrapping each axis at sixteen bits.</summary>
    public static Position16<TSpace> operator +(Position16<TSpace> position, Offset16<TSpace> offset) =>
        new(unchecked((ushort)(position.X + offset.X)), unchecked((ushort)(position.Y + offset.Y)));

    /// <summary>Translates by the negated offset, wrapping each axis at sixteen bits.</summary>
    public static Position16<TSpace> operator -(Position16<TSpace> position, Offset16<TSpace> offset) =>
        new(unchecked((ushort)(position.X - offset.X)), unchecked((ushort)(position.Y - offset.Y)));

    /// <summary>The wrapped signed delta from <paramref name="origin"/> to <paramref name="position"/>.</summary>
    public static Offset16<TSpace> operator -(Position16<TSpace> position, Position16<TSpace> origin) =>
        new(unchecked((short)(position.X - origin.X)), unchecked((short)(position.Y - origin.Y)));
}
