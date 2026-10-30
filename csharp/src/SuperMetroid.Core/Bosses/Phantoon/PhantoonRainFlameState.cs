using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What one of Phantoon's rain flames keeps: how long it hangs before it falls.</summary>
/// <param name="fallDelay">Updates the flame hangs before it falls.</param>
internal sealed class PhantoonRainFlameState(int fallDelay) : EnemyProjectileState
{
    /// <summary>Updates the flame still hangs; it falls, with its falling sound, on the update this reaches zero.</summary>
    internal int FallDelay
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    } = Ensure.AtLeastZero(fallDelay);
}
