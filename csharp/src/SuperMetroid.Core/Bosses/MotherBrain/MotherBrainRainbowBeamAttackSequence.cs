using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's repeatable phase-two rainbow-beam, finish-off, and Baby-drain/corpse
/// function chains at <c>$A9:B8EB-$A9:BFCF</c>.
/// </summary>
/// <remarks>
/// The earlier general attack-selection logic and the spawned Baby Metroid's independent AI
/// remain separate actor phases. This class owns the neck extension, both charge waits, body
/// walks and posture changes, active beam, low-energy attack selection, final charge, the four
/// frame-spread Baby tile transfers, spawn request, final-beam hold, painful stagger, live neck
/// angles/brain coordinates, rear-room retreat, crouch, grey transition, and corpse handshake.
/// Palette, HDMA, projectile, earthquake, VRAM, spawn, and sound writes are retained as
/// inspectable requests; coordinate, resource, timer, instruction-list, and Samus-command
/// mutations execute directly.
/// </remarks>
public sealed class MotherBrainRainbowBeamAttackSequence
{
    /// <summary>
    /// Signed beam-explosion positions from $A9:BCA6/$BCB6. The index increments before
    /// lookup, so its initial zero value selects the second record.
    /// </summary>
    private static readonly Offset16<RoomPixels>[] ExplosionOffsets =
        [new(-8, -7), new(6, 2), new(-4, 5), new(2, -4), new(3, 6), new(-6, -2), new(8, -6), new(0, 7)];

    /// <summary>Owns the forced Samus movement during beam capture and the later drain sequence.</summary>
    private readonly MotherBrainRainbowBeamSamusMovement _movement = new();
    /// <summary>Advances the post-fight corpse animation and its room effects.</summary>
    internal readonly MotherBrainCorpseRottingState CorpseRotting = new();
    /// <summary>Optional room artwork used to emit corpse tile transfers in installed rooms.</summary>
    private readonly RoomCharacterAtlas? _corpseArtwork;

    /// <summary>Standalone cartridge fixtures omit artwork and staging; installed rooms bind both.</summary>
    public MotherBrainRainbowBeamAttackSequence(RoomCharacterAtlas? corpseArtwork = null,
        EnemyStagingSurfaces? corpseStaging = null)
    {
        _corpseArtwork = corpseArtwork;
        BoundCorpseStaging = corpseStaging;
    }

    /// <summary>The room's enemy staging surfaces, which the corpse rotting rewrites; bound with the artwork.</summary>
    private EnemyStagingSurfaces? BoundCorpseStaging { get; }

    /// <summary>The bound staging surfaces; corpse rotting requires them.</summary>
    private EnemyStagingSurfaces CorpseStaging => BoundCorpseStaging ?? throw new InvalidDataException(
        "Mother Brain corpse rotting requires the room's enemy staging surfaces.");

    /// <summary>
    /// Body enemy-slot animation state. The caller advances its enemy-instruction stage
    /// after <see cref="Step"/>, matching the native AI-then-instruction order.
    /// </summary>
    public MotherBrainBodyAnimationState Body { get; } = new();

    /// <summary>Current body-function equivalent.</summary>
    public MotherBrainRainbowBeamAttackPhase Phase { get; internal set; } =
        MotherBrainRainbowBeamAttackPhase.Inactive;

    /// <summary>Mother Brain brain-slot room position used by `$A9:BB82/$BB8F`'s aim vector.</summary>
    public Position16<RoomPixels> BrainPosition { get; set; }

    /// <summary>Current brain program installed through `$A9:C447`, or null before the first request.</summary>
    public MotherBrainHeadProgram? HeadProgram { get; private set; }

    /// <summary>
    /// Monotonic witness for calls to `$A9:C447`. The native body AI may deliberately
    /// reinstall the same list address, so pointer equality alone cannot identify a new
    /// request at the live physical head-slot boundary.
    /// </summary>
    private uint _headInstructionListRequestSerial;

    /// <summary>Neck angular speed in 8.8 angle units (Mother Brain body extra word `$0FBC`); every producer writes a positive speed of at most $0900.</summary>
    public int NeckAngleDelta { get; internal set; }

    /// <summary>Whether articulated neck angle dispatch runs on the brain's turn.</summary>
    public bool NeckMovementEnabled { get; internal set; }

    /// <summary>Lower neck movement index selected by the current actor phase.</summary>
    public MotherBrainNeckMovement LowerNeckMovementIndex { get; internal set; }

    /// <summary>Upper neck movement index selected by the current actor phase.</summary>
    public MotherBrainNeckMovement UpperNeckMovementIndex { get; internal set; }

    /// <summary>Native body function timer at enemy word <c>$0FB0</c>.</summary>
    public NativeCountdown FunctionTimer { get; internal set; }

    /// <summary>Rainbow-beam angular width in 8.8 table units, initialized and floored at <c>$0200</c> and capped at <c>$0C00</c>.</summary>
    public int AngularWidth { get; internal set; }

    /// <summary>Native signed SFX queue countdown. Six produces seven queue attempts.</summary>
    public NativeCountdown SoundQueueCount { get; internal set; }

    /// <summary>Rainbow-beam explosion timer at Mother Brain body extra word <c>$0FA8</c>.</summary>
    public NativeCountdown ExplosionTimer { get; private set; }

    /// <summary>Index of the offset pair the latest beam explosion used, cycling through the eight pairs.</summary>
    public int ExplosionIndex { get; private set; }

    /// <summary>Host-readable equivalent of the spawned rainbow-beam HDMA object's enable.</summary>
    public bool HdmaActive { get; private set; }

    /// <summary>Native rainbow-beam SFX-playing flag.</summary>
    public bool RainbowBeamSoundPlaying { get; internal set; }

    /// <summary>
    /// Current byte angle produced by `$A9:BBA9` for the bank-$88 rainbow HDMA renderer.
    /// Exposing the translated movement owner's word lets the live room adapter publish the
    /// real beam geometry without maintaining a second aim calculation.
    /// </summary>
    public SnesAngle RainbowBeamAngle => _movement.RainbowBeamAngle;

    /// <summary>
    /// Lower neck angle updated by the brain-slot handler at <c>$A9:9072</c>. The final
    /// beam has already lowered it to <c>$3000</c> before the Baby interrupts the body.
    /// </summary>
    public SnesAngle LowerNeckAngle { get; internal set; } = SnesAngle.FromRaw(12288);

    /// <summary>Upper neck angle, correspondingly lowered to <c>$2000</c>.</summary>
    public SnesAngle UpperNeckAngle { get; internal set; } = SnesAngle.FromRaw(8192);

    /// <summary>Main brain-shake timer seeded to fifty by <c>$A9:BE96</c> when clear.</summary>
    public NativeCountdown BrainMainShakeTimer { get; internal set; }

    /// <summary>Current zero-based painful-walk stage at WRAM <c>$7E:802A</c>, 0 through 8.</summary>
    [IntegerQuantity("zero-based count of painful-walk steps taken, 0 through 8")]
    public int PainfulWalkingStage { get; internal set; }

    /// <summary>Walk animation delay derived from the current stage: 2, 4, 6, 8 or 10 updates.</summary>
    public int PainfulWalkingAnimationDelay { get; internal set; }

    /// <summary>Pause timer used after each completed forward/backward body list.</summary>
    public NativeCountdown PainfulWalkingFunctionTimer { get; private set; }

    /// <summary>True while the nested painful-walk function is on its forward half.</summary>
    public bool PainfulWalkingForward { get; internal set; }

    /// <summary>Native brain-palette handling flag cleared when the rainbow beam expires.</summary>
    public bool BrainPaletteHandlingEnabled { get; internal set; } = true;

    /// <summary>Drool generation flag cleared as Mother Brain enters low-power mode.</summary>
    public bool DroolGenerationEnabled { get; internal set; } = true;

    /// <summary>Small-purple-breath flag cleared when the grey corpse is published.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; } = true;

    /// <summary>
    /// Count of body-AI writes to <see cref="SmallPurpleBreathGenerationEnabled"/>. The live
    /// flag has a second native writer, the brain-list opcode $A9:9F8E, so the live state
    /// publishes this flag only when the body actually wrote it.
    /// </summary>
    public uint SmallPurpleBreathGenerationWriteCount { get; private set; }

    /// <summary>Records a body-AI write to the breath flag so a later brain-list opcode can remain a separate writer.</summary>
    private void EnableSmallPurpleBreathGeneration()
    {
        SmallPurpleBreathGenerationEnabled = true;
        SmallPurpleBreathGenerationWriteCount++;
    }

    /// <summary>Stops the small purple breath, counting the native write as <see cref="EnableSmallPurpleBreathGeneration"/> does.</summary>
    private void DisableSmallPurpleBreathGeneration()
    {
        SmallPurpleBreathGenerationEnabled = false;
        SmallPurpleBreathGenerationWriteCount++;
    }

    /// <summary>
    /// Native Mother Brain health-based body-palette flag. Revival writes one only after
    /// the walk to X `$50` has really completed; it is not synonymous with the separate
    /// brain-slot palette handler above.
    /// </summary>
    public bool HealthBasedPaletteHandlingEnabled { get; internal set; }

    /// <summary>Palette-transition record index at WRAM <c>$7E:802E</c>.</summary>
    public int GreyTransitionCounter { get; internal set; }

    /// <summary>
    /// Cross-actor corpse handshake polled by the Baby: <see cref="MotherBrainPhase2CorpseState.Corpse"/> once
    /// Mother Brain has completed the ninth grey-table probe, then <see cref="MotherBrainPhase2CorpseState.Revived"/>.
    /// </summary>
    public MotherBrainPhase2CorpseState Phase2CorpseState { get; internal set; }

    /// <summary>
    /// Saturating `$00-$0C` counter incremented by head instruction `$A9:9EA3` at the start
    /// of each four-ring attack against the Baby. It also selects the intended cry pitch.
    /// </summary>
    public int BabyMetroidAttackCounter { get; internal set; }

    /// <summary>
    /// Number of live Mother Brain bomb enemy projectiles at body WRAM <c>$7E:802A</c>.
    /// </summary>
    /// <remarks>
    /// The counter belongs to Mother Brain even though bank <c>$86</c> owns each bomb's
    /// motion. Bomb initialization increments it and both native deletion paths decrement
    /// it. Keeping the mutation behind these two methods makes that cross-bank ownership
    /// visible instead of silently deriving a count from the host projectile collection.
    /// </remarks>
    public int BombCounter { get; private set; }

    /// <summary>Brain-slot health rewritten to 36,000 when corpse state one is published.</summary>
    public int BrainHealth { get; internal set; } = 3000;

    /// <summary>
    /// Index of the room shake this actor last requested (0, 2, 5 or 8), written by the one-frame
    /// delay, the drain initializer and the death sequence; the room earthquake owns the shake.
    /// </summary>
    public int EarthquakeType { get; private set; }

    /// <summary>
    /// Earthquake timer as last written by this actor. The global earthquake updater owns
    /// independent per-frame decrements, so this class does not fabricate them.
    /// </summary>
    public NativeCountdown EarthquakeTimer { get; private set; }

    /// <summary>Samus projectile cooldown written at beam shutdown.</summary>
    public NativeCountdown SamusProjectileCooldownTimer { get; private set; }

