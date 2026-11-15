namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// How one axis of a room state's background follows the camera (#2046): it copies the foreground, keeps its place, or
/// trails the foreground at a parallax rate.
/// </summary>
public abstract record BackgroundAxisScroll
{
    /// <summary>Closes the hierarchy to the choices declared here.</summary>
    private BackgroundAxisScroll()
    {
    }

    /// <summary>The axis copies the foreground's position.</summary>
    public sealed record Follow : BackgroundAxisScroll
    {
        /// <inheritdoc />
        internal override int? BackgroundPositionFor(int foregroundPosition) => foregroundPosition;
    }

    /// <summary>The axis keeps the background's position and view wherever they were: the camera never moves it.</summary>
    public sealed record Hold : BackgroundAxisScroll
    {
        /// <inheritdoc />
        internal override int? BackgroundPositionFor(int foregroundPosition) => null;
    }

    /// <summary>The axis trails the foreground at <paramref name="Rate"/> of its movement.</summary>
    /// <param name="Rate">The share of the foreground's movement the background moves by.</param>
    public sealed record Parallax(ParallaxRate Rate) : BackgroundAxisScroll
    {
        /// <inheritdoc />
        internal override int? BackgroundPositionFor(int foregroundPosition) => Rate.Trail(foregroundPosition);
    }

    /// <summary>An axis that copies the foreground's position.</summary>
    public static BackgroundAxisScroll FollowsForeground { get; } = new Follow();

    /// <summary>An axis the camera never moves.</summary>
    public static BackgroundAxisScroll Fixed { get; } = new Hold();

    /// <summary>An axis that trails the foreground at <paramref name="rate"/> of its movement.</summary>
    /// <param name="rate">The share of the foreground's movement the background moves by.</param>
    public static BackgroundAxisScroll ParallaxAt(ParallaxRate rate) => new Parallax(rate);

    /// <summary>
    /// The background's position on this axis when the foreground stands at <paramref name="foregroundPosition"/>, or
    /// <see langword="null"/> when the axis keeps the background where it is.
    /// </summary>
    /// <param name="foregroundPosition">The foreground's position on the axis, not negative.</param>
    internal abstract int? BackgroundPositionFor(int foregroundPosition);
}
