namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A6 function pointers used by both Ridley encounters. Keeping the original
/// addresses in debugger-visible state makes every transition directly comparable with
/// a WRAM trace and prevents host-only ordinals from obscuring shared cartridge code.
/// </summary>
public enum RidleyAiFunction : ushort
{
    /// <summary>$A6:A354: clears shared motion before returning to the flying dispatcher.</summary>
    ClearVelocity = 0xa354,
    /// <summary>$A6:A35B: waits for the room-entry door transition before starting Ridley's reveal.</summary>
    WaitForDoorTransition = 0xa35b,
    /// <summary>$A6:A377: counts the initial reveal delay before fading the eyes.</summary>
    InitialDelay = 0xa377,
    /// <summary>$A6:A389: advances the shared eye-palette fade before body reveal.</summary>
    FadeInEyes = 0xa389,
    /// <summary>$A6:A3DF: advances the body-palette fade and prepares the roar.</summary>
    FadeInBody = 0xa3df,
    /// <summary>$A6:A455: waits before selecting the pre-flight roar animation.</summary>
    WaitBeforeRoar = 0xa455,
    /// <summary>$A6:A478: holds the pre-flight roar before encounter-specific liftoff.</summary>
    WaitBeforeLiftoff = 0xa478,
    /// <summary>$A6:A6AF: accelerates the Ceres body upward during liftoff.</summary>
    CeresLiftoffAccelerating = 0xa6af,
    /// <summary>$A6:A6C8: decelerates Ceres liftoff into the hovering phase.</summary>
    CeresLiftoffDecelerating = 0xa6c8,
    /// <summary>$A6:A6E8: runs Ceres hovering and chooses its next scripted attack.</summary>
    CeresHovering = 0xa6e8,
    /// <summary>$A6:A782: moves the Ceres body to its fireball firing position.</summary>
    CeresFireballMoveToPosition = 0xa782,
    /// <summary>$A6:A7F9: maintains the Ceres fireball attack before returning to hover.</summary>
    CeresFireballShooting = 0xa7f9,
    /// <summary>$A6:A83C: initializes the Ceres lunge movement and phase timer.</summary>
    CeresLungeSetup = 0xa83c,
    /// <summary>$A6:A84E: advances the Ceres lunge until recovery.</summary>
    CeresLungeMain = 0xa84e,
    /// <summary>$A6:A88D: initializes the Ceres downward-aiming swoop.</summary>
    CeresSwoopSetup = 0xa88d,
    /// <summary>$A6:A8A4: steers Ceres Ridley to the swoop start position.</summary>
    CeresSwoopMoveToPosition = 0xa8a4,
    /// <summary>$A6:A8D4: descends through the downward-aiming portion of the Ceres swoop.</summary>
    CeresSwoopDescendingAimingDown = 0xa8d4,
    /// <summary>$A6:A8F8: turns the descending Ceres swoop toward a leftward aim.</summary>
    CeresSwoopDescendingAimingLeft = 0xa8f8,
    /// <summary>$A6:A923: starts the upward-aiming ascent of the Ceres swoop.</summary>
    CeresSwoopAscendingAimingUp = 0xa923,
    /// <summary>$A6:A947: continues the faster upward-aiming Ceres swoop ascent.</summary>
    CeresSwoopAscendingAimingUpFaster = 0xa947,
    /// <summary>$A6:A971: raises Ceres Ridley beyond the room for the real getaway.</summary>
    CeresRealRetreatRising = 0xa971,
    /// <summary>$A6:A9A0: prepares retreat palettes and walls while waiting to publish the getaway.</summary>
    CeresRetreatDelay = 0xa9a0,
    /// <summary>$A6:AA11: publishes the Ceres Mode-7 getaway handoff.</summary>
    CeresPublishEscapeHandoff = 0xaa11,
    /// <summary>$A6:AA1B: the inactive Ceres body entry while its getaway presentation owns the scene.</summary>
    CeresInactive = 0xaa1b,
    /// <summary>$A6:C04E: advances Ceres self-destruct transfers, countdown setup, and optional Japanese typewriter phases.</summary>
    CeresActivateSelfDestruct = 0xc04e,
    /// <summary>$A6:AA50: retains the Ceres alarm-palette update after self-destruct setup.</summary>
    CeresSelfDestructPaletteOnly = 0xaa50,

