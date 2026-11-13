using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Named projection of Ridley's common enemy slot and his encounter's own values.
/// Ceres-only Baby/rotating-getaway values and Norfair-only combat values intentionally coexist here:
/// one initializer serves both encounters and only one Ridley can exist at a time.
/// </summary>
/// <remarks>
/// The Lower Norfair combat values carry the meanings their consumers establish.
/// Ceres's private Baby actor and rotating getaway share Ridley's encounter values, and the Ceres self-destruct
/// presentation's function timer deliberately keeps the phase values 0, 2, 4, 6, 8, 10 and 12.
/// </remarks>
public sealed class RidleyEnemyState
{
    /// <summary>Gets the live AI function identity, shared by reveal logic and encounter-specific attack or escape phases.</summary>
    public RidleyAiFunction Function { get; internal set; }
    /// <summary>Gets the encounter mode: zero during reveal, positive during combat, and dying during Lower Norfair death.</summary>
    public RidleyFightMode FightMode { get; internal set; }
    /// <summary>
    /// Gets the number of Ceres shots Ridley has taken, used for health-palette progression and the hundred-hit fake-retreat
    /// trigger. It only counts up, one per accepted shot, and shots stop landing once the retreat publishes the escape
    /// handoff, so it stays far below the range where the game's counter would wrap.
    /// </summary>
    public int HitCounter { get; internal set; }
    /// <summary>The colours the encounter has loaded over its sprites and its room (#1512).</summary>
    internal RidleyColours Colours { get; } = new();
    /// <summary>Whether Ridley's tail and wings draw in the hurt flash's colours this update.</summary>
    internal bool TailAndWingsFlashing { get; set; }
    /// <summary>Whether the common enemy draw path draws Ridley's body in the hurt flash's colours this update.</summary>
    internal bool BodyFlashing { get; set; }
    /// <summary>The shading Ridley's tail and wings draw in: his own colours, or the hurt flash's (#1512).</summary>
    public PictureShading TailAndWingsShading => Colours.SpriteShading(TailAndWingsFlashing);
    /// <summary>The shading the common enemy draw path draws Ridley's body in (#1512).</summary>
    public PictureShading CommonDrawShading => Colours.SpriteShading(BodyFlashing);
    /// <summary>Gets whether shared body motion and wing/tail updates run; cleared when the intact death body is hidden.</summary>
    public bool MovementAnimationEnabled { get; internal set; }
    /// <summary>Gets the facing-table index: zero for left, one for front-facing transition art, and two for right.</summary>
    public RidleyFacing FacingDirection { get; internal set; }
    /// <summary>Gets whether neutral tail control may start random or proximity-triggered whips; the Ceres swoop disables it.</summary>
    public bool IdleTailWhipEnabled { get; internal set; }
    /// <summary>Gets the tail tip's contact damage before Samus's suit reduction: fifteen in Ceres, 120 in Lower Norfair.</summary>
    public int TailDamage { get; internal set; }
    /// <summary>Gets the current AI phase's countdown, which times how long the phase lasts.</summary>
    public int FunctionTimer { get; internal set; }
    /// <summary>
    /// Gets the shared reveal fade's step: the eye-fade schedule step (zero through 64), the body-fade shade (zero through
    /// fifteen), or the Lower Norfair reveal step (zero through fifteen). Each fade resets it to zero when it completes.
    /// </summary>
    public int FadeStep { get; internal set; }
    /// <summary>Gets the movement clamp's minimum whole-pixel corner, retaining negative limits wrapped into the coordinate range.</summary>
    public Position16<RoomPixels> MovementMinimum { get; internal set; }
    /// <summary>Gets the movement clamp's maximum whole-pixel corner.</summary>
    public Position16<RoomPixels> MovementMaximum { get; internal set; }
    /// <summary>Whether Ridley's movement hit a room boundary, consumed by the swoop.</summary>
    public bool HitRoomBoundary { get; internal set; }
    /// <summary>Gets Ridley's signed 8.8 velocity, each axis held unchanged.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Gets the shared wing animation phase, zero through nine, advancing when its timer underflows.</summary>
    public int WingFrame { get; internal set; }
    /// <summary>Gets the wing animation's countdown, reloaded to 32 once it runs below zero.</summary>
    public int WingAnimationTimer { get; internal set; }
    /// <summary>Gets the decrement (zero through 48) selected from velocity magnitude for the wing countdown; nonnegative vertical motion halves it.</summary>
    public int WingAnimationTimerDelta { get; internal set; }
    /// <summary>Gets the shared tail-controller index; zero retains offsets, one selects neutral control, and the other defined indexes select pogo/stab variants.</summary>
    public RidleyTailFunction TailFunctionIndex { get; internal set; }
    /// <summary>Gets the angular step (zero through eight) used by shared tail segment motion and stagger propagation.</summary>
    public int TailAngleDelta { get; internal set; }
    /// <summary>Gets the facing-dependent furthest-clockwise angle clockwise tail movement may reach.</summary>
    public RidleyTailAngle TailMinimumClockwiseAngle { get; internal set; }
    /// <summary>Gets the facing-dependent furthest-counterclockwise angle counterclockwise tail movement may reach.</summary>
    public RidleyTailAngle TailMaximumCounterClockwiseAngle { get; internal set; }
    /// <summary>
    /// Gets the tail angle a clockwise whip stops at, or null while no clockwise whip is aimed. Aimed targets lie within
    /// a turn either side of straight down.
    /// </summary>
    public RidleyTailAngle? ClockwiseWhipTarget { get; internal set; }
    /// <summary>
    /// Gets the tail angle a counterclockwise whip stops at, or null while no counterclockwise whip is aimed. Aimed
    /// targets lie within a turn either side of straight down.
    /// </summary>
    public RidleyTailAngle? CounterClockwiseWhipTarget { get; internal set; }
    /// <summary>
    /// Gets the pending one-shot whip request, zero or one; its value minus one is how many whole turns further along
    /// its swing the next aimed whip's target lies, before neutral tail control consumes it.
    /// </summary>
    public int TailWhipRequest { get; internal set; }
    /// <summary>Gets the distance (fifteen sixteenths or one and a half pixels) tail segments extend each update toward their targets.</summary>
    public SignedEightEight TailExtensionSpeed { get; internal set; }
    /// <summary>Gets the angular stagger threshold (eight through 23) used to propagate a wave or whip from one segment to the next.</summary>
    public int IdealInterSegmentTailAngle { get; internal set; }
    /// <summary>Gets the seven tail segments, base first, installed by shared initialization; the array is empty before that layout is initialized.</summary>
    public RidleyTailSegment[] TailSegments { get; internal set; } = [];
    /// <summary>Gets whether Ridley's animation has enabled the roar presentation.</summary>
    public bool Roaring { get; internal set; }
    /// <summary>Gets the Ceres hover-cycle counter used to choose the next scripted attack; every attack resets it before it passes 124.</summary>
    public int HoverCounter { get; internal set; }
    /// <summary>Ridley's playing body animation and where it stands (#641, #1600); before the first start, the initial animation.</summary>
    internal AnimationPlayback<RidleyBodySegment> BodyPlayback { get; set; } =
        new(RidleyAnimations.StartingWith(RidleyBodySegment.Initial), AnimationPlace.StartOfAnimation);
    /// <summary>Gets the claw-height pose Ridley's body program last selected: zero standing, one legs half extended, two legs extended.</summary>
    public int FeetDistanceIndex { get; internal set; }
    /// <summary>Gets the Ceres fireball attack's whole-pixel body anchor; firing motion aims at random jitter around this saved position.</summary>
    public Position16<RoomPixels> FireballBasePosition { get; internal set; }
    /// <summary>Gets the signed 8.8 velocity computed from the clamped fireball aim angle.</summary>
    public EightEightVector<RoomPixels> FireballVelocity { get; internal set; }
    /// <summary>Independent swoop phase countdown used by the swoop functions.</summary>
    public int SwoopPhaseTimer { get; internal set; }
    /// <summary>Gets the swoop's heading, which wraps around the full turn; its table index selects the sine sample.</summary>
    public RotationAngle SwoopAngleAccumulator { get; internal set; }
    /// <summary>
    /// Gets the 8.8 swoop speed magnitude (zero through 1280, in 1/256 pixels per update), converged 32 at a time toward
    /// each phase's target before resolving axis velocities.
    /// </summary>
    public int SwoopSpeedMagnitude { get; internal set; }

