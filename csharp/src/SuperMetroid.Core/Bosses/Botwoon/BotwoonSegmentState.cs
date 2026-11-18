using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// One of Botwoon's body segments (#2178): the projectile showing it, the head it trails, its place in the body, its phase,
/// whether it travels inside the wall, the pose its angle to the segment ahead selects and the animation that shows it,
/// its delay before it falls when Botwoon dies, and its place along the quadratic speed table as it falls.
/// </summary>
/// <param name="head">Botwoon's head, which the segment trails and whose state its death and colours follow.</param>
/// <param name="index">The segment's place in the body, zero (the tail) through twelve (behind the head).</param>
/// <param name="projectile">The enemy projectile Botwoon spawned to show the segment.</param>
/// <param name="animation">The segment's animation, started on its projectile, whose pose the segment starts in.</param>
internal sealed class BotwoonSegmentState(RoomEnemyPlace head, int index, EnemyProjectile projectile, BotwoonSegmentAnimation animation)
    : EnemyProjectileState
{
    /// <summary>Botwoon's head, which the segment trails: its state starts the segment's death and colours it.</summary>
    internal RoomEnemyPlace Head { get; } = Ensure.NotNull(head);

    /// <summary>The segment's place in the body, zero (the tail) through twelve (behind the head).</summary>
    internal int Index { get; } = Ensure.BetweenInclusive(0, BotwoonFightDefinitions.BodySegments - 1, index);

    /// <summary>Whether the segment is the tail, which shows the tail cap and signals the head when it lands.</summary>
    internal bool IsTail => Index == 0;

    /// <summary>The enemy projectile showing the segment, which Botwoon places along his head's trail every moving update.</summary>
    internal EnemyProjectile Projectile { get; } = Ensure.NotNull(projectile);

    /// <summary>
    /// The segment's phase (#1413): it lives until Botwoon dies, then waits, falls and lands.
    /// </summary>
    internal BotwoonBodyPhase Phase { get; set; } = BotwoonBodyPhase.Main;

    /// <summary>
    /// Whether the segment travels inside the wall: it starts there, and crossing the head's saved trail place toggles it.
    /// </summary>
    internal bool InsideHole { get; set; } = true;

    /// <summary>The pose the segment's angle to the segment ahead selects; the segment shows it while Botwoon lives.</summary>
    internal BotwoonBodyAnimationSegment Pose { get; set; } = animation.PlayingSegment;

    /// <summary>The segment's animation, which shows its pose while Botwoon lives (#2400).</summary>
    internal BotwoonSegmentAnimation Animation { get; } = Ensure.NotNull(animation);

    /// <summary>
    /// The segment's wait before it falls once Botwoon dies (#2463): its death starts it to last the updates its projectile
    /// place gives it (<see cref="Compatibility.BotwoonSegmentFallDelay"/>), its death's first update counting, and the
    /// segment falls from the update after it runs out.
    /// </summary>
    internal BossCountdown FallWait { get; } = new();

    /// <summary>
    /// The falling segment's place along the quadratic speed table: its whole rows choose the speed it falls at, and it
    /// starts at the first row and moves on each update the segment has not yet landed.
    /// </summary>
    internal QuadraticSpeedCurvePlace FallPlace { get; set; }
}
