using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What a Phantoon flame circling its body keeps: where on its circle it is, how far it turns each update, the circle's
/// radius, and, for an intro flame, how long it circles before its circle starts closing.
/// </summary>
/// <param name="angle">The flame's starting angle around the body.</param>
internal sealed class PhantoonOrbitingFlameState(TrigonometryAngle angle) : EnemyProjectileState
{
    /// <summary>The flame's angle around Phantoon's body.</summary>
    internal TrigonometryAngle Angle { get; set; } = angle;

    /// <summary>The table units a rage flame turns each update, negative when it circles the other way.</summary>
    internal int AngleStep { get; init; }

    /// <summary>
    /// The circle's radius in pixels, zero or more: an enraged or spiral flame's grows each update for as long as it
    /// stays in the room, and an intro flame's closes to zero. The flame is placed by the short circle product, which
    /// uses the radius's place within 256 (<see cref="ShortCircleProduct.RadiusInUse"/>), so a radius past 255 places
    /// the flame as its remainder does (#1627).
    /// </summary>
    internal int Radius
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>Updates an intro flame circles at its full radius before the circle starts closing.</summary>
    internal int StartDelay
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
