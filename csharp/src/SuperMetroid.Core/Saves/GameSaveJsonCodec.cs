using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Saves;

/// <summary>Strict deterministic conversion between named JSON saves and the owned save store.</summary>
public static class GameSaveJsonCodec
{
    /// <summary>Known equipment bits accepted by the named save schema.</summary>
    private static readonly SamusEquipmentFlags AllEquipmentFlags =
        Enum.GetValues<SamusEquipmentFlags>().Aggregate((left, right) => left | right);
    /// <summary>Known beam bits accepted by the named save schema.</summary>
    private static readonly SamusBeamFlags AllBeamFlags =
        Enum.GetValues<SamusBeamFlags>().Aggregate((left, right) => left | right);
    /// <summary>Known boss-defeat bits accepted by the named save schema.</summary>
    private static readonly BossBits AllBossFlags =
        Enum.GetValues<BossBits>().Aggregate((left, right) => left | right);

    /// <summary>Captures all meaningful persistent state without a duplicate native-memory image.</summary>
    public static GameSaveJsonDocument Capture(SaveSlotStore saves)
    {
        Ensure.NotNull(saves);
        var slots = new GameSaveSlotJsonDocument?[SaveSlotStore.SlotCount];
        for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            SuperMetroidSaveSlot? slot = saves.ReadSlot(slotIndex);
            slots[slotIndex] = slot is null ? null : CaptureSlot(slot);
        }

