namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A7 function pointers used during Kraid's room lockout and rise. Values are retained
/// verbatim so a debugger can compare this C# state with native <c>$0FA8/$7800</c> words.
/// Later combat/death functions will be added here as their handlers are translated.
/// </summary>
public enum KraidAiFunction : ushort
{
    NoOperation = 0x804b,
    LintInactive = 0xb831,
    LintProduce = 0xb832,
    LintCharge = 0xb868,
    LintFire = 0xb89b,
    AlignPartToKraid = 0xb923,
    HandleFunctionTimer = 0xb92d,
    DecrementFunctionTimerAndStartWalk = 0xb93f,
    FootFirstPhaseThinking = 0xb960,
    ProcessHeadInstructionAndTimer = 0xb965,
    FootSecondPhaseThinking = 0xba2e,
    FootSecondPhaseWalkingRight = 0xbb45,
    FootSecondPhaseWalkToStart = 0xbb6e,
    FootSecondPhaseInitialize = 0xbba4,
    FootSecondPhaseWalkingLeft = 0xbbae,
    FingernailWaitForLint = 0xb907,
    FingernailInitialize = 0xbd60,
    FingernailFire = 0xbe8e,
    FootPrepareFirstPhaseLunge = 0xbf2d,
    FootFirstPhaseLunge = 0xbf5d,
    FootFirstPhaseRetreat = 0xbfab,

    RestrictSamusToFirstScreen = 0xc865,
    RaiseKraidThroughFloor = 0xc86b,
    RaiseLoadBottomTilemap = 0xc89a,
    RaiseRocksEvery16Frames = 0xc8e0,
    RaiseRocksEvery8Frames = 0xc902,
    RaiseBody = 0xc924,
    MainloopThinking = 0xaea4,
    MouthOpenReaction = 0xaee4,
    InitializeEyeGlow = 0xb6bf,
    GlowEye = 0xb6d7,
    UnglowEye = 0xb73d,
    MainAttackWithMouthOpen = 0xbbea,
    GrowBreakCeilingPlatforms = 0xac4d,
    GrowSetBg2Priority = 0xad3a,
    GrowFinishBg2Update = 0xad61,
    GrowDrawRoomBackground = 0xad8e,
    GrowFadeInRoomBackground = 0xae23,
    SecondPhaseThinking = 0xaec4,
    GrowReleaseCamera = 0xc0a1,
    DeathInitialize = 0xc360,
    DeathFadeOut = 0xc3f9,
    DeathUpdateTopTilemap = 0xc4a4,
    DeathUpdateBottomTilemap = 0xc4c8,
    DeathSink = 0xc537,
    DeathClearTopTilemap = 0xc715,
    DeathClearBottomTilemap = 0xc751,
    DeathLoadBg3Quarter1 = 0xc777,
    DeathLoadBg3Quarter2 = 0xc7a3,
    DeathLoadBg3Quarter3 = 0xc7c9,
    DeathLoadBg3Quarter4 = 0xc7ef,
    DeathFadeInBackground = 0xc815,
    DeathFinishedWasAlive = 0xc843,
    DeathFinishedWasDead = 0xc851,
}

/// <summary>
/// A sound request emitted by Kraid's private bank-$A7 logic. The library number matters:
/// the roar uses library two, while spat rocks use library three despite sharing the same
/// room-enemy scheduler.
/// </summary>
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
    public KraidPartState[] Parts { get; } = Enumerable.Range(0, 8)
        .Select(_ => new KraidPartState())
        .ToArray();

    public ushort Unknown2 { get; internal set; }
    public ushort Unknown4 { get; internal set; }
    public ushort ThinkingTimer { get; internal set; }
    public ushort MinimumYPositionForEjection { get; internal set; }
    public ushort MouthFlags { get; internal set; }
    public ushort[] HealthEighthThresholds { get; } = new ushort[8];
    public ushort TargetX { get; internal set; }
    public ushort[] HealthQuarterThresholds { get; } = new ushort[4];
    public ushort HurtFrame { get; internal set; }
    public ushort HurtFrameTimer { get; internal set; }
    public ushort CurrentHeadTilemap { get; internal set; }
    public ushort VulnerableMouthHitbox { get; internal set; }
    public ushort InvulnerableMouthHitbox { get; internal set; }

    /// <summary>True after `$A7:AAC6` prepared Kraid's two decompressed BG2 tilemaps.</summary>
    public bool BackgroundTilemapsPrepared { get; internal set; }

    /// <summary>Native BG2 top/bottom upload requests issued during the rise sequence.</summary>
    public int TopTilemapUploadCount { get; internal set; }
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
    public ushort RoomBackgroundFadeStep { get; internal set; }
    public int CeilingRockSpawnCount { get; internal set; }
    public bool Bg2PriorityBitsSet { get; internal set; }
    public bool CameraReleasedForSecondPhase { get; internal set; }
    public ushort DeathSoundTimer { get; internal set; }
    public int SinkTableEventCount { get; internal set; }
    public int DeathDropRequestCount { get; internal set; }
    public int DeathBg3TransferCount { get; internal set; }
    public bool BossDefeatPersisted { get; internal set; }
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

    /// <summary>Active typed state when the loaded room owns retail Kraid slot zero.</summary>
    public KraidEnemyState? Kraid => _kraidState;

    private void ResetKraidRoomState() => _kraidState = null;

    private KraidEnemyState RequireKraidState(RoomEnemySlot slot)
    {
        if (_kraidState is null || _slots[0].EnemyDefinitionPointer != KraidDefinition)
        {
            throw new InvalidOperationException(
                $"Enemy ${slot.EnemyDefinitionPointer:X4} requires an initialized Kraid body in slot zero.");
        }
        return _kraidState;
    }

    private static bool IsKraidPartDefinition(ushort definition) => definition is
        KraidDefinition or
        KraidArmDefinition or
        KraidTopLintDefinition or
        KraidMiddleLintDefinition or
        KraidBottomLintDefinition or
        KraidFootDefinition or
        KraidGoodNailDefinition or
        KraidBadNailDefinition;
}
