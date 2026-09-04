using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-compatible owner of Super Metroid's three battery-backed save slots.
/// </summary>
/// <remarks>
/// <c>$81:8000-$81:812A</c> stores one contiguous WRAM mirror ($D7C0-$DE1B), then writes
/// both a checksum and its complement into two redundant SRAM directories. Keeping that
/// physical layout remains the in-memory cartridge ABI and the lossless source for legacy
/// emulator-save migration, even though the desktop now persists a named JSON projection.
/// </remarks>
public sealed class SuperMetroidSaveRam
{
    public const int SlotCount = SaveRamLayout.SlotCount;
    public const int SlotByteCount = SaveRamLayout.SlotByteCount;
    public const int SelectedSlotOffset = SaveRamLayout.SelectedSlotOffset;

    private readonly ISnesAddressSpace bus;

    public SuperMetroidSaveRam(ISnesAddressSpace bus) =>
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));

    /// <summary>Returns a decoded slot only when either redundant checksum pair is valid.</summary>
    public SuperMetroidSaveSlot? ReadSlot(int slot)
    {
        int slotOffset = GetSlotOffset(slot);
        ushort checksum = CalculateChecksum(slotOffset);
        ushort complement = unchecked((ushort)~checksum);
        bool primaryValid =
            ReadSramWord(SaveRamLayout.PrimaryChecksumOffset + slot * 2) == checksum &&
            ReadSramWord(SaveRamLayout.PrimaryComplementOffset + slot * 2) == complement;
        bool backupValid =
            ReadSramWord(SaveRamLayout.BackupChecksumOffset + slot * 2) == checksum &&
            ReadSramWord(SaveRamLayout.BackupComplementOffset + slot * 2) == complement;
        if (!primaryValid && !backupValid)
            return null;

        return new SuperMetroidSaveSlot(
            Slot: slot,
            EquippedItems: ReadSramWord(slotOffset + SaveRamLayout.EquippedItemsOffset),
            CollectedItems: ReadSramWord(slotOffset + SaveRamLayout.CollectedItemsOffset),
            EquippedBeams: ReadSramWord(slotOffset + SaveRamLayout.EquippedBeamsOffset),
            CollectedBeams: ReadSramWord(slotOffset + SaveRamLayout.CollectedBeamsOffset),
            ReserveMode: ReadSramWord(slotOffset + SaveRamLayout.ReserveModeOffset),
            Health: ReadSramWord(slotOffset + SaveRamLayout.HealthOffset),
            MaxHealth: ReadSramWord(slotOffset + SaveRamLayout.MaxHealthOffset),
            Missiles: ReadSramWord(slotOffset + SaveRamLayout.MissilesOffset),
            MaxMissiles: ReadSramWord(slotOffset + SaveRamLayout.MaxMissilesOffset),
            SuperMissiles: ReadSramWord(slotOffset + SaveRamLayout.SuperMissilesOffset),
            MaxSuperMissiles: ReadSramWord(slotOffset + SaveRamLayout.MaxSuperMissilesOffset),
            PowerBombs: ReadSramWord(slotOffset + SaveRamLayout.PowerBombsOffset),
            MaxPowerBombs: ReadSramWord(slotOffset + SaveRamLayout.MaxPowerBombsOffset),
            HudItem: ReadSramWord(slotOffset + SaveRamLayout.HudItemOffset),
            MaxReserveEnergy: ReadSramWord(slotOffset + SaveRamLayout.MaxReserveEnergyOffset),
            ReserveEnergy: ReadSramWord(slotOffset + SaveRamLayout.ReserveEnergyOffset),
            GameTimeFrames: ReadSramWord(slotOffset + SaveRamLayout.GameTimeFramesOffset),
            GameTimeSeconds: ReadSramWord(slotOffset + SaveRamLayout.GameTimeSecondsOffset),
            GameTimeMinutes: ReadSramWord(slotOffset + SaveRamLayout.GameTimeMinutesOffset),
            GameTimeHours: ReadSramWord(slotOffset + SaveRamLayout.GameTimeHoursOffset),
            EventBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.EventsOffset,
                Bank80SystemState.EventByteCount),
            BossBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.BossBitsOffset,
                Bank80SystemState.AreaCount),
            RoomChozoBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.RoomChozoBitsOffset,
                Bank80SystemState.RoomChozoBitByteCount),
            CollectedItemBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.CollectedItemBitsOffset,
                Bank80SystemState.ItemBitByteCount),
            OpenedDoorBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.OpenedDoorBitsOffset,
                Bank80SystemState.DoorBitByteCount),
            UsedSaveStationBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.UsedSaveStationsOffset,
                Bank80SystemState.UsedSaveStationByteCount),
            MapStationBytes: ReadSramBytes(
                slotOffset + SaveRamLayout.MapStationsOffset,
                Bank80SystemState.MapStationByteCount),
            ExploredMapBytes: UnpackExploredMap(ReadSramBytes(
                slotOffset + SaveRamLayout.CompressedMapDataOffset,
                SaveRamLayout.CompressedMapDataByteCount)),
            SaveStation: ReadSramWord(slotOffset + SaveRamLayout.SaveStationOffset),
            Area: ReadSramWord(slotOffset + SaveRamLayout.AreaOffset))
        {
            // The first four words are the immutable D-pad directions. The remaining
            // seven are laid out by WRAM address rather than by the options-screen row
            // order: cancel precedes select and aim-down precedes aim-up in the save mirror.
            ControllerBindings = new ControllerBindings(
                Shoot: ReadSramWord(slotOffset + SaveRamLayout.ShootButtonOffset),
                Jump: ReadSramWord(slotOffset + SaveRamLayout.JumpButtonOffset),
                Dash: ReadSramWord(slotOffset + SaveRamLayout.DashButtonOffset),
                ItemSelect: ReadSramWord(slotOffset + SaveRamLayout.SelectButtonOffset),
                ItemCancel: ReadSramWord(slotOffset + SaveRamLayout.CancelButtonOffset),
                AimUp: ReadSramWord(slotOffset + SaveRamLayout.AimUpButtonOffset),
                AimDown: ReadSramWord(slotOffset + SaveRamLayout.AimDownButtonOffset))
                .RequireRetailPermutation(),
            MoonwalkEnabled = ReadSramWord(slotOffset + SaveRamLayout.MoonwalkOffset) != 0,
            DebugFlag = ReadSramWord(slotOffset + SaveRamLayout.DebugFlagOffset),
            NewFileMarker = ReadSramWord(slotOffset + SaveRamLayout.NewFileMarkerOffset),
            IconCancelEnabled = ReadSramWord(slotOffset + SaveRamLayout.IconCancelOffset) != 0,
        };
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c> for the translated state currently owned by C# objects.
    /// The slot payload and four directory words are byte-for-byte cartridge structures.
    /// </summary>
    public void SaveSlot(int slot, SuperMetroidSaveSnapshot snapshot)
        => SaveSlot(slot, snapshot, preserveUntranslatedBytes: false);

    /// <summary>
    /// Writes every translated field while retaining bytes that are not yet represented by
    /// <see cref="SuperMetroidSaveSnapshot"/>. JSON migration uses this path after restoring
    /// its preservation image; ordinary cartridge-created saves continue using a clean slot.
    /// </summary>
    public void SaveSlotPreservingUntranslatedBytes(int slot, SuperMetroidSaveSnapshot snapshot)
        => SaveSlot(slot, snapshot, preserveUntranslatedBytes: true);

    private void SaveSlot(
        int slot,
        SuperMetroidSaveSnapshot snapshot,
        bool preserveUntranslatedBytes)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        int slotOffset = GetSlotOffset(slot);
        byte[] payload = preserveUntranslatedBytes
            ? ReadSramBytes(slotOffset, SlotByteCount)
            : new byte[SlotByteCount];

        WriteWord(payload, SaveRamLayout.EquippedItemsOffset, snapshot.EquippedItems);
        WriteWord(payload, SaveRamLayout.CollectedItemsOffset, snapshot.CollectedItems);
        WriteWord(payload, SaveRamLayout.EquippedBeamsOffset, snapshot.EquippedBeams);
        WriteWord(payload, SaveRamLayout.CollectedBeamsOffset, snapshot.CollectedBeams);

        // NewSaveFile at $81:B2CB installs these eleven literal SNES controller words.
        // Later saves copy the live configurable action words, so do not silently restore
        // defaults every time the player reaches a save station.
        ControllerBindings bindings = snapshot.ControllerBindings.RequireRetailPermutation();
        ushort[] buttons =
        [
            (ushort)SnesButton.Up,
            (ushort)SnesButton.Down,
            (ushort)SnesButton.Left,
            (ushort)SnesButton.Right,
            bindings.Shoot,
            bindings.Jump,
            bindings.Dash,
            bindings.ItemCancel,
            bindings.ItemSelect,
            bindings.AimDown,
            bindings.AimUp,
        ];
        for (int index = 0; index < buttons.Length; index++)
            WriteWord(payload, SaveRamLayout.ButtonConfigOffset + index * 2, buttons[index]);

        WriteWord(payload, SaveRamLayout.ReserveModeOffset, snapshot.ReserveMode);
        WriteWord(payload, SaveRamLayout.HealthOffset, snapshot.Health);
        WriteWord(payload, SaveRamLayout.MaxHealthOffset, snapshot.MaxHealth);
        WriteWord(payload, SaveRamLayout.MissilesOffset, snapshot.Missiles);
        WriteWord(payload, SaveRamLayout.MaxMissilesOffset, snapshot.MaxMissiles);
        WriteWord(payload, SaveRamLayout.SuperMissilesOffset, snapshot.SuperMissiles);
        WriteWord(payload, SaveRamLayout.MaxSuperMissilesOffset, snapshot.MaxSuperMissiles);
        WriteWord(payload, SaveRamLayout.PowerBombsOffset, snapshot.PowerBombs);
        WriteWord(payload, SaveRamLayout.MaxPowerBombsOffset, snapshot.MaxPowerBombs);
        WriteWord(payload, SaveRamLayout.HudItemOffset, snapshot.HudItem);
        WriteWord(payload, SaveRamLayout.MaxReserveEnergyOffset, snapshot.MaxReserveEnergy);
        WriteWord(payload, SaveRamLayout.ReserveEnergyOffset, snapshot.ReserveEnergy);
        WriteWord(payload, SaveRamLayout.GameTimeFramesOffset, snapshot.GameTimeFrames);
        WriteWord(payload, SaveRamLayout.GameTimeSecondsOffset, snapshot.GameTimeSeconds);
        WriteWord(payload, SaveRamLayout.GameTimeMinutesOffset, snapshot.GameTimeMinutes);
        WriteWord(payload, SaveRamLayout.GameTimeHoursOffset, snapshot.GameTimeHours);
        WriteWord(payload, SaveRamLayout.MoonwalkOffset, snapshot.MoonwalkEnabled ? (ushort)1 : (ushort)0);
        WriteWord(payload, SaveRamLayout.DebugFlagOffset, snapshot.DebugFlag);
        WriteWord(payload, SaveRamLayout.NewFileMarkerOffset, snapshot.NewFileMarker);
        WriteWord(payload, SaveRamLayout.IconCancelOffset, snapshot.IconCancelEnabled ? (ushort)1 : (ushort)0);

        if (snapshot.EventBytes.Length != Bank80SystemState.EventByteCount)
            throw new InvalidDataException("A save snapshot requires exactly eight event bytes.");
        if (snapshot.BossBytes.Length != Bank80SystemState.AreaCount)
            throw new InvalidDataException("A save snapshot requires exactly eight area-boss bytes.");
        snapshot.EventBytes.CopyTo(payload, SaveRamLayout.EventsOffset);
        snapshot.BossBytes.CopyTo(payload, SaveRamLayout.BossBitsOffset);
        if (snapshot.RoomChozoBytes.Length != Bank80SystemState.RoomChozoBitByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 64 room-Chozo bytes.");
        }
        snapshot.RoomChozoBytes.CopyTo(payload, SaveRamLayout.RoomChozoBitsOffset);
        if (snapshot.CollectedItemBytes.Length != Bank80SystemState.ItemBitByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 64 collected-item bytes.");
        }
        snapshot.CollectedItemBytes.CopyTo(payload, SaveRamLayout.CollectedItemBitsOffset);
        if (snapshot.OpenedDoorBytes.Length != Bank80SystemState.DoorBitByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 64 opened-door bytes.");
        }
        snapshot.OpenedDoorBytes.CopyTo(payload, SaveRamLayout.OpenedDoorBitsOffset);
        if (snapshot.UsedSaveStationBytes.Length != Bank80SystemState.UsedSaveStationByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 16 used save/elevator bytes.");
        }
        snapshot.UsedSaveStationBytes.CopyTo(payload, SaveRamLayout.UsedSaveStationsOffset);
        if (snapshot.MapStationBytes.Length != Bank80SystemState.MapStationByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 12 map-station bytes.");
        snapshot.MapStationBytes.CopyTo(payload, SaveRamLayout.MapStationsOffset);
        WriteWord(payload, SaveRamLayout.SaveStationOffset, snapshot.SaveStation);
        WriteWord(payload, SaveRamLayout.AreaOffset, snapshot.Area);
        byte[] compressedMap = PackExploredMap(snapshot.ExploredMapBytes);
        compressedMap.CopyTo(payload, SaveRamLayout.CompressedMapDataOffset);

        for (int index = 0; index < payload.Length; index++)
            WriteSramByte(slotOffset + index, payload[index]);

        ushort checksum = CalculateChecksum(slotOffset);
        ushort complement = unchecked((ushort)~checksum);
        WriteSramWord(SaveRamLayout.PrimaryChecksumOffset + slot * 2, checksum);
        WriteSramWord(SaveRamLayout.PrimaryComplementOffset + slot * 2, complement);
        WriteSramWord(SaveRamLayout.BackupChecksumOffset + slot * 2, checksum);
        WriteSramWord(SaveRamLayout.BackupComplementOffset + slot * 2, complement);
    }

    /// <summary>Stores the menu's selected-slot word and complement at $1FEC/$1FEE.</summary>
    public void SelectSlot(int slot)
    {
        ValidateSlot(slot);
        WriteSramWord(SelectedSlotOffset, unchecked((ushort)slot));
        WriteSramWord(SelectedSlotOffset + 2, unchecked((ushort)~slot));
    }

    /// <summary>
    /// Copies the cartridge's complete checksummed payload and both redundant directory
    /// pairs, matching file-copy menu index 13 at <c>$81:9A2C</c> byte for byte.
    /// </summary>
    public void CopySlot(int sourceSlot, int destinationSlot)
    {
        int sourceOffset = GetSlotOffset(sourceSlot);
        int destinationOffset = GetSlotOffset(destinationSlot);
        if (sourceSlot == destinationSlot)
            throw new ArgumentException("File copy requires two different SRAM slots.");
        if (ReadSlot(sourceSlot) is null)
            throw new InvalidOperationException($"Cannot copy empty save slot {sourceSlot}.");

        // Do not decode and re-encode here. The native menu copies all $65C bytes,
        // including untranslated/reserved fields which a domain snapshot cannot preserve.
        for (int index = 0; index < SlotByteCount; index++)
            WriteSramByte(destinationOffset + index, ReadSramByte(sourceOffset + index));
        CopyDirectoryWord(SaveRamLayout.PrimaryChecksumOffset, sourceSlot, destinationSlot);
        CopyDirectoryWord(SaveRamLayout.PrimaryComplementOffset, sourceSlot, destinationSlot);
        CopyDirectoryWord(SaveRamLayout.BackupChecksumOffset, sourceSlot, destinationSlot);
        CopyDirectoryWord(SaveRamLayout.BackupComplementOffset, sourceSlot, destinationSlot);
    }

    /// <summary>
    /// Clears one complete native slot and its four checksum-directory words, matching
    /// file-clear menu index 25 at <c>$81:9C9E</c>.
    /// </summary>
    public void ClearSlot(int slot)
    {
        int slotOffset = GetSlotOffset(slot);
        for (int index = 0; index < SlotByteCount; index++)
            WriteSramByte(slotOffset + index, 0);
        WriteSramWord(SaveRamLayout.PrimaryChecksumOffset + slot * 2, 0);
        WriteSramWord(SaveRamLayout.PrimaryComplementOffset + slot * 2, 0);
        WriteSramWord(SaveRamLayout.BackupChecksumOffset + slot * 2, 0);
        WriteSramWord(SaveRamLayout.BackupComplementOffset + slot * 2, 0);
    }

    /// <summary>Reads the persistent menu selection, falling back to slot A if corrupt.</summary>
    public int ReadSelectedSlot()
    {
        ushort slot = ReadSramWord(SelectedSlotOffset);
        ushort complement = ReadSramWord(SelectedSlotOffset + 2);
        return slot < SlotCount && unchecked((ushort)~slot) == complement ? slot : 0;
    }

    private ushort CalculateChecksum(int slotOffset)
    {
        ushort checksum = 0;
        for (int offset = 0; offset < SlotByteCount; offset += 2)
            checksum = unchecked((ushort)(checksum + ReadSramWord(slotOffset + offset)));
        return checksum;
    }

    private void CopyDirectoryWord(int directoryOffset, int sourceSlot, int destinationSlot) =>
        WriteSramWord(
            directoryOffset + destinationSlot * 2,
            ReadSramWord(directoryOffset + sourceSlot * 2));

    private static int GetSlotOffset(int slot)
    {
        ValidateSlot(slot);
        return SaveRamLayout.SlotOffsets[slot];
    }

    private static void ValidateSlot(int slot)
    {
        if ((uint)slot >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Save slot must be A, B, or C (0-2).");
    }

    private byte ReadSramByte(int offset) =>
        bus.ReadByte((int)new SnesAddress(0x70, (ushort)(offset & 0x1fff)));

    private ushort ReadSramWord(int offset) => unchecked((ushort)(
        ReadSramByte(offset) | (ReadSramByte(offset + 1) << 8)));

    private byte[] ReadSramBytes(int offset, int count)
    {
        var bytes = new byte[count];
        for (int index = 0; index < count; index++)
            bytes[index] = ReadSramByte(offset + index);
        return bytes;
    }

    private void WriteSramByte(int offset, byte value) =>
        bus.WriteByte((int)new SnesAddress(0x70, (ushort)(offset & 0x1fff)), value);

    private void WriteSramWord(int offset, ushort value)
    {
        WriteSramByte(offset, unchecked((byte)value));
        WriteSramByte(offset + 1, unchecked((byte)(value >> 8)));
    }

    private byte[] PackExploredMap(ReadOnlySpan<byte> exploredMap)
    {
        int expectedByteCount =
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea;
        if (exploredMap.Length != expectedByteCount)
        {
            throw new InvalidDataException(
                $"A save snapshot requires exactly {expectedByteCount} unpacked explored-map bytes.");
        }

        var compressed = new byte[SaveRamLayout.CompressedMapDataByteCount];
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            int count = bus.ReadByte(
                (int)SaveRamLayout.PackedMapByteCountTable.AddWithinBank(area));
            int destination = ReadBusWord(
                SaveRamLayout.PackedMapDestinationOffsetTable.AddWithinBank(area * 2));
            ushort sourceIndexPointer = ReadBusWord(
                SaveRamLayout.PackedMapSourceIndexPointerTable.AddWithinBank(area * 2));
            for (int index = 0; index < count; index++)
            {
                int compressedIndex = destination + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("ROM packed-map table escapes the $500-byte SRAM field.");
                int areaByteIndex = bus.ReadByte(
                    (int)new SnesAddress(
                        SaveRamLayout.PackedMapSourceIndexPointerTable.Bank,
                        unchecked((ushort)(sourceIndexPointer + index))));
                compressed[compressedIndex] = exploredMap[
                    area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndex];
            }
        }
        return compressed;
    }

    private byte[] UnpackExploredMap(ReadOnlySpan<byte> compressed)
    {
        if (compressed.Length != SaveRamLayout.CompressedMapDataByteCount)
            throw new ArgumentException("Compressed map payload must contain exactly $500 bytes.", nameof(compressed));

        var explored = new byte[
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            int count = bus.ReadByte(
                (int)SaveRamLayout.PackedMapByteCountTable.AddWithinBank(area));
            int source = ReadBusWord(
                SaveRamLayout.PackedMapDestinationOffsetTable.AddWithinBank(area * 2));
            ushort destinationIndexPointer = ReadBusWord(
                SaveRamLayout.PackedMapSourceIndexPointerTable.AddWithinBank(area * 2));
            for (int index = 0; index < count; index++)
            {
                int compressedIndex = source + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("ROM packed-map table escapes the $500-byte SRAM field.");
                int areaByteIndex = bus.ReadByte(
                    (int)new SnesAddress(
                        SaveRamLayout.PackedMapSourceIndexPointerTable.Bank,
                        unchecked((ushort)(destinationIndexPointer + index))));
                explored[area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndex] =
                    compressed[compressedIndex];
            }
        }
        return explored;
    }

    private ushort ReadBusWord(SnesAddress source)
    {
        return unchecked((ushort)(
            bus.ReadByte((int)source) |
            (bus.ReadByte((int)source.AddWithinBank(1)) << 8)));
    }

    private static void WriteWord(Span<byte> destination, int offset, ushort value)
    {
        destination[offset] = unchecked((byte)value);
        destination[offset + 1] = unchecked((byte)(value >> 8));
    }
}

