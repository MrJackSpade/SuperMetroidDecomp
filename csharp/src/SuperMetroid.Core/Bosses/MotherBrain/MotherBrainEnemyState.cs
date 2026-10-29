using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Shared state behind retail enemy records <c>$EC7F</c> (body) and <c>$EC3F</c> (brain).
/// The SNES stores most of these words in named extended WRAM rather than in either common
/// <c>$40</c>-byte enemy slot. A dedicated object therefore preserves both the physical slot
/// boundary and the original one-owner/two-record relationship.
/// </summary>
public sealed class MotherBrainEnemyState
{
    /// <summary>Starts the shared encounter state with the physical body slot before the head is attached.</summary>
    internal MotherBrainEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Physical slot zero, enemy definition <c>$EC7F</c>.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>Physical slot one, enemy definition <c>$EC3F</c>.</summary>
    public RoomEnemySlot Head { get => field ?? throw new InvalidOperationException("Mother Brain's head slot is not attached; head initialization attaches it."); internal set; }

    /// <summary>
    /// Exact <c>$7E:9000/$7E:9700</c> corpse graphics and rot-table producer initialized by
    /// the head record even though it is not consumed until the much later death sequence.
    /// </summary>
    public MotherBrainCorpseRottingState CorpseRotting { get; } = new();

    /// <summary>The encounter stage that shot, bomb and footstep handling branch on; the glass phase at room entry.</summary>
    public MotherBrainForm Form { get; internal set; }

    /// <summary>The contact regions that currently hurt Samus: the brain alone at room entry, all three from the ascent.</summary>
    public MotherBrainHitboxParts HitboxesEnabled { get; internal set; }

    /// <summary>The body record's bank-$A9 function pointer, selecting first-phase, resurrection, combat, or cutscene control for its next AI turn.</summary>
    public MotherBrainBodyFunction Function { get; internal set; }
    /// <summary>The brain record's independent bank-$A9 function pointer selecting its custom head-only or head-and-neck draw setup.</summary>
    public MotherBrainBrainFunction BrainFunction { get; internal set; }

    /// <summary>
    /// Native <c>mbn_var_F</c>. Every fake-death pause decrements this unsigned word and
    /// branches when bit 15 becomes set; zero therefore expires immediately to $FFFF.
    /// </summary>
    public WrappingCountdown FunctionTimer { get; internal set; }

    /// <summary>
    /// Body-animation state written by private instruction opcodes. The body AI reads this
    /// word independently of the current spritemap, notably while waiting for the slow
    /// post-ascent uncrouch to publish <see cref="MotherBrainBodyPose.Standing"/>.
    /// </summary>
    public MotherBrainBodyPose Pose { get; internal set; }

    /// <summary>Zero-based count of fake-death grey palette steps taken, 0 through the eight-step table length plus one.</summary>
    public int GrayFadeIndex { get; internal set; }

    /// <summary>Eight-frame cadence and wrapping coordinate cursor for fake-death dust.</summary>
    public WrappingCountdown FakeDeathExplosionTimer { get; internal set; }
    /// <summary>Wrapping index 0 through 7 into authored fake-death explosion positions; decremented modulo eight whenever the explosion timer underflows.</summary>
    public int FakeDeathExplosionIndex { get; internal set; }

    /// <summary>
    /// The room flash's current frame (#641), or null while no flash runs. Its timer counts upward, unlike enemy
    /// instruction timers, because handler $D192 compares elapsed frames with each duration.
    /// </summary>
    public int? RoomFlashFrame { get; internal set; }
    /// <summary>Elapsed-update count for the current room-flash frame, advanced upward by $A9:D192 and reset when the frame changes; never above the frame's two-update duration.</summary>
    public int RoomFlashTimer { get; internal set; }

    /// <summary>Head-record sub-dispatch and its independent underflow timer.</summary>
    public MotherBrainTubeCollapseFunction TubeCollapseFunction { get; internal set; }
    /// <summary>Independent wrapping word countdown used by selected tube spawn stages; expiration occurs after decrement sets the sign bit.</summary>
    public WrappingCountdown TubeCollapseTimer { get; internal set; }

