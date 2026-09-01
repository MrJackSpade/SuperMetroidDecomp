using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-compatible owner of Super Metroid's three battery-backed save slots.
/// </summary>
/// <remarks>
/// <c>$81:8000-$81:812A</c> stores one contiguous WRAM mirror ($D7C0-$DE1B), then writes
/// both a checksum and its complement into two redundant SRAM directories. Keeping that
/// physical layout means the desktop-produced <c>.srm</c> is inspectable by ordinary SNES
/// tools instead of being a private C# serialization format.
/// </remarks>
public sealed class SuperMetroidSaveRam
{
    public const int SlotCount = 3;
    public const int SlotByteCount = 0x065c;
    public const int SelectedSlotOffset = 0x1fec;

    private static readonly ushort[] SlotOffsets = [0x0010, 0x066c, 0x0cc8];

    private const int PrimaryChecksumOffset = 0x0000;
    private const int PrimaryComplementOffset = 0x0008;
    private const int BackupChecksumOffset = 0x1ff0;
    private const int BackupComplementOffset = 0x1ff8;

    // Offsets within the copied $D7C0-$DE1B save mirror.
    private const int EquippedItemsOffset = 0x0000;
    private const int CollectedItemsOffset = 0x0002;
    private const int EquippedBeamsOffset = 0x0004;
    private const int CollectedBeamsOffset = 0x0006;
    private const int ButtonConfigOffset = 0x0008;
    private const int ReserveModeOffset = 0x001e;
    private const int HealthOffset = 0x0020;
    private const int MaxHealthOffset = 0x0022;
    private const int MissilesOffset = 0x0024;
    private const int MaxMissilesOffset = 0x0026;
    private const int SuperMissilesOffset = 0x0028;
    private const int MaxSuperMissilesOffset = 0x002a;
    private const int PowerBombsOffset = 0x002c;
    private const int MaxPowerBombsOffset = 0x002e;
    private const int HudItemOffset = 0x0030;
    private const int MaxReserveEnergyOffset = 0x0032;
    private const int ReserveEnergyOffset = 0x0034;
    private const int GameTimeFramesOffset = 0x0038;
    private const int GameTimeSecondsOffset = 0x003a;
    private const int GameTimeMinutesOffset = 0x003c;
    private const int GameTimeHoursOffset = 0x003e;
    private const int MoonwalkOffset = 0x0042;
    private const int DebugFlagOffset = 0x0044;
    private const int NewFileMarkerOffset = 0x0046;
    private const int IconCancelOffset = 0x0048;
    private const int EventsOffset = 0x0060;
    private const int BossBitsOffset = 0x0068;
    private const int RoomChozoBitsOffset = 0x0070;
    private const int CollectedItemBitsOffset = 0x00b0;
    private const int OpenedDoorBitsOffset = 0x00f0;
    private const int UsedSaveStationsOffset = 0x0138;
    private const int MapStationsOffset = 0x0148;
    private const int SaveStationOffset = 0x0156;
    private const int AreaOffset = 0x0158;
    private const int CompressedMapDataOffset = 0x015c;
    private const int CompressedMapDataByteCount = 0x0500;

    // Bank-$81 table addresses consumed by PackMapToSave/UnpackMapFromSave. The count and
    // packed-offset tables describe six SRAM-backed areas; each unpacked-offset pointer
    // selects a cartridge list of byte indexes within that area's 256-byte bit plane.
    private const int PackedMapByteCountTable = 0x818131;
    private const int PackedMapDestinationOffsetTable = 0x818138;
    private const int PackedMapSourceIndexPointerTable = 0x8182d6;

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
            ReadSramWord(PrimaryChecksumOffset + slot * 2) == checksum &&
            ReadSramWord(PrimaryComplementOffset + slot * 2) == complement;
        bool backupValid =
            ReadSramWord(BackupChecksumOffset + slot * 2) == checksum &&
            ReadSramWord(BackupComplementOffset + slot * 2) == complement;
        if (!primaryValid && !backupValid)
            return null;

