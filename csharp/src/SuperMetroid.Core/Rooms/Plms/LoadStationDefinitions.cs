using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Direct load-station placement selection for native LoadFromLoadStation ($80:C437).</summary>
/// <remarks>Area lists are consecutive fourteen-byte records starting at $80:C4C5.
/// Named area/station cases select active room, door, BTS and placement settings;
/// valid unused slots select the native inert placeholder. Ceres repeats one
/// placement with Y offset72 for station0 and64 for stations1..16. Debug-area data
/// beginning atCC19 is outside the accepted domain. No record or pointer array is stored.</remarks>
public static class LoadStationDefinitions
{
    /// <summary>Native list lengths, including inert placeholders.</summary>
    public static int Count(AreaId area) => area switch
    {
        AreaId.Crateria => 19,
        AreaId.Brinstar => 19,
        AreaId.Norfair => 23,
        AreaId.WreckedShip => 18,
        AreaId.Maridia => 20,
        AreaId.Tourian => 18,
        AreaId.Ceres => 17,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID."),
    };

    /// <summary>Constructs one exact native placement after validating area and station bounds.</summary>
    public static LoadStationEntry Get(AreaId areaIndex, byte stationIndex)
    {
        int area = AreaIds.ToIndex(areaIndex);
        int count = Count(areaIndex);
        if (stationIndex >= count)
            throw new ArgumentOutOfRangeException(nameof(stationIndex), stationIndex,
                $"{areaIndex} has {count} load-station slots.");
        int preceding = 0;
        for (int index = 0; index < area; index++) preceding += Count((AreaId)index);
        LoadStationDefinition entry = areaIndex == AreaId.Ceres
            ? new(RoomHeaderDefinitions.CeresElev, DoorDefinitions.FallingTile0, Position16<RoomPixels>.Zero, new(0, (short)(stationIndex == 0 ? 72 : 64)))
            : Placement(areaIndex, stationIndex);
        return new LoadStationEntry(entry.Room, entry.Door, entry.CameraPosition, entry.SamusOffset);
    }

