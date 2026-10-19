using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The room-owned services a PLM population installs once for every resident actor: the system state, area,
/// Samus, boss and event flags, layer-3 FX, earthquake and enemy-side effects. each room's PLM system is built
/// with them, so no slot can reach the previous room's owners.
/// </summary>
internal sealed class PlmRoomBindings
{
    /// <summary>Current room's bank-$80 shared system state, cleared with its resident PLMs.</summary>
    internal Bank80SystemState? System { get; }

    /// <summary>Area identity used by room PLMs; reset to Crateria when no room is bound.</summary>
    internal AreaId Area { get; }

    /// <summary>Progress reader for Tourian statue PLMs in the current room.</summary>
    internal ITourianStatueProgress? TourianStatue { get; }

    /// <summary>Current room's Samus access used by PLM contact and scripted movement.</summary>
    internal IRoomSamusAccess? Samus { get; }

    /// <summary>Area boss flags consulted by room PLM conditions.</summary>
    internal IAreaBossFlagReader? Bosses { get; }

    /// <summary>Global event flags consulted by room PLM conditions.</summary>
    internal IGlobalEventFlags? Events { get; }

    /// <summary>Layer-3 FX state modified by room-specific PLM effects.</summary>
    internal RoomLayer3FxState? RoomFx { get; }

    /// <summary>Room earthquake sink for PLM instructions that trigger shaking.</summary>
    internal IRoomEarthquakeWriter? Earthquake { get; }

    /// <summary>Enemy-side effects requested by PLM instructions in the bound room.</summary>
    internal IRoomPlmEnemyEffects? Enemies { get; }

    /// <summary>The room width in blocks, as the downward gate's collision search uses it.</summary>
    internal int RoomWidth { get; }

    /// <summary>Captures one loaded room's service owners and width for every PLM in its population.</summary>
    internal PlmRoomBindings(PlmRoom room, RoomPlmServices services)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(services);
        System = room.System;
        Area = room.AreaIndex;
        TourianStatue = services.TourianStatue;
        Samus = services.Samus;
        Bosses = services.Bosses;
        Events = services.Events;
        RoomFx = services.RoomFx;
        Earthquake = services.Earthquake;
        Enemies = services.Enemies;
        RoomWidth = room.Level.Size.X;
    }
}
