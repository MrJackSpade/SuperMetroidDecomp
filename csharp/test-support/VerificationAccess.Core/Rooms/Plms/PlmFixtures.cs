using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Standalone PLM systems for fixtures that drive PLM families without a runtime. Production builds one PLM system per
/// room with its bindings (#1329); a fixture may start unbound, as the runtime's system was between rooms, and binds to
/// a room when it loads a population there.
/// </summary>
internal static class PlmFixtures
{
    /// <summary>The installed artwork each fixture was built with, so a rebound fixture draws the same way.</summary>
    private static readonly ConditionalWeakTable<RoomPlmSystem, PlmShotBlockVisuals> visualsByFixture = [];

    /// <summary>A PLM system bound to no room: families that need room services fail at that dependency.</summary>
    /// <param name="visuals">The installed artwork, or none.</param>
    internal static RoomPlmSystem Unbound(PlmShotBlockVisuals? visuals = null)
    {
        visuals ??= new PlmShotBlockVisuals();
        var room = (PlmRoom)RuntimeHelpers.GetUninitializedObject(typeof(PlmRoom));
        var services = (RoomPlmServices)RuntimeHelpers.GetUninitializedObject(typeof(RoomPlmServices));
        var plms = new RoomPlmSystem(room, services, visuals);
        visualsByFixture.Add(plms, visuals);
        return plms;
    }

    /// <summary>A new PLM system bound to <paramref name="room"/>, drawing with <paramref name="fixture"/>'s artwork, as a room load builds it.</summary>
    internal static RoomPlmSystem Bound(PlmRoom room, RoomPlmServices services, RoomPlmSystem fixture)
    {
        PlmShotBlockVisuals visuals = VisualsOf(fixture);
        var plms = new RoomPlmSystem(room, services, visuals);
        visualsByFixture.Add(plms, visuals);
        return plms;
    }

    /// <summary>Binds <paramref name="fixture"/> to <paramref name="room"/> as a room load does, then loads the population.</summary>
    /// <returns>The number of records spawned.</returns>
    internal static int LoadPopulation(ref RoomPlmSystem fixture, PlmRoom room, RoomPlmPopulationDefinition population, RoomPlmServices services)
    {
        fixture = Bound(room, services, fixture);
        return fixture.PopulationLoader.LoadRoomPopulation(room, population, services);
    }

    /// <summary>Loads a population into a new room-bound copy of <paramref name="fixture"/> that the caller does not keep.</summary>
    /// <returns>The number of records spawned.</returns>
    internal static int LoadPopulation(RoomPlmSystem fixture, PlmRoom room, RoomPlmPopulationDefinition population, RoomPlmServices services) =>
        LoadPopulation(ref fixture, room, population, services);

    /// <summary>The installed artwork a fixture draws with; editing it changes what the fixture draws.</summary>
    internal static PlmShotBlockVisuals VisualsOf(RoomPlmSystem fixture) =>
        visualsByFixture.TryGetValue(fixture, out PlmShotBlockVisuals? visuals)
            ? visuals
            : throw new InvalidOperationException("The PLM system was not built by PlmFixtures.");
}
