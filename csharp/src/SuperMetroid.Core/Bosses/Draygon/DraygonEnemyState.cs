using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's encounter-wide state and the values the cartridge keeps in the body's common
/// record, which are emptied with the record when the body is killed (<see cref="ClearWithRecord"/>). The four visible
/// enemy records remain physical <see cref="RoomEnemySlot"/> instances; the eye's record values live in its own
/// <see cref="DraygonEyeState"/>.
/// </summary>
public sealed class DraygonEnemyState
{
    /// <summary>Anchors the encounter-wide state to Draygon's physical body slot.</summary>
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


    /// <summary>The boss-room load disables the normal minimap and marks its four tiles.</summary>
    public bool MinimapDisabledAndBossTilesExplored { get; internal set; }

    /// <summary>
    /// The off-screen point at the left where Draygon is placed between attack rounds: the body's position when the
    /// intro delay ends. Its Y is the common reset height, which also bounds how high a swoop path climbs (compared
    /// by the wrapped signed difference) and where a round that chose another swoop restarts.
    /// </summary>
    public Position16<RoomPixels> LeftSideReset { get; internal set; }

    /// <summary>
    /// The off-screen point at the right, 672 pixels right of <see cref="LeftSideReset"/> at the same height. Only the X
    /// axis is read: a leftward swoop aims from it and a leftward goop pass starts there.
    /// </summary>
    public Position16<RoomPixels> RightSideReset { get; internal set; }

    /// <summary>
    /// Downward 8.8 acceleration a swoop path is built with, in 1/256 pixel per frame per frame. Only the Y axis is
    /// meaningful. It starts at 24, each eye hit adds 8 while the result stays below 160, and a tail-whip hit on
    /// grabbed Samus restores 24.
    /// </summary>
    public EightEightVector<RoomPixels> SwoopAcceleration { get; internal set; }

    /// <summary>
    /// How many updates the intro dance has run: zero when the encounter starts and one more after each of the dance's
    /// 1,232 updates. Each Evir takes the traced path's step this far past its own start delay, and waits while it has
    /// not reached the delay.
    /// </summary>
    public int IntroDanceStep { get; internal set; }

    /// <summary>
    /// Signed pixel offset written by the tail-whip instruction $A5:9E0A and added to the body's
    /// BG2 scroll words, displacing the body graphics without moving the enemy records.
    /// </summary>
    public Offset16<BackgroundPixels> BodyGraphicsDisplacement { get; internal set; }

    /// <summary>
    /// Where Draygon's body layer is placed (#1354): the layer position shown at the screen's top-left, derived after
    /// enemies draw from the final camera, the body position and <see cref="BodyGraphicsDisplacement"/>. It is
    /// Draygon's own, never the room's background placement.
    /// </summary>
    public Position16<BackgroundPixels> BodyLayerPlacement { get; internal set; }

    /// <summary>The most heights a swoop path may hold; building a longer one fails.</summary>
    internal const int SwoopPathCapacity = 512;

    /// <summary>
    /// The swoop path: the body's height at each entry, from the swoop's lowest point up toward the reset height,
    /// followed by the height the body started the swoop at.
    /// </summary>
    public int[] SwoopYPositions { get; } = new int[SwoopPathCapacity + 1];

    /// <summary>How many heights the swoop path was built with, not counting the starting height after them.</summary>
    public int SwoopPathEntryCount { get; internal set; }

    /// <summary>Native long facing word: zero left, one right.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>Number of physical object-$18 breath bubbles admitted during a swoop.</summary>
    public int BreathBubblesSpawned { get; internal set; }

    /// <summary>
    /// Phase of the goop pass's vertical bob, in 256ths of a turn: zero when a pass starts and one unit further each
    /// movement update, wrapping around the turn, which the bob depends on. The body's height is 384 plus the cosine
    /// of this phase times 32.
    /// </summary>
    public RotationAngle GoopYOscillationAngle { get; internal set; }

    /// <summary>Native long count initialized to sixteen when a firing pass begins.</summary>
    public WrappingCountdown GoopCounter { get; internal set; }

    /// <summary>Count of wall-turret projectiles admitted to the shared pool; disabled cannons and exhausted allocations do not increment it.</summary>
    public int WallTurretsSpawned { get; internal set; }
    /// <summary>Count of goop projectiles admitted by the facing-specific body-list producers at $A5:9F7C/$9FAE.</summary>
    public int GoopProjectilesSpawned { get; internal set; }

    /// <summary>
    /// Horizontal radius of the carry spiral in 16.16 pixels; the vertical radius is a quarter of its whole pixels.
    /// It is cleared by a grab and grows by one eighth of a pixel per spiral frame, so its whole part advances every
    /// eighth frame; the spiral ends when the whole part reaches 160.
    /// </summary>
    public SignedSixteenSixteen SpiralXRadius { get; internal set; }

    /// <summary>
    /// Native center point <c>$7E:7810/$7812</c> of the carry spiral, initialized to ($100, $180).
    /// The Y axis carries its fractional companion word: retail subtracts <c>$0000.4000</c> each
    /// spiral frame, lifting the center by one pixel every four. X has no fractional word and
    /// stays whole.
    /// </summary>
    public FixedPosition<RoomPixels> SpiralCenter { get; internal set; }