    /// <summary>
    /// Zero-based index of the next Baby Metroid sprite-tile transfer. Four means the
    /// terminating zero entry has been observed and the spawn handoff has run.
    /// </summary>
    public int BabyMetroidTileTransferIndex { get; internal set; }

    /// <summary>
    /// Phase-three walking function selected through native long word <c>$7E:801E</c>.
    /// This is deliberately separate from the visible body instruction list: the function
    /// decides what Mother Brain wants to do only while the bytecode-owned pose is standing.
    /// </summary>
    public MotherBrainPhase3WalkingPhase Phase3WalkingPhase { get; private set; } =
        MotherBrainPhase3WalkingPhase.Inactive;

    /// <summary>
    /// The walk-credit word <c>$7E:780E</c>, which an encounter shares with the sequence it attaches; a standalone
    /// sequence keeps its own.
    /// </summary>
    private MotherBrainWalkCounter _walk = new();

    /// <summary>
    /// Mother Brain's walk credit (<c>$7E:780E</c>) as the sequence uses it. Ordinary projectiles subtract
    /// <c>$0100</c>; Hyper Beam subtracts <c>$010A</c>; the walking function adds
    /// <c>$0020</c> whenever it gets a standing AI call.
    /// </summary>
    public MotherBrainWalkCredit Phase3WalkCounter
    {
        get => _walk.Credit;
        internal set => _walk.Credit = value;
    }

    /// <summary>Reads and writes <paramref name="walk"/> from now on, the word of the encounter attaching this sequence.</summary>
    internal void ShareWalkCounter(MotherBrainWalkCounter walk) => _walk = Ensure.NotNull(walk);

    /// <summary>Current phase-three walking target X in room pixels (<c>$7E:803A</c>), within a pixel of the body's walking range.</summary>
    public int Phase3TargetXPosition { get; private set; }

    /// <summary>Phase-three neck function selected through native long word <c>$7E:801A</c>.</summary>
    public MotherBrainPhase3NeckPhase Phase3NeckPhase { get; private set; } =
        MotherBrainPhase3NeckPhase.Inactive;

    /// <summary>Signed-underflow timer shared by the two phase-three recoil functions.</summary>
    public NativeCountdown Phase3NeckFunctionTimer { get; private set; }

    /// <summary>
    /// Whether phase-three attacks are suspended (<c>$7E:803E</c>). Hyper Beam recoil sets it
    /// and the recoil timer clears it before the recovery pose begins.
    /// </summary>
    public bool Phase3DisableAttacks { get; private set; }

    /// <summary>
    /// Exact body enemy property word touched by `$A9:AEE1` and `$A9:AFF7`. Keeping the raw
    /// flags avoids guessing names for engine-wide enemy-property bits before bank `$A0` is
    /// fully translated: death sets `$0400`, then fade completion sets `$0100` and clears
    /// `$2000` with the cartridge's literal OR/AND sequence.
    /// </summary>
    public EnemyPropertyWord BodyProperties { get; private set; }

    /// <summary>Exact brain enemy property word; death/rotting sets raw bits `$0400/$0100`.</summary>
    public EnemyPropertyWord BrainProperties { get; private set; }

    /// <summary>Second brain property word cleared when the rot animation completes.</summary>
    public EnemyExtraPropertyWord BrainProperties2 { get; private set; }

    /// <summary>Second body property word cleared when the faded body becomes non-interactive.</summary>
    public EnemyExtraPropertyWord BodyProperties2 { get; private set; }

    /// <summary>Shared Mother Brain hitbox-enable word cleared at both death boundaries.</summary>
    public bool HitboxesEnabled { get; private set; } = true;

    /// <summary>
    /// Native word <c>$0FF0</c>: the explosion interval timer of <c>$A9:B03E</c> and
    /// <c>$A9:B346</c>, aliased with the body sub-function pointer.
    /// </summary>
    public NativeCountdown DeathExplosionIntervalTimer { get; internal set; }

    /// <summary>
    /// Native word <c>$0FF2</c>, one word under three names: the ascent dust's body
    /// sub-function timer, the seven-record death-explosion index (<c>$A9:B046</c>) and the
    /// four-record escape-door dust index (<c>$A9:B355</c>). Each effect inherits the value
    /// the previous one left unless native code explicitly clears it.
    /// </summary>
    public NativeCountdown DeathAndEscapeExplosionIndex { get; internal set; }

    /// <summary>
    /// Inherits word <c>$0FF2</c> from the room's encounter state when the live sequence
    /// attaches; the fake-death ascent dust last wrote it.
    /// </summary>
    internal void InheritDeathAndEscapeExplosionIndex(ushort bodySubFunctionTimer) =>
        DeathAndEscapeExplosionIndex = NativeCountdown.FromRaw(bodySubFunctionTimer);

    /// <summary>OBJ palette selection forced to palette seven when the dying brain effects shut down.</summary>
    public SnesObjAttributeWord BrainPalette { get; internal set; }

    /// <summary>Number of calls made to the body flicker producer at `$A9:AFB6`.</summary>
    public uint BodyFlickerCallCount { get; private set; }

    /// <summary>
    /// Host witness for the `$02C6..0` BG2 tilemap clear and following NMI transfer request.
    /// Rendering code can consume this without pretending the global WRAM tilemap is local.
    /// </summary>
    public bool EnemyBg2TilemapClearRequested { get; private set; }

    /// <summary>Index of the next of six `$A9:9003` corpse sprite-tile DMA records.</summary>
    public int CorpseTileTransferIndex { get; private set; }

    /// <summary>Index of the next NTSC escape-timer tile record at <c>$A6:C4CB</c>.</summary>
    public int EscapeTimerTileTransferIndex { get; private set; }

    /// <summary>Index of the next exploded-door tile record at <c>$A9:902F</c>.</summary>
    public int ExplodedDoorTileTransferIndex { get; private set; }

    /// <summary>Unpause-hook enable word cleared when the escape typewriter is installed.</summary>
    public bool MotherBrainUnpauseHookEnabled { get; internal set; } = true;

    /// <summary>
    /// Set when `$A9:B11B` installs the brain-slot draw setup before the decapitated head
    /// starts falling. The separate brain AI is not silently folded into the body function.
    /// </summary>
    public bool BrainDrawSetupRequested { get; private set; }

    /// <summary>Rainbow-beam palette animation index reset immediately before the final shot.</summary>
    public int RainbowBeamPaletteAnimationIndex { get; internal set; }

    /// <summary>True once `$A9:BE1B` has requested the cutscene Baby enemy population entry.</summary>
    public bool BabyMetroidSpawned { get; internal set; }

    /// <summary>
    /// Executes the corpse-table and graphics-buffer half of head initialization
    /// <c>$A9:8705-$870B</c>. A real encounter calls this once when the brain enemy spawns.
    /// </summary>
    public void InitializeCorpseRotting() =>
        CorpseRotting.Initialize(CorpseStaging, _corpseArtwork ?? throw new InvalidDataException(
            "Mother Brain corpse rotting requires installed tile artwork."));

    /// <summary>
    /// Starts the repeatable rainbow-beam cycle at `$A9:B8EB`, before its two charge waits.
    /// </summary>
    public void StartAttackCycle()
    {
        BabyMetroidTileTransferIndex = 0;
        BabyMetroidSpawned = false;
        RainbowBeamPaletteAnimationIndex = 0;
        BeginExtendingNeckForAttack();
    }

    /// <summary>
    /// Imports the physical room actor words before one live scheduler call. The standalone
    /// verifier may continue to own <see cref="Body"/> directly; gameplay instead lets the
    /// ordinary enemy bytecode move the real body and feeds those resulting words back here.
    /// This deliberately does not copy instruction pointers or timers: there must be only
    /// one visible animation interpreter in a live room.
    /// </summary>
    internal void SynchronizeLiveActor(MotherBrainLiveActorSnapshot actor)
    {
        Body.Position = actor.Body;
        Body.Pose = actor.BodyPose;
        Body.Form = actor.Form;
        BodyProperties = actor.BodyProperties;
        BodyProperties2 = actor.BodyExtraProperties;
        BrainPosition = actor.Brain;
        BrainHealth = actor.BrainHealth;
        BrainProperties = actor.BrainProperties;
        BrainProperties2 = actor.BrainExtraProperties;
        LowerNeckAngle = actor.LowerNeckAngle;
        UpperNeckAngle = actor.UpperNeckAngle;
        NeckMovementEnabled = actor.NeckMovementEnabled;
        LowerNeckMovementIndex = actor.LowerNeckMovementIndex;
        UpperNeckMovementIndex = actor.UpperNeckMovementIndex;
        BombCounter = actor.BombCounter;
        HitboxesEnabled = actor.HitboxesEnabled;
    }

    /// <summary>
    /// The list most recently requested by the state machine's body helper. Gameplay copies
    /// this pointer into its physical body slot only when the step result reports a walk or
    /// posture request; it never advances this private mirror's instruction timer.
    /// </summary>
    internal MotherBrainBodyProgram? RequestedBodyProgram => Body.Program;

    /// <summary>
    /// Starts at `$A9:B983`, immediately after the power-bomb gate and charge countdown.
    /// </summary>
    public void StartActiveBeam(SamusState samus)
    {
        Ensure.NotNull(samus);

        SetHeadProgram(MotherBrainHeadProgram.FiringRainbowBeam);
        AngularWidth = 512;
        ExplosionIndex = 0;
        ExplosionTimer = NativeCountdown.Start(0);
        SoundQueueCount = NativeCountdown.Start(6);
        RainbowBeamSoundPlaying = false;
        HdmaActive = true;
        EarthquakeType = 0;
        EarthquakeTimer = NativeCountdown.Start(0);
        SamusProjectileCooldownTimer = NativeCountdown.Start(0);
        NeckAngleDelta = 64;
        NeckMovementEnabled = true;
        LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
        UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;

        // `$A9:B9C5-$B9D3` selects command five at 700 energy or above and command `$18`
        // below it. CPY/BPL is a signed 16-bit branch, retained by NativeAtLeast.
        if (samus.Health >= 700)
            samus.Drained.SetupForRainbowBeamAbleToStand(samus);
        else
            samus.Drained.SetupForRainbowBeamUnableToStand(samus);

        Phase = MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall;
    }

    /// <summary>
    /// Ports the cross-enemy function-pointer write at <c>$A9:C8CD</c>. The Baby actor
    /// installs <c>$BE38</c> after it has reached the brain; Mother Brain executes that new
    /// function on her next enemy-AI turn rather than inside the Baby's current turn.
    /// </summary>
    public void InterruptFinalBeamForBabyDrain()
    {
        Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback;
        RainbowBeamSoundPlaying = true; // `$A9:C8DA-$C8DD` writes the shared SFX flag.
    }

    /// <summary>
    /// Ports the Baby's cross-enemy write at <c>$A9:CB23</c>. The native routine does not
    /// wait for Mother Brain's stand-up animation before repeatedly requesting the backward
    /// walk; the later Baby route decides when to replace this function with <c>$C19A</c>.
    /// </summary>
    public void PrepareForFinalBabyMetroidAttack() =>
        Phase = MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack;

