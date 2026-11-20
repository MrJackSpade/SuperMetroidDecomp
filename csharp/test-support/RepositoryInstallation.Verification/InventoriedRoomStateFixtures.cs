using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Synthetic room states for the room block inventory's checks (#2570): a one-screen room of an area, a state of a
/// graphics set placing some room objects and no enemies, a level showing one block and installed terrain of the given
/// reactions, inventoried through <see cref="InventoriedRoomState.Of"/> as the import inventories a retail state.
/// </summary>
internal static class InventoriedRoomStateFixtures
{
    /// <summary>The level the fixture states load.</summary>
    private static readonly RoomLevelId Level = new("InventoriedFixtureLevel");

    /// <summary>
    /// A state of <paramref name="graphicsSet"/> in a room of <paramref name="area"/> whose level shows
    /// <paramref name="placedBlock"/> alone, its installed terrain one block of each of <paramref name="terrain"/> (one block of
    /// air reacting to nothing when there are none), a Wave shot's straddled reads setting off
    /// <paramref name="straddledShots"/>, placing <paramref name="objects"/>, running <paramref name="main"/> and showing
    /// <paramref name="floor"/>; no door leads into its room.
    /// </summary>
    internal static InventoriedRoomState State(RoomGraphicsSet graphicsSet, AreaId area, int placedBlock, IReadOnlyList<RoomTerrainReaction> terrain,
        IReadOnlySet<RoomTerrainReaction> straddledShots, RoomObjectPopulation objects, RoomMainBehavior main = RoomMainBehavior.None,
        RoomTurningFloor floor = RoomTurningFloor.None)
    {
        var room = new RoomDefinition("InventoriedFixtureRoom", area, new(0, 0), new(1, 1), 0, 0, RoomDoorTransitionStyle.Standard);
        var state = new RoomStateSettings("InventoriedFixtureState", Level, graphicsSet, RoomMusic.Inherited, null,
            new RoomEnemyPopulationDefinition("InventoriedFixtureEnemies", [], 0),
            new BackgroundScrollModes(BackgroundAxisScroll.FollowsForeground, BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Placed),
            RoomScrollDefinitions.OpenWithTopAlignedBottomRow, null, main, objects, null, RoomSetupBehavior.None)
        {
            TurningFloor = floor,
        };
        var artwork = new RoomLevelArtwork(Level, new Extent16<RoomBlocks>(1, 1), [RoomBlockAppearance.Of(placedBlock)], [default]);
        IReadOnlyList<RoomTerrainReaction> blocks = terrain.Count == 0 ? [new(RoomCollisionType.Air, RoomBlockBehavior.None)] : terrain;
        var installed = new InstalledRoomLevel(new Extent16<RoomBlocks>(blocks.Count, 1),
            [.. blocks.Select(block => RoomTerrainBlock.Of(0, BlockMirroring.None, block.Collision))],
            [.. blocks.Select(block => block.Behavior)], [.. new RoomBlockAppearance[blocks.Count]], [], 0);
        return InventoriedRoomState.Of(room, state, artwork, installed, straddledShots, []);
    }

    /// <summary>A population placing no room object.</summary>
    internal static RoomObjectPopulation NoObjects { get; } = RoomObjectPopulation.FromPlacements("InventoriedFixtureObjects", []);
}
