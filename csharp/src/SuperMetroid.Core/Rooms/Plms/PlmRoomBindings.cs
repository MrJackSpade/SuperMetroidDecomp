using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The room-owned services a PLM population installs once for every resident actor: the system state, area,
/// Samus, boss and event flags, layer-3 FX, earthquake and enemy-side effects. <see cref="RoomPlmSystem.Reset"/>
/// clears them with the population so no reused slot can reach the previous room's owners.
/// </summary>
internal sealed class PlmRoomBindings
{
    /// <summary>Current room's bank-$80 shared system state, cleared with its resident PLMs.</summary>
    internal Bank80SystemState? System { get; private set; }

    /// <summary>Area identity used by room PLMs; reset to Crateria when no room is bound.</summary>
    internal AreaId Area { get; private set; } = AreaId.Crateria;

    /// <summary>Progress reader for Tourian statue PLMs in the current room.</summary>
    internal ITourianStatueProgress? TourianStatue { get; private set; }

    /// <summary>Current room's Samus access used by PLM contact and scripted movement.</summary>
    internal IRoomSamusAccess? Samus { get; private set; }

    /// <summary>Area boss flags consulted by room PLM conditions.</summary>
    internal IAreaBossFlagReader? Bosses { get; private set; }

    /// <summary>Global event flags consulted by room PLM conditions.</summary>
    internal IGlobalEventFlags? Events { get; private set; }

    /// <summary>Layer-3 FX state modified by room-specific PLM effects.</summary>
    internal RoomLayer3FxState? RoomFx { get; private set; }

    /// <summary>Room earthquake sink for PLM instructions that trigger shaking.</summary>
    internal IRoomEarthquakeWriter? Earthquake { get; private set; }

    /// <summary>Enemy-side effects requested by PLM instructions in the bound room.</summary>
    internal IRoomPlmEnemyEffects? Enemies { get; private set; }

    /// <summary>The room width in blocks, as the downward gate's collision search uses it.</summary>
    internal int RoomWidth { get; private set; }

    /// <summary>Captures one loaded room's service owners and width for every PLM in its population.</summary>
    internal void Install(PlmRoom room, RoomPlmServices services)
    {
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

    /// <summary>Releases all prior-room owners and width before PLM slots are reused for another population.</summary>
    internal void Clear()
    {
        System = null;
        Area = AreaId.Crateria;
        TourianStatue = null;
        Samus = null;
        Bosses = null;
        Events = null;
        RoomFx = null;
        Earthquake = null;
        Enemies = null;
        RoomWidth = 0;
    }
}