    /// <summary>
    /// Ports the Baby's cross-enemy write at <c>$A9:CBA8</c>. Mother Brain installs one last
    /// `$9DB1` four-ring head program and then leaves her body function at the native RTS.
    /// </summary>
    public void ExecuteFinalBabyMetroidAttack() =>
        Phase = MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack;

    /// <summary>
    /// Applies physical head opcode <c>$A9:9EA3</c>. Live gameplay advances the ordinary
    /// room instruction list, while the reusable head verifier advances its private list;
    /// both must mutate this one native saturating counter.
    /// </summary>
    internal void IncrementLiveBabyMetroidAttackCounter() =>
        BabyMetroidAttackCounter = Math.Min(BabyMetroidAttackCounter + 1, 12);

    /// <summary>Applies physical head opcode <c>$A9:9EB5</c>.</summary>
    internal void ResetLiveBabyMetroidAttackCounter() => BabyMetroidAttackCounter = 0;

    /// <summary>
    /// Ports the Baby's final cross-enemy write at <c>$A9:CD02-$CD05</c>. The body does
    /// not execute <c>$C1CF</c> inside the Baby's slot; increasing-slot enemy order makes
    /// this installed phase visible on Mother Brain's following frame.
    /// </summary>
    public void BeginPhase3RecoveryFromBabyCutscene()
    {
        Phase = MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance;

        // Native clears only the shared enemy-slot index. This host flag is the equivalent
        // liveness witness used by head attack selection after the actor has deleted itself.
        BabyMetroidSpawned = false;
    }

    /// <summary>
    /// Applies the movement/recoil half of Mother Brain's shared phase-two/three shot
    /// reaction at <c>$A9:B562-$B5C4</c>. The ordinary enemy-shot routine owns damage,
    /// projectile deletion, and flash time; this method intentionally does not duplicate
    /// any of those separately translated systems.
    /// </summary>
    public void ApplyPhase2Or3ShotReaction(MotherBrainProjectileType projectileType)
    {
        // `$B58E` masks the projectile type to three bits before indexing an eight-byte
        // table. Validate the host enum so a caller cannot accidentally smuggle a larger
        // value past that native domain.
        MotherBrainShotReactionResult reaction =
            MotherBrainShotReaction.Resolve(Body.Form, projectileType, Phase3WalkCounter);
        Phase3WalkCounter = reaction.WalkCredit;
        if (reaction.HyperBeamRecoil)
        {
            // The neck function itself runs on Mother Brain's next ordinary phase-three
            // main call.
            Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil;
            FunctionTimer = NativeCountdown.Start(0);
        }
    }

    /// <summary>
    /// Ports the Baby's <c>$A9:C879</c> call to the standard backwards-walk helper using
    /// animation-delay index two and target <c>Body.X-1</c>.
    /// </summary>
    public bool RequestBabyStumbleBackward()
    {
        ushort targetX = unchecked((ushort)(Body.Position.X - 1));
        return MakeBodyWalkBackwards(targetX, MotherBrainBodyProgram.WalkBackwardReallyFast).Requested;
    }

    /// <summary>Resets the phase-two head list and starts the 256-count neck extension before beam charging.</summary>
    private void BeginExtendingNeckForAttack()
    {
        // `$A9:B8EB-$B916` resets the neutral phase-two head program and selects the
        // ordinary 2/4 neck geometry before beginning its first 256-count wait.
        SetHeadProgram(MotherBrainHeadProgram.NeutralPhase2);
        NeckAngleDelta = 64;
        NeckMovementEnabled = true;
        LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
        UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        Phase = MotherBrainRainbowBeamAttackPhase.StartCharging;
        FunctionTimer = NativeCountdown.Start(256);
    }

    /// <summary>Restarts the separate brain instruction stream and publishes a new head-list request serial.</summary>
    private void SetHeadProgram(MotherBrainHeadProgram program)
    {
        // `$A9:C447` restarts the separate brain program and its timer. Unlike ordinary
        // enemy programs it has no loop-counter write here.
        HeadProgram = program;
        _headInstructionListRequestSerial++;
    }

    /// <summary>Sets the NTSC neck angle and opposed raise/lower indices for retreat after the attack.</summary>
    private void RetractHead()
    {
        // NTSC takes `$0050` from the regional `$0050/$0063` constant at `$A9:BB51`.
        NeckAngleDelta = 80;
        NeckMovementEnabled = true;
        LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
        UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
    }

    /// <summary>Alternates painful forward and backward walk requests, advancing the stage only after its pause expires.</summary>
    internal bool StepPainfulWalking()
    {
        // The native stores four separate function pointers (request forward, wait forward,
        // request backward, wait backward). Two booleans express the same state without
        // hiding when a body instruction list is actually installed.
        if (!PainfulWalkingFunctionTimer.IsZero)
        {
            PainfulWalkingFunctionTimer = PainfulWalkingFunctionTimer.Decrement().Remaining;
            if (PainfulWalkingFunctionTimer.IsZero)
            {
                PainfulWalkingStage++;
                PainfulWalkingForward = !PainfulWalkingForward;
            }
            return false;
        }

        MotherBrainWalkResult walk = PainfulWalkingForward
            ? RequestWalkForward(72, PainfulWalkingAnimationDelay)
            : RequestWalkBackward(40, PainfulWalkingAnimationDelay);
        if (walk.ReachedTarget)
        {
            int timerIndex = Math.Min(PainfulWalkingStage, 7);
            PainfulWalkingFunctionTimer = NativeCountdown.Start(MotherBrainPainfulWalkingDefinitions.FunctionTimer(timerIndex));
        }
        return walk.Requested;
    }

    /// <summary>Schedules phase-three inching and two retreat speeds only while the body is standing.</summary>
    private bool StepPhase3WalkingHandler(MotherBrainRandomSample randomNumberSeed)
    {
        // `$C25A` refuses to call the walking function while body bytecode advertises any
        // nonzero pose. This makes each request edge-triggered: the requested walk runs to
        // its standing opcode before the scheduler is allowed to inspect its target again.
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return false;

        switch (Phase3WalkingPhase)
        {
            case MotherBrainPhase3WalkingPhase.Inactive:
                return false;

            case MotherBrainPhase3WalkingPhase.TryToInchForward:
                if (Phase3WalkCounter.IsEmpty)
                {
                    // Zero credit does not merely pause. `$C2A1` chooses a point fourteen
                    // pixels left, installs the quick-retreat function, and falls into it.
                    Phase3TargetXPosition = Body.Position.X - 14;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatQuickly;
                    goto case MotherBrainPhase3WalkingPhase.RetreatQuickly;
                }

                // Native ADC/CMP is unsigned here. A wrapped counter can therefore spend
                // more calls below `$0100`; no host integer widening or saturation belongs.
                Phase3WalkCounter = Phase3WalkCounter.AddCredit(32);
                if (Phase3WalkCounter.BelowStep)
                    return false;

                Phase3TargetXPosition = Body.Position.X + 1;
                int forwardDelay = (randomNumberSeed.HasBit(1) ? 2 : 0) + 4;

                // MakeMotherBrainWalkForwards reports carry only for a strictly overshot
                // target or the independent X `$80` limit. Equality still requests a walk.
                bool reachedForwardTarget =
                    unchecked((short)(Phase3TargetXPosition - Body.Position.X)) < 0 ||
                    NativeAtLeast(Body.Position.X, 128);
                if (reachedForwardTarget)
                {
                    Phase3WalkCounter = MotherBrainWalkCredit.Of(128);
                    return false;
                }

                return RequestWalkForward(Phase3TargetXPosition, forwardDelay).Requested;

            case MotherBrainPhase3WalkingPhase.RetreatQuickly:
                MotherBrainWalkResult quickRetreat =
                    RequestWalkBackward(Phase3TargetXPosition, animationDelay: 2);
                if (!quickRetreat.ReachedTarget)
                    return quickRetreat.Requested;

                // Reaching the first target chooses another point fourteen pixels left but
                // does not fall through to the slow request. That request starts only on the
                // next standing AI call.
                Phase3TargetXPosition = Body.Position.X - 14;
                Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.RetreatSlowly;
                return false;

            case MotherBrainPhase3WalkingPhase.RetreatSlowly:
                MotherBrainWalkResult slowRetreat =
                    RequestWalkBackward(Phase3TargetXPosition, animationDelay: 4);
                if (!slowRetreat.ReachedTarget)
                    return slowRetreat.Requested;

                // `$C313` writes all three words: forty walk-credit, the inch-forward
                // function, and a one-pixel-ahead target used by later calls.
                SetPhase3WalkingToTryToInchForward(64);
                return false;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three walking function {Phase3WalkingPhase}.");
        }
    }

    /// <summary>Resets walk credit and targets the next pixel ahead after a completed retreat.</summary>
    private void SetPhase3WalkingToTryToInchForward(int credit)
    {
        Phase3WalkCounter = MotherBrainWalkCredit.Of(credit);
        Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;
        Phase3TargetXPosition = Body.Position.X + 1;
    }

