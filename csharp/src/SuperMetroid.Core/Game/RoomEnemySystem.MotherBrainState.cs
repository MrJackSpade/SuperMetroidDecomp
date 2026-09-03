namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers used by Mother Brain's body dispatcher in bank <c>$A9</c>.
/// Keeping the cartridge addresses as enum values makes a debugger watch line up with the
/// disassembly without pretending that unrelated phases are interchangeable host states.
/// </summary>
public enum MotherBrainBodyFunction : ushort
{
    FirstPhase = 0x87e1,
    FakeDeathDescentInitialPause = 0x881d,
    FakeDeathDescentPauseBeforeLock = 0x8829,
    FakeDeathDescentPauseBeforeMusic = 0x884d,
    FakeDeathDescentPauseBeforeUnlock = 0x886c,
    FakeDeathDescentPauseBeforeFlash = 0x8884,
    FakeDeathDescentFadeToGray = 0x88b2,
    FakeDeathDescentCollapseTubes = 0x88d3,
    FakeDeathAscentDrawRows2And3 = 0x8c87,
    FakeDeathAscentDrawRows4And5 = 0x8c9e,
    FakeDeathAscentDrawRows6And7 = 0x8cb5,
    FakeDeathAscentDrawRows8And9 = 0x8ccc,
    FakeDeathAscentDrawRowsAAndB = 0x8ce3,
    FakeDeathAscentDrawRowsCAndD = 0x8cfa,
    FakeDeathAscentSetupPhase2Graphics = 0x8d11,
    FakeDeathAscentSetupPhase2Brain = 0x8d49,
    FakeDeathAscentPauseForSuspense = 0x8d79,
    FakeDeathAscentPrepareForRising = 0x8d8b,
    FakeDeathAscentLoadLegTiles = 0x8db4,
    FakeDeathAscentContinuePausing = 0x8dc3,
    FakeDeathAscentStartMusicAndEarthquake = 0x8dec,
    FakeDeathAscentRaiseMotherBrain = 0x8e4d,
    FakeDeathAscentWaitUntilUncrouched = 0x8e95,
    FakeDeathAscentTransitionFromGray = 0x8eaa,
    SecondPhaseStretchingShakeHead = 0x8ef5,
    SecondPhaseStretchingBringHeadUp = 0x8f14,
    SecondPhaseStretchingFinish = 0x8f33,
    SecondPhaseThinking = 0xb605,
    SecondPhaseTryAttack = 0xb64b,
    SecondPhaseBombDecideWalking = 0xb781,
    SecondPhaseBombWalkingBackwards = 0xb7ac,
    SecondPhaseBombCrouch = 0xb7c6,
    SecondPhaseBombFired = 0xb7e8,
    SecondPhaseBombStandUp = 0xb7f8,
    SecondPhaseLaserPositionHeadQuickly = 0xb80e,
    SecondPhaseLaserPositionHeadSlowlyAndFire = 0xb839,
    SecondPhaseLaserFinishAttack = 0xb863,
    SecondPhaseHandBeam = 0xb87d,
    SecondPhaseRainbowExtendNeck = 0xb8eb,
    SecondPhaseRainbowStartCharging = 0xb91a,
    SecondPhaseRainbowRetractNeck = 0xb92b,
    SecondPhaseRainbowWaitForCharge = 0xb93f,
    SecondPhaseRainbowExtendNeckDown = 0xb951,
    SecondPhaseRainbowStartFiring = 0xb975,
    SecondPhaseRainbowMoveSamusTowardWall = 0xb9e5,
    SecondPhaseRainbowOneFrameDelay = 0xba00,
    SecondPhaseRainbowStartDrainingSamus = 0xba27,
    SecondPhaseRainbowDrainingSamus = 0xba3c,
    SecondPhaseRainbowFinishFiring = 0xba5e,
    SecondPhaseRainbowLetSamusFall = 0xbac4,
    SecondPhaseRainbowWaitForSamusToLand = 0xbad1,
    SecondPhaseRainbowLowerHead = 0xbadd,
    SecondPhaseRainbowDecideNextAction = 0xbb06,
    SecondPhaseFinishSamusOff = 0xbd45,
    SecondPhaseFinishSamusOffStandUp = 0xbd98,
    SecondPhaseFinishSamusOffAdmire = 0xbda9,
    SecondPhaseFinishSamusOffChargeFinalBeam = 0xbdc1,
    SecondPhaseFinishSamusOffLoadBabyTiles = 0xbdd2,
    SecondPhaseFinishSamusOffFireFinalBeam = 0xbded,
    SecondPhaseFinalRainbowBeamHolding = 0xbe1a,
    SecondPhaseDrainedByBabyTakenAback = 0xbe38,
    SecondPhaseDrainedByBabyRegainBalance = 0xbe5d,
    SecondPhaseDrainedByBabyFiringRainbowBeam = 0xbe96,
    SecondPhaseDrainedByBabyRainbowBeamRunOut = 0xbf0e,
    SecondPhaseDrainedByBabyMoveToBackOfRoom = 0xbf41,
    SecondPhaseDrainedByBabyGoIntoLowPowerMode = 0xbf56,
    SecondPhaseDrainedByBabyPrepareTransitionToGrey = 0xbf7d,
    SecondPhaseDrainedByBabyTransitionToGrey = 0xbf95,
    SecondPhaseReviveInanimateGrey = 0xc059,
    SecondPhaseReviveShowSignsOfLife = 0xc066,
    SecondPhaseReviveTransitionFromGrey = 0xc08f,
    SecondPhaseReviveWakeUp = 0xc0ba,
    SecondPhaseReviveWakeUpStretch = 0xc0e4,
    SecondPhaseReviveWalkUpToBaby = 0xc0fb,
    SecondPhaseRevivePrepareNeckForBabyDeath = 0xc11e,
    SecondPhaseReviveFinishPreparingForBabyDeath = 0xc147,
    SecondPhaseMurderBabyAttack = 0xc15c,
    SecondPhaseMurderBabyAttackCooldown = 0xc182,
    SecondPhasePrepareForFinalBabyAttack = 0xc18e,
    SecondPhaseExecuteFinalBabyAttack = 0xc19a,
    SecondPhaseFinalBabyAttackHolding = 0xc1a6,
    ThirdPhaseRecoverMakeSomeDistance = 0xc1cf,
    ThirdPhaseRecoverSetupForFighting = 0xc1f0,
    ThirdPhaseFightingMain = 0xc209,
    ThirdPhaseFightingAttackCooldown = 0xc24e,
}

