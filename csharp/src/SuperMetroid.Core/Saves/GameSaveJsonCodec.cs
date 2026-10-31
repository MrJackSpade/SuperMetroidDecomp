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
    /// <param name="maps">Installed stock map rules; each slot keeps the explored cells a save keeps.</param>
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
            AreaIds.FromIndex(Ensure.BetweenInclusive(0, byte.MaxValue, slot.Area), $"slot {slot.Slot} checkpoint"),
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
        ReserveMode = slot.ReserveMode.ToWord(),
        HudItem = slot.HudItem.ToWord(),
        MoonwalkEnabled = slot.MoonwalkEnabled,
        IconCancelEnabled = slot.IconCancelEnabled,
        JapaneseText = slot.JapaneseText,
        LoadedItemCount = slot.LoadedItemCount,
        DebugMode = slot.DebugFlag,
        NewFileMarker = slot.NewFileMarker,
        LoadingGameState = slot.LoadingGameState,
        Progression = CaptureProgression(slot.Progression),
    };

    /// <summary>Lists each set of <paramref name="progress"/> by its identities.</summary>
    private static SaveProgressionJsonDocument CaptureProgression(PersistentProgress progress) => new()
    {
        Events = [.. progress.Events.Members.Select(number => EventNumberCodec.TryFromEventNumber(number, out EventNumber named)
            ? named.ToString()
            : $"0x{number:X2}")],
        DefeatedBosses = [.. RetailAreas.Select(area => new SaveAreaBossesJsonDocument(area, progress.DefeatedBosses(area)))],
        BrokenChozoOrbs = [.. progress.ChozoOrbs.Members],
        CollectedItems = [.. progress.CollectedItems.Members],
        OpenedDoors = [.. progress.OpenedDoors.Members],
        UsedStations = [.. RetailAreas.Select(area => new SaveAreaStationsJsonDocument(area, [.. progress.UsedStations(area).Members]))],
        AcquiredMaps = [.. RetailAreas.Where(progress.HasAreaMap)],
        ExploredAreas = [.. RetailAreas.Select(area => new ExploredAreaJsonDocument(area,
            [.. progress.ExploredCells(area).Select(MapCoordinateJsonDocument.FromCell)]))],
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
            DebugFlag = slot.DebugMode,
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
            Progress = DecodeProgression(slot.Progression, "progression"),
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
        RequireRange(slot.DebugMode, SaveFieldValue.MaximumValue, expectedSlot, "cartridgeDebugFlag");
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
        if ((slot.Inventory.CollectedItems & ~AllEquipmentFlags) != SamusEquipmentFlags.None ||
            (slot.Inventory.EquippedItems & ~AllEquipmentFlags) != SamusEquipmentFlags.None ||
            (slot.Inventory.CollectedBeams & ~AllBeamFlags) != SamusBeamFlags.None ||
            (slot.Inventory.EquippedBeams & ~AllBeamFlags) != SamusBeamFlags.None)
            throw new InvalidDataException($"slots[{expectedSlot}].inventory contains unknown bits");
        if ((slot.Inventory.EquippedItems & ~slot.Inventory.CollectedItems) != SamusEquipmentFlags.None ||
            (slot.Inventory.EquippedBeams & ~slot.Inventory.CollectedBeams) != SamusBeamFlags.None)
            throw new InvalidDataException($"slots[{expectedSlot}].inventory equips an uncollected upgrade");
        new ControllerBindings(
            slot.Controller.Shoot,
            slot.Controller.Jump,
            slot.Controller.Dash,
            slot.Controller.ItemSelect,
            slot.Controller.ItemCancel,
            slot.Controller.AimUp,
            slot.Controller.AimDown).RequireRetailPermutation();

        _ = DecodeProgression(slot.Progression, $"slots[{expectedSlot}].progression");
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

    /// <summary>Every retail area, in area order.</summary>
    private static IEnumerable<AreaId> RetailAreas => Enumerable.Range(0, AreaIds.RetailCount).Select(AreaIds.At);

    /// <summary>Rebuilds the persistent progress a validated document lists, rejecting anything out of place.</summary>
    /// <param name="progression">The listed progress.</param>
    /// <param name="property">The document's path, named in errors.</param>
    private static PersistentProgress DecodeProgression(SaveProgressionJsonDocument progression, string property)
    {
        if (progression.Events is null || progression.DefeatedBosses is null || progression.BrokenChozoOrbs is null ||
            progression.CollectedItems is null || progression.OpenedDoors is null || progression.UsedStations is null ||
            progression.AcquiredMaps is null || progression.ExploredAreas is null)
            throw new InvalidDataException($"{property} is incomplete");
        var progress = new PersistentProgress();
        DecodeNumbers(DecodeEvents(progression.Events, $"{property}.events"), progress.Events, $"{property}.events");
        DecodeNumbers(progression.BrokenChozoOrbs, progress.ChozoOrbs, $"{property}.brokenChozoOrbs");
        DecodeNumbers(progression.CollectedItems, progress.CollectedItems, $"{property}.collectedItems");
        DecodeNumbers(progression.OpenedDoors, progress.OpenedDoors, $"{property}.openedDoors");
        foreach (AreaId area in DecodeAreaOrder(progression.DefeatedBosses.Select(entry => entry?.Area), $"{property}.defeatedBosses"))
        {
            BossBits bosses = progression.DefeatedBosses[AreaIds.ToIndex(area)].Bosses;
            if ((bosses & ~AllBossFlags) != BossBits.None)
                throw new InvalidDataException($"{property}.defeatedBosses names an unknown boss in {area}");
            progress.SetDefeatedBosses(area, bosses);
        }
        foreach (AreaId area in DecodeAreaOrder(progression.UsedStations.Select(entry => entry?.Area), $"{property}.usedStations"))
        {
            int[] stations = progression.UsedStations[AreaIds.ToIndex(area)].Stations ??
                throw new InvalidDataException($"{property}.usedStations of {area} is required");
            DecodeNumbers(stations, progress.UsedStations(area), $"{property}.usedStations of {area}");
        }
        int previousMap = -1;
        foreach (AreaId area in progression.AcquiredMaps)
        {
            int index = AreaIds.ToIndex(area);
            if (index <= previousMap)
                throw new InvalidDataException($"{property}.acquiredMaps must be in area order without duplicates");
            progress.SetAreaMap(area, acquired: true);
            previousMap = index;
        }
        foreach (AreaId area in DecodeAreaOrder(progression.ExploredAreas.Select(entry => entry?.Area), $"{property}.exploredAreas"))
        {
            MapCoordinateJsonDocument[] tiles = progression.ExploredAreas[AreaIds.ToIndex(area)].Tiles ??
                throw new InvalidDataException($"{property}.exploredAreas of {area} requires tiles");
            int previousIndex = -1;
            foreach (MapCoordinateJsonDocument tile in tiles)
            {
                if (tile is null || !AreaMapLayout.Size.Contains(tile.Cell))
                    throw new InvalidDataException($"{property}.exploredAreas of {area} contains an invalid tile");
                int logicalIndex = AreaMapLayout.Size.RowMajorIndex(tile.Cell);
                if (logicalIndex <= previousIndex)
                    throw new InvalidDataException($"{property}.exploredAreas of {area} must be row-major and unique");
                progress.SetExplored(area, tile.Cell, explored: true);
                previousIndex = logicalIndex;
            }
        }
        return progress;
    }

    /// <summary>Requires one entry per retail area, in area order, and returns the areas.</summary>
    private static List<AreaId> DecodeAreaOrder(IEnumerable<AreaId?> areas, string property)
    {
        var listed = new List<AreaId>();
        foreach (AreaId? entry in areas)
        {
            if (entry is not { } area || AreaIds.ToIndex(area) != listed.Count)
                throw new InvalidDataException($"{property}[{listed.Count}] is missing or out of area order");
            listed.Add(area);
        }
        if (listed.Count != AreaIds.RetailCount)
            throw new InvalidDataException($"{property} must contain {AreaIds.RetailCount} areas");
        return listed;
    }

    /// <summary>The event numbers validated symbolic event names list.</summary>
    private static int[] DecodeEvents(string[] events, string property)
    {
        var numbers = new int[events.Length];
        for (int index = 0; index < events.Length; index++)
        {
            string value = events[index] ?? throw new InvalidDataException($"{property}[{index}] is null");
            if (Enum.TryParse(value, ignoreCase: true, out EventNumber named) && Enum.IsDefined(named))
                numbers[index] = named.ToEventNumber();
            else if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int raw))
                numbers[index] = raw;
            else
                throw new InvalidDataException($"{property}[{index}] '{value}' is not a known name or 0xNN event number");
        }
        return numbers;
    }

    /// <summary>Turns on the listed flags, rejecting duplicates, disorder and numbers outside the range.</summary>
    private static void DecodeNumbers(int[] numbers, NumberedFlagSet flags, string property)
    {
        int previous = -1;
        for (int index = 0; index < numbers.Length; index++)
        {
            int number = numbers[index];
            if ((uint)number >= (uint)flags.Capacity)
                throw new InvalidDataException($"{property}[{index}]={number} is outside 0 through {flags.Capacity - 1}");
            if (number <= previous)
                throw new InvalidDataException($"{property} must be strictly increasing without duplicates");
            flags.Add(number);
            previous = number;
        }
    }
}