        return new GameSaveJsonDocument
        {
            SelectedSlot = saves.ReadSelectedSlot(),
            Slots = slots,
            GameCompleted = saves.HasCompletedGame,
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
        Ensure.NotNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        try
        {
            GameSaveJsonDocument document = JsonSerializer.Deserialize<GameSaveJsonDocument>(
                json,
                CreateOptions()) ?? throw new InvalidDataException(
                    $"Game save '{sourceName}' contains JSON null instead of an object.");
            Validate(document);
            return document;
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
    /// Builds the save store named persistent state describes. A JSON slot that is absent is an empty slot.
    /// </summary>
    /// <param name="document">The validated named save.</param>
    /// <param name="maps">Installed stock map rules; each slot keeps the explored bytes SRAM would.</param>
    public static SaveSlotStore ToStore(GameSaveJsonDocument document, IAreaMapSource maps)
    {
        Ensure.NotNull(maps);
        Validate(document);
        var saves = new SaveSlotStore(maps);
        for (int slotIndex = 0; slotIndex < document.Slots.Length; slotIndex++)
        {
            if (document.Slots[slotIndex] is { } slot)
                saves.SaveSlot(slotIndex, ToSnapshot(slot));
        }
        saves.SelectSlot(document.SelectedSlot);
        saves.SetGameCompleted(document.GameCompleted);
        return saves;
    }

    /// <summary>Projects one native save slot into named resources, flags, events, and explored map cells.</summary>
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
            slot.CollectedItems,
            slot.EquippedItems,
            slot.CollectedBeams,
            slot.EquippedBeams),
        GameTime = new SaveGameTimeJsonDocument(
            slot.GameTimeHours,
            slot.GameTimeMinutes,
            slot.GameTimeSeconds,
            slot.GameTimeFrames),
        Controller = new SaveControllerJsonDocument(
            slot.ControllerBindings.Shoot,
            slot.ControllerBindings.Jump,
            slot.ControllerBindings.Dash,
            slot.ControllerBindings.ItemSelect,
            slot.ControllerBindings.ItemCancel,
            slot.ControllerBindings.AimUp,
            slot.ControllerBindings.AimDown),
        ReserveMode = slot.ReserveMode.ToNativeWord(),
        HudItem = slot.HudItem.ToNativeWord(),
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

    /// <summary>Reconstructs the native save snapshot from one validated named slot.</summary>
    private static SuperMetroidSaveSnapshot ToSnapshot(GameSaveSlotJsonDocument slot)
    {
        ValidateSlot(slot, slot.Slot);
        ControllerBindings bindings = new(
            slot.Controller.Shoot,
            slot.Controller.Jump,
            slot.Controller.Dash,
            slot.Controller.ItemSelect,
            slot.Controller.ItemCancel,
            slot.Controller.AimUp,
            slot.Controller.AimDown);
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
            EquippedItems = slot.Inventory.EquippedItems,
            CollectedItems = slot.Inventory.CollectedItems,
            EquippedBeams = slot.Inventory.EquippedBeams,
            CollectedBeams = slot.Inventory.CollectedBeams,
            ReserveMode = ReserveTankModes.Decode(SaveFieldValue.Encode(slot.ReserveMode, "reserveMode")),
            Health = slot.Resources.Health,
            MaxHealth = slot.Resources.MaxHealth,
            Missiles = slot.Resources.Missiles,
            MaxMissiles = slot.Resources.MaxMissiles,
            SuperMissiles = slot.Resources.SuperMissiles,
            MaxSuperMissiles = slot.Resources.MaxSuperMissiles,
            PowerBombs = slot.Resources.PowerBombs,
            MaxPowerBombs = slot.Resources.MaxPowerBombs,
            HudItem = HudItems.Decode(SaveFieldValue.Encode(slot.HudItem, "hudItem")),
            MaxReserveEnergy = slot.Resources.MaxReserveEnergy,
            ReserveEnergy = slot.Resources.ReserveEnergy,
            ReserveMissiles = slot.Resources.ReserveMissiles,
            GameTimeFrames = slot.GameTime.Frames,
            GameTimeSeconds = slot.GameTime.Seconds,
            GameTimeMinutes = slot.GameTime.Minutes,
            GameTimeHours = slot.GameTime.Hours,
            SaveStation = slot.Checkpoint.SaveStation,
            Area = AreaIds.ToIndex(slot.Checkpoint.Area),
            EventBytes = DecodeEvents(slot.Progression.Events),
            BossFlags = slot.Progression.BossFlagsByAreaIndex.ToArray(),
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

    /// <summary>Requires a supported schema, valid selected slot, and exactly the native slot count.</summary>
    private static void Validate(GameSaveJsonDocument document)
    {
        Ensure.NotNull(document);
        if (document.SchemaVersion != GameSaveJsonFormat.SchemaVersion)
        {
            throw new InvalidDataException(
                $"schemaVersion {document.SchemaVersion} is unsupported; expected " +
                $"{GameSaveJsonFormat.SchemaVersion}");
        }
        if ((uint)document.SelectedSlot >= SaveSlotStore.SlotCount)
            throw new InvalidDataException("selectedSlot must be 0, 1, or 2");
        if (document.Slots is null || document.Slots.Length != SaveSlotStore.SlotCount)
            throw new InvalidDataException("slots must contain exactly three entries");

        for (int slotIndex = 0; slotIndex < document.Slots.Length; slotIndex++)
        {
            if (document.Slots[slotIndex] is { } slot)
                ValidateSlot(slot, slotIndex);
        }
    }

    /// <summary>Checks one slot's identity, resource ranges, equipment flags, and map/event domains.</summary>
    private static void ValidateSlot(GameSaveSlotJsonDocument slot, int expectedSlot)
    {
        if (slot.Slot != expectedSlot)
            throw new InvalidDataException($"slots[{expectedSlot}].slot must equal {expectedSlot}");
        if (slot.Checkpoint is null || slot.Resources is null || slot.Inventory is null ||
            slot.GameTime is null || slot.Controller is null || slot.Progression is null)
            throw new InvalidDataException($"slots[{expectedSlot}] is missing a required domain object");
        _ = AreaIds.ToIndex(slot.Checkpoint.Area);
        RequireRange(slot.Checkpoint.SaveStation, SaveFieldValue.MaximumValue, expectedSlot, "checkpoint.saveStation");
        RequireRange(slot.ReserveMode, SaveFieldValue.MaximumValue, expectedSlot, "reserveMode");
        RequireRange(slot.HudItem, SaveFieldValue.MaximumValue, expectedSlot, "hudItem");
        RequireRange(slot.LoadedItemCount, SaveFieldValue.MaximumValue, expectedSlot, "loadedItemCount");
        RequireRange(slot.CartridgeDebugFlag, SaveFieldValue.MaximumValue, expectedSlot, "cartridgeDebugFlag");
        RequireRange(slot.NewFileMarker, SaveFieldValue.MaximumValue, expectedSlot, "newFileMarker");
        RequireRange(slot.LoadingGameState, SaveFieldValue.MaximumValue, expectedSlot, "loadingGameState");
        SaveResourcesJsonDocument resources = slot.Resources;
        foreach ((int value, string property) in new SaveFieldCheck[]
                 {
                     new SaveFieldCheck(resources.Health, "health"), new SaveFieldCheck(resources.MaxHealth, "maxHealth"),
                     new SaveFieldCheck(resources.Missiles, "missiles"), new SaveFieldCheck(resources.MaxMissiles, "maxMissiles"),
                     new SaveFieldCheck(resources.SuperMissiles, "superMissiles"), new SaveFieldCheck(resources.MaxSuperMissiles, "maxSuperMissiles"),
                     new SaveFieldCheck(resources.PowerBombs, "powerBombs"), new SaveFieldCheck(resources.MaxPowerBombs, "maxPowerBombs"),
                     new SaveFieldCheck(resources.ReserveEnergy, "reserveEnergy"), new SaveFieldCheck(resources.MaxReserveEnergy, "maxReserveEnergy"),
                     new SaveFieldCheck(resources.ReserveMissiles, "reserveMissiles"),
                 })
            RequireRange(value, SamusQuantity.MinimumValue, SamusQuantity.MaximumValue, expectedSlot, $"resources.{property}");
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
        if (slot.GameTime.Frames is < 0 or > 59 || slot.GameTime.Seconds is < 0 or > 59 ||
            slot.GameTime.Minutes is < 0 or > 59 || slot.GameTime.Hours is < 0 or > 99)
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
            slot.Controller.Shoot,
            slot.Controller.Jump,
            slot.Controller.Dash,
            slot.Controller.ItemSelect,
            slot.Controller.ItemCancel,
            slot.Controller.AimUp,
            slot.Controller.AimDown).RequireRetailPermutation();

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

    /// <summary>
    /// Rejects a numeric slot field outside <c>[0, <paramref name="maximum"/>]</c>, the range its domain or its SRAM
    /// field holds, so a JSON number never wraps on its way into the store.
    /// </summary>
    private static void RequireRange(int value, int maximum, int expectedSlot, string property) =>
        RequireRange(value, 0, maximum, expectedSlot, property);

    /// <summary>Rejects a numeric slot field outside <c>[<paramref name="minimum"/>, <paramref name="maximum"/>]</c>.</summary>
    private static void RequireRange(int value, int minimum, int maximum, int expectedSlot, string property)
    {
        if (value < minimum || value > maximum)
            throw new InvalidDataException($"slots[{expectedSlot}].{property} {value} is outside [{minimum}, {maximum}]");
    }

    /// <summary>Configures strict enum names, property handling, and stable save JSON formatting.</summary>
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

    /// <summary>Lists set event bits by their named persistent event identities.</summary>
    private static string[] CaptureEvents(ReadOnlySpan<byte> bytes) =>
        CaptureSetBits(bytes).Select(bit => Enum.IsDefined((EventNumber)bit)
            ? ((EventNumber)bit).ToString()
            : $"0x{bit:X2}").ToArray();

    /// <summary>Rebuilds event bytes from validated symbolic event names.</summary>
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

    /// <summary>Lists set flag positions in byte-order for compact persistent state.</summary>
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

    /// <summary>Rebuilds fixed-size bitfields while rejecting duplicates and out-of-range positions.</summary>
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

    /// <summary>Splits SRAM exploration bytes into named per-area map data.</summary>
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
                    var cell = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
                    int byteIndex = areaIndex * Bank80SystemState.ExploredMapBytesPerArea +
                        AreaMapLayout.GetBitByteIndex(cell);
                    if ((bytes[byteIndex] & AreaMapLayout.GetBitMask(cell.X)) != 0)
                        tiles.Add(MapCoordinateJsonDocument.FromCell(cell));
                }
            }
            areas[areaIndex] = new ExploredAreaJsonDocument(area, tiles.ToArray());
        }
        return areas;
    }

    /// <summary>Rebuilds the cartridge's fixed exploration layout from seven area documents.</summary>
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
                if (tile is null || !AreaMapLayout.Size.Contains(tile.Cell))
                    throw new InvalidDataException(
                        $"progression.exploredAreas[{areaIndex}] contains an invalid tile");
                Position16<MapTiles> cell = tile.Cell;
                int logicalIndex = AreaMapLayout.Size.RowMajorIndex(cell);
                if (logicalIndex <= previousIndex)
                    throw new InvalidDataException(
                        $"progression.exploredAreas[{areaIndex}].tiles must be row-major and unique");
                int byteIndex = areaIndex * Bank80SystemState.ExploredMapBytesPerArea +
                    AreaMapLayout.GetBitByteIndex(cell);
                result[byteIndex] |= AreaMapLayout.GetBitMask(cell.X);
                previousIndex = logicalIndex;
            }
        }
        return result;
    }

}
