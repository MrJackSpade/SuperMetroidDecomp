using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Hud;

/// <summary>
/// Checks the area map's column wrap (#1832): a room whose screens run past the map's right edge has the minimap explore
/// the wrapped column at the left edge, and explored cells are saved, so the wrap is gameplay-observable.
/// </summary>
internal static class MinimapColumnWrapVerification
{
    /// <summary>Checks the wrap and the row bounds, then the minimap exploring the wrapped column.</summary>
    internal static void VerifyMinimapColumnWrap()
    {
        AssertEqual(63, AreaMapSize.WrapColumn(63), "the right edge's column stays");
        AssertEqual(0, AreaMapSize.WrapColumn(64), "one column past the right edge is the left edge's");
        AssertEqual(1, AreaMapSize.WrapColumn(65), "two columns past the right edge are the second column");
        AssertEqual(63, AreaMapSize.WrapColumn(-1), "one column before the left edge is the right edge's");
        AssertTrue(AreaMapSize.ContainsRow(0) && AreaMapSize.ContainsRow(31), "the map's top and bottom rows lie on it");
        AssertTrue(!AreaMapSize.ContainsRow(-1) && !AreaMapSize.ContainsRow(32), "rows above and below the map do not wrap");

        // A two-screen room placed at the map's last column, Samus in its second screen.
        var system = new SharedGameState(new SharedRandom());
        var hud = new HudState();
        hud.BindPresentation(InstalledFixtures.Maps.GameplayHud);
        hud.Initialize(HudSnapshot.CeresDebug);
        hud.UpdateMinimap(system, Frame(roomColumn: 63, roomRow: 5));
        AssertTrue(system.IsMapTileExplored(AreaId.Crateria, new Position16<MapTiles>(0, 6)),
            "the second screen of a room at the last column explores the left edge's column");
        AssertTrue(!system.IsMapTileExplored(AreaId.Crateria, new Position16<MapTiles>(63, 6)),
            "the room's first column stays unexplored while Samus is in its second screen");

        // The same room one row below the map's bottom explores nothing.
        var below = new SharedGameState(new SharedRandom());
        hud.UpdateMinimap(below, Frame(roomColumn: 63, roomRow: 31));
        AssertTrue(!below.Progress.ExploredCells(AreaId.Crateria).Any(), "a screen below the map explores nothing");

        Console.WriteLine("Minimap column wrap: the wrapped column is explored at the map's left edge; rows do not wrap.");
    }

    /// <summary>A minimap update with Samus in the second screen of a two-screen room at the given map place.</summary>
    private static MinimapFrame Frame(int roomColumn, int roomRow) => new()
    {
        Area = AreaId.Crateria,
        RoomMap = new Position16<MapTiles>(roomColumn, roomRow),
        RoomSize = new Extent16<RoomBlocks>(32, 16),
        Samus = new Position16<RoomPixels>(300, 100),
        FrameCounter = 0,
        PresentationMap = new DiscoverableMap(),
    };

    /// <summary>A Crateria map whose every cell can be explored, so exploration does not depend on the map's art.</summary>
    private sealed class DiscoverableMap : IAreaMapView
    {
        /// <summary>Crateria.</summary>
        public AreaId Area => AreaId.Crateria;

        /// <summary>A blank cell.</summary>
        public MapCell Cell(Position16<MapTiles> cell) => default;

        /// <summary>Every cell can be explored.</summary>
        public bool IsDiscoverable(Position16<MapTiles> cell) => true;

        /// <summary>No cell is revealed by a map station.</summary>
        public bool IsRevealedByMapStation(Position16<MapTiles> cell) => false;

        /// <summary>No cell reveals the one above it.</summary>
        public bool RevealsCellAbove(Position16<MapTiles> cell) => false;
    }
}
