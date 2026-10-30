using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What a Phantoon flame circling its body keeps: where on its circle it is, how far it turns each update, the circle's
/// radius, and, for an intro flame, how long it circles before its circle starts closing.
/// </summary>
/// <param name="angle">The flame's starting angle around the body.</param>
internal sealed class PhantoonOrbitingFlameState(ByteAngle angle) : EnemyProjectileState
{
    /// <summary>How many radius values the eight-bit radius holds before it wraps.</summary>
    private const int RadiusRange = 256;

    /// <summary>The flame's angle around Phantoon's body.</summary>
    internal ByteAngle Angle { get; set; } = angle;

    /// <summary>The table units a rage flame turns each update, negative when it circles the other way.</summary>
    internal int AngleStep { get; init; }

    /// <summary>The circle's radius in pixels, 0 through 255; it grows by wrapping past 255 back to zero.</summary>
    internal int Radius
    {
        get;
        set => field = ((value % RadiusRange) + RadiusRange) % RadiusRange;
    }

    /// <summary>Updates an intro flame circles at its full radius before the circle starts closing.</summary>
    internal int StartDelay
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
