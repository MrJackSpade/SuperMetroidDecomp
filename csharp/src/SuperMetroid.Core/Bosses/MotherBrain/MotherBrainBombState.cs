using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one of Mother Brain's bouncing bombs keeps: the sideways speed each bounce restores, and how many bounces it has made.</summary>
/// <param name="bounceSpeed">The sideways speed each floor bounce restores, in 1/256 pixel per update.</param>
internal sealed class MotherBrainBombState(int bounceSpeed) : EnemyProjectileState
{
    /// <summary>The sideways speed each floor bounce restores, in 1/256 pixel per update.</summary>
    internal int BounceSpeed { get; } = bounceSpeed;

    /// <summary>How many times the bomb has bounced off the floor; zero while it makes its first fall.</summary>
    internal int Bounces
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