    /// <summary>Exact delayed music writes made during the most recent enemy frame.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Last library-two fake-death/tube sound emitted during this enemy frame.</summary>
    public SoundEffectId? LastSoundEffect { get; internal set; }

    /// <summary>Last library-one rainbow-beam sound emitted during this enemy frame.</summary>
    public SoundEffectId? LastSoundEffectLibrary1 { get; internal set; }

    /// <summary>Last library-three sound emitted by a Mother Brain private opcode.</summary>
    public SoundEffectId? LastSoundEffectLibrary3 { get; internal set; }

    /// <summary>Count of dynamically spawned physical falling-tube enemy records.</summary>
    public int SpawnedFallingTubeCount { get; internal set; }

    /// <summary>Whether the unpause hook must restore Mother Brain's BG2 image and beam SFX.</summary>
    public bool EnableUnpauseHook { get; internal set; }

    /// <summary>True after all <c>$800</c> enemy-BG2 words have been filled with tile <c>$0338</c>.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>The colours the five neck joints draw in (#1464).</summary>
    public SpriteColouring NeckColouring { get; internal set; } = SharedSpriteColouring.Common;

    /// <summary>The colours the custom brain draw applies outside its white hurt-flash frames (#1464).</summary>
    public SpriteColouring BrainColouring { get; internal set; } = SharedSpriteColouring.Common;

    /// <summary>
    /// The brain's own program (#641), separate from the head enemy's dummy program: the cursor rests on the frame
    /// being shown, as <c>MotherBrainBody.brainInstListPointer</c> ($7E:8002) does. Null means the brain is not live
    /// and not drawn. Only the program setter (<c>$A9:C447</c>) and the draw-time runner (<c>$A9:92AF</c>) write it.
    /// </summary>
    internal AnimationProgramCursor<MotherBrainHeadProgram>? BrainCursor { get; set; }

    /// <summary>
    /// <c>MotherBrainBody.brainInstructionTimer</c> ($7E:8000). It counts draws up from one
    /// and the program advances once it exceeds the frame's duration, so each frame shows for
    /// its duration plus one draw.
    /// </summary>
    public int BrainFrameTimer { get; internal set; }

    /// <summary>Where the body enemy's program resumes (#641).</summary>
    internal AnimationProgramCursor<MotherBrainBodyProgram> BodyCursor { get; set; } =
        AnimationProgramCursor<MotherBrainBodyProgram>.Start(MotherBrainBodyProgram.InitialDummy);

    /// <summary>Where the head enemy's program resumes; it keeps the initial dummy for life.</summary>
    internal AnimationProgramCursor<MotherBrainBodyProgram> HeadSlotCursor { get; set; } =
        AnimationProgramCursor<MotherBrainBodyProgram>.Start(MotherBrainBodyProgram.InitialDummy);

    /// <summary>Earthquake timer copied into the head-shake word after the glass event.</summary>
    public WrappingCountdown BrainMainShakeTimer { get; internal set; }

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// Set by the head main on frames where the cartridge's enemy-graphics-drawn hook points
    /// at <c>$A9:87DD</c>. It is deliberately rebuilt every frame rather than treated as
    /// ordinary visibility because both physical records carry property bit <c>$0100</c>.
    /// </summary>
    public bool DrawBrain { get; internal set; }

    /// <summary>Byte cursor at $7E:7842 into Mother Brain's rainbow palette list: an even offset from zero to two bytes per rainbow frame.</summary>
    public int RainbowPaletteCursor { get; internal set; }

    /// <summary>Whether the active bank-$A9 draw hook appends five articulated neck joints.</summary>
    public bool DrawNeck { get; internal set; }

    /// <summary>
    /// Each neck joint's pixel distance from its anchor, indexed by joint (<see cref="MotherBrainNeckJointDefinitions"/>):
    /// the lower joints' from the shared body anchor, the upper joints' from the upper-neck anchor. <c>$A9:903F</c>
    /// initializes them.
    /// </summary>
    public int[] NeckJointDistances { get; } = new int[MotherBrainNeckJointDefinitions.Count];

    /// <summary>
    /// The neck joints' wrapping world-pixel positions, indexed by joint (<see cref="MotherBrainNeckJointDefinitions"/>):
    /// the lower joints hang from the body on the lower angle, the upper joints from the upper-neck anchor on the upper
    /// angle. Neck drawing and collision use them, and the brain anchor places the head.
    /// </summary>
    public Position16<RoomPixels>[] NeckJoints { get; } = new Position16<RoomPixels>[MotherBrainNeckJointDefinitions.Count];

