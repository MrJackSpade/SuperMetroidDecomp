using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms a statically identified extended-frame omission through the production loader.</summary>
internal static class ExtendedEnemyCompositionResourceChecks
{
    /// <summary>Confirms an extended enemy family loads and merges across its schema boundary.</summary>
    internal static void Run(string family, EnemyExtendedFrameDefinition[] selected,
        int previousVersion, int previousCount)
    {
        EnemyExtendedFrameDefinition[] expected = EnemyExtendedFrameDefinitions.Frames.ToArray();
        EnemyExtendedFrameDocument document = CreateDocument();
        EnemyExtendedFrameCatalog stock = Load(document);
        foreach (var frame in selected) Confirm(stock, frame);
        Require(expected.Skip(previousCount).Take(selected.Length).SequenceEqual(selected),
            "schema append must contain exactly the statically identified frame set");

        EnemyExtendedFrameDefinition edited = expected[0];
        EnemyExtendedFrameDefinition alternate = expected[1];
        var previous = expected[..previousCount];
        var legacy = document with
        {
            Version = previousVersion,
            Frames = previous.ToDictionary(frame => frame.Name, frame => frame == alternate
                ? new[] { Component(-1) } : document.Frames[frame.Name]),
            DisplayFrames = previous.ToDictionary(frame => frame.Name,
                frame => frame == edited ? alternate.Name : frame.Name),
        };
        EnemyExtendedFrameCatalog merged = Load(legacy, stock);
        Require(merged.TryGetDisplay(edited.Bank, edited.Pointer, out var old) &&
            old.Span[0].OffsetX == -1 && merged.GetDisplayPointer(edited.Bank, edited.Pointer) == alternate.Pointer,
            "legacy component edits and display bindings must survive stock inheritance");
        foreach (var frame in selected) Confirm(merged, frame);
        bool rejected = false;
        try { _ = Load(legacy); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "incomplete old stock must request repair rather than omit the new family");
        Console.WriteLine($"{family}: {selected.Length} extended compositions load; legacy component edits and bindings survive; old stock is rejected.");
    }

    /// <summary>Creates a deterministic extended-frame component at the specified offset.</summary>
    internal static EnemyExtendedVisualComponent Component(int x) => new()
    {
        OffsetX = x, OffsetY = -x,
        Parts = [new SpriteVisualPart
        {
            OffsetX = 0, OffsetY = 0, Size = 8, Priority = 2, Palette = 0,
            TileColumn = 0, TileRow = 0, FlipX = false, FlipY = false,
        }],
    };

    /// <summary>Creates a complete deterministic extended-frame artwork document.</summary>
    internal static EnemyExtendedFrameDocument CreateDocument()
    {
        var expected = EnemyExtendedFrameDefinitions.Frames.ToArray();
        return new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameDefinitions.Version,
            Frames = expected.ToDictionary(frame => frame.Name, frame =>
                EnemyExtendedFrameDefinitions.IsBg2Only(frame) ? [] : new[] { Component(frame.Pointer & 0x7f) }),
            DisplayFrames = expected.ToDictionary(frame => frame.Name, frame => frame.Name),
        };
    }

    /// <summary>Loads an extended-frame document with an optional stock fallback.</summary>
    internal static EnemyExtendedFrameCatalog Load(EnemyExtendedFrameDocument document,
        EnemyExtendedFrameCatalog? stock = null) => EnemyExtendedFrameCatalog.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })), stock);

    /// <summary>Confirms one extended frame resolves its installed display component.</summary>
    private static void Confirm(EnemyExtendedFrameCatalog catalog, EnemyExtendedFrameDefinition frame)
    {
        Require(catalog.TryGetDisplay(frame.Bank, frame.Pointer, out var components),
            $"extended sprite {frame.Bank:X2}:{frame.Pointer:X4} must have an installed display binding");
        Require(components.Span.Length == 1 && components.Span[0].OffsetX == (frame.Pointer & 0x7f) &&
            components.Span[0].OffsetY == -(frame.Pointer & 0x7f) && components.Span[0].Parts.Length == 1,
            "the selected component offsets and OAM pieces must be preserved");
    }

    /// <summary>Throws when an extended composition contract expectation is not satisfied.</summary>
    internal static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Extended composition contract failed: " + message);
    }
}
