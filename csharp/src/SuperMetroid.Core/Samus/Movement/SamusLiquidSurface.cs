namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The liquid surface Samus's physics reads (#2560): a closed choice of none, a water surface with whether her movement
/// ignores it until a room object enables it, and a lava or acid surface. A room has at most one; a surface a tide carries
/// above the room is none.
/// </summary>
public abstract record SamusLiquidSurface
{
    /// <summary>Only the three cases below are surfaces.</summary>
    private SamusLiquidSurface()
    {
    }

    /// <summary>No liquid surface.</summary>
    public static SamusLiquidSurface None { get; } = new NoSurface();

    /// <summary>No liquid surface.</summary>
    private sealed record NoSurface : SamusLiquidSurface;

    /// <summary>A water surface at <paramref name="Line"/>.</summary>
    /// <param name="Line">The surface's room Y, a line of the room, zero or more.</param>
    /// <param name="IgnoredUntilEnabled">
    /// Whether Samus's movement ignores the surface until a room object enables it (the n00b tube's glass), Samus's copy
    /// of <see cref="Fx.RoomWater.IgnoresSurfaceUntilEnabled"/>. While set, water does not affect her even when the
    /// geometric surface comparison says she is below it.
    /// </param>
    public sealed record Water(int Line, bool IgnoredUntilEnabled) : SamusLiquidSurface
    {
        /// <summary>The surface's room Y, a line of the room, zero or more.</summary>
        public int Line { get; } = Ensure.AtLeastZero(Line);
    }

    /// <summary>A lava or acid surface at <paramref name="Line"/>.</summary>
    /// <param name="Line">The surface's room Y, a line of the room, zero or more.</param>
    public sealed record LavaOrAcid(int Line) : SamusLiquidSurface
    {
        /// <summary>The surface's room Y, a line of the room, zero or more.</summary>
        public int Line { get; } = Ensure.AtLeastZero(Line);
    }
}
