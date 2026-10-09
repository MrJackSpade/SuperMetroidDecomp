using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms identified special-sheet admission gaps with constructed zero artwork, not gameplay.</summary>
internal static class MotherBrainSheetsPresentationContractChecks
{
    /// <summary>Confirms reviewed Mother Brain sheet presentation ownership and selection.</summary>
    internal static void Run()
    {
        RoomCharacterAtlas Sheet(int count)
        {
            int tiles = count / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int rows = (tiles + columns - 1) / columns;
            using var png = new MemoryStream();
            IndexedPng.Write(png, columns * 8, rows * 8, new byte[columns * rows * 64],
                Enumerable.Range(0, 16).Select(_ => new Rgba32(0, 0, 0, 255)).ToArray());
            png.Position = 0;
            return RoomCharacterAtlas.Load(png, count);
        }
        var sheets = MotherBrainSpecialSpriteArtworkDefinitions.All.ToDictionary(sheet => sheet.SourceAddress,
            sheet => Sheet(sheet.ByteCount));
        int first = MotherBrainSpecialSpriteArtworkDefinitions.Legs.SourceAddress;
        var malformed = new Dictionary<int, RoomCharacterAtlas>(sheets);
        malformed[first] = null!;
        bool rejected = false;
        try { _ = new MotherBrainSpecialSpriteArtworkCatalog(malformed); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a required null sheet must fail at construction");
        malformed[first] = Sheet(RoomCharacterAtlasFormat.BytesPerTile);
        rejected = false;
        try { _ = new MotherBrainSpecialSpriteArtworkCatalog(malformed); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a required sheet with incomplete transfer pages must fail at construction");
        var catalog = new MotherBrainSpecialSpriteArtworkCatalog(sheets);
        sheets.Remove(first);
        Require(catalog.Get(first).Transfer.Length ==
            MotherBrainSpecialSpriteArtworkDefinitions.Legs.ByteCount, "catalog keys must be independent of the input dictionary");

        var trees = MotherBrainSheetsClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<mb-sheets-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Assets;
            class SheetConsumer {
                void Inspect(MotherBrainSpecialSpriteArtworkCatalog sheets, int selected) {
                    sheets.Get({{first}});
                    sheets.Get(selected);
                    sheets.Get(0);
                }
            }
            """, path: "fixture-mb-sheets-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("MotherBrainSheetsFixture", sources,
            platforms.Append(typeof(MotherBrainSpecialSpriteArtworkCatalog).Assembly.Location).Distinct()
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
            after.MissingCount == 0 && after.Consumers.Count == 3, "complete sheet admission must reject known unowned source keys");
        const string provider = "csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkCatalog.cs";
        var changed = trees.Select(tree => tree.FilePath == provider ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("artwork.Transfer.Length != definition.ByteCount", "false", StringComparison.Ordinal),
                path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(changed), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 3,
            "removing page-size admission must revoke the provider proof");
    }

    /// <summary>Throws when a Mother Brain sheet contract expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Mother Brain sheet confirmation failed: " + reason);
    }
}