/// <summary>
/// Index into Mother Brain's three-entry phase-two attack dispatcher at
/// <c>$A9:B654</c>. The values are array indices in the cartridge, not host-only states.
/// </summary>
public enum MotherBrainAttackPhase : ushort
{
    ChooseAttack = 0,
    Cooldown = 1,
    EndAttack = 2,
}

/// <summary>
/// Index into Mother Brain's four-entry hand-beam dispatcher at <c>$A9:B887</c>. Phase two
/// is deliberately bytecode-owned: the body list advances it only after its complete
/// charge/fire/hold animation has elapsed.
/// </summary>
public enum MotherBrainHandBeamPhase : ushort
{
    BackUp = 0,
    WaitForBombs = 1,
    Firing = 2,
    Finish = 3,
}

/// <summary>Values written by Mother Brain's body instruction opcodes at $A9:9700-$972F.</summary>
public enum MotherBrainBodyPose : ushort
{
    Standing = 0,
    Walking = 1,
    CrouchingTransition = 2,
    Crouched = 3,
    DeathBeam = 4,
    LeaningDown = 6,
}

/// <summary>
/// Native function pointers stored in the head record's secondary Mother Brain dispatcher.
/// During fake death this independent state machine serializes the physical tube actors,
/// ceiling projectiles, and one-frame bank-$84 room mutations.
/// </summary>
public enum MotherBrainTubeCollapseFunction : ushort
{
    WaitForFourFreeProjectileSlots = 0x8949,
    ClearBottomLeftTube = 0x896e,
    SpawnTopRightTube = 0x8983,
    ClearCeilingColumn9 = 0x89a0,
    SpawnTopLeftTube = 0x89b5,
    ClearCeilingColumn6 = 0x89d2,
    SpawnBottomRightTube = 0x89e7,
    ClearBottomRightTube = 0x89fa,
    SpawnBottomMiddleLeftTube = 0x8a0f,
    ClearBottomMiddleLeftTube = 0x8a22,
    SpawnTopMiddleLeftTube = 0x8a37,
    ClearCeilingColumn7 = 0x8a54,
    SpawnTopMiddleRightTube = 0x8a69,
    ClearCeilingColumn8 = 0x8a86,
    SpawnBottomMiddleRightTube = 0x8a9b,
    ClearBottomMiddleRightTube = 0x8aae,
    SpawnMainTube = 0x8ac3,
    ClearBottomMiddleTubes = 0x8ad6,
    Finished = 0x8ae4,
}

