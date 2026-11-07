using System.Runtime.CompilerServices;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Standalone PLM systems for fixtures that drive PLM families without a runtime. Production builds one PLM system per
/// room with its bindings (#1329); a fixture may start unbound, as the runtime's system was between rooms, and binds to
/// a room when it loads a population there.
/// </summary>
internal static class RoomObjectFixtures
{
    /// <summary>The installed artwork each fixture was built with, so a rebound fixture draws the same way.</summary>
    private static readonly ConditionalWeakTable<RoomObjectSystem, RoomObjectVisuals> visualsByFixture = [];

    /// <summary>A PLM system bound to no room: families that need room services fail at that dependency.</summary>
    /// <param name="visuals">The installed artwork, or none.</param>
    internal static RoomObjectSystem Unbound(RoomObjectVisuals? visuals = null)
    {
        visuals ??= new RoomObjectVisuals();
        var room = (RoomObjectRoom)RuntimeHelpers.GetUninitializedObject(typeof(RoomObjectRoom));
        var services = (RoomObjectServices)RuntimeHelpers.GetUninitializedObject(typeof(RoomObjectServices));
        var roomObjects = new RoomObjectSystem(room, services, visuals);
        visualsByFixture.Add(roomObjects, visuals);
        return roomObjects;
    }

    /// <summary>A new PLM system bound to <paramref name="room"/>, drawing with <paramref name="fixture"/>'s artwork, as a room load builds it.</summary>
    internal static RoomObjectSystem Bound(RoomObjectRoom room, RoomObjectServices services, RoomObjectSystem fixture)
    {
        RoomObjectVisuals visuals = VisualsOf(fixture);
        var roomObjects = new RoomObjectSystem(room, services, visuals);
        visualsByFixture.Add(roomObjects, visuals);
        return roomObjects;
    }

    /// <summary>Binds <paramref name="fixture"/> to <paramref name="room"/> as a room load does, then loads the population.</summary>
    /// <returns>The number of records spawned.</returns>
    internal static int LoadPopulation(ref RoomObjectSystem fixture, RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services)
    {
        fixture = Bound(room, services, fixture);
        return fixture.PopulationLoader.LoadRoomPopulation(room, population, services);
    }

    /// <summary>Loads a population into a new room-bound copy of <paramref name="fixture"/> that the caller does not keep.</summary>
    /// <returns>The number of records spawned.</returns>
    internal static int LoadPopulation(RoomObjectSystem fixture, RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services) =>
        LoadPopulation(ref fixture, room, population, services);

    /// <summary>The installed artwork a fixture draws with; editing it changes what the fixture draws.</summary>
    internal static RoomObjectVisuals VisualsOf(RoomObjectSystem fixture) =>
        visualsByFixture.TryGetValue(fixture, out RoomObjectVisuals? visuals)
            ? visuals
            : throw new InvalidOperationException("The PLM system was not built by RoomObjectFixtures.");
}
