using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>Reads installed enemy OAM composition documents against the enemy-bank frame schema (#1395).</summary>
public static class EnemySpritemapDocuments
{
    /// <summary>
    /// Validates authored frames and compiles visual-only fields into OAM parts.
    /// A complete stock catalog permits a previous-version override to retain its
    /// existing edits while newly added frame identities come from stock content.
    /// </summary>
    /// <param name="json">Caller-owned JSON stream, left open; duplicate and unknown properties are rejected.</param>
    /// <param name="stockForLegacyOverride">Optional complete current stock catalog, required to accept supported older schemas and supply their missing newer frame identities/bindings.</param>
    /// <returns>Compiled visual parts and same-bank display bindings; collision, AI frame identity, and instruction timing are not imported from the artwork document.</returns>
    /// <exception cref="InvalidDataException">The schema, complete named-frame set, OAM fields/capacity, or display bindings are invalid.</exception>
    public static EnemySpritemapCatalog Load(Stream json,
        EnemySpritemapCatalog? stockForLegacyOverride = null)
    {
        Ensure.NotNull(json);
        EnemySpritemapDocument document;
        document = JsonAssetDocument.Read<EnemySpritemapDocument>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, }, "enemy composition");
        var schema = (EnemySpritemapSchema)document.Version;
        int expectedCount = !Enum.IsDefined(schema) ||
            (schema != EnemySpritemapSchema.Current && stockForLegacyOverride is null)
            ? -1
            : EnemySpritemapFrameCounts.FrameCount(schema);
        bool legacyOverride = expectedCount >= 0 && schema != EnemySpritemapSchema.Current;
        ReadOnlySpan<EnemySpritemapDefinition> expected = expectedCount >= 0
            ? EnemySpritemapDefinitions.Frames[..expectedCount]
            : [];
        if (expectedCount < 0 ||
            document.Frames is null || document.Frames.Count != expected.Length ||
            (legacyOverride && (stockForLegacyOverride ?? throw new InvalidDataException("A legacy enemy composition override requires the current stock catalog.")).frames.Count !=
                EnemySpritemapDefinitions.Frames.Length))
            throw new InvalidDataException(
                "Enemy compositions require the current version and every named frame.");

        var frames = new Dictionary<int, EnemySpritemapParts>();
        BabyMetroidSpriteParts? sharedBabyBody = null;
        var identities = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition frame in expected)
        {
            if (!document.Frames.TryGetValue(frame.Name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemySpritemapCatalog.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} is missing or exceeds OAM capacity.");
            EnemySpritemapParts parts = BabyMetroidSpriteParts.Compile(((int)new SnesAddress(frame.Bank, frame.Pointer)),
                EnemySpritemapCatalog.CompileParts(visual, frame.Name), ref sharedBabyBody);
            parts = ChozoStrideGeometryDefinitions.Compile(((int)new SnesAddress(frame.Bank, frame.Pointer)), parts);
            if (!frames.TryAdd(((int)new SnesAddress(frame.Bank, frame.Pointer)), parts))
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} repeats a visual identity.");
            identities.Add(frame.Name, ((int)new SnesAddress(frame.Bank, frame.Pointer)));
        }
        var displayFrames = new Dictionary<int, int>();
        // Schema/count validation above has already rejected unknown versions.
        // Every accepted schema after the binding boundary owns its authored
        // selection; only earlier art-only schemas inherit stock bindings.
        // Do not maintain a second version list that can silently lose new edits.
        bool hasAuthoredBindings =
            document.Version > (int)EnemySpritemapSchema.PreDisplayBindings;
        if (hasAuthoredBindings)
        {
            if (document.DisplayFrames is null ||
                document.DisplayFrames.Count != identities.Count)
                throw new InvalidDataException(
                    "Enemy display bindings require every named native frame.");
            foreach ((string name, int identity) in identities)
            {
                if (!document.DisplayFrames.TryGetValue(name, out string? selectedName) ||
                    selectedName is null ||
                    !identities.TryGetValue(selectedName, out int selected) ||
                    SnesAddress.FromBusAddress(identity).Bank != SnesAddress.FromBusAddress(selected).Bank)
                    throw new InvalidDataException(
                        $"Enemy display binding {name} must select a named frame in the same bank.");
                displayFrames.Add(identity, selected);
            }
        }
        if (!legacyOverride)
            return new EnemySpritemapCatalog(frames, displayFrames);
        var merged = new Dictionary<int, EnemySpritemapParts>((stockForLegacyOverride ?? throw new InvalidDataException("A legacy enemy composition override requires the current stock catalog.")).frames);
        foreach ((int identity, EnemySpritemapParts parts) in frames)
            merged[identity] = parts;
        var mergedBindings = new Dictionary<int, int>(stockForLegacyOverride.displayFrames);
        foreach ((int identity, int selected) in displayFrames)
            mergedBindings[identity] = selected;
        return new EnemySpritemapCatalog(merged, mergedBindings);
    }
}