/// <summary>Native function pointers used by Mother Brain's separate brain record.</summary>
public enum MotherBrainBrainFunction : ushort
{
    SetupBrainAndNeckToBeDrawn = 0x87a2,
    SetupBrainToBeDrawn = 0x87d0,
}

/// <summary>
/// Shared state behind retail enemy records <c>$EC7F</c> (body) and <c>$EC3F</c> (brain).
/// The SNES stores most of these words in named extended WRAM rather than in either common
/// <c>$40</c>-byte enemy slot. A dedicated object therefore preserves both the physical slot
/// boundary and the original one-owner/two-record relationship.
/// </summary>
public sealed class MotherBrainEnemyState
{
    internal MotherBrainEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Physical slot zero, enemy definition <c>$EC7F</c>.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>Physical slot one, enemy definition <c>$EC3F</c>.</summary>
    public RoomEnemySlot? Head { get; internal set; }

    /// <summary>
    /// Exact <c>$7E:9000/$7E:9700</c> corpse graphics and rot-table producer initialized by
    /// the head record even though it is not consumed until the much later death sequence.
    /// </summary>
    public MotherBrainCorpseRottingState CorpseRotting { get; } = new();

    /// <summary>Mother Brain form word: zero is glass/first phase, one begins fake death.</summary>
    public ushort Form { get; internal set; }

    /// <summary>Native shared hitbox-enable word, initialized to two.</summary>
    public ushort HitboxesEnabled { get; internal set; }

    public MotherBrainBodyFunction Function { get; internal set; }
    public MotherBrainBrainFunction BrainFunction { get; internal set; }

    /// <summary>
    /// Native <c>mbn_var_F</c>. Every fake-death pause decrements this unsigned word and
    /// branches when bit 15 becomes set; zero therefore expires immediately to $FFFF.
    /// </summary>
    public ushort FunctionTimer { get; internal set; }

    /// <summary>
    /// Body-animation state written by private instruction opcodes. The body AI reads this
    /// word independently of the current spritemap, notably while waiting for the slow
    /// post-ascent uncrouch to publish <see cref="MotherBrainBodyPose.Standing"/>.
    /// </summary>
    public MotherBrainBodyPose Pose { get; internal set; }

    /// <summary>Zero-based palette-step count stored in native <c>mbn_var_37</c>.</summary>
    public ushort GrayFadeIndex { get; internal set; }

    /// <summary>Eight-frame cadence and wrapping coordinate cursor for fake-death dust.</summary>
    public ushort FakeDeathExplosionTimer { get; internal set; }
    public ushort FakeDeathExplosionIndex { get; internal set; }

    /// <summary>
    /// Bank-$A9 room-palette bytecode pointer/timer. The timer counts upward, unlike enemy
    /// instruction timers, because handler $D192 compares elapsed frames with each duration.
    /// </summary>
    public ushort RoomPaletteInstructionPointer { get; internal set; }
    public ushort RoomPaletteInstructionTimer { get; internal set; }

    /// <summary>Head-record sub-dispatch and its independent underflow timer.</summary>
    public MotherBrainTubeCollapseFunction TubeCollapseFunction { get; internal set; }
    public ushort TubeCollapseTimer { get; internal set; }

    /// <summary>Exact delayed music writes made during the most recent enemy frame.</summary>
    public IReadOnlyList<MotherBrainMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Hardcoded bank-$84 objects requested during the most recent enemy frame.</summary>
    public IReadOnlyList<MotherBrainPlmRequest> PlmRequests => _plmRequests;

    /// <summary>Last library-two fake-death/tube sound emitted during this enemy frame.</summary>
    public ushort? LastSoundEffect { get; internal set; }

    /// <summary>Last library-one rainbow-beam sound emitted during this enemy frame.</summary>
    public ushort? LastSoundEffectLibrary1 { get; internal set; }

