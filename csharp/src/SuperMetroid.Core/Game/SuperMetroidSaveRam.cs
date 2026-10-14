using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-compatible owner of Super Metroid's three battery-backed save slots.
/// </summary>
/// <remarks>
/// <c>$81:8000-$81:812A</c> stores one contiguous WRAM mirror ($D7C0-$DE1B), then writes
/// both a checksum and its complement into two redundant SRAM directories. Keeping that
/// physical layout remains the in-memory cartridge ABI for legacy emulator-save imports;
/// durable JSON stores meaningful state and rebuilds padding and checksum directories.
/// </remarks>
[CpuBusBoundary("codec: native SRAM save slots, checksum directories and completion marker ($81:8000-$81:812A)")]
public sealed class SuperMetroidSaveRam
{
    /// <summary>The retail format's fixed number of checksum-protected save slots.</summary>
    public const int SlotCount = SaveRamLayout.SlotCount;

    /// <summary>The byte length of one native slot payload, including reserved layout space.</summary>
    public const int SlotByteCount = SaveRamLayout.SlotByteCount;

    /// <summary>The global SRAM offset of the selected-slot word used by the file-select menu.</summary>
    public const int SelectedSlotOffset = SaveRamLayout.SelectedSlotOffset;

    /// <summary>Stock area-map rules bound to this codec for packed explored-map translation.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Mutable address space containing the three cartridge-format SRAM slots.</summary>
    private readonly ISnesMemoryBus bus;

    /// <summary>Creates an SRAM codec over a mutable SNES address space.</summary>
    /// <param name="bus">Address space that supplies cartridge SRAM reads and writes.</param>
    /// <param name="mapPresentation">Optional area-map metadata used to translate explored-map bits.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not expose mutable memory.</exception>
    public SuperMetroidSaveRam(ISnesAddressSpace bus, AreaMapPresentationCatalog? mapPresentation)
    {
        this.mapPresentation = mapPresentation;
        Ensure.NotNull(bus);
        this.bus = bus as ISnesMemoryBus ??
            throw new ArgumentException("Save slots require an SRAM-capable address space.", nameof(bus));
    }

    /// <summary>Provides bounded byte and word access to the native SRAM image.</summary>
    private SramBytes Image => new(bus, SaveRamLayout.Image);

    /// <summary>Installs map metadata after restoring a codec that did not serialize its catalog.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? maps) => mapPresentation = maps;

