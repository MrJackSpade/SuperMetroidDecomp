using System.Text.Json.Nodes;
using SuperMetroid.Tooling;

namespace SuperMetroid.Verification.Saves;

/// <summary>Older-schema saves built from a save the current schema writes, for the upgrade checks.</summary>
internal static class SaveSchemaFixtures
{
    /// <summary>
    /// <paramref name="json"/>, a save the current schema writes, marked as schema <paramref name="schemaVersion"/> and
    /// keeping the save file's history as schemas one through fifteen kept it, as two marks (#2560). Each check makes
    /// its own schema's other differences.
    /// </summary>
    internal static JsonObject AtSchema(string json, int schemaVersion)
    {
        JsonObject save = RequiredJson.Parse(json).AsObject();
        string history = save.Required("history").GetValue<string>();
        save.Remove("history");
        save["gameCompleted"] = string.Equals(history, "gameCompleted", StringComparison.Ordinal);
        save["powerOnFoundNoValidSave"] = string.Equals(history, "powerOnFoundNoValidSave", StringComparison.Ordinal);
        save["schemaVersion"] = schemaVersion;
        return save;
    }
}