    // Lower Norfair Ridley begins at $B2F3 after the shared reveal sequence. These names
    // describe the native movement/attack phases; their values are not host inventions.
    /// <summary>$A6:B2F3: moves Lower Norfair Ridley into the arena after the shared reveal.</summary>
    NorfairEnterArena = 0xb2f3,
    /// <summary>$A6:B321: selects Lower Norfair Ridley's next action from combat state.</summary>
    NorfairSelectAttack = 0xb321,
    /// <summary>$A6:B3EC: the native missed-lunge setup entry, preparing the translated hover recovery.</summary>
    NorfairHoverSetup = 0xb3ec,
    /// <summary>$A6:B3F8: the native missed-lunge move-to-position entry, advancing translated hover recovery.</summary>
    NorfairHover = 0xb3f8,
    /// <summary>$A6:B441: initializes the Lower Norfair swoop phase countdown and motion.</summary>
    NorfairSwoopSetup = 0xb441,
    /// <summary>$A6:B455: moves Lower Norfair Ridley to its swoop start.</summary>
    NorfairSwoopMoveToStart = 0xb455,
    /// <summary>$A6:B493: descends through the downward-aiming Lower Norfair swoop phase.</summary>
    NorfairSwoopAimDown = 0xb493,
    /// <summary>$A6:B4D1: rotates the descending Lower Norfair swoop toward horizontal aim.</summary>
    NorfairSwoopAimSideways = 0xb4d1,
    /// <summary>$A6:B516: transitions the Lower Norfair swoop into upward aim.</summary>
    NorfairSwoopAimUp = 0xb516,
    /// <summary>$A6:B554: accelerates the upward-aiming Lower Norfair swoop ascent.</summary>
    NorfairSwoopClimb = 0xb554,
    /// <summary>$A6:B594: decelerates the swoop and chooses attack selection or grab approach.</summary>
    NorfairSwoopRecover = 0xb594,
    /// <summary>$A6:B5C4: the native hover-action entry used by the translated pogo setup dispatcher.</summary>
    NorfairPogoSetup = 0xb5c4,
    /// <summary>$A6:B5E5: the native hover move-to-wall-behind entry used by the translated descending pogo phase.</summary>
    NorfairPogoDescending = 0xb5e5,
    /// <summary>$A6:B613: the native hover move-to-wall-in-front entry used by the translated ascending pogo phase.</summary>
    NorfairPogoAscending = 0xb613,
    /// <summary>$A6:B68B: the native pogo fly-to-position entry, initializing the translated ground-attack sequence.</summary>
    NorfairFireballSetup = 0xb68b,
    /// <summary>$A6:B6A7: the translated ground-attack side-position phase.</summary>
    NorfairFireballMoveToSide = 0xb6a7,
    /// <summary>$A6:B6DD: the translated ground-attack height-position phase.</summary>
    NorfairFireballMoveToHeight = 0xb6dd,
    /// <summary>$A6:B70E: the native pogo descending entry used by the translated ground-attack dispatcher.</summary>
    NorfairFireballAttack = 0xb70e,
    /// <summary>$A6:B7B9: the translated ground-attack recovery phase before choosing the next action.</summary>
    NorfairFireballRecover = 0xb7b9,
    /// <summary>$A6:BAB7: the native lunge action used to approach and grab Samus.</summary>
    NorfairGrabApproach = 0xbab7,
    /// <summary>$A6:BB8F: initializes the Lower Norfair carry state after grabbing Samus.</summary>
    NorfairCarrySetup = 0xbb8f,
    /// <summary>$A6:BBC4: moves the grabbed Samus toward the carry anchor.</summary>
    NorfairCarryMoveToAnchor = 0xbbc4,
    /// <summary>$A6:BBF1: rises while carrying Samus until the release timer expires.</summary>
    NorfairCarryRise = 0xbbf1,
    /// <summary>$A6:BC2E: recovers after releasing Samus and restores ordinary tail spacing.</summary>
    NorfairCarryRelease = 0xbc2e,
    /// <summary>$A6:BD4E, Function_Ridley_DodgingPowerBomb: recover from a lunge while a bomb is armed.</summary>
    NorfairReturnToArena = 0xbd4e,