/// <summary>Decoded debugger view of the player fields and checkpoint in one valid slot.</summary>
public sealed record SuperMetroidSaveSlot(
    int Slot,
    ushort EquippedItems,
    ushort CollectedItems,
    ushort EquippedBeams,
    ushort CollectedBeams,
    ushort ReserveMode,
    ushort Health,
    ushort MaxHealth,
    ushort Missiles,
    ushort MaxMissiles,
    ushort SuperMissiles,
    ushort MaxSuperMissiles,
    ushort PowerBombs,
    ushort MaxPowerBombs,
    ushort HudItem,
    ushort MaxReserveEnergy,
    ushort ReserveEnergy,
    ushort GameTimeFrames,
    ushort GameTimeSeconds,
    ushort GameTimeMinutes,
    ushort GameTimeHours,
    byte[] EventBytes,
    byte[] BossBytes,
    byte[] RoomChozoBytes,
    byte[] CollectedItemBytes,
    byte[] OpenedDoorBytes,
    byte[] UsedSaveStationBytes,
    byte[] MapStationBytes,
    byte[] ExploredMapBytes,
    ushort SaveStation,
    ushort Area)
{
    /// <summary>The checksum-backed seven-action controller permutation.</summary>
    public ControllerBindings ControllerBindings { get; init; } = ControllerBindings.Default;

    /// <summary>Saved nonzero WRAM word <c>$09E4</c>.</summary>
    public bool MoonwalkEnabled { get; init; }

    /// <summary>Checksummed cartridge word at WRAM <c>$09E6</c>.</summary>
    public ushort DebugFlag { get; init; }

    /// <summary>Checksummed new-file marker at WRAM <c>$09E8</c>.</summary>
    public ushort NewFileMarker { get; init; }

    /// <summary>Saved nonzero WRAM word <c>$09EA</c>.</summary>
    public bool IconCancelEnabled { get; init; }

    /// <summary>Creates the complete translated snapshot accepted by the SRAM encoder.</summary>
    public SuperMetroidSaveSnapshot ToSnapshot() => new()
    {
        ControllerBindings = ControllerBindings,
        MoonwalkEnabled = MoonwalkEnabled,
        DebugFlag = DebugFlag,
        NewFileMarker = NewFileMarker,
        IconCancelEnabled = IconCancelEnabled,
        EquippedItems = EquippedItems,
        CollectedItems = CollectedItems,
        EquippedBeams = EquippedBeams,
        CollectedBeams = CollectedBeams,
        ReserveMode = ReserveMode,
        Health = Health,
        MaxHealth = MaxHealth,
        Missiles = Missiles,
        MaxMissiles = MaxMissiles,
        SuperMissiles = SuperMissiles,
        MaxSuperMissiles = MaxSuperMissiles,
        PowerBombs = PowerBombs,
        MaxPowerBombs = MaxPowerBombs,
        HudItem = HudItem,
        MaxReserveEnergy = MaxReserveEnergy,
        ReserveEnergy = ReserveEnergy,
        GameTimeFrames = GameTimeFrames,
        GameTimeSeconds = GameTimeSeconds,
        GameTimeMinutes = GameTimeMinutes,
        GameTimeHours = GameTimeHours,
        SaveStation = SaveStation,
        Area = Area,
        EventBytes = EventBytes.ToArray(),
        BossBytes = BossBytes.ToArray(),
        RoomChozoBytes = RoomChozoBytes.ToArray(),
        CollectedItemBytes = CollectedItemBytes.ToArray(),
        OpenedDoorBytes = OpenedDoorBytes.ToArray(),
        UsedSaveStationBytes = UsedSaveStationBytes.ToArray(),
        MapStationBytes = MapStationBytes.ToArray(),
        ExploredMapBytes = ExploredMapBytes.ToArray(),
    };

    /// <summary>Restores the subset already represented by the translated Samus owner.</summary>
    public void ApplyTo(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        samus.EquippedItems = EquippedItems;
        samus.CollectedItems = CollectedItems;
        samus.EquippedBeams = EquippedBeams;
        samus.CollectedBeams = CollectedBeams;
        samus.ReserveTankMode = ReserveMode;
        samus.Health = Health;
        samus.MaxHealth = MaxHealth;
        samus.Missiles = Missiles;
        samus.MaxMissiles = MaxMissiles;
        samus.SuperMissiles = SuperMissiles;
        samus.MaxSuperMissiles = MaxSuperMissiles;
        samus.PowerBombs = PowerBombs;
        samus.MaxPowerBombs = MaxPowerBombs;
        samus.SelectedHudItem = HudItem;
        samus.MaxReserveEnergy = MaxReserveEnergy;
        samus.ReserveEnergy = ReserveEnergy;
    }

    /// <summary>Restores player inventory and all progression bytes represented in SRAM.</summary>
    public void ApplyTo(SamusState samus, Bank80SystemState system)
    {
        ApplyTo(samus);
        ArgumentNullException.ThrowIfNull(system);
        system.LoadEventBytes(EventBytes);
        system.LoadBossBytes(BossBytes);
        system.LoadRoomChozoBytes(RoomChozoBytes);
        system.LoadCollectedItemBytes(CollectedItemBytes);
        system.LoadOpenedDoorBytes(OpenedDoorBytes);
        system.LoadUsedSaveStationBytes(UsedSaveStationBytes);
        system.LoadMapStationBytes(MapStationBytes);
        system.LoadExploredMapBytes(ExploredMapBytes);
    }
}

