using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What one of Botwoon's body segments keeps: the pose its angle to the segment ahead selects, and its delay before it falls when Botwoon dies.</summary>
/// <param name="pose">The pose the segment starts in.</param>
internal sealed class BotwoonSegmentState(BotwoonSegmentProgram pose) : EnemyProjectileState
{
    /// <summary>The pose the segment's angle to the segment ahead selects; the segment shows it while Botwoon lives.</summary>
    internal BotwoonSegmentProgram Pose { get; set; } = pose;

    /// <summary>
    /// The segment's count toward falling once Botwoon dies: it starts later for segments in later projectile slots and
    /// the segment falls when it reaches <see cref="FallAt"/>.
    /// </summary>
    internal int DeathDelay
    {
        get;
        set => field = Ensure.BetweenInclusive(0, FallAt, value);
    }

    /// <summary>The count at which a dying segment falls.</summary>
    internal const int FallAt = 256;

    /// <summary>The count a dying segment in the first projectile slot starts at.</summary>
    internal const int FirstDeathDelay = 96;

    /// <summary>How much higher each later projectile slot starts its dying segment's count, so later slots fall sooner.</summary>
    internal const int DeathDelayPerSlot = 8;
}
