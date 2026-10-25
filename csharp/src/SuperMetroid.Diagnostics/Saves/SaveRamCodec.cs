using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The cartridge's 8-KiB SRAM image as a byte codec (#1322): legacy emulator saves, replay reset headers and recorder
/// seeds carry saves in this format, and <see cref="SaveSlotStore"/> is decoded from and encoded to it at those
/// boundaries only, by the hosts. It is the encoding owner of the SRAM image layout <see cref="SaveRamLayout"/> describes.
/// </summary>
/// <remarks>
/// <c>$81:8000-$81:812A</c> stores one contiguous WRAM mirror ($D7C0-$DE1B) per slot, then writes both a checksum and
/// its complement into two redundant directories. Decoding applies the cartridge's validity, selected-slot and
/// boot-carry rules to the image as given; encoding writes the canonical image of a store, without padding or
/// noncanonical marker bytes.
/// <para>
/// The encoding owned here: the little-endian slot words at the <see cref="SaveRamLayout"/> offsets,
/// the controller bindings stored as controller-button words, the equipment, beam and per-area boss flags stored as
/// their words and bytes, the checksum directories, the selected-slot pair and the marker field. It is the one place
/// a save's domain values become SRAM words and SRAM words become domain values; every other reader of an SRAM image,
/// including the legacy JSON upgrade, goes through it.
/// </para>
/// </remarks>
[EncodingOwner("legacy 8-KiB save RAM image")]
public static class SaveRamCodec
{
    /// <summary>Decodes every slot, the selected slot, completion and the boot carry from a whole SRAM image.</summary>
    /// <param name="image">The exact 8-KiB SRAM image.</param>
    /// <param name="maps">Installed stock map rules that unpack each valid slot's explored map.</param>
    /// <exception cref="ArgumentException"><paramref name="image"/> is not exactly 8 KiB.</exception>
    public static SaveSlotStore Decode(ReadOnlySpan<byte> image, IAreaMapSource maps)
    {
        Ensure.LengthEqual(image, SaveRamLayout.ImageByteCount);
        Ensure.NotNull(maps);
        var slots = new SuperMetroidSaveSlot?[SaveRamLayout.SlotCount];
        for (int slot = 0; slot < slots.Length; slot++)
            slots[slot] = IsSlotValid(image, slot) ? DecodeSlot(image, slot, maps) : null;
        return SaveSlotStore.Imported(slots, ReadSelectedSlot(image), HasCompletedGame(image), HasCorruptMarker(image),
            MarkerCarry(image.Slice(SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length)), maps);
    }

    /// <summary>Encodes the canonical SRAM image of <paramref name="store"/>.</summary>
    /// <param name="store">The saves to encode.</param>
    /// <param name="maps">Installed stock map rules that pack each slot's explored map.</param>
    public static byte[] Encode(SaveSlotStore store, IAreaMapSource maps)
    {
        Ensure.NotNull(store);
        Ensure.NotNull(maps);
        var image = new byte[SaveRamLayout.ImageByteCount];
        for (int slot = 0; slot < SaveRamLayout.SlotCount; slot++)
        {
            if (store.ReadSlot(slot) is { } saved)
                WriteSlot(image, slot, EncodePayload(ToSnapshot(saved), maps));
        }
        WriteSelectedSlot(image, store.ReadSelectedSlot());
        if (store.CorruptMarkerWritten)
            WriteMarker(image, SaveRamLayout.CorruptSramMarker);
        else
            WriteCompletion(image, store.HasCompletedGame);
        return image;
    }

    /// <summary>
    /// <c>LoadFromSRAM</c>'s corruption test: either redundant checksum pair matches the slot's contents. This reads
    /// no decoded fields, so it needs no map rules.
    /// </summary>
    internal static bool IsSlotValid(ReadOnlySpan<byte> image, int slot)
    {
        ushort checksum = Checksum(Payload(image, slot));
        ushort complement = unchecked((ushort)~checksum);
        bool primaryValid =
            ReadWord(image, SaveRamLayout.PrimaryChecksumOffset + slot * 2) == checksum &&
            ReadWord(image, SaveRamLayout.PrimaryComplementOffset + slot * 2) == complement;
        bool backupValid =
            ReadWord(image, SaveRamLayout.BackupChecksumOffset + slot * 2) == checksum &&
            ReadWord(image, SaveRamLayout.BackupComplementOffset + slot * 2) == complement;
        return primaryValid || backupValid;
    }

