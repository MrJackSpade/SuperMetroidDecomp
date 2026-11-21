using System.Text.Json.Nodes;
using SuperMetroid.Core.Saves;
using SuperMetroid.Tooling;

namespace SuperMetroid.Verification.Saves;

/// <summary>Older-schema saves built from a save the current schema writes, for the upgrade checks.</summary>
internal static class SaveSchemaFixtures
{
    /// <summary>The last schema that listed the slots, and numbered the selected one, by their place in the file select's list.</summary>
    private const int LastListedSlotsSchema = 16;

    /// <summary>The last schema that kept the save file's history as two marks.</summary>
    private const int LastHistoryMarksSchema = 15;

    /// <summary>
    /// <paramref name="json"/>, a save the current schema writes, stepped back to schema <paramref name="schemaVersion"/>:
    /// its slots listed as schemas one through sixteen listed them (#2565, <see cref="ToListedSlots"/>), then, for schema
    /// fifteen or earlier, the save file's history kept as schemas one through fifteen kept it, as two marks (#2560). Each
    /// check makes its own schema's other differences.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="schemaVersion"/> is not an earlier schema with listed slots.</exception>
    internal static JsonObject AtSchema(string json, int schemaVersion)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(schemaVersion, LastListedSlotsSchema);
        JsonObject save = RequiredJson.Parse(json).AsObject();
        ToListedSlots(save);
        if (schemaVersion <= LastHistoryMarksSchema)
        {
            string history = save.Required("history").GetValue<string>();
            save.Remove("history");
            save["gameCompleted"] = string.Equals(history, "gameCompleted", StringComparison.Ordinal);
            save["powerOnFoundNoValidSave"] = string.Equals(history, "powerOnFoundNoValidSave", StringComparison.Ordinal);
        }
        save["schemaVersion"] = schemaVersion;
        return save;
    }

    /// <summary>
    /// A copy of <paramref name="legacy"/>, an older-schema save from <see cref="AtSchema"/>, marked as the current schema,
    /// naming its history and keying its slots as the current schema does, so the current reader judges only the check's
    /// own difference.
    /// </summary>
    internal static JsonObject AsCurrent(JsonObject legacy)
    {
        JsonObject save = RequiredJson.Parse(legacy.ToJsonString()).AsObject();
        if (save.Required("schemaVersion").GetValue<int>() <= LastHistoryMarksSchema)
        {
            bool completed = save.Required("gameCompleted").GetValue<bool>();
            bool foundNone = save.Required("powerOnFoundNoValidSave").GetValue<bool>();
            save.Remove("gameCompleted");
            save.Remove("powerOnFoundNoValidSave");
            save["history"] = completed ? "gameCompleted" : foundNone ? "powerOnFoundNoValidSave" : "none";
        }
        ToNamedSlots(save);
        save["schemaVersion"] = GameSaveJsonFormat.SchemaVersion;
        return save;
    }

    /// <summary>
    /// Rewrites a current save's slots as schemas one through sixteen wrote them (#2565): the three slots listed in the file
    /// select's order, an empty slot as null and each saved slot numbering its place, and the selected slot numbered by its
    /// place.
    /// </summary>
    private static void ToListedSlots(JsonObject root)
    {
        JsonObject named = root.Required("slots").AsObject();
        var listed = new JsonArray();
        int place = 0;
        foreach (SaveSlot slot in SaveSlots.All)
        {
            if (named[GameSaveJsonFormat.SlotKey(slot)] is JsonObject saved)
            {
                var numbered = new JsonObject { ["slot"] = place };
                foreach ((string field, JsonNode? value) in saved)
                    numbered[field] = value?.DeepClone();
                listed.Add(numbered);
            }
            else
            {
                listed.Add(null);
            }
            place++;
        }
        root["slots"] = listed;
        SaveSlot selected = GameSaveJsonFormat.SlotNamed(root.Required("selectedSlot").GetValue<string>());
        root["selectedSlot"] = SaveSlots.All.ToList().IndexOf(selected);
    }

    /// <summary>
    /// Rewrites slots listed as schemas one through sixteen listed them (<see cref="ToListedSlots"/>) as the current schema
    /// keys them, each by its slot's name without its number, and names the selected slot.
    /// </summary>
    private static void ToNamedSlots(JsonObject root)
    {
        JsonArray listed = root.Required("slots").AsArray();
        var named = new JsonObject();
        int place = 0;
        foreach (SaveSlot slot in SaveSlots.All)
        {
            if (listed[place] is JsonObject saved)
            {
                var entry = new JsonObject();
                foreach ((string field, JsonNode? value) in saved)
                {
                    if (field != "slot")
                        entry[field] = value?.DeepClone();
                }
                named[GameSaveJsonFormat.SlotKey(slot)] = entry;
            }
            place++;
        }
        root["slots"] = named;
        root["selectedSlot"] = GameSaveJsonFormat.SlotKey(SaveSlots.All[root.Required("selectedSlot").GetValue<int>()]);
    }
}
