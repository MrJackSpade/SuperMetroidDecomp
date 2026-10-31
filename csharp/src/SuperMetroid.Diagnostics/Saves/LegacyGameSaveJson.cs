using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The one-way upgrade of older game saves to the schema Core reads: schema one (#1421) carried the cartridge's whole
/// SRAM image as raw pages, whose remaining fields are read back through <see cref="SaveRamCodec"/>; schema two (#1475)
/// listed bosses, used stations and acquired maps by the cartridge's table positions, which become lists by area.
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

    /// <summary>One-way upgrade of schemas one and two to the current schema; schemas past two are returned unchanged.</summary>
    private static string Upgrade(string json, out int sourceSchemaVersion)
    {
        JsonObject root = JsonNode.Parse(json) as JsonObject ??
            throw new InvalidDataException("Game save must be a JSON object.");
        if (root["schemaVersion"] is not JsonValue version || !version.TryGetValue(out sourceSchemaVersion))
            throw new InvalidDataException("schemaVersion must be an integer");
        if (sourceSchemaVersion > SchemaTwo) return json;
        if (sourceSchemaVersion < SchemaTwo)
            UpgradeSchemaOne(root);
        UpgradeSchemaTwo(root);
        root["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        return root.ToJsonString();
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
            for (int area = 0; area < AreaIds.RetailCount; area++)
            {
                string name = JsonNamingPolicy.CamelCase.ConvertName(AreaIds.At(area).ToString());
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
