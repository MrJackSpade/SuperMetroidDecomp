using System.Runtime.CompilerServices;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
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

    /// <summary>The repository installation's pictures of Draygon's cannon frames, read once per process.</summary>
    private static readonly Lazy<InstalledDraygonCannonFrames> DraygonCannonFrames = new(() => InstalledDraygonCannonFrames.Stock());

    /// <summary>The repository installation's pictures of Phantoon's door frames, read once per process.</summary>
    private static readonly Lazy<InstalledPhantoonDoorFrames> PhantoonDoorFrames = new(() => InstalledPhantoonDoorFrames.Stock());

    /// <summary>The repository installation's pictures of Mother Brain's glass frames, read once per process.</summary>
    private static readonly Lazy<InstalledMotherBrainGlassFrames> MotherBrainGlassFrames = new(() => InstalledMotherBrainGlassFrames.Stock());

    /// <summary>The repository installation's pictures of the escape gate's frames, read once per process.</summary>
    private static readonly Lazy<InstalledEscapeGateFrames> EscapeGateFrames = new(() => InstalledEscapeGateFrames.Stock());

    /// <summary>The repository installation's pictures of Mother Brain's fake-death room frames, read once per process.</summary>
    private static readonly Lazy<InstalledMotherBrainFakeDeathFrames> MotherBrainFakeDeathFrames = new(() => InstalledMotherBrainFakeDeathFrames.Stock());

    /// <summary>
    /// A PLM system bound to no room: families that need room services fail at that dependency. A room object shows only
    /// its installed pictures (#1557), so a fixture that names no artwork draws with the repository installation's.
    /// </summary>
    /// <param name="visuals">The installed artwork, or null for the repository installation's.</param>
    internal static RoomObjectSystem Unbound(RoomObjectVisuals? visuals = null)
    {
        visuals ??= Installed();
        visuals.RoomObjectDraygonCannonVisuals ??= DraygonCannonFrames.Value;
        visuals.RoomObjectPhantoonDoorVisuals ??= PhantoonDoorFrames.Value;
        visuals.RoomObjectMotherBrainGlassVisuals ??= MotherBrainGlassFrames.Value;
        visuals.RoomObjectEscapeGateVisuals ??= EscapeGateFrames.Value;
        visuals.RoomObjectMotherBrainFakeDeathVisuals ??= MotherBrainFakeDeathFrames.Value;
        var room = (RoomObjectRoom)RuntimeHelpers.GetUninitializedObject(typeof(RoomObjectRoom));
        var services = (RoomObjectServices)RuntimeHelpers.GetUninitializedObject(typeof(RoomObjectServices));
        var roomObjects = new RoomObjectSystem(room, services, visuals, new CollectibleBlockPairs(), new RoomObjectPlaces());
        visualsByFixture.Add(roomObjects, visuals);
        return roomObjects;
    }

    /// <summary>A new PLM system bound to <paramref name="room"/>, drawing with <paramref name="fixture"/>'s artwork, as a room load builds it.</summary>
    internal static RoomObjectSystem Bound(RoomObjectRoom room, RoomObjectServices services, RoomObjectSystem fixture)
    {
        RoomObjectVisuals visuals = VisualsOf(fixture);
        var roomObjects = new RoomObjectSystem(room, services, visuals, new CollectibleBlockPairs(), new RoomObjectPlaces());
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

    /// <summary>Every room-object family's pictures as the repository installation holds them.</summary>
    private static RoomObjectVisuals Installed() => new()
    {
        RoomObjectShotBlockVisuals = InstalledRoomObjectShotBlockVisuals.Stock(),
        RoomObjectGrappleBlockVisuals = InstalledRoomObjectGrappleBlockVisuals.Stock(),
        RoomObjectStationVisuals = InstalledRoomObjectStationVisuals.Stock(),
        RoomObjectBlueDoorVisuals = InstalledRoomObjectBlueDoorVisuals.Stock(),
        RoomObjectColoredDoorVisuals = InstalledRoomObjectColoredDoorVisuals.Stock(),
        RoomObjectGreyDoorVisuals = InstalledRoomObjectGreyDoorVisuals.Stock(),
        RoomObjectEyeDoorVisuals = InstalledRoomObjectEyeDoorVisuals.Stock(),
        RoomObjectMotherBrainGlassVisuals = MotherBrainGlassFrames.Value,
        RoomObjectNoobTubeVisuals = InstalledRoomObjectNoobTubeVisuals.Stock(),
        RoomObjectDownwardGateVisuals = InstalledRoomObjectDownwardGateVisuals.Stock(),
        RoomObjectElevatorPlatformVisuals = InstalledRoomObjectElevatorPlatformVisuals.Stock(),
        RoomObjectEscapeGateVisuals = EscapeGateFrames.Value,
        RoomObjectBombTorizoHandVisuals = InstalledRoomObjectBombTorizoHandVisuals.Stock(),
        RoomObjectDraygonCannonVisuals = DraygonCannonFrames.Value,
        RoomObjectPhantoonDoorVisuals = PhantoonDoorFrames.Value,
        RoomObjectChozoStatueVisuals = InstalledRoomObjectChozoStatueVisuals.Stock(),
        RoomObjectLinkedRestoreVisuals = InstalledRoomObjectLinkedRestoreVisuals.Stock(),
        RoomObjectTourianAccessVisuals = RoomObjectTourianAccessVisuals.Stock(),
        RoomObjectSpeedBoosterVisuals = InstalledRoomObjectSpeedBoosterVisuals.Stock(),
        RoomObjectMaridiaElevatubeVisuals = InstalledRoomObjectMaridiaElevatubeVisuals.Stock(),
        RoomObjectSporeSpawnCeilingVisuals = InstalledRoomObjectSporeSpawnCeilingVisuals.Stock(),
        RoomObjectSamusEaterVisuals = InstalledRoomObjectSamusEaterVisuals.Stock(),
        RoomObjectBotwoonWallVisuals = InstalledRoomObjectBotwoonWallVisuals.Stock(),
        RoomObjectKraidVisuals = InstalledRoomObjectKraidVisuals.Stock(),
        RoomObjectCrocomireVisuals = InstalledRoomObjectCrocomireVisuals.Stock(),
        RoomObjectMotherBrainFakeDeathVisuals = MotherBrainFakeDeathFrames.Value,
        RoomObjectCollectibleVisuals = InstalledRoomObjectCollectibleVisuals.Stock(),
        RoomObjectRevealAndWallVisuals = InstalledRoomObjectRevealAndWallVisuals.Stock(),
        RoomObjectSetupBlocks = InstalledRoomObjectSetupBlocks.Stock(),
    };

    /// <summary>The installed artwork a fixture draws with; editing it changes what the fixture draws.</summary>
    internal static RoomObjectVisuals VisualsOf(RoomObjectSystem fixture) =>
        visualsByFixture.TryGetValue(fixture, out RoomObjectVisuals? visuals)
            ? visuals
            : throw new InvalidOperationException("The PLM system was not built by RoomObjectFixtures.");
}
