using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-oam-v1", content =>
        {
            foreach ((int frame, EnemySpritemapParts parts) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.AppendEnemyParts(parts);
            }
            foreach ((int native, int selected) in displayFrames.OrderBy(pair => pair.Key))
            {
                content.Append("native-binding", native);
                content.Append("selected-binding", selected);
            }
        });

    /// <summary>Compiled OAM parts keyed by the native bank-and-pointer identity of each authored frame.</summary>
    internal readonly Dictionary<int, EnemySpritemapParts> frames;
    /// <summary>Visual-only mapping from each native frame identity to its selected displayed frame.</summary>
    private readonly Dictionary<int, int> displayFrames;

    /// <summary>Retains validated frame compositions and same-bank display selections.</summary>
    internal EnemySpritemapCatalog(Dictionary<int, EnemySpritemapParts> frames,
        Dictionary<int, int> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>
    /// Resolves an editable presentation binding without changing the native frame pointer
    /// retained by enemy AI, hitbox selection, or instruction timing.
    /// </summary>
    /// <param name="bank">Native enemy spritemap bank, combined with the same-bank pointer to identify the authored binding.</param>
    /// <param name="nativePointer">Unmodified native frame word retained by gameplay; the selected visual frame must belong to the same bank.</param>
    /// <param name="parts">Installed OAM composition selected by the binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</param>
    /// <returns>True when both a display binding and its selected composition exist; missing identities return false without reading cartridge data.</returns>
    public bool TryGetDisplay(byte bank, ushort nativePointer,
        out EnemySpritemapParts parts)
    {
        int identity = ((int)new SnesAddress(bank, nativePointer));
        if (displayFrames.TryGetValue(identity, out int selected) &&
            frames.TryGetValue(selected, out EnemySpritemapParts? found))
        {
            parts = found;
            return true;
        }
        parts = EnemySpritemapParts.Empty;
        return false;
    }

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
            : EnemySpritemapDefinitions.FrameCount(schema);
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
                visual is null || visual.Length > EnemySpritemapDefinitions.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} is missing or exceeds OAM capacity.");
            EnemySpritemapParts parts = BabyMetroidSpriteParts.Compile(((int)new SnesAddress(frame.Bank, frame.Pointer)),
                CompileParts(visual, frame.Name), ref sharedBabyBody);
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
                    (identity >> 16) != (selected >> 16))
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

    /// <summary>Validates OAM capacity, offsets, tile bounds, palette, size, and priority before packing ordinary or extended frame pieces.</summary>
    internal static SpritemapPart[] CompileParts(
        SpriteVisualPart[] visual, string frameName)
    {
        if (visual.Length > EnemySpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy composition {frameName} exceeds OAM part capacity.");
        var parts = new SpritemapPart[visual.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= EnemySpritemapDefinitions.TileColumns ||
                part.TileRow is < 0 or >= EnemySpritemapDefinitions.TileRows)
                throw new InvalidDataException(
                    $"Enemy composition {frameName} part {index} has invalid visual fields.");
            SnesTileFlipFlags flips =
                (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
            parts[index] = new SpritemapPart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                SnesSpritemapYByte.Create(part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemySpritemapDefinitions.TileColumns + part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    /// <summary>Rejects duplicate composition keys before JSON deserialization can overwrite an earlier edit.</summary>
    internal static void RejectDuplicateProperties(JsonElement element, StringComparer? propertyComparer = null) =>
        JsonAssetDocument.RejectDuplicateProperties(element, propertyComparer ?? StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate enemy composition property {name}."));
}
