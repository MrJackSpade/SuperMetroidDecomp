using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Named projection of Ridley's common enemy slot and three extended WRAM workspaces.
/// Ceres-only Baby/Mode-7 words and Norfair-only combat words intentionally coexist here:
/// the original shared initializer owns one layout and only one Ridley can exist at a time.
/// </summary>
public sealed class RidleyEnemyState
{
    /// <summary>Gets the live bank-$A6 AI function identity, shared by reveal logic and encounter-specific attack or escape phases.</summary>
    public RidleyAiFunction Function { get; internal set; }
    /// <summary>Gets the native encounter-mode word: zero during reveal, positive during combat, and $FFFF during Lower Norfair death.</summary>
    public RidleyFightMode FightMode { get; internal set; }
    /// <summary>Gets the wrapped count of Ceres hits used for health-palette progression and the hundred-hit fake-retreat trigger.</summary>
    public ushort HitCounter { get; internal set; }
    /// <summary>Gets the native OBJ palette bits used when drawing Ridley's composed body parts.</summary>
    public ushort SpritemapPaletteIndex { get; internal set; }
    /// <summary>Gets the palette word retained for the common enemy draw path alongside Ridley's composed-body palette.</summary>
    public ushort CommonDrawPaletteIndex { get; internal set; }
    /// <summary>Gets the native nonzero enable word for shared body motion and wing/tail updates, cleared when the intact death body is hidden.</summary>
    public ushort MovementAnimationEnabled { get; internal set; }
    /// <summary>Gets the native facing-table index: zero for left, one for front-facing transition art, and two for right.</summary>
    public RidleyFacing FacingDirection { get; internal set; }
    /// <summary>Gets the nonzero enable word for random or proximity-triggered neutral tail whips.</summary>
    public ushort IdleTailWhipEnabled { get; internal set; }
    /// <summary>Gets the shared tail-contact damage value before Samus's suit reduction.</summary>
    public ushort TailDamage { get; internal set; }
    /// <summary>Gets the current AI phase's native countdown word; Ceres self-destruct reuses it as the even dispatch index 0, 2, 4, 6, 8, 10, or 12.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }
    /// <summary>Gets the current row index in the shared eye/body reveal palette fade.</summary>
    public ushort FadePaletteOffset { get; internal set; }
    /// <summary>Gets the movement clamp's minimum whole-pixel corner, retaining negative limits as wrapped native words.</summary>
    public Position16<RoomPixels> MovementMinimum { get; internal set; }
    /// <summary>Gets the movement clamp's maximum whole-pixel corner.</summary>
    public Position16<RoomPixels> MovementMaximum { get; internal set; }
    /// <summary>Nonzero predicate of $A6:D86B's hitARoomBoundary, consumed by $A6:BABC.</summary>
    public bool HitRoomBoundary { get; internal set; }
    /// <summary>Gets Ridley's signed 8.8 velocity, each axis stored as its unchanged native word.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Gets the shared wing animation phase, cycling through ten frames when its timer underflows.</summary>
    public ushort WingFrame { get; internal set; }
    /// <summary>Gets the wing animation's native countdown accumulator, reloaded to $20 after signed underflow.</summary>
    public NativeCountdown WingAnimationTimer { get; internal set; }
    /// <summary>Gets the decrement selected from velocity magnitude for the wing accumulator; nonnegative vertical motion halves it.</summary>
    public ushort WingAnimationTimerDelta { get; internal set; }
    /// <summary>Gets the shared native tail-controller index; zero retains offsets, one selects neutral control, and the other defined indexes select pogo/stab variants.</summary>
    public RidleyTailFunction TailFunctionIndex { get; internal set; }
    /// <summary>Gets the angular step used by shared tail segment motion and stagger propagation.</summary>
    public ushort TailAngleDelta { get; internal set; }
    /// <summary>Gets the facing-dependent lower full-word angle clamp for clockwise tail movement.</summary>
    public ushort TailMinimumClockwiseAngle { get; internal set; }
    /// <summary>Gets the facing-dependent upper full-word angle clamp for counterclockwise tail movement.</summary>
    public ushort TailMaximumCounterClockwiseAngle { get; internal set; }
    /// <summary>Gets the clockwise whip target angle; bit $8000 marks an inactive target.</summary>
    public ushort TailWhipTargetClockwiseAngle { get; internal set; }
    /// <summary>Gets the counterclockwise whip target angle; bit $8000 marks an inactive target.</summary>
    public ushort TailWhipTargetCounterClockwiseAngle { get; internal set; }
    /// <summary>Whether the clockwise whip has no target (bit $8000 of <see cref="TailWhipTargetClockwiseAngle"/>).</summary>
    public bool ClockwiseWhipTargetIdle =>
        (TailWhipTargetClockwiseAngle & RidleyTailDefinitions.InactiveWhipTargetBit) != 0;
    /// <summary>Whether the counterclockwise whip has no target (bit $8000 of <see cref="TailWhipTargetCounterClockwiseAngle"/>).</summary>
    public bool CounterClockwiseWhipTargetIdle =>
        (TailWhipTargetCounterClockwiseAngle & RidleyTailDefinitions.InactiveWhipTargetBit) != 0;
    /// <summary>Gets the pending one-shot whip request; its value minus one supplies the additional aim-angle byte before consumption.</summary>
    public ushort TailWhipRequest { get; internal set; }
    /// <summary>Gets the unsigned 8.8 distance increment applied while tail segments extend toward their targets.</summary>
    public ushort TailExtensionSpeed { get; internal set; }
    /// <summary>Gets the angular stagger threshold used to propagate a wave or whip from one segment to the next.</summary>
    public ushort IdealInterSegmentTailAngle { get; internal set; }
    /// <summary>Gets the seven native-order tail segments installed by shared initialization; the array is empty before that layout is initialized.</summary>
    public RidleyTailSegment[] TailSegments { get; internal set; } = [];
    /// <summary>Gets whether Ridley's instruction list has enabled the roar presentation.</summary>
    public bool Roaring { get; internal set; }
    /// <summary>Gets the Ceres hover-cycle counter used to choose the next scripted attack.</summary>
    public ushort HoverCounter { get; internal set; }
    /// <summary>Where Ridley's typed body program resumes (#641).</summary>
    internal AnimationProgramCursor<RidleyBodyProgram> BodyProgram { get; set; }
    /// <summary>Where each death-breakup fragment's typed program resumes, by its enemy slot (#641).</summary>
    internal Dictionary<int, AnimationProgramCursor<RidleyBreakupFrame>> BreakupPrograms { get; } = [];
    /// <summary>Gets the native feet-position table index installed by Ridley's body animation instructions.</summary>
    public ushort FeetDistanceIndex { get; internal set; }
    /// <summary>Gets the Ceres fireball attack's whole-pixel body anchor; firing motion aims at random jitter around this saved position.</summary>
    public Position16<RoomPixels> FireballBasePosition { get; internal set; }
    /// <summary>Gets the signed 8.8 velocity computed from the clamped fireball aim angle.</summary>
    public EightEightVector<RoomPixels> FireballVelocity { get; internal set; }
    /// <summary>$7E:7800, independent swoop phase countdown used by $A6:B441-$B594.</summary>
    public NativeCountdown SwoopPhaseTimer { get; internal set; }
    /// <summary>Gets the wrapped swoop angle accumulator; its high byte indexes the native sine table.</summary>
    public ushort SwoopAngleAccumulator { get; internal set; }
    /// <summary>Gets the unsigned 8.8 swoop speed magnitude converged toward each phase's target before resolving axis velocities.</summary>
    public ushort SwoopSpeedMagnitude { get; internal set; }

