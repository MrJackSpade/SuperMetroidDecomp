using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's state beyond the head's enemy record, including its thirteen body segments, which are not ordinary enemies
/// and so are named here rather than given enemy records.
/// </summary>
public sealed class BotwoonEnemyState
{
    /// <summary>
    /// The fight's phase, owning initial waiting, traversal/spitting, and synchronized death progression.
    /// </summary>
    public BotwoonPhase? Phase { get; internal set; }

    /// <summary>The independent movement behaviour, choosing direct hole movement or an authored path.</summary>
    public BotwoonMovement? Movement { get; internal set; }

    /// <summary>
    /// The independent head behaviour, controlling movement orientation, aiming, spit-frame admission, and cooldown.
    /// </summary>
    public BotwoonHeadAction? HeadAction { get; internal set; }

    /// <summary>The trail ring's next place, 0 through 255 (<see cref="BotwoonTrailRing"/>); each moving update writes the head's position there and moves on one place.</summary>
    public int RingIndex { get; internal set; }
    /// <summary>History-entry distance between successive body samples, selected inversely to movement speed: 6, 4, or 3.</summary>
    public int SegmentSpacingEntries { get; internal set; }
    /// <summary>Frames left of the opening delay, seeded to 256 and counted down to zero before normal traversal begins.</summary>
    public int InitialDelayTimer { get; internal set; }
    /// <summary>Frames left of the spit-action cooldown, seeded to 48 for the stationary attack and counted down past zero by the head behaviour before being pinned to zero.</summary>
    public int AttackTimer { get; internal set; }
    /// <summary>
    /// Pre-fall death count: seeded to 240 once the tail completes the hole transition and incremented once per update
    /// until it reaches 256, when the head starts falling; it stays within 240..256.
    /// </summary>
    public int DeathTimer { get; internal set; }
    /// <summary>
    /// Head-fall place along the quadratic speed table: each still-airborne update adds three quarters of a record, and
    /// the whole records select the fall speed. The fall reaches the floor before the speed table runs out, so it stays
    /// well below record 95.
    /// </summary>
    public EnemyQuadraticSpeedIndex DeathFallAccumulator { get; internal set; }
    /// <summary>Elapsed wall-explosion update count 0 through 192, also used as the vertical progression of randomized explosion positions.</summary>
    public int WallExplosionFrame { get; internal set; }
    /// <summary>Frames until the next large wall explosion, active from update 64 and reloaded to 12 once it runs past zero.</summary>
    public int LargeExplosionTimer { get; internal set; }
    /// <summary>Frames until the next pair of small wall explosions, active from update 64 and reloaded to four once it runs past zero.</summary>
    public int SmallExplosionTimer { get; internal set; }
    /// <summary>Health-selected movement magnitude 2, 3, or 4: pixels per direct-vector update or path steps taken per authored-path update.</summary>
    public int Speed { get; internal set; }
    /// <summary>The hole, zero through three, that direct movement targets and the next path choice starts from.</summary>
    public int TargetHoleIndex { get; internal set; }
    /// <summary>Wrapping movement angle in 256ths of a turn: a quarter turn minus the clamped head-to-hole angle, consumed by the signed fixed-point vector helper.</summary>
    public RotationAngle MovementAngle { get; internal set; }
    /// <summary>Wrapping spit angle in 256ths of a turn: a quarter turn minus the head-to-Samus angle; volley children add their spread offsets to it.</summary>
    public RotationAngle SpitAngle { get; internal set; }
    /// <summary>
    /// The authored path chosen next, zero through 31: one of four random variants for the target hole, in the visible
    /// half of the table or, from inside a hole, the hidden half.
    /// </summary>
    public int PathChoice { get; internal set; }
    /// <summary>The authored path being walked, an index into <see cref="BotwoonPathDefinitions.Paths"/>.</summary>
    public int PathIndex { get; internal set; }
    /// <summary>
    /// The next step of <see cref="PathIndex"/> to take. A forward walk ends when it passes the last step and a backward
    /// walk when it passes the first, so it ranges from one before the first step to one after the last.
    /// </summary>
    public int PathStep { get; internal set; }
    /// <summary>Which way <see cref="PathIndex"/> is walked: a backward walk takes the steps in reverse order, each reversed.</summary>
    public BotwoonPathTraversal PathTraversal { get; internal set; }
    /// <summary>The head animation the AI last installed and where it stands; reinstalling the same animation does not restart it.</summary>
    internal AnimationPlayback<BotwoonHeadAnimationSegment> HeadPlayback { get; set; } =
        new(BotwoonAnimations.HeadStartingWith(BotwoonHeadAnimationSegment.Hidden), AnimationPlace.StartOfAnimation);
    /// <summary>The head animation the AI last installed: the segment its animation's one phase plays.</summary>
    internal BotwoonHeadAnimationSegment HeadAnimation => HeadPlayback.Animation.Phases[HeadPlayback.Place.Phase].Segment;
    /// <summary>The shading his head and body show: the last health band shown, or his own colours before any (#1512).</summary>
    internal PictureShading BodyShading => BotwoonHealthBands.LastBandShown(HealthBandsPassed) is { } band
        ? PictureShading.Shaded(BotwoonShades.Health(band))
        : PictureShading.Standard;
    /// <summary>The health thresholds he has passed, 0 through 8: each shows the next health band, and 8 marks all passed.</summary>
    public int HealthBandsPassed { get; internal set; }
    /// <summary>Initial maximum health shifted right one, used as the strict threshold for movement health phase 1.</summary>
    public int HalfHealth { get; internal set; }
    /// <summary>Initial maximum health shifted right two, used as the strict threshold for the fastest movement health phase.</summary>
    public int QuarterHealth { get; internal set; }
    /// <summary>Movement/spit speed record index 0, 1, or 2, refreshed when choosing a path outside a hole according to half/quarter-health thresholds.</summary>
    [IntegerQuantity("count of health thresholds passed, zero through two, indexing the speed records")]
    public int HealthPhase { get; internal set; }
    /// <summary>Whether the head travels within the wall/hole; crossing a hole rectangle toggles this state and hidden movement disables collision.</summary>
    public bool InsideHole { get; internal set; }
    /// <summary>Inside-hole state observed by the direct-movement behaviour on its previous call; a difference marks that traversal complete.</summary>
    public bool PreviousInsideHole { get; internal set; }
    /// <summary>Suppresses repeated rectangle-trigger toggles until the last body point reaches the saved ring offset or a new hidden traversal clears it.</summary>
    public bool HoleLatch { get; internal set; }
    /// <summary>Initial-action latch forcing the first completed hole traversal into authored movement rather than consuming the normal spit-choice RNG.</summary>
    public bool InitialAction { get; internal set; }
    /// <summary>Movement completion signal, raised when a path ends or on an observed hole-state transition and consumed by the main action dispatcher.</summary>
    public bool PathComplete { get; internal set; }
    /// <summary>Flag set by the head animation's private spit cue; the five-projectile spit consumes and clears it on emission.</summary>
    public bool SpitFrameReached { get; internal set; }
    /// <summary>Fatal-hit request that preserves ongoing traversal until the final body point completes the next outward hole transition.</summary>
    public bool PendingDeath { get; internal set; }
    /// <summary>Tail synchronization signal set when body argument zero crosses the saved history offset while the head is outside, permitting pending death to begin.</summary>
    public bool ExitTransitionComplete { get; internal set; }
    /// <summary>Shared death-start flag observed by all thirteen independent body-projectile turns to replace their normal animation with staggered falling.</summary>
    public bool BodyDeathStarted { get; internal set; }
    /// <summary>Landing signal from retained body argument zero, releasing the head's wait-for-body phase into wall crumbling and specialized drops.</summary>
    public bool LastBodySegmentLanded { get; internal set; }
    /// <summary>Ring index saved at a head hole crossing so delayed body samples toggle at the same position; null means no crossing remains pending.</summary>
    public int? SavedHoleRingIndex { get; internal set; }