    /// <summary>Last library-three sound emitted by a Mother Brain private opcode.</summary>
    public ushort? LastSoundEffectLibrary3 { get; internal set; }

    /// <summary>Count of dynamically spawned physical falling-tube enemy records.</summary>
    public int SpawnedFallingTubeCount { get; internal set; }

    /// <summary>FX table entry requested by the body initializer.</summary>
    public ushort FxEntry { get; internal set; }

    /// <summary>Whether the unpause hook must restore Mother Brain's BG2 image and beam SFX.</summary>
    public bool EnableUnpauseHook { get; internal set; }

    /// <summary>True after all <c>$800</c> enemy-BG2 words have been filled with tile <c>$0338</c>.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>Palette selector shared by the four neck segments.</summary>
    public ushort NeckPaletteIndex { get; internal set; }

    /// <summary>Palette selector applied by the custom brain drawing routine.</summary>
    public ushort BrainPaletteIndex { get; internal set; }

    /// <summary>Countdown used by the normal head-palette setup routine, initially ten.</summary>
    public ushort BrainPaletteTimer { get; internal set; }

    /// <summary>Earthquake timer copied into the head-shake word after the glass event.</summary>
    public ushort BrainMainShakeTimer { get; internal set; }

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// Set by the head main on frames where the cartridge's enemy-graphics-drawn hook points
    /// at <c>$A9:87DD</c>. It is deliberately rebuilt every frame rather than treated as
    /// ordinary visibility because both physical records carry property bit <c>$0100</c>.
    /// </summary>
    public bool DrawBrain { get; internal set; }

    /// <summary>Whether the active bank-$A9 draw hook appends five articulated neck joints.</summary>
    public bool DrawNeck { get; internal set; }

    /// <summary>Native fake-ascent neck lengths installed by <c>$A9:903F</c>.</summary>
    public ushort NeckSegment0Distance { get; internal set; }
    public ushort NeckSegment1Distance { get; internal set; }
    public ushort NeckSegment2Distance { get; internal set; }
    public ushort NeckSegment3Distance { get; internal set; }
    public ushort NeckSegment4Distance { get; internal set; }

    /// <summary>Five current world-space neck joints; segment four anchors the brain.</summary>
    public MotherBrainNeckPoint NeckSegment0 { get; internal set; }
    public MotherBrainNeckPoint NeckSegment1 { get; internal set; }
    public MotherBrainNeckPoint NeckSegment2 { get; internal set; }
    public MotherBrainNeckPoint NeckSegment3 { get; internal set; }
    public MotherBrainNeckPoint NeckSegment4 { get; internal set; }

    /// <summary>8.8-style angle words and dispatch indices used by both neck halves.</summary>
    public ushort LowerNeckAngle { get; internal set; }
    public ushort UpperNeckAngle { get; internal set; }
    public ushort NeckAngleDelta { get; internal set; }
    public bool NeckMovementEnabled { get; internal set; }
    public ushort LowerNeckMovementIndex { get; internal set; }
    public ushort UpperNeckMovementIndex { get; internal set; }

    /// <summary>
    /// Pointer to the next seven-byte entry in the multi-frame sprite-tile transfer list.
    /// Zero means no list is active, exactly matching native WRAM <c>$7E:8004</c>.
    /// </summary>
    public ushort SpriteTileTransferEntryPointer { get; internal set; }

    /// <summary>True while bank-$88's rising layer-mask object is alive.</summary>
    public bool RisingHdmaActive { get; internal set; }

    /// <summary>Last verified room layer-blending configuration written by the body AI.</summary>
    public ushort LayerBlendingDefaultConfig { get; internal set; }

    /// <summary>Direct BG2 scroll-register mirrors owned by the phase-two body art.</summary>
    public ushort Bg2XScroll { get; internal set; }
    public ushort Bg2YScroll { get; internal set; }
    public bool HasBg2ScrollOverride { get; internal set; }

    /// <summary>Visible prefix of the enemy BG2 staging tilemap requested at ascent completion.</summary>
    public ushort EnemyBg2TilemapSize { get; internal set; }
    public bool EnemyBg2TilemapTransferRequested { get; internal set; }

    /// <summary>Four-frame cadence cursor for the eight ascent dust positions.</summary>
    public ushort BodySubFunctionTimer { get; internal set; }

    /// <summary>Zero-based palette pointer-table index used while leaving fake-death grey.</summary>
    public ushort GrayTransitionCounter { get; internal set; }

