using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
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
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int comparisonCount = 0;
        foreach (ushort roomPointer in MapCrossViewAuditDefinitions.RepresentativeRooms)
        {
            CartridgeRoomHeader room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer);
            comparisonCount += VerifyRoom(room, hasAreaMap: false);
            comparisonCount += VerifyRoom(room, hasAreaMap: true);
        }

        CartridgeRoomHeader reportedRoom = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(
            bus,
            MapCrossViewAuditDefinitions.WaterRoom);
        if (reportedRoom.Identity != new RoomIdentity(AreaId.Brinstar, 0x27) ||
            reportedRoom.MapPosition.X < AreaMapLayout.PageWidthInTiles)
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
        CartridgeRoomHeader room,
        bool hasAreaMap)
    {
        // A half-screen local coordinate keeps Samus inside every representative room and
        // makes the expected map cell independent of door-entry pose or camera history.
        var samus = new SamusState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)),
        };
        var system = new Bank80SystemState();
        if (hasAreaMap)
            system.SetAreaMapAcquired(room.AreaIndex);

        var hud = new HudState();
        hud.Initialize(HudSnapshot.CeresDebug);
        hud.UpdateMinimap(system, new MinimapFrame { AreaIndex = room.AreaIndex, RoomMap = room.MapPosition, RoomSize = room.Size.ToRoomBlocks(), Samus = samus.Position.Whole, FrameCounter = 8 });

        var pause = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = system, AreaIndex = room.AreaIndex, RoomMapPosition = room.MapPosition });
        _ = pause.Render();

        Position16<OamPixels> expectedMarker = hud.MinimapCenter
            .ToMapPixels().ToScreen(pause.MapScroll).ToOam();
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
                MapTileWord hudWord = new(hud.Tiles[26 + outputY * HudState.WidthInTiles + outputX]);
                MapTileWord pauseWord = pause.ReadDisplayedMapTile(new(checked((ushort)mapX), checked((ushort)mapY)));
                if (hudWord.IsBlank != pauseWord.IsBlank ||
                    (!hudWord.IsBlank &&
                        (hudWord.CharacterIndex != pauseWord.CharacterIndex ||
                         hudWord.FlipFlags != pauseWord.FlipFlags)))
                {
                    throw new InvalidDataException(
                        $"Room {room.Identity} map cell ({mapX},{mapY}) disagreed " +
                        $"with area-map acquisition={hasAreaMap}: HUD=${hudWord.Raw:X4}, " +
                        $"pause=${pauseWord.Raw:X4}.");
                }
                comparisons++;
            }
        }
        return comparisons;
    }
}
