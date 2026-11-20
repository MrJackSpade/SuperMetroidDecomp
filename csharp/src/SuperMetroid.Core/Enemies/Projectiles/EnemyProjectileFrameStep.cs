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

    /// <summary>
    /// The animation shows <paramref name="Frame"/> for <paramref name="Duration"/> projectile passes, drawn
    /// <paramref name="Placement"/> off the projectile's place.
    /// </summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">How many projectile passes the frame lasts.</param>
    /// <param name="Placement">Where the frame is drawn from the projectile's place (#2557).</param>
    internal sealed record Show(EnemyProjectileFrameId Frame, int Duration, Offset16<RoomPixels> Placement) : EnemyProjectileFrameStep;

    /// <summary>The animation shows <paramref name="Frame"/> from now on, drawn <paramref name="Placement"/> off the projectile's place, and rests on it.</summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Placement">Where the frame is drawn from the projectile's place (#2557).</param>
    internal sealed record ShowForGood(EnemyProjectileFrameId Frame, Offset16<RoomPixels> Placement) : EnemyProjectileFrameStep;

    /// <summary>The animation rests on the frame it shows.</summary>
    internal sealed record Rest : EnemyProjectileFrameStep;
}
