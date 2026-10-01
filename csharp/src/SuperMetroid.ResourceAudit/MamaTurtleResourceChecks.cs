using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified tatori-family extraction and display dependency.</summary>
internal static class MamaTurtleResourceChecks
{
    public static void Run()
    {
        // Construct only presentation data. The dependency is selected by the
        // compiled program, not by a room probe or an animation search.
        Require(MamaTurtleEnemyDefinitionCatalog.TryGet(
            MamaTurtleEnemyDefinitionCatalog.MamaPointer, out RoomEnemyDefinition definition),
            "the production Mama Turtle header must exist");
        var pointers = new HashSet<ushort>();
        for (int index = 0; index < MamaTurtleInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index);
            Require(CompiledEnemyVisualSelectors.TryGet(definition.Bank, operand, out ushort pointer),
                $"compiled tatori visual operand {operand:X4} must resolve");
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
            Require(stock.TryGetDisplay(definition.Bank, pointer, out var parts),
                $"tatori sprite {definition.Bank:X2}:{pointer:X4} must load through the production display binding");
            Require(parts.Span.Length == 1 && parts.Span[0].X.SignedOffset == (pointer & 0x7f),
                $"tatori sprite {pointer:X4} must retain its selected composition");
        }
        Require(pointers.Count == MamaTurtleVisualDefinitions.FrameCount,
            "the selected frame set must match the documented native family");
        Require(expected.Length - EnemySpritemapDefinitions.PreMamaTurtleFrameCount == pointers.Count,
            "the schema append must add only the identified missing family");
        EnemySpritemapDefinition edited = expected[0];
        EnemySpritemapDefinition[] previous = expected[..EnemySpritemapDefinitions.PreMamaTurtleFrameCount];
        var legacy = document with
        {
            Version = EnemySpritemapDefinitions.PreMamaTurtleVersion,
            Frames = previous.ToDictionary(frame => frame.Name,
                frame => frame == edited ? new[] { Part(-1) } : document.Frames[frame.Name], StringComparer.Ordinal),
            DisplayFrames = previous.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        EnemySpritemapCatalog merged = Load(legacy, stock);
        Require(merged.TryGetDisplay(edited.Bank, edited.Pointer, out var oldParts) &&
            oldParts.Span[0].X.SignedOffset == -1, "schema-61 edits must survive inheritance");
        foreach (ushort pointer in pointers)
            Require(merged.TryGetDisplay(definition.Bank, pointer, out var parts) &&
                parts.Span[0].X.SignedOffset == (pointer & 0x7f), "legacy overrides must inherit every new stock frame");
        bool rejected = false;
        try { _ = Load(legacy); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "incomplete schema-61 stock must request repair rather than silently omit frames");
        Console.WriteLine($"Mama/Baby Turtle resources: all {pointers.Count} compositions load; legacy edits survive, new stock inherits, incomplete stock is rejected.");
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
        if (!condition) throw new InvalidOperationException("Mama Turtle resource contract failed: " + message);
    }
}
