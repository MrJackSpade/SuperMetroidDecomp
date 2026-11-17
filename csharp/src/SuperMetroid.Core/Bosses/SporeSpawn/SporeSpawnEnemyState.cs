using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's state beyond what every enemy has: the boss's swing angle, stalk anchor and
/// death velocity, each named rather than kept in generic enemy values, with their wrapping arithmetic.
/// </summary>
public sealed class SporeSpawnEnemyState
{
    /// <summary>A new fight's state, which his initialization installs on his body (#2231).</summary>
    internal SporeSpawnEnemyState()
    {
    }

    /// <summary>Spore Spawn's body's and his room's colours (#1512).</summary>
    internal SporeSpawnColours Colours { get; } = new();

    /// <summary>Current main-AI movement: the phase of the fight it runs each update.</summary>
    public SporeSpawnMovement Movement { get; internal set; }
    /// <summary>The body's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SporeSpawnBodySegment> BodyPlayback { get; set; }
    /// <summary>
    /// His stalk's four links, from the one at the ceiling down toward his body (#1932): his last spawned link hangs at the
    /// ceiling. They live as long as he does; his death clears every other projectile.
    /// </summary>
    internal IReadOnlyList<EnemyProjectile> StalkLinks { get; set; } = [];
    /// <summary>
    /// Fixed stalk interpolation anchor in room pixels: the spawn body's X and a Y 72 pixels
    /// above it.
    /// </summary>
    public Position16<RoomPixels> StalkAnchor { get; internal set; }
    /// <summary>
    /// Where his body spawned in room pixels, the centre of his sway, kept before the live fight raises him 128 pixels.
    /// </summary>
    public Position16<RoomPixels> MovementCenter { get; internal set; }
    /// <summary>
    /// Sway phase in 256ths of a turn, starting at three quarters; X samples cosine and Y samples the doubled,
    /// shifted phase. The phase wraps around the turn as it advances, which the sway depends on.
    /// </summary>
    public RotationAngle Angle { get; internal set; }
    /// <summary>How wide he sways: set by his descent and his fight's start, and widened by each closing of his shell.</summary>
    internal SporeSpawnSwayRadius SwayRadius { get; set; }
    /// <summary>
    /// How far and which way his sway's phase turns each movement update: a step from his descent and his fight's start,
    /// quickened to two low on health, and reversed by the first damaging hit of each opening.
    /// </summary>
    internal SporeSpawnSwayStep SwayStep { get; set; } = SporeSpawnSwayStep.None;
    /// <summary>Whether the ceiling spore emitters are stopped: the animation sets it during protected phases and clears it, and death clears it.</summary>
    public bool SporeGenerationSuppressed { get; internal set; }
    /// <summary>One-hit guard for an open phase: once set, further hits neither reverse the sway nor close the head until the animation clears it.</summary>
    public bool Damaged { get; internal set; }
    /// <summary>
    /// His death drift in 1/65536 pixels per update on each axis, set once as his death begins: one pixel per update
    /// shared between the axes along his bearing to the arena's centre at (128,624), each axis signed toward it.
    /// </summary>
    public FixedVector<RoomPixels> DeathVelocity { get; internal set; }
    /// <summary>Guard recording that zero health has already triggered projectile cleanup, the defeated flag, the death animation, and the ceiling-crumble request.</summary>
    public bool DeathStarted { get; internal set; }

}
