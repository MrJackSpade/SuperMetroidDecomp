using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Shared focused confirmation for identified ordinary enemy composition omissions.</summary>
internal static class EnemyCompositionResourceChecks
{
    public static void Run(string family, byte bank, ReadOnlySpan<ushort> operands,
        int familyFrameCount, int previousVersion, int previousFrameCount, int? familyStart = null)
    {
        // Construct only presentation data. The dependency is selected by the
        // compiled program, not by a room probe or an animation search.
        var pointers = new HashSet<ushort>();
        foreach (ushort operand in operands)
        {
            Require(CompiledEnemyVisualSelectors.TryGet(bank, operand, out ushort pointer),
                $"compiled {family} visual operand {operand:X4} must resolve");
            if (CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer)) continue;
            pointers.Add(pointer);
        }
        EnemySpritemapDefinition[] expected = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.Version,
            Frames = expected.ToDictionary(frame => frame.Name,
                frame => new[] { Part(frame.Pointer & 0x7f) }, StringComparer.Ordinal),
            DisplayFrames = expected.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        EnemySpritemapCatalog stock = Load(document);
        foreach (ushort pointer in pointers)
        {
            Require(stock.TryGetDisplay(bank, pointer, out var parts),
                $"{family} sprite {bank:X2}:{pointer:X4} must load through the production display binding");
            Require(parts.Length == 1 && parts[0].X.SignedOffset == (pointer & 0x7f),
                $"{family} sprite {pointer:X4} must retain its selected composition");
        }
        Require(pointers.Count == familyFrameCount,
            "the selected frame set must match the documented native family");
        int firstFamilyFrame = familyStart ?? previousFrameCount;
        Require(expected.Skip(firstFamilyFrame).Take(familyFrameCount).All(frame =>
            frame.Bank == bank && pointers.Contains(frame.Pointer)) &&
            expected.Skip(firstFamilyFrame).Take(familyFrameCount).Select(frame => frame.Pointer).Distinct().Count() == pointers.Count,
            "the family's schema append must contain exactly the identified missing identities");
        EnemySpritemapDefinition edited = expected[0];
        EnemySpritemapDefinition[] previous = expected[..previousFrameCount];
        var legacy = document with
        {
            Version = previousVersion,
            Frames = previous.ToDictionary(frame => frame.Name,
                frame => frame == edited ? new[] { Part(-1) } : document.Frames[frame.Name], StringComparer.Ordinal),
            DisplayFrames = previous.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        EnemySpritemapCatalog merged = Load(legacy, stock);
        Require(merged.TryGetDisplay(edited.Bank, edited.Pointer, out var oldParts) &&
            oldParts[0].X.SignedOffset == -1, "previous-schema edits must survive inheritance");
        foreach (ushort pointer in pointers)
            Require(merged.TryGetDisplay(bank, pointer, out var parts) &&
                parts[0].X.SignedOffset == (pointer & 0x7f), "legacy overrides must inherit every new stock frame");
        bool rejected = false;
        try { _ = Load(legacy); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "incomplete previous-schema stock must request repair rather than silently omit frames");
        Console.WriteLine($"{family} resources: all {pointers.Count} compositions load; legacy edits survive, new stock inherits, incomplete stock is rejected.");
    }

    private static SpriteVisualPart Part(int x) => new()
    {
        OffsetX = x, OffsetY = 0, Size = 8, Priority = 2, Palette = 0,
        TileColumn = 0, TileRow = 0, FlipX = false, FlipY = false,
    };

    private static EnemySpritemapCatalog Load(EnemySpritemapDocument document,
        EnemySpritemapCatalog? stock = null) =>
        EnemySpritemapCatalog.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })), stock);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Enemy composition resource contract failed: " + message);
    }
}
