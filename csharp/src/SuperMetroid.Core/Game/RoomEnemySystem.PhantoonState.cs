namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$A7 function pointers installed in Phantoon body variable F. Keeping the
/// actual addresses makes debugger watches line up with WRAM <c>$0FB2</c> and prevents a
/// host-only state machine from quietly diverging from the cartridge dispatcher.
/// </summary>
public enum PhantoonAiFunction : ushort
{
    /// <summary>$A7:D4A9: allocates the introductory circle of eight flames from the physical projectile pool.</summary>
    SpawnStartingFlames = 0xd4a9,
    /// <summary>$A7:D4EE: waits before activating the introductory flame circle's spin.</summary>
    WaitBeforeActivatingStartingFlames = 0xd4ee,
    /// <summary>$A7:D508: waits for the introductory flame orbit to finish before wavy materialization.</summary>
    WaitForStartingFlamesToDisappear = 0xd508,
    /// <summary>$A7:D54A: advances the introductory wavy palette fade-in.</summary>
    WavyFadeIn = 0xd54a,
    /// <summary>$A7:D596: selects the first figure-eight round's movement pattern.</summary>
    PickFirstRoundPattern = 0xd596,
    /// <summary>$A7:D5E7: advances the selected figure-eight path until the eye-opening window.</summary>
    MoveInFigureEightThenOpenEye = 0xd5e7,
    /// <summary>$A7:D60D: tracks Samus with the eye during the figure-eight vulnerability window.</summary>
    EyeTracksSamus = 0xd60d,
    /// <summary>$A7:D65C: makes Phantoon opaque and initializes its triggered swoop.</summary>
    BecomeSolidAndSwoop = 0xd65c,
    /// <summary>$A7:D678: advances the opaque swoop toward Samus.</summary>
    Swooping = 0xd678,
    /// <summary>$A7:D6B9: continues the swoop while fading the body out.</summary>
    FadeOutWhileSwooping = 0xd6b9,
    /// <summary>$A7:D6D4: counts the hidden delay before another figure-eight appearance.</summary>
    WaitAfterFadeOut = 0xd6d4,
    /// <summary>$A7:D6E2: places Phantoon for the next figure-eight round.</summary>
    PickNextAppearance = 0xd6e2,
    /// <summary>$A7:D72D: materializes the next figure-eight appearance.</summary>
    FadeInBeforeFigureEight = 0xd72d,
    /// <summary>$A7:D73F: the flame-rain show entry, installing the next visible/vulnerable-window setup.</summary>
    BecomeSolidAfterFlameRain = 0xd73f,
    /// <summary>$A7:D767: advances the flame-rain materialization before entering its vulnerability window.</summary>
    FadeInDuringFlameRain = 0xd767,
    /// <summary>$A7:D788: advances the flame-rain vulnerability window before Phantoon hides again.</summary>
    TrackSamusDuringFlameRain = 0xd788,
    /// <summary>$A7:D7D5: fades out between successive flame-rain attacks.</summary>
    FadeOutDuringFlameRain = 0xd7d5,
    /// <summary>$A7:D7F7: schedules another flame rain while Phantoon is hidden.</summary>
    SpawnFlameRain = 0xd7f7,
    /// <summary>$A7:D82A: initializes the first flame rain while fading away from the preceding round.</summary>
    FadeOutBeforeFirstFlameRain = 0xd82a,
    /// <summary>$A7:D85C: fades out before the enraged attack sequence.</summary>
    FadeOutBeforeRage = 0xd85c,
    /// <summary>$A7:D874: positions hidden Phantoon at the rage anchor and waits for its appearance.</summary>
    MoveToTopCenterForRage = 0xd874,
    /// <summary>$A7:D891: fades into the enraged attack's top-center appearance.</summary>
    FadeInForRage = 0xd891,
    /// <summary>$A7:D8AC: advances the enraged flame pattern before the post-rage fade.</summary>
    Enraged = 0xd8ac,
    /// <summary>$A7:D916: fades out after the enraged attack.</summary>
    FadeOutAfterRage = 0xd916,
    /// <summary>$A7:D92E: completes the fatal-damage swoop before starting the death choreography.</summary>
    FinishFatalSwoop = 0xd92e,
    /// <summary>$A7:D948: alternates death fade-in/fade-out cycles before the explosion phase.</summary>
    DyingFadeInOut = 0xd948,
    /// <summary>$A7:D98B: schedules physical death explosions and prepares the final distortion.</summary>
    DyingExplosions = 0xd98b,
    /// <summary>$A7:DA51: initializes the final wavy HDMA and mosaic death presentation.</summary>
    BeginFinalWavyDeath = 0xda51,
    /// <summary>$A7:DA86: advances the final wavy/mosaic palette fade-out.</summary>
    DyingFadeOut = 0xda86,
    /// <summary>$A7:DAD7: clears Phantoon graphics and releases distortion before Wrecked Ship activation.</summary>
    AlmostDead = 0xdad7,
    /// <summary>$A7:DB3D: restores the powered Wrecked Ship palette, publishes drops and boss persistence, restores the door, and deletes all four parts.</summary>
    Dead = 0xdb3d,
    /// <summary>$A7:D4A8: an explicit return entry with no encounter-state mutation.</summary>
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

