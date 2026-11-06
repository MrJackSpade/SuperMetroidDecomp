using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of Mother Brain's bouncing bombs keeps: the sideways speed each bounce restores, how many bounces it has
/// made, and how many afterburns its explosion chains.
/// </summary>
/// <param name="bounceSpeed">The sideways speed each floor bounce restores, in 1/256 pixel per update.</param>
/// <param name="afterburns">The afterburns the bomb's explosion chains when it expires, as its spawn set them.</param>
internal sealed class MotherBrainBombState(int bounceSpeed, int afterburns) : EnemyProjectileState
{
    /// <summary>The sideways speed each floor bounce restores, in 1/256 pixel per update.</summary>
    internal int BounceSpeed { get; } = bounceSpeed;

    /// <summary>The afterburns the bomb's explosion chains when it expires, as its spawn set them.</summary>
    internal int Afterburns { get; } = Ensure.AtLeastZero(afterburns);

    /// <summary>How many times the bomb has bounced off the floor; zero while it makes its first fall.</summary>
    internal int Bounces
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
