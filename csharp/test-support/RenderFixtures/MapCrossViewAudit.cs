using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Map;
namespace SuperMetroid.TestSupport.RenderFixtures;
using SuperMetroid.Core;
using SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Retail-ROM regression for issue #252. The audit gives the HUD and pause screen the
/// same persistent state, room header, and Samus world coordinate, then compares every
/// visible 5x3 cell and the independently rendered pause-map marker.
/// </summary>
internal static class MapCrossViewAudit
{
    /// <summary>Checks HUD cells and pause marker positions with and without each area's downloaded map.</summary>
    public static int Run(string romPath)
    {
        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int comparisonCount = 0;
        foreach (ushort roomPointer in MapCrossViewAuditDefinitions.RepresentativeRooms)
        {
            SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            comparisonCount += VerifyRoom(room, hasAreaMap: false);
            comparisonCount += VerifyRoom(room, hasAreaMap: true);
        }

        SelectedRoom reportedRoom = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(
            bus,
            MapCrossViewAuditDefinitions.WaterRoom);
        if (reportedRoom.Identity != new RoomIdentity(AreaId.Brinstar, 0x27) ||
            reportedRoom.MapPosition.X < AreaMapPages.PageWidthInTiles)
        {
            throw new InvalidDataException(
                $"Map audit expected right-page room $01/$27, got {reportedRoom.Identity} " +
                $"at map {reportedRoom.MapPosition}.");
        }

        Console.WriteLine(
            $"Map cross-view audit passed: {comparisonCount} HUD cells and eight pause " +
            "markers agree across Crateria, Brinstar $01/$27, Norfair, and Maridia, " +
            "with and without downloaded area maps.");
        return 0;
    }

    /// <summary>Compares fifteen HUD map cells and the pause marker for one room and map-acquisition state.</summary>
    private static int VerifyRoom(
        SelectedRoom room,
        bool hasAreaMap)
    {
        // A half-screen local coordinate keeps Samus inside every representative room and
        // makes the expected map cell independent of door-entry pose or camera history.
        var samus = new SamusState(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)),
        };
        var system = new SharedGameState(new SharedRandom());
        if (hasAreaMap)
            system.SetAreaMapAcquired(room.AreaIndex);

        var hud = new HudState();
        hud.Initialize(HudSnapshot.CeresDebug);
        hud.UpdateMinimap(system, new MinimapFrame { AreaIndex = room.AreaIndex, RoomMap = room.MapPosition, RoomSize = room.Size.ToRoomBlocks(), Samus = samus.Position.Whole, FrameCounter = 8 });

        var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = room.AreaIndex, RoomMapPosition = room.MapPosition });
        _ = pause.Render();

        Position16<SpritePixels> expectedMarker = hud.MinimapCenter
            .ToMapPixels().ToScreen(pause.MapScroll).ToSpritePosition();
        if (pause.LastIndicatorOrigin != expectedMarker)
        {
            throw new InvalidDataException(
                $"Room {room.Identity} map marker mismatch: HUD center=" +
                $"({hud.MinimapCenter.X},{hud.MinimapCenter.Y}), pause origin=" +
                $"{pause.LastIndicatorOrigin}, expected={expectedMarker}.");
        }

        int comparisons = 0;
        for (int outputY = 0; outputY < 3; outputY++)
        {
            int mapY = hud.MinimapCenter.Y + outputY - 1;
            for (int outputX = 0; outputX < 5; outputX++)
            {
                int mapX = (hud.MinimapCenter.X + outputX - 2) & 0x3f;
                // Both views show the same map cell (#1513); a cell the HUD hides is the empty cell the pause map shows.
                MapCell hudCell = hud.Minimap[outputY * GameplayHudPresentation.MinimapSize.X + outputX] is MinimapMapCell shown
                    ? shown.Cell
                    : MapCell.Empty;
                MapCell pauseCell = pause.ReadDisplayedMapCell(new(checked((ushort)mapX), checked((ushort)mapY))).Cell;
                if (hudCell != pauseCell)
                {
                    throw new InvalidDataException(
                        $"Room {room.Identity} map cell ({mapX},{mapY}) disagreed " +
                        $"with area-map acquisition={hasAreaMap}: HUD={hudCell.Name}, " +
                        $"pause={pauseCell.Name}.");
                }
                comparisons++;
            }
        }
        return comparisons;
    }
}