/// <summary>Domain snapshot supplied to the native SRAM encoder.</summary>
public sealed record SuperMetroidSaveSnapshot
{
    public ControllerBindings ControllerBindings { get; init; } = ControllerBindings.Default;
    public bool MoonwalkEnabled { get; init; }
    /// <summary>Checksummed cartridge word at WRAM <c>$09E6</c>.</summary>
    public ushort DebugFlag { get; init; } = 1;
    /// <summary>Checksummed new-file marker at WRAM <c>$09E8</c>.</summary>
    public ushort NewFileMarker { get; init; } = 1;
    public bool IconCancelEnabled { get; init; }
    public ushort EquippedItems { get; init; }
    public ushort CollectedItems { get; init; }
    public ushort EquippedBeams { get; init; }
    public ushort CollectedBeams { get; init; }
    public ushort ReserveMode { get; init; }
    public ushort Health { get; init; } = 99;
    public ushort MaxHealth { get; init; } = 99;
    public ushort Missiles { get; init; }
    public ushort MaxMissiles { get; init; }
    public ushort SuperMissiles { get; init; }
    public ushort MaxSuperMissiles { get; init; }
    public ushort PowerBombs { get; init; }
    public ushort MaxPowerBombs { get; init; }
    public ushort HudItem { get; init; }
    public ushort MaxReserveEnergy { get; init; }
    public ushort ReserveEnergy { get; init; }
    public ushort GameTimeFrames { get; init; }
    public ushort GameTimeSeconds { get; init; }
    public ushort GameTimeMinutes { get; init; }
    public ushort GameTimeHours { get; init; }
    public ushort SaveStation { get; init; }
    public ushort Area { get; init; }
    public byte[] EventBytes { get; init; } = new byte[Bank80SystemState.EventByteCount];
    public byte[] BossBytes { get; init; } = new byte[Bank80SystemState.AreaCount];
    public byte[] RoomChozoBytes { get; init; } =
        new byte[Bank80SystemState.RoomChozoBitByteCount];
    public byte[] CollectedItemBytes { get; init; } =
        new byte[Bank80SystemState.ItemBitByteCount];
    public byte[] OpenedDoorBytes { get; init; } =
        new byte[Bank80SystemState.DoorBitByteCount];
    public byte[] UsedSaveStationBytes { get; init; } =
        new byte[Bank80SystemState.UsedSaveStationByteCount];
    public byte[] MapStationBytes { get; init; } =
        new byte[Bank80SystemState.MapStationByteCount];
    public byte[] ExploredMapBytes { get; init; } = new byte[
        Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];

