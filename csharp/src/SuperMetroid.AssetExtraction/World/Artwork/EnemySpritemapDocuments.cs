using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Serialization;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Reads installed enemy OAM composition documents against the enemy-bank frame schema (#1395).</summary>
public static class EnemySpritemapDocuments
{
    /// <summary>The enemies showing each frame.</summary>
    private static readonly Lazy<ILookup<EnemyFrameId, EnemyDefinitionId>> Showing = new(() =>
        EnemyDefinitionFrameDefinitions.Frames.SelectMany(entry => entry.Value, (entry, frame) => KeyValuePair.Create(frame, entry.Key))
            .ToLookup(shown => shown.Key, shown => shown.Value));

    /// <summary>
    /// Validates authored frames and compiles visual-only fields into OAM parts.
    /// A complete stock catalog permits a previous-version override to retain its
    /// existing edits while newly added frame identities come from stock content.
    /// </summary>
    /// <param name="json">Caller-owned JSON stream, left open; duplicate and unknown properties are rejected.</param>
    /// <param name="stockForLegacyOverride">Optional complete current stock catalog, required to accept supported older schemas and supply their missing newer frame identities/bindings.</param>
    /// <returns>Compiled visual parts and display bindings within the enemies showing each frame; collision, AI frame identity, and instruction timing are not imported from the artwork document.</returns>
    /// <exception cref="InvalidDataException">The schema, complete named-frame set, OAM fields/capacity, or display bindings are invalid.</exception>
    public static EnemySpritemapCatalog Load(Stream json,
        EnemySpritemapCatalog? stockForLegacyOverride = null)
    {
        Ensure.NotNull(json);
        EnemySpritemapDocument document;
        document = JsonAssetDocument.Read<EnemySpritemapDocument>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, }, "enemy composition");
        EnemySpritemapSchema schema = EnemySpritemapSchema.FromRevision(document.Version) ??
            throw new InvalidDataException("Enemy compositions require the current version and every named frame.");
        bool legacyOverride = schema != EnemySpritemapSchema.Current;
        if (legacyOverride && stockForLegacyOverride is null)
            throw new InvalidDataException("Enemy compositions require the current version and every named frame.");
        ReadOnlySpan<EnemySpritemapDefinition> expected = EnemySpritemapDefinitions.Frames[..schema.FrameCount];
        if (document.Frames is null || document.Frames.Count != expected.Length ||
            (legacyOverride && (stockForLegacyOverride ?? throw new InvalidDataException("A legacy enemy composition override requires the current stock catalog.")).selection.Frames.Count !=
                EnemySpritemapDefinitions.Frames.Length))
            throw new InvalidDataException(
                "Enemy compositions require the current version and every named frame.");

        var frames = new Dictionary<EnemyFrameId, EnemySpritemapParts>();
        BabyMetroidSpriteParts? sharedBabyBody = null;
        var identities = new Dictionary<string, EnemyFrameId>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition frame in expected)
        {
            if (!document.Frames.TryGetValue(frame.Name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemySpritemapCompiler.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} is missing or exceeds OAM capacity.");
            EnemySpritemapParts parts = BabyMetroidSpriteParts.Compile(frame.Frame,
                EnemySpritemapCompiler.CompileParts(visual, frame.Name), ref sharedBabyBody);
            parts = ChozoStrideArtwork.Compile(frame.Frame, parts);
            if (!frames.TryAdd(frame.Frame, parts))
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} repeats a visual identity.");
            identities.Add(frame.Name, frame.Frame);
        }
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        // Every accepted schema after the binding boundary owns its authored
        // selection; only earlier art-only schemas inherit stock bindings.
        if (schema.OwnsDisplayBindings)
        {
            if (document.DisplayFrames is null ||
                document.DisplayFrames.Count != identities.Count)
                throw new InvalidDataException(
                    "Enemy display bindings require every named native frame.");
            foreach ((string name, EnemyFrameId identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out EnemyFrameId selected))
                    throw new InvalidDataException(
                        $"Enemy display binding {name} must select a named frame.");
                // Each enemy's sheet composes the selected frame over that enemy's own characters, which only the
                // frames it shows were authored against (#1392).
                if (Showing.Value[identity].Except(Showing.Value[selected]).Any())
                    throw new InvalidDataException(
                        $"Enemy display binding {name} must select a frame every enemy showing it shows.");
                displayFrames.Add(identity, selected);
            }
        }
        if (!legacyOverride)
            return new EnemySpritemapCatalog(frames, displayFrames);
        var merged = new Dictionary<EnemyFrameId, EnemySpritemapParts>((stockForLegacyOverride ?? throw new InvalidDataException("A legacy enemy composition override requires the current stock catalog.")).selection.Frames);
        foreach ((EnemyFrameId identity, EnemySpritemapParts parts) in frames)
            merged[identity] = parts;
        var mergedBindings = new Dictionary<EnemyFrameId, EnemyFrameId>(stockForLegacyOverride.selection.DisplayFrames);
        foreach ((EnemyFrameId identity, EnemyFrameId selected) in displayFrames)
            mergedBindings[identity] = selected;
        return new EnemySpritemapCatalog(merged, mergedBindings);
    }
}
