using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Debugger-facing state shared by Phantoon's four consecutive physical enemy records.
/// Most gameplay words deliberately remain on those records: native code aliases the same
/// offsets by adding enemy indexes <c>$00/$40/$80/$C0</c>, and flattening them would hide
/// exactly the relationships a decompilation is supposed to expose.
/// </summary>
public sealed class PhantoonEnemyState
{
    /// <summary>Attaches shared encounter state to the physical body slot at native index $00.</summary>
    internal PhantoonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Gets physical slot zero, the native $00 body owner of function dispatch, health, motion, and shared encounter words.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>The common-record words of the body, eye, tentacles and mouth, in slot order.</summary>
    public PhantoonPartState[] Parts { get; } = [new(), new(), new(), new()];
    /// <summary>Gets the native $40 eye record, attached by part initialization and sharing the body's position while retaining its own animation/palette counters.</summary>
    public RoomEnemySlot Eye { get => field ?? throw new InvalidOperationException("Phantoon's eye part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Gets the native $80 tentacle record, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemySlot Tentacles { get => field ?? throw new InvalidOperationException("Phantoon's tentacle part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Gets the native $C0 mouth record, attached by part initialization and also retaining the wave/blending control words.</summary>
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
    /// Modeled BG2 scroll registers written after the body has moved: camera minus body plus
    /// forty whole pixels on each axis when ordinary synchronization runs.
    /// </summary>
    public Position16<Bg2Pixels> Bg2Scroll { get; internal set; }

    /// <summary>
    /// Whether Phantoon draws translucent: the native layer-blending bit <c>$4000</c>, the only bit Phantoon sets or
    /// clears; $88:E44E tests it to select the translucent blending configuration $1A.
    /// </summary>
    public bool SemiTransparent { get; internal set; }

    /// <summary>True once the room's 2048-word enemy BG2 map has been cleared to tile $0338.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>The eight requested/allocated starting flames are counted independently.</summary>
    public int StartingFlameRequests { get; internal set; }
    /// <summary>Gets the number of introductory flame requests that acquired a live bank-$86 projectile slot.</summary>
    public int StartingFlamesSpawned { get; internal set; }

    /// <summary>Last delayed music request issued by Phantoon's private AI.</summary>
    public MusicCommand? MusicRequest { get; internal set; }

    /// <summary>Position zero through two in the repeating three-sound materialization cycle; the next materialization plays this one and advances it.</summary>
    public int MaterializationSoundIndex { get; internal set; }
    /// <summary>Gets the most recent materialization sound requested by the private instruction stream, cleared at the next body pass.</summary>
    public SoundEffectId? LastMaterializationSound { get; internal set; }

    /// <summary>Last combat sound requested this frame: a shot cry, death explosion or dying cry.</summary>
    public SoundEffectId? LastCombatSoundEffect { get; internal set; }
    /// <summary>Gets the count of projectile collisions accepted by the vulnerable body path, including hits whose vulnerability produces zero damage.</summary>
    public int AcceptedProjectileHits { get; internal set; }

    /// <summary>Death choreography observability: physical explosions and mosaic size.</summary>
    public int DeathExplosionRequests { get; internal set; }
    /// <summary>Gets the number of death-explosion requests that allocated a physical effect actor.</summary>
    public int DeathExplosionsSpawned { get; internal set; }
    /// <summary>
    /// Gets the live BG2 mosaic block size in screen pixels before display latching: one (no quantization) until
    /// the dying fade grows it one pixel every sixteenth frame up to sixteen, and one again once the body is cleared.
    /// </summary>
    public int MosaicBlockSize { get; internal set; } = 1;

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
    public PlmHeaderId? BossDoorPlmRequest { get; internal set; }

    /// <summary>Gets the encounter's lazily created wave effect, which advances the per-line ripple separately from accepted-NMI display latching.</summary>
    public PhantoonWaveEffect Wave => field ??= new();

    /// <summary>Gets the encounter's lazily created bank-$88 blending owner, retaining live transparency control and separately latched blending/mosaic display state.</summary>
    public PhantoonBlendingState Blending => field ??= new();
}