    /// <summary>Advances normal neck motion and the timed recovery after regular or hyper-beam recoil.</summary>
    private void StepPhase3NeckHandler()
    {
        switch (Phase3NeckPhase)
        {
            case MotherBrainPhase3NeckPhase.Inactive:
                return;

            case MotherBrainPhase3NeckPhase.Normal:
                // `$C330` briefly writes lower index one and immediately overwrites it with
                // two before any other code can observe the word. Preserve the observable
                // result while documenting the retail no-op (`>_<` in the disassembly).
                NeckAngleDelta = 128;
                LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
                UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Inactive;
                return;

            case MotherBrainPhase3NeckPhase.SetupRecoilRecovery:
                NeckMovementEnabled = true;
                NeckAngleDelta = 1280;
                LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
                UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.RecoilRecovery;
                Phase3NeckFunctionTimer = NativeCountdown.Start(16);

                // `$C354` falls directly into `$C37B`; the newly loaded sixteen is already
                // fifteen when this single handler call returns.
                goto case MotherBrainPhase3NeckPhase.RecoilRecovery;

            case MotherBrainPhase3NeckPhase.RecoilRecovery:
                if (!Phase3NeckFunctionTimer.IsZero)
                {
                    Phase3NeckFunctionTimer = Phase3NeckFunctionTimer.Decrement().Remaining;
                    return;
                }

                // DEC of zero branches negative without storing `$FFFF`, so the readable
                // timer remains zero while the four-ring recovery list is installed.
                SetHeadProgram(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                return;

            case MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil:
                NeckMovementEnabled = true;
                Phase3DisableAttacks = true;
                SetHeadProgram(MotherBrainHeadProgram.HyperBeamRecoil);
                BrainMainShakeTimer = NativeCountdown.Start(50);
                NeckAngleDelta = 2304;
                LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
                UpperNeckMovementIndex = MotherBrainNeckMovement.Raise;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.HyperBeamRecoil;
                Phase3NeckFunctionTimer = NativeCountdown.Start(11);

                // Setup also falls through, making the externally visible first timer `$0A`.
                goto case MotherBrainPhase3NeckPhase.HyperBeamRecoil;

            case MotherBrainPhase3NeckPhase.HyperBeamRecoil:
                if (!Phase3NeckFunctionTimer.IsZero)
                {
                    Phase3NeckFunctionTimer = Phase3NeckFunctionTimer.Decrement().Remaining;
                    return;
                }

                NeckAngleDelta = 128;
                Phase3DisableAttacks = false;
                Phase3NeckPhase = MotherBrainPhase3NeckPhase.SetupRecoilRecovery;
                return;

            default:
                throw new InvalidOperationException(
                    $"Unsupported phase-three neck function {Phase3NeckPhase}.");
        }
    }

    /// <summary>Selects the forward body list for the native animation delay and applies the forward target rules.</summary>
    private MotherBrainWalkResult RequestWalkForward(int targetX, int animationDelay)
    {
        MotherBrainBodyProgram program = animationDelay switch
        {
            2 => MotherBrainBodyProgram.WalkForwardReallyFast,
            4 => MotherBrainBodyProgram.WalkForwardFast,
            6 => MotherBrainBodyProgram.WalkForwardMedium,
            8 => MotherBrainBodyProgram.WalkForwardSlow,
            10 => MotherBrainBodyProgram.WalkForwardReallySlow,
            _ => throw new InvalidOperationException(
                $"Unsupported painful forward animation delay ${animationDelay:X4}."),
        };
        return MakeBodyWalkForwards(targetX, program);
    }

    /// <summary>Selects the backward body list for the native animation delay and applies the backward target rules.</summary>
    private MotherBrainWalkResult RequestWalkBackward(int targetX, int animationDelay)
    {
        MotherBrainBodyProgram program = animationDelay switch
        {
            2 => MotherBrainBodyProgram.WalkBackwardReallyFast,
            4 => MotherBrainBodyProgram.WalkBackwardFast,
            6 => MotherBrainBodyProgram.WalkBackwardMedium,
            8 => MotherBrainBodyProgram.WalkBackwardSlow,
            10 => MotherBrainBodyProgram.WalkBackwardReallySlow,
            _ => throw new InvalidOperationException(
                $"Unsupported painful backward animation delay ${animationDelay:X4}."),
        };
        return MakeBodyWalkBackwards(targetX, program);
    }

    /// <summary>Requests the slow forward body animation until its target is reached.</summary>
    internal MotherBrainWalkResult RequestWalkForwardReallySlow(int targetX) =>
        MakeBodyWalkForwards(targetX, MotherBrainBodyProgram.WalkForwardReallySlow);

    /// <summary>Requests the slow backward body animation until its target is reached.</summary>
    private MotherBrainWalkResult RequestWalkBackwardReallySlow(int targetX) =>
        MakeBodyWalkBackwards(targetX, MotherBrainBodyProgram.WalkBackwardReallySlow);

    /// <summary>
    /// Ports <c>$A9:C601</c>. A signed overshoot of the target reports carry first; at
    /// equality the walk is still requested. A body still mid-pose then reports no carry,
    /// ahead of the <c>$80</c> arena limit that also reports carry. Only a standing body
    /// inside both limits installs the walk.
    /// </summary>
    private MotherBrainWalkResult MakeBodyWalkForwards(int targetX, MotherBrainBodyProgram program)
    {
        if (unchecked((short)(targetX - Body.Position.X)) < 0)
            return MotherBrainWalkResult.Reached;
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (NativeAtLeast(Body.Position.X, 128))
            return MotherBrainWalkResult.Reached;
        Body.SetProgram(program);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>
    /// Ports <c>$A9:C647</c>. Reaching or passing the target reports carry first, so a walk
    /// can complete the AI phase on the frame its movement opcode arrives. A body still
    /// mid-pose then reports no carry, ahead of the <c>$30</c> arena limit that also reports
    /// carry. Only a standing body inside both limits installs the walk.
    /// </summary>
    private MotherBrainWalkResult MakeBodyWalkBackwards(int targetX, MotherBrainBodyProgram program)
    {
        if (unchecked((short)(targetX - Body.Position.X)) >= 0)
            return MotherBrainWalkResult.Reached;
        if (Body.Pose != MotherBrainBodyPose.Standing)
            return MotherBrainWalkResult.Waiting;
        if (unchecked((short)(Body.Position.X - 48)) < 0)
            return MotherBrainWalkResult.Reached;
        Body.SetProgram(program);
        return MotherBrainWalkResult.WalkInstalled;
    }

    /// <summary>Applies Gravity/Varia damage reduction and the native extra scaling used only by the $50 beam strike.</summary>
    private static ushort CalculateFinishOffHealthThreshold(SamusState samus, ushort nominalDamage)
    {
        // `$A0:A45E` gives Gravity Suit priority and divides damage by four. Otherwise
        // Varia's bit zero divides by two; Power Suit leaves it untouched. `$A9:BD4C`
        // then multiplies the divided `$50` result by four, while `$BD68` deliberately
        // does not multiply the divided `$A0` result. The caller selects that distinction.
        ushort divided = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? (ushort)(nominalDamage >> 2)
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? (ushort)(nominalDamage >> 1)
                : nominalDamage;

        return nominalDamage == 80
            ? unchecked((ushort)(divided * 4 + 20))
            : unchecked((ushort)(divided + 20));
    }

    /// <summary>Uses the random low byte at or above $C0 to toggle a standing or leaning body list.</summary>
    private bool MaybeRequestStandUpOrLeanDown(MotherBrainRandomSample randomNumberSeed)
    {
        // `$A9:C1A7` ignores every pose except standing (zero) and leaning (six), and its
        // low-byte threshold is inclusive at `$C0`. A request still executes as body
        // instruction bytecode later in the enemy-processing stage.
        if (randomNumberSeed.LowByte < 192)
            return false;

        if (Body.Pose == MotherBrainBodyPose.Standing)
        {
            Body.SetProgram(MotherBrainBodyProgram.LeanDown);
            return true;
        }

        if (Body.Pose == MotherBrainBodyPose.LeaningDown)
        {
            MakeBodyStandUp(out bool requested);
            return requested;
        }

        return false;
    }

    /// <summary>Requests the pose-specific standing animation and reports whether the body is already standing.</summary>
    private bool MakeBodyStandUp(out bool animationRequested)
    {
        animationRequested = false;
        if (Body.Pose == MotherBrainBodyPose.Standing)
            return true;

        MotherBrainBodyProgram? program = Body.Pose switch
        {
            MotherBrainBodyPose.Crouched => MotherBrainBodyProgram.StandAfterCrouchFast,
            MotherBrainBodyPose.LeaningDown => MotherBrainBodyProgram.StandAfterLeaning,
            MotherBrainBodyPose.Standing or MotherBrainBodyPose.Walking or
                MotherBrainBodyPose.CrouchingTransition or MotherBrainBodyPose.DeathBeam => null,
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainBodyPose)} {(int)Body.Pose}."),
        };
        if (program is { } standUp)
        {
            Body.SetProgram(standUp);
            animationRequested = true;
        }

        return false;
    }

