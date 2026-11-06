using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// State shared by Phantoon's four consecutive enemy records: body, eye, tentacles and mouth.
/// Most gameplay values remain on those records, because the parts' behaviour reads one
/// another's records, and keeping the values per part keeps those relationships visible.
/// </summary>
public sealed class PhantoonEnemyState
{
    /// <summary>Attaches shared encounter state to the body's slot.</summary>
    internal PhantoonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Phantoon's body's and his room's colours (#1512).</summary>
    internal PhantoonColours Colours { get; } = new();

    /// <summary>Gets the body, the first of the four records and owner of function dispatch, health, motion, and shared encounter values.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>The record values of the body, eye, tentacles and mouth, in slot order.</summary>
    public PhantoonPartState[] Parts { get; } = [new(), new(), new(), new()];
    /// <summary>Gets the eye, the second record, attached by part initialization and sharing the body's position while retaining its own animation/palette counters.</summary>
    public RoomEnemySlot Eye { get => field ?? throw new InvalidOperationException("Phantoon's eye part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Gets the tentacles, the third record, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemySlot Tentacles { get => field ?? throw new InvalidOperationException("Phantoon's tentacle part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Gets the mouth, the fourth record, attached by part initialization and also retaining the wave/blending controls.</summary>
    public RoomEnemySlot Mouth { get => field ?? throw new InvalidOperationException("Phantoon's mouth part is not attached; part initialization attaches it."); internal set; }

    /// <summary>Each part's typed program cursor (#641), by its slot offset from the body: body, eye, tentacles, mouth.</summary>
    internal AnimationProgramCursor<PhantoonPartProgram>[] PartPrograms { get; } = new AnimationProgramCursor<PhantoonPartProgram>[PartCount];

    /// <summary>Phantoon's four consecutive physical records.</summary>
    private const int PartCount = 4;

    /// <summary>The program cursor of <paramref name="part"/>, one of Phantoon's four consecutive records.</summary>
    internal ref AnimationProgramCursor<PhantoonPartProgram> PartProgram(RoomEnemySlot part)
    {
        Ensure.NotNull(part);
        int offset = part.SlotIndex - Body.SlotIndex;
        if ((uint)offset >= PartCount)
            throw new InvalidOperationException($"Enemy slot {part.SlotIndex} is not one of Phantoon's four records.");
        return ref PartPrograms[offset];
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
    public PlmHeaderId? BossDoorPlmRequest { get; internal set; }

    /// <summary>Gets the encounter's lazily created wave effect, which advances the per-line ripple separately from display latching.</summary>
    public PhantoonWaveEffect Wave => field ??= new();

    /// <summary>Gets the encounter's lazily created blending owner, retaining live transparency control and separately latched blending/mosaic display state.</summary>
    public PhantoonBlendingState Blending => field ??= new();
}
