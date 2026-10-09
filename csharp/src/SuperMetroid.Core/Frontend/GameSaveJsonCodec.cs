using SuperMetroid.Core.Assets;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Strict deterministic conversion between named JSON saves and cartridge SRAM.</summary>
public static partial class GameSaveJsonCodec
{
    /// <summary>Mask of every defined Samus equipment bit accepted in collected and equipped item fields.</summary>
    private static readonly SamusEquipmentFlags AllEquipmentFlags =
        Enum.GetValues<SamusEquipmentFlags>().Aggregate((left, right) => left | right);
    /// <summary>Mask of every defined beam bit accepted in collected and equipped beam fields.</summary>
    private static readonly SamusBeamFlags AllBeamFlags =
        Enum.GetValues<SamusBeamFlags>().Aggregate((left, right) => left | right);
    /// <summary>Mask of every defined boss flag accepted in per-area progression data.</summary>
    private static readonly BossBits AllBossFlags =
        Enum.GetValues<BossBits>().Aggregate((left, right) => left | right);

    /// <summary>Captures all meaningful persistent state without a duplicate native-memory image.</summary>
    public static GameSaveJsonDocument Capture(SuperMetroidAddressSpace addressSpace, AreaMapPresentationCatalog maps)
    {
        ArgumentNullException.ThrowIfNull(addressSpace);
        var saveRam = new SuperMetroidSaveRam(addressSpace, maps);
        var slots = new GameSaveSlotJsonDocument?[SuperMetroidSaveRam.SlotCount];
        for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            SuperMetroidSaveSlot? slot = saveRam.ReadSlot(slotIndex);
            slots[slotIndex] = slot is null ? null : CaptureSlot(slot);
        }

