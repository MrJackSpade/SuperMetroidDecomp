using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One visual component of an extended enemy frame, without hitbox metadata.</summary>
internal readonly record struct EnemyExtendedDrawComponent(
    short OffsetX, short OffsetY, ReadOnlyMemory<EnemySpritemapPart> Parts);

/// <summary>
/// Installed visual compositions for extended enemy frames. Hitbox records and
/// callback pointers remain application-owned and never come from this JSON.
/// </summary>
public sealed class EnemyExtendedFrameCatalog
{
    private readonly Dictionary<int, EnemyExtendedDrawComponent[]> frames;

    private EnemyExtendedFrameCatalog(
        Dictionary<int, EnemyExtendedDrawComponent[]> frames) => this.frames = frames;

    internal bool TryGet(byte bank, ushort pointer,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        // Bank-$B2's common empty extended frame has no OAM parts. It is a
        // compiled draw identity, not user artwork, including during the first
        // frame after an enemy slot is initialized.
        if (bank == EnemyExtendedFrameDefinitions.Bank &&
            pointer == EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap)
        {
            components = ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
            return true;
        }
        if (frames.TryGetValue((bank << 16) | pointer,
                out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>
    /// Validates named frames and compiles visual-only OAM pieces. A complete
    /// current stock catalog lets version-one/two overrides retain their edits
    /// while newly introduced frame families come from verified stock.
    /// </summary>
    public static EnemyExtendedFrameCatalog Load(Stream json,
        EnemyExtendedFrameCatalog? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        EnemyExtendedFrameDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyExtendedFrameDocument>(
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                }) ?? throw new InvalidDataException("Extended enemy composition JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid extended enemy composition JSON.", error);
        }
        int expectedCount = document.Version switch
        {
            EnemyExtendedFrameDefinitions.FirstVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.WalkingFrameCount,
            EnemyExtendedFrameDefinitions.PreviousVersion
                when stockForLegacyOverride is not null =>
                EnemyExtendedFrameDefinitions.WalkingFrameCount +
                EnemyExtendedFrameDefinitions.WallFrameCount,
            EnemyExtendedFrameDefinitions.Version =>
                EnemyExtendedFrameDefinitions.ExpectedFrameCount,
            _ => -1,
        };
        bool legacyOverride = expectedCount >= 0 &&
            expectedCount != EnemyExtendedFrameDefinitions.ExpectedFrameCount;
        if (expectedCount < 0 || document.Frames is null ||
            document.Frames.Count != expectedCount ||
            (legacyOverride && stockForLegacyOverride!.frames.Count !=
                EnemyExtendedFrameDefinitions.ExpectedFrameCount))
            throw new InvalidDataException(
                "Extended enemy compositions require the current version and every named frame.");
        ReadOnlySpan<EnemyExtendedFrameDefinition> expected =
            EnemyExtendedFrameDefinitions.Frames[..expectedCount];

        var frames = new Dictionary<int, EnemyExtendedDrawComponent[]>();
        foreach (EnemyExtendedFrameDefinition definition in expected)
        {
            if (!document.Frames.TryGetValue(definition.Name,
                    out EnemyExtendedVisualComponent[]? visual) ||
                visual is null || visual.Length is < 1 or >
                    EnemyExtendedFrameDefinitions.MaximumComponents)
                throw new InvalidDataException(
                    $"Extended enemy frame {definition.Name} is missing or exceeds component capacity.");
            var compiled = new EnemyExtendedDrawComponent[visual.Length];
            int totalParts = 0;
            for (int index = 0; index < visual.Length; index++)
            {
                EnemyExtendedVisualComponent? component = visual[index];
                if (component is null || component.OffsetX is < short.MinValue or > short.MaxValue ||
                    component.OffsetY is < short.MinValue or > short.MaxValue ||
                    component.Parts is null)
                    throw new InvalidDataException(
                        $"Extended enemy frame {definition.Name} component {index} is invalid.");
                EnemySpritemapPart[] parts = EnemySpritemapCatalog.CompileParts(
                    component.Parts, $"{definition.Name} component {index}");
                totalParts += parts.Length;
                if (totalParts > EnemySpritemapDefinitions.MaximumParts)
                    throw new InvalidDataException(
                        $"Extended enemy frame {definition.Name} exceeds OAM capacity.");
                compiled[index] = new EnemyExtendedDrawComponent(
                    (short)component.OffsetX, (short)component.OffsetY, parts);
            }
            if (!frames.TryAdd((definition.Bank << 16) | definition.Pointer,
                    compiled))
                throw new InvalidDataException(
                    $"Extended enemy frame {definition.Name} repeats a visual identity.");
        }
        if (!legacyOverride)
            return new EnemyExtendedFrameCatalog(frames);
        var merged = new Dictionary<int, EnemyExtendedDrawComponent[]>(
            stockForLegacyOverride!.frames);
        foreach ((int identity, EnemyExtendedDrawComponent[] components) in frames)
            merged[identity] = components;
        return new EnemyExtendedFrameCatalog(merged);
    }
}

/// <summary>Editable, visual-only coordinates and OAM parts of one component.</summary>
public sealed record EnemyExtendedVisualComponent
{
    public required int OffsetX { get; init; }
    public required int OffsetY { get; init; }
    public required SpriteVisualPart[] Parts { get; init; }
}

/// <summary>Versioned walking/wall/ninja-Pirate extended-frame compositions.</summary>
public sealed record EnemyExtendedFrameDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, EnemyExtendedVisualComponent[]> Frames { get; init; }
}