    /// <summary>Selects a validated non-Ceres station; unused slots receive the native inert record.</summary>
    private static LoadStationDefinition Placement(AreaId area, byte station) => (area, station) switch
    {
        (AreaId.Crateria, 0) => new(RoomHeaderDefinitions.LandingSite, DoorDefinitions.Parlor1, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        (AreaId.Crateria, 1) => new(RoomHeaderDefinitions.CrateriaSave, DoorDefinitions.Parlor5, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Crateria, 8) => new(RoomHeaderDefinitions.ForgottenHighwayElev, DoorDefinitions.ForgottenHighwayElbow1, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        (AreaId.Crateria, 9) => new(RoomHeaderDefinitions.RedBinstarElev, DoorDefinitions.CrateriaKihunter2, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        (AreaId.Crateria, 10) => new(RoomHeaderDefinitions.ElevToBlueBrinstar, DoorDefinitions.Pit1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Crateria, 11) => new(RoomHeaderDefinitions.GreenBrinstarElev, DoorDefinitions.LowerMushrooms1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Crateria, 12) => new(RoomHeaderDefinitions.Statues, DoorDefinitions.StatuesHallway1, new(0x0000, 0x0100), new(0x0000, 0x0098)),
        (AreaId.Crateria, 16) => new(RoomHeaderDefinitions.LandingSite, DoorDefinitions.Parlor1, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        (AreaId.Crateria, 17) => new(RoomHeaderDefinitions.EastOcean, DoorDefinitions.ForgottenHighwayKagos0, new(0x0000, 0x0400), new(0x0000, 0x0095)),
        (AreaId.Crateria, 18) => new(RoomHeaderDefinitions.LandingSite, DoorDefinitions.LandingSiteLandingCutscene, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Brinstar, 0) => new(RoomHeaderDefinitions.BigPinkSaveRoom, DoorDefinitions.BigPink4, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 1) => new(RoomHeaderDefinitions.GreenBrinstarSave, DoorDefinitions.GreenBrinstarMainShaftA, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 2) => new(RoomHeaderDefinitions.EtecoonSave, DoorDefinitions.EtecoonETank3, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 3) => new(RoomHeaderDefinitions.WarehouseSave, DoorDefinitions.WarehouseKihunter2, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Brinstar, 4) => new(RoomHeaderDefinitions.RedBrinstarSave, DoorDefinitions.Caterpillar6, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Brinstar, 8) => new(RoomHeaderDefinitions.GreenBrinstarMainShaft, DoorDefinitions.BrinstarPreMap1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 9) => new(RoomHeaderDefinitions.MorphBall, DoorDefinitions.GreenHillZone1, new(0x0500, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 10) => new(RoomHeaderDefinitions.Caterpillar, DoorDefinitions.Hellway1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 11) => new(RoomHeaderDefinitions.WarehouseEntrance, DoorDefinitions.EastTunnel1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Brinstar, 16) => new(RoomHeaderDefinitions.GreenBrinstarMainShaft, DoorDefinitions.BrinstarPreMap1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 17) => new(RoomHeaderDefinitions.KraidEyeDoor, DoorDefinitions.Kraid0, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        (AreaId.Brinstar, 18) => new(RoomHeaderDefinitions.BigPink, DoorDefinitions.PinkBrinstarPowerBombs0, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Norfair, 0) => new(RoomHeaderDefinitions.PostCrocSave, DoorDefinitions.PostCrocFarming3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 1) => new(RoomHeaderDefinitions.BubbleMountainSave, DoorDefinitions.BubbleMountain6, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Norfair, 2) => new(RoomHeaderDefinitions.FrogSave, DoorDefinitions.FrogSpeedway0, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 3) => new(RoomHeaderDefinitions.CrocomireSave, DoorDefinitions.CrocomireSpeedway2, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 4) => new(RoomHeaderDefinitions.LNElevSave, DoorDefinitions.LowerNorfairElev3, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Norfair, 5) => new(RoomHeaderDefinitions.LNSave, DoorDefinitions.RedKihunterShaft3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 8) => new(RoomHeaderDefinitions.BusinessCenter, DoorDefinitions.CathedralEntrance0, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Norfair, 9) => new(RoomHeaderDefinitions.LowerNorfairElev, DoorDefinitions.LavaDive1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Norfair, 10) => new(RoomHeaderDefinitions.MainHall, DoorDefinitions.AcidStatue1, new(0x0400, 0x0200), new(0x0000, 0x0088)),
        (AreaId.Norfair, 16) => new(RoomHeaderDefinitions.BusinessCenter, DoorDefinitions.IceBeamGate2, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Norfair, 17) => new(RoomHeaderDefinitions.CrocomireSpeedway, DoorDefinitions.Crocomire1, new(0x0c00, 0x0200), new(0x0000, 0x00a0)),
        (AreaId.Norfair, 18) => new(RoomHeaderDefinitions.LNFarming, DoorDefinitions.Plowerhouse0, new(0x0000, 0x0000), new(0x0000, 0x00a0)),
        (AreaId.Norfair, 19) => new(RoomHeaderDefinitions.PostCrocFarming, DoorDefinitions.PostCrocPowerBombs0, new(0x0000, 0x0000), new(0x0000, 0x00b5)),
        (AreaId.Norfair, 20) => new(RoomHeaderDefinitions.MainHall, DoorDefinitions.AcidStatue1, new(0x0500, 0x0200), new(0x0000, 0x0035)),
        (AreaId.Norfair, 21) => new(RoomHeaderDefinitions.GoldenTorizo, DoorDefinitions.GTEnergyRefill0, new(0x0200, 0x0200), new(0x0000, 0x0000)),
        (AreaId.Norfair, 22) => new(RoomHeaderDefinitions.GoldenTorizo, DoorDefinitions.AcidStatue0, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        (AreaId.WreckedShip, 0) => new(RoomHeaderDefinitions.WreckedShipSave, DoorDefinitions.WreckedShipMainShaft6, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.WreckedShip, 16) => new(RoomHeaderDefinitions.WreckedShipEntrance, DoorDefinitions.WreckedShipMainShaft0, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        (AreaId.WreckedShip, 17) => new(RoomHeaderDefinitions.Basement, DoorDefinitions.WreckedShipMap0, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Maridia, 0) => new(RoomHeaderDefinitions.GlassTunnelSave, DoorDefinitions.GlassTunnel3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 1) => new(RoomHeaderDefinitions.ForgottenHighwaySave, DoorDefinitions.MaridiaElev1, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 2) => new(RoomHeaderDefinitions.AqueductSave, DoorDefinitions.Aqueduct5, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Maridia, 3) => new(RoomHeaderDefinitions.DraygonSave, DoorDefinitions.Colosseum1, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 8) => new(RoomHeaderDefinitions.MaridiaElev, DoorDefinitions.ThreadTheNeedle1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Maridia, 16) => new(RoomHeaderDefinitions.PseudoPlasmaSpark, DoorDefinitions.NWestMaridiaBug1, new(0x0000, 0x0000), new(0x0000, 0x00d0)),
        (AreaId.Maridia, 17) => new(RoomHeaderDefinitions.ThePrecious, DoorDefinitions.ThePreciousMaridiaLoad11, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        (AreaId.Maridia, 18) => new(RoomHeaderDefinitions.BotwoonHallway, DoorDefinitions.Aqueduct3, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Maridia, 19) => new(RoomHeaderDefinitions.Oasis, DoorDefinitions.WestSandHall1, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        (AreaId.Tourian, 0) => new(RoomHeaderDefinitions.MotherBrainSave, DoorDefinitions.RinkaShaft1, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Tourian, 1) => new(RoomHeaderDefinitions.UpperTourianSave, DoorDefinitions.TourianFirst3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Tourian, 8) => new(RoomHeaderDefinitions.TourianFirst, DoorDefinitions.Metroids10, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Tourian, 16) => new(RoomHeaderDefinitions.RinkaShaft, DoorDefinitions.TourianEyeDoor1, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        (AreaId.Tourian, 17) => new(RoomHeaderDefinitions.RinkaShaft, DoorDefinitions.BigBoy0, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        _ => new(null, null, new(0x0400, 0x0400), new(0, 0x00b0)),
    };

    /// <summary>Compiled room, door, camera, and Samus offsets before projection into a load entry.</summary>
    private readonly record struct LoadStationDefinition(
        RoomHeaderDefinition? Room, CartridgeDoorHeader? Door, Position16<RoomPixels> CameraPosition,
        Offset16<RoomPixels> SamusOffset);
}
