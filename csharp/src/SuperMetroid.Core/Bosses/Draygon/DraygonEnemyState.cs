using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's encounter-wide WRAM extension. The four visible enemy records remain physical
/// <see cref="RoomEnemySlot"/> instances; only the bank-$A5 words outside those common
/// records live here. This avoids pretending that the boss is one ordinary compact actor.
/// </summary>
public sealed class DraygonEnemyState
{
    /// <summary>Anchors encounter-wide extension words to Draygon's physical body slot.</summary>
    internal DraygonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Native enemy slot $0000, which owns encounter movement, health, and the body-function dispatcher.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Where each part's typed program resumes, keyed by the part's record; it replaces the native instruction pointers.</summary>
    private readonly Dictionary<RoomEnemySlot, AnimationProgramCursor<DraygonProgram>> _cursors = new(ReferenceEqualityComparer.Instance);

    /// <summary>Where <paramref name="part"/>'s program resumes.</summary>
    internal AnimationProgramCursor<DraygonProgram> Cursor(RoomEnemySlot part) =>
        _cursors.TryGetValue(part, out AnimationProgramCursor<DraygonProgram> cursor)
            ? cursor
            : throw new InvalidOperationException("A Draygon part has no program.");

    /// <summary>Sets where <paramref name="part"/>'s program resumes.</summary>
    internal void SetCursor(RoomEnemySlot part, AnimationProgramCursor<DraygonProgram> cursor) => _cursors[Ensure.NotNull(part)] = cursor;

