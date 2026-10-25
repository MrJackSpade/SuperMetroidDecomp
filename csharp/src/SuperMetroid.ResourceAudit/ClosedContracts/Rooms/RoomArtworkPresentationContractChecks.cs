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
                    sky.Page(0, RoomSkyTilemapFormat.PageByteCount);
                    sky.Page(RoomSkyTilemapFormat.PageCount - 1, RoomSkyTilemapFormat.PageByteCount);
                    sky.Page(0, 64);
                    sky.Page(RoomSkyTilemapFormat.PageCount, RoomSkyTilemapFormat.PageByteCount);
                    sky.Page(-1, RoomSkyTilemapFormat.PageByteCount);
                    sky.Page(0, 0);
                    library.Get(0);
                }
            }
            """;
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
        Require(before.UnresolvedCount == 7, "constructed calls must first expose absent adapters" + " (" + before.Describe() + ")");
        // The sky copies whole pages only; its rows are read from the pages, not transferred (#1347). So the first and
        // last whole pages qualify, and a partial page or a page outside the seven stays a finding (#1414).
        Require(after.Classifications.Count == 2 && after.UnresolvedCount == 5 && after.MissingCount == 0 &&
            after.Consumers.Count == 7, "whole sky pages qualify, not partial pages or pages outside the sky" + " (" + after.Describe() + ")");
        Require(after.Findings.Any(item => item.Owner == "RoomBackgroundTilemapCatalog.Get"),
            "an unowned background source must not borrow required graphics-set source closure");
    }

    /// <summary>Reports the failed room-artwork ownership property.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room artwork provider confirmation failed: " + reason);
    }
}
