namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A5 function pointers stored in Draygon's body <c>var_A</c>. Keeping the ROM
/// addresses as enum values makes a debugger watch directly comparable with the disassembly
/// while preventing the main dispatcher from becoming a collection of unexplained words.
/// </summary>
public enum DraygonAiFunction : ushort
{
    IntroInitialDelay = 0x871b,
    IntroDance = 0x878b,
    SwoopRightSetup = 0x87f4,
    SwoopRightDescending = 0x88b1,
    SwoopRightApex = 0x8922,
    SwoopRightAscending = 0x8951,
    SwoopLeftSetup = 0x89b3,
    SwoopLeftDescending = 0x8a00,
    SwoopLeftApex = 0x8a50,
    SwoopLeftAscending = 0x8a90,
    GoopRightSetup = 0x8b0a,
    GoopRight = 0x8b52,
    GoopRightTail = 0x8bae,
    GoopRightRecovery = 0x8c33,
    GoopLeftSetup = 0x8c8e,
    GoopLeft = 0x8cd4,
    GoopLeftTail = 0x8d30,
    GoopLeftRecovery = 0x8db2,
    TryGrabSamus = 0x8e19,
    GrabbedSamus = 0x8f10,
    CarrySamus = 0x8f1e,
    FlailWithSamus = 0x8fd6,
    TailWhipWithSamus = 0x90d4,
    FinalTailWhips = 0x9105,
    FinalTailWhipsWait = 0x9124,
    ReleaseSamus = 0x9128,
    FlyStraightUp = 0x9154,
    Dying = 0x9185,
    DyingSink = 0x9294,
    DyingFinish = 0x92ab,
}

/// <summary>
/// Draygon's encounter-wide WRAM extension. The four visible enemy records remain physical
/// <see cref="RoomEnemySlot"/> instances; only the bank-$A5 words outside those common
/// records live here. This avoids pretending that the boss is one ordinary compact actor.
/// </summary>
public sealed class DraygonEnemyState
{
    internal DraygonEnemyState(RoomEnemySlot body) => Body = body;

    public RoomEnemySlot Body { get; }
    public RoomEnemySlot? Eye { get; internal set; }
    public RoomEnemySlot? Tail { get; internal set; }
    public RoomEnemySlot? Arms { get; internal set; }

    /// <summary>Native <c>enemy_bg2_tilemap_size</c>, measured in words.</summary>
    public ushort Bg2TilemapSize { get; internal set; }

    /// <summary>True after the $1000-byte enemy BG2 surface has been filled with tile $338.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>Room-loading IRQ command selected by Draygon's body instruction stream.</summary>
    public ushort RoomLoadingIrqCommand { get; internal set; }

    /// <summary>The boss-room load disables the normal minimap and marks its four tiles.</summary>
    public bool MinimapDisabledAndBossTilesExplored { get; internal set; }

    /// <summary>Native <c>$7E:7800</c>: left-side off-screen reset X position.</summary>
    public ushort LeftSideResetXPosition { get; internal set; }

    /// <summary>Native <c>$7E:7802</c>: common reset Y position.</summary>
    public ushort ResetYPosition { get; internal set; }

    /// <summary>Native <c>$7E:7804</c>: right-side off-screen reset X position.</summary>
    public ushort RightSideResetXPosition { get; internal set; }

    /// <summary>Native <c>$7E:781E</c>: 8.8 acceleration used to construct swoop heights.</summary>
    public ushort SwoopYAcceleration { get; internal set; }

    /// <summary>Native <c>$7E:808C</c> byte index into the intro dance stream.</summary>
    public ushort FightIntroDanceIndex { get; internal set; }

    /// <summary>Native long WRAM offsets applied by the tail-whip instruction stream.</summary>
    public ushort BodyGraphicsXDisplacement { get; internal set; }
    public ushort BodyGraphicsYDisplacement { get; internal set; }

    /// <summary>
    /// The native table at $7E:9002 is written every four bytes and addressed by a byte
    /// offset. This dense view stores those meaningful words at <c>offset / 4</c>.
    /// </summary>
    public ushort[] SwoopYPositions { get; } = new ushort[0x0201];

    /// <summary>Number of generated descent samples, excluding the terminal reset-Y word.</summary>
    public int SwoopPathEntryCount { get; internal set; }

    /// <summary>Native long facing word: zero left, one right.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>Number of physical object-$18 breath bubbles admitted during a swoop.</summary>
    public int BreathBubblesSpawned { get; internal set; }

    /// <summary>Native long goop-path cosine angle, constrained to its low byte.</summary>
    public ushort GoopYOscillationAngle { get; internal set; }

    /// <summary>Native long count initialized to sixteen when a firing pass begins.</summary>
    public ushort GoopCounter { get; internal set; }