    // The following functions are shared with Ceres's Baby retrieval route but are only
    // selected during the real fight when Ridley has grabbed Samus.
    /// <summary>$A6:BD9A: steers the fake Ceres retreat to its start; the shared entry can also be selected during a Lower Norfair grab.</summary>
    CeresFakeRetreatMoveToPosition = 0xbd9a,
    /// <summary>$A6:BDBC: raises Ceres Ridley until the Baby starts falling; shared with the grab route.</summary>
    CeresFakeRetreatRising = 0xbdbc,
    /// <summary>$A6:BDF2: starts the Baby's fall and waits before the Ceres retrieval animation.</summary>
    CeresWaitBeforeRetrievingBaby = 0xbdf2,
    /// <summary>$A6:BE03: steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    CeresRetrieveBaby = 0xbe03,

    /// <summary>$A6:C538: moves zero-health Ridley to the death spot while retaining any grabbed Samus.</summary>
    NorfairReleaseSamus = 0xc538,
    /// <summary>$A6:C53E: selects the death-roar instruction list and starts its thirty-two-frame phase.</summary>
    NorfairDeathStart = 0xc53e,
    /// <summary>$A6:C551: continues death-spot convergence during the roar, then stops motion and starts acid drainage.</summary>
    NorfairDeathExplosions = 0xc551,
    /// <summary>$A6:C588: emits small explosions while acid drains; expiry releases Samus and allocates breakup actors.</summary>
    NorfairDeathFall = 0xc588,
    /// <summary>$A6:C5A8: hides the intact body and disables shared wing/tail animation after breakup allocation.</summary>
    NorfairDeathImpact = 0xc5a8,
    /// <summary>$A6:C5C8: counts the deliberate thirty-two-frame wait before the terminal death delay.</summary>
    NorfairDeathWait = 0xc5c8,
    /// <summary>$A6:C5DA: completes the terminal wait, publishes boss persistence, scatters drops, requests music, and deletes the body.</summary>
    NorfairDeathFinish = 0xc5da,
    /// <summary>$A6:C600: terminal RTS selected by $C5FD after drops, music and deletion.</summary>
    NorfairDeathComplete = 0xc600,
}

/// <summary>
/// One of the seven twenty-byte logical tail entries initialized by $A6:D2D6. The tail
/// is shared by Ceres Ridley and Lower Norfair Ridley; encounter-specific controllers only
/// change these common targets and activation words.
/// </summary>
public sealed class RidleyTailSegment
{
    /// <summary>Gets whether this tail segment is participating in the current wave or whip; deactivation resets its stagger and reverses its movement direction.</summary>
    public bool Active { get; internal set; }
    /// <summary>Gets the accumulated angular stagger before this segment activates the next segment; $FFFF marks propagation already performed.</summary>
    public ushort StaggerAngle { get; internal set; }
    /// <summary>Gets the native direction word; bit $8000 selects clockwise/decreasing-angle motion.</summary>
    public ushort MovementDirection { get; internal set; }
    /// <summary>Gets the live unsigned 8.8 distance from the preceding tail anchor; its high byte supplies whole pixels for offset composition.</summary>
    public ushort Distance { get; internal set; }

    /// <summary>
    /// Nonzero while this segment is extending for a whip. Native stores this separately
    /// from the live radius and clears it after the live distance passes the request.
    /// </summary>
    public ushort TargetDistance { get; internal set; }

