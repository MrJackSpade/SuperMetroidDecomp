using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Named projection of Ridley's common enemy slot and three extended WRAM workspaces.
/// Ceres-only Baby/Mode-7 words and Norfair-only combat words intentionally coexist here:
/// the original shared initializer owns one layout and only one Ridley can exist at a time.
/// </summary>
/// <remarks>
/// The Lower Norfair combat words replace opaque numbered words with the meanings their bank-$A6 consumers establish.
/// Ceres's private Baby actor and Mode-7 getaway reuse Ridley's extended workspaces, and the Ceres self-destruct
/// presentation's function timer deliberately keeps the native phase values 0, 2, 4, 6, 8, 10 and 12.
/// </remarks>
public sealed class RidleyEnemyState
{
    /// <summary>Gets the live bank-$A6 AI function identity, shared by reveal logic and encounter-specific attack or escape phases.</summary>
    public RidleyAiFunction Function { get; internal set; }
    /// <summary>Gets the native encounter-mode word: zero during reveal, positive during combat, and $FFFF during Lower Norfair death.</summary>
    public RidleyFightMode FightMode { get; internal set; }
    /// <summary>
    /// Gets the number of Ceres shots Ridley has taken, used for health-palette progression and the hundred-hit fake-retreat
    /// trigger. It only counts up, one per accepted shot, and shots stop landing once the retreat publishes the escape
    /// handoff, so it stays far below the range where the cartridge's counter would wrap.
    /// </summary>
    public int HitCounter { get; internal set; }
    /// <summary>
    /// Gets the colours Ridley's tail and wings are drawn with: the colours Ridley loads for itself, or the common sprite
    /// colours on hurt-flash updates.
    /// </summary>
    public SpriteColouring SpritemapColouring { get; internal set; } = SharedSpriteColouring.Common;
    /// <summary>
    /// Gets the colours the common enemy draw path uses for Ridley's body: the slot's colours, or the common sprite
    /// colours on hurt-flash updates.
    /// </summary>
    public SpriteColouring CommonDrawColouring { get; internal set; } = SharedSpriteColouring.Common;
    /// <summary>
    /// The colours both Ridleys load for themselves and draw their body, tail and wings in: the sprite set whose shade
    /// their health changes.
    /// </summary>
    internal static SpriteColouring OwnColours { get; } = new EncounterSpriteColouring(RidleyColourDefinitions.Health);
    /// <summary>Gets whether shared body motion and wing/tail updates run; cleared when the intact death body is hidden.</summary>
    public bool MovementAnimationEnabled { get; internal set; }
    /// <summary>Gets the native facing-table index: zero for left, one for front-facing transition art, and two for right.</summary>
    public RidleyFacing FacingDirection { get; internal set; }
    /// <summary>Gets whether neutral tail control may start random or proximity-triggered whips; the Ceres swoop disables it.</summary>
    public bool IdleTailWhipEnabled { get; internal set; }
    /// <summary>Gets the tail tip's contact damage before Samus's suit reduction: fifteen in Ceres, 120 in Lower Norfair.</summary>
    public int TailDamage { get; internal set; }
    /// <summary>Gets the current AI phase's native countdown word; Ceres self-destruct reuses it as the even dispatch index 0, 2, 4, 6, 8, 10, or 12.</summary>
    public WrappingCountdown FunctionTimer { get; internal set; }
    /// <summary>
    /// Gets the shared reveal fade's progress: the eye-fade schedule step (zero through 64), the body-fade row (zero through
    /// fifteen), or the Lower Norfair reveal row (zero through fifteen). Each fade resets it to zero when it completes.
    /// </summary>
    public int FadePaletteOffset { get; internal set; }
    /// <summary>Gets the movement clamp's minimum whole-pixel corner, retaining negative limits as wrapped native words.</summary>
    public Position16<RoomPixels> MovementMinimum { get; internal set; }
    /// <summary>Gets the movement clamp's maximum whole-pixel corner.</summary>
    public Position16<RoomPixels> MovementMaximum { get; internal set; }
    /// <summary>Nonzero predicate of $A6:D86B's hitARoomBoundary, consumed by $A6:BABC.</summary>
    public bool HitRoomBoundary { get; internal set; }
    /// <summary>Gets Ridley's signed 8.8 velocity, each axis stored as its unchanged native word.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Gets the shared wing animation phase, zero through nine, advancing when its timer underflows.</summary>
    public int WingFrame { get; internal set; }
    /// <summary>Gets the wing animation's native countdown accumulator, reloaded to $20 after signed underflow.</summary>
    public WrappingCountdown WingAnimationTimer { get; internal set; }
    /// <summary>Gets the decrement (zero through 48) selected from velocity magnitude for the wing accumulator; nonnegative vertical motion halves it.</summary>
    public int WingAnimationTimerDelta { get; internal set; }
    /// <summary>Gets the shared native tail-controller index; zero retains offsets, one selects neutral control, and the other defined indexes select pogo/stab variants.</summary>
    public RidleyTailFunction TailFunctionIndex { get; internal set; }
    /// <summary>Gets the angular step (zero through eight) used by shared tail segment motion and stagger propagation.</summary>
    public int TailAngleDelta { get; internal set; }
    /// <summary>Gets the facing-dependent lowest angle clockwise tail movement may reach.</summary>
    public RotationAngle TailMinimumClockwiseAngle { get; internal set; }
    /// <summary>Gets the facing-dependent highest angle counterclockwise tail movement may reach.</summary>
    public RotationAngle TailMaximumCounterClockwiseAngle { get; internal set; }
    /// <summary>
    /// Gets the tail angle (see <see cref="RidleyTailSegment.Angle"/>) a clockwise whip stops at, or null while no clockwise
    /// whip is aimed. Aimed targets lie within a quarter turn of straight down.
    /// </summary>
    public int? ClockwiseWhipTarget { get; internal set; }
    /// <summary>
    /// Gets the tail angle (see <see cref="RidleyTailSegment.Angle"/>) a counterclockwise whip stops at, or null while no
    /// counterclockwise whip is aimed. Aimed targets lie within a quarter turn of straight down.
    /// </summary>
    public int? CounterClockwiseWhipTarget { get; internal set; }
    /// <summary>
    /// Gets the pending one-shot whip request, zero or one; its value minus one supplies the additional aim-angle byte
    /// before neutral tail control consumes it.
    /// </summary>
    public int TailWhipRequest { get; internal set; }
    /// <summary>Gets the 8.8 distance increment (240 or 384, in 1/256 pixels) applied while tail segments extend toward their targets.</summary>
    public int TailExtensionSpeed { get; internal set; }
    /// <summary>Gets the angular stagger threshold (eight through 23) used to propagate a wave or whip from one segment to the next.</summary>
    public int IdealInterSegmentTailAngle { get; internal set; }
    /// <summary>Gets the seven native-order tail segments installed by shared initialization; the array is empty before that layout is initialized.</summary>
    public RidleyTailSegment[] TailSegments { get; internal set; } = [];
    /// <summary>Gets whether Ridley's instruction list has enabled the roar presentation.</summary>
    public bool Roaring { get; internal set; }
    /// <summary>Gets the Ceres hover-cycle counter used to choose the next scripted attack; every attack resets it before it passes 124.</summary>
    public int HoverCounter { get; internal set; }
    /// <summary>Where Ridley's typed body program resumes (#641).</summary>
    internal AnimationProgramCursor<RidleyBodyProgram> BodyProgram { get; set; }
    /// <summary>Where each death-breakup fragment's typed program resumes, by its enemy slot (#641).</summary>
    internal Dictionary<int, AnimationProgramCursor<RidleyBreakupFrame>> BreakupPrograms { get; } = [];
    /// <summary>Gets the claw-height pose Ridley's body program last selected: zero standing, one legs half extended, two legs extended.</summary>
    public int FeetDistanceIndex { get; internal set; }
    /// <summary>Gets the Ceres fireball attack's whole-pixel body anchor; firing motion aims at random jitter around this saved position.</summary>
    public Position16<RoomPixels> FireballBasePosition { get; internal set; }
    /// <summary>Gets the signed 8.8 velocity computed from the clamped fireball aim angle.</summary>
    public EightEightVector<RoomPixels> FireballVelocity { get; internal set; }
    /// <summary>$7E:7800, independent swoop phase countdown used by $A6:B441-$B594.</summary>
    public WrappingCountdown SwoopPhaseTimer { get; internal set; }
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
    public WrappingCountdown IntangibilityTimer { get; internal set; }
    /// <summary>Gets the current Lower Norfair movement target's whole-pixel room position.</summary>
    public Position16<RoomPixels> Target { get; internal set; }
    /// <summary>Gets the Lower Norfair pogo bounce counter, reset after its two-bounce threshold.</summary>
    public int PogoBounceCount { get; internal set; }
    /// <summary>Gets the positive 8.8 velocity increment (1/256 pixels per update) selected by health stage for downward pogo motion.</summary>
    public int PogoDownwardAcceleration { get; internal set; }
    /// <summary>Gets the positive 8.8 velocity increment (1/256 pixels per update) selected by health stage for upward pogo motion.</summary>
    public int PogoUpwardAcceleration { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel displacement from the claw anchor; carry updates decay each axis toward zero.</summary>
    public Offset16<RoomPixels> GrabOffset { get; internal set; }
    /// <summary>
    /// Gets the Lower Norfair hit-pressure accumulator, zero through eight: each hurt frame raises it by one and each ordinary
    /// AI frame lowers it by four; at eight a hurt frame also carries a grabbed Samus.
    /// </summary>
    public int HurtMovementClamp { get; internal set; }
    /// <summary>Gets the number of zero-health Lower Norfair lunges completed before the authored death transition.</summary>
    public int ZeroHealthLungeCount { get; internal set; }
    /// <summary>Gets the Lower Norfair small-explosion countdown, reloaded to four for one explosion every five calls.</summary>
    public WrappingCountdown DeathExplosionTimer { get; internal set; }
    /// <summary>Gets the Lower Norfair index, zero through nine, cycling through ten authored death-explosion placements.</summary>
    public int DeathExplosionCount { get; internal set; }
    /// <summary>Gets the whole-pixel room-liquid target height published by Lower Norfair death or combat logic.</summary>
    public int FxTargetYPosition { get; internal set; }
    /// <summary>Gets the authored room-liquid motion delay, in updates, published with Ridley's FX target and velocity.</summary>
    public int FxTimer { get; internal set; }
    /// <summary>Gets whether Lower Norfair's twelve independently moving breakup actors have already been allocated.</summary>
    public bool DeathBreakupSpawned { get; internal set; }
    /// <summary>Gets whether the terminal Lower Norfair death routine has written the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPublished { get; internal set; }
    /// <summary>Gets this enemy frame's death-explosion sound request for the outer audio consumer, or null when none was made.</summary>
    public SoundEffectId? LastDeathSoundEffect { get; internal set; }
    /// <summary>Gets the typed music request published by Lower Norfair death or Ceres self-destruct for the outer music queue.</summary>
    public MusicRequest? MusicRequest { get; internal set; }

    /// <summary>The Ceres Baby's typed cutscene program, positioned after its current pose; owned separately from Ridley's body.</summary>
    internal AnimationProgramCursor<CeresBabyProgram> BabyProgram { get; set; }
    /// <summary>How many draw updates the Baby's current pose lasts; zero before its program shows the first pose.</summary>
    internal int BabyPoseDuration { get; set; }
    /// <summary>Gets the Ceres Baby's elapsed draw-update count in its current pose, compared with the pose's duration and restarted at one.</summary>
    public int BabyInstructionTimer { get; internal set; }
    /// <summary>Gets the Ceres Baby's native bank-$A6 behavior pointer selecting hand attachment, falling setup, falling motion, or idle.</summary>
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
    public int BabyVerticalVelocity { get; internal set; }
    /// <summary>Gets whether Ceres's getaway transform currently owns the Ridley/Baby presentation.</summary>
    public bool GetawayActive { get; internal set; }
    /// <summary>Gets whether the Ceres getaway table reached its terminator and released the active transform.</summary>
    public bool GetawayFinished { get; internal set; }
    /// <summary>Gets the frame index into Ceres's getaway motion table, advanced by one each update (native byte offset twice this).</summary>
    public int GetawayFrameIndex { get; internal set; }
    /// <summary>Gets the native wrapping angle for the Ceres getaway transform; its table index selects sine/cosine coefficients.</summary>
    public RotationAngle FloorAngle { get; internal set; }
    /// <summary>Gets the native signed M7HOFS/M7VOFS offset words applied to the Ceres getaway transform.</summary>
    public Offset16<FloorPixels> FloorOffset { get; internal set; }
    /// <summary>
    /// Gets the getaway's zoom in 1/256 of unit scale, which scales its sine/cosine matrix: 2048 when the getaway
    /// starts, then each table frame's zoom, which the table keeps from zero through 65535.
    /// </summary>
    public int FloorZoom { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 A coefficient, computed from zoom-scaled cosine.</summary>
    public SignedEightEight FloorMatrixA { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 B coefficient, computed from zoom-scaled sine.</summary>
    public SignedEightEight FloorMatrixB { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 C coefficient, the negated B coefficient.</summary>
    public SignedEightEight FloorMatrixC { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 D coefficient, equal to its A coefficient.</summary>
    public SignedEightEight FloorMatrixD { get; internal set; }
    /// <summary>Gets the Ceres getaway's whole-pixel Mode-7 center, initialized to ($40,$40) and cleared on completion.</summary>
    public Position16<FloorPixels> FloorCenter { get; internal set; }
    /// <summary>Gets the four-phase Ceres Baby graphics-transfer animation index (zero through three) during the Mode-7 getaway.</summary>
    public int GetawayBabyFrame { get; internal set; }
    /// <summary>Gets the two-phase Ridley wing graphics-transfer animation index (zero or one) during the Mode-7 getaway.</summary>
    public int GetawayWingFrame { get; internal set; }

    /// <summary>Where the Ceres self-destruct stands in its current page list, advanced as each page is shown (#1406).</summary>
    internal CeresEscapePageCursor CeresEscapePages { get; set; }
    /// <summary>Gets the Ceres warning phase or glyph delay countdown, including the initial 128-frame wait.</summary>
    public WrappingCountdown CeresEscapeTextDelayTimer { get; internal set; }
    /// <summary>Gets the installed Ceres warning typewriter state, created after the initial delay when presentation data is available.</summary>
    public EscapeTypewriterState? CeresEscapeTypewriter { get; internal set; }
    /// <summary>Gets the sixteen-phase Ceres self-destruct alarm-palette frame, zero through fifteen, returning to zero after fifteen.</summary>
    public int CeresEscapePaletteFrame { get; internal set; }
}
