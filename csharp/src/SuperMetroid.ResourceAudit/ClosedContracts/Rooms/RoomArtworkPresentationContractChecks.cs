using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified required graphics-set sources and contiguous sky upload rules.</summary>
internal static class RoomArtworkPresentationContractChecks
{
    /// <summary>Checks required tileset sources and bounded sky transfers
    /// qualify for closed artwork ownership, then revokes a stale provider proof.</summary>
    internal static void Run()
    {
        var trees = RoomArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        string consumer = """
            using SuperMetroid.Core.Rooms.Artwork;
            class RoomArtConsumer {
                void Inspect(RoomCharacterAtlasCatalog characters, RoomMetatileCatalog metatiles,
                    RoomStaticPaletteCatalog colors, RoomSkyTilemapCatalog sky, RoomBackgroundTilemapCatalog library) {
                    characters.Get(CHARACTER_SOURCE);
                    characters.Get(0);
                    metatiles.Get(METATILE_SOURCE);
                    metatiles.Get(0);
                    colors.Get(PALETTE_SOURCE);
                    colors.Get(0);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress, RoomSkyTilemapFormat.PageByteCount, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 64, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 2, RoomSkyTilemapFormat.PageByteCount, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 2, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 1, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + RoomSkyTilemapFormat.TotalByteCount - 64, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + RoomSkyTilemapFormat.TotalByteCount - 2, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress, 0, out _);
                    sky.TryResolve(0, 0, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress - 1, int.MaxValue, out _);
                    library.Get(0);
                }
            }
            """;
        TilesetDefinition first = RoomTilesetDefinitions.Get(0);
        consumer = consumer.Replace("CHARACTER_SOURCE", first.CharacterAddress.ToString(), StringComparison.Ordinal)
            .Replace("METATILE_SOURCE", first.BlockDefinitionsAddress.ToString(), StringComparison.Ordinal)
            .Replace("PALETTE_SOURCE", first.PaletteAddress.ToString(), StringComparison.Ordinal);
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-room-art-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("RoomArtworkProviderFixture", sources,
            platforms.Append(typeof(RoomCharacterAtlasCatalog).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AuditReport Inspect(CSharpCompilation compilation, bool adapted)
        {
            var exports = new ResourceIndex();
            var adapter = adapted ? new ClosedPresentationAudit(compilation, exports) : null;
            var report = new AuditReport();
            foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
            return report;
        }
        CSharpCompilation compilation = Compile(trees);
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 17, "constructed calls must first expose absent adapters" + " (" + before.Describe() + ")");
        // The sky uploads whole aligned pages only; its rows are read from the pages, not transferred (#1347). So the
        // first page and the two queries outside the pages qualify, and every partial-page transfer stays a finding.
        Require(after.Classifications.Count == 6 && after.UnresolvedCount == 11 && after.MissingCount == 0 &&
            after.Consumers.Count == 17, "required sources, whole sky pages and false queries qualify, not optional keys, partial pages or bad transfers" + " (" + after.Describe() + ")");
        Require(after.Findings.Any(item => item.Owner == "RoomBackgroundTilemapCatalog.Get"),
            "an unowned background source must not borrow required graphics-set source closure");
    }

    /// <summary>Reports the failed room-artwork ownership property.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room artwork provider confirmation failed: " + reason);
    }
}
