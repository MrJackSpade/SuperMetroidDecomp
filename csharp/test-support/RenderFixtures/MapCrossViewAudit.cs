using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Representative retail rooms used to compare both map presentation paths.</summary>
internal static class MapCrossViewAuditDefinitions
{
    /// <summary>Landing Site, a left-page Crateria room at <c>$8F:91F8</c>.</summary>
    public const ushort LandingSiteRoom = 0x91f8;
    /// <summary>Water room <c>$01/$27</c>, on Brinstar's right map page at <c>$8F:A3DD</c>.</summary>
    public const ushort WaterRoom = 0xa3dd;
    /// <summary>Crocomire room, a left-page Norfair room at <c>$8F:A98D</c>.</summary>
    public const ushort CrocomireRoom = 0xa98d;
    /// <summary>Botwoon room, a left-page Maridia room at <c>$8F:D95E</c>.</summary>
    public const ushort BotwoonRoom = 0xd95e;

    public static readonly ushort[] RepresentativeRooms =
    [
        LandingSiteRoom,
        WaterRoom,
        CrocomireRoom,
        BotwoonRoom,
    ];
}

/// <summary>
/// Retail-ROM regression for issue #252. The audit gives the HUD and pause screen the
/// same persistent state, room header, and Samus world coordinate, then compares every
/// visible 5x3 cell and the independently rendered pause-map marker.
/// </summary>
internal static class MapCrossViewAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int comparisonCount = 0;
        foreach (ushort roomPointer in MapCrossViewAuditDefinitions.RepresentativeRooms)
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
            comparisonCount += VerifyRoom(bus, room, hasAreaMap: false);
            comparisonCount += VerifyRoom(bus, room, hasAreaMap: true);
        }

        CartridgeRoomHeader reportedRoom = CartridgeRoomHeader.Load(
            bus,
            MapCrossViewAuditDefinitions.WaterRoom);
        if (reportedRoom.Identity != new RoomIdentity(AreaId.Brinstar, 0x27) ||
            reportedRoom.MapX < AreaMapLayout.PageWidthInTiles)
        {
            throw new InvalidDataException(
                $"Map audit expected right-page room $01/$27, got {reportedRoom.Identity} " +
                $"at map ({reportedRoom.MapX},{reportedRoom.MapY}).");
        }

        Console.WriteLine(
            $"Map cross-view audit passed: {comparisonCount} HUD cells and eight pause " +
            "markers agree across Crateria, Brinstar $01/$27, Norfair, and Maridia, " +
            "with and without downloaded area maps.");
        return 0;
    }

    private static int VerifyRoom(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        bool hasAreaMap)
    {
        // A half-screen local coordinate keeps Samus inside every representative room and
        // makes the expected map cell independent of door-entry pose or camera history.
        var samus = new SamusState
        {
            XPosition = 0x0080,
            YPosition = 0x0080,
        };
        var system = new Bank80SystemState();
        if (hasAreaMap)
            system.SetAreaMapAcquired(room.AreaIndex);

        var hud = new HudState();
        hud.Initialize(bus, HudSnapshot.CeresDebug);
        hud.UpdateMinimap(
            bus,
            system,
            room.AreaIndex,
            room.MapX,
            room.MapY,
            room.WidthInScreens * 16,
            room.HeightInScreens * 16,
            samus.XPosition,
            samus.YPosition,
            nmiFrameCounter: 8);

        var pause = new PauseMenuState(
            bus,
            samus,
            system,
            room.AreaIndex,
            room.MapX,
            room.MapY);
        _ = pause.Render();

        ushort expectedMarkerX = unchecked((ushort)(
            8 * hud.MinimapCenterX - pause.MapHorizontalScroll));
        ushort expectedMarkerY = unchecked((ushort)(
            8 * hud.MinimapCenterY - pause.MapVerticalScroll));
        if (pause.LastIndicatorOriginX != expectedMarkerX ||
            pause.LastIndicatorOriginY != expectedMarkerY)
        {
            throw new InvalidDataException(
                $"Room {room.Identity} map marker mismatch: HUD center=" +
                $"({hud.MinimapCenterX},{hud.MinimapCenterY}), pause origin=" +
                $"({pause.LastIndicatorOriginX},{pause.LastIndicatorOriginY}), expected=" +
                $"({expectedMarkerX},{expectedMarkerY}).");
        }

        int comparisons = 0;
        for (int outputY = 0; outputY < 3; outputY++)
        {
            int mapY = hud.MinimapCenterY + outputY - 1;
            for (int outputX = 0; outputX < 5; outputX++)
            {
                int mapX = (hud.MinimapCenterX + outputX - 2) & 0x3f;
                MapTileWord hudWord = hud.Tiles[26 + outputY * HudState.WidthInTiles + outputX];
                MapTileWord pauseWord = pause.ReadDisplayedMapTile(mapX, mapY);
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