    /// <summary>Consumes the next Baby Metroid sprite-tile page, rejecting calls after the native list ends.</summary>
    private MotherBrainSpriteTileTransferRequest CreateNextBabyMetroidTileTransfer()
    {
        int index = BabyMetroidTileTransferIndex;
        if ((uint)index >= MotherBrainTileTransferDefinitions.BabyTileCount)
            throw new InvalidOperationException("Baby Metroid sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request =
            MotherBrainTileTransferDefinitions.BabyTileTransfer(index);
        BabyMetroidTileTransferIndex++;
        return request;
    }

    /// <summary>Publishes the next corpse VRAM page from the authored source and destination rows.</summary>
    private MotherBrainSpriteTileTransferRequest CreateNextCorpseTileTransfer()
    {
        int index = CorpseTileTransferIndex;
        if ((uint)index >= MotherBrainCorpseArtworkDefinitions.RowCount)
            throw new InvalidOperationException("Mother Brain corpse tile transfer list is already complete.");

        var request = new MotherBrainSpriteTileTransferRequest(
            EntryIndex: (ushort)index,
            Size: MotherBrainCorpseArtworkDefinitions.VramPageByteCount,
            SourceAddress: MotherBrainCorpseArtworkDefinitions.VramPageSource(index),
            VramDestination: MotherBrainCorpseArtworkDefinitions.VramPageDestination(index));
        CorpseTileTransferIndex++;
        return request;
    }

    /// <summary>Consumes the next escape-timer text tile transfer in its fixed artwork order.</summary>
    internal MotherBrainSpriteTileTransferRequest CreateNextEscapeTimerTileTransfer()
    {
        int index = EscapeTimerTileTransferIndex;
        if ((uint)index >= MotherBrainEscapeTextArtworkDefinitions.TransferCount)
            throw new InvalidOperationException("Escape-timer sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request = MotherBrainEscapeTextArtworkDefinitions.Transfer(index);
        EscapeTimerTileTransferIndex++;
        return request;
    }

    /// <summary>Consumes the next exploded-door sprite page until that artwork list is exhausted.</summary>
    internal MotherBrainSpriteTileTransferRequest CreateNextExplodedDoorTileTransfer()
    {
        int index = ExplodedDoorTileTransferIndex;
        if ((uint)index >= (uint)MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.PageCount)
            throw new InvalidOperationException("Exploded-door sprite-tile transfer list is already complete.");

        MotherBrainSpriteTileTransferRequest request = MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(index);
        ExplodedDoorTileTransferIndex++;
        return request;
    }

    /// <summary>
    /// Emits the death-phase explosions at the native timer cadence, consuming one global
    /// random value per projectile so later enemy AI sees the same RNG sequence.
    /// </summary>
    internal void GenerateDeathExplosions(
        MotherBrainDeathExplosionStyle style,
        Func<int>? nextRandomNumber,
        List<MotherBrainDeathExplosionRequest> requests)
    {
        // `$B03E` decrements before testing BPL. A cleared timer therefore wraps and emits
        // immediately. Crucially, the interval literal remains in A across the memory DEC,
        // so the smoky/mixed caller writes exactly `$10`/`$08` after an expiry.
        DeathExplosionIntervalTimer = DeathExplosionIntervalTimer.Decrement().Remaining;
        if (!DeathExplosionIntervalTimer.IsNegative)
            return;

        DeathExplosionIntervalTimer = NativeCountdown.Start(style == MotherBrainDeathExplosionStyle.Mixed ? 8 : 16);
        DeathAndEscapeExplosionIndex = DeathAndEscapeExplosionIndex.Decrement().Remaining;
        if (DeathAndEscapeExplosionIndex.IsNegative)
            DeathAndEscapeExplosionIndex = NativeCountdown.Start(MotherBrainDeathExplosionDefinitions.GroupCount - 1);

        int simultaneousCount = style == MotherBrainDeathExplosionStyle.Mixed ? MotherBrainDeathExplosionDefinitions.AnchorsPerGroup : 2;
        int pairIndex = DeathAndEscapeExplosionIndex.RawValue * MotherBrainDeathExplosionDefinitions.AnchorsPerGroup;
        for (int explosionIndex = 0; explosionIndex < simultaneousCount; explosionIndex++)
        {
            // The global RNG is called once per projectile, not once per visual batch. A
            // callback is required here so the owning frame runtime advances its real global
            // state; silently deriving private random values would desynchronize later AI.
            if (nextRandomNumber is null)
            {
                throw new InvalidOperationException(
                    "Mother Brain death explosions require the global next-random-number producer.");
            }

            int random = nextRandomNumber();
            int parameter = style == MotherBrainDeathExplosionStyle.Mixed
                ? random < 16384 ? 0 : random < 57344 ? 1 : 2
                : 1;
            (short xOffset, short yOffset) = MotherBrainDeathExplosionDefinitions.Anchor(pairIndex + explosionIndex);
            requests.Add(new MotherBrainDeathExplosionRequest(
                Offset: new Offset16<RoomPixels>(xOffset, yOffset),
                ProjectileParameter: parameter,
                SoundEffect: MotherBrainSoundDefinitions.Explosion));
        }
    }

    /// <summary>
    /// Emits the next four-frame escape-door blast from its repeating native position table.
    /// Returns no request on frames before the shared interval timer expires.
    /// </summary>
    private DustExplosionRequest? GenerateEscapeDoorExplosion(
        Func<int>? nextRandomNumber)
    {
        // `$B346` shares the old death-explosion interval word but replaces its cadence with
        // four. A cleared word underflows on the first call, so a new explosion is immediate.
        DeathExplosionIntervalTimer = DeathExplosionIntervalTimer.Decrement().Remaining;
        if (!DeathExplosionIntervalTimer.IsNegative)
            return null;

        DeathExplosionIntervalTimer = NativeCountdown.Start(4);
        DeathAndEscapeExplosionIndex = DeathAndEscapeExplosionIndex.Decrement().Remaining;
        if (DeathAndEscapeExplosionIndex.IsNegative)
            DeathAndEscapeExplosionIndex = NativeCountdown.Start(3);

        // The table is stored as interleaved X/Y words and indexed by `index * 4` bytes.
        // Spell out the semantic pairs so neither host endianness nor array stride can alter
        // the native 3,2,1,0 repeating order.
        Position16<RoomPixels> position = DeathAndEscapeExplosionIndex.RawValue switch
        {
            0 => new(8, 108),
            1 => new(24, 128),
            2 => new(9, 144),
            3 => new(24, 116),
            _ => throw new InvalidOperationException(
                $"Escape-door explosion index ${DeathAndEscapeExplosionIndex.RawValue:X4} escaped its native 0..3 range."),
        };

        if (nextRandomNumber is null)
        {
            throw new InvalidOperationException(
                "Escape-door explosions require the global next-random-number producer.");
        }

        // Native `$B377` advances the one shared RNG exactly once per emitted projectile.
        // Values below a quarter of the range select the smoke animation ($0C); all remaining
        // values retain the preloaded small explosion ($03).
        int random = nextRandomNumber();
        MiscDustAnimation animation = random < 16384 ? MiscDustAnimation.Smoke : MiscDustAnimation.SmallExplosion;
        return new DustExplosionRequest(
            Position: position,
            Animation: animation,
            SoundEffect: SoundEffectLibrary2Sounds.SmallExplosion);
    }

    /// <summary>Widens the beam by $180 up to $C00 and refreshes its angle toward Samus.</summary>
    private void IncreaseWidthAndAim(SamusState samus)
    {
        AngularWidth = Math.Min(AngularWidth + 384, 3072);
        AimBeamAtSamus(samus);
    }

    /// <summary>
    /// Advances the held beam's sound countdown, alternating palette request, aim, width,
    /// and explosion timer in the same call after the charge phase completes.
    /// </summary>
    private void RunContinuingBeamEffects(
        SamusState samus,
        int enemyFrameCounter,
        MotherBrainRainbowBeamGrowth growth,
        ref bool soundQueued,
        ref bool paletteRequested,
        ref MotherBrainRainbowExplosionRequest? explosion)
    {
        // `$A9:BB2E` checks the signed counter before DEC. Starting at six therefore queues
        // on values 6,5,4,3,2,1,0 and leaves `$FFFF` to suppress all later calls.
        if (!SoundQueueCount.IsNegative)
        {
            SoundQueueCount = SoundQueueCount.Decrement().Remaining;
            RainbowBeamSoundPlaying = true;
            soundQueued = true;
        }

        // Palette handling runs only when enemy frame-counter bit one is set. The palette
        // program itself remains presentation work, but its native cadence is observable.
        paletteRequested = (enemyFrameCounter & 2) != 0;

        if (growth == MotherBrainRainbowBeamGrowth.Widening)
            IncreaseWidthAndAim(samus);
        else
            AimBeamAtSamus(samus);
        explosion = StepExplosionTimer();
    }

    /// <summary>Converts the angle from the offset brain origin to Samus into the rainbow beam's orientation.</summary>
    private void AimBeamAtSamus(SamusState samus)
    {
        // `$A9:BB82` aims from brain position (+16,+4) to Samus. Bank `$A0:C0B1` consumes
        // signed 16-bit offsets; `$A9:BBA0-$BBA8` converts its clockwise-up angle to the
        // rainbow beam's anti-clockwise-down convention with `-$angle + $80`.
        short deltaX = unchecked((short)(samus.Position.X.Whole - BrainPosition.X - 16));
        short deltaY = unchecked((short)(samus.Position.Y.Whole - BrainPosition.Y - 4));
        SnesAngle sourceAngle = SamusGrappleMovement.CalculateAngleFromXY(new(deltaX, deltaY));
        _movement.RainbowBeamAngle = SnesAngle.HalfTurn.AddRaw(-sourceAngle.RawValue);
    }

    /// <summary>Emits the next cyclic beam explosion offset when the eight-frame countdown underflows.</summary>
    private MotherBrainRainbowExplosionRequest? StepExplosionTimer()
    {
        ExplosionTimer = ExplosionTimer.Decrement().Remaining;
        if (!ExplosionTimer.IsNegative)
            return null;

        ExplosionTimer = NativeCountdown.Start(8);
        ExplosionIndex = (ExplosionIndex + 1) % ExplosionOffsets.Length;
        return new MotherBrainRainbowExplosionRequest(
            ExplosionOffsets[ExplosionIndex]);
    }

    /// <summary>Drains one health with Varia or two without, clamping native underflow to zero.</summary>
    private static void DamageSamusDueToRainbowBeam(SamusState samus)
    {
        // `$A9:C57D` loads `$FFFE` in both branches. The meaningful Varia distinction is
        // carry left by LSR EquippedItems: bit zero adds one back, producing -1 with Varia
        // and -2 without it. CMP #1/BPL then clamps wrapped/below-one health to zero.
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? 1 : 2;
        int candidate = samus.Health - damage;
        samus.Health = candidate >= 1 ? candidate : 0;
    }

    /// <summary>
    /// Drains missiles and supers every fourth enemy pass and power bombs every pass while
    /// the rainbow beam holds Samus.
    /// </summary>
    private static void DecrementAmmoDueToRainbowBeam(
        SamusState samus,
        int mainEnemyExecutionCounter)
    {
        // Missiles and supers are visited only every fourth main-enemy pass. Power bombs
        // are visited every call, matching the branch layout rather than sharing the gate.
        if ((mainEnemyExecutionCounter & 3) == 0)
        {
            samus.Missiles = DecrementOneAmmoType(samus, samus.Missiles, selectedItem: HudItem.Missile);
            samus.SuperMissiles = DecrementOneAmmoType(samus, samus.SuperMissiles, selectedItem: HudItem.SuperMissile);
        }

        samus.PowerBombs = DecrementOneAmmoType(samus, samus.PowerBombs, selectedItem: HudItem.PowerBomb);
    }

    /// <summary>Decrements one ammo count and clears the selected HUD item on depletion.</summary>
    private static int DecrementOneAmmoType(
        SamusState samus,
        int current,
        HudItem selectedItem)
    {
        if (current == 0)
            return 0;

        int decremented = current - 1;
        if (decremented >= 1)
            return decremented;

        // The 65816 briefly reloads A with SelectedHUDItem for the comparison, but both the
        // matching and nonmatching branches converge at a label whose first instruction is
        // `LDA #$0000`. Therefore the depleted count is always zero; only the selected-item
        // store itself is conditional. Spell that convergence out to avoid the tempting but
        // incorrect interpretation that the HUD item number leaks into the ammo count.
        if (samus.SelectedHudItem == selectedItem)
            samus.SelectedHudItem = HudItem.Beam;

        samus.AutoCancelHudItemIndex = HudItem.Beam;
        return 0;
    }

    /// <summary>Performs the cartridge's signed 16-bit subtraction test for a lower bound.</summary>
    private static bool NativeAtLeast(ushort value, ushort threshold) =>
        unchecked((short)(value - threshold)) >= 0;

    /// <summary>Executes one call through the current Mother Brain body function.</summary>
    public MotherBrainRainbowBeamAttackStepResult Step(SamusState samus, RainbowBeamAttackFrame frame)
    {
        Ensure.NotNull(samus);
        if (Phase == MotherBrainRainbowBeamAttackPhase.Inactive)
            throw new InvalidOperationException("The active rainbow-beam sequence has not started.");

        MotherBrainRainbowBeamAttackPhase phaseBefore = Phase;
        uint headInstructionListRequestSerialBefore = _headInstructionListRequestSerial;
        MotherBrainForcedSamusMovementResult? movement = null;
        bool soundQueued = false;
        bool paletteRequested = false;
        MotherBrainRainbowExplosionRequest? explosion = null;
        int healthBefore = samus.Health;
        int missilesBefore = samus.Missiles;
        int supersBefore = samus.SuperMissiles;
        int bombsBefore = samus.PowerBombs;
        bool chargeSoundQueued = false;
        bool bodyWalkRequested = false;
        bool bodyPostureRequested = false;
        MotherBrainSpriteTileTransferRequest? spriteTileTransfer = null;
        bool babySpawnRequested = false;
        bool finalBeamSoundQueued = false;
        var deathExplosions = new List<MotherBrainDeathExplosionRequest>(capacity: 4);
        var corpseRottingVramTransfers =
            new List<MotherBrainSpriteTileTransferRequest>(capacity: 6);
        var corpseDustRequests = new List<MotherBrainCorpseDustRequest>(capacity: 1);
        bool musicStopQueued = false;
        bool escapeMusicQueued = false;
        var escapeSequenceTileTransfers =
            new List<MotherBrainSpriteTileTransferRequest>(capacity: 2);
        bool explodedDoorPaletteRequested = false;
        bool escapeMusicTrackQueued = false;
        var escapePaletteFxRequests = new List<PaletteFxObject>(capacity: 4);
        bool escapeTypewriterSetupRequested = false;
        bool timeBombSetSubtitleSpawnRequested = false;
        DustExplosionRequest? escapeDoorExplosion = null;
        bool timerHandlingEnableRequested = false;
        bool motherBrainEscapeTimerStartRequested = false;
        bool motherBrainBossBitRequested = false;
        bool zebesTimebombEventRequested = false;
        var escapeDoorParticleSpawns =
            new List<MotherBrainEscapeDoorParticleSpawnRequest>(capacity: 8);
        MotherBrainEscapeDoorPlmRequest? escapeDoorPlm = null;
        bool earthquakeTimerRefreshed = false;

        switch (Phase)
        {
            case MotherBrainRainbowBeamAttackPhase.RepeatAttack:
                // `$A9:BB13` only installs `$B8EB`; the setup executes on the next enemy
                // AI call and deliberately does not fall through into its 256 timer.
                BeginExtendingNeckForAttack();
                break;

            case MotherBrainRainbowBeamAttackPhase.StartCharging:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    SetHeadProgram(MotherBrainHeadProgram.ChargingRainbowBeam);
                    Phase = MotherBrainRainbowBeamAttackPhase.RetractNeck;
                    goto case MotherBrainRainbowBeamAttackPhase.RetractNeck;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.RetractNeck:
                MotherBrainWalkResult retract = RequestWalkBackwardReallySlow(targetX: 40);
                bodyWalkRequested = retract.Requested;
                if (retract.ReachedTarget)
                {
                    RetractHead();
                    Phase = MotherBrainRainbowBeamAttackPhase.WaitForCharge;
                    FunctionTimer = NativeCountdown.Start(256);
                    goto case MotherBrainRainbowBeamAttackPhase.WaitForCharge;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.WaitForCharge:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    chargeSoundQueued = true; // Sound library two, effect `$71`.
                    Phase = MotherBrainRainbowBeamAttackPhase.ExtendNeckDown;
                    goto case MotherBrainRainbowBeamAttackPhase.ExtendNeckDown;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.ExtendNeckDown:
                SamusProjectileCooldownTimer = NativeCountdown.Start(8);
                LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
                UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                NeckAngleDelta = 1280; // NTSC value of regional `$0500/$0700`.
                Phase = MotherBrainRainbowBeamAttackPhase.StartFiring;
                FunctionTimer = NativeCountdown.Start(16); // NTSC value of regional `$0010/$000C`.
                goto case MotherBrainRainbowBeamAttackPhase.StartFiring;

            case MotherBrainRainbowBeamAttackPhase.StartFiring:
                // `$A9:B975` widens/aims even while an active power bomb freezes the timer.
                IncreaseWidthAndAim(samus);
                if (!frame.PowerBombActive)
                {
                    FunctionTimer = FunctionTimer.Decrement().Remaining;
                    if (FunctionTimer.IsNegative)
                    {
                        // The cartridge redundantly rereads the power-bomb flag here. The
                        // parameter is one stable WRAM sample for this translated AI call.
                        SamusProjectileCooldownTimer = NativeCountdown.Start(0);
                        StartActiveBeam(samus);
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall:
                RunContinuingBeamEffects(
                    samus,
                    frame.EnemyFrameCounter,
                    growth: MotherBrainRainbowBeamGrowth.Widening,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                movement = _movement.MoveTowardWall(samus);
                if (movement.Value.ReachedVerticalBoundary)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.OneFrameDelay;
                    FunctionTimer = NativeCountdown.Start(0);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.OneFrameDelay:
                RunContinuingBeamEffects(
                    samus,
                    frame.EnemyFrameCounter,
                    growth: MotherBrainRainbowBeamGrowth.Widening,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                movement = _movement.MoveTowardWall(samus);

                // `$A9:BA0F` decrements zero to `$FFFF`; BPL fails on this very call.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    EarthquakeType = 8;
                    EarthquakeTimer = NativeCountdown.Start(8);
                    Phase = MotherBrainRainbowBeamAttackPhase.StartDrainingSamus;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.StartDrainingSamus:
                // `$A9:BA27` installs draining, seeds both timers with `$012B`, and falls
                // through into the draining function in the same enemy-processing call.
                Phase = MotherBrainRainbowBeamAttackPhase.DrainingSamus;
                FunctionTimer = NativeCountdown.Start(299);
                EarthquakeTimer = NativeCountdown.Start(299);
                EarthquakeType = 8;
                goto case MotherBrainRainbowBeamAttackPhase.DrainingSamus;

            case MotherBrainRainbowBeamAttackPhase.DrainingSamus:
                RunContinuingBeamEffects(
                    samus,
                    frame.EnemyFrameCounter,
                    growth: MotherBrainRainbowBeamGrowth.Widening,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);
                DamageSamusDueToRainbowBeam(samus);
                DecrementAmmoDueToRainbowBeam(samus, frame.MainEnemyExecutionCounter);
                movement = MotherBrainRainbowBeamSamusMovement.MoveTowardMiddleOfWall(samus);

                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                    Phase = MotherBrainRainbowBeamAttackPhase.FinishFiring;
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishFiring:
                RunContinuingBeamEffects(
                    samus,
                    frame.EnemyFrameCounter,
                    growth: MotherBrainRainbowBeamGrowth.FullWidth,
                    ref soundQueued,
                    ref paletteRequested,
                    ref explosion);

                // `$A9:BA6A-$BA7E` narrows first, then compares the signed result with
                // `$0200`. Width is pinned only after it crosses below that floor.
                AngularWidth -= 384;
                if (AngularWidth < 512)
                {
                    AngularWidth = 512;
                    _movement.BeginFallingAfterRainbowBeam();
                    HdmaActive = false;
                    EarthquakeTimer = NativeCountdown.Start(0);
                    RainbowBeamSoundPlaying = false;
                    samus.Control.Unlock(); // Samus command one at `$90:F117`.
                    SamusProjectileCooldownTimer = NativeCountdown.Start(8);
                    Phase = MotherBrainRainbowBeamAttackPhase.LetSamusFall;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.LetSamusFall:
                // Controller zero changes the pose/radius, then `$A9:BACB` installs the wait
                // function and falls straight through to its first custom movement call.
                samus.Drained.LetFall(samus);
                Phase = MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;
                goto case MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand;

            case MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand:
                movement = _movement.StepFallingAfterRainbowBeam(samus);
                if (movement.Value.ReachedVerticalBoundary)
                    Phase = MotherBrainRainbowBeamAttackPhase.LowerHead;
                break;

            case MotherBrainRainbowBeamAttackPhase.LowerHead:
                // Neck words are presentation/actor-animation state. This phase's movement-
                // relevant effect is the exact function/timer handoff to `$A9:BB06`.
                Phase = MotherBrainRainbowBeamAttackPhase.DecideNextAction;
                FunctionTimer = NativeCountdown.Start(128);
                break;

            case MotherBrainRainbowBeamAttackPhase.DecideNextAction:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    if (samus.Health >= 400)
                    {
                        Phase = MotherBrainRainbowBeamAttackPhase.RepeatAttack;
                    }
                    else
                    {
                        // `$A9:BB1A-$BB24` performs this call before installing the later
                        // finish-off function. It starts one complete really-slow forward
                        // body animation when Mother Brain is standing and left of `$80`.
                        bodyWalkRequested = RequestWalkForwardReallySlow(
                            unchecked((ushort)(Body.Position.X + 16))).Requested;
                        BabyMetroidTileTransferIndex = 0;
                        BabyMetroidSpawned = false;
                        Phase = MotherBrainRainbowBeamAttackPhase.FinishSamusOff;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishSamusOff:
                // `$A9:BD45-$BD54` asks whether one more ordinary phase-two hit could put
                // Samus at the encounter's intended low-energy floor. The first nominal
                // damage is `$50`, suit-divided, multiplied by four, then padded by twenty.
                // The native BPL comparison treats equality as done and changes the body
                // function without falling into the stand-up handler on this call.
                if (CalculateFinishOffHealthThreshold(samus, 80) >= samus.Health)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.FinishStandUp;
                    break;
                }

                // Above the floor, 4000/4096 low-twelve-bit RNG values do nothing. Every
                // 32nd enemy frame that idle route may ask the posture helper to alternate
                // between standing and leaning down. This is visible body bytecode, not a
                // direct pose assignment, so retain the separate instruction-stage delay.
                if (frame.RandomNumberSeed.LowBits(12) < 4000)
                {
                    if ((frame.EnemyFrameCounter & 31) == 0)
                        bodyPostureRequested = MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
                    break;
                }

                // A weaker `$A0` nominal-hit threshold biases wounded Samus toward onion
                // rings. Only when Samus is above it does the top 16/4096 RNG band choose a
                // bomb; all remaining admitted values still choose two onion rings.
                if (CalculateFinishOffHealthThreshold(samus, 160) >= samus.Health ||
                    frame.RandomNumberSeed.LowBits(12) < 4080)
                {
                    SetHeadProgram(MotherBrainHeadProgram.TwoOnionRingsPhase2);
                }
                else
                {
                    SetHeadProgram(MotherBrainHeadProgram.BombPhase2);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinishStandUp:
                // `$A9:C670` reports carry only if pose was already zero at call entry.
                // Crouched/leaning poses request their matching stand animation and return
                // clear; walking/transition poses simply wait for their current bytecode.
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
                    FunctionTimer = NativeCountdown.Start(16);
                    goto case MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone:
                // Stand-up falls through here, so the freshly written `$10` becomes `$0F`
                // on the same AI call. BPL accepts zero and expires only after underflow.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    SetHeadProgram(MotherBrainHeadProgram.StretchingPhase2);
                    Phase = MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam;
                    FunctionTimer = NativeCountdown.Start(256);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    SetHeadProgram(MotherBrainHeadProgram.ChargingRainbowBeam);
                    Phase = MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles;
                    goto case MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles:
                // `$A9:C5BE` processes exactly one seven-byte table entry per call. The
                // fourth entry sees the following zero terminator, clears its saved pointer,
                // and returns carry set on that same call.
                spriteTileTransfer = CreateNextBabyMetroidTileTransfer();
                if (BabyMetroidTileTransferIndex == MotherBrainTileTransferDefinitions.BabyTileCount)
                {
                    RetractHead();
                    BabyMetroidSpawned = true;
                    babySpawnRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam;
                    FunctionTimer = NativeCountdown.Start(256);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    RainbowBeamPaletteAnimationIndex = 0;
                    SetHeadProgram(MotherBrainHeadProgram.FiringRainbowBeam);
                    LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                    NeckAngleDelta = 1280;
                    finalBeamSoundQueued = true; // Sound library two, effect `$71`.
                    Phase = MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding:
                // `$A9:BE14` stores the address of its own RTS. Mother Brain remains here
                // until the independently spawned Baby actor overwrites her body function
                // with `$BE38` after latching onto her head.
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback:
                // `$A9:BE38` is not executed by the Baby. It runs on Mother Brain's next
                // actor turn, changes form/neck geometry, seeds `$30`, and falls through so
                // that this first regain-balance call immediately decrements it to `$2F`.
                Body.Form = MotherBrainForm.DrainedByBabyMetroid;
                LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
                UpperNeckMovementIndex = MotherBrainNeckMovement.Raise;
                NeckAngleDelta = 1792;
                Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance;
                FunctionTimer = NativeCountdown.Start(48);
                goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance:
                paletteRequested = true;
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    // `$BE80-$BE83` contains an apparent missing STA: it loads one and then
                    // immediately reloads the neck-enable flag. Preserve that no-op rather
                    // than silently "fixing" the cartridge.
                    PainfulWalkingForward = true;
                    PainfulWalkingStage = 0;
                    PainfulWalkingAnimationDelay = 2;
                    PainfulWalkingFunctionTimer = NativeCountdown.Start(0);
                    LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam:
                // `$BE96` refreshes the shake timer only if another subsystem has cleared it.
                // The head animation owns any decrement; retaining the write gate avoids
                // fabricating a body-side countdown that does not exist in this routine.
                if (BrainMainShakeTimer.IsZero)
                    BrainMainShakeTimer = NativeCountdown.Start(50);
                paletteRequested = true;
                bodyWalkRequested = StepPainfulWalking();

                // The outer function updates delay and neck delta after *every* nested call.
                // Stage six is therefore observed on the exact call which ends its prior
                // pause, before the stage-six forward walk has even begun.
                if (PainfulWalkingStage < MotherBrainPainfulWalkingDefinitions.StageCount)
                {
                    PainfulWalkingAnimationDelay =
                        MotherBrainPainfulWalkingDefinitions.AnimationDelay(PainfulWalkingStage);
                    NeckAngleDelta = MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(PainfulWalkingStage);
                }
                if (PainfulWalkingStage == 6)
                {
                    RainbowBeamSoundPlaying = false;
                    BrainPaletteHandlingEnabled = false;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut;
                    soundQueued = true; // Sound library one, effect `$02`.
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut:
                bodyWalkRequested = StepPainfulWalking();
                if (PainfulWalkingStage >= 8)
                {
                    NeckAngleDelta = 64;
                    LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.Raise;
                    SetHeadProgram(MotherBrainHeadProgram.DyingDrool);
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom:
                // `$BF41` does not reload Y before the shared walk helper. The surrounding
                // sequence's final painful-animation selector is `$000A`, so the observable
                // retail list is the really-slow backward program at `$993A`.
                MotherBrainWalkResult moveToBack = RequestWalkBackwardReallySlow(targetX: 40);
                bodyWalkRequested = moveToBack.Requested;
                if (moveToBack.ReachedTarget)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.None;
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode:
                // The brain slot independently advances the one-way neck raises. The body
                // clears drool once both indices reach zero, before waiting for its pose.
                if (LowerNeckMovementIndex == MotherBrainNeckMovement.None &&
                    UpperNeckMovementIndex == MotherBrainNeckMovement.None)
                {
                    DroolGenerationEnabled = false;
                    if (Body.Pose != MotherBrainBodyPose.Standing)
                        break;
                    Body.SetProgram(MotherBrainBodyProgram.CrouchFast);
                    bodyPostureRequested = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey;
                    FunctionTimer = NativeCountdown.Start(64);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
                    FunctionTimer = NativeCountdown.Start(16);
                    goto case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    FunctionTimer = NativeCountdown.Start(16);

                    // Bank `$AD:EF4A` has eight nonzero palette pointers followed by zero.
                    // The caller increments its counter, passes the old index, and treats
                    // the ninth (index-eight) probe as completion without copying colours.
                    int paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex >= 8)
                    {
                        BrainHealth = 36000;
                        Phase2CorpseState = MotherBrainPhase2CorpseState.Corpse;
                        DisableSmallPurpleBreathGeneration();
                        Body.Form = MotherBrainForm.SecondPhase;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey:
                // `$C059` only installs the next function and its `$0300` timer. It does not
                // fall through, so the first pre-decrement belongs to the following call.
                Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife;
                FunctionTimer = NativeCountdown.Start(768);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife:
                // BPL accepts zero: 768 reaches zero without advancing and only the 769th
                // call underflows to `$FFFF`. Revival then re-enables both breath producers,
                // seeds `$E0`, and falls through two function installations to `$C08F`.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    EnableSmallPurpleBreathGeneration();
                    DroolGenerationEnabled = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
                    FunctionTimer = NativeCountdown.Start(224);
                    GreyTransitionCounter = 0;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    FunctionTimer = NativeCountdown.Start(16);

                    // `$AD:ED9C` reverses the same eight grey palettes used by the corpse
                    // transition and follows them with zero. The body counter is incremented
                    // before the old index is passed, so index eight is a terminating probe
                    // with no copy. The existing result flag exposes each real copy cadence.
                    int paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex >= 8)
                    {
                        BrainPaletteHandlingEnabled = true;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp;
                        goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp:
                // `$C670` returns carry only when pose zero was already visible at entry.
                // A corpse pose therefore installs `$99C6` and waits for the ordinary body
                // instruction stage to publish standing before setting the neck/timer words.
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                    NeckAngleDelta = 1280;
                    NeckMovementEnabled = true;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch;
                    FunctionTimer = NativeCountdown.Start(16);
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    SetHeadProgram(MotherBrainHeadProgram.StretchingPhase3);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid;
                    FunctionTimer = NativeCountdown.Start(128);
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid:
                // Once `$80` underflows, the timer deliberately remains negative and the
                // helper is retried after every body-program call until X has overshot `$50`.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    bodyWalkRequested = RequestWalkForward(80, 4).Requested;
                    bool reachedWalkTarget = unchecked((short)(80 - Body.Position.X)) < 0 ||
                        NativeAtLeast(Body.Position.X, 128);
                    if (reachedWalkTarget)
                    {
                        Phase2CorpseState = MotherBrainPhase2CorpseState.Revived;
                        HealthBasedPaletteHandlingEnabled = true;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath;
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath:
                // `$C11E` is a one-call setup function which immediately falls into `$C147`.
                // The separate body instruction stage may still be finishing the last walk.
                BabyMetroidAttackCounter = 0;
                NeckMovementEnabled = true;
                LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
                UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
                NeckAngleDelta = 64;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath;
                goto case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath;

            case MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath:
                if (MakeBodyStandUp(out bodyPostureRequested))
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                    bodyWalkRequested = RequestWalkForward(80, 10).Requested;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack:
                bodyPostureRequested = MaybeRequestStandUpOrLeanDown(frame.RandomNumberSeed);
                if (frame.RandomNumberSeed.IsNegative)
                {
                    // A nonzero Baby enemy index selects `$9DB1`; zero instead targets Samus.
                    // This translated cutscene owns a spawned Baby, so retain the live actor
                    // condition instead of unconditionally substituting the desired list.
                    SetHeadProgram(BabyMetroidSpawned
                        ? MotherBrainHeadProgram.AttackingBabyMetroid
                        : MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown;
                    FunctionTimer = NativeCountdown.Start(64);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack;
                break;

            case MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack:
                // This native function has no state transition. It requests stand-up and a
                // fast backward walk toward `$40` every call until the Baby overwrites it.
                MakeBodyStandUp(out bodyPostureRequested);
                bodyWalkRequested |= RequestWalkBackward(64, 4).Requested;
                break;

            case MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack:
                SetHeadProgram(MotherBrainHeadProgram.AttackingBabyMetroid);
                Phase = MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding;
                break;

            case MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding:
                // `$C1A6` is the installed RTS. The last ring volley owns the remaining
                // health reduction and the Baby actor owns its death/recovery sequence.
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance:
                // `$C1CF` changes form before installing the setup wait. Its backward-walk
                // request uses target X-14 and delay index two; the ordinary helper may
                // legitimately reject it at the arena's hard left limit.
                Body.Form = MotherBrainForm.ThirdPhase;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting;
                FunctionTimer = NativeCountdown.Start(32);
                bodyWalkRequested = RequestWalkBackward(
                    unchecked((ushort)(Body.Position.X - 14)),
                    animationDelay: 2).Requested;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting:
                // `$20` remains valid through zero and expires only when DEC wraps negative,
                // yielding 33 setup calls after the one-frame `$C1CF` producer above.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                    Phase3NeckPhase = MotherBrainPhase3NeckPhase.Normal;
                    Phase3WalkingPhase = MotherBrainPhase3WalkingPhase.TryToInchForward;

                    // `$C1F0-$C205` has no RTS after installing the three function pointers;
                    // execution falls directly into `$C209` on this same enemy AI call.
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3FightingMain:
                // `$C209` tests death before either sub-handler. It installs the translated
                // `$AEE1` explosion/fade sequence without executing that function's property
                // writes until the following body turn; no phase-two behavior leaks through.
                if (BrainHealth == 0)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom;
                    break;
                }

                StepPhase3NeckHandler();
                bodyWalkRequested = StepPhase3WalkingHandler(frame.RandomNumberSeed);

                // Walking bytecode changes pose later in the enemy instruction stage. The
                // AI therefore still sees pose zero on the call that requests a walk and may
                // also select an attack, exactly as the source's post-handler `$C21B` load.
                if (Body.Pose != MotherBrainBodyPose.Standing || Phase3DisableAttacks ||
                    !frame.RandomNumberSeed.IsNegative)
                    break;

                if (frame.RandomNumberSeed.LowByte < 128)
                {
                    SetHeadProgram(MotherBrainHeadProgram.BombPhase3);
                }
                else
                {
                    SetHeadProgram(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3);
                }

                Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown;
                FunctionTimer = NativeCountdown.Start(64);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown:
                // `$C24E` accepts timer zero and returns to main only after underflow. It
                // does not fall through, so neck and walking handlers pause for all 65 calls.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3FightingMain;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom:
                // `$A9:AEE1` makes both enemy slots non-interactive and disables the shared
                // hitboxes on every call, then reuses the ordinary medium-speed backward
                // walk producer. Carry means X `$28` has been reached/overshot; the new `$80`
                // timer falls through so the first smoky explosion and decrement happen now.
                BodyProperties = BodyProperties.With(EnemyProperties.IgnoreSamusCollision);
                BrainProperties = BrainProperties.With(EnemyProperties.IgnoreSamusCollision);
                HitboxesEnabled = false;
                MotherBrainWalkResult dyingRetreat = RequestWalkBackward(40, animationDelay: 6);
                bodyWalkRequested = dyingRetreat.Requested;
                if (dyingRetreat.ReachedTarget)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding;
                    FunctionTimer = NativeCountdown.Start(128);
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding:
                GenerateDeathExplosions(
                    style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, deathExplosions);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom:
                // `$AF21` emits smoke before asking for a really-fast forward walk to `$60`.
                // Equality still starts a complete body program; only signed overshoot or the
                // independent `$80` arena clamp reports carry and advances the death script.
                GenerateDeathExplosions(
                    style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, deathExplosions);
                MotherBrainWalkResult dyingAdvance = RequestWalkForward(96, animationDelay: 2);
                bodyWalkRequested = dyingAdvance.Requested;
                if (dyingAdvance.ReachedTarget)
                {
                    SetHeadProgram(MotherBrainHeadProgram.DyingDrool);
                    LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                    NeckAngleDelta = 1280;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects;
                    FunctionTimer = NativeCountdown.Start(32);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects:
                GenerateDeathExplosions(
                    style: MotherBrainDeathExplosionStyle.Smoky, frame.NextRandomNumber, deathExplosions);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    // `$AF5C-$AF94` removes the neck/breath/palette producers, copies sprite
                    // palette 1 into palette 7, performs one health-palette refresh, and then
                    // deliberately falls through with the already-negative timer.
                    LowerNeckMovementIndex = MotherBrainNeckMovement.None;
                    UpperNeckMovementIndex = MotherBrainNeckMovement.None;
                    DroolGenerationEnabled = false;
                    DisableSmallPurpleBreathGeneration();
                    BrainPaletteHandlingEnabled = false;
                    HealthBasedPaletteHandlingEnabled = false;
                    BrainPalette = SnesObjPalettes.Palette(7);
                    paletteRequested = true;
                    // `$AF91/$AF94` clear words $0FF0 and $0FF2 before the fade-out explosions.
                    DeathExplosionIntervalTimer = NativeCountdown.Start(0);
                    DeathAndEscapeExplosionIndex = NativeCountdown.Start(0);
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut:
                GenerateDeathExplosions(
                    style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, deathExplosions);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
                    FunctionTimer = NativeCountdown.Start(0);
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody:
                // Body flickering runs every AI call, independently of the seventeen-call
                // palette cadence. Counter values 0..15 copy blackening palettes; value 16
                // is the null terminator and completes the transition without a copy.
                BodyFlickerCallCount++;
                GenerateDeathExplosions(
                    style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, deathExplosions);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    FunctionTimer = NativeCountdown.Start(16);
                    int paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested |= paletteIndex < 16;
                    if (paletteIndex >= 16)
                    {
                        EnemyBg2TilemapClearRequested = true;
                        BodyProperties = BodyProperties
                            .With(EnemyProperties.Invisible)
                            .Without(EnemyProperties.ProcessInstructions);
                        BodyProperties2 = default;
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions;
                        FunctionTimer = NativeCountdown.Start(16);
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions:
                GenerateDeathExplosions(
                    style: MotherBrainDeathExplosionStyle.Mixed, frame.NextRandomNumber, deathExplosions);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation:
                // The body installs the decapitated list and a separate brain-slot draw
                // setup, clears its 8.8 falling velocity, then falls through immediately.
                SetHeadProgram(MotherBrainHeadProgram.Decapitated);
                BrainDrawSetupRequested = true;
                FunctionTimer = NativeCountdown.Start(0);
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround;
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround:
                // `$B12D` treats FunctionTimer as unsigned 8.8 velocity: add `$20`, use only
                // its high byte as this frame's whole-pixel displacement, and accumulate Y.
                // Crossing `$C4` clamps the head and publishes earthquake type two for 20.
                FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(FunctionTimer.RawValue + 32)));
                ushort fallingDisplacement = SnesLittleEndian.HighByte(FunctionTimer.RawValue);
                ushort candidateBrainY = unchecked((ushort)(BrainPosition.Y + fallingDisplacement));
                if (candidateBrainY >= 196)
                {
                    EarthquakeType = 2;
                    EarthquakeTimer = NativeCountdown.Start(20);
                    BrainPosition = BrainPosition.WithY(196);
                    CorpseTileTransferIndex = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles;
                    FunctionTimer = NativeCountdown.Start(256);
                }
                else
                {
                    BrainPosition = BrainPosition.WithY(candidateBrainY);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles:
                // ProcessSpriteTilesTransfers handles one record per call and notices the
                // following zero terminator on the sixth call, just like the Baby tile list.
                spriteTileTransfer = CreateNextCorpseTileTransfer();
                if (CorpseTileTransferIndex == MotherBrainCorpseArtworkDefinitions.RowCount)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey;
                    FunctionTimer = NativeCountdown.Start(32);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    GreyTransitionCounter = 0;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey;
                    FunctionTimer = NativeCountdown.Start(0);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    int paletteIndex = GreyTransitionCounter;
                    GreyTransitionCounter++;
                    paletteRequested = paletteIndex < 8;
                    if (paletteIndex < 8)
                    {
                        FunctionTimer = NativeCountdown.Start(16);
                    }
                    else
                    {
                        SetHeadProgram(MotherBrainHeadProgram.Corpse);
                        Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver;
                        FunctionTimer = NativeCountdown.Start(256);
                    }
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway;
                    BrainProperties = BrainProperties.With(EnemyProperties.IgnoreSamusCollision);
                    HitboxesEnabled = false;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway:
                // Retail initialization happens when the brain enemy slot is created, many
                // phases before death. Keep an explicit public initializer for that normal
                // route, but lazily perform the same work for focused debugger fixtures that
                // start at a late phase. Nothing touches this private table/buffer in between,
                // so the delayed call is state-equivalent while preventing a fake host setup.
                if (!CorpseRotting.IsInitialized)
                    InitializeCorpseRotting();

                MotherBrainCorpseRottingStepResult corpseRotting = CorpseRotting.Step(
                    CorpseStaging,
                    BrainPosition,
                    frame.RandomNumberSeed,
                    frame.MainEnemyExecutionCounter);
                corpseRottingVramTransfers.AddRange(corpseRotting.VramTransfers);
                corpseDustRequests.AddRange(corpseRotting.DustRequests);
                if (corpseRotting.StillRotting)
                    break;

                // Carry clear at `$B1DB` makes the brain invisible/non-solid, clears its
                // second property word, and queues stop + escape music through the ordinary
                // eight-frame-delay music queue. `$14` is loaded and immediately decremented
                // by the fallthrough into `$B211`, leaving observable timer `$13` now.
                BrainProperties = BrainProperties
                    .With(EnemyProperties.Invisible)
                    .Without(EnemyProperties.ProcessInstructions);
                BrainProperties2 = default;
                musicStopQueued = true;   // Queue music value `$0000`.
                escapeMusicQueued = true; // Queue music value `$FF24`.
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay;
                FunctionTimer = NativeCountdown.Start(20);
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay:
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    // The decapitated brain remains at the corpse coordinates for the full
                    // delay. `$B216/$B219` finally park it at (0,0) before escape-timer tile
                    // loading begins; this is a real actor-coordinate mutation, not rendering.
                    BrainPosition = Position16<RoomPixels>.Zero;
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles:
                escapeSequenceTileTransfers.Add(CreateNextEscapeTimerTileTransfer());
                if (EscapeTimerTileTransferIndex == MotherBrainEscapeTextArtworkDefinitions.TransferCount)
                {
                    // Carry set after entry six clears the shared list cursor, installs
                    // `$B26D`, and falls through far enough to emit exploded-door entry zero
                    // during this same enemy AI call.
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape;
                    goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape:
                escapeSequenceTileTransfers.Add(CreateNextExplodedDoorTileTransfer());
                if (ExplodedDoorTileTransferIndex != MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.PageCount)
                    break;

                // The second door record observes the zero terminator and performs the full
                // `$B275-$B2CD` handoff on that call: copy fourteen colors (palette zero is
                // skipped), start escape music, hold the quake indefinitely, create four
                // palette-FX objects, disable the old unpause hook, and initialize text.
                explodedDoorPaletteRequested = true; // `$A9:9534`, 14 colors -> target `$0122`.
                escapeMusicTrackQueued = true;        // Eight-frame-delay music value `$0007`.
                EarthquakeType = 5;
                EarthquakeTimer = NativeCountdown.Start(ushort.MaxValue);
                escapePaletteFxRequests.Add(PaletteFxObject.TourianShutterRedFlashing);
                escapePaletteFxRequests.Add(PaletteFxObject.TourianBackgroundRedFlashing);
                escapePaletteFxRequests.Add(PaletteFxObject.TourianGeneralLevelRedFlashing);
                escapePaletteFxRequests.Add(PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs);
                MotherBrainUnpauseHookEnabled = false;
                escapeTypewriterSetupRequested = true;
                FunctionTimer = NativeCountdown.Start(32);
                Phase = frame.AlternateEscapeText
                    ? MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle
                    : MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle:
                // `$B2D1` decrements first. BPL does *not* merely wait: it branches directly
                // into `$B2E3`, so the underlying typewriter advances during all 32..0
                // subtitle-delay values. On underflow the native code installs `$B2E3`,
                // spawns the persistent Japanese subtitle projectile, and falls through to
                // that same typewriter call. Keep the producer request separate because the
                // bank-$86 allocator owns whether a physical slot is actually available.
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (FunctionTimer.IsNegative)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;
                    timeBombSetSubtitleSpawnRequested = true;
                }
                goto case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText:
                // `$B2E3` calls the independently owned typewriter with list pointer `$2610`.
                // A request plus explicit completion input preserves that scheduler boundary:
                // the boss never invents character cadence, yet its carry-dependent handoff
                // remains directly testable and debuggable.
                if (frame.TypewriterFinished)
                {
                    Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer;
                    FunctionTimer = NativeCountdown.Start(32);
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer:
                // `$B2F9` always runs the periodic dust producer before touching the escape
                // countdown. The freshly loaded `$20` therefore receives 33 calls (32..0,
                // then `$FFFF`) and can emit dust even on the transition call.
                escapeDoorExplosion = GenerateEscapeDoorExplosion(frame.NextRandomNumber);
                FunctionTimer = FunctionTimer.Decrement().Remaining;
                if (!FunctionTimer.IsNegative)
                    break;

                // Samus command `$0F`, TimerStatus `$0002`, boss bit `$02`, and event `$0E`
                // belong to four different native subsystems. Publish every edge instead of
                // silently writing the wrong debug room's area flags inside this actor.
                timerHandlingEnableRequested = true;
                motherBrainEscapeTimerStartRequested = true;
                motherBrainBossBitRequested = true;
                zebesTimebombEventRequested = true;
                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBlowUpEscapeDoor;
                DeathExplosionIntervalTimer = NativeCountdown.Start(0);
                DeathAndEscapeExplosionIndex = NativeCountdown.Start(0);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBlowUpEscapeDoor:
                // `$B3A3` invokes the ordinary highest-free-slot allocator eight times with
                // parameters zero through seven. Allocation success is intentionally decided
                // by the shared enemy-projectile system, not assumed by the boss actor.
                for (int parameter = 0; parameter < 8; parameter++)
                    escapeDoorParticleSpawns.Add(new(parameter));

                Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing;
                escapeDoorPlm = new(
                    Block: new(0, 6),
                    PlmEntry: PlmHeaderId.MotherBrainsRoomEscapeDoor);
                break;

            case MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing:
                // EarthquakeTimer is global WRAM and is normally decremented outside enemy
                // AI. A supplied sample is therefore authoritative. `$B33C` changes only a
                // visible zero to `$FFFF`; every nonzero value, including `$FFFF`, survives.
                if (frame.GlobalEarthquakeTimer is NativeCountdown observedEarthquakeTimer)
                    EarthquakeTimer = observedEarthquakeTimer;
                if (EarthquakeTimer.IsZero)
                {
                    EarthquakeTimer = NativeCountdown.Start(ushort.MaxValue);
                    earthquakeTimerRefreshed = true;
                }
                break;

            case MotherBrainRainbowBeamAttackPhase.Inactive:
            default:
                throw new InvalidOperationException($"Unsupported rainbow-beam phase {Phase}.");
        }

        return new MotherBrainRainbowBeamAttackStepResult(
            phaseBefore,
            Phase,
            movement,
            soundQueued,
            paletteRequested,
            explosion,
            chargeSoundQueued,
            bodyWalkRequested,
            _headInstructionListRequestSerial != headInstructionListRequestSerialBefore,
            bodyPostureRequested,
            spriteTileTransfer,
            babySpawnRequested,
            finalBeamSoundQueued,
            deathExplosions,
            corpseRottingVramTransfers,
            corpseDustRequests,
            musicStopQueued,
            escapeMusicQueued,
            escapeSequenceTileTransfers,
            explodedDoorPaletteRequested,
            escapeMusicTrackQueued,
            escapePaletteFxRequests,
            escapeTypewriterSetupRequested,
            timeBombSetSubtitleSpawnRequested,
            escapeDoorExplosion,
            timerHandlingEnableRequested,
            motherBrainEscapeTimerStartRequested,
            motherBrainBossBitRequested,
            zebesTimebombEventRequested,
            escapeDoorParticleSpawns,
            escapeDoorPlm,
            earthquakeTimerRefreshed);
    }
}