    /// <summary>Lower-neck angle; the neck dispatchers add and subtract the angular speed with full-turn wraparound, and its table index places joints 0 through 2.</summary>
    public RotationAngle LowerNeckAngle { get; internal set; }
    /// <summary>Upper-neck angle; moved with full-turn wraparound like the lower angle, and its table index places joints 3 and 4.</summary>
    public RotationAngle UpperNeckAngle { get; internal set; }
    /// <summary>Neck angular speed in 8.8 angle units, added or subtracted by both neck motion dispatchers each enabled update; every producer writes a positive speed of at most $0900.</summary>
    public int NeckAngleDelta { get; internal set; }
    /// <summary>Whether articulated neck angle dispatch is enabled; positions and custom drawing remain separate from this motion flag.</summary>
    public bool NeckMovementEnabled { get; internal set; }
    /// <summary>Lower-neck table byte offset: 0 inert, 2/4 alternating down/up, 6 one-way lowering, or 8 one-way raising; endpoints update this offset.</summary>
    public MotherBrainNeckMovement LowerNeckMovementIndex { get; internal set; }
    /// <summary>Upper-neck table byte offset 0, 2, 4, 6, or 8; processed after the lower half so raising targets observe its newly updated angle.</summary>
    public MotherBrainNeckMovement UpperNeckMovementIndex { get; internal set; }

