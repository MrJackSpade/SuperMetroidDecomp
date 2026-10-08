namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A7 function pointers used during Kraid's room lockout and rise. Values are retained
/// verbatim so a debugger can compare this C# state with native <c>$0FA8/$7800</c> words.
/// Later combat/death functions will be added here as their handlers are translated.
/// </summary>
public enum KraidAiFunction : ushort
{
    /// <summary>$A7:804B RTS: an explicit no-op function retaining the part slot.</summary>
    NoOperation = 0x804b,
    /// <summary>$A7:B831: the inactive lint entry before projectile production begins.</summary>
    LintInactive = 0xb831,
    /// <summary>$A7:B832: produces the lint and aligns its starting position to Kraid.</summary>
    LintProduce = 0xb832,
    /// <summary>$A7:B868: advances the lint's charge preparation before firing.</summary>
    LintCharge = 0xb868,
    /// <summary>$A7:B89B: moves the fired lint away from Kraid.</summary>
    LintFire = 0xb89b,
    /// <summary>$A7:B923: horizontally aligns the current physical part to Kraid's body.</summary>
    AlignPartToKraid = 0xb923,
    /// <summary>$A7:B92D: decrements the current part's function timer and installs its saved next function on expiry.</summary>
    HandleFunctionTimer = 0xb92d,
    /// <summary>$A7:B93F: the first-phase foot's start-retreat timer entry.</summary>
    DecrementFunctionTimerAndStartWalk = 0xb93f,
    /// <summary>$A7:B960: the first-phase foot's thinking entry before its independently timed lunge.</summary>
    FootFirstPhaseThinking = 0xb960,
    /// <summary>$A7:B965: processes Kraid's private head instruction stream and its timer.</summary>
    ProcessHeadInstructionAndTimer = 0xb965,
    /// <summary>$A7:BA2E: counts the second-phase foot's think delay and selects a walking target.</summary>
    FootSecondPhaseThinking = 0xba2e,
    /// <summary>$A7:BB45: the native backward-walk entry moving the body toward its rightward target.</summary>
    FootSecondPhaseWalkingRight = 0xbb45,
    /// <summary>$A7:BB6E: walks to the second-phase starting point before its setup delay.</summary>
    FootSecondPhaseWalkToStart = 0xbb6e,
    /// <summary>$A7:BBA4: initializes second-phase foot thinking after the starting-point walk.</summary>
    FootSecondPhaseInitialize = 0xbba4,
    /// <summary>$A7:BBAE: the native forward-walk entry moving the body toward its leftward target.</summary>
    FootSecondPhaseWalkingLeft = 0xbbae,
    /// <summary>$A7:B907: waits until the top lint's X position reaches at least $100 before firing a nail.</summary>
    FingernailWaitForLint = 0xb907,
    /// <summary>$A7:BD60: selects the fingernail's starting side, animation, and firing transition.</summary>
    FingernailInitialize = 0xbd60,
    /// <summary>$A7:BE8E: advances the fingernail's independent firing instruction stream.</summary>
    FingernailFire = 0xbe8e,
    /// <summary>$A7:BF2D: prepares the first-phase foot's forward lunge.</summary>
    FootPrepareFirstPhaseLunge = 0xbf2d,
    /// <summary>$A7:BF5D: advances the first-phase forward lunge before retreat.</summary>
    FootFirstPhaseLunge = 0xbf5d,
    /// <summary>$A7:BFAB: retreats from the first-phase lunge and restores the foot's thinking cycle.</summary>
    FootFirstPhaseRetreat = 0xbfab,