    /// <summary>Native enemy slot $0040, installed by the eye initializer; tracks Samus using coordinates copied from the body.</summary>
    public RoomEnemySlot Eye { get => field ?? throw new InvalidOperationException("Draygon's eye part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Native enemy slot $0080, installed by the tail initializer; its instruction lists drive whips and the final release handshake.</summary>
    public RoomEnemySlot Tail { get => field ?? throw new InvalidOperationException("Draygon's tail part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Native enemy slot $00C0, installed by the arms initializer; its animation follows the body's authoritative coordinates.</summary>
    public RoomEnemySlot Arms { get => field ?? throw new InvalidOperationException("Draygon's arms part is not attached; part initialization attaches it."); internal set; }

    /// <summary>Native <c>enemy_bg2_tilemap_size</c>, measured in words.</summary>
    public ushort Bg2TilemapSize { get; internal set; }

    /// <summary>True after the $1000-byte enemy BG2 surface has been filled with tile $338.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>Room-loading IRQ command selected by Draygon's body instruction stream.</summary>
    public RoomLoadingInterruptCommand RoomLoadingIrqCommand { get; internal set; }

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

    /// <summary>
    /// Signed pixel offset written by the tail-whip instruction $A5:9E0A and added to the body's
    /// BG2 scroll words, displacing the body graphics without moving the enemy records.
    /// </summary>
    public Offset16<Bg2Pixels> BodyGraphicsDisplacement { get; internal set; }

    /// <summary>
    /// The native table at $7E:9002 is written every four bytes and addressed by a byte
    /// offset. This dense view stores those meaningful words at <c>offset / 4</c>.
    /// </summary>
    public ushort[] SwoopYPositions { get; } = new ushort[513];

    /// <summary>Number of generated descent samples, excluding the terminal reset-Y word.</summary>
    public int SwoopPathEntryCount { get; internal set; }

    /// <summary>Native long facing word: zero left, one right.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>Number of physical object-$18 breath bubbles admitted during a swoop.</summary>
    public int BreathBubblesSpawned { get; internal set; }

    /// <summary>Native long goop-path cosine angle, constrained to its low byte.</summary>
    public ushort GoopYOscillationAngle { get; internal set; }

    /// <summary>Native long count initialized to sixteen when a firing pass begins.</summary>
    public NativeCountdown GoopCounter { get; internal set; }

    /// <summary>Count of wall-turret projectiles admitted to the shared pool; disabled cannons and exhausted allocations do not increment it.</summary>
    public int WallTurretsSpawned { get; internal set; }
    /// <summary>Count of goop projectiles admitted by the facing-specific body-list producers at $A5:9F7C/$9FAE.</summary>
    public int GoopProjectilesSpawned { get; internal set; }

    /// <summary>
    /// Native long WRAM <c>$7E:780C/$780E</c>. Draygon expands this 16.16 radius by
    /// exactly <c>$0000.2000</c> per spiral frame, so keeping both words is necessary to
    /// reproduce the eight-frame pixel cadence rather than rounding it into host floats.
    /// </summary>
    public ushort SpiralXRadius { get; internal set; }
    /// <summary>Native $7E:780E fractional low word paired with <see cref="SpiralXRadius"/>; each $2000 increment accumulates one eighth of a pixel.</summary>
    public ushort SpiralXSubradius { get; internal set; }

    /// <summary>
    /// Native center point <c>$7E:7810/$7812</c> of the carry spiral, initialized to ($100, $180).
    /// The Y axis carries its fractional companion word: retail subtracts <c>$0000.4000</c> each
    /// spiral frame, lifting the center by one pixel every four. X has no fractional word and
    /// stays whole.
    /// </summary>
    public FixedPosition<RoomPixels> SpiralCenter { get; internal set; }

    /// <summary>Low-byte polar angle and 8.8 angular delta at <c>$7E:7814/$7816</c>.</summary>
    public ushort SpiralAngle { get; internal set; }
    /// <summary>Native $7E:7816 unsigned 8.8 angle step; initialized to $0800, decremented by one per spiral frame, and applied through its high byte.</summary>
    public ushort SpiralAngleDelta { get; internal set; }

    /// <summary>Native <c>$7E:7818</c> countdown for an incidental 64-frame tail whip.</summary>
    public NativeCountdown TailWhipTimer { get; internal set; }

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
    /// <summary>
    /// The 16.16 death-velocity magnitudes calculated on the fatal reaction; retained for WRAM
    /// parity but not used by drift movement, since death frames recalculate their own vector.
    /// </summary>
    public FixedVector<RoomPixels> UnusedDeathVelocity { get; internal set; }

    /// <summary>Number of random body-list explosions admitted during the fatal animation.</summary>
    public int DeathAnimationObjectsSpawned { get; internal set; }

    /// <summary>Number of fixed-cadence smoke objects requested below the burial line.</summary>
    public int DeathSmokeObjectsSpawned { get; internal set; }

    /// <summary>True after the six table-positioned burial Evirs replace the shared pool.</summary>
    public bool DeathEvirsSpawned { get; internal set; }

    /// <summary>Debugger-visible cartridge requests emitted by the terminal boss path.</summary>
    public ushort? LastSoundLibrary3 { get; internal set; }
    /// <summary>Music command published when the dying body reaches its burial point; selects track three, or remains null before that transition.</summary>
    public MusicCommand? MusicRequest { get; internal set; }
    /// <summary>True after the terminal burial routine scatters sixteen physical pickups using the body's drop definition.</summary>
    public bool ItemDropRequested { get; internal set; }
    /// <summary>True after terminal burial invokes the persistent area-boss-defeated callback; prevents repeating that persistence write.</summary>
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

    /// <summary>
    /// Exact extra-enemy WRAM words disabled by Draygon setup or a destroyed room PLM.
    /// This is the shared storage observed by bank $A5's firing selector.
    /// </summary>
    internal HashSet<DraygonCannonControlWord> DisabledCannonWords { get; } = [];

    /// <summary>Bank-$A5 body-function address stored in native <c>var_A</c>; body main AI dispatches this before copying coordinates to the three parts.</summary>
    public DraygonAiFunction Function
    {
        get => (DraygonAiFunction)Body.VariableA;
        internal set => Body.VariableA = (ushort)value;
    }

    /// <summary>Body <c>var_B</c>: intro elapsed-frame count, swoop-table byte offset advanced by four, or burial-wait countdown according to <see cref="Function"/>.</summary>
    public NativeCountdown FunctionTimer
    {
        get => NativeCountdown.FromRaw(Body.VariableB);
        internal set => Body.VariableB = value.RawValue;
    }
}
