using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    private readonly Dictionary<int, EnemySpritemapPart[]> frames;
    private readonly Dictionary<int, int> displayFrames;

    private EnemySpritemapCatalog(Dictionary<int, EnemySpritemapPart[]> frames,
        Dictionary<int, int> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>Returns a known installed frame; an unrelated enemy may still use its ROM path.</summary>
    public bool TryGet(byte bank, ushort pointer, out ReadOnlyMemory<EnemySpritemapPart> parts)
    {
        if (frames.TryGetValue((bank << 16) | pointer, out EnemySpritemapPart[]? found))
        {
            parts = found;
            return true;
        }
        parts = default;
        return false;
    }

    /// <summary>
    /// Resolves an editable presentation binding without changing the native frame pointer
    /// retained by enemy AI, hitbox selection, or instruction timing.
    /// </summary>
    public bool TryGetDisplay(byte bank, ushort nativePointer,
        out ReadOnlyMemory<EnemySpritemapPart> parts)
    {
        int identity = (bank << 16) | nativePointer;
        if (displayFrames.TryGetValue(identity, out int selected) &&
            frames.TryGetValue(selected, out EnemySpritemapPart[]? found))
        {
            parts = found;
            return true;
        }
        parts = default;
        return false;
    }

    /// <summary>
    /// Validates authored frames and compiles visual-only fields into OAM parts.
    /// A complete stock catalog permits a previous-version override to retain its
    /// existing edits while newly added frame identities come from stock content.
    /// </summary>
    public static EnemySpritemapCatalog Load(Stream json,
        EnemySpritemapCatalog? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        EnemySpritemapDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemySpritemapDocument>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            }) ?? throw new InvalidDataException("Enemy composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy composition JSON.", error);
        }
        int expectedCount = document.Version switch
        {
            EnemySpritemapDefinitions.PreCeresBabyVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            EnemySpritemapDefinitions.PreCeresDoorVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            EnemySpritemapDefinitions.PreDisplayBindingsVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreDisplayBindingsFrameCount,
            EnemySpritemapDefinitions.PreMagdolliteVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreMagdolliteFrameCount,
            EnemySpritemapDefinitions.PreFirefleaVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreFirefleaFrameCount,
            EnemySpritemapDefinitions.LegacyVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.LegacyFrameCount,
            EnemySpritemapDefinitions.IntermediateVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.IntermediateFrameCount,
            EnemySpritemapDefinitions.PreOwtchStokeVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreOwtchStokeFrameCount,
            EnemySpritemapDefinitions.PreRipperVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreRipperFrameCount,
            EnemySpritemapDefinitions.EarlierVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.EarlierFrameCount,
            EnemySpritemapDefinitions.PriorVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PriorFrameCount,
            EnemySpritemapDefinitions.PreviousVersion when stockForLegacyOverride is not null =>
                EnemySpritemapDefinitions.PreviousFrameCount,
            EnemySpritemapDefinitions.Version => EnemySpritemapDefinitions.Frames.Length,
            _ => -1,
        };
        bool legacyOverride = expectedCount >= 0 &&
            document.Version != EnemySpritemapDefinitions.Version;
        ReadOnlySpan<EnemySpritemapDefinition> expected = expectedCount >= 0
            ? EnemySpritemapDefinitions.Frames[..expectedCount]
            : [];
        if (expectedCount < 0 ||
            document.Frames is null || document.Frames.Count != expected.Length ||
            (legacyOverride && stockForLegacyOverride!.frames.Count !=
                EnemySpritemapDefinitions.Frames.Length))
            throw new InvalidDataException(
                "Enemy compositions require the current version and every named frame.");

        var frames = new Dictionary<int, EnemySpritemapPart[]>();
        var identities = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (EnemySpritemapDefinition frame in expected)
        {
            if (!document.Frames.TryGetValue(frame.Name, out SpriteVisualPart[]? visual) ||
                visual is null || visual.Length > EnemySpritemapDefinitions.MaximumParts)
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} is missing or exceeds OAM capacity.");
            EnemySpritemapPart[] parts = CompileParts(visual, frame.Name);
            if (!frames.TryAdd((frame.Bank << 16) | frame.Pointer, parts))
                throw new InvalidDataException(
                    $"Enemy composition {frame.Name} repeats a visual identity.");
            identities.Add(frame.Name, (frame.Bank << 16) | frame.Pointer);
        }
        var displayFrames = new Dictionary<int, int>();
        // Versions fourteen and fifteen already had editable display bindings. Keep
        // those user choices when newly added Ceres frames are supplied by stock;
        // older versions had only art and inherit all stock bindings.
        bool hasAuthoredBindings = !legacyOverride ||
            document.Version is EnemySpritemapDefinitions.PreCeresDoorVersion or
                EnemySpritemapDefinitions.PreCeresBabyVersion;
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
        var merged = new Dictionary<int, EnemySpritemapPart[]>(stockForLegacyOverride!.frames);
        foreach ((int identity, EnemySpritemapPart[] parts) in frames)
            merged[identity] = parts;
        var mergedBindings = new Dictionary<int, int>(stockForLegacyOverride.displayFrames);
        foreach ((int identity, int selected) in displayFrames)
            mergedBindings[identity] = selected;
        return new EnemySpritemapCatalog(merged, mergedBindings);
    }

    /// <summary>Compiles ordinary OAM pieces shared by plain and extended enemy frames.</summary>
    internal static EnemySpritemapPart[] CompileParts(
        SpriteVisualPart[] visual, string frameName)
    {
        if (visual.Length > EnemySpritemapDefinitions.MaximumParts)
            throw new InvalidDataException(
                $"Enemy composition {frameName} exceeds OAM part capacity.");
        var parts = new EnemySpritemapPart[visual.Length];
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
            parts[index] = new EnemySpritemapPart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                unchecked((byte)(sbyte)part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemySpritemapDefinitions.TileColumns + part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    internal static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException(
                        $"Duplicate enemy composition property {property.Name}.");
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (JsonElement item in element.EnumerateArray())
                RejectDuplicateProperties(item);
    }
}

/// <summary>Versioned, semantic enemy frame names mapped to editable OAM parts.</summary>
public sealed record EnemySpritemapDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Visual-only native-frame-to-displayed-frame bindings; never AI timing.</summary>
    public Dictionary<string, string>? DisplayFrames { get; init; }
}
