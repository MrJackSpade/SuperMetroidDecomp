namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A two-dimensional signed native 8.8 per-frame velocity: one signed word per axis holding
/// eight whole and eight fractional bits, as Samus projectiles and the grapple endpoint store
/// them. Each axis wraps at sixteen bits.
/// </summary>
/// <typeparam name="TSpace">The coordinate space the velocity is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y 8.8 words.")]
public readonly record struct Vector88<TSpace>(short X, short Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No velocity.</summary>
    public static Vector88<TSpace> Zero => default;

    /// <summary>Replaces the X word.</summary>
    public Vector88<TSpace> WithX(short x) => this with { X = x };

    /// <summary>Replaces the Y word.</summary>
    public Vector88<TSpace> WithY(short y) => this with { Y = y };

    /// <summary>Adds two velocities (an acceleration step), wrapping each word at sixteen bits.</summary>
    public static Vector88<TSpace> operator +(Vector88<TSpace> left, Vector88<TSpace> right) =>
        new(unchecked((short)(left.X + right.X)), unchecked((short)(left.Y + right.Y)));
}