    // Lower Norfair combat state ($7E:78xx/$80xx). These replace opaque numbered words
    // with the meanings established by the corresponding bank-$A6 consumers.
    /// <summary>Gets Lower Norfair's health-table stage from zero through three, changing below 9000, 5400, and 1800 health.</summary>
    public ushort HealthStage { get; internal set; }
    /// <summary>Gets the Lower Norfair nonzero grabbed-Samus state controlling claw attachment and release.</summary>
    public ushort GrabState { get; internal set; }
    /// <summary>Gets the Lower Norfair countdown keeping the body intangible after a grab release.</summary>
    public NativeCountdown IntangibilityTimer { get; internal set; }
    /// <summary>Gets the current Lower Norfair movement target's whole-pixel room position.</summary>
    public Position16<RoomPixels> Target { get; internal set; }
    /// <summary>Gets the Lower Norfair pogo bounce counter, reset after its two-bounce threshold.</summary>
    public ushort PogoBounceCount { get; internal set; }
    /// <summary>Gets the native 8.8 velocity increment selected by health stage for downward pogo motion.</summary>
    public ushort PogoDownwardAcceleration { get; internal set; }
    /// <summary>Gets the native 8.8 velocity increment selected by health stage for upward pogo motion.</summary>
    public ushort PogoUpwardAcceleration { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel displacement from the claw anchor; carry updates decay each axis toward zero.</summary>
    public Offset16<RoomPixels> GrabOffset { get; internal set; }
    /// <summary>Gets the Lower Norfair hit-pressure accumulator, increased by shots and decayed during AI; its threshold can force a grabbed-Samus release.</summary>
    public ushort HurtMovementClamp { get; internal set; }
    /// <summary>Gets the number of zero-health Lower Norfair lunges completed before the authored death transition.</summary>
    public ushort ZeroHealthLungeCount { get; internal set; }
    /// <summary>Gets the Lower Norfair small-explosion countdown, reloaded to four for one explosion every five calls.</summary>
    public NativeCountdown DeathExplosionTimer { get; internal set; }
    /// <summary>Gets the Lower Norfair index cycling through ten authored death-explosion placements.</summary>
    public ushort DeathExplosionCount { get; internal set; }
    /// <summary>Gets the native whole-pixel room-liquid target published by Lower Norfair death or combat logic.</summary>
    public ushort FxTargetYPosition { get; internal set; }
    /// <summary>Gets the packed signed native 8.8 room-liquid vertical velocity published to the room FX owner.</summary>
    public ushort FxYSubVelocity { get; internal set; }
    /// <summary>Gets the authored room-liquid motion delay published with Ridley's FX target and velocity.</summary>
    public ushort FxTimer { get; internal set; }
    /// <summary>Gets whether Lower Norfair's twelve independently moving breakup actors have already been allocated.</summary>
    public bool DeathBreakupSpawned { get; internal set; }
    /// <summary>Gets whether the terminal Lower Norfair death routine has written the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPublished { get; internal set; }
    /// <summary>Gets the most recent death sound request, including the $24 small-explosion sound, for the outer audio consumer.</summary>
    public ushort? LastDeathSoundEffect { get; internal set; }
    /// <summary>Gets the typed music request published by Lower Norfair death or Ceres self-destruct for the outer music queue.</summary>
    public MusicCommand? MusicRequest { get; internal set; }

    // Ceres's private Baby actor and Mode-7 getaway reuse Ridley's extended workspaces.
    /// <summary>The Ceres Baby's typed cutscene program, positioned after its current pose; owned separately from Ridley's body.</summary>
    internal AnimationProgramCursor<CeresBabyProgram> BabyProgram { get; set; }
    /// <summary>How many draw updates the Baby's current pose lasts; zero before its program shows the first pose.</summary>
    internal int BabyPoseDuration { get; set; }
    /// <summary>Gets the Ceres Baby's elapsed draw-frame counter, compared with the current pose's duration and restarted at one.</summary>
    public ushort BabyInstructionTimer { get; internal set; }
    /// <summary>Gets the Ceres Baby's native bank-$A6 behavior pointer selecting hand attachment, falling setup, falling motion, or idle.</summary>
    public CeresBabyFunction BabyFunction { get; internal set; }
    /// <summary>Gets the current native spritemap identity selected by the Ceres Baby's animation list.</summary>
    public ushort BabyCurrentSpritemap { get; internal set; }
    /// <summary>Gets the Ceres Baby's wrapped whole-pixel room position, updated while falling or attached to a hand.</summary>
    public Position16<RoomPixels> BabyPosition { get; internal set; }
    /// <summary>Gets the Ceres Baby's sixteen-bit fractional Y position used by its falling integrator.</summary>
    public ushort BabyYSubposition { get; internal set; }
    /// <summary>Gets the Ceres Baby's signed 8.8 falling velocity, with a native increment of eight each falling update.</summary>
    public ushort BabyVerticalVelocity { get; internal set; }
    /// <summary>Gets whether Ceres's getaway transform currently owns the Ridley/Baby presentation.</summary>
    public bool Mode7Active { get; internal set; }
    /// <summary>Gets whether the Ceres getaway table reached its terminator and released the active transform.</summary>
    public bool Mode7Finished { get; internal set; }
    /// <summary>Gets the even byte offset into Ceres's getaway motion table, advanced by two each update.</summary>
    public ushort Mode7TableByteIndex { get; internal set; }
    /// <summary>Gets the native wrapping angle for the Ceres getaway transform; its table index selects sine/cosine coefficients.</summary>
    public SnesAngle Mode7Angle { get; internal set; }
    /// <summary>Gets the native signed M7HOFS/M7VOFS offset words applied to the Ceres getaway transform.</summary>
    public Offset16<Mode7Pixels> Mode7Offset { get; internal set; }
    /// <summary>Gets the native 8.8 zoom magnitude selected by Ceres's getaway table and used to scale its sine/cosine matrix.</summary>
    public ushort Mode7Zoom { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 A coefficient, computed from zoom-scaled cosine.</summary>
    public ushort Mode7MatrixA { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 B coefficient, computed from zoom-scaled sine.</summary>
    public ushort Mode7MatrixB { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 C coefficient, the negated B coefficient.</summary>
    public ushort Mode7MatrixC { get; internal set; }
    /// <summary>Gets the Ceres getaway's signed 8.8 Mode-7 D coefficient, equal to its A coefficient.</summary>
    public ushort Mode7MatrixD { get; internal set; }
    /// <summary>Gets the Ceres getaway's whole-pixel Mode-7 center, initialized to ($40,$40) and cleared on completion.</summary>
    public Position16<Mode7Pixels> Mode7Center { get; internal set; }
    /// <summary>Gets the four-phase Ceres Baby graphics-transfer animation index during the Mode-7 getaway.</summary>
    public ushort Mode7BabyFrame { get; internal set; }
    /// <summary>Gets the two-phase Ridley wing graphics-transfer animation index during the Mode-7 getaway.</summary>
    public ushort Mode7WingFrame { get; internal set; }

    // Ceres self-destruct presentation ($A6:C04E-$C135). FunctionTimer deliberately
    // retains the native phase values 0,2,4,6,8,10,12 while these words project the
    // extended enemy workspaces used by the DMA-list and optional Japanese typewriter.
    /// <summary>Gets the native cursor in the Ceres self-destruct seven-byte VRAM transfer records, advanced as each record is queued.</summary>
    public ushort CeresEscapeTransferListPointer { get; internal set; }
    /// <summary>Gets the Ceres warning typewriter's native text-program cursor mirrored from its installed program state.</summary>
    public ushort CeresEscapeTextPointer { get; internal set; }
    /// <summary>Gets the Ceres warning typewriter's current tilemap destination offset mirrored from its program state.</summary>
    public ushort CeresEscapeTextDestination { get; internal set; }
    /// <summary>Gets the Ceres warning phase or glyph delay countdown, including the initial 128-frame wait.</summary>
    public ushort CeresEscapeTextDelayTimer { get; internal set; }
    /// <summary>Gets the Ceres warning typewriter's current inter-glyph delay mirrored from its program state.</summary>
    public ushort CeresEscapeTextDelay { get; internal set; }
    /// <summary>Gets the Ceres typewriter's alternating glyph-sound phase, mirrored as the low bit of glyphs written.</summary>
    public ushort CeresEscapeTextSoundCounter { get; internal set; }
    /// <summary>Gets the installed Ceres warning typewriter state, created after the initial delay when presentation data is available.</summary>
    public EscapeTypewriterState? CeresEscapeTypewriter { get; internal set; }
    /// <summary>Gets the sixteen-phase Ceres self-destruct alarm-palette frame, wrapping with mask $0F.</summary>
    public ushort CeresEscapePaletteFrame { get; internal set; }
}
