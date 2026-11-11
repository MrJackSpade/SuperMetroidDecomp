using SuperMetroid.AssetExtraction.World.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms a statically identified extended-frame omission through the production loader.</summary>
internal static class ExtendedEnemyCompositionResourceChecks
{
    /// <summary>Checks newly added frames load from a complete document.</summary>
    internal static void Run(string family, EnemyFrameName[] selected, int previousCount)
    {
        EnemyFrameName[] expected = EnemyExtendedFrameDefinitions.Frames.ToArray();
        EnemyExtendedFrameDocument document = CreateDocument();
        EnemyExtendedFrameCatalog stock = Load(document);
        foreach (var frame in selected) Confirm(stock, frame);
        Require(expected.Skip(previousCount).Take(selected.Length).SequenceEqual(selected),
            "schema append must contain exactly the statically identified frame set");
        Console.WriteLine($"{family}: {selected.Length} extended compositions load.");
    }

    /// <summary>Constructs a one-part visual component with distinguishable X and Y offsets.</summary>
    internal static EnemyExtendedVisualComponent Component(int x) => new()
    {
        OffsetX = x, OffsetY = -x,
        Parts = [new SpriteVisualPart
        {
            OffsetX = 0, OffsetY = 0, Size = 8, Priority = 2, Palette = 0,
            TileColumn = 0, TileRow = 0, FlipX = false, FlipY = false,
        }],
    };

    /// <summary>Builds a complete current-schema frame and display-binding document for the closed frame set.</summary>
    internal static EnemyExtendedFrameDocument CreateDocument()
    {
        var expected = EnemyExtendedFrameDefinitions.Frames.ToArray();
        return new EnemyExtendedFrameDocument
        {
            Version = EnemyExtendedFrameSchema.Revision,
            Frames = expected.ToDictionary(frame => frame.Name, frame =>
                EnemyExtendedFrameBounds.IsBg2Only(frame) ? [] : new[] { Component(frame.Pointer & 0x7f) }),
            DisplayFrames = expected.ToDictionary(frame => frame.Name, frame => frame.Name),
        };
    }

    /// <summary>Loads an in-memory document through the production catalog parser.</summary>
    internal static EnemyExtendedFrameCatalog Load(EnemyExtendedFrameDocument document) => EnemyExtendedFrameDocuments.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));

    /// <summary>Checks a selected frame resolves to its expected component offset and single OAM part.</summary>
    private static void Confirm(EnemyExtendedFrameCatalog catalog, EnemyFrameName frame)
    {
        Require(catalog.TryGetDisplay(frame.Bank, frame.Pointer, out var components),
            $"extended sprite {frame.Bank:X2}:{frame.Pointer:X4} must have an installed display binding");
        Require(components.Span.Length == 1 && components.Span[0].Offset.X == (frame.Pointer & 0x7f) &&
            components.Span[0].Offset.Y == -(frame.Pointer & 0x7f) && components.Span[0].Parts.Length == 1,
            "the selected component offsets and OAM pieces must be preserved");
    }

    /// <summary>Reports a failed extended-composition contract through the verifier's exception boundary.</summary>
    internal static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Extended composition contract failed: " + message);
    }
}
