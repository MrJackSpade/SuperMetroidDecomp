using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Game;

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
    /// <summary>Gets the native $40 eye record, attached by part initialization and sharing the body's position while retaining its own animation/palette counters.</summary>
    public RoomEnemySlot Eye { get => field ?? throw new InvalidOperationException("Phantoon's eye part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Gets the native $80 tentacle record, attached by part initialization and positioned from the body after its AI pass.</summary>
    public RoomEnemySlot Tentacles { get => field ?? throw new InvalidOperationException("Phantoon's tentacle part is not attached; part initialization attaches it."); internal set; }
    /// <summary>Gets the native $C0 mouth record, attached by part initialization and also retaining the wave/blending control words.</summary>
    public RoomEnemySlot Mouth { get => field ?? throw new InvalidOperationException("Phantoon's mouth part is not attached; part initialization attaches it."); internal set; }

    /// <summary>Native enemy-BG2 tilemap byte count at WRAM <c>$7E:19F6</c>.</summary>
    public ushort Bg2TilemapSize { get; internal set; }

    /// <summary>
    /// Modeled BG2 scroll registers written after the body has moved: camera minus body plus
    /// forty whole pixels on each axis when ordinary synchronization runs.
    /// </summary>
    public Position16<Bg2Pixels> Bg2Scroll { get; internal set; }

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
    public PlmHeaderId? BossDoorPlmRequest { get; internal set; }

    /// <summary>Gets the encounter's lazily created bank-$88 wave owner, which advances scanline scroll mechanics separately from accepted-NMI display latching.</summary>
    public PhantoonWaveHdmaState Wave => field ??= new();

    /// <summary>Gets the encounter's lazily created bank-$88 blending owner, retaining live transparency control and separately latched blending/mosaic display state.</summary>
    public PhantoonBlendingState Blending => field ??= new();
}
