using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Diagnostics.Saves;

/// <summary>
/// The one-way upgrade of schema-one game saves (#1421), whose JSON carried the cartridge's whole SRAM image as raw
/// pages: the fields that schema left in the image are read back through <see cref="SaveRamCodec"/>, and the save
/// becomes the current schema Core reads.
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

    /// <summary>One-way import of the old full-SRAM envelope; current output has no raw pages.</summary>
    private static string Upgrade(string json, out int sourceSchemaVersion)
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
            resources["reserveMissiles"] = Word(SaveRamLayout.ReserveMissilesOffset);
            slot["japaneseText"] = Word(SaveRamLayout.JapaneseTextOffset) != 0;
            slot["loadedItemCount"] = Word(SaveRamLayout.LoadedItemCountOffset);
            ushort Word(int offset) => SaveRamCodec.ReadSlotWord(sram, index, offset);
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
