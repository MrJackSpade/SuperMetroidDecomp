using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>Strict deterministic conversion between named JSON saves and the owned save store.</summary>
public static class GameSaveJsonCodec
{
    /// <summary>Known equipment flags accepted by the named save schema.</summary>
    private static readonly SamusEquipmentFlags AllEquipmentFlags =
        Enum.GetValues<SamusEquipmentFlags>().Aggregate((left, right) => left | right);
    /// <summary>Known beam flags accepted by the named save schema.</summary>
    private static readonly SamusBeamFlags AllBeamFlags =
        Enum.GetValues<SamusBeamFlags>().Aggregate((left, right) => left | right);
    /// <summary>Known defeated bosses accepted by the named save schema.</summary>
    private static readonly DefeatedBosses AllDefeatableBosses =
        Enum.GetValues<DefeatedBosses>().Aggregate((left, right) => left | right);

    /// <summary>Captures all meaningful persistent state without a duplicate raw save-memory image.</summary>
    public static GameSaveJsonDocument Capture(SaveSlotStore saves)
    {
        Ensure.NotNull(saves);
        var slots = new GameSaveSlotJsonDocument?[SaveSlots.All.Count];
        for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
        {
            SuperMetroidSaveSlot? slot = saves.ReadSlot(SlotAt(slotIndex));
            slots[slotIndex] = slot is null ? null : CaptureSlot(slot);
        }

        return new GameSaveJsonDocument
        {
            SelectedSlot = SlotNumber(saves.ReadSelectedSlot()),
            Slots = slots,
            GameCompleted = saves.History == SaveFileHistory.GameCompleted,
            PowerOnFoundNoValidSave = saves.History == SaveFileHistory.PowerOnFoundNoValidSave,
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
                saves.SaveSlot(SlotAt(slotIndex), ToSnapshot(slot));
        }
        saves.SelectSlot(SlotAt(document.SelectedSlot));
        saves.RestoreHistory(document.GameCompleted ? SaveFileHistory.GameCompleted
            : document.PowerOnFoundNoValidSave ? SaveFileHistory.PowerOnFoundNoValidSave
            : SaveFileHistory.None);
        return saves;
    }