    /// <summary>Decodes the payload of <paramref name="slot"/> in <paramref name="image"/>, whatever its checksums.</summary>
    internal static SuperMetroidSaveSlot DecodeSlot(ReadOnlySpan<byte> image, int slot, IAreaMapSource? maps) =>
        DecodePayload(Payload(image, slot), slot, maps);

    /// <summary>Decodes one native slot payload into the slot record <see cref="SaveSlotStore"/> holds.</summary>
    internal static SuperMetroidSaveSlot DecodePayload(ReadOnlySpan<byte> payload, int slot, IAreaMapSource? maps)
    {
        Ensure.LengthEqual(payload, SaveRamLayout.SlotByteCount);
        return new SuperMetroidSaveSlot(
            Slot: slot,
            EquippedItems: SamusEquipmentWordCodec.FromEquipmentWord(ReadWord(payload, SaveRamLayout.EquippedItemsOffset)),
            CollectedItems: SamusEquipmentWordCodec.FromEquipmentWord(ReadWord(payload, SaveRamLayout.CollectedItemsOffset)),
            EquippedBeams: (SamusBeamFlags)ReadWord(payload, SaveRamLayout.EquippedBeamsOffset),
            CollectedBeams: (SamusBeamFlags)ReadWord(payload, SaveRamLayout.CollectedBeamsOffset),
            ReserveMode: ReserveTankModes.Decode(ReadWord(payload, SaveRamLayout.ReserveModeOffset)),
            Health: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.HealthOffset)),
            MaxHealth: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.MaxHealthOffset)),
            Missiles: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.MissilesOffset)),
            MaxMissiles: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.MaxMissilesOffset)),
            SuperMissiles: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.SuperMissilesOffset)),
            MaxSuperMissiles: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.MaxSuperMissilesOffset)),
            PowerBombs: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.PowerBombsOffset)),
            MaxPowerBombs: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.MaxPowerBombsOffset)),
            HudItem: HudItems.Decode(ReadWord(payload, SaveRamLayout.HudItemOffset)),
            MaxReserveEnergy: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.MaxReserveEnergyOffset)),
            ReserveEnergy: SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.ReserveEnergyOffset)),
            GameTimeFrames: ReadWord(payload, SaveRamLayout.GameTimeFramesOffset),
            GameTimeSeconds: ReadWord(payload, SaveRamLayout.GameTimeSecondsOffset),
            GameTimeMinutes: ReadWord(payload, SaveRamLayout.GameTimeMinutesOffset),
            GameTimeHours: ReadWord(payload, SaveRamLayout.GameTimeHoursOffset),
            EventBytes: payload.Slice(SaveRamLayout.EventsOffset, SharedGameState.EventByteCount).ToArray(),
            BossBytes: payload.Slice(SaveRamLayout.BossBitsOffset, SharedGameState.AreaCount).ToArray(),
            RoomChozoBytes: payload.Slice(SaveRamLayout.RoomChozoBitsOffset, SharedGameState.RoomChozoBitByteCount).ToArray(),
            CollectedItemBytes: payload.Slice(SaveRamLayout.CollectedItemBitsOffset, SharedGameState.ItemBitByteCount).ToArray(),
            OpenedDoorBytes: payload.Slice(SaveRamLayout.OpenedDoorBitsOffset, SharedGameState.DoorBitByteCount).ToArray(),
            UsedSaveStationBytes: payload.Slice(SaveRamLayout.UsedSaveStationsOffset, SharedGameState.UsedSaveStationByteCount).ToArray(),
            MapStationBytes: payload.Slice(SaveRamLayout.MapStationsOffset, SharedGameState.MapStationByteCount).ToArray(),
            ExploredMapBytes: ExploredMapStorage.Unpack(
                payload.Slice(SaveRamLayout.CompressedMapDataOffset, SaveRamLayout.CompressedMapDataByteCount), maps),
            LoadingGameState: ReadWord(payload, SaveRamLayout.LoadingGameStateOffset),
            SaveStation: ReadWord(payload, SaveRamLayout.SaveStationOffset),
            Area: ReadWord(payload, SaveRamLayout.AreaOffset))
        {
            ControllerBindings = ReadControllerBindings(payload).RequireRetailPermutation(),
            MoonwalkEnabled = ReadWord(payload, SaveRamLayout.MoonwalkOffset) != 0,
            DebugFlag = ReadWord(payload, SaveRamLayout.DebugFlagOffset),
            NewFileMarker = ReadWord(payload, SaveRamLayout.NewFileMarkerOffset),
            IconCancelEnabled = ReadWord(payload, SaveRamLayout.IconCancelOffset) != 0,
            ReserveMissiles = SamusQuantity.Decode(ReadWord(payload, SaveRamLayout.ReserveMissilesOffset)),
            JapaneseText = ReadWord(payload, SaveRamLayout.JapaneseTextOffset) != 0,
            LoadedItemCount = ReadWord(payload, SaveRamLayout.LoadedItemCountOffset),
        };
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c>'s payload for the translated state a snapshot holds. The payload is a byte-for-byte
    /// cartridge structure; <see cref="WriteSlot"/> adds its directory words.
    /// </summary>
    internal static byte[] EncodePayload(SuperMetroidSaveSnapshot snapshot, IAreaMapSource? maps)
    {
        Ensure.NotNull(snapshot);
        byte[] payload = new byte[SaveRamLayout.SlotByteCount];

        WriteWord(payload, SaveRamLayout.EquippedItemsOffset, snapshot.EquippedItems.ToEquipmentWord());
        WriteWord(payload, SaveRamLayout.CollectedItemsOffset, snapshot.CollectedItems.ToEquipmentWord());
        WriteWord(payload, SaveRamLayout.EquippedBeamsOffset, snapshot.EquippedBeams.ToEquipmentWord());
        WriteWord(payload, SaveRamLayout.CollectedBeamsOffset, snapshot.CollectedBeams.ToEquipmentWord());

        // NewSaveFile at $81:B2CB installs these eleven literal SNES controller words.
        // Later saves copy the live configurable action words, so do not silently restore
        // defaults every time the player reaches a save station.
        ControllerBindings bindings = snapshot.ControllerBindings.RequireRetailPermutation();
        ushort[] buttons = ButtonConfigurationWords(bindings);
        for (int index = 0; index < buttons.Length; index++)
            WriteWord(payload, SaveRamLayout.ButtonConfigOffset + index * 2, buttons[index]);

        WriteWord(payload, SaveRamLayout.ReserveModeOffset, snapshot.ReserveMode.ToWord());
        WriteWord(payload, SaveRamLayout.HealthOffset, SamusQuantity.Encode(snapshot.Health));
        WriteWord(payload, SaveRamLayout.MaxHealthOffset, SamusQuantity.Encode(snapshot.MaxHealth));
        WriteWord(payload, SaveRamLayout.MissilesOffset, SamusQuantity.Encode(snapshot.Missiles));
        WriteWord(payload, SaveRamLayout.MaxMissilesOffset, SamusQuantity.Encode(snapshot.MaxMissiles));
        WriteWord(payload, SaveRamLayout.SuperMissilesOffset, SamusQuantity.Encode(snapshot.SuperMissiles));
        WriteWord(payload, SaveRamLayout.MaxSuperMissilesOffset, SamusQuantity.Encode(snapshot.MaxSuperMissiles));
        WriteWord(payload, SaveRamLayout.PowerBombsOffset, SamusQuantity.Encode(snapshot.PowerBombs));
        WriteWord(payload, SaveRamLayout.MaxPowerBombsOffset, SamusQuantity.Encode(snapshot.MaxPowerBombs));
        WriteWord(payload, SaveRamLayout.HudItemOffset, snapshot.HudItem.ToWord());
        WriteWord(payload, SaveRamLayout.MaxReserveEnergyOffset, SamusQuantity.Encode(snapshot.MaxReserveEnergy));
        WriteWord(payload, SaveRamLayout.ReserveEnergyOffset, SamusQuantity.Encode(snapshot.ReserveEnergy));
        WriteWord(payload, SaveRamLayout.ReserveMissilesOffset, SamusQuantity.Encode(snapshot.ReserveMissiles));
        WriteWord(payload, SaveRamLayout.JapaneseTextOffset, snapshot.JapaneseText ? (ushort)1 : (ushort)0);
        WriteWord(payload, SaveRamLayout.LoadedItemCountOffset,
            SaveFieldValue.Encode(snapshot.LoadedItemCount, nameof(snapshot.LoadedItemCount)));
        WriteWord(payload, SaveRamLayout.GameTimeFramesOffset,
            SaveFieldValue.Encode(snapshot.GameTimeFrames, nameof(snapshot.GameTimeFrames)));
        WriteWord(payload, SaveRamLayout.GameTimeSecondsOffset,
            SaveFieldValue.Encode(snapshot.GameTimeSeconds, nameof(snapshot.GameTimeSeconds)));
        WriteWord(payload, SaveRamLayout.GameTimeMinutesOffset,
            SaveFieldValue.Encode(snapshot.GameTimeMinutes, nameof(snapshot.GameTimeMinutes)));
        WriteWord(payload, SaveRamLayout.GameTimeHoursOffset,
            SaveFieldValue.Encode(snapshot.GameTimeHours, nameof(snapshot.GameTimeHours)));
        WriteWord(payload, SaveRamLayout.MoonwalkOffset, snapshot.MoonwalkEnabled ? (ushort)1 : (ushort)0);
        WriteWord(payload, SaveRamLayout.DebugFlagOffset, SaveFieldValue.Encode(snapshot.DebugFlag, nameof(snapshot.DebugFlag)));
        WriteWord(payload, SaveRamLayout.NewFileMarkerOffset,
            SaveFieldValue.Encode(snapshot.NewFileMarker, nameof(snapshot.NewFileMarker)));
        WriteWord(payload, SaveRamLayout.IconCancelOffset, snapshot.IconCancelEnabled ? (ushort)1 : (ushort)0);

        if (snapshot.EventBytes.Length != SharedGameState.EventByteCount)
            throw new InvalidDataException("A save snapshot requires exactly eight event bytes.");
        if (snapshot.BossFlags.Length != SharedGameState.AreaCount)
            throw new InvalidDataException("A save snapshot requires exactly eight area-boss flag sets.");
        snapshot.EventBytes.CopyTo(payload, SaveRamLayout.EventsOffset);
        for (int area = 0; area < snapshot.BossFlags.Length; area++)
            payload[SaveRamLayout.BossBitsOffset + area] = (byte)snapshot.BossFlags[area];
        if (snapshot.RoomChozoBytes.Length != SharedGameState.RoomChozoBitByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 64 room-Chozo bytes.");
        snapshot.RoomChozoBytes.CopyTo(payload, SaveRamLayout.RoomChozoBitsOffset);
        if (snapshot.CollectedItemBytes.Length != SharedGameState.ItemBitByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 64 collected-item bytes.");
        snapshot.CollectedItemBytes.CopyTo(payload, SaveRamLayout.CollectedItemBitsOffset);
        if (snapshot.OpenedDoorBytes.Length != SharedGameState.DoorBitByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 64 opened-door bytes.");
        snapshot.OpenedDoorBytes.CopyTo(payload, SaveRamLayout.OpenedDoorBitsOffset);
        if (snapshot.UsedSaveStationBytes.Length != SharedGameState.UsedSaveStationByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 16 used save/elevator bytes.");
        snapshot.UsedSaveStationBytes.CopyTo(payload, SaveRamLayout.UsedSaveStationsOffset);
        if (snapshot.MapStationBytes.Length != SharedGameState.MapStationByteCount)
            throw new InvalidDataException("A save snapshot requires exactly 12 map-station bytes.");
        snapshot.MapStationBytes.CopyTo(payload, SaveRamLayout.MapStationsOffset);
        WriteWord(payload, SaveRamLayout.LoadingGameStateOffset,
            SaveFieldValue.Encode(snapshot.LoadingGameState, nameof(snapshot.LoadingGameState)));
        WriteWord(payload, SaveRamLayout.SaveStationOffset, SaveFieldValue.Encode(snapshot.SaveStation, nameof(snapshot.SaveStation)));
        WriteWord(payload, SaveRamLayout.AreaOffset, SaveFieldValue.Encode(snapshot.Area, nameof(snapshot.Area)));
        ExploredMapStorage.Pack(snapshot.ExploredMapBytes, maps).CopyTo(payload, SaveRamLayout.CompressedMapDataOffset);
        return payload;
    }

    /// <summary>The snapshot that re-encodes <paramref name="slot"/> to its own payload.</summary>
    internal static SuperMetroidSaveSnapshot ToSnapshot(SuperMetroidSaveSlot slot) => new()
    {
        ControllerBindings = slot.ControllerBindings,
        MoonwalkEnabled = slot.MoonwalkEnabled,
        DebugFlag = slot.DebugFlag,
        NewFileMarker = slot.NewFileMarker,
        IconCancelEnabled = slot.IconCancelEnabled,
        ReserveMissiles = slot.ReserveMissiles,
        JapaneseText = slot.JapaneseText,
        LoadedItemCount = slot.LoadedItemCount,
        EquippedItems = slot.EquippedItems,
        CollectedItems = slot.CollectedItems,
        EquippedBeams = slot.EquippedBeams,
        CollectedBeams = slot.CollectedBeams,
        ReserveMode = slot.ReserveMode,
        Health = slot.Health,
        MaxHealth = slot.MaxHealth,
        Missiles = slot.Missiles,
        MaxMissiles = slot.MaxMissiles,
        SuperMissiles = slot.SuperMissiles,
        MaxSuperMissiles = slot.MaxSuperMissiles,
        PowerBombs = slot.PowerBombs,
        MaxPowerBombs = slot.MaxPowerBombs,
        HudItem = slot.HudItem,
        MaxReserveEnergy = slot.MaxReserveEnergy,
        ReserveEnergy = slot.ReserveEnergy,
        GameTimeFrames = slot.GameTimeFrames,
        GameTimeSeconds = slot.GameTimeSeconds,
        GameTimeMinutes = slot.GameTimeMinutes,
        GameTimeHours = slot.GameTimeHours,
        LoadingGameState = slot.LoadingGameState,
        SaveStation = slot.SaveStation,
        Area = slot.Area,
        EventBytes = slot.EventBytes.ToArray(),
        BossFlags = slot.BossBytes.Select(value => (BossBits)value).ToArray(),
        RoomChozoBytes = slot.RoomChozoBytes.ToArray(),
        CollectedItemBytes = slot.CollectedItemBytes.ToArray(),
        OpenedDoorBytes = slot.OpenedDoorBytes.ToArray(),
        UsedSaveStationBytes = slot.UsedSaveStationBytes.ToArray(),
        MapStationBytes = slot.MapStationBytes.ToArray(),
        ExploredMapBytes = slot.ExploredMapBytes.ToArray(),
    };

    /// <summary>Writes <paramref name="payload"/> into <paramref name="slot"/> with both redundant checksum pairs.</summary>
    internal static void WriteSlot(Span<byte> image, int slot, ReadOnlySpan<byte> payload)
    {
        Ensure.LengthEqual(payload, SaveRamLayout.SlotByteCount);
        payload.CopyTo(image.Slice(SaveRamLayout.SlotOffset(slot), SaveRamLayout.SlotByteCount));
        ushort checksum = Checksum(payload);
        WriteDirectory(image, slot, checksum, unchecked((ushort)~checksum));
    }

    /// <summary>Writes one slot's four checksum-directory words.</summary>
    internal static void WriteDirectory(Span<byte> image, int slot, ushort checksum, ushort complement)
    {
        WriteWord(image, SaveRamLayout.PrimaryChecksumOffset + slot * 2, checksum);
        WriteWord(image, SaveRamLayout.PrimaryComplementOffset + slot * 2, complement);
        WriteWord(image, SaveRamLayout.BackupChecksumOffset + slot * 2, checksum);
        WriteWord(image, SaveRamLayout.BackupComplementOffset + slot * 2, complement);
    }

    /// <summary>
    /// The carry of comparing the marker field's words with the completion signature's, from the last down: a full
    /// match leaves the final comparison's carry set, and the first mismatch leaves the unsigned comparison.
    /// </summary>
    internal static bool MarkerCarry(ReadOnlySpan<byte> field)
    {
        ReadOnlySpan<byte> marker = SaveRamLayout.CompletionMarker;
        Ensure.LengthEqual(field, marker.Length);
        for (int offset = marker.Length - 2; offset >= 0; offset -= 2)
        {
            ushort sram = ReadWord(field, offset);
            ushort text = SnesLittleEndian.Word(marker[offset], marker[offset + 1]);
            if (sram != text)
                return sram >= text;
        }
        return true;
    }

    /// <summary>True for the native completed-game signature, independent of slot checksums.</summary>
    internal static bool HasCompletedGame(ReadOnlySpan<byte> image) =>
        image.Slice(SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length)
            .SequenceEqual(SaveRamLayout.CompletionMarker);

    /// <summary>Whether the marker field holds the corrupt-SRAM signature boot writes when every slot is invalid.</summary>
    internal static bool HasCorruptMarker(ReadOnlySpan<byte> image) =>
        image.Slice(SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CorruptSramMarker.Length)
            .SequenceEqual(SaveRamLayout.CorruptSramMarker);

    /// <summary>Writes the completion signature, or zeros, over the marker field.</summary>
    internal static void WriteCompletion(Span<byte> image, bool completed)
    {
        Span<byte> field = image.Slice(SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length);
        if (completed)
            SaveRamLayout.CompletionMarker.CopyTo(field);
        else
            field.Clear();
    }

    /// <summary>Writes <paramref name="marker"/> over the completion-marker field.</summary>
    internal static void WriteMarker(Span<byte> image, ReadOnlySpan<byte> marker) =>
        marker.CopyTo(image.Slice(SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length));

    /// <summary>
    /// The persistent menu selection under the cartridge's validity rule (<c>$81:9B0C-9B24</c>,
    /// <c>$81:9ED8-9EF0</c>): the stored word must be a signed 0-2 and share no bits with its complement word;
    /// otherwise slot A is selected. The complement need not be the exact inverse, so imported noncanonical SRAM keeps
    /// the slot the cartridge would preselect.
    /// </summary>
    internal static int ReadSelectedSlot(ReadOnlySpan<byte> image)
    {
        ushort slot = ReadWord(image, SaveRamLayout.SelectedSlotOffset);
        ushort complement = ReadWord(image, SaveRamLayout.SelectedSlotOffset + 2);
        // Unsigned "slot < SlotCount" also rejects the negative words the BMI excludes.
        return slot < SaveRamLayout.SlotCount && (slot & complement) == 0 ? slot : 0;
    }

    /// <summary>Stores the menu's selected-slot word and complement at $1FEC/$1FEE.</summary>
    internal static void WriteSelectedSlot(Span<byte> image, int slot)
    {
        ValidateSlot(slot);
        WriteWord(image, SaveRamLayout.SelectedSlotOffset, unchecked((ushort)slot));
        WriteWord(image, SaveRamLayout.SelectedSlotOffset + 2, unchecked((ushort)~slot));
    }

    /// <summary>Rejects slot numbers outside the cartridge's A, B, and C directories.</summary>
    internal static void ValidateSlot(int slot)
    {
        if ((uint)slot >= SaveRamLayout.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "Save slot must be A, B, or C (0-2).");
    }

    /// <summary>
    /// The word at <paramref name="offset"/> within <paramref name="slot"/>'s payload, whatever the slot's checksums:
    /// the legacy JSON upgrade reads the fields schema one omitted from its preserved image.
    /// </summary>
    internal static ushort ReadSlotWord(ReadOnlySpan<byte> image, int slot, int offset)
    {
        Ensure.LengthEqual(image, SaveRamLayout.ImageByteCount);
        return ReadWord(Payload(image, slot), offset);
    }

    /// <summary>One slot's native payload within the image.</summary>
    private static ReadOnlySpan<byte> Payload(ReadOnlySpan<byte> image, int slot)
    {
        ValidateSlot(slot);
        return image.Slice(SaveRamLayout.SlotOffset(slot), SaveRamLayout.SlotByteCount);
    }

    /// <summary>Sums the payload's native little-endian words with 16-bit wraparound.</summary>
    private static ushort Checksum(ReadOnlySpan<byte> payload)
    {
        ushort checksum = 0;
        for (int offset = 0; offset < payload.Length; offset += 2)
            checksum = unchecked((ushort)(checksum + ReadWord(payload, offset)));
        return checksum;
    }

    /// <summary>
    /// A slot's seven configurable bindings. They follow the four immutable D-pad words and are laid out by WRAM
    /// address rather than by the options-screen row order: cancel precedes select and aim-down precedes aim-up.
    /// The words are taken unchecked so <see cref="ControllerBindings.RequireRetailPermutation"/> reports a corrupt
    /// permutation verbatim.
    /// </summary>
    [ButtonWordEncoding("SRAM stores each binding as its controller word")]
    private static ControllerBindings ReadControllerBindings(ReadOnlySpan<byte> payload) => new(
        Shoot: (ControllerButton)ReadWord(payload, SaveRamLayout.ShootButtonOffset),
        Jump: (ControllerButton)ReadWord(payload, SaveRamLayout.JumpButtonOffset),
        Dash: (ControllerButton)ReadWord(payload, SaveRamLayout.DashButtonOffset),
        ItemSelect: (ControllerButton)ReadWord(payload, SaveRamLayout.SelectButtonOffset),
        ItemCancel: (ControllerButton)ReadWord(payload, SaveRamLayout.CancelButtonOffset),
        AimUp: (ControllerButton)ReadWord(payload, SaveRamLayout.AimUpButtonOffset),
        AimDown: (ControllerButton)ReadWord(payload, SaveRamLayout.AimDownButtonOffset));

    /// <summary>The eleven controller words a slot stores: the four D-pad directions, then the bindings in WRAM order.</summary>
    [ButtonWordEncoding("SRAM stores the button configuration as controller words")]
    private static ushort[] ButtonConfigurationWords(ControllerBindings bindings) =>
    [
        (ushort)ControllerButton.Up,
        (ushort)ControllerButton.Down,
        (ushort)ControllerButton.Left,
        (ushort)ControllerButton.Right,
        (ushort)bindings.Shoot,
        (ushort)bindings.Jump,
        (ushort)bindings.Dash,
        (ushort)bindings.ItemCancel,
        (ushort)bindings.ItemSelect,
        (ushort)bindings.AimDown,
        (ushort)bindings.AimUp,
    ];

    /// <summary>Reads one little-endian word.</summary>
    private static ushort ReadWord(ReadOnlySpan<byte> bytes, int offset) => SnesLittleEndian.Word(bytes[offset], bytes[offset + 1]);

    /// <summary>Writes one word in SNES byte order.</summary>
    private static void WriteWord(Span<byte> destination, int offset, ushort value)
    {
        destination[offset] = unchecked((byte)value);
        destination[offset + 1] = SnesLittleEndian.HighByte(value);
    }
}
