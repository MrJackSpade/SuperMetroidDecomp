using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Serialization;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Reads the enemy OAM composition documents against the enemy-bank frame schema (#1395): import data whose parts are the
/// original's packed sprite placements (#1638), which only the import composes the enemy sheets from.
/// </summary>
public static class EnemySpritemapDocuments
{
    /// <summary>Validates authored frames and compiles visual-only fields into OAM parts.</summary>
    /// <param name="json">Caller-owned JSON stream, left open; duplicate and unknown properties are rejected.</param>
    /// <returns>Compiled visual parts and display bindings within the enemies showing each frame; collision, AI frame identity, and instruction timing are not imported from the artwork document.</returns>
    /// <exception cref="InvalidDataException">The schema, complete named-frame set, OAM fields/capacity, or display bindings are invalid.</exception>
    public static EnemySpritemapCatalog Load(Stream json)
    {
        Ensure.NotNull(json);
        EnemySpritemapDocument document;
        document = JsonAssetDocument.Read<EnemySpritemapDocument>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, }, "enemy composition");
        EnemySpritemapFrameList expected = EnemySpritemapDefinitions.Frames;
        if (document.Version != EnemySpritemapSchema.Revision || document.Frames is null || document.Frames.Count != expected.Count)
            throw new InvalidDataException(
                "Enemy compositions require the current version and every named frame.");

        var frames = new Dictionary<EnemyFrameId, EnemySpritemapParts>();
        foreach (EnemyFrameName frame in expected)
        {
            string authoredName = frame.Name;
            if (!document.Frames.TryGetValue(authoredName, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemySpritemapCompiler.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {authoredName} is missing or exceeds OAM capacity.");
            EnemySpritemapParts parts = EnemySpritemapParts.FromOwnedArray(EnemySpritemapCompiler.CompileParts(visual, frame.Name));
            if (!frames.TryAdd(frame.Frame, parts))
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} repeats a visual identity.");
        }
        return new EnemySpritemapCatalog(frames, EnemyFrameBindingRules.Ordinary(document.DisplayFrames));
    }
}
