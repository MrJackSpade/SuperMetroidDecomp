using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's state beyond what his head shares with every enemy, including its body segments, which are not
/// ordinary enemies and so are kept here rather than spawned as enemies.
/// </summary>
[ArchitectureReview("context-bag", "Botwoon's fight state: every member is a wait, stage, band, path or segment of his own fight that he owns for the room (his spit cooldown, pre-fall and explosion waits, speed stage and health band, #2266, #2267), not a service carried through.")]
public sealed class BotwoonEnemyState
{
    /// <summary>
    /// The fight's phase, owning initial waiting, traversal/spitting, and synchronized death progression; the fight
    /// starts in its opening delay.
    /// </summary>
    public BotwoonPhase Phase { get; internal set; } = BotwoonPhase.InitialDelay;

    /// <summary>
    /// The independent movement behaviour, choosing direct hole movement or an authored path, which holds the walk he
    /// chose; he starts moving toward his target hole.
    /// </summary>
    public BotwoonMovement Movement { get; internal set; } = BotwoonMovement.MoveTowardHole;

    /// <summary>
    /// The independent head behaviour, controlling movement orientation, aiming, spit-frame admission, and cooldown; the
    /// head starts animating from his movement.
    /// </summary>
    public BotwoonHeadAction HeadAction { get; internal set; } = BotwoonHeadAction.AnimateFromMovement;

    /// <summary>The trail ring's next place (<see cref="BotwoonTrailRing"/>); each moving update writes the head's position there and moves on one place.</summary>
    public int RingIndex { get; internal set; }
    /// <summary>
    /// His wait in his hole before his first traversal (#2538): his setup starts it for
    /// <see cref="BotwoonFightDefinitions.OpeningDelay"/>, and he chooses his first action on the update after it runs out.
    /// </summary>
    public BossCountdown OpeningWait { get; } = new();
    /// <summary>
    /// His head's cooldown after a spit volley (#2267): it lasts the volley's <see cref="BotwoonSpitVolley.CooldownUpdates"/>,
    /// and when it runs out his head follows his movement again.
    /// </summary>
    public BossCountdown SpitCooldown { get; } = new();
    /// <summary>
    /// His wait before his head falls (#2267): it starts once his tail has finished crossing into the wall after the fatal
    /// hit and lasts <see cref="BotwoonDeathDefinitions.PreFallWaitUpdates"/>, his head falling from the update after.
    /// </summary>
    public BossCountdown PreFallWait { get; } = new();
    /// <summary>
    /// Head-fall place along the quadratic speed curve: each still-airborne update moves it on by the head's fall step,
    /// and the whole marks choose the fall speed. The head reaches the floor well before the curve ends.
    /// </summary>
    public QuadraticSpeedCurvePlace FallPlace { get; internal set; }
    /// <summary>
    /// The wall sequence, lasting <see cref="BotwoonDeathDefinitions.WallExplosionDuration"/>; the band its explosions
    /// burst in moves down the wall with its age.
    /// </summary>
    public UpdateCountdown WallSequence { get; internal set; } = UpdateCountdown.Ended();
    /// <summary>
    /// The wait for the next large wall explosion (#2267): from <see cref="BotwoonDeathDefinitions.WallExplosionsBegin"/> one bursts at once, then one
    /// every <see cref="BotwoonDeathDefinitions.LargeExplosionInterval"/>.
    /// </summary>
    public BossCountdown LargeExplosionWait { get; } = new();
    /// <summary>
    /// The wait for the next pair of small wall explosions (#2267): from <see cref="BotwoonDeathDefinitions.WallExplosionsBegin"/> a pair bursts at
    /// once, then a pair every <see cref="BotwoonDeathDefinitions.SmallExplosionInterval"/>.
    /// </summary>
    public BossCountdown SmallExplosionWait { get; } = new();
    /// <summary>
    /// His speed stage (#2266): how fast he moves and spits and how far apart his body trails, refreshed from his health
    /// each time he chooses a walk outside a hole; he starts at full speed.
    /// </summary>
    public BotwoonSpeedStage SpeedStage { get; internal set; } = BotwoonSpeedStageDefinitions.Full;
    /// <summary>The hole that direct movement targets and the next walk starts from.</summary>
    public BotwoonHole TargetHole { get; internal set; }
    /// <summary>The direction he moves in toward his target hole: the bearing from his head to it, its distance clamped on each axis.</summary>
    public RotationAngle MovementAngle { get; internal set; }
    /// <summary>The direction his spits fly: the bearing from his head to Samus, to which a volley's other spits add their spread.</summary>
    public RotationAngle SpitAngle { get; internal set; }
    /// <summary>The head animation the AI last installed and where it stands; reinstalling the same animation does not restart it.</summary>
    internal AnimationPlayback<BotwoonHeadAnimationSegment> HeadPlayback { get; set; } =
        new(BotwoonAnimations.HeadStartingWith(BotwoonHeadAnimationSegment.Hidden), AnimationPlace.StartOfAnimation);
    /// <summary>The shading his head and body show: the health band he shows, or his own colours before any (#1512).</summary>
    internal PictureShading BodyShading => HealthBand is { } band
        ? PictureShading.Shaded(BotwoonShades.Health(band))
        : PictureShading.Standard;
    /// <summary>The health band his head and body show (#2266), or null while he still shows his own colours.</summary>
    public BotwoonHealthBand? HealthBand { get; internal set; }
    /// <summary>The health his living setup starts him with, before combat can change it.</summary>
    public int StartingHealth { get; internal set; }
    /// <summary>Half his starting health, rounded down: below it he moves at <see cref="BotwoonSpeedStageDefinitions.BelowHalf"/>.</summary>
    internal int HalfHealth => StartingHealth / BotwoonFightDefinitions.FasterBelowShareOfHealth;
    /// <summary>A quarter of his starting health, rounded down: below it he moves at <see cref="BotwoonSpeedStageDefinitions.BelowQuarter"/>.</summary>
    internal int QuarterHealth => StartingHealth / BotwoonFightDefinitions.FastestBelowShareOfHealth;
    /// <summary>Whether the head travels within the wall/hole; crossing a hole rectangle toggles this state and hidden movement disables collision.</summary>
    public bool InsideHole { get; internal set; }
    /// <summary>Inside-hole state observed by the direct-movement behaviour on its previous update; a difference marks that traversal complete.</summary>
    public bool PreviousInsideHole { get; internal set; }
    /// <summary>Keeps the hole his head crossed from toggling his hole state again until his tail reaches the place the head crossed at, or a new walk starts inside the wall.</summary>
    public bool HoleLatch { get; internal set; }
    /// <summary>Set until his first crossing: it sends him on a walk without drawing his next action at random.</summary>
    public bool InitialAction { get; internal set; }
    /// <summary>Set by the fatal hit: he keeps on his walk until his tail has next crossed out of the wall, and then dies.</summary>
    public bool FatallyHit { get; internal set; }
    /// <summary>Set when his tail crosses the place his head crossed at while the head is outside the wall, letting a fatal hit's death begin.</summary>
    public bool ExitTransitionComplete { get; internal set; }
    /// <summary>Set as his death begins: each of his body segments then stops following his head and starts its own delay before it falls.</summary>
    public bool BodyDeathStarted { get; internal set; }
    /// <summary>Landing signal from the tail segment, releasing the head's wait-for-body phase into wall crumbling and specialized drops.</summary>
    public bool LastBodySegmentLanded { get; internal set; }
    /// <summary>Ring index saved at a head hole crossing so delayed body samples toggle at the same position; null means no crossing remains pending.</summary>
    public int? SavedHoleRingIndex { get; internal set; }

