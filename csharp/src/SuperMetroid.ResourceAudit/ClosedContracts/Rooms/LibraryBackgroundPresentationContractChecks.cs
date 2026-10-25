using SuperMetroid.Tooling;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms exact required tilemap admission, not background pixels or room execution.</summary>
internal static class LibraryBackgroundPresentationContractChecks
{
    /// <summary>Checks exact source-key admission, catalog ownership, and audit revocation when the provider's source test is removed.</summary>
    internal static void Run()
    {
        var cell = new RoomBackgroundTilemapCell { TileColumn = 0, TileRow = 0, Palette = 0,
            Priority = false, FlipX = false, FlipY = false };
        using var json = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(new RoomBackgroundTilemapDocument {
            Version = RoomBackgroundTilemapFormat.Version,
            Pages = [new RoomBackgroundTilemapPage {
                Cells = Enumerable.Repeat(cell, RoomBackgroundTilemapFormat.CellsPerPage).ToArray() }],
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        RoomBackgroundTilemapAtlas atlas = RoomBackgroundTilemapAtlas.Load(json, RoomBackgroundTilemapFormat.BytesPerPage);
        var owned = Enum.GetValues<RoomBackgroundTilemap>().ToDictionary(tilemap => tilemap, _ => atlas);
        var substituted = new Dictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas>(owned);
        RoomBackgroundTilemap first = Enum.GetValues<RoomBackgroundTilemap>()[0];
        substituted.Remove(first);
        substituted.Add(default, atlas);
        bool rejected = false;
        try { _ = new RoomBackgroundTilemapCatalog(substituted); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a correct count with one substituted tilemap must fail at construction");
        var catalog = new RoomBackgroundTilemapCatalog(owned);
        owned.Remove(first);
        Require(ReferenceEquals(catalog.Get(first), atlas), "catalog keys must be independent of the caller's dictionary");

        var trees = LibraryBackgroundClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Rooms.Artwork;
            using SuperMetroid.Core.Rooms.Backgrounds;
            class BackgroundConsumer {
                void Inspect(RoomBackgroundTilemapCatalog catalog, RoomBackgroundTilemap selected) {
                    catalog.Get(RoomBackgroundTilemap.{{first}});
                    catalog.Get(selected);
                    catalog.Get((RoomBackgroundTilemap)0);
                }
            }
            """, path: "fixture-library-bg-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("LibraryBackgroundProviderFixture", sources,
            platforms.Append(typeof(RoomBackgroundTilemapCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 3 && after.Classifications.Count == 2 && after.UnresolvedCount == 1 &&
            after.MissingCount == 0 && after.Consumers.Count == 3, "required-source completeness must not accept a known unowned key" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports the particular background source-admission contract that failed.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Library background confirmation failed: " + reason);
    }
}
