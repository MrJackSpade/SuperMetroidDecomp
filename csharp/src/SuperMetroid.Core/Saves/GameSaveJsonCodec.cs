using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>Strict deterministic conversion between named JSON saves and the owned save store.</summary>
public static partial class GameSaveJsonCodec
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

    /// <summary>
    /// The save's Japanese-text field for each text language (#2560): the one map between a slot's language and the true or
    /// false field saves have always kept for it, read in both directions.
    /// </summary>
    private static readonly FrozenDictionary<TextLanguage, bool> SavedJapaneseText = new Dictionary<TextLanguage, bool>
    {
        [TextLanguage.English] = false,
        [TextLanguage.Japanese] = true,
    }.ToFrozenDictionary();

    /// <summary>The text language each value of a save's Japanese-text field names: <see cref="SavedJapaneseText"/> read back.</summary>
    private static readonly FrozenDictionary<bool, TextLanguage> SavedLanguages =
        SavedJapaneseText.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>Captures all meaningful persistent state without a duplicate raw save-memory image.</summary>
    public static GameSaveJsonDocument Capture(SaveSlotStore saves)
    {
        Ensure.NotNull(saves);
        var slots = new Dictionary<string, GameSaveSlotJsonDocument>(StringComparer.Ordinal);
        foreach (SaveSlot slot in SaveSlots.All)
        {
            if (saves.ReadSlot(slot) is { } saved)
                slots.Add(GameSaveJsonFormat.SlotKey(slot), CaptureSlot(saved));
        }

        return new GameSaveJsonDocument
        {
            SelectedSlot = GameSaveJsonFormat.SlotKey(saves.ReadSelectedSlot()),
            Slots = slots,
            History = saves.History,
        };
    }

    /// <summary>Serializes with stable declaration order, indentation, and symbolic enums.</summary>
    public static string Serialize(GameSaveJsonDocument document)
    {
        Validate(document);
        return JsonSerializer.Serialize(document, SaveJson.Info<global::SuperMetroid.Core.Saves.GameSaveJsonDocument>(CreateOptions())) + Environment.NewLine;
    }

    /// <summary>Parses one strict schema and reports malformed properties with JSON paths.</summary>
    public static GameSaveJsonDocument Deserialize(string json, string sourceName = "game save")
    {
        Ensure.NotNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        try
        {
            GameSaveJsonDocument document = JsonSerializer.Deserialize<GameSaveJsonDocument>(json, SaveJson.Info<global::SuperMetroid.Core.Saves.GameSaveJsonDocument>(CreateOptions())) ?? throw new InvalidDataException(
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
        foreach ((string key, GameSaveSlotJsonDocument slot) in document.Slots)
            saves.SaveSlot(GameSaveJsonFormat.SlotNamed(key), ToSnapshot(slot, key));
        saves.SelectSlot(GameSaveJsonFormat.SlotNamed(document.SelectedSlot));
        saves.RestoreHistory(document.History);
        return saves;
    }

    /// <summary>Projects one save slot into named resources, flags, events, and explored map cells.</summary>
    private static GameSaveSlotJsonDocument CaptureSlot(SuperMetroidSaveSlot slot) => new()
    {
        Checkpoint = slot.Checkpoint.Name,
        Resources = ResourcesOf(slot.Inventory),
        Inventory = new SaveInventoryJsonDocument(
            slot.Inventory.CollectedItems,
            slot.Inventory.EquippedItems,
            slot.Inventory.CollectedBeams,
            slot.Inventory.EquippedBeams),
        GameTime = new SaveGameTimeJsonDocument(
            slot.GameTime.Hours,
            slot.GameTime.Minutes,
            slot.GameTime.Seconds,
            slot.GameTime.Frames),
        Controller = ControllerDocument(slot.ControllerBindings),
        ReserveMode = slot.Inventory.ReserveMode,
        HudItem = slot.HudItem,
        MoonwalkEnabled = slot.MoonwalkEnabled,
        IconCancelEnabled = slot.IconCancelEnabled,
        JapaneseText = Ensure.Listed(SavedJapaneseText, slot.Language),
        StartupRoute = slot.StartupRoute,
        Progression = CaptureProgression(slot.Progression),
    };

    /// <summary>The save file's resources document of <paramref name="inventory"/>: its energy, ammunition and reserve with their capacities.</summary>
    private static SaveResourcesJsonDocument ResourcesOf(SamusInventory inventory) => new(
        inventory.Health,
        inventory.MaxHealth,
        inventory.Missiles,
        inventory.MaxMissiles,
        inventory.SuperMissiles,
        inventory.MaxSuperMissiles,
        inventory.PowerBombs,
        inventory.MaxPowerBombs,
        inventory.ReserveEnergy,
        inventory.MaxReserveEnergy);

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

    /// <summary>The save's record of the button each action is bound to, by the action's name.</summary>
    private static SaveControllerJsonDocument ControllerDocument(ControllerBindings bindings) => new(
        Shoot: bindings[ControllerAction.Shoot],
        Jump: bindings[ControllerAction.Jump],
        Dash: bindings[ControllerAction.Dash],
        ItemSelect: bindings[ControllerAction.ItemSelect],
        ItemCancel: bindings[ControllerAction.ItemCancel],
        AimUp: bindings[ControllerAction.AimUp],
        AimDown: bindings[ControllerAction.AimDown]);

    /// <summary>The bindings a save's controller record names, each action's button read by the action's name.</summary>
    /// <exception cref="InvalidDataException">The bindings are not a unique permutation of the assignable buttons.</exception>
    private static ControllerBindings Bindings(SaveControllerJsonDocument controller) => new ControllerBindings(new Dictionary<ControllerAction, ControllerButton>
    {
        [ControllerAction.Shoot] = controller.Shoot,
        [ControllerAction.Jump] = controller.Jump,
        [ControllerAction.Dash] = controller.Dash,
        [ControllerAction.ItemSelect] = controller.ItemSelect,
        [ControllerAction.ItemCancel] = controller.ItemCancel,
        [ControllerAction.AimUp] = controller.AimUp,
        [ControllerAction.AimDown] = controller.AimDown,
    }).RequireUniquePermutation();

    /// <summary>Reconstructs the save snapshot from one validated named slot, named <paramref name="key"/>.</summary>
    private static SuperMetroidSaveSnapshot ToSnapshot(GameSaveSlotJsonDocument slot, string key)
    {
        ValidateSlot(slot, key);
        ControllerBindings bindings = Bindings(slot.Controller);
        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = bindings,
            MoonwalkEnabled = slot.MoonwalkEnabled,
            IconCancelEnabled = slot.IconCancelEnabled,
            Language = SavedLanguages[slot.JapaneseText],
            StartupRoute = slot.StartupRoute,
            Inventory = new SamusInventory(
                slot.Inventory.EquippedItems,
                slot.Inventory.CollectedItems,
                slot.Inventory.EquippedBeams,
                slot.Inventory.CollectedBeams,
                slot.ReserveMode,
                slot.Resources.Health,
                slot.Resources.MaxHealth,
                slot.Resources.Missiles,
                slot.Resources.MaxMissiles,
                slot.Resources.SuperMissiles,
                slot.Resources.MaxSuperMissiles,
                slot.Resources.PowerBombs,
                slot.Resources.MaxPowerBombs,
                slot.Resources.MaxReserveEnergy,
                slot.Resources.ReserveEnergy),
            HudItem = slot.HudItem,
            GameTime = new GameClockReading(slot.GameTime.Frames, slot.GameTime.Seconds, slot.GameTime.Minutes, slot.GameTime.Hours),
            Checkpoint = Checkpoint(slot.Checkpoint, $"slots.{key}.checkpoint"),
            Progress = DecodeProgression(slot.Progression, "progression"),
        };
    }

    /// <summary>The load station a slot's checkpoint names.</summary>
    /// <exception cref="InvalidDataException">The checkpoint names no load station.</exception>
    private static LoadStation Checkpoint(string checkpoint, string property) =>
        LoadStationDefinitions.All.SingleOrDefault(station => string.Equals(station.Name, checkpoint, StringComparison.Ordinal))
            ?? throw new InvalidDataException($"{property} '{checkpoint}' names no load station");

    /// <summary>Requires a supported schema, a selected slot and saved slots that each name a slot of the game.</summary>
    private static void Validate(GameSaveJsonDocument document)
    {
        Ensure.NotNull(document);
        if (document.SchemaVersion != GameSaveJsonFormat.SchemaVersion)
        {
            throw new InvalidDataException(
                $"schemaVersion {document.SchemaVersion} is unsupported; expected " +
                $"{GameSaveJsonFormat.SchemaVersion}");
        }
        _ = GameSaveJsonFormat.SlotNamed(document.SelectedSlot);
        if (document.Slots is null)
            throw new InvalidDataException("slots is required");

        foreach ((string key, GameSaveSlotJsonDocument? slot) in document.Slots)
        {
            _ = GameSaveJsonFormat.SlotNamed(key);
            ValidateSlot(slot ?? throw new InvalidDataException($"slots.{key} is null; an empty slot has no entry"), key);
        }
    }

    /// <summary>Checks one slot's resource ranges, equipment flags, and map/event domains.</summary>
    private static void ValidateSlot(GameSaveSlotJsonDocument slot, string expectedSlot)
    {
        if (slot.Checkpoint is null || slot.Resources is null || slot.Inventory is null ||
            slot.GameTime is null || slot.Controller is null || slot.Progression is null)
            throw new InvalidDataException($"slots.{expectedSlot} is missing a required domain object");
        _ = Checkpoint(slot.Checkpoint, $"slots.{expectedSlot}.checkpoint");
        if (!Enum.IsDefined(slot.ReserveMode))
            throw new InvalidDataException($"slots.{expectedSlot}.reserveMode names no reserve-tank mode");
        if (!Enum.IsDefined(slot.HudItem))
            throw new InvalidDataException($"slots.{expectedSlot}.hudItem names no HUD item");
        if (!Enum.IsDefined(slot.StartupRoute))
            throw new InvalidDataException($"slots.{expectedSlot}.startupRoute names no startup route");
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
            throw new InvalidDataException($"slots.{expectedSlot}.resources.health exceeds maxHealth");
        if (slot.Resources.Missiles > slot.Resources.MaxMissiles ||
            slot.Resources.SuperMissiles > slot.Resources.MaxSuperMissiles ||
            slot.Resources.PowerBombs > slot.Resources.MaxPowerBombs ||
            slot.Resources.ReserveEnergy > slot.Resources.MaxReserveEnergy)
        {
            throw new InvalidDataException(
                $"slots.{expectedSlot}.resources contains a current value above its maximum");
        }
        if (!GameClockReading.IsReading(slot.GameTime.Frames, slot.GameTime.Seconds, slot.GameTime.Minutes, slot.GameTime.Hours))
            throw new InvalidDataException($"slots.{expectedSlot}.gameTime is not a reading of the game clock");
        if ((slot.Inventory.CollectedItems & ~AllEquipmentFlags) != SamusEquipmentFlags.None ||
            (slot.Inventory.EquippedItems & ~AllEquipmentFlags) != SamusEquipmentFlags.None ||
            (slot.Inventory.CollectedBeams & ~AllBeamFlags) != SamusBeamFlags.None ||
            (slot.Inventory.EquippedBeams & ~AllBeamFlags) != SamusBeamFlags.None)
            throw new InvalidDataException($"slots.{expectedSlot}.inventory names an item or beam the game does not have");
        if ((slot.Inventory.EquippedItems & ~slot.Inventory.CollectedItems) != SamusEquipmentFlags.None ||
            (slot.Inventory.EquippedBeams & ~slot.Inventory.CollectedBeams) != SamusBeamFlags.None)
            throw new InvalidDataException($"slots.{expectedSlot}.inventory equips an uncollected upgrade");
        _ = Bindings(slot.Controller);

        _ = DecodeProgression(slot.Progression, $"slots.{expectedSlot}.progression");
    }

    /// <summary>Rejects a numeric slot field outside <c>[<paramref name="minimum"/>, <paramref name="maximum"/>]</c>.</summary>
    private static void RequireRange(int value, int minimum, int maximum, string expectedSlot, string property)
    {
        if (value < minimum || value > maximum)
            throw new InvalidDataException($"slots.{expectedSlot}.{property} {value} is outside [{minimum}, {maximum}]");
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
        options.Converters.Add(new SaveKeyJsonConverter<SaveFileHistory>(SaveFileHistoryKeys.Installed));
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
        RequireAreaOrder(progression.DefeatedBosses.Select(entry => entry?.Area), $"{property}.defeatedBosses");
        foreach (SaveAreaBossesJsonDocument entry in progression.DefeatedBosses)
        {
            if ((entry.Bosses & ~AllDefeatableBosses) != DefeatedBosses.None)
                throw new InvalidDataException($"{property}.defeatedBosses names an unknown boss in {entry.Area}");
            progress.SetDefeatedBosses(entry.Area, entry.Bosses);
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
        RequireAreaOrder(progression.ExploredAreas.Select(entry => entry?.Area), $"{property}.exploredAreas");
        foreach (ExploredAreaJsonDocument entry in progression.ExploredAreas)
        {
            AreaId area = entry.Area;
            MapCoordinateJsonDocument[] tiles = entry.Tiles ??
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

    /// <summary>Requires one entry per area, each naming its area, in area order.</summary>
    private static void RequireAreaOrder(IEnumerable<AreaId?> areas, string property)
    {
        int listed = 0;
        foreach (AreaId? entry in areas)
        {
            if (entry is not { } area || ListPosition(area) != listed)
                throw new InvalidDataException($"{property}[{listed}] is missing or out of area order");
            listed++;
        }
        if (listed != SavedAreas.Count)
            throw new InvalidDataException($"{property} must contain {SavedAreas.Count} areas");
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
