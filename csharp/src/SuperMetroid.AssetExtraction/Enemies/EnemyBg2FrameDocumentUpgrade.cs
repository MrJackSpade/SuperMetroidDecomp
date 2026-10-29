using System.Collections.Immutable;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Carries a player's override of a Draygon or Crocomire BG2 frame document written at revision
/// <see cref="EnemyFrameLegacyNameDefinitions.LastBg2FrameRevision"/>, which named each frame by where the cartridge
/// stores it, onto the current revision's names for the same frames (#1465), so the override keeps loading. A document of
/// any other revision passes through unchanged for its loader to judge.
/// </summary>
internal static class EnemyBg2FrameDocumentUpgrade
{
    /// <summary>
    /// <paramref name="json"/> at revision <paramref name="version"/> with <paramref name="definitions"/>' current names
    /// when it is an older document naming them by <paramref name="legacyNames"/>, else <paramref name="json"/> itself.
    /// </summary>
    /// <param name="json">The selected stock or override document.</param>
    /// <param name="definitions">The frames the document holds, with their current names.</param>
    /// <param name="version">The current revision.</param>
    /// <param name="legacyNames">The name the older revision gave each frame it named differently.</param>
    /// <param name="family">The boss named in errors.</param>
    /// <exception cref="InvalidDataException">The document is not a JSON object, or an older one names a frame twice.</exception>
    internal static byte[] Upgrade(byte[] json, ImmutableArray<EnemySpritemapDefinition> definitions, int version,
        IReadOnlyDictionary<EnemyFrameId, string> legacyNames, string family)
    {
        Ensure.NotNull(legacyNames);
        string document = $"{family} BG2 frame document";
        JsonObject root = JsonFrameNames.Parse(json, document);
        if (root["version"] is not JsonValue stored || !stored.TryGetValue(out int revision) ||
            revision != EnemyFrameLegacyNameDefinitions.LastBg2FrameRevision)
            return json;
        Dictionary<string, string> current = definitions.ToDictionary(
            definition => legacyNames.TryGetValue(definition.Frame, out string? legacy) ? legacy : definition.Name,
            definition => definition.Name, StringComparer.Ordinal);
        // A name the older revision never gave stays as written, and the loader reports the frame it leaves missing.
        JsonFrameNames.Rename(root, "frames", name => current.TryGetValue(name, out string? renamed) ? renamed : name, document);
        root["version"] = version;
        return JsonFrameNames.Write(root);
    }
}