    /// <summary>The pages the ascent shows one per call: the eight leg pages, then the four attack pages the legs covered.</summary>
    public static int LegLoadPageCount =>
        MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.Legs) + MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.Attack);

    /// <summary>
    /// The next of the twelve leg-loading pages the ascent shows, one per call. Zero both starts the list and is the
    /// resting value after the final page, because a list that is not running starts from the first page.
    /// </summary>
    public int NextLegTilePage
    {
        get;
        internal set => field = (uint)value < LegLoadPageCount
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Mother Brain's ascent shows twelve pages.");
    }

    /// <summary>True while the floor clips Mother Brain's body as she rises from the fake death.</summary>
    public bool RisingFloorClipActive { get; internal set; }

    /// <summary>Last verified room layer-blending configuration written by the body AI.</summary>
    public LayerBlendingConfiguration LayerBlendingDefaultConfig { get; internal set; } = LayerBlendingConfiguration.Unconfigured;

    /// <summary>
    /// Direct BG2 scroll-register mirrors owned by the phase-two body art. The vertical word is
    /// updated oppositely to physical body Y movement, keeping the large background-rendered
    /// body attached.
    /// </summary>
    public Position16<BackgroundPixels> Bg2Scroll { get; internal set; }
    /// <summary>Whether body AI has published encounter-owned BG2 scroll values for the runtime to apply instead of ordinary room scrolling.</summary>
    public bool HasBg2ScrollOverride { get; internal set; }
    /// <summary>ADF41C's main-screen BG2 flicker, separate from the OAM invisibility bit.</summary>
    public bool DeathBg2Hidden { get; internal set; }
    /// <summary>One-shot latch for the AFE0 body tilemap clear.</summary>
    public bool DeathBg2Cleared { get; internal set; }
    /// <summary>Live escape message owner installed by $A6:C23F.</summary>
    public EscapeTypewriterState? EscapeTypewriter { get; internal set; }

    /// <summary>
    /// The body Mother Brain stages from her initialization until her phase-two transition shows it (#1356, #1466): her
    /// body frames are drawn on it meanwhile.
    /// </summary>
    internal BossBodyCanvas? StagedBody { get; init; }

    /// <summary>Four-frame cadence cursor for the eight ascent dust positions.</summary>
    public WrappingCountdown BodySubFunctionTimer { get; internal set; }

    /// <summary>Zero-based palette pointer-table index used while leaving fake-death grey.</summary>
    public int GrayTransitionCounter { get; internal set; }

    /// <summary>Enables the head's ordinary palette processing; resurrection and draining transitions suspend it while their own palettes own the colors.</summary>
    public bool BrainPaletteHandlingEnabled { get; internal set; }
    /// <summary>Allows head bytecode's mouth-drool generation, enabled during live combat and cleared by low-power/death transitions.</summary>
    public bool DroolGenerationEnabled { get; internal set; }
    /// <summary>Allows the head's small purple breath/dust producer, enabled after stretching and disabled for the drained corpse interval.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; }

    /// <summary>
    /// <c>MotherBrainBody.smallPurpleBreathActiveFlag</c> ($7E:786A): set by the small breath's
    /// initializer ($86:CA9B) and cleared by its last instruction ($86:CAEE), so at most one
    /// small breath is alive.
    /// </summary>
    public bool SmallPurpleBreathActive { get; internal set; }

    /// <summary>
    /// Attached-mouth offset selector used by drool opcode <c>$A9:9B3C</c>. Native
    /// increments before spawning and wraps at six, so a zero-initialized encounter emits
    /// parameter one first.
    /// </summary>
    public int DroolProjectileParameter { get; internal set; }

    /// <summary>The walk-credit word this encounter owns and an attached sequence shares.</summary>
    internal MotherBrainWalkCounter Walk { get; } = new();

    /// <summary>Walk credit: standing turns earn it toward a forward step and the player's hits spend it.</summary>
    public MotherBrainWalkCredit WalkCounter
    {
        get => Walk.Credit;
        internal set => Walk.Credit = value;
    }

    /// <summary>
    /// Native <c>attackPhase</c> dispatch index used by <c>$A9:B64B</c>. Head animation
    /// runs independently while this advances from selection, through 64 cooldown frames,
    /// and back to the ordinary thinking function.
    /// </summary>
    public MotherBrainAttackPhase AttackPhase { get; internal set; }

    /// <summary>Unsigned 64-frame attack cooldown decremented by <c>$A9:B764</c>.</summary>
    public WrappingCountdown AttackCooldown { get; internal set; }

    /// <summary>
    /// Number of live Mother Brain bombs. Phase two refuses another bomb when this is at
    /// least one; <see cref="RecordBombSpawned"/> and <see cref="RecordBombRemoved"/> keep it.
    /// </summary>
    public int BombCounter { get; internal set; }

    /// <summary>Counts a bomb the shared bank-$86 bomb initializer has just spawned.</summary>
    internal void RecordBombSpawned() => BombCounter++;

    /// <summary>
    /// Uncounts a bomb that has just been deleted. Every deleted bomb was counted when it
    /// spawned, so the count is positive here; a zero count means a bomb was deleted twice.
    /// </summary>
    internal void RecordBombRemoved() => BombCounter = BombCounter > 0
        ? BombCounter - 1
        : throw new InvalidOperationException("A Mother Brain bomb was deleted while none was counted.");

    /// <summary>
    /// Native <c>bodyTargetXPosition</c> used while the bomb decision walks the body toward
    /// X=$40 or $60 before entering its posture sequence.
    /// </summary>
    public int BodyTargetXPosition { get; internal set; }

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
    /// <remarks>Charging starts it at the beam position with zero subpixels (body Y minus $30).</remarks>
    public FixedPosition<RoomPixels> HandBeamNextPosition { get; internal set; }
    /// <summary>Signed native 8.8 aimed velocity; advances the shared cursor before each child receives its independent randomized scatter velocity.</summary>
    public EightEightVector<RoomPixels> HandBeamNextVelocity { get; internal set; }
    /// <summary>Aim angle in 256-unit-turn table units (0 through 255), calculated from hand to Samus at charging setup and used as the children's randomized scatter-angle base.</summary>
    public int HandBeamNextAngle { get; internal set; }

    /// <summary>
    /// The already verified rainbow/finish-off/Baby/phase-three state machine, attached to
    /// the real body/head pair only after phase-two health reaches zero. Physical actor
    /// coordinates and bytecode remain owned by this room system; this object owns the long
    /// bank-$A9 function chain and forced-Samus calculations.
    /// </summary>
    public MotherBrainRainbowBeamAttackSequence? RainbowBeamSequence
    {
        get;
        internal set
        {
            // An attached sequence reads and writes the encounter's own walk-credit word.
            value?.ShareWalkCounter(Walk);
            field = value;
        }
    }

    /// <summary>
    /// The dynamically allocated physical <c>$ECBF</c> enemy record created by
    /// <c>$A9:BE1B</c>. Keeping the slot and its extended cutscene state side by side makes
    /// the native cross-enemy index observable without pretending the Baby is a draw-only
    /// effect owned by Mother Brain's body.
    /// </summary>
    public RoomEnemySlot? BabyMetroidSlot { get; internal set; }
    /// <summary>Extended cutscene AI state attached to the dynamically allocated baby slot; its independent turn owns latch, draining, healing, and death handshakes.</summary>
    public BabyMetroidCutsceneState? BabyMetroid { get; internal set; }


    /// <summary>
    /// Last Baby program copied into its physical slot. The slot's cursor advances
    /// independently, so this latch changes only when Baby AI explicitly invokes the
    /// native set-list helper.
    /// </summary>
    internal MotherBrainBabyProgram BabyAppliedProgram { get; set; }

    /// <summary>Where the cutscene Baby's slot program resumes (#641).</summary>
    internal AnimationProgramCursor<MotherBrainBabyProgram> BabyCursor { get; set; }

    /// <summary>
    /// Shared cry counter incremented by bank-$86 onion-ring collision and consumed by the
    /// Baby's later healing/idle functions. It is deliberately a count, not a boolean:
    /// native <c>INC</c> permits more than one ring to land before the next Baby turn.
    /// </summary>
    internal int PendingBabyCryCount { get; set; }

    /// <summary>Whether body AI currently shows the rainbow beam.</summary>
    public bool RainbowBeamActive { get; internal set; }
    /// <summary>The rainbow beam's colour and cone, advanced by the simulation rather than by painting a frame.</summary>
    public MotherBrainRainbowBeamEffect RainbowBeam { get; } = new();
    /// <summary>Current native angular aim published from the beam's forced-movement owner and consumed by the rainbow beam effect.</summary>
    public RotationAngle RainbowBeamAngle { get; internal set; }
    /// <summary>Rainbow-beam angular width in 8.8 table units, published by body AI as the beam widens or narrows; independent of pixel width at a given scanline.</summary>
    public int RainbowBeamAngularWidth { get; internal set; }
    /// <summary>Whether this body-function call requested a rainbow palette step; cleared at the beginning of every enemy frame.</summary>
    public bool RainbowBeamPaletteRequested { get; internal set; }

    /// <summary>
    /// Last brain program copied from the rainbow state machine into the brain.
    /// The brain cursor advances away from the program start, so comparing against it
    /// would reinstall the program and reset its timer every frame. This separate
    /// producer-owned latch mirrors the fact that `$A9:C447` runs only when body AI
    /// explicitly requests a different brain program.
    /// </summary>
    internal MotherBrainHeadProgram? RainbowAppliedHeadProgram { get; set; }

    /// <summary>
    /// Last <see cref="MotherBrainLateFightState.SmallPurpleBreathGenerationWriteCount"/>
    /// published to <see cref="SmallPurpleBreathGenerationEnabled"/>, so a brain-list clear
    /// survives until the body next writes the flag.
    /// </summary>
    internal uint RainbowAppliedSmallPurpleBreathWrites { get; set; }


    /// <summary>
    /// Clamped aim angle in 256-unit-turn table units, 16 through 72, written by head opcode
    /// <c>$A9:9E5B</c> and consumed by the next <c>$A9:9E29</c> onion-ring spawn.
    /// </summary>
    public int OnionRingsTargetAngle { get; internal set; }

    /// <summary>Music commands emitted during the current enemy frame.</summary>
    private readonly List<DelayedMusicRequest> _musicRequests = new();

    /// <summary>Clears one-frame encounter requests and diagnostic witnesses before AI advances.</summary>
    internal void BeginFrame()
    {
        _musicRequests.Clear();
        LastSoundEffect = null;
        LastSoundEffectLibrary1 = null;
        LastSoundEffectLibrary3 = null;
        RainbowBeamPaletteRequested = false;
    }

    /// <summary>Queues one music request with its delay for this frame.</summary>
    internal void RequestMusic(MusicRequest request, MusicQueueDelay delay) =>
        _musicRequests.Add(new DelayedMusicRequest(request, delay));
}
