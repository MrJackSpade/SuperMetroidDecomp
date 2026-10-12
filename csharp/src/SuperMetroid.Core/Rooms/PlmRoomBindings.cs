using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The room-owned services a PLM population installs once for every resident actor: the system state, area,
/// Samus, boss and event flags, layer-3 FX, earthquake and enemy-side effects. <see cref="RoomPlmSystem.Reset"/>
/// clears them with the population so no reused slot can reach the previous room's owners.
/// </summary>
internal sealed class PlmRoomBindings
{
    /// <summary>Gets or sets system.</summary>
    internal Bank80SystemState? System { get; private set; }

    /// <summary>Gets or sets area.</summary>
    internal AreaId Area { get; private set; } = AreaId.Crateria;

    /// <summary>Gets or sets tourian statue.</summary>
    internal ITourianStatueProgress? TourianStatue { get; private set; }

    /// <summary>Gets or sets samus.</summary>
    internal IRoomSamusAccess? Samus { get; private set; }

    /// <summary>Gets or sets bosses.</summary>
    internal IAreaBossFlagReader? Bosses { get; private set; }

    /// <summary>Gets or sets events.</summary>
    internal IGlobalEventFlags? Events { get; private set; }

    /// <summary>Gets or sets room fx.</summary>
    internal RoomLayer3FxState? RoomFx { get; private set; }

    /// <summary>Gets or sets earthquake.</summary>
    internal IRoomEarthquakeWriter? Earthquake { get; private set; }

    /// <summary>Gets or sets enemies.</summary>
    internal IRoomPlmEnemyEffects? Enemies { get; private set; }

    /// <summary>The room width in blocks, as the downward gate's collision search uses it.</summary>
    internal int RoomWidth { get; private set; }

    /// <summary>install.</summary>
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

    /// <summary>clear.</summary>
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