    public bool BrainPaletteHandlingEnabled { get; internal set; }
    public bool DroolGenerationEnabled { get; internal set; }
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; }

    /// <summary>
    /// Attached-mouth offset selector used by drool opcode <c>$A9:9B3C</c>. Native
    /// increments before spawning and wraps at six, so a zero-initialized encounter emits
    /// parameter one first.
    /// </summary>
    public ushort DroolProjectileParameter { get; internal set; }

    /// <summary>Native phase-two walking/shot-reaction accumulator.</summary>
    public ushort WalkCounter { get; internal set; }

    /// <summary>
    /// Native <c>attackPhase</c> dispatch index used by <c>$A9:B64B</c>. Head animation
    /// runs independently while this advances from selection, through 64 cooldown frames,
    /// and back to the ordinary thinking function.
    /// </summary>
    public MotherBrainAttackPhase AttackPhase { get; internal set; }

    /// <summary>Unsigned 64-frame attack cooldown decremented by <c>$A9:B764</c>.</summary>
    public ushort AttackCooldown { get; internal set; }

    /// <summary>
    /// Number of active Mother Brain bombs. Phase two refuses another bomb when this is
    /// at least one; the shared bank-$86 bomb implementation owns increments and decrements.
    /// </summary>
    public ushort BombCounter { get; internal set; }

    /// <summary>
    /// Native <c>bodyTargetXPosition</c> used while the bomb decision walks the body toward
    /// X=$40 or $60 before entering its posture sequence.
    /// </summary>
    public ushort BodyTargetXPosition { get; internal set; }

    /// <summary>
    /// Native <c>deathBeamAttackPhase</c> consumed by <c>$A9:B87D</c>. Despite the historical
    /// symbol name, this is the ordinary phase-two red hand-beam attack, not the later HDMA
    /// rainbow beam used when the head's health reaches zero.
    /// </summary>
    public MotherBrainHandBeamPhase HandBeamPhase { get; internal set; }

    /// <summary>
    /// Shared cursor used by the bank-$86 hand-beam projectile chain. Every fired child
    /// starts from this exact 16.16 position, advances the cursor by the aimed velocity,
    /// then receives its own randomized secondary velocity.
    /// </summary>
    public ushort HandBeamNextXPosition { get; internal set; }
    public ushort HandBeamNextXSubposition { get; internal set; }
    public ushort HandBeamNextYPosition { get; internal set; }
    public ushort HandBeamNextYSubposition { get; internal set; }
    public ushort HandBeamNextXVelocity { get; internal set; }
    public ushort HandBeamNextYVelocity { get; internal set; }
    public ushort HandBeamNextAngle { get; internal set; }

    /// <summary>
    /// The already verified rainbow/finish-off/Baby/phase-three state machine, attached to
    /// the real body/head pair only after phase-two health reaches zero. Physical actor
    /// coordinates and bytecode remain owned by this room system; this object owns the long
    /// bank-$A9 function chain and forced-Samus calculations.
    /// </summary>
    public MotherBrainRainbowBeamAttackSequence? RainbowBeamSequence { get; internal set; }

    /// <summary>
    /// The dynamically allocated physical <c>$ECBF</c> enemy record created by
    /// <c>$A9:BE1B</c>. Keeping the slot and its extended cutscene state side by side makes
    /// the native cross-enemy index observable without pretending the Baby is a draw-only
    /// effect owned by Mother Brain's body.
    /// </summary>
    public RoomEnemySlot? BabyMetroidSlot { get; internal set; }
    public BabyMetroidCutsceneState? BabyMetroid { get; internal set; }

    /// <summary>Debugger witness from the Baby's most recent physical enemy turn.</summary>
    public BabyMetroidCutsceneStepResult? LastBabyMetroidStep { get; internal set; }

    /// <summary>
    /// Last Baby instruction list copied into its physical slot. The ordinary bank-$A9
    /// interpreter advances the slot pointer independently, so this latch changes only
    /// when Baby AI explicitly invokes the native set-list helper.
    /// </summary>
    internal ushort BabyAppliedInstructionList { get; set; }

    /// <summary>
    /// Shared cry counter incremented by bank-$86 onion-ring collision and consumed by the
    /// Baby's later healing/idle functions. It is deliberately a count, not a boolean:
    /// native <c>INC</c> permits more than one ring to land before the next Baby turn.
    /// </summary>
    internal ushort PendingBabyCryCount { get; set; }

    /// <summary>Debugger witness from the most recent live rainbow body-function call.</summary>
    public MotherBrainRainbowBeamAttackStepResult? LastRainbowBeamStep { get; internal set; }

    /// <summary>Live bank-$88 rainbow-beam presentation state published by body AI.</summary>
    public bool RainbowBeamHdmaActive { get; internal set; }
    public SnesAngle RainbowBeamAngle { get; internal set; }
    public ushort RainbowBeamAngularWidth { get; internal set; }
    public bool RainbowBeamPaletteRequested { get; internal set; }
    public MotherBrainRainbowExplosionRequest? LastRainbowBeamExplosion { get; internal set; }

    /// <summary>
    /// Last head list copied from the rainbow state machine into the physical head slot.
    /// The physical instruction pointer advances away from the list origin, so comparing
    /// against <c>Head.CurrentInstruction</c> would reinstall the list and reset its timer
    /// every frame. This separate producer-owned latch mirrors the fact that `$A9:C447`
    /// runs only when body AI explicitly requests a different head program.
    /// </summary>
    internal ushort RainbowAppliedHeadInstructionList { get; set; }

    /// <summary>
    /// Last drop request emitted when an exploding Samus bomb destroys Mother Brain's bomb.
    /// Pickup selection remains owned by the common enemy-drop seam; retaining the head
    /// definition and exact coordinate makes that request inspectable in the debugger.
    /// </summary>
    public MotherBrainBombDropRequest? LastBombDropRequest { get; internal set; }

    /// <summary>
    /// Clamped byte-angle written by head opcode <c>$A9:9E5B</c> and consumed by the next
    /// <c>$A9:9E29</c> onion-ring spawn. It remains a word because native extended WRAM is
    /// word-addressed even though the calculation deliberately operates in 8-bit mode.
    /// </summary>
    public ushort OnionRingsTargetAngle { get; internal set; }

    /// <summary>
    /// Parameters passed to the twelve <c>$86</c> Mother Brain turret initializers during
    /// body load. The matching actors occupy the shared eighteen-slot projectile pool; this
    /// retained request list makes their native creation order explicit in debugger state.
    /// </summary>
    public IReadOnlyList<ushort> InitialTurretParameters => _initialTurretParameters;

    private readonly ushort[] _initialTurretParameters = new ushort[12];
    private readonly List<MotherBrainMusicRequest> _musicRequests = new();
    private readonly List<MotherBrainPlmRequest> _plmRequests = new();

    internal void RecordInitialTurretRequests()
    {
        for (ushort parameter = 0; parameter < _initialTurretParameters.Length; parameter++)
            _initialTurretParameters[parameter] = parameter;
    }

    internal void BeginFrame()
    {
        _musicRequests.Clear();
        _plmRequests.Clear();
        LastSoundEffect = null;
        LastSoundEffectLibrary1 = null;
        LastSoundEffectLibrary3 = null;
        LastBombDropRequest = null;
        LastRainbowBeamStep = null;
        LastBabyMetroidStep = null;
        RainbowBeamPaletteRequested = false;
        LastRainbowBeamExplosion = null;
    }

    internal void RequestMusic(MusicCommand command, MusicCommandDelay delay) =>
        _musicRequests.Add(new MotherBrainMusicRequest(command, delay));

    internal void RequestPlm(byte blockX, byte blockY, ushort header) =>
        _plmRequests.Add(new MotherBrainPlmRequest(blockX, blockY, header));
}

/// <summary>
/// One typed <c>QueueMusic_Delayed*</c> call. The command retains its complete cartridge
/// word, including unknown commands, without conflating data uploads with track indices.
/// </summary>
public readonly record struct MotherBrainMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>One literal <c>SpawnHardcodedPLM</c> call issued by Mother Brain's bank-$A9 AI.</summary>
public readonly record struct MotherBrainPlmRequest(byte BlockX, byte BlockY, ushort Header);

/// <summary>One <c>$86:C5BB</c> enemy-drop request produced by a destroyed Mother Brain bomb.</summary>
public readonly record struct MotherBrainBombDropRequest(
    ushort X,
    ushort Y,
    ushort EnemyDefinitionPointer);
