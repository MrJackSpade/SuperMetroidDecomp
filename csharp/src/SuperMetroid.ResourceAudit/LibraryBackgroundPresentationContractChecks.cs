using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms exact required source admission, not background pixels or room execution.</summary>
internal static class LibraryBackgroundPresentationContractChecks
{
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
        var owned = RoomBackgroundTilemapSources.All.ToDictionary(source => source, _ => atlas);
        var substituted = new Dictionary<int, RoomBackgroundTilemapAtlas>(owned);
        int first = RoomBackgroundTilemapSources.All[0];
        substituted.Remove(first);
        substituted.Add(0, atlas);
        bool rejected = false;
        try { _ = new RoomBackgroundTilemapCatalog(substituted); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a correct count with one substituted source must fail at construction");
        var catalog = new RoomBackgroundTilemapCatalog(owned);
        owned.Remove(first);
        Require(ReferenceEquals(catalog.Get(first), atlas), "catalog keys must be independent of the caller's dictionary");

        var trees = LibraryBackgroundClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<library-bg-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Assets;
            class BackgroundConsumer {
                void Inspect(RoomBackgroundTilemapCatalog catalog, int selected) {
                    catalog.Get({{first}});
                    catalog.Get(selected);
                    catalog.Get(0);
                }
            }
            """, path: "fixture-library-bg-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
            after.MissingCount == 0 && after.Consumers.Count == 3, "required-source completeness must not accept a known unowned key");
        const string provider = "csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapCatalog.cs";
        var changed = trees.Select(tree => tree.FilePath == provider ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("RoomBackgroundTilemapSources.All.Any", "Array.Empty<int>().Any", StringComparison.Ordinal),
                path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(changed), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 3,
            "removing exact source admission must revoke even valid-looking lookups");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Library background confirmation failed: " + reason);
    }
}
