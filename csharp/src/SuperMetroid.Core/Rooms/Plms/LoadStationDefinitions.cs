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
        ushort listPointer = checked((ushort)((LoadStationRomData.DataStart & 0xffff) +
            LoadStationRomData.EntryByteCount * preceding));
        LoadStationDefinition entry = areaIndex == AreaId.Ceres
            ? new(RoomHeaderDefinitions.CeresElev, 0xab58, Position16<RoomPixels>.Zero, new(0, (short)(stationIndex == 0 ? 72 : 64)))
            : Placement(areaIndex, stationIndex);
        return new LoadStationEntry(entry.Room, entry.DoorPointer, entry.CameraPosition, entry.SamusOffset);
    }

    /// <summary>Selects a validated non-Ceres station; unused slots receive the native inert record.</summary>
    private static LoadStationDefinition Placement(AreaId area, byte station) => (area, station) switch
    {
        (AreaId.Crateria, 0) => new(RoomHeaderDefinitions.LandingSite, 0x896a, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        (AreaId.Crateria, 1) => new(RoomHeaderDefinitions.CrateriaSave, 0x899a, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Crateria, 8) => new(RoomHeaderDefinitions.ForgottenHighwayElev, 0x8aba, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        (AreaId.Crateria, 9) => new(RoomHeaderDefinitions.RedBinstarElev, 0x8a42, new(0x0000, 0x0000), new(0x0000, 0x00a8)),
        (AreaId.Crateria, 10) => new(RoomHeaderDefinitions.ElevToBlueBrinstar, 0x8b86, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Crateria, 11) => new(RoomHeaderDefinitions.GreenBrinstarElev, 0x8c22, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Crateria, 12) => new(RoomHeaderDefinitions.Statues, 0x91f2, new(0x0000, 0x0100), new(0x0000, 0x0098)),
        (AreaId.Crateria, 16) => new(RoomHeaderDefinitions.LandingSite, 0x896a, new(0x0400, 0x0400), new(0x0000, 0x0040)),
        (AreaId.Crateria, 17) => new(RoomHeaderDefinitions.EastOcean, 0x8a7e, new(0x0000, 0x0400), new(0x0000, 0x0095)),
        (AreaId.Crateria, 18) => new(RoomHeaderDefinitions.LandingSite, 0x88fe, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Brinstar, 0) => new(RoomHeaderDefinitions.BigPinkSaveRoom, 0x8df6, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 1) => new(RoomHeaderDefinitions.GreenBrinstarSave, 0x8d12, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 2) => new(RoomHeaderDefinitions.EtecoonSave, 0x8f52, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Brinstar, 3) => new(RoomHeaderDefinitions.WarehouseSave, 0x9186, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Brinstar, 4) => new(RoomHeaderDefinitions.RedBrinstarSave, 0x90d2, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Brinstar, 8) => new(RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x8d42, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 9) => new(RoomHeaderDefinitions.MorphBall, 0x8e86, new(0x0500, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 10) => new(RoomHeaderDefinitions.Caterpillar, 0x908a, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 11) => new(RoomHeaderDefinitions.WarehouseEntrance, 0xa384, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Brinstar, 16) => new(RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x8d42, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Brinstar, 17) => new(RoomHeaderDefinitions.KraidEyeDoor, 0x91ce, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        (AreaId.Brinstar, 18) => new(RoomHeaderDefinitions.BigPink, 0x8e62, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Norfair, 0) => new(RoomHeaderDefinitions.PostCrocSave, 0x9456, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 1) => new(RoomHeaderDefinitions.BubbleMountainSave, 0x959a, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Norfair, 2) => new(RoomHeaderDefinitions.FrogSave, 0x97da, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 3) => new(RoomHeaderDefinitions.CrocomireSave, 0x93ba, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 4) => new(RoomHeaderDefinitions.LNElevSave, 0x9702, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Norfair, 5) => new(RoomHeaderDefinitions.LNSave, 0x9a0e, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Norfair, 8) => new(RoomHeaderDefinitions.BusinessCenter, 0x92a6, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Norfair, 9) => new(RoomHeaderDefinitions.LowerNorfairElev, 0x96de, new(0x0000, 0x0000), new(0x0000, 0x0088)),
        (AreaId.Norfair, 10) => new(RoomHeaderDefinitions.MainHall, 0x9846, new(0x0400, 0x0200), new(0x0000, 0x0088)),
        (AreaId.Norfair, 16) => new(RoomHeaderDefinitions.BusinessCenter, 0x932a, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Norfair, 17) => new(RoomHeaderDefinitions.CrocomireSpeedway, 0x93ea, new(0x0c00, 0x0200), new(0x0000, 0x00a0)),
        (AreaId.Norfair, 18) => new(RoomHeaderDefinitions.LNFarming, 0x995a, new(0x0000, 0x0000), new(0x0000, 0x00a0)),
        (AreaId.Norfair, 19) => new(RoomHeaderDefinitions.PostCrocFarming, 0x946e, new(0x0000, 0x0000), new(0x0000, 0x00b5)),
        (AreaId.Norfair, 20) => new(RoomHeaderDefinitions.MainHall, 0x9846, new(0x0500, 0x0200), new(0x0000, 0x0035)),
        (AreaId.Norfair, 21) => new(RoomHeaderDefinitions.GoldenTorizo, 0x98a6, new(0x0200, 0x0200), new(0x0000, 0x0000)),
        (AreaId.Norfair, 22) => new(RoomHeaderDefinitions.GoldenTorizo, 0x983a, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        (AreaId.WreckedShip, 0) => new(RoomHeaderDefinitions.WreckedShipSave, 0xa240, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.WreckedShip, 16) => new(RoomHeaderDefinitions.WreckedShipEntrance, 0xa1f8, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        (AreaId.WreckedShip, 17) => new(RoomHeaderDefinitions.Basement, 0xa2b8, new(0x0400, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Maridia, 0) => new(RoomHeaderDefinitions.GlassTunnelSave, 0xa354, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 1) => new(RoomHeaderDefinitions.ForgottenHighwaySave, 0xa588, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 2) => new(RoomHeaderDefinitions.AqueductSave, 0xa744, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Maridia, 3) => new(RoomHeaderDefinitions.DraygonSave, 0xa7ec, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Maridia, 8) => new(RoomHeaderDefinitions.MaridiaElev, 0xa570, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Maridia, 16) => new(RoomHeaderDefinitions.PseudoPlasmaSpark, 0xa4a4, new(0x0000, 0x0000), new(0x0000, 0x00d0)),
        (AreaId.Maridia, 17) => new(RoomHeaderDefinitions.ThePrecious, 0xa81c, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        (AreaId.Maridia, 18) => new(RoomHeaderDefinitions.BotwoonHallway, 0xa72c, new(0x0300, 0x0000), new(0x0000, 0x0080)),
        (AreaId.Maridia, 19) => new(RoomHeaderDefinitions.Oasis, 0xa648, new(0x0000, 0x0100), new(0x0000, 0x0080)),
        (AreaId.Tourian, 0) => new(RoomHeaderDefinitions.MotherBrainSave, 0xaabc, new(0x0000, 0x0000), new(-0x0020, 0x0098)),
        (AreaId.Tourian, 1) => new(RoomHeaderDefinitions.UpperTourianSave, 0xa99c, new(0x0000, 0x0000), new(0x0000, 0x0098)),
        (AreaId.Tourian, 8) => new(RoomHeaderDefinitions.TourianFirst, 0xa9a8, new(0x0000, 0x0200), new(0x0000, 0x00a8)),
        (AreaId.Tourian, 16) => new(RoomHeaderDefinitions.RinkaShaft, 0xaaa4, new(0x0000, 0x0200), new(0x0000, 0x0080)),
        (AreaId.Tourian, 17) => new(RoomHeaderDefinitions.RinkaShaft, 0xaa38, new(0x0000, 0x0000), new(0x0000, 0x0080)),
        _ => new(null, 0, new(0x0400, 0x0400), new(0, 0x00b0)),
    };

    /// <summary>Compiled room, door, camera, and Samus offsets before projection into a load entry.</summary>
    private readonly record struct LoadStationDefinition(
        RoomHeaderDefinition? Room, ushort DoorPointer, Position16<RoomPixels> CameraPosition,
        Offset16<RoomPixels> SamusOffset);
}