        return new SuperMetroidSaveSlot(
            Slot: slot,
            EquippedItems: ReadSramWord(slotOffset + EquippedItemsOffset),
            CollectedItems: ReadSramWord(slotOffset + CollectedItemsOffset),
            EquippedBeams: ReadSramWord(slotOffset + EquippedBeamsOffset),
            CollectedBeams: ReadSramWord(slotOffset + CollectedBeamsOffset),
            ReserveMode: ReadSramWord(slotOffset + ReserveModeOffset),
            Health: ReadSramWord(slotOffset + HealthOffset),
            MaxHealth: ReadSramWord(slotOffset + MaxHealthOffset),
            Missiles: ReadSramWord(slotOffset + MissilesOffset),
            MaxMissiles: ReadSramWord(slotOffset + MaxMissilesOffset),
            SuperMissiles: ReadSramWord(slotOffset + SuperMissilesOffset),
            MaxSuperMissiles: ReadSramWord(slotOffset + MaxSuperMissilesOffset),
            PowerBombs: ReadSramWord(slotOffset + PowerBombsOffset),
            MaxPowerBombs: ReadSramWord(slotOffset + MaxPowerBombsOffset),
            HudItem: ReadSramWord(slotOffset + HudItemOffset),
            MaxReserveEnergy: ReadSramWord(slotOffset + MaxReserveEnergyOffset),
            ReserveEnergy: ReadSramWord(slotOffset + ReserveEnergyOffset),
            GameTimeFrames: ReadSramWord(slotOffset + GameTimeFramesOffset),
            GameTimeSeconds: ReadSramWord(slotOffset + GameTimeSecondsOffset),
            GameTimeMinutes: ReadSramWord(slotOffset + GameTimeMinutesOffset),
            GameTimeHours: ReadSramWord(slotOffset + GameTimeHoursOffset),
            EventBytes: ReadSramBytes(
                slotOffset + EventsOffset,
                Bank80SystemState.EventByteCount),
            BossBytes: ReadSramBytes(
                slotOffset + BossBitsOffset,
                Bank80SystemState.AreaCount),
            RoomChozoBytes: ReadSramBytes(
                slotOffset + RoomChozoBitsOffset,
                Bank80SystemState.RoomChozoBitByteCount),
            CollectedItemBytes: ReadSramBytes(
                slotOffset + CollectedItemBitsOffset,
                Bank80SystemState.ItemBitByteCount),
            OpenedDoorBytes: ReadSramBytes(
                slotOffset + OpenedDoorBitsOffset,
                Bank80SystemState.DoorBitByteCount),
            UsedSaveStationBytes: ReadSramBytes(
                slotOffset + UsedSaveStationsOffset,
                Bank80SystemState.UsedSaveStationByteCount),
            MapStationBytes: ReadSramBytes(
                slotOffset + MapStationsOffset,
                Bank80SystemState.MapStationByteCount),
            ExploredMapBytes: UnpackExploredMap(ReadSramBytes(
                slotOffset + CompressedMapDataOffset,
                CompressedMapDataByteCount)),
            SaveStation: ReadSramWord(slotOffset + SaveStationOffset),
            Area: ReadSramWord(slotOffset + AreaOffset))
        {
            // The first four words are the immutable D-pad directions. The remaining
            // seven are laid out by WRAM address rather than by the options-screen row
            // order: cancel precedes select and aim-down precedes aim-up in the save mirror.
            ControllerBindings = new ControllerBindings(
                Shoot: ReadSramWord(slotOffset + ButtonConfigOffset + 8),
                Jump: ReadSramWord(slotOffset + ButtonConfigOffset + 10),
                Dash: ReadSramWord(slotOffset + ButtonConfigOffset + 12),
                ItemSelect: ReadSramWord(slotOffset + ButtonConfigOffset + 16),
                ItemCancel: ReadSramWord(slotOffset + ButtonConfigOffset + 14),
                AimUp: ReadSramWord(slotOffset + ButtonConfigOffset + 20),
                AimDown: ReadSramWord(slotOffset + ButtonConfigOffset + 18)).OrDefault(),
            MoonwalkEnabled = ReadSramWord(slotOffset + MoonwalkOffset) != 0,
            IconCancelEnabled = ReadSramWord(slotOffset + IconCancelOffset) != 0,
        };
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c> for the translated state currently owned by C# objects.
    /// The slot payload and four directory words are byte-for-byte cartridge structures.
    /// </summary>
    public void SaveSlot(int slot, SuperMetroidSaveSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        int slotOffset = GetSlotOffset(slot);
        var payload = new byte[SlotByteCount];

        WriteWord(payload, EquippedItemsOffset, snapshot.EquippedItems);
        WriteWord(payload, CollectedItemsOffset, snapshot.CollectedItems);
        WriteWord(payload, EquippedBeamsOffset, snapshot.EquippedBeams);
        WriteWord(payload, CollectedBeamsOffset, snapshot.CollectedBeams);

        // NewSaveFile at $81:B2CB installs these eleven literal SNES controller words.
        // Later saves copy the live configurable action words, so do not silently restore
        // defaults every time the player reaches a save station.
        ControllerBindings bindings = snapshot.ControllerBindings.OrDefault();
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
            WriteWord(payload, ButtonConfigOffset + index * 2, buttons[index]);

        WriteWord(payload, ReserveModeOffset, snapshot.ReserveMode);
        WriteWord(payload, HealthOffset, snapshot.Health);
        WriteWord(payload, MaxHealthOffset, snapshot.MaxHealth);
        WriteWord(payload, MissilesOffset, snapshot.Missiles);
        WriteWord(payload, MaxMissilesOffset, snapshot.MaxMissiles);
        WriteWord(payload, SuperMissilesOffset, snapshot.SuperMissiles);
        WriteWord(payload, MaxSuperMissilesOffset, snapshot.MaxSuperMissiles);
        WriteWord(payload, PowerBombsOffset, snapshot.PowerBombs);
        WriteWord(payload, MaxPowerBombsOffset, snapshot.MaxPowerBombs);
        WriteWord(payload, HudItemOffset, snapshot.HudItem);
        WriteWord(payload, MaxReserveEnergyOffset, snapshot.MaxReserveEnergy);
        WriteWord(payload, ReserveEnergyOffset, snapshot.ReserveEnergy);
        WriteWord(payload, GameTimeFramesOffset, snapshot.GameTimeFrames);
        WriteWord(payload, GameTimeSecondsOffset, snapshot.GameTimeSeconds);
        WriteWord(payload, GameTimeMinutesOffset, snapshot.GameTimeMinutes);
        WriteWord(payload, GameTimeHoursOffset, snapshot.GameTimeHours);
        WriteWord(payload, MoonwalkOffset, snapshot.MoonwalkEnabled ? (ushort)1 : (ushort)0);
        // NewSaveFile deliberately initializes both of these otherwise obscure words to
        // one before the intro's first save. They are part of the checksummed 96-byte copy.
        WriteWord(payload, DebugFlagOffset, 1);
        WriteWord(payload, NewFileMarkerOffset, 1);
        WriteWord(payload, IconCancelOffset, snapshot.IconCancelEnabled ? (ushort)1 : (ushort)0);

        if (snapshot.EventBytes.Length != Bank80SystemState.EventByteCount)
            throw new InvalidDataException("A save snapshot requires exactly eight event bytes.");
        if (snapshot.BossBytes.Length != Bank80SystemState.AreaCount)
            throw new InvalidDataException("A save snapshot requires exactly eight area-boss bytes.");
        snapshot.EventBytes.CopyTo(payload, EventsOffset);
        snapshot.BossBytes.CopyTo(payload, BossBitsOffset);
        if (snapshot.RoomChozoBytes.Length != Bank80SystemState.RoomChozoBitByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 64 room-Chozo bytes.");
        }
        snapshot.RoomChozoBytes.CopyTo(payload, RoomChozoBitsOffset);
        if (snapshot.CollectedItemBytes.Length != Bank80SystemState.ItemBitByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 64 collected-item bytes.");
        }
        snapshot.CollectedItemBytes.CopyTo(payload, CollectedItemBitsOffset);
        if (snapshot.OpenedDoorBytes.Length != Bank80SystemState.DoorBitByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 64 opened-door bytes.");
        }
        snapshot.OpenedDoorBytes.CopyTo(payload, OpenedDoorBitsOffset);
        if (snapshot.UsedSaveStationBytes.Length != Bank80SystemState.UsedSaveStationByteCount)
        {
            throw new InvalidDataException(
                "A save snapshot requires exactly 16 used save/elevator bytes.");
        }
        snapshot.UsedSaveStationBytes.CopyTo(payload, UsedSaveStationsOffset);
        if (snapshot.MapStationBytes.Length != Bank80SystemState.MapStationByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 12 map-station bytes.");
        snapshot.MapStationBytes.CopyTo(payload, MapStationsOffset);
        WriteWord(payload, SaveStationOffset, snapshot.SaveStation);
        WriteWord(payload, AreaOffset, snapshot.Area);
        byte[] compressedMap = PackExploredMap(snapshot.ExploredMapBytes);
        compressedMap.CopyTo(payload, CompressedMapDataOffset);

        for (int index = 0; index < payload.Length; index++)
            WriteSramByte(slotOffset + index, payload[index]);

        ushort checksum = CalculateChecksum(slotOffset);
        ushort complement = unchecked((ushort)~checksum);
        WriteSramWord(PrimaryChecksumOffset + slot * 2, checksum);
        WriteSramWord(PrimaryComplementOffset + slot * 2, complement);
        WriteSramWord(BackupChecksumOffset + slot * 2, checksum);
        WriteSramWord(BackupComplementOffset + slot * 2, complement);
    }

    /// <summary>Stores the menu's selected-slot word and complement at $1FEC/$1FEE.</summary>
    public void SelectSlot(int slot)
    {
        ValidateSlot(slot);
        WriteSramWord(SelectedSlotOffset, unchecked((ushort)slot));
        WriteSramWord(SelectedSlotOffset + 2, unchecked((ushort)~slot));
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

    private static int GetSlotOffset(int slot)
    {
        ValidateSlot(slot);
        return SlotOffsets[slot];
    }

    private static void ValidateSlot(int slot)
    {
        if ((uint)slot >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Save slot must be A, B, or C (0-2).");
    }

    private byte ReadSramByte(int offset) => bus.ReadByte(0x700000 | (offset & 0x1fff));

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
        bus.WriteByte(0x700000 | (offset & 0x1fff), value);

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

        var compressed = new byte[CompressedMapDataByteCount];
        for (int area = 0; area < 6; area++)
        {
            int count = bus.ReadByte(PackedMapByteCountTable + area);
            int destination = ReadBusWord(PackedMapDestinationOffsetTable + area * 2);
            ushort sourceIndexPointer = ReadBusWord(PackedMapSourceIndexPointerTable + area * 2);
            for (int index = 0; index < count; index++)
            {
                int compressedIndex = destination + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("ROM packed-map table escapes the $500-byte SRAM field.");
                int areaByteIndex = bus.ReadByte(0x810000 | ((sourceIndexPointer + index) & 0xffff));
                compressed[compressedIndex] = exploredMap[
                    area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndex];
            }
        }
        return compressed;
    }

    private byte[] UnpackExploredMap(ReadOnlySpan<byte> compressed)
    {
        if (compressed.Length != CompressedMapDataByteCount)
            throw new ArgumentException("Compressed map payload must contain exactly $500 bytes.", nameof(compressed));

        var explored = new byte[
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int area = 0; area < 6; area++)
        {
            int count = bus.ReadByte(PackedMapByteCountTable + area);
            int source = ReadBusWord(PackedMapDestinationOffsetTable + area * 2);
            ushort destinationIndexPointer = ReadBusWord(PackedMapSourceIndexPointerTable + area * 2);
            for (int index = 0; index < count; index++)
            {
                int compressedIndex = source + index;
                if ((uint)compressedIndex >= compressed.Length)
                    throw new InvalidDataException("ROM packed-map table escapes the $500-byte SRAM field.");
                int areaByteIndex = bus.ReadByte(
                    0x810000 | ((destinationIndexPointer + index) & 0xffff));
                explored[area * Bank80SystemState.ExploredMapBytesPerArea + areaByteIndex] =
                    compressed[compressedIndex];
            }
        }
        return explored;
    }

    private ushort ReadBusWord(int address) => unchecked((ushort)(
        bus.ReadByte(address) | (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));

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

    /// <summary>Saved nonzero WRAM word <c>$09EA</c>.</summary>
    public bool IconCancelEnabled { get; init; }

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
            ControllerBindings = (controllerBindings ?? ControllerBindings.Default).OrDefault(),
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
