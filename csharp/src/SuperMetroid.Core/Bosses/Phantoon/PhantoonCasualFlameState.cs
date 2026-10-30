using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What one of Phantoon's casual flames keeps once it has landed: how long it rests at the floor, then how many times
/// it has bounced.
/// </summary>
internal sealed class PhantoonCasualFlameState : EnemyProjectileState
{
    /// <summary>Updates the landed flame pauses at the floor before it starts bouncing.</summary>
    private const int LandingPause = 8;

    /// <summary>Updates left in the flame's pause at the floor; it starts bouncing on the update this reaches zero.</summary>
    internal int ImpactPause
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    } = LandingPause;

    /// <summary>How many times the bouncing flame has struck the floor.</summary>
    internal int Bounces
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
