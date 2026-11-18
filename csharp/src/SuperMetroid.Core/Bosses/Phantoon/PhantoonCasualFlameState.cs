using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What one of Phantoon's casual flames keeps once it has landed: how long it rests at the floor, then how many times
/// it has bounced.
/// </summary>
internal sealed class PhantoonCasualFlameState : EnemyProjectileState
{
    /// <summary>The updates the landed flame pauses at the floor, the last being the one it starts bouncing on.</summary>
    private const int LandingPauseUpdates = 8;

    /// <summary>The flame's pause at the floor (#2469); it starts bouncing on the update the pause runs out on.</summary>
    internal BossCountdown LandingPause { get; } = BossCountdown.Lasting(LandingPauseUpdates);

    /// <summary>How many times the bouncing flame has struck the floor.</summary>
    internal int Bounces
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
