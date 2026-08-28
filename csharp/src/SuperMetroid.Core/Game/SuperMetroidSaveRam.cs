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
    private const int DebugFlagOffset = 0x0044;
    private const int NewFileMarkerOffset = 0x0046;
    private const int EventsOffset = 0x0060;
    private const int BossBitsOffset = 0x0068;
    private const int SaveStationOffset = 0x0156;
    private const int AreaOffset = 0x0158;

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
            SaveStation: ReadSramWord(slotOffset + SaveStationOffset),
            Area: ReadSramWord(slotOffset + AreaOffset));
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
        ushort[] buttons =
        [
            (ushort)SnesButton.Up,
            (ushort)SnesButton.Down,
            (ushort)SnesButton.Left,
            (ushort)SnesButton.Right,
            (ushort)SnesButton.X,
            (ushort)SnesButton.A,
            (ushort)SnesButton.B,
            (ushort)SnesButton.Y,
            (ushort)SnesButton.Select,
            (ushort)SnesButton.L,
            (ushort)SnesButton.R,
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
        // NewSaveFile deliberately initializes both of these otherwise obscure words to
        // one before the intro's first save. They are part of the checksummed 96-byte copy.
        WriteWord(payload, DebugFlagOffset, 1);
        WriteWord(payload, NewFileMarkerOffset, 1);

        if (snapshot.EventBytes.Length != Bank80SystemState.EventByteCount)
            throw new InvalidDataException("A save snapshot requires exactly eight event bytes.");
        if (snapshot.BossBytes.Length != Bank80SystemState.AreaCount)
            throw new InvalidDataException("A save snapshot requires exactly eight area-boss bytes.");
        snapshot.EventBytes.CopyTo(payload, EventsOffset);
        snapshot.BossBytes.CopyTo(payload, BossBitsOffset);
        WriteWord(payload, SaveStationOffset, snapshot.SaveStation);
        WriteWord(payload, AreaOffset, snapshot.Area);

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

    private void WriteSramByte(int offset, byte value) =>
        bus.WriteByte(0x700000 | (offset & 0x1fff), value);

    private void WriteSramWord(int offset, ushort value)
    {
        WriteSramByte(offset, unchecked((byte)value));
        WriteSramByte(offset + 1, unchecked((byte)(value >> 8)));
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
    ushort SaveStation,
    ushort Area)
{
    /// <summary>Restores the subset already represented by the translated Samus owner.</summary>
    public void ApplyTo(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        samus.EquippedItems = EquippedItems;
        samus.EquippedBeams = EquippedBeams;
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
}

/// <summary>Domain snapshot supplied to the native SRAM encoder.</summary>
public sealed record SuperMetroidSaveSnapshot
{
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

    public static SuperMetroidSaveSnapshot Capture(
        SamusState samus,
        Bank80SystemState system,
        ushort area,
        ushort saveStation)
    {
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(system);
        var events = new byte[Bank80SystemState.EventByteCount];
        var bosses = new byte[Bank80SystemState.AreaCount];
        for (int index = 0; index < events.Length; index++)
            events[index] = system.GetEventByteRaw(index);
        for (int index = 0; index < bosses.Length; index++)
            bosses[index] = system.GetBossBitsRaw(index);

        return new SuperMetroidSaveSnapshot
        {
            EquippedItems = samus.EquippedItems,
            CollectedItems = samus.EquippedItems,
            EquippedBeams = samus.EquippedBeams,
            CollectedBeams = samus.EquippedBeams,
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
            SaveStation = saveStation,
            Area = area,
            EventBytes = events,
            BossBytes = bosses,
        };
    }
}