    /// <summary>Gets Lower Norfair's health-table stage from zero through three, changing below 9000, 5400, and 1800 health.</summary>
    [IntegerQuantity("count of health thresholds passed, zero through three, indexing the health tables")]
    public int HealthStage { get; internal set; }
    /// <summary>Gets the Lower Norfair grabbed-Samus state controlling claw attachment and release: one while Ridley carries Samus, otherwise zero.</summary>
    public RidleyGrabState GrabState { get; internal set; }
    /// <summary>Gets the Lower Norfair countdown keeping the body intangible after a grab release.</summary>
    public int IntangibilityTimer { get; internal set; }
    /// <summary>Gets the current Lower Norfair movement target's whole-pixel room position.</summary>
    public Position16<RoomPixels> Target { get; internal set; }
    /// <summary>Gets the Lower Norfair pogo bounce counter, reset after its two-bounce threshold.</summary>
    public int PogoBounceCount { get; internal set; }
    /// <summary>Gets the positive velocity increment selected by health stage for downward pogo motion.</summary>
    public SignedEightEight PogoDownwardAcceleration { get; internal set; }
    /// <summary>Gets the positive velocity increment selected by health stage for upward pogo motion.</summary>
    public SignedEightEight PogoUpwardAcceleration { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel displacement from the claw anchor; carry updates decay each axis toward zero.</summary>
    public Offset16<RoomPixels> GrabOffset { get; internal set; }
    /// <summary>
    /// Gets the Lower Norfair hit-pressure count, zero through eight: each hurt frame raises it by one and each ordinary
    /// AI frame lowers it by four; at eight a hurt frame also carries a grabbed Samus.
    /// </summary>
    public int HurtMovementClamp { get; internal set; }
    /// <summary>Gets the number of zero-health Lower Norfair lunges completed before the authored death transition.</summary>
    public int ZeroHealthLungeCount { get; internal set; }
    /// <summary>Gets the Lower Norfair small-explosion countdown, reloaded to four for one explosion every five calls.</summary>
    public int DeathExplosionTimer { get; internal set; }
    /// <summary>Gets the Lower Norfair index, zero through nine, cycling through ten authored death-explosion placements.</summary>
    public int DeathExplosionCount { get; internal set; }
    /// <summary>Gets whether Lower Norfair's twelve independently moving breakup actors have already been allocated.</summary>
    public bool DeathBreakupSpawned { get; internal set; }

    /// <summary>The Ceres Baby's playing cutscene animation, standing after its current pose; owned separately from Ridley's body.</summary>
    internal AnimationPlayback<CeresBabySegment> BabyPlayback { get; set; } =
        new(CeresBabyAnimations.StartingWith(CeresBabySegment.Initial), AnimationPlace.StartOfAnimation);
    /// <summary>How many draw updates the Baby's current pose lasts; zero before its program shows the first pose.</summary>
    internal int BabyPoseDuration { get; set; }
    /// <summary>Gets the Ceres Baby's elapsed draw-update count in its current pose, compared with the pose's duration and restarted at one.</summary>
    public int BabyFrameHold { get; internal set; }
    /// <summary>Gets the Ceres Baby's behavior selecting hand attachment, falling setup, falling motion, or idle.</summary>
    public CeresBabyFunction BabyFunction { get; internal set; }
    /// <summary>The pose the Ceres Baby's program currently shows, or null before it has shown one.</summary>
    internal CeresBabyPose? BabyCurrentPose { get; set; }
    /// <summary>
    /// Gets the Ceres Baby's wrapped room position. Its vertical fraction carries the falling integrator's subpixels and
    /// survives the hand attachments, which replace only the whole pixels.
    /// </summary>
    public FixedPosition<RoomPixels> BabyPosition { get; internal set; }
    /// <summary>
    /// Gets the Ceres Baby's 8.8 falling speed (1/256 pixels per update): zero until it is dropped, then eight more each
    /// falling update until it lands.
    /// </summary>
    public SignedEightEight BabyVerticalVelocity { get; internal set; }
    /// <summary>Gets whether Ceres's getaway transform currently owns the Ridley/Baby presentation.</summary>
    public bool GetawayActive { get; internal set; }
    /// <summary>Gets whether the Ceres getaway's motion has ended, its curve having passed its last frame, and released the active transform.</summary>
    public bool GetawayFinished { get; internal set; }
    /// <summary>Gets the frame of Ceres's getaway motion the next update shows, advanced by one each update.</summary>
    public int GetawayFrameIndex { get; internal set; }
    /// <summary>Gets how far the Ceres getaway has turned its floor clockwise, wrapping at a full turn.</summary>
    public RotationAngle FloorAngle { get; internal set; }
    /// <summary>Gets the floor point the Ceres getaway places at the screen's top-left before turning and zooming the floor.</summary>
    public Offset16<FloorPixels> FloorOffset { get; internal set; }
    /// <summary>
    /// Gets the getaway's zoom: eight floor pixels per screen pixel when the getaway starts, then each table frame's
    /// authored zoom. It also selects the floor's shade.
    /// </summary>
    public FloorZoom FloorZoom { get; internal set; }
    /// <summary>Gets the floor point the Ceres getaway turns and zooms its floor about, (64, 64) while it runs and cleared on completion.</summary>
    public Position16<FloorPixels> FloorCenter { get; internal set; }
    /// <summary>Gets the four-phase Ceres Baby graphics animation index (zero through three) during the getaway.</summary>
    public int GetawayBabyFrame { get; internal set; }
    /// <summary>Gets the two-phase Ridley wing graphics animation index (zero or one) during the getaway.</summary>
    public int GetawayWingFrame { get; internal set; }

    /// <summary>Which step of the Ceres self-destruct warning runs next; the getaway's end starts it.</summary>
    internal CeresSelfDestructPhase CeresSelfDestructPhase { get; set; }
    /// <summary>Where the Ceres self-destruct stands in its current stage of readying the warning, advanced each update (#1406, #1538).</summary>
    internal CeresEscapeReadying CeresEscapeReadying { get; set; }
    /// <summary>Gets the Ceres warning phase or glyph delay countdown, including the initial 128-frame wait.</summary>
    public int CeresEscapeTextDelayTimer { get; internal set; }
    /// <summary>Gets the installed Ceres warning typewriter state, created after the initial delay when presentation data is available.</summary>
    public EscapeTypewriterState? CeresEscapeTypewriter { get; internal set; }
    /// <summary>Gets the step the Ceres self-destruct warning's sixteen-step alarm shows, zero through fifteen, returning to zero after fifteen.</summary>
    public int SelfDestructAlarmStep { get; internal set; }
}
