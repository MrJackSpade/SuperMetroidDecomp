namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a projectile's animation does when its frame expires (#1607): shows a frame for a duration, shows a frame for
/// good, or rests on the frame it shows.
/// </summary>
internal abstract record EnemyProjectileFrameStep
{
    /// <summary>Allows only the steps below.</summary>
    private EnemyProjectileFrameStep()
    {
    }

    /// <summary>The animation rests on the frame it shows: its timeline holds.</summary>
    internal static EnemyProjectileFrameStep Rests { get; } = new Rest();

    /// <summary>The animation shows <paramref name="Frame"/> for <paramref name="Duration"/> projectile passes.</summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">How many projectile passes the frame lasts.</param>
    internal sealed record Show(EnemyProjectileFrameId Frame, int Duration) : EnemyProjectileFrameStep;

    /// <summary>The animation shows <paramref name="Frame"/> from now on and rests on it.</summary>
    /// <param name="Frame">The frame shown.</param>
    internal sealed record ShowForGood(EnemyProjectileFrameId Frame) : EnemyProjectileFrameStep;

    /// <summary>The animation rests on the frame it shows.</summary>
    internal sealed record Rest : EnemyProjectileFrameStep;

    /// <summary>
    /// The animation's play is over: a cue handed the projectile over to <paramref name="Next"/>, which plays on from its
    /// start within the same pass (#2554).
    /// </summary>
    /// <param name="Next">The animation the projectile plays on in.</param>
    internal sealed record HandOver(EnemyProjectileAnimation Next) : EnemyProjectileFrameStep;

    /// <summary>The projectile shows no frame for <paramref name="Duration"/> updates.</summary>
    /// <param name="Duration">The updates the projectile stays hidden.</param>
    internal sealed record Hide(int Duration) : EnemyProjectileFrameStep;
}