    /// <summary>
    /// His body segments, from the tail (zero) to the segment behind the head (#2178): each segment's projectile, its phase
    /// and whether it travels inside the wall are its own (<see cref="BotwoonSegmentState"/>). He starts with none; his
    /// setup spawns all <see cref="BotwoonFightDefinitions.BodySegments"/> of them, each holding this state, unless he is
    /// already defeated, when he spawns no body (#2554).
    /// </summary>
    internal IReadOnlyList<BotwoonSegmentState> Body { get; private set; } = [];

    /// <summary>Holds the <paramref name="body"/> his setup spawned.</summary>
    /// <param name="body">His body segments from the tail to the segment behind the head.</param>
    internal void HoldBody(IReadOnlyList<BotwoonSegmentState> body) => Body = Ensure.NotNull(body);

    /// <summary>The head's last moving positions, one for each of the trail ring's places (<see cref="BotwoonTrailRing"/>), all at the room's origin as he spawns.</summary>
    internal Position16<RoomPixels>[] History { get; } = new Position16<RoomPixels>[BotwoonTrailRing.Places];

    /// <summary>The head's delayed positions, newest first, the oldest of which chooses the visible head orientation.</summary>
    internal Position16<RoomPixels>[] HeadHistory { get; } = new Position16<RoomPixels>[BotwoonFightDefinitions.HeadHistoryLength];
}
