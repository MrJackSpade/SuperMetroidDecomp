using System.Runtime.CompilerServices;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Standalone PLM systems for fixtures that drive PLM families without a runtime. Production builds one PLM system per
/// room with its bindings (#1329); a fixture may start unbound, as the runtime's system was between rooms, and binds to
/// a room when it loads a population there.
/// </summary>
internal static class RoomObjectFixtures
{
    /// <summary>
    /// A one-screen scroll grid in power-on cells for a room-object step whose objects change no scrolls: every step hands
    /// its room objects the active room's grid, which these fixtures do not otherwise build.
    /// </summary>
    internal static RoomScrollGrid UnreadScrolls() =>
        RoomScrollGrid.CreateImplicit(RoomScrollGrid.PowerOnCells(), new Extent16<RoomScreens>(1, 1), RoomScrollState.Open);

    /// <summary>
    /// The installed artwork each fixture was built with, so a rebound fixture draws the same way, kept in one of several
    /// tables chosen by the fixture's identity: a table takes one lock per added fixture, and a sweep building a fixture
    /// per case on every core spent an eighth of its time waiting on a single table's lock (#1757).
    /// </summary>
    private static readonly ConditionalWeakTable<RoomObjectSystem, RoomObjectVisuals>[] visualsByFixture =
        [.. Enumerable.Range(0, 64).Select(_ => new ConditionalWeakTable<RoomObjectSystem, RoomObjectVisuals>())];

    /// <summary>The table that keeps <paramref name="fixture"/>'s artwork.</summary>
    private static ConditionalWeakTable<RoomObjectSystem, RoomObjectVisuals> VisualsTable(RoomObjectSystem fixture) =>
        visualsByFixture[(RuntimeHelpers.GetHashCode(fixture) & int.MaxValue) % visualsByFixture.Length];

    /// <summary>The repository installation's pictures of Draygon's cannon frames, read once per process.</summary>
    private static readonly Lazy<InstalledDraygonCannonFrames> DraygonCannonFrames = new(() => InstalledDraygonCannonFrames.Stock());

    /// <summary>The repository installation's pictures of Phantoon's door frames, read once per process.</summary>
    private static readonly Lazy<InstalledPhantoonDoorFrames> PhantoonDoorFrames = new(() => InstalledPhantoonDoorFrames.Stock());

    /// <summary>The repository installation's pictures of Mother Brain's glass frames, read once per process.</summary>
    private static readonly Lazy<InstalledMotherBrainGlassFrames> MotherBrainGlassFrames = new(() => InstalledMotherBrainGlassFrames.Stock());

    /// <summary>The repository installation's pictures of Mother Brain's fake-death room frames, read once per process.</summary>
    private static readonly Lazy<InstalledMotherBrainFakeDeathFrames> MotherBrainFakeDeathFrames = new(() => InstalledMotherBrainFakeDeathFrames.Stock());

    /// <summary>
    /// A PLM system bound to no room, drawing with the repository installation's room-object pictures: families that need
    /// room services fail at that dependency.
    /// </summary>
    internal static RoomObjectSystem Unbound() => Unbound(Installed);

    /// <summary>
    /// A PLM system bound to no room, drawing with <paramref name="artwork"/> (#2290): families that need room services fail
    /// at that dependency. A room object shows only its installed pictures (#1557), so a check that draws one family with
    /// its own pictures replaces that family's in the repository installation's whole set.
    /// </summary>
    /// <param name="artwork">Every family's installed pictures.</param>
    internal static RoomObjectSystem Unbound(RoomObjectArtwork artwork)
    {
        var visuals = new RoomObjectVisuals(artwork);
        var room = (RoomObjectRoom)RuntimeHelpers.GetUninitializedObject(typeof(RoomObjectRoom));
        var services = (RoomObjectServices)RuntimeHelpers.GetUninitializedObject(typeof(RoomObjectServices));
        var roomObjects = new RoomObjectSystem(room, services, visuals, new CollectibleBlockPairs(), new RoomObjectPlaces(), ShotCapClosing(),
            new RoomObjectOutputs(GameplayAudioRequests.Standalone(), UnspawnedThrows));
        VisualsTable(roomObjects).Add(roomObjects, visuals);
        return roomObjects;
    }

    /// <summary>
    /// Where a fixture's thrown projectiles would spawn: a fixture has no enemy projectiles, so a thrown projectile that
    /// spawns fails there; checks read the step's thrown requests instead.
    /// </summary>
    private static readonly RoomObjectThrownProjectileSpawners UnspawnedThrows = new(new NoEnemyProjectiles(), new NoEnemyProjectiles());

    /// <summary>A fixture's absent enemy projectiles, which fail any thrown projectile's spawn.</summary>
    private sealed class NoEnemyProjectiles : SuperMetroid.Core.Bosses.MotherBrain.IMotherBrainGlassShardSpawner,
        SuperMetroid.Core.Bosses.Torizo.IBombTorizoStatueFragmentSpawner
    {
        /// <inheritdoc/>
        public void SpawnMotherBrainGlassProjectile(SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassProjectileRequest request) =>
            throw new InvalidOperationException("A fixture's room objects spawned a glass shard with no enemy projectiles.");

        /// <inheritdoc/>
        public void SpawnBombTorizoStatueBreakingProjectile(SuperMetroid.Core.Bosses.Torizo.BombTorizoStatueProjectileRequest request) =>
            throw new InvalidOperationException("A fixture's room objects spawned a statue fragment with no enemy projectiles.");
    }

