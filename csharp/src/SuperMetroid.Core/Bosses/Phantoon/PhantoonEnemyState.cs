using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// State shared by Phantoon's four parts: body, eye, tentacles and mouth. Each part is its own enemy, attached by
/// reference when it initializes, and keeps its own typed state here, because the parts' behaviour reads one another's
/// values, and keeping the values per part keeps those relationships visible.
/// </summary>
public sealed class PhantoonEnemyState
{
    /// <summary>Attaches shared encounter state to the body's slot.</summary>
    internal PhantoonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Phantoon's body's and his room's colours (#1512).</summary>
    internal PhantoonColours Colours { get; } = new();

    /// <summary>Gets the body, owner of function dispatch, health, motion, and shared encounter values; it runs first.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>The body's own state.</summary>
    public PhantoonBodyState BodyState { get; } = new();

    /// <summary>The eye's own state.</summary>
    public PhantoonEyeState EyeState { get; } = new();

    /// <summary>The tentacles' own state.</summary>
    public PhantoonTentaclesState TentaclesState { get; } = new();

    /// <summary>The mouth's own state, which every part's initialization also writes.</summary>
    public PhantoonMouthState MouthState { get; } = new();

    /// <summary>The eye, once attached (<see cref="Eye"/>).</summary>
    private RoomEnemySlot? _eye;

    /// <summary>The tentacles, once attached (<see cref="Tentacles"/>).</summary>
    private RoomEnemySlot? _tentacles;

    /// <summary>The mouth, once attached (<see cref="Mouth"/>).</summary>
    private RoomEnemySlot? _mouth;

    /// <summary>Gets the eye, attached by part initialization and sharing the body's position while retaining its own animation/palette counters.</summary>
    public RoomEnemySlot Eye { get => _eye ?? throw new InvalidOperationException("Phantoon's eye part is not attached; part initialization attaches it."); internal set => _eye = value; }
    /// <summary>Gets the tentacles, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemySlot Tentacles { get => _tentacles ?? throw new InvalidOperationException("Phantoon's tentacle part is not attached; part initialization attaches it."); internal set => _tentacles = value; }
    /// <summary>Gets the mouth, attached by part initialization and also retaining the wave/blending controls.</summary>
    public RoomEnemySlot Mouth { get => _mouth ?? throw new InvalidOperationException("Phantoon's mouth part is not attached; part initialization attaches it."); internal set => _mouth = value; }

    /// <summary>The body's typed program cursor (#641).</summary>
    private AnimationProgramCursor<PhantoonPartProgram> _bodyProgram;

    /// <summary>The eye's typed program cursor.</summary>
    private AnimationProgramCursor<PhantoonPartProgram> _eyeProgram;

    /// <summary>The tentacles' typed program cursor.</summary>
    private AnimationProgramCursor<PhantoonPartProgram> _tentaclesProgram;

    /// <summary>The mouth's typed program cursor.</summary>
    private AnimationProgramCursor<PhantoonPartProgram> _mouthProgram;

    /// <summary>The program cursor of <paramref name="part"/>, which is Phantoon's body or one of his attached parts.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> is not one of Phantoon's parts.</exception>
    internal ref AnimationProgramCursor<PhantoonPartProgram> PartProgram(RoomEnemySlot part)
    {
        Ensure.NotNull(part);
        if (ReferenceEquals(part, Body))
            return ref _bodyProgram;
        if (ReferenceEquals(part, _eye))
            return ref _eyeProgram;
        if (ReferenceEquals(part, _tentacles))
            return ref _tentaclesProgram;
        if (ReferenceEquals(part, _mouth))
            return ref _mouthProgram;
        throw new InvalidOperationException($"Enemy {part.DefinitionId} in slot {part.SlotIndex} is not one of Phantoon's parts.");
    }

    /// <summary>
    /// Where Phantoon's body layer is placed, written after the body has moved: camera minus body plus forty whole
    /// pixels on each axis when ordinary synchronization runs. His room shows his body layer as its background layer.
    /// </summary>
    public Position16<BackgroundPixels> BodyLayerPlacement { get; internal set; }

    /// <summary>
    /// Whether Phantoon draws translucent: the layer-blending bit, the only bit Phantoon sets or
    /// clears; the blending update tests it to select the translucent blending configuration.
    /// </summary>
    public bool SemiTransparent { get; internal set; }


    /// <summary>The eight requested/allocated starting flames are counted independently.</summary>
    public int StartingFlameRequests { get; internal set; }
    /// <summary>Gets the number of introductory flame requests that acquired a live projectile slot.</summary>
    public int StartingFlamesSpawned { get; internal set; }

    /// <summary>Position zero through two in the repeating three-sound materialization cycle; the next materialization plays this one and advances it.</summary>
    public int MaterializationSoundIndex { get; internal set; }
    /// <summary>Gets the count of projectile collisions accepted by the vulnerable body path, including hits whose vulnerability produces zero damage.</summary>
    public int AcceptedProjectileHits { get; internal set; }

    /// <summary>Death choreography observability: physical explosions and mosaic size.</summary>
    public int DeathExplosionRequests { get; internal set; }
    /// <summary>Gets the number of death-explosion requests that allocated a physical effect actor.</summary>
    public int DeathExplosionsSpawned { get; internal set; }
    /// <summary>
    /// Gets the live background mosaic block size in screen pixels before display latching: one (no quantization) until
    /// the dying fade grows it one pixel every sixteenth frame up to sixteen, and one again once the body is cleared.
    /// </summary>
    public int MosaicBlockSize { get; internal set; } = 1;

    /// <summary>Gets whether the terminal activation routine has requested the authored sixteen-pickup arena scatter.</summary>
    public bool ItemDropRequested { get; internal set; }
    /// <summary>Gets whether the terminal activation routine has published the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPersisted { get; internal set; }
    /// <summary>Gets whether the powered Wrecked Ship fade over colours zero through 111 and the final deletion/door/music handoff have completed.</summary>
    public bool WreckedShipPowerPaletteComplete { get; internal set; }

    /// <summary>
    /// The room PLM at block (0,6) closes/opens the boss door. The PLM system is an outer
    /// owner, so the enemy publishes the exact authored request rather than editing terrain.
    /// </summary>
    public RoomObjectHeaderId? BossDoorObjectRequest { get; internal set; }

    /// <summary>Gets the encounter's lazily created wave effect, which advances the per-line ripple separately from display latching.</summary>
    public PhantoonWaveEffect Wave => field ??= new();

    /// <summary>Gets the encounter's lazily created blending owner, retaining live transparency control and separately latched blending/mosaic display state.</summary>
    public PhantoonBlendingState Blending => field ??= new();
}