    /// <summary>Thirteen actors, indexed by spawn argument 0,2,...24 divided by two.</summary>
    public EnemyProjectile?[] BodySegments { get; } =
        new EnemyProjectile?[13];

    /// <summary>The spawned body segment at <paramref name="index"/>; body movement runs only after all thirteen spawn.</summary>
    internal EnemyProjectile Segment(int index) => BodySegments[index] ??
        throw new InvalidOperationException($"Botwoon body segment {index} has not spawned or lost its projectile slot.");

    /// <summary>
    /// Whether each body segment is inside a hole: true means that body point is travelling inside the
    /// wall/hole; crossing the head's saved ring offset toggles it.
    /// </summary>
    internal bool[] SegmentInsideHole { get; } = new bool[13];

    /// <summary>
    /// Each body segment's phase, by segment index (#1413). The game kept it in the segment projectile's horizontal
    /// velocity, which no body motion reads; the phase is the segment's own state here.
    /// </summary>
    internal BotwoonBodyPhase[] SegmentPhases { get; } = new BotwoonBodyPhase[13];

    /// <summary>The head's last moving positions, one for each of the trail ring's places (<see cref="BotwoonTrailRing"/>), cleared as the room's enemies load.</summary>
    internal Position16<RoomPixels>[] History { get; } = new Position16<RoomPixels>[BotwoonTrailRing.Places];

    /// <summary>Four delayed head positions used to choose the visible head orientation.</summary>
    internal Position16<RoomPixels>[] HeadHistory { get; } = new Position16<RoomPixels>[4];
}
