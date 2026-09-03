using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Guards the cartridge SRAM schema independently of serializer behavior. Intentional
    /// holes are allowed, but no named field may overlap another field or leave its slot.
    /// </summary>
    static void VerifySaveRamLayout()
    {
        (int Offset, int Length, string Name)[] payloadRegions =
        [
            (SaveRamLayout.EquippedItemsOffset, 2, "equipped items"),
            (SaveRamLayout.CollectedItemsOffset, 2, "collected items"),
            (SaveRamLayout.EquippedBeamsOffset, 2, "equipped beams"),
            (SaveRamLayout.CollectedBeamsOffset, 2, "collected beams"),
            (SaveRamLayout.ButtonConfigOffset,
                SaveRamLayout.ButtonConfigWordCount * SaveRamLayout.WordByteCount,
                "button configuration"),
            (SaveRamLayout.ReserveModeOffset, 2, "reserve mode"),
            (SaveRamLayout.HealthOffset, 2, "health"),
            (SaveRamLayout.MaxHealthOffset, 2, "maximum health"),
            (SaveRamLayout.MissilesOffset, 2, "missiles"),
            (SaveRamLayout.MaxMissilesOffset, 2, "maximum missiles"),
            (SaveRamLayout.SuperMissilesOffset, 2, "super missiles"),
            (SaveRamLayout.MaxSuperMissilesOffset, 2, "maximum super missiles"),
            (SaveRamLayout.PowerBombsOffset, 2, "power bombs"),
            (SaveRamLayout.MaxPowerBombsOffset, 2, "maximum power bombs"),
            (SaveRamLayout.HudItemOffset, 2, "HUD item"),
            (SaveRamLayout.MaxReserveEnergyOffset, 2, "maximum reserve energy"),
            (SaveRamLayout.ReserveEnergyOffset, 2, "reserve energy"),
            (SaveRamLayout.GameTimeFramesOffset, 2, "game-time frames"),
            (SaveRamLayout.GameTimeSecondsOffset, 2, "game-time seconds"),
            (SaveRamLayout.GameTimeMinutesOffset, 2, "game-time minutes"),
            (SaveRamLayout.GameTimeHoursOffset, 2, "game-time hours"),
            (SaveRamLayout.MoonwalkOffset, 2, "moonwalk option"),
            (SaveRamLayout.DebugFlagOffset, 2, "debug flag"),
            (SaveRamLayout.NewFileMarkerOffset, 2, "new-file marker"),
            (SaveRamLayout.IconCancelOffset, 2, "icon-cancel option"),
            (SaveRamLayout.EventsOffset, Bank80SystemState.EventByteCount, "event bits"),
            (SaveRamLayout.BossBitsOffset, Bank80SystemState.AreaCount, "boss bits"),
            (SaveRamLayout.RoomChozoBitsOffset,
                Bank80SystemState.RoomChozoBitByteCount, "room Chozo bits"),
            (SaveRamLayout.CollectedItemBitsOffset,
                Bank80SystemState.ItemBitByteCount, "collected-item bits"),
            (SaveRamLayout.OpenedDoorBitsOffset,
                Bank80SystemState.DoorBitByteCount, "opened-door bits"),
            (SaveRamLayout.UsedSaveStationsOffset,
                Bank80SystemState.UsedSaveStationByteCount, "used save stations"),
            (SaveRamLayout.MapStationsOffset,
                Bank80SystemState.MapStationByteCount, "map stations"),
            (SaveRamLayout.SaveStationOffset, 2, "save station"),
            (SaveRamLayout.AreaOffset, 2, "area"),
            (SaveRamLayout.CompressedMapDataOffset,
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

        ReadOnlySpan<ushort> slotOffsets = SaveRamLayout.SlotOffsets;
        AssertEqual(SaveRamLayout.SlotCount, slotOffsets.Length,
            "SRAM layout publishes every native slot");
        for (int slot = 1; slot < slotOffsets.Length; slot++)
        {
            AssertEqual(
                slotOffsets[slot - 1] + SaveRamLayout.SlotByteCount,
                slotOffsets[slot],
                $"SRAM slot {slot - 1} ends where slot {slot} begins");
        }
        AssertTrue(slotOffsets[^1] + SaveRamLayout.SlotByteCount <=
            SaveRamLayout.SelectedSlotOffset,
            "slot payloads do not overlap selected-slot directory");
        AssertTrue(SaveRamLayout.PrimaryComplementOffset +
            SaveRamLayout.SlotCount * SaveRamLayout.WordByteCount <= slotOffsets[0],
            "primary checksum directories do not overlap slot zero");
        AssertTrue(SaveRamLayout.SelectedSlotComplementOffset + SaveRamLayout.WordByteCount <=
            SaveRamLayout.BackupChecksumOffset,
            "selected-slot pair does not overlap backup checksums");
        AssertTrue(SaveRamLayout.BackupComplementOffset +
            SaveRamLayout.SlotCount * SaveRamLayout.WordByteCount <=
            SaveRamLayout.SramOffsetMask + 1,
            "backup checksum directories remain within SRAM");

        AssertTrue(SaveRamLayout.PackedMapByteCountTable.IsUpperLoRomWindow &&
            SaveRamLayout.PackedMapDestinationOffsetTable.IsUpperLoRomWindow &&
            SaveRamLayout.PackedMapSourceIndexPointerTable.IsUpperLoRomWindow,
            "packed-map ROM tables use validated upper-LoROM addresses");

        Console.WriteLine(
            "  SRAM layout: payload regions, slot boundaries, directories, and ROM tables agree.");
    }
}
