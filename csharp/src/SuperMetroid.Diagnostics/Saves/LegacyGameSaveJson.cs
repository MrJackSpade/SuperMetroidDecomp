using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The one-way upgrade of older game saves to the schema Core reads: schema one (#1421) carried the cartridge's whole
/// SRAM image as raw pages, whose remaining fields are read back through <see cref="SaveRamCodec"/>; schema two (#1475)
/// listed bosses, used stations and acquired maps by the cartridge's table positions, which become lists by area; schema
/// three (#1523) kept the cartridge's loading-state word, which becomes the startup route it selects; schema four
/// (#1527) listed the Zebetite count as three event bits, which become a count, and named an event after its number;
/// schema five (#1553) listed event-table bits no story event uses by number, and the Chozo orbs, item locations and
/// doors by room-data number, which become the locations and doors they number; schema six (#1605) kept the HUD
/// selection and reserve-tank mode as numbers, which become their names, and two settings the game never reads.
/// </summary>
public static class LegacyGameSaveJson
{
    /// <summary>How the legacy pages' records are named.</summary>
    private static readonly JsonSerializerOptions PageOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };

    /// <summary>Reads a game save of any supported schema, upgrading a schema-one save first.</summary>
    /// <param name="json">The save's JSON text.</param>
    /// <param name="sourceName">Where the save came from, named in errors.</param>
    /// <param name="migrated">Whether the save was an older schema, which its owner rewrites.</param>
    public static GameSaveJsonDocument Deserialize(string json, string sourceName, out bool migrated)
    {
        Ensure.NotNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        string current;
        int sourceSchemaVersion;
        try
        {
            current = Upgrade(json, out sourceSchemaVersion);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Game save '{sourceName}' is malformed at {exception.Path ?? "the root"}: " + exception.Message, exception);
        }
        catch (InvalidDataException exception) when (!exception.Message.Contains(sourceName, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Game save '{sourceName}' is invalid: {exception.Message}", exception);
        }
        migrated = sourceSchemaVersion < GameSaveJsonFormat.SchemaVersion;
        return GameSaveJsonCodec.Deserialize(current, sourceName);
    }

    /// <summary>One-way upgrade of schemas one through six to the current schema; the current schema is returned unchanged.</summary>
    private static string Upgrade(string json, out int sourceSchemaVersion)
    {
        JsonObject root = JsonNode.Parse(json) as JsonObject ??
            throw new InvalidDataException("Game save must be a JSON object.");
        if (root["schemaVersion"] is not JsonValue version || !version.TryGetValue(out sourceSchemaVersion))
            throw new InvalidDataException("schemaVersion must be an integer");
        if (sourceSchemaVersion >= GameSaveJsonFormat.SchemaVersion) return json;
        if (sourceSchemaVersion < SchemaTwo)
            UpgradeSchemaOne(root);
        if (sourceSchemaVersion <= SchemaTwo)
            UpgradeSchemaTwo(root);
        if (sourceSchemaVersion <= SchemaThree)
            UpgradeSchemaThree(root);
        if (sourceSchemaVersion <= SchemaFour)
            UpgradeSchemaFour(root);
        if (sourceSchemaVersion <= SchemaFive)
            UpgradeSchemaFive(root);
        UpgradeSchemaSix(root);
        root["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        return root.ToJsonString();
    }

    /// <summary>The schema that listed item locations and doors by room-data number.</summary>
    private const int SchemaFive = 5;

    /// <summary>
    /// Schema six to seven (#1605): schema six kept each slot's HUD selection as its place in the selection ring and its
    /// reserve-tank mode as none, auto or manual counted from zero (<see cref="SaveSlotNumbering"/>); both become their
    /// names, and a number naming no selection or mode is rejected, as schema six's loader rejected it. Schema six also
    /// carried <c>cartridgeDebugFlag</c>, the debug build's Samus-placement setting, and <c>newFileMarker</c>, a word a
    /// new file sets; no retail routine reads either, so, like schema two's padding, they are not carried.
    /// </summary>
    private static void UpgradeSchemaSix(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            slot["hudItem"] = EnumName(SaveSlotNumbering.SavedHudItem(TakeInteger(slot, "hudItem", index)));
            slot["reserveMode"] = EnumName(SaveSlotNumbering.SavedReserveMode(TakeInteger(slot, "reserveMode", index)));
            _ = TakeInteger(slot, "cartridgeDebugFlag", index);
            _ = TakeInteger(slot, "newFileMarker", index);
        }
    }

    /// <summary>Removes and returns a schema-six slot's integer property.</summary>
    private static int TakeInteger(JsonObject slot, string name, int index)
    {
        int value = slot[name] is JsonValue number && number.TryGetValue(out int parsed) ? parsed
            : throw new InvalidDataException($"slots[{index}].{name} must be an integer");
        slot.Remove(name);
        return value;
    }

    /// <summary>The name the current schema writes for <paramref name="value"/>.</summary>
    private static string EnumName<T>(T value) where T : struct, Enum => JsonNamingPolicy.CamelCase.ConvertName(value.ToString());

    /// <summary>The schema that kept the cartridge's loading-state word.</summary>
    private const int SchemaThree = 3;

    /// <summary>The schema that listed the Zebetite count as three event bits.</summary>
    private const int SchemaFour = 4;

    /// <summary>
    /// Schema five to six (#1553): schema five listed a bit of the event table that no story event uses as its number,
    /// <c>0xNN</c>, and listed the Chozo orbs, item locations and doors by the numbers room data gives them. The item
    /// locations and doors become the names of the locations and doors those numbers name. Nothing in the game reads an
    /// unnamed event bit, a Chozo orb, or an item or door number that no retail placement has (only SpaceTime's
    /// overwrite, a tester assist or an imported save image could set one), so, like schema two's padding, they are not
    /// carried; every named event, item location and door is kept as it was.
    /// </summary>
    private static void UpgradeSchemaFive(JsonObject root)
    {
        foreach ((JsonObject progression, int index) in Progressions(root))
        {
            JsonArray events = progression["events"] as JsonArray ??
                throw new InvalidDataException($"slots[{index}].progression.events is required");
            var named = new JsonArray();
            foreach (JsonNode? entry in events)
            {
                string name = entry is JsonValue value && value.TryGetValue(out string? text) ? text
                    : throw new InvalidDataException($"slots[{index}].progression.events lists an event that is not a string");
                if (!name.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    named.Add(name);
            }
            progression["events"] = named;
            if (progression["brokenChozoOrbs"] is not JsonArray)
                throw new InvalidDataException($"slots[{index}].progression.brokenChozoOrbs is required");
            progression.Remove("brokenChozoOrbs");
            progression["collectedItems"] = PlacementNames(progression, "collectedItems", index,
                number => SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.TryItem(number, out var location) ? location.Name : null);
            progression["openedDoors"] = PlacementNames(progression, "openedDoors", index,
                number => SuperMetroid.AssetExtraction.Progression.RoomPlacementNumbers.TryDoor(number, out var door) ? door.Name : null);
        }
    }

    /// <summary>
    /// The names of the placements a schema-five list numbers, in its order; a number no retail placement has names
    /// nothing the game reads and is not carried.
    /// </summary>
    private static JsonArray PlacementNames(JsonObject progression, string list, int slot, Func<int, string?> nameOf)
    {
        var names = new JsonArray();
        foreach (int number in Numbers(Take(progression, list, slot), slot))
        {
            if (nameOf(number) is { } name)
                names.Add(name);
        }
        return names;
    }

    /// <summary>Each saved slot's progression object and the slot's index.</summary>
    private static List<SlotProgression> Progressions(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        var progressions = new List<SlotProgression>();
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            progressions.Add(new SlotProgression(slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required"), index));
        }
        return progressions;
    }

    /// <summary>One saved slot's progression object.</summary>
    /// <param name="Progression">The slot's progression object.</param>
    /// <param name="Index">The slot's index.</param>
    private sealed record SlotProgression(JsonObject Progression, int Index);

    /// <summary>The event names through which schema four listed the Zebetite counter's bits, lowest bit first.</summary>
    private static readonly string[] ZebetiteCounterEventNames =
        ["ZebetiteDestroyedBit0", "ZebetiteDestroyedBit1", "ZebetiteDestroyedBit2"];

    /// <summary>The name schema four gave the event schema five names <see cref="StoryEvent.ShitroidEncounterMusicSkipped"/>.</summary>
    private const string ShitroidEncounterMusicSkippedSchemaFourName = "Unused14";

    /// <summary>
    /// Schema four to five (#1527): schema four listed the Zebetite count as the three events
    /// <c>ZebetiteDestroyedBit0</c> through <c>ZebetiteDestroyedBit2</c>, the bits of the cartridge's counter; they leave
    /// a slot's events and become its <c>zebetitesDestroyed</c> count, read as the cartridge's table codec reads the
    /// counter. The event schema four called <c>Unused14</c> is renamed to <see cref="StoryEvent.ShitroidEncounterMusicSkipped"/>.
    /// </summary>
    private static void UpgradeSchemaFour(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject progression = slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required");
            if (progression.ContainsKey("zebetitesDestroyed"))
                throw new InvalidDataException($"slots[{index}].progression.zebetitesDestroyed is not part of schema four");
            JsonArray events = progression["events"] as JsonArray ??
                throw new InvalidDataException($"slots[{index}].progression.events is required");
            var upgraded = new JsonArray();
            int counter = 0;
            foreach (JsonNode? entry in events)
            {
                string name = entry is JsonValue value && value.TryGetValue(out string? text) ? text
                    : throw new InvalidDataException($"slots[{index}].progression.events lists an event that is not a string");
                int bit = Array.FindIndex(ZebetiteCounterEventNames,
                    counterName => string.Equals(counterName, name, StringComparison.OrdinalIgnoreCase));
                if (bit >= 0)
                    counter |= 1 << bit;
                else if (string.Equals(name, ShitroidEncounterMusicSkippedSchemaFourName, StringComparison.OrdinalIgnoreCase))
                    upgraded.Add(nameof(StoryEvent.ShitroidEncounterMusicSkipped));
                else
                    upgraded.Add(name);
            }
            progression["events"] = upgraded;
            progression["zebetitesDestroyed"] = SuperMetroid.AssetExtraction.Progression.ProgressionTableCodec.ZebetitesDestroyed(counter);
        }
    }

    /// <summary>
    /// Schema three to four (#1523): a slot's <c>loadingGameState</c>, the cartridge's dispatcher word, becomes the
    /// <c>startupRoute</c> it selects; a word the dispatcher does not name resumes the main game, as the cartridge does.
    /// A slot without the word resumes the main game.
    /// </summary>
    private static void UpgradeSchemaThree(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is not JsonObject slot)
                continue;
            if (slot["loadingGameState"] is not JsonNode word)
                continue;
            int value = word is JsonValue number && number.TryGetValue(out int parsed) ? parsed
                : throw new InvalidDataException($"slots[{index}].loadingGameState must be an integer");
            slot.Remove("loadingGameState");
            slot["startupRoute"] = JsonNamingPolicy.CamelCase.ConvertName(
                SuperMetroid.AssetExtraction.Progression.SaveLoadingGameStateCodec.Route(value).ToString());
        }
    }

    /// <summary>The schema that listed progress as bit positions of the cartridge's tables.</summary>
    private const int SchemaTwo = 2;

    /// <summary>Bits a schema-two station or map table byte held.</summary>
    private const int BitsPerTableByte = 8;

    /// <summary>
    /// Schema two to three (#1475): schema two listed bosses by the cartridge's eight area slots, used stations as bit
    /// positions of the sixteen-byte station table and acquired maps as bit positions of the twelve-byte map-station
    /// table. Schema three lists each by area: the station table's bit <c>n</c> is station <c>n % 16</c> of area
    /// <c>n / 16</c>, and an area's map is acquired when any bit of its map-station byte is set. The eighth area slot,
    /// which names no area, and the map-station table's padding bytes hold nothing the game reads and are not carried.
    /// The orb, item and door lists are renamed; their numbers are already room-data numbers.
    /// </summary>
    private static void UpgradeSchemaTwo(JsonObject root)
    {
        JsonArray slots = root["slots"] as JsonArray ?? throw new InvalidDataException("slots is required");
        for (int index = 0; index < slots.Count; index++)
        {
            if (slots[index] is null) continue;
            JsonObject slot = slots[index] as JsonObject ?? throw new InvalidDataException($"slots[{index}] must be an object");
            JsonObject progression = slot["progression"] as JsonObject ??
                throw new InvalidDataException($"slots[{index}].progression is required");
            JsonArray bosses = Take(progression, "bossFlagsByAreaIndex", index);
            JsonArray stations = Take(progression, "usedSaveStationBits", index);
            JsonArray maps = Take(progression, "acquiredMapStationBits", index);
            progression["brokenChozoOrbs"] = Take(progression, "roomChozoBits", index);
            progression["collectedItems"] = Take(progression, "collectedItemBits", index);
            progression["openedDoors"] = Take(progression, "openedDoorBits", index);
            var defeated = new JsonArray();
            var used = new JsonArray();
            var acquired = new JsonArray();
            for (int area = 0; area < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; area++)
            {
                string name = JsonNamingPolicy.CamelCase.ConvertName(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area).ToString());
                defeated.Add(new JsonObject
                {
                    ["area"] = name,
                    ["bosses"] = bosses.Count > area ? bosses[area]?.DeepClone() : null,
                });
                var areaStations = new JsonArray();
                bool mapAcquired = false;
                foreach (int bit in Numbers(stations, index))
                {
                    if (bit / PersistentProgress.StationsPerArea == area)
                        areaStations.Add(bit % PersistentProgress.StationsPerArea);
                }
                foreach (int bit in Numbers(maps, index))
                    mapAcquired |= bit / BitsPerTableByte == area;
                used.Add(new JsonObject { ["area"] = name, ["stations"] = areaStations });
                if (mapAcquired)
                    acquired.Add(name);
            }
            progression["defeatedBosses"] = defeated;
            progression["usedStations"] = used;
            progression["acquiredMaps"] = acquired;
        }
    }

    /// <summary>Removes and returns a schema-two progression list.</summary>
    private static JsonArray Take(JsonObject progression, string name, int slot)
    {
        JsonArray list = progression[name] as JsonArray ??
            throw new InvalidDataException($"slots[{slot}].progression.{name} is required");
        progression.Remove(name);
        return list;
    }

    /// <summary>The integers a schema-two bit-position list holds.</summary>
    private static IEnumerable<int> Numbers(JsonArray list, int slot) =>
        list.Select(entry => entry is JsonValue value && value.TryGetValue(out int number)
            ? number
            : throw new InvalidDataException($"slots[{slot}].progression lists a bit position that is not an integer"));

    /// <summary>Schema one to two: reads the fields schema one left in its preserved image back through the codec.</summary>
    private static void UpgradeSchemaOne(JsonObject root)
    {
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
            resources["reserveMissiles"] = Word(SaveRamLayout.ReserveMissilesOffset);
            slot["japaneseText"] = Word(SaveRamLayout.JapaneseTextOffset) != 0;
            slot["loadedItemCount"] = Word(SaveRamLayout.LoadedItemCountOffset);
            ushort Word(int offset) => SaveRamCodec.ReadSlotWord(sram, index, offset);
        }
        root["gameCompleted"] = sram.AsSpan(SaveRamLayout.CompletionMarkerOffset,
            SaveRamLayout.CompletionMarker.Length).SequenceEqual(SaveRamLayout.CompletionMarker);
        root.Remove("preservedUntranslatedSram");
    }

    /// <summary>Bytes per page of the save image schema one preserved; no later schema has pages.</summary>
    private const int PreservationPageSize = 256;

    /// <summary>Validates the older page-preservation format before upgrading named save data.</summary>
    private static byte[] DecodeLegacyPages(JsonArray pages)
    {
        int size = PreservationPageSize;
        int expectedPages = SaveRamLayout.ImageByteCount / size;
        if (pages.Count != expectedPages)
            throw new InvalidDataException($"preservedUntranslatedSram must contain {expectedPages} pages");
        var sram = new byte[SaveRamLayout.ImageByteCount];
        for (int page = 0; page < pages.Count; page++)
        {
            var entry = pages[page]?.Deserialize<LegacySramPage>(PageOptions) ??
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
