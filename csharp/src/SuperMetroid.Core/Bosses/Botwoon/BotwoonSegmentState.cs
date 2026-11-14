using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// What one of Botwoon's body segments keeps: the pose its angle to the segment ahead selects, its delay before it falls
/// when Botwoon dies, and its place along the quadratic speed table as it falls.
/// </summary>
/// <param name="pose">The pose the segment starts in.</param>
internal sealed class BotwoonSegmentState(BotwoonBodyAnimationSegment pose) : EnemyProjectileState
{
    /// <summary>The pose the segment's angle to the segment ahead selects; the segment shows it while Botwoon lives.</summary>
    internal BotwoonBodyAnimationSegment Pose { get; set; } = pose;

    /// <summary>
    /// The segment's count toward falling once Botwoon dies: it starts by the segment's projectile place
    /// (<see cref="Compatibility.BotwoonSegmentFallDelay"/>) and the segment falls when it reaches <see cref="FallAt"/>.
    /// </summary>
    internal int DeathDelay
    {
        get;
        set => field = Ensure.BetweenInclusive(0, FallAt, value);
    }

    /// <summary>The count at which a dying segment falls.</summary>
    internal const int FallAt = 256;

    /// <summary>
    /// The falling segment's place along the quadratic speed table: its whole rows choose the speed it falls at, and it
    /// starts at the first row and moves on each update the segment has not yet landed.
    /// </summary>
    internal QuadraticSpeedCurvePlace FallPlace { get; set; }
}
