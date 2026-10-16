using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Input;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
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
            slot.ControllerBindings.Shoot,
            slot.ControllerBindings.Jump,
            slot.ControllerBindings.Dash,
            slot.ControllerBindings.ItemSelect,
            slot.ControllerBindings.ItemCancel,
            slot.ControllerBindings.AimUp,
            slot.ControllerBindings.AimDown),
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

    /// <summary>One-way import of the old full-SRAM envelope; current output has no raw pages.</summary>
    private static string UpgradeLegacyJson(string json, out int sourceSchemaVersion)
    {
        JsonObject root = JsonNode.Parse(json) as JsonObject ??
            throw new InvalidDataException("Game save must be a JSON object.");
        if (root["schemaVersion"] is not JsonValue version || !version.TryGetValue(out sourceSchemaVersion))
            throw new InvalidDataException("schemaVersion must be an integer");
        if (sourceSchemaVersion != 1) return json;
        JsonArray pages = root["preservedUntranslatedSram"] as JsonArray ??
            throw new InvalidDataException("Legacy preservedUntranslatedSram is required.");
        byte[] sram = DecodeLegacyPages(pages);
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        if (slots.Count != SaveSlotStore.SlotCount)
            throw new InvalidDataException("slots must contain exactly three entries");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject resources = slot["resources"] as JsonObject ?? throw new InvalidDataException($"slots[{index}].resources is required");
            int origin = SaveRamLayout.SlotOffset(index);
            resources["reserveMissiles"] = Word(SaveRamLayout.ReserveMissilesOffset);
            slot["japaneseText"] = Word(SaveRamLayout.JapaneseTextOffset) != 0;
            slot["loadedItemCount"] = Word(SaveRamLayout.LoadedItemCountOffset);
            ushort Word(int offset) => (ushort)(SnesLittleEndian.Word(sram[origin + offset], sram[origin + offset + 1]));
        }
        root["gameCompleted"] = sram.AsSpan(SaveRamLayout.CompletionMarkerOffset,
            SaveRamLayout.CompletionMarker.Length).SequenceEqual(SaveRamLayout.CompletionMarker);
        root.Remove("preservedUntranslatedSram");
        root["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        return root.ToJsonString();
    }

    /// <summary>Validates the older page-preservation format before upgrading named save data.</summary>
    private static byte[] DecodeLegacyPages(JsonArray pages)
    {
        int size = GameSaveJsonFormat.PreservationPageByteCount;
        int expectedPages = SaveRamLayout.ImageByteCount / size;
        if (pages.Count != expectedPages)
            throw new InvalidDataException($"preservedUntranslatedSram must contain {expectedPages} pages");
        var sram = new byte[SaveRamLayout.ImageByteCount];
        for (int page = 0; page < pages.Count; page++)
        {
            var entry = pages[page]?.Deserialize<LegacySramPage>(CreateOptions()) ??
                throw new InvalidDataException($"preservedUntranslatedSram[{page}] is null");
            if (!string.Equals(entry.Offset, $"0x{page * size:X4}", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"preservedUntranslatedSram[{page}].offset must be 0x{page * size:X4}");
            byte[] bytes;
            try { bytes = Convert.FromHexString(entry.Bytes ?? throw new InvalidDataException($"preservedUntranslatedSram[{page}].bytes is required")); }
            catch (FormatException error) { throw new InvalidDataException($"preservedUntranslatedSram[{page}].bytes is not hexadecimal", error); }
            if (bytes.Length != size) throw new InvalidDataException($"preservedUntranslatedSram[{page}].bytes must decode to {size} bytes");
            bytes.CopyTo(sram, page * size);
        }
        return sram;
    }

    /// <summary>One fixed-size raw SRAM page in a legacy save awaiting schema upgrade.</summary>
    private sealed record LegacySramPage
    {
        /// <summary>Expected hexadecimal SRAM page origin used to verify ordering.</summary>
        public required string Offset { get; init; }
        /// <summary>Hexadecimal page bytes decoded into the legacy SRAM image.</summary>
        public required string Bytes { get; init; }
    }
}