    public static SuperMetroidSaveSnapshot Capture(
        SamusState samus,
        Bank80SystemState system,
        ushort area,
        ushort saveStation,
        GameTimeState? gameTime = null,
        ControllerBindings? controllerBindings = null,
        bool moonwalkEnabled = false,
        bool iconCancelEnabled = false)
    {
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(system);
        var events = new byte[Bank80SystemState.EventByteCount];
        var bosses = new byte[Bank80SystemState.AreaCount];
        var roomChozo = new byte[Bank80SystemState.RoomChozoBitByteCount];
        var collectedItems = new byte[Bank80SystemState.ItemBitByteCount];
        var openedDoors = new byte[Bank80SystemState.DoorBitByteCount];
        var usedSaveStations = new byte[Bank80SystemState.UsedSaveStationByteCount];
        var mapStations = new byte[Bank80SystemState.MapStationByteCount];
        var exploredMap = new byte[
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int index = 0; index < events.Length; index++)
            events[index] = system.GetEventByteRaw(index);
        for (int index = 0; index < bosses.Length; index++)
            bosses[index] = system.GetBossBitsRaw(index);
        for (int index = 0; index < roomChozo.Length; index++)
            roomChozo[index] = system.GetRoomChozoByteRaw(index);
        for (int index = 0; index < collectedItems.Length; index++)
            collectedItems[index] = system.GetCollectedItemByteRaw(index);
        for (int index = 0; index < openedDoors.Length; index++)
            openedDoors[index] = system.GetOpenedDoorByteRaw(index);
        for (int index = 0; index < usedSaveStations.Length; index++)
            usedSaveStations[index] = system.GetUsedSaveStationByteRaw(index);
        for (int index = 0; index < mapStations.Length; index++)
            mapStations[index] = system.GetMapStationByteRaw(index);
        for (int areaIndex = 0; areaIndex < Bank80SystemState.ExploredMapAreaCount; areaIndex++)
        {
            for (int byteIndex = 0;
                byteIndex < Bank80SystemState.ExploredMapBytesPerArea;
                byteIndex++)
            {
                exploredMap[areaIndex * Bank80SystemState.ExploredMapBytesPerArea + byteIndex] =
                    system.GetExploredMapByteRaw(areaIndex, byteIndex);
            }
        }

        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = (controllerBindings ?? ControllerBindings.Default)
                .RequireRetailPermutation(),
            MoonwalkEnabled = moonwalkEnabled,
            IconCancelEnabled = iconCancelEnabled,
            EquippedItems = samus.EquippedItems,
            CollectedItems = samus.CollectedItems,
            EquippedBeams = samus.EquippedBeams,
            CollectedBeams = samus.CollectedBeams,
            ReserveMode = samus.ReserveTankMode,
            Health = samus.Health,
            MaxHealth = samus.MaxHealth,
            Missiles = samus.Missiles,
            MaxMissiles = samus.MaxMissiles,
            SuperMissiles = samus.SuperMissiles,
            MaxSuperMissiles = samus.MaxSuperMissiles,
            PowerBombs = samus.PowerBombs,
            MaxPowerBombs = samus.MaxPowerBombs,
            HudItem = samus.SelectedHudItem,
            MaxReserveEnergy = samus.MaxReserveEnergy,
            ReserveEnergy = samus.ReserveEnergy,
            GameTimeFrames = gameTime?.Frames ?? 0,
            GameTimeSeconds = gameTime?.Seconds ?? 0,
            GameTimeMinutes = gameTime?.Minutes ?? 0,
            GameTimeHours = gameTime?.Hours ?? 0,
            SaveStation = saveStation,
            Area = area,
            EventBytes = events,
            BossBytes = bosses,
            RoomChozoBytes = roomChozo,
            CollectedItemBytes = collectedItems,
            OpenedDoorBytes = openedDoors,
            UsedSaveStationBytes = usedSaveStations,
            MapStationBytes = mapStations,
            ExploredMapBytes = exploredMap,
        };
    }
}
