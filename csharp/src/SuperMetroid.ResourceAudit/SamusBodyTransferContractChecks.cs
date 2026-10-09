using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified missing-record admission gap using authored data, not poses in gameplay.</summary>
internal static class SamusBodyTransferContractChecks
{
    internal static void Run()
    {
        bool rejected = false;
        try { _ = Body(shortFirstGroup: true); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "construction must reject a nonempty group missing one required physical definition");
        SamusBodyArtworkCatalog body = Body(shortFirstGroup: false);
        SamusBodyFrameSelection frame = body.Frame(0, 0);
        int address = body.DefinitionAddress(true, frame.TopSet, frame.TopPosition);
        Require(ReferenceEquals(body.DefinitionAt(address), body.GetDefinition(true, 0, 0)),
            "a published frame resolves an admitted physical definition");
        Require(ReferenceEquals(body.GetDefinition(true, 0, 2), body.GetDefinition(true, 1, 0)) &&
            ReferenceEquals(body.GetDefinition(true, SamusBodyArtworkCatalog.TopSetCount - 1, 1), body.GetDefinition(false, 0, 0)),
            "native position arithmetic can cross set and half boundaries");
        body = Body(shortFirstGroup: false, invalidAdjacentFrame: true);
        _ = body.Frame(0, 0);
        rejected = false;
        try { _ = body.Frame(0, 1); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "unselected backing bytes are accepted but invalid frame selection still fails loudly");
        ConfirmStaticCalls();
    }

