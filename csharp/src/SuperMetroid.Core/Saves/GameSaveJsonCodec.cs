using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Rooms.RoomObjects;

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
    /// <summary>Known defeated bosses accepted by the named save schema.</summary>
    private static readonly DefeatedBosses AllBossFlags =
        Enum.GetValues<DefeatedBosses>().Aggregate((left, right) => left | right);

    /// <summary>Captures all meaningful persistent state without a duplicate raw save-memory image.</summary>
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

    /// <summary>Projects one save slot into named resources, flags, events, and explored map cells.</summary>
    private static GameSaveSlotJsonDocument CaptureSlot(SuperMetroidSaveSlot slot) => new()
    {
        Slot = slot.Slot,
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
        Events = [.. progress.Events.Members.Select(storyEvent => storyEvent.ToString())],
        ZebetitesDestroyed = progress.ZebetitesDestroyed,
        DefeatedBosses = [.. RetailAreas.Select(area => new SaveAreaBossesJsonDocument(area, progress.BossesDefeatedIn(area)))],
        CollectedItems = [.. progress.CollectedItems.Members.Select(location => location.Name)],
        OpenedDoors = [.. progress.OpenedDoors.Members.Select(door => door.Name)],
        UsedStations = [.. progress.UsedStations.Members.Select(station => station.Name)],
        AcquiredMaps = [.. RetailAreas.Where(progress.HasAreaMap)],
        ExploredAreas = [.. RetailAreas.Select(area => new ExploredAreaJsonDocument(area,
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
        bindings.RequireRetailPermutation();
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
            ReserveMissiles = slot.Resources.ReserveMissiles,
            GameTimeFrames = slot.GameTime.Frames,
            GameTimeSeconds = slot.GameTime.Seconds,
            GameTimeMinutes = slot.GameTime.Minutes,
            GameTimeHours = slot.GameTime.Hours,
            Checkpoint = Checkpoint(slot.Checkpoint, $"slots[{slot.Slot}].checkpoint"),
            Progress = DecodeProgression(slot.Progression, "progression"),
        };
    }

    /// <summary>The load station a slot's checkpoint names.</summary>
    /// <exception cref="InvalidDataException">The checkpoint names no load station.</exception>
    private static LoadStation Checkpoint(string checkpoint, string property) =>
        LoadStationDefinitions.All.SingleOrDefault(station => string.Equals(station.Name, checkpoint, StringComparison.Ordinal))
            ?? throw new InvalidDataException($"{property} '{checkpoint}' names no load station");

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
                     new SaveFieldCheck(resources.ReserveMissiles, "reserveMissiles"),
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
        if (!GameTimeState.IsInDomain(slot.GameTime.Frames, slot.GameTime.Seconds, slot.GameTime.Minutes, slot.GameTime.Hours))
            throw new InvalidDataException($"slots[{expectedSlot}].gameTime is not a reading of the game clock");
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
    /// Rejects a numeric slot field outside <c>[0, <paramref name="maximum"/>]</c>, the range its domain or its saved
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
    private static IReadOnlyList<AreaId> RetailAreas => AreaIds.All;

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
            if ((bosses & ~AllBossFlags) != DefeatedBosses.None)
                throw new InvalidDataException($"{property}.defeatedBosses names an unknown boss in {area}");
            progress.SetDefeatedBosses(area, bosses);
        }
        DecodeNamed(progression.UsedStations, LoadStationDefinitions.Usable, station => station.Name, progress.UsedStations, $"{property}.usedStations");
        int previousMap = -1;
        foreach (AreaId area in progression.AcquiredMaps)
        {
            int index = ListPosition(area);
            if (index <= previousMap)
                throw new InvalidDataException($"{property}.acquiredMaps must be in area order without duplicates");
            progress.SetAreaMap(area, acquired: true);
            previousMap = index;
        }
        foreach (AreaId area in DecodeAreaOrder(progression.ExploredAreas.Select(entry => entry?.Area), $"{property}.exploredAreas"))
        {
            MapCoordinateJsonDocument[] tiles = progression.ExploredAreas[ListPosition(area)].Tiles ??
                throw new InvalidDataException($"{property}.exploredAreas of {area} requires tiles");
            int previousIndex = -1;
            foreach (MapCoordinateJsonDocument tile in tiles)
            {
                if (tile is null || !AreaMapSize.Extent.Contains(tile.Cell))
                    throw new InvalidDataException($"{property}.exploredAreas of {area} contains an invalid tile");
                int logicalIndex = AreaMapSize.Extent.RowMajorIndex(tile.Cell);
                if (logicalIndex <= previousIndex)
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
        for (int position = 0; position < RetailAreas.Count; position++)
            if (RetailAreas[position] == area)
                return position;
        throw new InvalidDataException($"The save names {area}, which is no area.");
    }

    /// <summary>Requires one entry per retail area, in area order, and returns the areas.</summary>
    private static List<AreaId> DecodeAreaOrder(IEnumerable<AreaId?> areas, string property)
    {
        var listed = new List<AreaId>();
        foreach (AreaId? entry in areas)
        {
            if (entry is not { } area || ListPosition(area) != listed.Count)
                throw new InvalidDataException($"{property}[{listed.Count}] is missing or out of area order");
            listed.Add(area);
        }
        if (listed.Count != RetailAreas.Count)
            throw new InvalidDataException($"{property} must contain {RetailAreas.Count} areas");
        return listed;
    }

    /// <summary>
    /// Records the story events <paramref name="events"/> names as happened, rejecting an unknown name, a duplicate and
    /// any name out of the order <see cref="StoryEvent"/> declares.
    /// </summary>
    private static void DecodeEvents(string[] events, ProgressSet<StoryEvent> happened, string property)
    {
        StoryEvent[] order = Enum.GetValues<StoryEvent>();
        int previous = -1;
        for (int index = 0; index < events.Length; index++)
        {
            string value = events[index] ?? throw new InvalidDataException($"{property}[{index}] is null");
            if (!Enum.TryParse(value, ignoreCase: true, out StoryEvent storyEvent) || !Enum.IsDefined(storyEvent) ||
                int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                throw new InvalidDataException($"{property}[{index}] '{value}' names no story event");
            int place = Array.IndexOf(order, storyEvent);
            if (place <= previous)
                throw new InvalidDataException($"{property} must list each event once, in the order the game lists them");
            happened.Add(storyEvent);
            previous = place;
        }
    }

    /// <summary>
    /// Records the members of <paramref name="kind"/> that <paramref name="names"/> lists as happened, rejecting an unknown
    /// name, a duplicate and any name out of the kind's order.
    /// </summary>
    private static void DecodeNamed<T>(string[] names, IReadOnlyList<T> kind, Func<T, string> nameOf, ProgressSet<T> happened, string property)
        where T : notnull
    {
        int previous = -1;
        for (int index = 0; index < names.Length; index++)
        {
            string name = names[index] ?? throw new InvalidDataException($"{property}[{index}] is null");
            int place = -1;
            for (int candidate = 0; candidate < kind.Count; candidate++)
            {
                if (string.Equals(nameOf(kind[candidate]), name, StringComparison.Ordinal))
                    place = candidate;
            }
            if (place < 0)
                throw new InvalidDataException($"{property}[{index}] '{name}' names nothing the game places");
            if (place <= previous)
                throw new InvalidDataException($"{property} must list each once, in the order room data lists them");
            happened.Add(kind[place]);
            previous = place;
        }
    }
}