    /// <summary>$A7:C865: restricts Samus to the room's first screen while waiting for the rise.</summary>
    RestrictSamusToFirstScreen = 0xc865,
    /// <summary>$A7:C86B: installs the top BG2 tilemap half at the start of Kraid's rise.</summary>
    RaiseKraidThroughFloor = 0xc86b,
    /// <summary>$A7:C89A: installs the bottom BG2 half, starts the quake, and requests boss music.</summary>
    RaiseLoadBottomTilemap = 0xc89a,
    /// <summary>$A7:C8E0: requests rise rocks on sixteen-frame timer boundaries during the first pre-rise wait.</summary>
    RaiseRocksEvery16Frames = 0xc8e0,
    /// <summary>$A7:C902: requests rise rocks on eight-frame timer boundaries during the faster pre-rise wait.</summary>
    RaiseRocksEvery8Frames = 0xc902,
    /// <summary>$A7:C924: raises Kraid at half a pixel per update with horizontal shaking until first-phase combat starts.</summary>
    RaiseBody = 0xc924,
    /// <summary>$A7:AEA4: the first-phase body thinker counting down to its next mouth attack.</summary>
    MainloopThinking = 0xaea4,
    /// <summary>$A7:AEE4: handles the shot/open-mouth reaction and pending mouth-reopen count.</summary>
    MouthOpenReaction = 0xaee4,
    /// <summary>$A7:B6BF: initializes the collision-driven eye glow before another mouth opening.</summary>
    InitializeEyeGlow = 0xb6bf,
    /// <summary>$A7:B6D7: advances the glowing-eye palette sequence.</summary>
    GlowEye = 0xb6d7,
    /// <summary>$A7:B73D: restores the eye palette after its glow sequence.</summary>
    UnglowEye = 0xb73d,
    /// <summary>$A7:BBEA: runs the open-mouth head stream, spat-rock cadence, and reopen continuation.</summary>
    MainAttackWithMouthOpen = 0xbbea,
    /// <summary>$A7:AC4D: breaks ceiling platforms into their authored room mutations as Kraid grows.</summary>
    GrowBreakCeilingPlatforms = 0xac4d,
    /// <summary>$A7:AD3A: sets Kraid's private BG2 tilemap priority bits during growth.</summary>
    GrowSetBg2Priority = 0xad3a,
    /// <summary>$A7:AD61: completes the growth sequence's BG2 tilemap updates.</summary>
    GrowFinishBg2Update = 0xad61,
    /// <summary>$A7:AD8E: installs the room background behind the enlarged Kraid.</summary>
    GrowDrawRoomBackground = 0xad8e,
    /// <summary>$A7:AE23: fades the room background into the enlarged encounter.</summary>
    GrowFadeInRoomBackground = 0xae23,
    /// <summary>$A7:AEC4: the enlarged body's second-phase thinking entry.</summary>
    SecondPhaseThinking = 0xaec4,
    /// <summary>$A7:C0A1: releases the room camera restrictions for the enlarged second phase.</summary>
    GrowReleaseCamera = 0xc0a1,
    /// <summary>$A7:C360: initializes Kraid's zero-health sequence and the authored part shutdowns.</summary>
    DeathInitialize = 0xc360,
    /// <summary>$A7:C3F9: fades the room background out while continuing the private head stream.</summary>
    DeathFadeOut = 0xc3f9,
    /// <summary>$A7:C4A4: updates the top BG2 half and parks the lint parts for sinking.</summary>
    DeathUpdateTopTilemap = 0xc4a4,
    /// <summary>$A7:C4C8: updates the bottom BG2 half and starts the death quake and sinking sound timer.</summary>
    DeathUpdateBottomTilemap = 0xc4c8,
    /// <summary>$A7:C537: sinks Kraid through the floor while processing authored debris, sounds, and drops.</summary>
    DeathSink = 0xc537,
    /// <summary>$A7:C715: clears the top private BG2 half before ordinary room background restoration.</summary>
    DeathClearTopTilemap = 0xc715,
    /// <summary>$A7:C751: clears the bottom private BG2 half before BG3 graphics restoration.</summary>
    DeathClearBottomTilemap = 0xc751,
    /// <summary>$A7:C777: queues the first of four standard BG3 graphics transfers.</summary>
    DeathLoadBg3Quarter1 = 0xc777,
    /// <summary>$A7:C7A3: queues the second standard BG3 graphics transfer.</summary>
    DeathLoadBg3Quarter2 = 0xc7a3,
    /// <summary>$A7:C7C9: queues the third standard BG3 graphics transfer.</summary>
    DeathLoadBg3Quarter3 = 0xc7c9,
    /// <summary>$A7:C7EF: queues the fourth standard BG3 graphics transfer before the background fade-in.</summary>
    DeathLoadBg3Quarter4 = 0xc7ef,
    /// <summary>$A7:C815: fades the ordinary background in, requests room music, and publishes boss persistence when newly defeated.</summary>
    DeathFadeInBackground = 0xc815,
    /// <summary>$A7:C843: the terminal death entry for a Kraid that was alive when this encounter loaded.</summary>
    DeathFinishedWasAlive = 0xc843,
    /// <summary>$A7:C851: the already-defeated terminal entry, also invalidating BG1 column-streaming position.</summary>
    DeathFinishedWasDead = 0xc851,
}