    /// <summary>
    /// Global enemy-projectile initialization parameter zero. Turret spawns publish speed
    /// three; Draygon goop deliberately inherits that word instead of initializing it.
    /// </summary>
    public ushort ProjectileSpeedParameter { get; internal set; }

    public int WallTurretsSpawned { get; internal set; }
    public int GoopProjectilesSpawned { get; internal set; }

    /// <summary>
    /// Native long WRAM <c>$7E:780C/$780E</c>. Draygon expands this 16.16 radius by
    /// exactly <c>$0000.2000</c> per spiral frame, so keeping both words is necessary to
    /// reproduce the eight-frame pixel cadence rather than rounding it into host floats.
    /// </summary>
    public ushort SpiralXRadius { get; internal set; }
    public ushort SpiralXSubradius { get; internal set; }

    /// <summary>Native center point <c>$7E:7810/$7812</c> used by the carry spiral.</summary>
    public ushort SpiralCenterX { get; internal set; }
    public ushort SpiralCenterY { get; internal set; }

    /// <summary>
    /// Fractional low word paired with <see cref="SpiralCenterY"/>. Retail subtracts
    /// <c>$0000.4000</c> each spiral frame, lifting the center by one pixel every four.
    /// </summary>
    public ushort SpiralCenterYSubposition { get; internal set; }

    /// <summary>Low-byte polar angle and 8.8 angular delta at <c>$7E:7814/$7816</c>.</summary>
    public ushort SpiralAngle { get; internal set; }
    public ushort SpiralAngleDelta { get; internal set; }

    /// <summary>Native <c>$7E:7818</c> countdown for an incidental 64-frame tail whip.</summary>
    public ushort TailWhipTimer { get; internal set; }

    /// <summary>Debugger-visible proof that the physical chase reached its grab window.</summary>
    public int SuccessfulGrabs { get; internal set; }

    /// <summary>Number of ROM tail-hit opcodes that actually damaged grabbed Samus.</summary>
    public int TailWhipHits { get; internal set; }

    /// <summary>Suit-scaled damage dealt by the most recent tail-hit instruction.</summary>
    public ushort LastTailWhipDamage { get; internal set; }

    /// <summary>
    /// Native byte offset <c>$7E:781C</c> into the eight health-palette bands. It advances
    /// 0,2,...,14 because the threshold table is word-addressed even though each palette
    /// record contains four colors.
    /// </summary>
    public ushort HealthPaletteTableByteIndex { get; internal set; }

    /// <summary>
    /// Native death-vector angle and its deliberately unused 16.16 velocity words. The
    /// cartridge computes these once in the fatal reaction, then recalculates movement each
    /// death frame; retaining them documents and exposes that observable WRAM side effect.
    /// </summary>
    public ushort DeathMovementAngle { get; internal set; }
    public ushort UnusedDeathXSpeed { get; internal set; }
    public ushort UnusedDeathXSubspeed { get; internal set; }
    public ushort UnusedDeathYSpeed { get; internal set; }
    public ushort UnusedDeathYSubspeed { get; internal set; }

    /// <summary>Number of random body-list explosions admitted during the fatal animation.</summary>
    public int DeathAnimationObjectsSpawned { get; internal set; }

    /// <summary>Number of fixed-cadence smoke objects requested below the burial line.</summary>
    public int DeathSmokeObjectsSpawned { get; internal set; }

    /// <summary>True after the six table-positioned burial Evirs replace the shared pool.</summary>
    public bool DeathEvirsSpawned { get; internal set; }

    /// <summary>Debugger-visible cartridge requests emitted by the terminal boss path.</summary>
    public ushort? LastSoundLibrary3 { get; internal set; }
    public MusicCommand? MusicRequest { get; internal set; }
    public bool ItemDropRequested { get; internal set; }
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>Most recent library-two SFX requested by Draygon's body instruction list.</summary>
    public ushort? LastSoundLibrary2 { get; internal set; }

    /// <summary>Set when the init routine schedules Evir tiles for VRAM $6D00.</summary>
    public bool IntroEvirGraphicsLoaded { get; internal set; }

    /// <summary>Number of physical object-$3B intro Evirs admitted to the shared pool.</summary>
    public int IntroEvirsSpawned { get; internal set; }

    /// <summary>Number of times the opening dance routine advanced its four actors.</summary>
    public int IntroDanceFrames { get; internal set; }

    /// <summary>Number of exact 64-frame turret-selection RNG draws observed.</summary>
    public int TurretCadenceChecks { get; internal set; }

    /// <summary>The body initializer disables turret index six through extra-enemy word $45.</summary>
    public bool BottomUnusedTurretDisabled { get; internal set; }

    public DraygonAiFunction Function
    {
        get => (DraygonAiFunction)Body.VariableA;
        internal set => Body.VariableA = (ushort)value;
    }

    public ushort FunctionTimer
    {
        get => Body.VariableB;
        internal set => Body.VariableB = value;
    }
}