    /// <summary>
    /// <c>LoadFromSRAM</c>'s corruption test: either redundant checksum pair matches the
    /// slot's contents. This reads no decoded fields, so it needs no map presentation.
    /// </summary>
    public bool IsSlotValid(int slot)
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
        return primaryValid || backupValid;
    }

    /// <summary>Returns a decoded slot only when either redundant checksum pair is valid.</summary>
    public SuperMetroidSaveSlot? ReadSlot(int slot)
    {
        if (!IsSlotValid(slot))
            return null;
        int slotOffset = GetSlotOffset(slot);

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
            LoadingGameState: ReadSramWord(
                slotOffset + SaveRamLayout.LoadingGameStateOffset),
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
            ReserveMissiles = ReadSramWord(slotOffset + SaveRamLayout.ReserveMissilesOffset),
            JapaneseText = ReadSramWord(slotOffset + SaveRamLayout.JapaneseTextOffset) != 0,
            LoadedItemCount = ReadSramWord(slotOffset + SaveRamLayout.LoadedItemCountOffset),
        };
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c> for the translated state currently owned by C# objects.
    /// The slot payload and four directory words are byte-for-byte cartridge structures.
    /// </summary>
    public void SaveSlot(int slot, SuperMetroidSaveSnapshot snapshot)
    {
        Ensure.NotNull(snapshot);
        int slotOffset = GetSlotOffset(slot);
        byte[] payload = new byte[SlotByteCount];

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
        WriteWord(payload, SaveRamLayout.ReserveMissilesOffset, snapshot.ReserveMissiles);
        WriteWord(payload, SaveRamLayout.JapaneseTextOffset, snapshot.JapaneseText ? (ushort)1 : (ushort)0);
        WriteWord(payload, SaveRamLayout.LoadedItemCountOffset, snapshot.LoadedItemCount);
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
        WriteWord(payload, SaveRamLayout.LoadingGameStateOffset, snapshot.LoadingGameState);
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

    /// <summary>
    /// The carry <c>DetermineNumberOfDemoSets</c> ($80:8261) returns at boot, which
    /// <c>MainGameLoop</c> then preserves (<see cref="Bank80SystemState.MainGameLoopCarry"/>).
    /// When all three slots fail <c>LoadFromSRAM</c>, its final carry-set return survives the
    /// corrupt-marker loop. Otherwise the completion-marker words are compared from the last
    /// down: a full match leaves the final comparison's carry set, and the first mismatch
    /// leaves the unsigned comparison of that SRAM word against the marker word.
    /// </summary>
    public bool DetermineBootMainLoopCarry()
    {
        if (!IsSlotValid(0) && !IsSlotValid(1) && !IsSlotValid(2))
            return true;
        ReadOnlySpan<byte> marker = SaveRamLayout.CompletionMarker;
        for (int offset = marker.Length - 2; offset >= 0; offset -= 2)
        {
            ushort sram = ReadSramWord(SaveRamLayout.CompletionMarkerOffset + offset);
            ushort text = (ushort)(SnesLittleEndian.Word(marker[offset], marker[offset + 1]));
            if (sram != text)
                return sram >= text;
        }
        return true;
    }

    /// <summary>
    /// Boot's <c>DetermineNumberOfDemoSets</c> ($80:8261), called once from $80:8564. When all
    /// three slots fail <c>LoadFromSRAM</c> it overwrites the completion marker with
    /// <see cref="SaveRamLayout.CorruptSramMarker"/> ($80:8283-8290) and enables three demo
    /// sets; otherwise the full completion marker enables the fourth. The cartridge never
    /// re-evaluates the count before reset.
    /// </summary>
    /// <returns>The attract-demo set count fixed for the session.</returns>
    public int DetermineNumberOfDemoSets()
    {
        if (!IsSlotValid(0) && !IsSlotValid(1) && !IsSlotValid(2))
        {
            ReadOnlySpan<byte> corrupt = SaveRamLayout.CorruptSramMarker;
            for (int index = 0; index < corrupt.Length; index++)
                WriteSramByte(SaveRamLayout.CompletionMarkerOffset + index, corrupt[index]);
            return AttractDemoRomData.DefaultSetCount;
        }
        return HasCompletedGame ? AttractDemoRomData.SetCount : AttractDemoRomData.DefaultSetCount;
    }

    /// <summary>True for the native completed-game signature, independent of slot checksums.</summary>
    public bool HasCompletedGame => ReadSramBytes(SaveRamLayout.CompletionMarkerOffset,
        SaveRamLayout.CompletionMarker.Length).SequenceEqual(SaveRamLayout.CompletionMarker);

    /// <summary>Encodes named completion state without retaining arbitrary signature bytes.</summary>
    public void SetGameCompleted(bool completed)
    {
        for (int index = 0; index < SaveRamLayout.CompletionMarker.Length; index++)
            WriteSramByte(SaveRamLayout.CompletionMarkerOffset + index,
                completed ? SaveRamLayout.CompletionMarker[index] : (byte)0);
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

    /// <summary>
    /// Reads the persistent menu selection with the cartridge's validity rule
    /// (<c>$81:9B0C-9B24</c>, <c>$81:9ED8-9EF0</c>): the stored word must be a signed
    /// 0-2 and share no bits with its complement word; otherwise slot A is selected.
    /// The complement need not be the exact inverse, so imported noncanonical SRAM keeps
    /// the slot the cartridge would preselect.
    /// </summary>
    public int ReadSelectedSlot()
    {
        ushort slot = ReadSramWord(SelectedSlotOffset);
        ushort complement = ReadSramWord(SelectedSlotOffset + 2);
        // Unsigned "slot < SlotCount" also rejects the negative words the BMI excludes.
        return slot < SlotCount && (slot & complement) == 0 ? slot : 0;
    }

    /// <summary>Sums the slot's native little-endian words with 16-bit wraparound.</summary>
    private ushort CalculateChecksum(int slotOffset)
    {
        ushort checksum = 0;
        for (int offset = 0; offset < SlotByteCount; offset += 2)
            checksum = unchecked((ushort)(checksum + ReadSramWord(slotOffset + offset)));
        return checksum;
    }

    /// <summary>Copies one checksum or complement entry between slots in an SRAM directory.</summary>
    private void CopyDirectoryWord(int directoryOffset, int sourceSlot, int destinationSlot) =>
        WriteSramWord(
            directoryOffset + destinationSlot * 2,
            ReadSramWord(directoryOffset + sourceSlot * 2));

    /// <summary>Validates a file-select slot and locates its native SRAM payload.</summary>
    private static int GetSlotOffset(int slot)
    {
        ValidateSlot(slot);
        return SaveRamLayout.SlotOffset(slot);
    }

    /// <summary>Rejects slot numbers outside the cartridge's A, B, and C directories.</summary>
    private static void ValidateSlot(int slot)
    {
        if ((uint)slot >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Save slot must be A, B, or C (0-2).");
    }

    /// <summary>Reads one byte at an offset in the bounded SRAM image.</summary>
    private byte ReadSramByte(int offset) => Image.Read(offset);

    /// <summary>Reads one little-endian word at an offset in the bounded SRAM image.</summary>
    private ushort ReadSramWord(int offset) => Image.ReadWord(offset);

    /// <summary>Copies a contiguous SRAM field into a detached byte array.</summary>
    private byte[] ReadSramBytes(int offset, int count)
    {
        var bytes = new byte[count];
        for (int index = 0; index < count; index++)
            bytes[index] = ReadSramByte(offset + index);
        return bytes;
    }

    /// <summary>Writes one byte at an offset in the bounded SRAM image.</summary>
    private void WriteSramByte(int offset, byte value) => Image.Write(offset, value);

    /// <summary>Writes one little-endian word at an offset in the bounded SRAM image.</summary>
    private void WriteSramWord(int offset, ushort value) => Image.WriteWord(offset, value);

    /// <summary>Packs each area's occupied explored-map bytes into the cartridge's fixed SRAM field.</summary>
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
            ExploredMapPackingDefinition definition =
                ExploredMapPackingDefinitions.Area(area, mapPresentation ??
                    throw new InvalidOperationException("Save-map packing requires bound immutable stock map rules."));
            ExploredMapPackingDefinitions.OccupiedByteIndexes areaByteIndexes = definition.AreaByteIndexes;
            for (int index = 0; index < areaByteIndexes.Count; index++)
            {
                int compressedIndex = definition.DestinationOffset + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("Compiled packed-map table escapes the $500-byte SRAM field.");
                int areaByteIndex = areaByteIndexes[index];
                compressed[compressedIndex] = exploredMap[
                    area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndex];
            }
        }
        return compressed;
    }

    /// <summary>Expands the packed SRAM map field into area-major explored-map bytes using stock map rules.</summary>
    private byte[] UnpackExploredMap(ReadOnlySpan<byte> compressed)
    {
        if (compressed.Length != SaveRamLayout.CompressedMapDataByteCount)
            throw new ArgumentException("Compressed map payload must contain exactly $500 bytes.", nameof(compressed));

        var explored = new byte[
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int area = 0; area < SaveRamLayout.PackedMapAreaCount; area++)
        {
            ExploredMapPackingDefinition definition =
                ExploredMapPackingDefinitions.Area(area, mapPresentation ??
                    throw new InvalidOperationException("Save-map packing requires bound immutable stock map rules."));
            ExploredMapPackingDefinitions.OccupiedByteIndexes areaByteIndexes = definition.AreaByteIndexes;
            for (int index = 0; index < areaByteIndexes.Count; index++)
            {
                int compressedIndex = definition.DestinationOffset + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("Compiled packed-map table escapes the $500-byte SRAM field.");
                int areaByteIndex = areaByteIndexes[index];
                explored[area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndex] =
                    compressed[compressedIndex];
            }
        }
        return explored;
    }

    /// <summary>Encodes a native word into a detached slot buffer in SNES byte order.</summary>
    private static void WriteWord(Span<byte> destination, int offset, ushort value)
    {
        destination[offset] = unchecked((byte)value);
        destination[offset + 1] = SnesLittleEndian.HighByte(value);
    }
}