/// <summary>
/// A sound request emitted by Kraid's private bank-$A7 logic. The library number matters:
/// the roar uses library two, while spat rocks use library three despite sharing the same
/// room-enemy scheduler.
/// </summary>
/// <param name="SoundEffect">The native sound identifier together with its library, retained for the outer audio queue.</param>
public readonly record struct KraidSoundRequest(
    SoundEffectId SoundEffect);

/// <summary>
/// Per-physical-slot projection of Kraid's bank-$7E extended workspace. Native code obtains
/// these words by adding the enemy's byte index to the shared `$7800` base; keeping one
/// typed record per slot exposes the same aliasing without flattening it into mystery fields.
/// </summary>
public sealed class KraidPartState
{
    /// <summary>
    /// Native `$7E:7800 + slot` aliases a function pointer with the second-phase foot's
    /// think timer. The raw word remains visible; the typed view is used only on functions.
    /// </summary>
    public ushort NextWord { get; internal set; }

    /// <summary>Gets the typed function view of the shared next-word alias when the current part treats it as a bank-$A7 pointer.</summary>
    public KraidAiFunction NextFunction
    {
        get => (KraidAiFunction)NextWord;
        internal set => NextWord = (ushort)value;
    }

    /// <summary>
    /// Fingernail side-selection word stored in the otherwise shared health-threshold area.
    /// It alternates body-side and fixed-left spawns when the sampled RNG chooses that path.
    /// </summary>
    public ushort AlternateSpawnFlag { get; internal set; }
}

/// <summary>
/// Shared Kraid encounter state at WRAM <c>$7E:7800</c>, plus the room-level effects whose
/// native owners sit outside an individual enemy record. No host-only combat simplification
/// is represented here: thresholds and function pointers are copied from retail arithmetic.
/// </summary>
public sealed class KraidEnemyState
{
    /// <summary>Gets eight native-order part workspaces: body, arm, three lints, foot, and two fingernails; they remain owned by the room after the body is deleted.</summary>
    public KraidPartState[] Parts { get; } = Enumerable.Range(0, 8)
        .Select(_ => new KraidPartState())
        .ToArray();