    /// <summary>The shot cap's closing rule the room loader composes each room's objects with (#1906).</summary>
    private static ShotOpenedCapClosing<LockedDoor> ShotCapClosing() =>
        new(LockedDoorDefinitions.LandingSiteGreenDoorFacingLeft);

    /// <summary>A new PLM system bound to <paramref name="room"/>, drawing with <paramref name="fixture"/>'s artwork, as a room load builds it.</summary>
    internal static RoomObjectSystem Bound(RoomObjectRoom room, RoomObjectServices services, RoomObjectSystem fixture)
    {
        RoomObjectVisuals visuals = VisualsOf(fixture);
        var roomObjects = new RoomObjectSystem(room, services, visuals, new CollectibleBlockPairs(), new RoomObjectPlaces(), ShotCapClosing(),
            new RoomObjectOutputs(GameplayAudioRequests.Standalone(), UnspawnedThrows));
        VisualsTable(roomObjects).Add(roomObjects, visuals);
        return roomObjects;
    }

    /// <summary>Binds <paramref name="fixture"/> to <paramref name="room"/> as a room load does, then loads the population.</summary>
    internal static void LoadPopulation(ref RoomObjectSystem fixture, RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services)
    {
        fixture = Bound(room, services, fixture);
        fixture.PopulationLoader.LoadRoomPopulation(room, population, services);
    }

    /// <summary>Loads a population into a new room-bound copy of <paramref name="fixture"/> that the caller does not keep.</summary>
    internal static void LoadPopulation(RoomObjectSystem fixture, RoomObjectRoom room, RoomObjectPopulation population, RoomObjectServices services) =>
        LoadPopulation(ref fixture, room, population, services);

    /// <summary>Every room-object family's pictures as the repository installation holds them, read once per process.</summary>
    internal static RoomObjectArtwork Installed => InstalledArtwork.Value;

    /// <summary>The repository installation's room-object pictures, read once per process.</summary>
    private static readonly Lazy<RoomObjectArtwork> InstalledArtwork = new(() => new RoomObjectArtwork
    {
        RoomObjectShotBlockVisuals = RoomObjectPicturesStock.ShotBlock,
        RoomObjectGrappleBlockVisuals = RoomObjectPicturesStock.GrappleBlock,
        RoomObjectStationVisuals = RoomObjectPicturesStock.Station,
        RoomObjectBlueDoorVisuals = RoomObjectPicturesStock.BlueDoor,
        RoomObjectColoredDoorVisuals = RoomObjectPicturesStock.ColoredDoor,
        RoomObjectGreyDoorVisuals = RoomObjectPicturesStock.GreyDoor,
        RoomObjectEyeDoorVisuals = RoomObjectPicturesStock.EyeDoor,
        RoomObjectMotherBrainGlassVisuals = MotherBrainGlassFrames.Value,
        RoomObjectNoobTubeVisuals = RoomObjectPicturesStock.NoobTube,
        RoomObjectDownwardGateVisuals = RoomObjectPicturesStock.DownwardGate,
        RoomObjectElevatorPlatformVisuals = RoomObjectPicturesStock.ElevatorPlatform,
        RoomObjectEscapeGateVisuals = RoomObjectPicturesStock.EscapeGate,
        RoomObjectBombTorizoHandVisuals = RoomObjectPicturesStock.BombTorizoHand,
        RoomObjectDraygonCannonVisuals = DraygonCannonFrames.Value,
        RoomObjectPhantoonDoorVisuals = PhantoonDoorFrames.Value,
        RoomObjectChozoStatueVisuals = RoomObjectPicturesStock.ChozoStatue,
        RoomObjectLinkedRestoreVisuals = RoomObjectPicturesStock.LinkedRestore,
        RoomObjectTourianAccessVisuals = RoomObjectTourianAccessVisuals.Stock(),
        RoomObjectSpeedBoosterVisuals = RoomObjectPicturesStock.SpeedBooster,
        RoomObjectMaridiaElevatubeVisuals = RoomObjectPicturesStock.MaridiaElevatube,
        RoomObjectSporeSpawnCeilingVisuals = RoomObjectPicturesStock.SporeSpawnCeiling,
        RoomObjectSamusEaterVisuals = RoomObjectPicturesStock.SamusEater,
        RoomObjectBotwoonWallVisuals = RoomObjectPicturesStock.BotwoonWall,
        RoomObjectKraidVisuals = RoomObjectPicturesStock.Kraid,
        RoomObjectCrocomireVisuals = RoomObjectPicturesStock.Crocomire,
        RoomObjectMotherBrainFakeDeathVisuals = MotherBrainFakeDeathFrames.Value,
        RoomObjectCollectibleVisuals = RoomObjectPicturesStock.Collectible,
        RoomObjectRevealAndWallVisuals = RoomObjectPicturesStock.RevealAndWall,
        RoomObjectSetupBlocks = InstalledRoomObjectSetupBlocks.Stock(),
    });

    /// <summary>The installed artwork a fixture draws with; editing it changes what the fixture draws.</summary>
    internal static RoomObjectVisuals VisualsOf(RoomObjectSystem fixture) =>
        VisualsTable(fixture).TryGetValue(fixture, out RoomObjectVisuals? visuals)
            ? visuals
            : throw new InvalidOperationException("The PLM system was not built by RoomObjectFixtures.");
}
