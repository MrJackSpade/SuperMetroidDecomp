using SuperMetroid.Diagnostics.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The cartridge's 8-KiB SRAM image as a byte codec (#1322): legacy emulator saves, replay reset headers and recorder
/// seeds carry saves in this format, and <see cref="SaveSlotStore"/> is decoded from and encoded to it at those
/// boundaries only, by the hosts. It is the encoding owner of the SRAM image layout <see cref="SaveRamLayout"/> describes.
/// </summary>
/// <remarks>
/// <c>$81:8000-$81:812A</c> stores one contiguous WRAM mirror ($D7C0-$DE1B) per slot, then writes both a checksum and
/// its complement into two redundant directories. Decoding applies the cartridge's validity and selected-slot rules to
/// the image as given, and decides from its raw completion mark the earthquake rumble delay power-on gives
/// (<see cref="PowerOnEarthquakeSoundDelay"/>); progress is read through <see cref="ProgressionTableCodec"/>, which keeps
/// what the game can observe. Encoding writes the canonical image of a store, without padding or noncanonical marker
/// bytes, and with the words the game itself writes in the fields no retail routine reads.
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
    /// <exception cref="InvalidDataException">A valid slot's clock words are not a reading of the game clock.</exception>
    public static SaveSlotStore Decode(ReadOnlySpan<byte> image, IAreaMapSource maps)
    {
        Ensure.LengthEqual(image, SaveRamLayout.ImageByteCount);
        Ensure.NotNull(maps);
        var slots = new SuperMetroidSaveSlot?[SaveRamLayout.SlotCount];
        for (int slot = 0; slot < slots.Length; slot++)
            slots[slot] = IsSlotValid(image, slot) ? DecodeSlot(image, slot, maps) : null;
        return SaveSlotStore.Imported(slots, ReadSelectedSlot(image), HasCompletedGame(image), HasCorruptMarker(image),
            MarkerCarry(image.Slice(SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length))
                ? PowerOnEarthquakeSoundDelay.OneFrame
                : PowerOnEarthquakeSoundDelay.None,
            maps);
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
    internal static SuperMetroidSaveSlot DecodeSlot(ReadOnlySpan<byte> image, int slot, IAreaMapSource maps) =>
        DecodePayload(Payload(image, slot), slot, maps);

    /// <summary>Decodes one native slot payload into the slot record <see cref="SaveSlotStore"/> holds.</summary>
    /// <exception cref="InvalidDataException">The payload's clock words are not a reading of the game clock.</exception>
    internal static SuperMetroidSaveSlot DecodePayload(ReadOnlySpan<byte> payload, int slot, IAreaMapSource maps)
    {
        Ensure.LengthEqual(payload, SaveRamLayout.SlotByteCount);
        int frames = ReadWord(payload, SaveRamLayout.GameTimeFramesOffset);
        int seconds = ReadWord(payload, SaveRamLayout.GameTimeSecondsOffset);
        int minutes = ReadWord(payload, SaveRamLayout.GameTimeMinutesOffset);
        int hours = ReadWord(payload, SaveRamLayout.GameTimeHoursOffset);
        // The game's clock words hold only the clock's own domain: every update rolls each component over at its unit and
        // saturates the hours, so words outside it are no save the game wrote.
        if (!GameTimeState.IsInDomain(frames, seconds, minutes, hours))
            throw new InvalidDataException($"Save slot {slot}'s clock words {hours}:{minutes}:{seconds}.{frames} are not a reading of the game clock.");
        return new SuperMetroidSaveSlot(
            Slot: slot,
            EquippedItems: SamusEquipmentWordCodec.FromSavedEquipmentWord(ReadWord(payload, SaveRamLayout.EquippedItemsOffset)),
            CollectedItems: SamusEquipmentWordCodec.FromSavedEquipmentWord(ReadWord(payload, SaveRamLayout.CollectedItemsOffset)),
            EquippedBeams: SamusEquipmentWordCodec.FromSavedBeamWord(ReadWord(payload, SaveRamLayout.EquippedBeamsOffset)),
            CollectedBeams: SamusEquipmentWordCodec.FromSavedBeamWord(ReadWord(payload, SaveRamLayout.CollectedBeamsOffset)),
            ReserveMode: SaveSlotNumbering.SavedReserveMode(ReadWord(payload, SaveRamLayout.ReserveModeOffset)),
            Health: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.HealthOffset)),
            MaxHealth: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.MaxHealthOffset)),
            Missiles: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.MissilesOffset)),
            MaxMissiles: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.MaxMissilesOffset)),
            SuperMissiles: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.SuperMissilesOffset)),
            MaxSuperMissiles: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.MaxSuperMissilesOffset)),
            PowerBombs: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.PowerBombsOffset)),
            MaxPowerBombs: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.MaxPowerBombsOffset)),
            HudItem: SaveSlotNumbering.SavedHudItem(ReadWord(payload, SaveRamLayout.HudItemOffset)),
            MaxReserveEnergy: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.MaxReserveEnergyOffset)),
            ReserveEnergy: SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.ReserveEnergyOffset)),
            GameTimeFrames: frames,
            GameTimeSeconds: seconds,
            GameTimeMinutes: minutes,
            GameTimeHours: hours,
            Progression: DecodeProgression(payload, maps),
            StartupRoute: SuperMetroid.AssetExtraction.Progression.SaveLoadingGameStateCodec.Route(ReadWord(payload, SaveRamLayout.LoadingGameStateOffset)),
            Checkpoint: LoadStationNumberCodec.Decode(
                SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(ReadWord(payload, SaveRamLayout.AreaOffset), "The save's checkpoint"),
                ReadWord(payload, SaveRamLayout.SaveStationOffset), "The save's checkpoint"))
        {
            ControllerBindings = ReadControllerBindings(payload).RequireRetailPermutation(),
            // The debug build's placement-mode word and the never-read word after it are not decoded: no retail
            // routine reads either (SaveRamLayout.DebugFlagSavedWord, NewFileMarkerSavedWord).
            MoonwalkEnabled = ReadWord(payload, SaveRamLayout.MoonwalkOffset) != 0,
            IconCancelEnabled = ReadWord(payload, SaveRamLayout.IconCancelOffset) != 0,
            ReserveMissiles = SamusCountWords.Decode(ReadWord(payload, SaveRamLayout.ReserveMissilesOffset)),
            JapaneseText = ReadWord(payload, SaveRamLayout.JapaneseTextOffset) != 0,
            LoadedItemCount = ReadWord(payload, SaveRamLayout.LoadedItemCountOffset),
        };
    }

    /// <summary>
    /// Ports <c>SaveToSRAM</c>'s payload for the translated state a snapshot holds. The payload is a byte-for-byte
    /// cartridge structure; <see cref="WriteSlot"/> adds its directory words.
    /// </summary>
    internal static byte[] EncodePayload(SuperMetroidSaveSnapshot snapshot, IAreaMapSource maps)
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

        WriteWord(payload, SaveRamLayout.ReserveModeOffset, FieldWord(snapshot.ReserveMode.SavedNumber(), nameof(snapshot.ReserveMode)));
        WriteWord(payload, SaveRamLayout.HealthOffset, SamusCountWords.Encode(snapshot.Health));
        WriteWord(payload, SaveRamLayout.MaxHealthOffset, SamusCountWords.Encode(snapshot.MaxHealth));
        WriteWord(payload, SaveRamLayout.MissilesOffset, SamusCountWords.Encode(snapshot.Missiles));
        WriteWord(payload, SaveRamLayout.MaxMissilesOffset, SamusCountWords.Encode(snapshot.MaxMissiles));
        WriteWord(payload, SaveRamLayout.SuperMissilesOffset, SamusCountWords.Encode(snapshot.SuperMissiles));
        WriteWord(payload, SaveRamLayout.MaxSuperMissilesOffset, SamusCountWords.Encode(snapshot.MaxSuperMissiles));
        WriteWord(payload, SaveRamLayout.PowerBombsOffset, SamusCountWords.Encode(snapshot.PowerBombs));
        WriteWord(payload, SaveRamLayout.MaxPowerBombsOffset, SamusCountWords.Encode(snapshot.MaxPowerBombs));
        WriteWord(payload, SaveRamLayout.HudItemOffset, FieldWord(snapshot.HudItem.SavedNumber(), nameof(snapshot.HudItem)));
        WriteWord(payload, SaveRamLayout.MaxReserveEnergyOffset, SamusCountWords.Encode(snapshot.MaxReserveEnergy));
        WriteWord(payload, SaveRamLayout.ReserveEnergyOffset, SamusCountWords.Encode(snapshot.ReserveEnergy));
        WriteWord(payload, SaveRamLayout.ReserveMissilesOffset, SamusCountWords.Encode(snapshot.ReserveMissiles));
        WriteWord(payload, SaveRamLayout.JapaneseTextOffset, snapshot.JapaneseText ? (ushort)1 : (ushort)0);
        WriteWord(payload, SaveRamLayout.LoadedItemCountOffset,
            FieldWord(snapshot.LoadedItemCount, nameof(snapshot.LoadedItemCount)));
        WriteWord(payload, SaveRamLayout.GameTimeFramesOffset,
            FieldWord(snapshot.GameTimeFrames, nameof(snapshot.GameTimeFrames)));
        WriteWord(payload, SaveRamLayout.GameTimeSecondsOffset,
            FieldWord(snapshot.GameTimeSeconds, nameof(snapshot.GameTimeSeconds)));
        WriteWord(payload, SaveRamLayout.GameTimeMinutesOffset,
            FieldWord(snapshot.GameTimeMinutes, nameof(snapshot.GameTimeMinutes)));
        WriteWord(payload, SaveRamLayout.GameTimeHoursOffset,
            FieldWord(snapshot.GameTimeHours, nameof(snapshot.GameTimeHours)));
        WriteWord(payload, SaveRamLayout.MoonwalkOffset, snapshot.MoonwalkEnabled ? (ushort)1 : (ushort)0);
        // Every save the game writes holds the words a new file stored in these two fields, since no retail routine
        // changes them; an imported save's other words are not carried.
        WriteWord(payload, SaveRamLayout.DebugFlagOffset, SaveRamLayout.DebugFlagSavedWord);
        WriteWord(payload, SaveRamLayout.NewFileMarkerOffset, SaveRamLayout.NewFileMarkerSavedWord);
        WriteWord(payload, SaveRamLayout.IconCancelOffset, snapshot.IconCancelEnabled ? (ushort)1 : (ushort)0);

        EncodeProgression(snapshot.Progress, payload);
        WriteWord(payload, SaveRamLayout.LoadingGameStateOffset,
            FieldWord(SuperMetroid.AssetExtraction.Progression.SaveLoadingGameStateCodec.Word(snapshot.StartupRoute), nameof(snapshot.StartupRoute)));
        WriteWord(payload, SaveRamLayout.SaveStationOffset, FieldWord(LoadStationNumberCodec.Number(snapshot.Checkpoint), nameof(snapshot.Checkpoint)));
        WriteWord(payload, SaveRamLayout.AreaOffset,
            FieldWord(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(snapshot.Checkpoint.Area), nameof(snapshot.Checkpoint)));
        SaveRamMapPacking.Pack(snapshot.Progress, maps).CopyTo(payload, SaveRamLayout.CompressedMapDataOffset);
        return payload;
    }

    /// <summary>The snapshot that re-encodes <paramref name="slot"/> to its own payload.</summary>
    internal static SuperMetroidSaveSnapshot ToSnapshot(SuperMetroidSaveSlot slot) => new()
    {
        ControllerBindings = slot.ControllerBindings,
        MoonwalkEnabled = slot.MoonwalkEnabled,
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
        StartupRoute = slot.StartupRoute,
        Checkpoint = slot.Checkpoint,
        Progress = slot.Progression.Copy(),
    };

    /// <summary>
    /// The persistent progress a slot payload's progression tables and packed explored map hold, read through the
    /// cartridge's table codec (<see cref="ProgressionTableCodec"/>).
    /// </summary>
    private static PersistentProgress DecodeProgression(ReadOnlySpan<byte> payload, IAreaMapSource maps)
    {
        var progress = new PersistentProgress();
        ProgressionTableCodec.ReadEvents(payload.Slice(SaveRamLayout.EventsOffset, ProgressionTableCodec.EventByteCount), progress);
        ProgressionTableCodec.ReadAreaBosses(payload.Slice(SaveRamLayout.BossBitsOffset, ProgressionTableCodec.AreaBossByteCount), progress);
        // The Chozo-orb table is not read: only unused room objects write it and nothing reads it.
        ProgressionTableCodec.ReadCollectedItems(payload.Slice(SaveRamLayout.CollectedItemBitsOffset, ProgressionTableCodec.RoomFlagByteCount), progress);
        ProgressionTableCodec.ReadOpenedDoors(payload.Slice(SaveRamLayout.OpenedDoorBitsOffset, ProgressionTableCodec.RoomFlagByteCount), progress);
        ProgressionTableCodec.ReadUsedStations(payload.Slice(SaveRamLayout.UsedSaveStationsOffset, ProgressionTableCodec.UsedStationByteCount), progress);
        ProgressionTableCodec.ReadAreaMaps(payload.Slice(SaveRamLayout.MapStationsOffset, ProgressionTableCodec.MapStationByteCount), progress);
        SaveRamMapPacking.Unpack(payload.Slice(SaveRamLayout.CompressedMapDataOffset, SaveRamLayout.CompressedMapDataByteCount), maps, progress);
        return progress;
    }

    /// <summary>Writes <paramref name="progress"/>'s progression tables into a slot payload; the packed map is written separately.</summary>
    private static void EncodeProgression(PersistentProgress progress, Span<byte> payload)
    {
        ProgressionTableCodec.WriteEvents(progress, payload.Slice(SaveRamLayout.EventsOffset, ProgressionTableCodec.EventByteCount));
        ProgressionTableCodec.WriteAreaBosses(progress, payload.Slice(SaveRamLayout.BossBitsOffset, ProgressionTableCodec.AreaBossByteCount));
        // The Chozo-orb table is left clear, as a new file leaves it.
        ProgressionTableCodec.WriteCollectedItems(progress, payload.Slice(SaveRamLayout.CollectedItemBitsOffset, ProgressionTableCodec.RoomFlagByteCount));
        ProgressionTableCodec.WriteOpenedDoors(progress, payload.Slice(SaveRamLayout.OpenedDoorBitsOffset, ProgressionTableCodec.RoomFlagByteCount));
        ProgressionTableCodec.WriteUsedStations(progress, payload.Slice(SaveRamLayout.UsedSaveStationsOffset, ProgressionTableCodec.UsedStationByteCount));
        ProgressionTableCodec.WriteAreaMaps(progress, payload.Slice(SaveRamLayout.MapStationsOffset, ProgressionTableCodec.MapStationByteCount));
    }

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
            ushort text = LittleEndianBytes.Word(marker[offset], marker[offset + 1]);
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
    /// Each word becomes buttons through <see cref="ControllerWordCodec"/>, which rejects bits no button produces;
    /// <see cref="ControllerBindings.RequireRetailPermutation"/> then rejects a corrupt permutation.
    /// </summary>
    private static ControllerBindings ReadControllerBindings(ReadOnlySpan<byte> payload) => new(
        Shoot: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.ShootButtonOffset), "save RAM shoot binding"),
        Jump: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.JumpButtonOffset), "save RAM jump binding"),
        Dash: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.DashButtonOffset), "save RAM dash binding"),
        ItemSelect: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.SelectButtonOffset), "save RAM item-select binding"),
        ItemCancel: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.CancelButtonOffset), "save RAM item-cancel binding"),
        AimUp: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.AimUpButtonOffset), "save RAM aim-up binding"),
        AimDown: ControllerWordCodec.FromJoypadWord(ReadWord(payload, SaveRamLayout.AimDownButtonOffset), "save RAM aim-down binding"));

    /// <summary>The eleven controller words a slot stores: the four D-pad directions, then the bindings in WRAM order.</summary>
    private static ushort[] ButtonConfigurationWords(ControllerBindings bindings) =>
    [
        ControllerWordCodec.ToJoypadWord(ControllerButton.Up),
        ControllerWordCodec.ToJoypadWord(ControllerButton.Down),
        ControllerWordCodec.ToJoypadWord(ControllerButton.Left),
        ControllerWordCodec.ToJoypadWord(ControllerButton.Right),
        ControllerWordCodec.ToJoypadWord(bindings.Shoot),
        ControllerWordCodec.ToJoypadWord(bindings.Jump),
        ControllerWordCodec.ToJoypadWord(bindings.Dash),
        ControllerWordCodec.ToJoypadWord(bindings.ItemCancel),
        ControllerWordCodec.ToJoypadWord(bindings.ItemSelect),
        ControllerWordCodec.ToJoypadWord(bindings.AimDown),
        ControllerWordCodec.ToJoypadWord(bindings.AimUp),
    ];

    /// <summary>A save field's value as the word its slot stores, rejected when the field cannot keep it.</summary>
    /// <exception cref="InvalidOperationException">The value is outside the field's range.</exception>
    private static ushort FieldWord(int value, string field) => checked((ushort)SaveFieldValue.Require(value, field));

    /// <summary>Reads one little-endian word.</summary>
    private static ushort ReadWord(ReadOnlySpan<byte> bytes, int offset) => LittleEndianBytes.Word(bytes[offset], bytes[offset + 1]);

    /// <summary>Writes one word in SNES byte order.</summary>
    private static void WriteWord(Span<byte> destination, int offset, ushort value)
    {
        destination[offset] = unchecked((byte)value);
        destination[offset + 1] = LittleEndianBytes.HighByte(value);
    }
}