        return new GameSaveJsonDocument
        {
            SelectedSlot = saveRam.ReadSelectedSlot(),
            Slots = slots,
            GameCompleted = saveRam.HasCompletedGame,
        };
    }

    /// <summary>Serializes with stable declaration order, indentation, and symbolic enums.</summary>
    public static string Serialize(GameSaveJsonDocument document)
    {
        Validate(document);
        return JsonSerializer.Serialize(document, CreateOptions()) + Environment.NewLine;
    }

    /// <summary>Parses one strict schema and reports malformed properties with JSON paths.</summary>
    public static GameSaveJsonDocument Deserialize(string json, string sourceName = "game save")
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        try
        {
            json = UpgradeLegacyJson(json, out int sourceSchemaVersion);
            GameSaveJsonDocument document = JsonSerializer.Deserialize<GameSaveJsonDocument>(
                json,
                CreateOptions()) ?? throw new InvalidDataException(
                    $"Game save '{sourceName}' contains JSON null instead of an object.");
            Validate(document);
            return document with { SourceSchemaVersion = sourceSchemaVersion };
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Game save '{sourceName}' is malformed at {exception.Path ?? "the root"}: " +
                exception.Message,
                exception);
        }
        catch (InvalidDataException exception) when (!exception.Message.Contains(sourceName, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Game save '{sourceName}' is invalid: {exception.Message}",
                exception);
        }
    }

    /// <summary>
    /// Rebuilds native SRAM from named persistent state, including the
    /// cartridge checksum directories. Missing JSON slots are cleared explicitly.
    /// </summary>
    public static void Apply(
        GameSaveJsonDocument document,
        SuperMetroidAddressSpace addressSpace, AreaMapPresentationCatalog maps)
    {
        ArgumentNullException.ThrowIfNull(addressSpace);
        Validate(document);
        addressSpace.SaveRam.Clear();
        var saveRam = new SuperMetroidSaveRam(addressSpace, maps);
        for (int slotIndex = 0; slotIndex < document.Slots.Length; slotIndex++)
        {
            GameSaveSlotJsonDocument? slot = document.Slots[slotIndex];
            if (slot is null)
                saveRam.ClearSlot(slotIndex);
            else
                saveRam.SaveSlot(slotIndex, ToSnapshot(slot));
        }
        saveRam.SelectSlot(document.SelectedSlot);
        saveRam.SetGameCompleted(document.GameCompleted);
    }

    /// <summary>Maps one decoded SRAM slot into the named JSON sections, including symbolic progression and map data.</summary>
    /// <param name="slot">Decoded persistent state for a single cartridge save slot.</param>
    /// <returns>JSON document sections representing that slot's persistent state.</returns>
    private static GameSaveSlotJsonDocument CaptureSlot(SuperMetroidSaveSlot slot) => new()
    {
        Slot = slot.Slot,
        Checkpoint = new SaveCheckpointJsonDocument(
            AreaIds.FromCartridge(checked((byte)slot.Area), $"slot {slot.Slot} checkpoint"),
            slot.SaveStation),
        Resources = new SaveResourcesJsonDocument(
            slot.Health,
            slot.MaxHealth,
            slot.Missiles,
            slot.MaxMissiles,
            slot.SuperMissiles,
            slot.MaxSuperMissiles,
            slot.PowerBombs,
            slot.MaxPowerBombs,
            slot.ReserveEnergy,
            slot.MaxReserveEnergy,
            slot.ReserveMissiles),
        Inventory = new SaveInventoryJsonDocument(
            (SamusEquipmentFlags)slot.CollectedItems,
            (SamusEquipmentFlags)slot.EquippedItems,
            (SamusBeamFlags)slot.CollectedBeams,
            (SamusBeamFlags)slot.EquippedBeams),
        GameTime = new SaveGameTimeJsonDocument(
            slot.GameTimeHours,
            slot.GameTimeMinutes,
            slot.GameTimeSeconds,
            slot.GameTimeFrames),
        Controller = new SaveControllerJsonDocument(
            (SnesButton)slot.ControllerBindings.Shoot,
            (SnesButton)slot.ControllerBindings.Jump,
            (SnesButton)slot.ControllerBindings.Dash,
            (SnesButton)slot.ControllerBindings.ItemSelect,
            (SnesButton)slot.ControllerBindings.ItemCancel,
            (SnesButton)slot.ControllerBindings.AimUp,
            (SnesButton)slot.ControllerBindings.AimDown),
        ReserveMode = slot.ReserveMode,
        HudItem = slot.HudItem,
        MoonwalkEnabled = slot.MoonwalkEnabled,
        IconCancelEnabled = slot.IconCancelEnabled,
        JapaneseText = slot.JapaneseText,
        LoadedItemCount = slot.LoadedItemCount,
        CartridgeDebugFlag = slot.DebugFlag,
        NewFileMarker = slot.NewFileMarker,
        LoadingGameState = slot.LoadingGameState,
        Progression = new SaveProgressionJsonDocument
        {
            Events = CaptureEvents(slot.EventBytes),
            BossFlagsByAreaIndex = slot.BossBytes.Select(value => (BossBits)value).ToArray(),
            RoomChozoBits = CaptureSetBits(slot.RoomChozoBytes),
            CollectedItemBits = CaptureSetBits(slot.CollectedItemBytes),
            OpenedDoorBits = CaptureSetBits(slot.OpenedDoorBytes),
            UsedSaveStationBits = CaptureSetBits(slot.UsedSaveStationBytes),
            AcquiredMapStationBits = CaptureSetBits(slot.MapStationBytes),
            ExploredAreas = CaptureExploredAreas(slot.ExploredMapBytes),
        },
    };

    /// <summary>Converts a validated named JSON slot into the byte-oriented snapshot consumed by the SRAM writer.</summary>
    /// <param name="slot">JSON data for one cartridge save slot.</param>
    /// <returns>Snapshot with controller, inventory, resources, time, checkpoint, and decoded progression bytes.</returns>
    private static SuperMetroidSaveSnapshot ToSnapshot(GameSaveSlotJsonDocument slot)
    {
        ValidateSlot(slot, slot.Slot);
        ControllerBindings bindings = new(
            (ushort)slot.Controller.Shoot,
            (ushort)slot.Controller.Jump,
            (ushort)slot.Controller.Dash,
            (ushort)slot.Controller.ItemSelect,
            (ushort)slot.Controller.ItemCancel,
            (ushort)slot.Controller.AimUp,
            (ushort)slot.Controller.AimDown);
        bindings.RequireRetailPermutation();
        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = bindings,
            MoonwalkEnabled = slot.MoonwalkEnabled,
            IconCancelEnabled = slot.IconCancelEnabled,
            JapaneseText = slot.JapaneseText,
            LoadedItemCount = slot.LoadedItemCount,
            DebugFlag = slot.CartridgeDebugFlag,
            NewFileMarker = slot.NewFileMarker,
            LoadingGameState = slot.LoadingGameState,
            EquippedItems = (ushort)slot.Inventory.EquippedItems,
            CollectedItems = (ushort)slot.Inventory.CollectedItems,
            EquippedBeams = (ushort)slot.Inventory.EquippedBeams,
            CollectedBeams = (ushort)slot.Inventory.CollectedBeams,
            ReserveMode = slot.ReserveMode,
            Health = slot.Resources.Health,
            MaxHealth = slot.Resources.MaxHealth,
            Missiles = slot.Resources.Missiles,
            MaxMissiles = slot.Resources.MaxMissiles,
            SuperMissiles = slot.Resources.SuperMissiles,
            MaxSuperMissiles = slot.Resources.MaxSuperMissiles,
            PowerBombs = slot.Resources.PowerBombs,
            MaxPowerBombs = slot.Resources.MaxPowerBombs,
            HudItem = slot.HudItem,
            MaxReserveEnergy = slot.Resources.MaxReserveEnergy,
            ReserveEnergy = slot.Resources.ReserveEnergy,
            ReserveMissiles = slot.Resources.ReserveMissiles,
            GameTimeFrames = slot.GameTime.Frames,
            GameTimeSeconds = slot.GameTime.Seconds,
            GameTimeMinutes = slot.GameTime.Minutes,
            GameTimeHours = slot.GameTime.Hours,
            SaveStation = slot.Checkpoint.SaveStation,
            Area = (byte)slot.Checkpoint.Area,
            EventBytes = DecodeEvents(slot.Progression.Events),
            BossBytes = slot.Progression.BossFlagsByAreaIndex.Select(value => (byte)value).ToArray(),
            RoomChozoBytes = DecodeSetBits(
                slot.Progression.RoomChozoBits,
                Bank80SystemState.RoomChozoBitByteCount,
                "progression.roomChozoBits"),
            CollectedItemBytes = DecodeSetBits(
                slot.Progression.CollectedItemBits,
                Bank80SystemState.ItemBitByteCount,
                "progression.collectedItemBits"),
            OpenedDoorBytes = DecodeSetBits(
                slot.Progression.OpenedDoorBits,
                Bank80SystemState.DoorBitByteCount,
                "progression.openedDoorBits"),
            UsedSaveStationBytes = DecodeSetBits(
                slot.Progression.UsedSaveStationBits,
                Bank80SystemState.UsedSaveStationByteCount,
                "progression.usedSaveStationBits"),
            MapStationBytes = DecodeSetBits(
                slot.Progression.AcquiredMapStationBits,
                Bank80SystemState.MapStationByteCount,
                "progression.acquiredMapStationBits"),
            ExploredMapBytes = DecodeExploredAreas(slot.Progression.ExploredAreas),
        };
    }

    /// <summary>Validates the document schema version, selected slot, slot-array shape, and each present slot.</summary>
    /// <param name="document">Root JSON save document to validate before serialization or SRAM application.</param>
    /// <exception cref="ArgumentNullException">The root document is null.</exception>
    /// <exception cref="InvalidDataException">The schema or any selected-slot data violates the save format.</exception>
    private static void Validate(GameSaveJsonDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.SchemaVersion != GameSaveJsonFormat.SchemaVersion)
        {
            throw new InvalidDataException(
                $"schemaVersion {document.SchemaVersion} is unsupported; expected " +
                $"{GameSaveJsonFormat.SchemaVersion}");
        }
        if ((uint)document.SelectedSlot >= SuperMetroidSaveRam.SlotCount)
            throw new InvalidDataException("selectedSlot must be 0, 1, or 2");
        if (document.Slots is null || document.Slots.Length != SuperMetroidSaveRam.SlotCount)
            throw new InvalidDataException("slots must contain exactly three entries");

        for (int slotIndex = 0; slotIndex < document.Slots.Length; slotIndex++)
        {
            if (document.Slots[slotIndex] is { } slot)
                ValidateSlot(slot, slotIndex);
        }
    }

    /// <summary>Checks one slot's nested sections, retail value ranges, inventory relationships, and progression encodings.</summary>
    /// <param name="slot">Slot document whose values are checked.</param>
    /// <param name="expectedSlot">Array index the slot's own slot identifier must match.</param>
    /// <exception cref="InvalidDataException">A required section is absent or a value is outside its cartridge domain.</exception>
    private static void ValidateSlot(GameSaveSlotJsonDocument slot, int expectedSlot)
    {
        if (slot.Slot != expectedSlot)
            throw new InvalidDataException($"slots[{expectedSlot}].slot must equal {expectedSlot}");
        if (slot.Checkpoint is null || slot.Resources is null || slot.Inventory is null ||
            slot.GameTime is null || slot.Controller is null || slot.Progression is null)
            throw new InvalidDataException($"slots[{expectedSlot}] is missing a required domain object");
        _ = AreaIds.ToIndex(slot.Checkpoint.Area);
        if (slot.Resources.Health > slot.Resources.MaxHealth)
            throw new InvalidDataException($"slots[{expectedSlot}].resources.health exceeds maxHealth");
        if (slot.Resources.Missiles > slot.Resources.MaxMissiles ||
            slot.Resources.SuperMissiles > slot.Resources.MaxSuperMissiles ||
            slot.Resources.PowerBombs > slot.Resources.MaxPowerBombs ||
            slot.Resources.ReserveEnergy > slot.Resources.MaxReserveEnergy)
        {
            throw new InvalidDataException(
                $"slots[{expectedSlot}].resources contains a current value above its maximum");
        }
        if (slot.GameTime.Frames > 59 || slot.GameTime.Seconds > 59 ||
            slot.GameTime.Minutes > 59 || slot.GameTime.Hours > 99)
            throw new InvalidDataException($"slots[{expectedSlot}].gameTime is outside retail ranges");
        if ((slot.Inventory.CollectedItems & ~AllEquipmentFlags) != 0 ||
            (slot.Inventory.EquippedItems & ~AllEquipmentFlags) != 0 ||
            (slot.Inventory.CollectedBeams & ~AllBeamFlags) != 0 ||
            (slot.Inventory.EquippedBeams & ~AllBeamFlags) != 0)
            throw new InvalidDataException($"slots[{expectedSlot}].inventory contains unknown bits");
        if ((slot.Inventory.EquippedItems & ~slot.Inventory.CollectedItems) != 0 ||
            (slot.Inventory.EquippedBeams & ~slot.Inventory.CollectedBeams) != 0)
            throw new InvalidDataException($"slots[{expectedSlot}].inventory equips an uncollected upgrade");
        new ControllerBindings(
            (ushort)slot.Controller.Shoot,
            (ushort)slot.Controller.Jump,
            (ushort)slot.Controller.Dash,
            (ushort)slot.Controller.ItemSelect,
            (ushort)slot.Controller.ItemCancel,
            (ushort)slot.Controller.AimUp,
            (ushort)slot.Controller.AimDown).RequireRetailPermutation();

        SaveProgressionJsonDocument progression = slot.Progression;
        if (progression.Events is null || progression.BossFlagsByAreaIndex is null ||
            progression.RoomChozoBits is null || progression.CollectedItemBits is null ||
            progression.OpenedDoorBits is null || progression.UsedSaveStationBits is null ||
            progression.AcquiredMapStationBits is null || progression.ExploredAreas is null)
            throw new InvalidDataException($"slots[{expectedSlot}].progression is incomplete");
        _ = DecodeEvents(progression.Events);
        if (progression.BossFlagsByAreaIndex.Length != Bank80SystemState.AreaCount ||
            progression.BossFlagsByAreaIndex.Any(flags => (flags & ~AllBossFlags) != 0))
            throw new InvalidDataException($"slots[{expectedSlot}].progression.bossFlagsByAreaIndex is invalid");
        _ = DecodeSetBits(progression.RoomChozoBits, Bank80SystemState.RoomChozoBitByteCount,
            $"slots[{expectedSlot}].progression.roomChozoBits");
        _ = DecodeSetBits(progression.CollectedItemBits, Bank80SystemState.ItemBitByteCount,
            $"slots[{expectedSlot}].progression.collectedItemBits");
        _ = DecodeSetBits(progression.OpenedDoorBits, Bank80SystemState.DoorBitByteCount,
            $"slots[{expectedSlot}].progression.openedDoorBits");
        _ = DecodeSetBits(progression.UsedSaveStationBits, Bank80SystemState.UsedSaveStationByteCount,
            $"slots[{expectedSlot}].progression.usedSaveStationBits");
        _ = DecodeSetBits(progression.AcquiredMapStationBits, Bank80SystemState.MapStationByteCount,
            $"slots[{expectedSlot}].progression.acquiredMapStationBits");
        _ = DecodeExploredAreas(progression.ExploredAreas);
    }

    /// <summary>Creates the strict JSON settings shared by save serialization and deserialization.</summary>
    /// <returns>Indented camel-case options that reject unknown properties and integer enum values.</returns>
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        return options;
    }

    /// <summary>Turns set event bits into symbolic names, preserving unnamed cartridge bits as hexadecimal values.</summary>
    /// <param name="bytes">Native event bit plane copied from one save slot.</param>
    /// <returns>Names or <c>0xNN</c> strings for each set event bit in ascending bit order.</returns>
    private static string[] CaptureEvents(ReadOnlySpan<byte> bytes) =>
        CaptureSetBits(bytes).Select(bit => Enum.IsDefined((EventNumber)bit)
            ? ((EventNumber)bit).ToString()
            : $"0x{bit:X2}").ToArray();

    /// <summary>Resolves symbolic or hexadecimal event entries into the native event bit plane.</summary>
    /// <param name="events">JSON event names or raw <c>0xNN</c> bit identifiers.</param>
    /// <returns>Cartridge-sized event bytes reconstructed from the listed bits.</returns>
    /// <exception cref="InvalidDataException">The array is missing or contains an unknown name or malformed bit value.</exception>
    private static byte[] DecodeEvents(string[] events)
    {
        if (events is null)
            throw new InvalidDataException("progression.events is required");
        var bits = new int[events.Length];
        for (int index = 0; index < events.Length; index++)
        {
            string value = events[index] ?? throw new InvalidDataException(
                $"progression.events[{index}] is null");
            if (Enum.TryParse(value, ignoreCase: true, out EventNumber named) && Enum.IsDefined(named))
                bits[index] = (int)named;
            else if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(value[2..], NumberStyles.AllowHexSpecifier,
                         CultureInfo.InvariantCulture, out int raw))
                bits[index] = raw;
            else
                throw new InvalidDataException(
                    $"progression.events[{index}] '{value}' is not a known name or 0xNN bit");
        }
        return DecodeSetBits(bits, Bank80SystemState.EventByteCount, "progression.events");
    }

    /// <summary>Lists every set bit in a byte plane as its ascending zero-based bit index.</summary>
    /// <param name="bytes">Native bytes whose set bits are captured.</param>
    /// <returns>Strictly increasing bit indexes suitable for the JSON set-bit representation.</returns>
    private static int[] CaptureSetBits(ReadOnlySpan<byte> bytes)
    {
        var result = new List<int>();
        for (int byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
        {
            for (int bit = 0; bit < 8; bit++)
            {
                if ((bytes[byteIndex] & (1 << bit)) != 0)
                    result.Add(byteIndex * 8 + bit);
            }
        }
        return result.ToArray();
    }

    /// <summary>Rebuilds a fixed-size bit plane from strictly increasing, unique bit indexes.</summary>
    /// <param name="bits">JSON indexes of bits that must be set.</param>
    /// <param name="byteCount">Required size of the resulting native byte plane.</param>
    /// <param name="property">JSON property path used in validation errors.</param>
    /// <returns>Byte plane with exactly the listed bits set.</returns>
    /// <exception cref="InvalidDataException">The list is null, contains an out-of-range bit, or is not strictly increasing.</exception>
    private static byte[] DecodeSetBits(int[] bits, int byteCount, string property)
    {
        if (bits is null)
            throw new InvalidDataException($"{property} is required");
        var result = new byte[byteCount];
        int previous = -1;
        for (int index = 0; index < bits.Length; index++)
        {
            int bit = bits[index];
            if ((uint)bit >= byteCount * 8)
                throw new InvalidDataException($"{property}[{index}]={bit} is outside the bit plane");
            if (bit <= previous)
                throw new InvalidDataException($"{property} must be strictly increasing without duplicates");
            result[bit >> 3] |= unchecked((byte)(1 << (bit & 7)));
            previous = bit;
        }
        return result;
    }

    /// <summary>Converts per-area explored-map bit planes into row-major coordinate lists.</summary>
    /// <param name="bytes">Native explored-map bytes for all cartridge areas.</param>
    /// <returns>One coordinate document per area, ordered by the cartridge's area index.</returns>
    private static ExploredAreaJsonDocument[] CaptureExploredAreas(ReadOnlySpan<byte> bytes)
    {
        var areas = new ExploredAreaJsonDocument[Bank80SystemState.ExploredMapAreaCount];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int areaIndex = AreaIds.ToIndex(area);
            var tiles = new List<MapCoordinateJsonDocument>();
            for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
            {
                for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
                {
                    int byteIndex = areaIndex * Bank80SystemState.ExploredMapBytesPerArea +
                        AreaMapLayout.GetBitByteIndex(x, y);
                    if ((bytes[byteIndex] & AreaMapLayout.GetBitMask(x)) != 0)
                        tiles.Add(new MapCoordinateJsonDocument(x, y));
                }
            }
            areas[areaIndex] = new ExploredAreaJsonDocument(area, tiles.ToArray());
        }
        return areas;
    }

    /// <summary>Reconstructs explored-map bit planes from ordered area documents and unique row-major tile coordinates.</summary>
    /// <param name="areas">JSON coordinate lists, one for each cartridge area in native order.</param>
    /// <returns>Native explored-map bytes for all areas.</returns>
    /// <exception cref="InvalidDataException">The area count, order, tile coordinates, or row-major uniqueness is invalid.</exception>
    private static byte[] DecodeExploredAreas(ExploredAreaJsonDocument[] areas)
    {
        if (areas.Length != Bank80SystemState.ExploredMapAreaCount)
            throw new InvalidDataException(
                $"progression.exploredAreas must contain {Bank80SystemState.ExploredMapAreaCount} areas");
        var result = new byte[
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int areaIndex = 0; areaIndex < areas.Length; areaIndex++)
        {
            ExploredAreaJsonDocument area = areas[areaIndex] ?? throw new InvalidDataException(
                $"progression.exploredAreas[{areaIndex}] is null");
            if (AreaIds.ToIndex(area.Area) != areaIndex)
                throw new InvalidDataException(
                    $"progression.exploredAreas[{areaIndex}].area is out of cartridge order");
            if (area.Tiles is null)
                throw new InvalidDataException(
                    $"progression.exploredAreas[{areaIndex}].tiles is required");
            int previousIndex = -1;
            foreach (MapCoordinateJsonDocument tile in area.Tiles)
            {
                if (tile is null || (uint)tile.X >= AreaMapLayout.WidthInTiles ||
                    (uint)tile.Y >= AreaMapLayout.HeightInTiles)
                    throw new InvalidDataException(
                        $"progression.exploredAreas[{areaIndex}] contains an invalid tile");
                int logicalIndex = tile.Y * AreaMapLayout.WidthInTiles + tile.X;
                if (logicalIndex <= previousIndex)
                    throw new InvalidDataException(
                        $"progression.exploredAreas[{areaIndex}].tiles must be row-major and unique");
                int byteIndex = areaIndex * Bank80SystemState.ExploredMapBytesPerArea +
                    AreaMapLayout.GetBitByteIndex(tile.X, tile.Y);
                result[byteIndex] |= AreaMapLayout.GetBitMask(tile.X);
                previousIndex = logicalIndex;
            }
        }
        return result;
    }
}
