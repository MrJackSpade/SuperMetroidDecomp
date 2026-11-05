using SuperMetroid.AssetExtraction.World.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Shared focused confirmation for identified ordinary enemy composition omissions.</summary>
internal static class EnemyCompositionResourceChecks
{
    /// <summary>Checks a compiled enemy family has every selected composition.</summary>
    public static void Run(string family, byte bank, ReadOnlySpan<ushort> operands,
        int familyFrameCount, int previousFrameCount, int? familyStart = null)
    {
        // Construct only presentation data. The dependency is selected by the
        // compiled program, not by a room probe or an animation search.
        var spritemaps = new List<ushort>(operands.Length);
        foreach (ushort operand in operands)
        {
            Require(CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort pointer),
                $"compiled {family} visual operand {operand:X4} must resolve");
            spritemaps.Add(pointer);
        }
        RunSpritemaps(family, bank, spritemaps, familyFrameCount, previousFrameCount, familyStart);
    }

    /// <summary>Checks the compositions a typed program shows (#641), as <see cref="Run"/> does for resolved operands.</summary>
    public static void RunSpritemaps(string family, byte bank, IEnumerable<ushort> spritemaps,
        int familyFrameCount, int previousFrameCount, int? familyStart = null)
    {
        var pointers = new HashSet<ushort>();
        foreach (ushort pointer in spritemaps)
        {
            if (CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(bank, pointer)) continue;
            pointers.Add(pointer);
        }
        EnemySpritemapDefinition[] expected = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapSchema.Revision,
            Frames = expected.ToDictionary(frame => frame.Name,
                frame => new[] { Part(frame.Pointer & 0x7f) }, StringComparer.Ordinal),
            DisplayFrames = expected.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        EnemySpritemapCatalog stock = Load(document);
        foreach (ushort pointer in pointers)
        {
            Require(stock.TryGetDisplay(bank, pointer, out var parts),
                $"{family} sprite {bank:X2}:{pointer:X4} must load through the production display binding");
            Require(parts.Length == 1 && parts[0].Offset.X == (pointer & 0x7f),
                $"{family} sprite {pointer:X4} must retain its selected composition");
        }
        Require(pointers.Count == familyFrameCount,
            "the selected frame set must match the documented native family");
        int firstFamilyFrame = familyStart ?? previousFrameCount;
        Require(expected.Skip(firstFamilyFrame).Take(familyFrameCount).All(frame =>
            frame.Bank == bank && pointers.Contains(frame.Pointer)) &&
            expected.Skip(firstFamilyFrame).Take(familyFrameCount).Select(frame => frame.Pointer).Distinct().Count() == pointers.Count,
            "the family's schema append must contain exactly the identified missing identities");
        Console.WriteLine($"{family} resources: all {pointers.Count} compositions load.");
    }

    /// <summary>Creates one distinguishable synthetic sprite part for a composition binding.</summary>
    private static SpriteVisualPart Part(int x) => new()
    {
        OffsetX = x, OffsetY = 0, Size = 8, Priority = 2, Palette = 0,
        TileColumn = 0, TileRow = 0, FlipX = false, FlipY = false,
    };

    /// <summary>Loads a constructed composition document.</summary>
    private static EnemySpritemapCatalog Load(EnemySpritemapDocument document) =>
        EnemySpritemapDocuments.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));

    /// <summary>Reports a failed composition resource contract with its specific reason.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Enemy composition resource contract failed: " + message);
    }
}
