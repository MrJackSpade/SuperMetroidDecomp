using System.Text.Json;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

public static partial class GameSaveJsonCodec
{
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
        if (slots.Count != SuperMetroidSaveRam.SlotCount)
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
            ushort Word(int offset) => (ushort)(sram[origin + offset] | sram[origin + offset + 1] << 8);
        }
        root["gameCompleted"] = sram.AsSpan(SaveRamLayout.CompletionMarkerOffset,
            SaveRamLayout.CompletionMarker.Length).SequenceEqual(SaveRamLayout.CompletionMarker);
        root.Remove("preservedUntranslatedSram");
        root["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        return root.ToJsonString();
    }

    /// <summary>Validates and concatenates the legacy save's ordered hexadecimal SRAM pages.</summary>
    /// <param name="pages">The preserved SRAM page array from schema version one.</param>
    /// <returns>The complete SRAM image reconstructed at each page's declared offset.</returns>
    /// <exception cref="InvalidDataException">The page count, offset, hexadecimal payload, or decoded page length is invalid.</exception>
    private static byte[] DecodeLegacyPages(JsonArray pages)
    {
        int size = GameSaveJsonFormat.PreservationPageByteCount;
        int expectedPages = SuperMetroidAddressSpace.SaveRamByteCount / size;
        if (pages.Count != expectedPages)
            throw new InvalidDataException($"preservedUntranslatedSram must contain {expectedPages} pages");
        var sram = new byte[SuperMetroidAddressSpace.SaveRamByteCount];
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

    /// <summary>One legacy JSON page pairing its SRAM offset with the page's hexadecimal bytes.</summary>
    private sealed record LegacySramPage
    {
        /// <summary>Hexadecimal SRAM address of this page, formatted as a four-digit offset.</summary>
        public required string Offset { get; init; }
        /// <summary>Page contents encoded as hexadecimal text and decoded during legacy import.</summary>
        public required string Bytes { get; init; }
    }
}