    /// <summary>Projects one save slot into named resources, flags, events, and explored map cells.</summary>
    private static GameSaveSlotJsonDocument CaptureSlot(SuperMetroidSaveSlot slot) => new()
    {
        Slot = SlotNumber(slot.Slot),
        Checkpoint = slot.Checkpoint.Name,
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
            slot.MaxReserveEnergy),
        Inventory = new SaveInventoryJsonDocument(
            slot.CollectedItems,
            slot.EquippedItems,
            slot.CollectedBeams,
            slot.EquippedBeams),
        GameTime = new SaveGameTimeJsonDocument(
            slot.GameTime.Hours,
            slot.GameTime.Minutes,
            slot.GameTime.Seconds,
            slot.GameTime.Frames),
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
        StartupRoute = slot.StartupRoute,
        Progression = CaptureProgression(slot.Progression),
    };

    /// <summary>Lists each set of <paramref name="progress"/> by its identities.</summary>
    private static SaveProgressionJsonDocument CaptureProgression(PersistentProgress progress) => new()
    {
        Events = [.. SavedEventOrder.Where(progress.HasEvent).Select(StoryEventKeys.Installed.Of)],
        ZebetitesDestroyed = progress.ZebetitesDestroyed,
        DefeatedBosses = [.. SavedAreas.Select(area => new SaveAreaBossesJsonDocument(area, progress.BossesDefeatedIn(area)))],
        CollectedItems = [.. progress.CollectedItems.Members.Select(location => location.Name)],
        OpenedDoors = [.. progress.OpenedDoors.Members.Select(door => door.Name)],
        UsedStations = [.. progress.UsedStations.Members.Select(station => station.Name)],
        AcquiredMaps = [.. SavedAreas.Where(progress.HasAreaMap)],
        ExploredAreas = [.. SavedAreas.Select(area => new ExploredAreaJsonDocument(area,
            [.. progress.ExploredCells(area).Select(MapCoordinateJsonDocument.FromCell)]))],
    };

    /// <summary>Reconstructs the save snapshot from one validated named slot.</summary>
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
        bindings.RequireUniquePermutation();
        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = bindings,
            MoonwalkEnabled = slot.MoonwalkEnabled,
            IconCancelEnabled = slot.IconCancelEnabled,
            JapaneseText = slot.JapaneseText,
            StartupRoute = slot.StartupRoute,
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
            GameTime = new GameClockReading(slot.GameTime.Frames, slot.GameTime.Seconds, slot.GameTime.Minutes, slot.GameTime.Hours),
            Checkpoint = Checkpoint(slot.Checkpoint, $"slots[{slot.Slot}].checkpoint"),
            Progress = DecodeProgression(slot.Progression, "progression"),
        };
    }

    /// <summary>The load station a slot's checkpoint names.</summary>
    /// <exception cref="InvalidDataException">The checkpoint names no load station.</exception>
    private static LoadStation Checkpoint(string checkpoint, string property) =>
        LoadStationDefinitions.All.SingleOrDefault(station => string.Equals(station.Name, checkpoint, StringComparison.Ordinal))
            ?? throw new InvalidDataException($"{property} '{checkpoint}' names no load station");

    /// <summary>The slot a JSON save numbers <paramref name="number"/>: its place in the file select's list, from zero.</summary>
    private static SaveSlot SlotAt(int number) => SaveSlots.All[number];

    /// <summary>The number a JSON save gives <paramref name="slot"/>: its place in the file select's list, from zero.</summary>
    private static int SlotNumber(SaveSlot slot)
    {
        for (int number = 0; number < SaveSlots.All.Count; number++)
            if (SaveSlots.All[number] == slot) return number;
        throw new ArgumentOutOfRangeException(nameof(slot), slot, "Undefined save slot.");
    }

    /// <summary>Requires a supported schema, valid selected slot, and exactly the game's slot count.</summary>
    private static void Validate(GameSaveJsonDocument document)
    {
        Ensure.NotNull(document);
        if (document.SchemaVersion != GameSaveJsonFormat.SchemaVersion)
        {
            throw new InvalidDataException(
                $"schemaVersion {document.SchemaVersion} is unsupported; expected " +
                $"{GameSaveJsonFormat.SchemaVersion}");
        }
        if ((uint)document.SelectedSlot >= SaveSlots.All.Count)
            throw new InvalidDataException("selectedSlot must be 0, 1, or 2");
        if (document.Slots is null || document.Slots.Length != SaveSlots.All.Count)
            throw new InvalidDataException("slots must contain exactly three entries");
        if (document.GameCompleted && document.PowerOnFoundNoValidSave)
            throw new InvalidDataException("gameCompleted and powerOnFoundNoValidSave cannot both hold");

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
        _ = Checkpoint(slot.Checkpoint, $"slots[{expectedSlot}].checkpoint");
        if (!Enum.IsDefined(slot.ReserveMode))
            throw new InvalidDataException($"slots[{expectedSlot}].reserveMode names no reserve-tank mode");
        if (!Enum.IsDefined(slot.HudItem))
            throw new InvalidDataException($"slots[{expectedSlot}].hudItem names no HUD item");
        if (!Enum.IsDefined(slot.StartupRoute))
            throw new InvalidDataException($"slots[{expectedSlot}].startupRoute names no startup route");
        SaveResourcesJsonDocument resources = slot.Resources;
        foreach ((int value, string property) in new SaveFieldCheck[]
                 {
                     new SaveFieldCheck(resources.Health, "health"), new SaveFieldCheck(resources.MaxHealth, "maxHealth"),
                     new SaveFieldCheck(resources.Missiles, "missiles"), new SaveFieldCheck(resources.MaxMissiles, "maxMissiles"),
                     new SaveFieldCheck(resources.SuperMissiles, "superMissiles"), new SaveFieldCheck(resources.MaxSuperMissiles, "maxSuperMissiles"),
                     new SaveFieldCheck(resources.PowerBombs, "powerBombs"), new SaveFieldCheck(resources.MaxPowerBombs, "maxPowerBombs"),
                     new SaveFieldCheck(resources.ReserveEnergy, "reserveEnergy"), new SaveFieldCheck(resources.MaxReserveEnergy, "maxReserveEnergy"),
                 })
            RequireRange(value, CountsRunBelowZero.MinimumValue, CountsRunBelowZero.MaximumValue, expectedSlot, $"resources.{property}");
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
        if (!GameClockReading.IsReading(slot.GameTime.Frames, slot.GameTime.Seconds, slot.GameTime.Minutes, slot.GameTime.Hours))
            throw new InvalidDataException($"slots[{expectedSlot}].gameTime is not a reading of the game clock");
        if ((slot.Inventory.CollectedItems & ~AllEquipmentFlags) != SamusEquipmentFlags.None ||
            (slot.Inventory.EquippedItems & ~AllEquipmentFlags) != SamusEquipmentFlags.None ||
            (slot.Inventory.CollectedBeams & ~AllBeamFlags) != SamusBeamFlags.None ||
            (slot.Inventory.EquippedBeams & ~AllBeamFlags) != SamusBeamFlags.None)
            throw new InvalidDataException($"slots[{expectedSlot}].inventory names an item or beam the game does not have");
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
            slot.Controller.AimDown).RequireUniquePermutation();

        _ = DecodeProgression(slot.Progression, $"slots[{expectedSlot}].progression");
    }

    /// <summary>Rejects a numeric slot field outside <c>[<paramref name="minimum"/>, <paramref name="maximum"/>]</c>.</summary>
    private static void RequireRange(int value, int minimum, int maximum, int expectedSlot, string property)
    {
        if (value < minimum || value > maximum)
            throw new InvalidDataException($"slots[{expectedSlot}].{property} {value} is outside [{minimum}, {maximum}]");
    }

    /// <summary>
    /// Configures strict enum names, each identity's written-out keys in camel case (#2520), property handling, and stable save
    /// JSON formatting.
    /// </summary>
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        };
        options.Converters.Add(new SaveKeyJsonConverter<ReserveTankMode>(ReserveTankModeKeys.Installed));
        options.Converters.Add(new SaveKeyJsonConverter<HudItem>(HudItemKeys.Installed));
        options.Converters.Add(new SaveKeyJsonConverter<SaveStartupRoute>(SaveStartupRouteKeys.Installed));
        options.Converters.Add(new SaveKeyJsonConverter<AreaId>(AreaIdKeys.Installed));
        options.Converters.Add(new SaveFlagsJsonConverter<ControllerButton>(ControllerButtonKeys.Installed, ControllerButtonKeys.ListedOrder, (left, right) => left | right));
        options.Converters.Add(new SaveFlagsJsonConverter<SamusEquipmentFlags>(SamusEquipmentFlagsKeys.Installed, SamusEquipmentFlagsKeys.ListedOrder, (left, right) => left | right));
        options.Converters.Add(new SaveFlagsJsonConverter<SamusBeamFlags>(SamusBeamFlagsKeys.Installed, SamusBeamFlagsKeys.ListedOrder, (left, right) => left | right));
        options.Converters.Add(new SaveFlagsJsonConverter<DefeatedBosses>(DefeatedBossesKeys.Installed, DefeatedBossesKeys.ListedOrder, (left, right) => left | right));
        return options;
    }

    /// <summary>Every area, in area order.</summary>
    private static IReadOnlyList<AreaId> SavedAreas => AreaIds.All;

    /// <summary>Rebuilds the persistent progress a validated document lists, rejecting anything out of place.</summary>
    /// <param name="progression">The listed progress.</param>
    /// <param name="property">The document's path, named in errors.</param>
    private static PersistentProgress DecodeProgression(SaveProgressionJsonDocument progression, string property)
    {
        if (progression.Events is null || progression.DefeatedBosses is null ||
            progression.CollectedItems is null || progression.OpenedDoors is null || progression.UsedStations is null ||
            progression.AcquiredMaps is null || progression.ExploredAreas is null)
            throw new InvalidDataException($"{property} is incomplete");
        var progress = new PersistentProgress();
        DecodeEvents(progression.Events, progress.Events, $"{property}.events");
        if (progression.ZebetitesDestroyed is < 0 or > PersistentProgress.ZebetiteCount)
            throw new InvalidDataException($"{property}.zebetitesDestroyed must be 0 through {PersistentProgress.ZebetiteCount}");
        progress.ZebetitesDestroyed = progression.ZebetitesDestroyed;
        DecodeNamed(progression.CollectedItems, ItemLocationDefinitions.All, location => location.Name, progress.CollectedItems, $"{property}.collectedItems");
        DecodeNamed(progression.OpenedDoors, LockedDoorDefinitions.All, door => door.Name, progress.OpenedDoors, $"{property}.openedDoors");
        foreach (AreaId area in DecodeAreaOrder(progression.DefeatedBosses.Select(entry => entry?.Area), $"{property}.defeatedBosses"))
        {
            DefeatedBosses bosses = progression.DefeatedBosses[ListPosition(area)].Bosses;
            if ((bosses & ~AllDefeatableBosses) != DefeatedBosses.None)
                throw new InvalidDataException($"{property}.defeatedBosses names an unknown boss in {area}");
            progress.SetDefeatedBosses(area, bosses);
        }
        DecodeNamed(progression.UsedStations, LoadStationDefinitions.Usable, station => station.Name, progress.UsedStations, $"{property}.usedStations");
        int? previousMap = null;
        foreach (AreaId area in progression.AcquiredMaps)
        {
            int index = ListPosition(area);
            if (previousMap is int previous && index <= previous)
                throw new InvalidDataException($"{property}.acquiredMaps must be in area order without duplicates");
            progress.SetAreaMap(area, acquired: true);
            previousMap = index;
        }
        foreach (AreaId area in DecodeAreaOrder(progression.ExploredAreas.Select(entry => entry?.Area), $"{property}.exploredAreas"))
        {
            MapCoordinateJsonDocument[] tiles = progression.ExploredAreas[ListPosition(area)].Tiles ??
                throw new InvalidDataException($"{property}.exploredAreas of {area} requires tiles");
            int? previousIndex = null;
            foreach (MapCoordinateJsonDocument tile in tiles)
            {
                if (tile is null || !AreaMapSize.Extent.Contains(tile.Cell))
                    throw new InvalidDataException($"{property}.exploredAreas of {area} contains an invalid tile");
                int logicalIndex = AreaMapSize.Extent.RowMajorIndex(tile.Cell);
                if (previousIndex is int previousCell && logicalIndex <= previousCell)
                    throw new InvalidDataException($"{property}.exploredAreas of {area} must be row-major and unique");
                progress.SetExplored(area, tile.Cell, explored: true);
                previousIndex = logicalIndex;
            }
        }
        return progress;
    }

    /// <summary>The position of <paramref name="area"/> in the save file's per-area lists, which follow <see cref="AreaIds.All"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="area"/> names no area.</exception>
    private static int ListPosition(AreaId area)
    {
        for (int position = 0; position < SavedAreas.Count; position++)
            if (SavedAreas[position] == area)
                return position;
        throw new InvalidDataException($"The save names {area}, which is no area.");
    }

    /// <summary>Requires one entry per area, in area order, and returns the areas.</summary>
    private static List<AreaId> DecodeAreaOrder(IEnumerable<AreaId?> areas, string property)
    {
        var listed = new List<AreaId>();
        foreach (AreaId? entry in areas)
        {
            if (entry is not { } area || ListPosition(area) != listed.Count)
                throw new InvalidDataException($"{property}[{listed.Count}] is missing or out of area order");
            listed.Add(area);
        }
        if (listed.Count != SavedAreas.Count)
            throw new InvalidDataException($"{property} must contain {SavedAreas.Count} areas");
        return listed;
    }

    /// <summary>The order a save lists the story events that have happened in, which lists every event once (#2543).</summary>
    private static readonly IReadOnlyList<StoryEvent> SavedEventOrder = Ensure.EveryMemberOnce(StoryEventKeys.SavedOrder);

    /// <summary>Every story event by its installed key, its case ignored as saves have always been read (#2520).</summary>
    private static readonly FrozenDictionary<string, StoryEvent> StoryEventsByKey =
        Enum.GetValues<StoryEvent>().ToFrozenDictionary(StoryEventKeys.Installed.Of, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Records the story events <paramref name="events"/> names as happened, in whatever order it lists them (#2543), so
    /// reordering <see cref="StoryEvent"/>'s members never stops a save loading; rejects an unknown name and a duplicate.
    /// </summary>
    private static void DecodeEvents(string[] events, ProgressSet<StoryEvent> happened, string property)
    {
        for (int index = 0; index < events.Length; index++)
        {
            string value = events[index] ?? throw new InvalidDataException($"{property}[{index}] is null");
            if (!StoryEventsByKey.TryGetValue(value, out StoryEvent storyEvent))
                throw new InvalidDataException($"{property}[{index}] '{value}' names no story event");
            if (happened.Contains(storyEvent))
                throw new InvalidDataException($"{property}[{index}] '{value}' lists an event listed before it");
            happened.Add(storyEvent);
        }
    }

    /// <summary>
    /// Records the members of <paramref name="kind"/> that <paramref name="names"/> lists as happened, in whatever order it
    /// lists them (#2543), so reordering the kind's listing never stops a save loading; rejects an unknown name and a
    /// duplicate.
    /// </summary>
    private static void DecodeNamed<T>(string[] names, IReadOnlyList<T> kind, Func<T, string> nameOf, ProgressSet<T> happened, string property)
        where T : notnull
    {
        for (int index = 0; index < names.Length; index++)
        {
            string name = names[index] ?? throw new InvalidDataException($"{property}[{index}] is null");
            int? named = null;
            for (int candidate = 0; candidate < kind.Count; candidate++)
            {
                if (string.Equals(nameOf(kind[candidate]), name, StringComparison.Ordinal))
                    named = candidate;
            }
            if (named is not int place)
                throw new InvalidDataException($"{property}[{index}] '{name}' names nothing the game places");
            if (happened.Contains(kind[place]))
                throw new InvalidDataException($"{property}[{index}] '{name}' lists a name listed before it");
            happened.Add(kind[place]);
        }
    }
}