    /// <summary>
    /// Draygon's position around the carry spiral, in 256ths of a turn: three quarters when a grab starts, advanced by
    /// the whole table units of <see cref="SpiralAngleDelta"/> each spiral frame and wrapping around the turn.
    /// </summary>
    public RotationAngle SpiralAngle { get; internal set; }
    /// <summary>
    /// Angular step of the carry spiral per frame: eight table units when a grab starts, slowed by 1/256 unit each
    /// spiral frame before it is applied, so the orbit decelerates as the radius grows.
    /// </summary>
    public RotationAngle SpiralAngleDelta { get; internal set; }

    /// <summary>Native <c>$7E:7818</c> countdown for an incidental 64-frame tail whip.</summary>
    public WrappingCountdown TailWhipTimer { get; internal set; }

    /// <summary>Debugger-visible proof that the physical chase reached its grab window.</summary>
    public int SuccessfulGrabs { get; internal set; }

    /// <summary>Number of ROM tail-hit opcodes that actually damaged grabbed Samus.</summary>
    public int TailWhipHits { get; internal set; }


    /// <summary>
    /// The installed health-palette band 0-7 (native <c>$7E:781C</c> holds twice this, since the threshold table is
    /// word-addressed); null until a band is installed.
    /// </summary>
    public int? HealthPaletteBand { get; internal set; }

    /// <summary>Number of random body-list explosions admitted during the fatal animation.</summary>
    public int DeathAnimationObjectsSpawned { get; internal set; }

    /// <summary>Number of fixed-cadence smoke objects requested below the burial line.</summary>
    public int DeathSmokeObjectsSpawned { get; internal set; }

    /// <summary>True after the six table-positioned burial Evirs replace the shared pool.</summary>
    public bool DeathEvirsSpawned { get; internal set; }

    /// <summary>The library-three sound effect Draygon's programs requested this enemy frame (the dying cry), or null.</summary>
    public SoundEffectId? LastSoundLibrary3 { get; internal set; }
    /// <summary>Music command published when the dying body reaches its burial point; selects track three, or remains null before that transition.</summary>
    public MusicRequest? MusicRequest { get; internal set; }
    /// <summary>True after the terminal burial routine scatters sixteen physical pickups using the body's drop definition.</summary>
    public bool ItemDropRequested { get; internal set; }
    /// <summary>True after terminal burial invokes the persistent area-boss-defeated callback; prevents repeating that persistence write.</summary>
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>The last library-two sound effect Draygon's programs requested this enemy frame, or null.</summary>
    public SoundEffectId? LastSoundLibrary2 { get; internal set; }

    /// <summary>Set when the init routine schedules Evir tiles for VRAM $6D00.</summary>
    public bool IntroEvirGraphicsLoaded { get; internal set; }

    /// <summary>Number of physical object-$3B intro Evirs admitted to the shared pool.</summary>
    public int IntroEvirsSpawned { get; internal set; }

    /// <summary>Number of exact 64-frame turret-selection RNG draws observed.</summary>
    public int TurretCadenceChecks { get; internal set; }

    /// <summary>
    /// Exact extra-enemy WRAM words disabled by Draygon setup or a destroyed room PLM.
    /// This is the shared storage observed by bank $A5's firing selector.
    /// </summary>
    internal HashSet<DraygonWallCannon> DisabledCannons { get; } = [];

    /// <summary>Bank-$A5 body-function address; body main AI dispatches this before copying coordinates to the three parts.</summary>
    public DraygonAiFunction Function { get; internal set; }

    /// <summary>
    /// The updates the current intro stage has run: zero when the opening delay starts and when the dance starts, and
    /// one more after each of that stage's updates. The opening delay ends at 256 and the dance at its duration.
    /// </summary>
    /// <remarks>
    /// The original game keeps this count and <see cref="BurialWait"/> in one record value (#1476). Neither is read
    /// under the other's meaning: each intro stage ends by zeroing the count, and the burial wait is loaded with its
    /// full length when the dying body reaches its burial point, so they are held apart.
    /// </remarks>
    public int IntroFrames
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, DraygonIntroDanceDefinitions.DurationFrames, value);
    }

    /// <summary>The updates the dying body waits at its burial point before it sinks, counted down to zero.</summary>
    public WrappingCountdown BurialWait { get; internal set; }

    /// <summary>
    /// The swoop path entry whose height the body takes next: it counts down from <see cref="SwoopPathEntryCount"/>
    /// to zero on the descent and back up on the ascent.
    /// </summary>
    internal int SwoopEntry
    {
        get;
        set => field = Ensure.BetweenInclusive(0, SwoopPathCapacity, value);
    }

    /// <summary>The unsigned 16.16 horizontal speed the swoop and goop passes add to or subtract from the body's X.</summary>
    internal uint SwoopVelocity { get; set; }

    /// <summary>Empties the values the cartridge keeps in the body's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = DraygonAiFunction.None;
        IntroFrames = 0;
        BurialWait = WrappingCountdown.Start(0);
        SwoopEntry = 0;
        SwoopPathEntryCount = 0;
        SwoopVelocity = 0;
    }
}
