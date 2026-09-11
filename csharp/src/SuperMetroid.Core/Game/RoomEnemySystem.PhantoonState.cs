namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$A7 function pointers installed in Phantoon body variable F. Keeping the
/// actual addresses makes debugger watches line up with WRAM <c>$0FB2</c> and prevents a
/// host-only state machine from quietly diverging from the cartridge dispatcher.
/// </summary>
public enum PhantoonAiFunction : ushort
{
    SpawnStartingFlames = 0xd4a9,
    WaitBeforeActivatingStartingFlames = 0xd4ee,
    WaitForStartingFlamesToDisappear = 0xd508,
    WavyFadeIn = 0xd54a,
    PickFirstRoundPattern = 0xd596,
    MoveInFigureEightThenOpenEye = 0xd5e7,
    EyeTracksSamus = 0xd60d,
    BecomeSolidAndSwoop = 0xd65c,
    Swooping = 0xd678,
    FadeOutWhileSwooping = 0xd6b9,
    WaitAfterFadeOut = 0xd6d4,
    PickNextAppearance = 0xd6e2,
    FadeInBeforeFigureEight = 0xd72d,
    BecomeSolidAfterFlameRain = 0xd73f,
    FadeInDuringFlameRain = 0xd767,
    TrackSamusDuringFlameRain = 0xd788,
    FadeOutDuringFlameRain = 0xd7d5,
    SpawnFlameRain = 0xd7f7,
    FadeOutBeforeFirstFlameRain = 0xd82a,
    FadeOutBeforeRage = 0xd85c,
    MoveToTopCenterForRage = 0xd874,
    FadeInForRage = 0xd891,
    Enraged = 0xd8ac,
    FadeOutAfterRage = 0xd916,
    FinishFatalSwoop = 0xd92e,
    DyingFadeInOut = 0xd948,
    DyingExplosions = 0xd98b,
    BeginFinalWavyDeath = 0xda51,
    DyingFadeOut = 0xda86,
    AlmostDead = 0xdad7,
    Dead = 0xdb3d,
    NoOperation = 0xd4a8,
}

/// <summary>
/// Debugger-facing state shared by Phantoon's four consecutive physical enemy records.
/// Most gameplay words deliberately remain on those records: native code aliases the same
/// offsets by adding enemy indexes <c>$00/$40/$80/$C0</c>, and flattening them would hide
/// exactly the relationships a decompilation is supposed to expose.
/// </summary>
public sealed class PhantoonEnemyState
{
    internal PhantoonEnemyState(RoomEnemySlot body) => Body = body;

    public RoomEnemySlot Body { get; }
    public RoomEnemySlot? Eye { get; internal set; }
    public RoomEnemySlot? Tentacles { get; internal set; }
    public RoomEnemySlot? Mouth { get; internal set; }

    /// <summary>Native enemy-BG2 tilemap byte count at WRAM <c>$7E:19F6</c>.</summary>
    public ushort Bg2TilemapSize { get; internal set; }

    /// <summary>Modeled BG2 scroll registers written after the body has moved.</summary>
    public ushort Bg2HorizontalScroll { get; internal set; }
    public ushort Bg2VerticalScroll { get; internal set; }

    /// <summary>
    /// Layer-blending bit <c>$4000</c>. Native HDMA consumes it to choose Phantoon's
    /// translucent materialization mode; retaining the word also exposes every transition.
    /// </summary>
    public ushort SemiTransparencyLayerFlags { get; internal set; }

    /// <summary>True once the room's 2048-word enemy BG2 map has been cleared to tile $0338.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>The eight requested/allocated starting flames are counted independently.</summary>
    public int StartingFlameRequests { get; internal set; }
    public int StartingFlamesSpawned { get; internal set; }

    /// <summary>Last delayed music request issued by Phantoon's private AI.</summary>
    public MusicCommand? MusicRequest { get; internal set; }

    /// <summary>Materialization sound-table cursor and most recent library-two request.</summary>
    public ushort MaterializationSoundIndex { get; internal set; }
    public ushort? LastMaterializationSound { get; internal set; }

    /// <summary>Last library-two combat sound and exact post-vulnerability damage.</summary>
    public ushort? LastCombatSoundEffect { get; internal set; }
    public ushort LastProjectileDamage { get; internal set; }
    public int AcceptedProjectileHits { get; internal set; }

    /// <summary>Death choreography observability: physical explosions and mosaic register.</summary>
    public int DeathExplosionRequests { get; internal set; }
    public int DeathExplosionsSpawned { get; internal set; }
    public byte MosaicRegister { get; internal set; }

    /// <summary>Final Wrecked Ship activation effects owned outside the enemy record.</summary>
    public bool MainScreenBg2Enabled { get; internal set; }
    public bool ItemDropRequested { get; internal set; }
    public bool BossDefeatPersisted { get; internal set; }
    public bool WreckedShipPowerPaletteComplete { get; internal set; }

    /// <summary>
    /// The room PLM at block (0,6) closes/opens the boss door. The PLM system is an outer
    /// owner, so the enemy publishes the exact authored request rather than editing terrain.
    /// </summary>
    public ushort? BossDoorPlmRequest { get; internal set; }
    private PhantoonWaveHdmaState? _wave;
    public PhantoonWaveHdmaState Wave => _wave ??= new();
}

/// <summary>
/// One pickup request emitted by projectile instruction <c>$86:980E</c> after Samus shoots
/// a destroyable Phantoon flame. Native passes the Phantoon-eye enemy header to the common
/// drop selector; retaining both that header and its table pointer makes the otherwise
/// implicit choice explicit to the eventual pickup-system owner.
/// </summary>
public readonly record struct PhantoonFlameDropRequest(
    ushort X,
    ushort Y,
    ushort EnemyDefinitionPointer,
    ushort ItemDropChancesPointer);

public sealed partial class RoomEnemySystem
{
    internal const ushort PhantoonBodyDefinition = 0xe4bf;
    internal const ushort PhantoonEyeDefinition = 0xe4ff;
    internal const ushort PhantoonTentaclesDefinition = 0xe53f;
    internal const ushort PhantoonMouthDefinition = 0xe57f;

    private PhantoonEnemyState? _phantoonState;
    private readonly List<PhantoonFlameDropRequest> _phantoonFlameDropRequests = new();

    /// <summary>Active four-slot encounter state when retail Phantoon occupies slot zero.</summary>
    public PhantoonEnemyState? Phantoon => _phantoonState;

    /// <summary>Pickup requests emitted by shot destroyable flames in this room load.</summary>
    public IReadOnlyList<PhantoonFlameDropRequest> PhantoonFlameDropRequests =>
        _phantoonFlameDropRequests;

    private void ResetPhantoonRoomState()
    {
        _phantoonState = null;
        _phantoonFlameDropRequests.Clear();
    }

    private PhantoonEnemyState RequirePhantoonState(RoomEnemySlot slot)
    {
        if (_phantoonState is null ||
            _slots[0].EnemyDefinitionPointer != PhantoonBodyDefinition)
        {
            throw new InvalidOperationException(
                $"Enemy ${slot.EnemyDefinitionPointer:X4} requires Phantoon's retail body in slot zero.");
        }
        return _phantoonState;
    }

    private static bool IsPhantoonPartDefinition(ushort definition) => definition is
        PhantoonBodyDefinition or PhantoonEyeDefinition or
        PhantoonTentaclesDefinition or PhantoonMouthDefinition;
}
