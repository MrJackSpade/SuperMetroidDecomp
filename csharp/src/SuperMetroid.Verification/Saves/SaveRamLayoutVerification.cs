using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Verification.Saves;

/// <summary>Checks cartridge SRAM field spans and slot directories against the declared layout.</summary>
internal static class SaveRamLayoutVerification
{
    /// <summary>
    /// Guards the cartridge SRAM schema independently of serializer behavior. Intentional
    /// holes are allowed, but no named field may overlap another field or leave its slot.
    /// </summary>
    internal static void VerifySaveRamLayout()
    {
        PayloadRegion[] payloadRegions =
        [
            new PayloadRegion(SaveRamLayout.EquippedItemsOffset, 2, "equipped items"),
            new PayloadRegion(SaveRamLayout.CollectedItemsOffset, 2, "collected items"),
            new PayloadRegion(SaveRamLayout.EquippedBeamsOffset, 2, "equipped beams"),
            new PayloadRegion(SaveRamLayout.CollectedBeamsOffset, 2, "collected beams"),
            new PayloadRegion(SaveRamLayout.ButtonConfigOffset,
                SaveRamLayout.ButtonConfigWordCount * SaveRamLayout.WordByteCount,
                "button configuration"),
            new PayloadRegion(SaveRamLayout.ReserveModeOffset, 2, "reserve mode"),
            new PayloadRegion(SaveRamLayout.HealthOffset, 2, "health"),
            new PayloadRegion(SaveRamLayout.MaxHealthOffset, 2, "maximum health"),
            new PayloadRegion(SaveRamLayout.MissilesOffset, 2, "missiles"),
            new PayloadRegion(SaveRamLayout.MaxMissilesOffset, 2, "maximum missiles"),
            new PayloadRegion(SaveRamLayout.SuperMissilesOffset, 2, "super missiles"),
            new PayloadRegion(SaveRamLayout.MaxSuperMissilesOffset, 2, "maximum super missiles"),
            new PayloadRegion(SaveRamLayout.PowerBombsOffset, 2, "power bombs"),
            new PayloadRegion(SaveRamLayout.MaxPowerBombsOffset, 2, "maximum power bombs"),
            new PayloadRegion(SaveRamLayout.HudItemOffset, 2, "HUD item"),
            new PayloadRegion(SaveRamLayout.MaxReserveEnergyOffset, 2, "maximum reserve energy"),
            new PayloadRegion(SaveRamLayout.ReserveEnergyOffset, 2, "reserve energy"),
            new PayloadRegion(SaveRamLayout.GameTimeFramesOffset, 2, "game-time frames"),
            new PayloadRegion(SaveRamLayout.GameTimeSecondsOffset, 2, "game-time seconds"),
            new PayloadRegion(SaveRamLayout.GameTimeMinutesOffset, 2, "game-time minutes"),
            new PayloadRegion(SaveRamLayout.GameTimeHoursOffset, 2, "game-time hours"),
            new PayloadRegion(SaveRamLayout.MoonwalkOffset, 2, "moonwalk option"),
            new PayloadRegion(SaveRamLayout.DebugFlagOffset, 2, "debug flag"),
            new PayloadRegion(SaveRamLayout.NewFileMarkerOffset, 2, "new-file marker"),
            new PayloadRegion(SaveRamLayout.IconCancelOffset, 2, "icon-cancel option"),
            new PayloadRegion(SaveRamLayout.EventsOffset, SharedGameState.EventByteCount, "event bits"),
            new PayloadRegion(SaveRamLayout.BossBitsOffset, SharedGameState.AreaCount, "boss bits"),
            new PayloadRegion(SaveRamLayout.RoomChozoBitsOffset,
                SharedGameState.RoomChozoBitByteCount, "room Chozo bits"),
            new PayloadRegion(SaveRamLayout.CollectedItemBitsOffset,
                SharedGameState.ItemBitByteCount, "collected-item bits"),
            new PayloadRegion(SaveRamLayout.OpenedDoorBitsOffset,
                SharedGameState.DoorBitByteCount, "opened-door bits"),
            new PayloadRegion(SaveRamLayout.UsedSaveStationsOffset,
                SharedGameState.UsedSaveStationByteCount, "used save stations"),
            new PayloadRegion(SaveRamLayout.MapStationsOffset,
                SharedGameState.MapStationByteCount, "map stations"),
            new PayloadRegion(SaveRamLayout.SaveStationOffset, 2, "save station"),
            new PayloadRegion(SaveRamLayout.AreaOffset, 2, "area"),
            new PayloadRegion(SaveRamLayout.CompressedMapDataOffset,
                SaveRamLayout.CompressedMapDataByteCount, "packed explored map"),
        ];

        int previousEnd = 0;
        foreach ((int offset, int length, string name) in payloadRegions)
        {
            AssertTrue(offset >= previousEnd, $"SRAM {name} does not overlap prior field");
            AssertTrue(offset + length <= SaveRamLayout.SlotByteCount,
                $"SRAM {name} remains within one slot");
            previousEnd = offset + length;
        }
        AssertEqual(SaveRamLayout.SlotByteCount, previousEnd,
            "packed map terminates exactly at slot boundary");


        for (int slot = 1; slot < SaveRamLayout.SlotCount; slot++)
        {
            AssertEqual(
                SaveRamLayout.SlotOffset(slot - 1) + SaveRamLayout.SlotByteCount,
                SaveRamLayout.SlotOffset(slot),
                $"SRAM slot {slot - 1} ends where slot {slot} begins");
        }
        AssertTrue(SaveRamLayout.SlotOffset(SaveRamLayout.SlotCount - 1) + SaveRamLayout.SlotByteCount <=
            SaveRamLayout.SelectedSlotOffset,
            "slot payloads do not overlap selected-slot directory");
        AssertTrue(SaveRamLayout.PrimaryComplementOffset +
            SaveRamLayout.SlotCount * SaveRamLayout.WordByteCount <= SaveRamLayout.SlotOffset(0),
            "primary checksum directories do not overlap slot zero");
        AssertTrue(SaveRamLayout.SelectedSlotComplementOffset + SaveRamLayout.WordByteCount <=
            SaveRamLayout.BackupChecksumOffset,
            "selected-slot pair does not overlap backup checksums");
        AssertTrue(SaveRamLayout.BackupComplementOffset +
            SaveRamLayout.SlotCount * SaveRamLayout.WordByteCount <=
            SaveRamLayout.SramOffsetMask + 1,
            "backup checksum directories remain within SRAM");

        Console.WriteLine(
            "  SRAM layout: payload regions, slot boundaries, and directories agree.");
    }
}
