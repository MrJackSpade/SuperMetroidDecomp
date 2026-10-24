using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Confirms identified special-sheet admission gaps with constructed zero artwork, not gameplay.</summary>
internal static class MotherBrainSheetsPresentationContractChecks
{
    /// <summary>Checks complete Mother Brain sheet admission and revokes audit closure when size validation disappears.</summary>
    internal static void Run()
    {
        RoomCharacterAtlas Sheet(int count)
        {
            int tiles = count / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int rows = (tiles + columns - 1) / columns;
            using var png = new MemoryStream();
            IndexedPng.Write(png, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))), new byte[columns * rows * 64],
                Enumerable.Range(0, 16).Select(_ => new Rgba32(0, 0, 0, 255)).ToArray());
            png.Position = 0;
            return RoomCharacterAtlas.Load(png, count);
        }
        var sheets = MotherBrainSpecialSpriteArtworkDefinitions.All.ToDictionary(sheet => sheet.SourceAddress,
            sheet => Sheet(sheet.ByteCount));
        int first = MotherBrainSpecialSpriteArtworkDefinitions.Legs.SourceAddress;
        var malformed = new Dictionary<int, RoomCharacterAtlas>(sheets);
        malformed[first] = Sheet(RoomCharacterAtlasFormat.BytesPerTile);
        bool rejected = false;
        try { _ = new MotherBrainSpecialSpriteArtworkCatalog(malformed); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a required sheet with incomplete transfer pages must fail at construction");
        var catalog = new MotherBrainSpecialSpriteArtworkCatalog(sheets);
        sheets.Remove(first);
        Require(catalog.Get(first).Characters.Length ==
            MotherBrainSpecialSpriteArtworkDefinitions.Legs.ByteCount, "catalog keys must be independent of the input dictionary");

        var trees = MotherBrainSheetsClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            class SheetConsumer {
                void Inspect(MotherBrainSpecialSpriteArtworkCatalog sheets, int selected) {
                    sheets.Get({{first}});
                    sheets.Get(selected);
                    sheets.Get(0);
                }
            }
            """, path: "fixture-mb-sheets-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
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
            after.MissingCount == 0 && after.Consumers.Count == 3, "complete sheet admission must reject known unowned source keys" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports a Mother Brain sheet contract failure through the resource audit boundary.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Mother Brain sheet confirmation failed: " + reason);
    }
}