    /// <summary>Gets the retained native word at $7E:7802; mouth-reopen continuations write two at $A7:AF24/$BC58 without an established broader meaning.</summary>
    public ushort Unknown2 { get; internal set; }
    /// <summary>Gets the body think countdown selected from the native random-delay table and consumed before the next mouth attack.</summary>
    public ushort ThinkingTimer { get; internal set; }
    /// <summary>Gets the minimum whole-pixel world Y allowed when Kraid's body ejects Samus; growth changes the floor-relative clamp from 324 to 164.</summary>
    public ushort MinimumYPositionForEjection { get; internal set; }
    /// <summary>Gets the native mouth-reopen word: low bits retain collision/reopen state and the high byte counts pending reopen continuations.</summary>
    public ushort MouthFlags { get; internal set; }
    /// <summary>Health captured by the living initializer before combat can change it.</summary>
    public ushort InitialHealth { get; internal set; }
    /// <summary>Returns the cartridge threshold for the selected eighth of the initializer's captured health.</summary>
    /// <param name="index">A zero-based table index from zero through seven, selecting one through eight eighths.</param>
    /// <returns>The initial health truncated to one eighth, multiplied by the selected number of eighths.</returns>
    public ushort HealthEighthThreshold(int index) => KraidHealthThresholdDefinitions.Eighth(InitialHealth, index);
    /// <summary>Gets the second-phase whole-pixel walking target; the native unused move-right instruction also reuses this word as a countdown.</summary>
    public ushort TargetX { get; internal set; }
    /// <summary>Gets the hurt-palette countdown phase, normally started at six by a vulnerable mouth hit and decremented by its frame timer.</summary>
    public ushort HurtFrame { get; internal set; }
    /// <summary>Gets the countdown pacing hurt-palette phases, normally reloaded to two and reused by background fades during death.</summary>
    public ushort HurtFrameTimer { get; internal set; }
    /// <summary>Gets the bank-$A7 tilemap offset selected by the current private eight-byte head instruction.</summary>
    public ushort CurrentHeadTilemap { get; internal set; }
    /// <summary>Gets the native bank-$A7 mouth-hitbox record offset accepting damaging shots for the current head frame.</summary>
    public ushort VulnerableMouthHitbox { get; internal set; }
    /// <summary>Gets the native bank-$A7 mouth-hitbox record offset used for nondamaging shot reactions in the current head frame.</summary>
    public ushort InvulnerableMouthHitbox { get; internal set; }

    /// <summary>
    /// Native $7E:2000-$2FFF combined BG2 tilemap. Keeping this surface separate from
    /// VRAM is essential because rise, growth, head animation, and sinking upload only
    /// selected portions of it on their authored frames.
    /// </summary>
    internal ushort[] BackgroundTilemapWords { get; } =
        new ushort[KraidBackgroundRomData.WorkingTilemapWords];

    /// <summary>True after `$A7:AAC6` prepared Kraid's two decompressed BG2 tilemaps.</summary>
    public bool BackgroundTilemapsPrepared { get; internal set; }

    /// <summary>
    /// True while native BG2SC=$43 makes Kraid's private 64x64 map the renderer's BG2.
    /// Death restores the ordinary room map at $4800 and relinquishes this ownership.
    /// </summary>
    public bool OwnsBg2Tilemap { get; internal set; }

    /// <summary>Live BG2HOFS calculated by Kraid main AI from camera and body X.</summary>
    public ushort Bg2HorizontalScroll { get; internal set; }

    /// <summary>Live BG2VOFS calculated by Kraid main AI from camera and body Y.</summary>
    public ushort Bg2VerticalScroll { get; internal set; }

    /// <summary>Native BG2 top/bottom upload requests issued during the rise sequence.</summary>
    public int TopTilemapUploadCount { get; internal set; }
    /// <summary>Gets the number of authored bottom-half BG2 tilemap uploads issued by rise, growth, and death logic.</summary>
    public int BottomTilemapUploadCount { get; internal set; }

    /// <summary>Number of `$A7:C995` rock/quake spawn requests made by retail cadence.</summary>
    public int RiseRockSpawnRequestCount { get; internal set; }

    /// <summary>Rise-rock requests that acquired a real bank-$86 projectile slot.</summary>
    public int SpawnedRiseRockCount { get; internal set; }

    /// <summary>BG2 head entries installed by the private eight-byte instruction stream.</summary>
    public int HeadTilemapUploadCount { get; internal set; }

    /// <summary>Retail `$BC0A` spat-rock requests issued while head tilemap three is active.</summary>
    public int SpitRockRequestCount { get; internal set; }

