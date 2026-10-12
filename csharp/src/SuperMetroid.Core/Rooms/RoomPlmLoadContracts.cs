using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The room a PLM population is installed into (#633): the address space PLM programs read, the
/// level and BG1 streamer they edit, VRAM, the system state that owns their bits, and the area.
/// </summary>
public sealed record PlmRoom
{
    /// <summary>Creates the room binding; every member is required.</summary>
    public PlmRoom(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, SnesVram vram,
        Bank80SystemState system, AreaId areaIndex)
    {
        Bus = Ensure.NotNull(bus);
        Level = Ensure.NotNull(level);
        Streamer = Ensure.NotNull(streamer);
        Vram = Ensure.NotNull(vram);
        System = Ensure.NotNull(system);
        AreaIndex = areaIndex;
    }

    /// <summary>Address space PLM programs and definitions read.</summary>
    public ISnesAddressSpace Bus { get; }

    /// <summary>Room collision and tilemap data PLMs edit.</summary>
    public RoomLevelData Level { get; }

    /// <summary>BG1 streamer PLM tile writes go through.</summary>
    public BackgroundTilemapStreamer Streamer { get; }

    /// <summary>VRAM PLM graphics load into.</summary>
    public SnesVram Vram { get; }

    /// <summary>System state owning item, door and event bits.</summary>
    public Bank80SystemState System { get; }

    /// <summary>The room's area.</summary>
    public AreaId AreaIndex { get; }
}

/// <summary>Samus as room PLMs see her (#633): absent until the room's Samus exists.</summary>
public interface IRoomSamusAccess
{
    /// <summary>The current Samus, or none.</summary>
    SamusState? Current { get; }
}

/// <summary>Tourian entrance statue progress that grey-door conditions read (#633).</summary>
public interface ITourianStatueProgress
{
    /// <summary>Whether the entrance statue sequence has finished.</summary>
    bool IsFinished { get; }
}

/// <summary>The room-shake writes PLMs make (#633).</summary>
public interface IRoomEarthquakeWriter
{
    /// <summary>Writes the earthquake timer.</summary>
    void SetTimer(ushort timer);

    /// <summary>Writes the earthquake type.</summary>
    void SetType(ushort type);
}

/// <summary>The N00b tube's shard projectiles (#633).</summary>
public interface INoobTubeProjectileSpawner
{
    /// <summary>Spawns one N00b tube shard projectile.</summary>
    void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request);
}

/// <summary>Eye-door projectiles (#633).</summary>
public interface IEyeDoorProjectileSpawner
{
    /// <summary>Spawns one eye-door projectile.</summary>
    void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request);
}

/// <summary>The old Tourian escape-shaft wall's explosion (#633).</summary>
public interface IOldTourianWallExplosion
{
    /// <summary>Spawns the old Tourian escape-shaft wall explosion.</summary>
    void SpawnOldTourianEscapeShaftWallExplosion();
}

/// <summary>Draygon cannon control (#633).</summary>
public interface IDraygonCannonControl
{
    /// <summary>Disables the Draygon cannon whose enemy-control word is at <paramref name="variablePointer"/>.</summary>
    void DisableDraygonCannon(ushort variablePointer);
}

/// <summary>
/// Every enemy-side effect room PLMs trigger (#633): the union of the PLM families' narrow
/// contracts; each family keeps only its own.
/// </summary>
public interface IRoomPlmEnemyEffects :
    INoobTubeProjectileSpawner, IEyeDoorProjectileSpawner, IOldTourianWallExplosion, IDraygonCannonControl;

/// <summary>
/// The room services resident PLMs use after load (#633). Samus and the area's boss flags are
/// required. Every other service is an explicit optional capability: the PLM family that uses it
/// checks for it where it previously checked its delegate, and a family whose program needs a missing
/// service fails loudly.
/// </summary>
public sealed record RoomPlmServices
{
    /// <summary>Samus as PLMs see her.</summary>
    public required IRoomSamusAccess Samus { get; init; }

    /// <summary>The area's boss, mini-boss and Torizo defeat bits.</summary>
    public required IAreaBossFlagReader Bosses { get; init; }

    /// <summary>Global event bits PLMs read and set.</summary>
    public IGlobalEventFlags? Events { get; init; }

    /// <summary>Tourian entrance statue progress for grey-door conditions.</summary>
    public ITourianStatueProgress? TourianStatue { get; init; }

    /// <summary>Layer-3 FX the Speed Booster escape and N00b tube drive.</summary>
    public RoomLayer3FxState? RoomFx { get; init; }

    /// <summary>Room-shake writes.</summary>
    public IRoomEarthquakeWriter? Earthquake { get; init; }

    /// <summary>Enemy-side effects.</summary>
    public IRoomPlmEnemyEffects? Enemies { get; init; }
}
