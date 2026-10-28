using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

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
    internal static readonly IReadOnlyDictionary<AreaId, int> Counts = new Dictionary<AreaId, int>
    {
        [AreaId.Crateria] = 19,
        [AreaId.Brinstar] = 19,
        [AreaId.Norfair] = 23,
        [AreaId.WreckedShip] = 18,
        [AreaId.Maridia] = 20,
        [AreaId.Tourian] = 18,
        [AreaId.Ceres] = 17,
    };

    /// <summary>Each named non-Ceres station's placement, by area and station.</summary>
    internal static readonly IReadOnlyDictionary<LoadStationKey, LoadStationDefinition> Placements =
        new Dictionary<LoadStationKey, LoadStationDefinition>
    {
        [new(AreaId.Crateria, 0)] = new(RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.Parlor1, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        [new(AreaId.Crateria, 1)] = new(RoomHeaderDefinitions.CrateriaSave, CrateriaDoorDefinitions.Parlor5, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Crateria, 8)] = new(RoomHeaderDefinitions.ForgottenHighwayElev, CrateriaDoorDefinitions.ForgottenHighwayElbow1, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        [new(AreaId.Crateria, 9)] = new(RoomHeaderDefinitions.RedBinstarElev, CrateriaDoorDefinitions.CrateriaKihunter2, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        [new(AreaId.Crateria, 10)] = new(RoomHeaderDefinitions.ElevToBlueBrinstar, CrateriaDoorDefinitions.Pit1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        [new(AreaId.Crateria, 11)] = new(RoomHeaderDefinitions.GreenBrinstarElev, CrateriaDoorDefinitions.LowerMushrooms1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        [new(AreaId.Crateria, 12)] = new(RoomHeaderDefinitions.Statues, CrateriaDoorDefinitions.StatuesHallway1, new(0x0000, 0x0100), new(0x0000, 0x0098)),
        [new(AreaId.Crateria, 16)] = new(RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.Parlor1, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        [new(AreaId.Crateria, 17)] = new(RoomHeaderDefinitions.EastOcean, CrateriaDoorDefinitions.ForgottenHighwayKagos0, new(0x0000, 0x0400), new(0x0000, 0x0095)),
        [new(AreaId.Crateria, 18)] = new(RoomHeaderDefinitions.LandingSite, CrateriaDoorDefinitions.LandingSiteLandingCutscene, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        [new(AreaId.Brinstar, 0)] = new(RoomHeaderDefinitions.BigPinkSaveRoom, BrinstarDoorDefinitions.BigPink4, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Brinstar, 1)] = new(RoomHeaderDefinitions.GreenBrinstarSave, BrinstarDoorDefinitions.GreenBrinstarMainShaftA, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Brinstar, 2)] = new(RoomHeaderDefinitions.EtecoonSave, BrinstarDoorDefinitions.EtecoonETank3, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Brinstar, 3)] = new(RoomHeaderDefinitions.WarehouseSave, BrinstarDoorDefinitions.WarehouseKihunter2, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Brinstar, 4)] = new(RoomHeaderDefinitions.RedBrinstarSave, BrinstarDoorDefinitions.Caterpillar6, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Brinstar, 8)] = new(RoomHeaderDefinitions.GreenBrinstarMainShaft, BrinstarDoorDefinitions.BrinstarPreMap1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Brinstar, 9)] = new(RoomHeaderDefinitions.MorphBall, BrinstarDoorDefinitions.GreenHillZone1, new(0x0500, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Brinstar, 10)] = new(RoomHeaderDefinitions.Caterpillar, BrinstarDoorDefinitions.Hellway1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Brinstar, 11)] = new(RoomHeaderDefinitions.WarehouseEntrance, MaridiaDoorDefinitions.EastTunnel1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        [new(AreaId.Brinstar, 16)] = new(RoomHeaderDefinitions.GreenBrinstarMainShaft, BrinstarDoorDefinitions.BrinstarPreMap1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Brinstar, 17)] = new(RoomHeaderDefinitions.KraidEyeDoor, BrinstarDoorDefinitions.Kraid0, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        [new(AreaId.Brinstar, 18)] = new(RoomHeaderDefinitions.BigPink, BrinstarDoorDefinitions.PinkBrinstarPowerBombs0, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        [new(AreaId.Norfair, 0)] = new(RoomHeaderDefinitions.PostCrocSave, NorfairDoorDefinitions.PostCrocFarming3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Norfair, 1)] = new(RoomHeaderDefinitions.BubbleMountainSave, NorfairDoorDefinitions.BubbleMountain6, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Norfair, 2)] = new(RoomHeaderDefinitions.FrogSave, NorfairDoorDefinitions.FrogSpeedway0, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Norfair, 3)] = new(RoomHeaderDefinitions.CrocomireSave, NorfairDoorDefinitions.CrocomireSpeedway2, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Norfair, 4)] = new(RoomHeaderDefinitions.LNElevSave, NorfairDoorDefinitions.LowerNorfairElev3, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Norfair, 5)] = new(RoomHeaderDefinitions.LNSave, NorfairDoorDefinitions.RedKihunterShaft3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Norfair, 8)] = new(RoomHeaderDefinitions.BusinessCenter, NorfairDoorDefinitions.CathedralEntrance0, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Norfair, 9)] = new(RoomHeaderDefinitions.LowerNorfairElev, NorfairDoorDefinitions.LavaDive1, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        [new(AreaId.Norfair, 10)] = new(RoomHeaderDefinitions.MainHall, NorfairDoorDefinitions.AcidStatue1, new(0x0400, 0x0200), new(0x0000, 0x0088)),
        [new(AreaId.Norfair, 16)] = new(RoomHeaderDefinitions.BusinessCenter, NorfairDoorDefinitions.IceBeamGate2, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Norfair, 17)] = new(RoomHeaderDefinitions.CrocomireSpeedway, NorfairDoorDefinitions.Crocomire1, new(0x0c00, 0x0200), new(0x0000, 0x00a0)),
        [new(AreaId.Norfair, 18)] = new(RoomHeaderDefinitions.LNFarming, NorfairDoorDefinitions.Plowerhouse0, new(0x0000, 0x0000), new(0x0000, 0x00a0)),
        [new(AreaId.Norfair, 19)] = new(RoomHeaderDefinitions.PostCrocFarming, NorfairDoorDefinitions.PostCrocPowerBombs0, new(0x0000, 0x0000), new(0x0000, 0x00b5)),
        [new(AreaId.Norfair, 20)] = new(RoomHeaderDefinitions.MainHall, NorfairDoorDefinitions.AcidStatue1, new(0x0500, 0x0200), new(0x0000, 0x0035)),
        [new(AreaId.Norfair, 21)] = new(RoomHeaderDefinitions.GoldenTorizo, NorfairDoorDefinitions.GTEnergyRefill0, new(0x0200, 0x0200), new(0x0000, 0x0000)),
        [new(AreaId.Norfair, 22)] = new(RoomHeaderDefinitions.GoldenTorizo, NorfairDoorDefinitions.AcidStatue0, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        [new(AreaId.WreckedShip, 0)] = new(RoomHeaderDefinitions.WreckedShipSave, WreckedShipDoorDefinitions.WreckedShipMainShaft6, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.WreckedShip, 16)] = new(RoomHeaderDefinitions.WreckedShipEntrance, WreckedShipDoorDefinitions.WreckedShipMainShaft0, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        [new(AreaId.WreckedShip, 17)] = new(RoomHeaderDefinitions.Basement, WreckedShipDoorDefinitions.WreckedShipMap0, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        [new(AreaId.Maridia, 0)] = new(RoomHeaderDefinitions.GlassTunnelSave, MaridiaDoorDefinitions.GlassTunnel3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Maridia, 1)] = new(RoomHeaderDefinitions.ForgottenHighwaySave, MaridiaDoorDefinitions.MaridiaElev1, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Maridia, 2)] = new(RoomHeaderDefinitions.AqueductSave, MaridiaDoorDefinitions.Aqueduct5, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Maridia, 3)] = new(RoomHeaderDefinitions.DraygonSave, MaridiaDoorDefinitions.Colosseum1, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Maridia, 8)] = new(RoomHeaderDefinitions.MaridiaElev, MaridiaDoorDefinitions.ThreadTheNeedle1, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Maridia, 16)] = new(RoomHeaderDefinitions.PseudoPlasmaSpark, MaridiaDoorDefinitions.NWestMaridiaBug1, new(0x0000, 0x0000), new(0x0000, 0x00d0)),
        [new(AreaId.Maridia, 17)] = new(RoomHeaderDefinitions.ThePrecious, MaridiaDoorDefinitions.ThePreciousMaridiaLoad11, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        [new(AreaId.Maridia, 18)] = new(RoomHeaderDefinitions.BotwoonHallway, MaridiaDoorDefinitions.Aqueduct3, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        [new(AreaId.Maridia, 19)] = new(RoomHeaderDefinitions.Oasis, MaridiaDoorDefinitions.WestSandHall1, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        [new(AreaId.Tourian, 0)] = new(RoomHeaderDefinitions.MotherBrainSave, TourianDoorDefinitions.RinkaShaft1, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        [new(AreaId.Tourian, 1)] = new(RoomHeaderDefinitions.UpperTourianSave, TourianDoorDefinitions.TourianFirst3, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        [new(AreaId.Tourian, 8)] = new(RoomHeaderDefinitions.TourianFirst, TourianDoorDefinitions.Metroids10, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        [new(AreaId.Tourian, 16)] = new(RoomHeaderDefinitions.RinkaShaft, TourianDoorDefinitions.TourianEyeDoor1, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        [new(AreaId.Tourian, 17)] = new(RoomHeaderDefinitions.RinkaShaft, TourianDoorDefinitions.BigBoy0, new(0x0000, 0x0000), new(0x0000, 0x0080)),
    };

    /// <summary>The native inert record an unused station slot receives.</summary>
    internal static readonly LoadStationDefinition Placeholder = new(null, null, new(0x0400, 0x0400), new(0, 0x00b0));

    /// <summary>Compiled room, door, camera, and Samus offsets before projection into a load entry.</summary>
    internal readonly record struct LoadStationDefinition(
        RoomHeaderDefinition? Room, DoorDefinition? Door, Position16<RoomPixels> CameraPosition,
        Offset16<RoomPixels> SamusOffset);
}