    private static SamusBodyArtworkCatalog Body(bool shortFirstGroup, bool invalidAdjacentFrame = false)
    {
        // Twenty-four groups, with two records in the first and one in each other
        // group. The final native definition ends at $92:D7D3, just like import.
        const int end = SamusBodyDefinitionLayout.EndOffset;
        int count = SamusBodyArtworkCatalog.TopSetCount + SamusBodyArtworkCatalog.BottomSetCount;
        int start = end - (count + 1) * SamusRenderingRomData.TileTransfers.DefinitionByteCount;
        ushort[] pointers = Enumerable.Range(0, count).Select(index =>
            (ushort)(start + (index == 0 ? 0 : index + 1) * SamusRenderingRomData.TileTransfers.DefinitionByteCount)).ToArray();
        SamusBodyTileDefinition[][] groups = Enumerable.Range(0, count).Select(index =>
            Enumerable.Range(0, index == 0 && !shortFirstGroup ? 2 : 1).Select(_ =>
                new SamusBodyTileDefinition(0x9a8000, 32, 0, new byte[32])).ToArray()).ToArray();
        var sprites = new SamusSpritemapArtworkCatalog(new ushort[SamusBodyArtworkCatalog.PoseCount],
            new ushort[SamusBodyArtworkCatalog.PoseCount], new ushort[SamusSpritemapArtworkCatalog.PointerCount], []);
        var atmosphere = new SamusAtmosphericArtworkCatalog(
            new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount],
            new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount]);
        ushort[][] Rows() => Enumerable.Range(0, SamusPaletteRomData.Death.PaletteCount)
            .Select(_ => new ushort[SamusDeathPaletteArtworkCatalog.ColorCount]).ToArray();
        var deathColors = new SamusDeathPaletteArtworkCatalog(
            Enumerable.Range(0, SamusDeathPaletteArtworkCatalog.SuitCount).Select(_ => Rows()).ToArray(), Rows(),
            new ushort[SamusPaletteRomData.Death.WhiteoutShadeCount], new ushort[SamusDeathExplosionTimingDefinitions.RecordCount]);
        using var deathPng = Png(SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height);
        SamusDeathTileAtlas deathTiles = SamusDeathTileAtlas.Load(deathPng);
        var cannonDocument = new SamusArmCannonArtworkDocument {
            Version = SamusArmCannonArtworkFormat.Version,
            PosePointers = Enumerable.Repeat((int)SamusArmCannonArtworkFormat.DrawingDataStart, SamusBodyArtworkCatalog.PoseCount).ToArray(),
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            SpriteAttributes = new int[SamusRenderingRomData.ArmCannon.DirectionCount],
            TileSources = Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(_ =>
                new[] { 0, (int)SamusArmCannonArtworkFormat.TileSourcePointers[0],
                    (int)SamusArmCannonArtworkFormat.TileSourcePointers[0], (int)SamusArmCannonArtworkFormat.TileSourcePointers[0] }).ToArray() };
        using var cannonJson = new MemoryStream(SamusArmCannonArtworkCatalog.Write(cannonDocument));
        using var cannonPng = Png(SamusArmCannonArtworkFormat.TileSourcePointers.Length * 8, 8);
        SamusArmCannonArtworkCatalog cannon = SamusArmCannonArtworkCatalogTooling.Load(cannonJson, cannonPng);
        var frames = new SamusBodyFrameSelection[SamusBodyArtworkCatalog.FrameCount];
        if (invalidAdjacentFrame) frames[1] = new(byte.MaxValue, 0, byte.MaxValue, 0);
        return new SamusBodyArtworkCatalog(pointers[..SamusBodyArtworkCatalog.TopSetCount],
            pointers[SamusBodyArtworkCatalog.TopSetCount..],
            Enumerable.Repeat((ushort)SamusBodyArtworkCatalog.FirstFrameOffset, SamusBodyArtworkCatalog.PoseCount).ToArray(),
            new sbyte[SamusBodyArtworkCatalog.PoseCount], frames,
            groups[..SamusBodyArtworkCatalog.TopSetCount], groups[SamusBodyArtworkCatalog.TopSetCount..],
            sprites, atmosphere, deathColors, deathTiles, cannon,
            new ushort[SamusRenderingRomData.Body.LandingVerticalOffsetByteCount],
            new sbyte[SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount],
            new sbyte[SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount]);
    }

    private static void ConfirmStaticCalls()
    {
        var trees = SamusBodyTransferClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path),
                path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<samus-transfer-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class SamusTransferConsumer {
                void Inspect(SamusBodyArtworkCatalog body, byte pose, ushort frame, bool upper, byte set, byte position, int address) {
                    body.Frame(pose, frame);
                    body.Frame(253, 0);
                    body.DefinitionAddress(true, 0, 2);
                    body.DefinitionAddress(true, 13, 0);
                    body.DefinitionAddress(false, 11, 0);
                    body.DefinitionAddress(upper, set, position);
                    body.DefinitionAt(true, address);
                    body.DefinitionAt(false, 0);
                }
            }
            """, path: "fixture-samus-transfer-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("SamusTransferFixture", sources,
            platforms.Append(typeof(SamusBodyArtworkCatalog).Assembly.Location).Distinct().Select(path => MetadataReference.CreateFromFile(path)),
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
        Require(before.UnresolvedCount == 8 && before.Consumers.Count == 8, "original unaccounted transfer boundaries");
        Require(after.Classifications.Count == 4 && after.UnresolvedCount == 4 && after.Consumers.Count == 8,
            "valid-domain transfer availability does not suppress known invalid pose, set or address constants");
        const string layout = "csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs";
        var stale = trees.Select(tree => tree.FilePath == layout ? CSharpSyntaxTree.ParseText(tree.GetText().ToString()
            .Replace("groups[set].Length != counts[pointers[set]]", "false", StringComparison.Ordinal), path: tree.FilePath) : tree);
        Require(Inspect(Compile(stale), true).UnresolvedCount == 8, "changed admission revokes physical record closure");
        var extra = CSharpSyntaxTree.ParseText("namespace SuperMetroid.Core.Assets; public sealed partial class SamusBodyArtworkCatalog { }",
            path: "fixture-unreviewed-body-partial.cs");
        Require(Inspect(Compile(trees.Append(extra)), true).UnresolvedCount == 8, "unreviewed private-state access revokes transfer closure");
    }

    private static MemoryStream Png(int width, int height)
    {
        var stream = new MemoryStream();
        IndexedPng.Write(stream, width, height, new byte[width * height], [new Rgba32(0, 0, 0)]);
        stream.Position = 0;
        return stream;
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus transfer confirmation failed: " + reason);
    }
}