    /// <summary>Gets the wrapped native tail angle; the low byte indexes the sine table and the full word retains winding and clamp state.</summary>
    public ushort Angle { get; internal set; }
    /// <summary>Gets the signed whole-pixel horizontal displacement from the preceding anchor, stored unchanged in a sixteen-bit word.</summary>
    public ushort XOffset { get; internal set; }
    /// <summary>Gets the signed whole-pixel vertical displacement from the preceding anchor, stored unchanged in a sixteen-bit word.</summary>
    public ushort YOffset { get; internal set; }
    /// <summary>Gets the composed sixteen-bit world X coordinate used for drawing and tail contact.</summary>
    public ushort XPosition { get; internal set; }
    /// <summary>Gets the composed sixteen-bit world Y coordinate used for drawing and tail contact.</summary>
    public ushort YPosition { get; internal set; }
}

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
    public ushort FightMode { get; internal set; }
    /// <summary>Gets the wrapped count of Ceres hits used for health-palette progression and the hundred-hit fake-retreat trigger.</summary>
    public ushort HitCounter { get; internal set; }
    /// <summary>Gets the native OBJ palette bits used when drawing Ridley's composed body parts.</summary>
    public ushort SpritemapPaletteIndex { get; internal set; }
    /// <summary>Gets the palette word retained for the common enemy draw path alongside Ridley's composed-body palette.</summary>
    public ushort CommonDrawPaletteIndex { get; internal set; }
    /// <summary>Gets the native nonzero enable word for shared body motion and wing/tail updates, cleared when the intact death body is hidden.</summary>
    public ushort MovementAnimationEnabled { get; internal set; }
    /// <summary>Gets the native facing-table index: zero for left, one for front-facing transition art, and two for right.</summary>
    public ushort FacingDirection { get; internal set; }
    /// <summary>Gets the nonzero enable word for random or proximity-triggered neutral tail whips.</summary>
    public ushort IdleTailWhipEnabled { get; internal set; }
    /// <summary>Gets the shared tail-contact damage value before Samus's suit reduction.</summary>
    public ushort TailDamage { get; internal set; }
    /// <summary>Gets the current AI phase's native countdown word; Ceres self-destruct reuses it as the even dispatch index 0, 2, 4, 6, 8, 10, or 12.</summary>
    public ushort FunctionTimer { get; internal set; }
    /// <summary>Gets the current row index in the shared eye/body reveal palette fade.</summary>
    public ushort FadePaletteOffset { get; internal set; }
    /// <summary>Gets the movement clamp's minimum whole-pixel Y boundary, retaining negative limits as wrapped native words.</summary>
    public ushort MinimumY { get; internal set; }
    /// <summary>Gets the movement clamp's maximum whole-pixel Y boundary.</summary>
    public ushort MaximumY { get; internal set; }
    /// <summary>Gets the movement clamp's minimum whole-pixel X boundary.</summary>
    public ushort MinimumX { get; internal set; }
    /// <summary>Gets the movement clamp's maximum whole-pixel X boundary.</summary>
    public ushort MaximumX { get; internal set; }
    /// <summary>Nonzero predicate of $A6:D86B's hitARoomBoundary, consumed by $A6:BABC.</summary>
    public bool HitRoomBoundary { get; internal set; }
    /// <summary>Gets Ridley's signed 8.8 horizontal velocity stored as its unchanged native word.</summary>
    public ushort HorizontalVelocity { get; internal set; }
    /// <summary>Gets Ridley's signed 8.8 vertical velocity stored as its unchanged native word.</summary>
    public ushort VerticalVelocity { get; internal set; }
    /// <summary>Gets the shared wing animation phase, cycling through ten frames when its timer underflows.</summary>
    public ushort WingFrame { get; internal set; }
    /// <summary>Gets the wing animation's native countdown accumulator, reloaded to $20 after signed underflow.</summary>
    public ushort WingAnimationTimer { get; internal set; }
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
    /// <summary>Gets the native feet-position table index installed by Ridley's body animation instructions.</summary>
    public ushort FeetDistanceIndex { get; internal set; }
    /// <summary>Gets the Ceres fireball attack's whole-pixel body X anchor; firing motion aims at random jitter around this saved position.</summary>
    public ushort FireballBaseXPosition { get; internal set; }
    /// <summary>Gets the Ceres fireball attack's whole-pixel body Y anchor; firing motion aims at random jitter around this saved position.</summary>
    public ushort FireballBaseYPosition { get; internal set; }
    /// <summary>Gets the signed 8.8 horizontal velocity computed from the clamped fireball aim angle.</summary>
    public ushort FireballXVelocity { get; internal set; }
    /// <summary>Gets the signed 8.8 vertical velocity computed from the clamped fireball aim angle.</summary>
    public ushort FireballYVelocity { get; internal set; }
    /// <summary>$7E:7800, independent swoop phase countdown used by $A6:B441-$B594.</summary>
    public ushort SwoopPhaseTimer { get; internal set; }
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
    public ushort IntangibilityTimer { get; internal set; }
    /// <summary>Gets the current Lower Norfair movement target's whole-pixel world X coordinate.</summary>
    public ushort TargetX { get; internal set; }
    /// <summary>Gets the current Lower Norfair movement target's whole-pixel world Y coordinate.</summary>
    public ushort TargetY { get; internal set; }
    /// <summary>Gets the Lower Norfair pogo bounce counter, reset after its two-bounce threshold.</summary>
    public ushort PogoBounceCount { get; internal set; }
    /// <summary>Gets the native 8.8 velocity increment selected by health stage for downward pogo motion.</summary>
    public ushort PogoDownwardAcceleration { get; internal set; }
    /// <summary>Gets the native 8.8 velocity increment selected by health stage for upward pogo motion.</summary>
    public ushort PogoUpwardAcceleration { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel X displacement from the claw anchor; carry updates decay it toward zero.</summary>
    public ushort GrabXOffset { get; internal set; }
    /// <summary>Gets Samus's signed whole-pixel Y displacement from the claw anchor; carry updates decay it toward zero.</summary>
    public ushort GrabYOffset { get; internal set; }
    /// <summary>Gets the Lower Norfair hit-pressure accumulator, increased by shots and decayed during AI; its threshold can force a grabbed-Samus release.</summary>
    public ushort HurtMovementClamp { get; internal set; }
    /// <summary>Gets the number of zero-health Lower Norfair lunges completed before the authored death transition.</summary>
    public ushort ZeroHealthLungeCount { get; internal set; }
    /// <summary>Gets the Lower Norfair small-explosion countdown, reloaded to four for one explosion every five calls.</summary>
    public ushort DeathExplosionTimer { get; internal set; }
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
    /// <summary>Gets the Ceres Baby's bank-$A6 animation-list offset, owned separately from Ridley's body instruction list.</summary>
    public ushort BabyInstruction { get; internal set; }
    /// <summary>Gets the Ceres Baby's elapsed draw-frame counter, compared with the current animation duration and restarted at one.</summary>
    public ushort BabyInstructionTimer { get; internal set; }
    /// <summary>Gets the Ceres Baby's native bank-$A6 behavior pointer selecting hand attachment, falling setup, falling motion, or idle.</summary>
    public CeresBabyFunction BabyFunction { get; internal set; }
    /// <summary>Gets the current native spritemap identity selected by the Ceres Baby's animation list.</summary>
    public ushort BabyCurrentSpritemap { get; internal set; }
    /// <summary>Gets the Ceres Baby's wrapped whole-pixel world X coordinate, independently updated while falling or attached to a hand.</summary>
    public ushort BabyXPosition { get; internal set; }
    /// <summary>Gets the Ceres Baby's wrapped whole-pixel world Y coordinate.</summary>
    public ushort BabyYPosition { get; internal set; }
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
    /// <summary>Gets the native signed horizontal offset word applied to the Ceres getaway transform.</summary>
    public ushort Mode7HorizontalOffset { get; internal set; }
    /// <summary>Gets the native signed vertical offset word applied to the Ceres getaway transform.</summary>
    public ushort Mode7VerticalOffset { get; internal set; }
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
    /// <summary>Gets the Ceres getaway's whole-pixel Mode-7 horizontal center, initialized to $40 and cleared on completion.</summary>
    public ushort Mode7CenterX { get; internal set; }
    /// <summary>Gets the Ceres getaway's whole-pixel Mode-7 vertical center, initialized to $40 and cleared on completion.</summary>
    public ushort Mode7CenterY { get; internal set; }
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