    /// <summary>Gets physical slot zero, the native $00 body owner of function dispatch, health, motion, and shared encounter words.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Gets the native $40 eye record, attached by part initialization and sharing the body's position while retaining its own animation/palette counters.</summary>
    public RoomEnemySlot? Eye { get; internal set; }
    /// <summary>Gets the native $80 tentacle record, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemySlot? Tentacles { get; internal set; }
    /// <summary>Gets the native $C0 mouth record, attached by part initialization and also retaining the wave/blending control words.</summary>
    public RoomEnemySlot? Mouth { get; internal set; }

    /// <summary>Native enemy-BG2 tilemap byte count at WRAM <c>$7E:19F6</c>.</summary>
    public ushort Bg2TilemapSize { get; internal set; }

    /// <summary>Modeled BG2 scroll registers written after the body has moved.</summary>
    public ushort Bg2HorizontalScroll { get; internal set; }
    /// <summary>Gets the native BG2 vertical scroll word, calculated as camera Y minus body Y plus forty whole pixels when ordinary synchronization runs.</summary>
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
    /// <summary>Gets the number of introductory flame requests that acquired a live bank-$86 projectile slot.</summary>
    public int StartingFlamesSpawned { get; internal set; }

    /// <summary>Last delayed music request issued by Phantoon's private AI.</summary>
    public MusicCommand? MusicRequest { get; internal set; }

    /// <summary>Materialization sound-table cursor and most recent library-two request.</summary>
    public ushort MaterializationSoundIndex { get; internal set; }
    /// <summary>Gets the most recent library-two materialization sound requested by the private instruction stream, cleared at the next body pass.</summary>
    public ushort? LastMaterializationSound { get; internal set; }

    /// <summary>Last library-two combat sound and exact post-vulnerability damage.</summary>
    public ushort? LastCombatSoundEffect { get; internal set; }
    /// <summary>Gets the actual health removed by the latest accepted projectile hit after native vulnerability arithmetic and lethal-health clamping.</summary>
    public ushort LastProjectileDamage { get; internal set; }
    /// <summary>Gets the count of projectile collisions accepted by the vulnerable body path, including hits whose vulnerability produces zero damage.</summary>
    public int AcceptedProjectileHits { get; internal set; }

    /// <summary>Death choreography observability: physical explosions and mosaic register.</summary>
    public int DeathExplosionRequests { get; internal set; }
    /// <summary>Gets the number of death-explosion requests that allocated a physical effect actor.</summary>
    public int DeathExplosionsSpawned { get; internal set; }
    /// <summary>Gets the live native MOSAIC register byte, retaining both the high-nibble block size and low-nibble layer enables before display latching.</summary>
    public byte MosaicRegister { get; internal set; }

    /// <summary>Final Wrecked Ship activation effects owned outside the enemy record.</summary>
    public bool MainScreenBg2Enabled { get; internal set; }
    /// <summary>Gets whether the terminal activation routine has requested the authored sixteen-pickup arena scatter.</summary>
    public bool ItemDropRequested { get; internal set; }
    /// <summary>Gets whether the terminal activation routine has published the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPersisted { get; internal set; }
    /// <summary>Gets whether the powered Wrecked Ship fade over CGRAM entries zero through 111 and the final deletion/door/music handoff have completed.</summary>
    public bool WreckedShipPowerPaletteComplete { get; internal set; }

    /// <summary>
    /// The room PLM at block (0,6) closes/opens the boss door. The PLM system is an outer
    /// owner, so the enemy publishes the exact authored request rather than editing terrain.
    /// </summary>
    public ushort? BossDoorPlmRequest { get; internal set; }
    private PhantoonWaveHdmaState? _wave;
    /// <summary>Gets the encounter's lazily created bank-$88 wave owner, which advances scanline scroll mechanics separately from accepted-NMI display latching.</summary>
    public PhantoonWaveHdmaState Wave => _wave ??= new();
    private PhantoonBlendingState? _blending;
    /// <summary>Gets the encounter's lazily created bank-$88 blending owner, retaining live transparency control and separately latched blending/mosaic display state.</summary>
    public PhantoonBlendingState Blending => _blending ??= new();
}

/// <summary>
/// One pickup request emitted by projectile instruction <c>$86:980E</c> after Samus shoots
/// a destroyable Phantoon flame. Native passes the Phantoon-eye enemy header to the common
/// drop selector; retaining both that header and its table pointer makes the otherwise
/// implicit choice explicit to the eventual pickup-system owner.
/// </summary>
public readonly record struct PhantoonFlameDropRequest();

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