    /// <summary>Spat-rock requests that acquired a real bank-$86 projectile slot.</summary>
    public int SpawnedSpitRockCount { get; internal set; }

    /// <summary>Number of private `$AF94` roar opcodes consumed from the head stream.</summary>
    public int RoarRequestCount { get; internal set; }

    /// <summary>Last eight-frame-delayed music request made by the rise sequence.</summary>
    public MusicCommand? MusicRequest { get; internal set; }
    /// <summary>Gets the number of growth-sequence ceiling rocks that acquired a live bank-$86 projectile slot.</summary>
    public int CeilingRockSpawnCount { get; internal set; }

    /// <summary>
    /// Set when <c>$A7:C85E</c> stores $FFFF to <c>PreviousLayer1XBlock</c>; the runtime
    /// applies it before this frame's background column streaming.
    /// </summary>
    public bool Layer1XBlockResetRequested { get; internal set; }
    /// <summary>Gets whether growth has installed the priority bits in Kraid's private BG2 tilemap.</summary>
    public bool Bg2PriorityBitsSet { get; internal set; }
    /// <summary>Gets whether growth has relinquished the first-screen camera lock for the enlarged encounter.</summary>
    public bool CameraReleasedForSecondPhase { get; internal set; }
    /// <summary>Gets the sinking sound countdown, initially forty-three and reloaded to thirty after each request.</summary>
    public ushort DeathSoundTimer { get; internal set; }
    /// <summary>Gets the number of authored sinking-table events consumed during the death descent.</summary>
    public int SinkTableEventCount { get; internal set; }
    /// <summary>Gets the number of pickup actors requested by Kraid's authored death scatter, independent of pool allocation success.</summary>
    public int DeathDropRequestCount { get; internal set; }
    /// <summary>Gets the number of standard BG3 graphics quarters queued during ordinary-background restoration.</summary>
    public int DeathBg3TransferCount { get; internal set; }
    /// <summary>Gets whether this encounter's death handoff newly wrote the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPersisted { get; internal set; }
    /// <summary>Gets whether background restoration and the death music/persistence handoff have reached the terminal death entry.</summary>
    public bool DeathSequenceComplete { get; internal set; }
}

public sealed partial class RoomEnemySystem
{
    internal const ushort KraidDefinition = 0xe2bf;
    internal const ushort KraidArmDefinition = 0xe2ff;
    internal const ushort KraidTopLintDefinition = 0xe33f;
    internal const ushort KraidMiddleLintDefinition = 0xe37f;
    internal const ushort KraidBottomLintDefinition = 0xe3bf;
    internal const ushort KraidFootDefinition = 0xe3ff;
    internal const ushort KraidGoodNailDefinition = 0xe43f;
    internal const ushort KraidBadNailDefinition = 0xe47f;

    private KraidEnemyState? _kraidState;
    private readonly List<KraidPlmRequest> _kraidPlmRequests = [];

    /// <summary>Active typed state when the loaded room owns retail Kraid slot zero.</summary>
    public KraidEnemyState? Kraid => _kraidState;

    /// <summary>Hardcoded Kraid room mutations published during the current enemy frame.</summary>
    public IReadOnlyList<KraidPlmRequest> KraidPlmRequests => _kraidPlmRequests;

    private void ResetKraidRoomState()
    {
        _kraidState = null;
        _kraidPlmRequests.Clear();
    }

    /// <summary>
    /// Kraid's room state, created by the body's initializer and kept for the room. Parts
    /// read slot zero's words directly, so they keep running after the dead-room body is
    /// deleted, as the fingernails do.
    /// </summary>
    private KraidEnemyState RequireKraidState(RoomEnemySlot slot)
    {
        if (_kraidState is null)
        {
            throw new InvalidOperationException(
                $"Enemy ${slot.EnemyDefinitionPointer:X4} requires Kraid's body to have initialized this room.");
        }
        return _kraidState;
    }
}
