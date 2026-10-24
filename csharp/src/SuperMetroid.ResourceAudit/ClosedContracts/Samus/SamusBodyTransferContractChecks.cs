using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Hardware;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Confirms the identified missing-record admission gap using authored data, not poses in gameplay.</summary>
internal static class SamusBodyTransferContractChecks
{
    /// <summary>Checks physical-definition admission, native cross-group addressing, and static consumer classification.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = Body(shortFirstGroup: true); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "construction must reject a nonempty group missing one required physical definition");
        SamusBodyArtworkCatalog body = Body(shortFirstGroup: false);
        SamusBodyFrameSelection frame = body.Frame(0, 0);
        int record = body.DefinitionRecord(SamusBodyHalf.Top, frame.TopSet, frame.TopPosition);
        Require(ReferenceEquals(body.DefinitionAt(record), body.GetDefinition(true, 0, 0)),
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

    /// <summary>Builds a minimal body catalog with a missing physical record or invalid adjacent frame when requested.</summary>
    private static SamusBodyArtworkCatalog Body(bool shortFirstGroup, bool invalidAdjacentFrame = false)
    {
        // Twenty-four groups, with two records in the first and one in each other
        // group. The final group ends at the last record, just like import.
        int count = SamusBodyArtworkCatalog.TopSetCount + SamusBodyArtworkCatalog.BottomSetCount;
        int start = SamusBodyDefinitionLayout.RecordCount - (count + 1);
        int[] starts = [.. Enumerable.Range(0, count).Select(index => start + (index == 0 ? 0 : index + 1))];
        SamusBodyTileDefinition[][] groups = Enumerable.Range(0, count).Select(index =>
            Enumerable.Range(0, index == 0 && !shortFirstGroup ? 2 : 1).Select(_ =>
                new SamusBodyTileDefinition(0x9a8000, 32, 0, new byte[32])).ToArray()).ToArray();
        var sprites = new SamusSpritemapArtworkCatalog(new ushort[SamusBodyArtworkCatalog.PoseCount],
            new ushort[SamusBodyArtworkCatalog.PoseCount], new SamusSpritemapId?[SamusSpritemapArtworkCatalog.PointerCount], []);
        var atmosphere = new SamusAtmosphericArtworkCatalog(
            new ushort[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount],
            new ushort[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount]);
        Bgr555[][] Rows() => Enumerable.Range(0, SamusPaletteDefinitions.Death.PaletteCount)
            .Select(_ => new Bgr555[SamusDeathPaletteArtworkCatalog.ColorCount]).ToArray();
        var deathColors = new SamusDeathPaletteArtworkCatalog(
            Enumerable.Range(0, SamusDeathPaletteArtworkCatalog.SuitCount).Select(_ => Rows()).ToArray(), Rows(),
            new Bgr555[SamusPaletteDefinitions.Death.WhiteoutShadeCount], new ushort[SamusDeathExplosionTimingDefinitions.RecordCount]);
        using var deathPng = Png(new(SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height));
        SamusDeathTileAtlas deathTiles = SamusDeathTileAtlas.Load(deathPng);
        var cannonDocument = new SamusArmCannonArtworkDocument {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = new int[SamusBodyArtworkCatalog.PoseCount],
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            SpriteAttributes = new int[SamusDrawingDefinitions.ArmCannon.DirectionCount],
            CoverTiles = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount).Select(_ =>
                new int[SamusArmCannonArtworkFormat.FramesPerDirection - 1]).ToArray() };
        using var cannonJson = new MemoryStream(SamusArmCannonArtworkCatalog.Write(cannonDocument));
        using var cannonPng = Png(new(SamusArmCannonArtworkFormat.CoverTileCount * 8, 8));
        SamusArmCannonArtworkCatalog cannon = SamusArmCannonArtworkCatalogTooling.Load(cannonJson, cannonPng);
        var frames = new SamusBodyFrameSelection[SamusBodyArtworkCatalog.FrameCount];
        if (invalidAdjacentFrame) frames[1] = new(byte.MaxValue, 0, byte.MaxValue, 0);
        return new SamusBodyArtworkCatalog(new SamusBodyArtworkParts { TopSetStarts = starts[..SamusBodyArtworkCatalog.TopSetCount], BottomSetStarts = starts[SamusBodyArtworkCatalog.TopSetCount..], PoseFirstFrames = new int[SamusBodyArtworkCatalog.PoseCount], GraphicsYOffsets = new sbyte[SamusBodyArtworkCatalog.PoseCount], Frames = frames, Top = groups[..SamusBodyArtworkCatalog.TopSetCount], Bottom = groups[SamusBodyArtworkCatalog.TopSetCount..], Spritemaps = sprites, Atmosphere = atmosphere, DeathPalettes = deathColors, DeathTiles = deathTiles, ArmCannon = cannon, LandingYOffsets = new ushort[SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount], PostureYOffsets = new sbyte[SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount], DrainedYOffsets = new sbyte[SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount] });
    }

    /// <summary>Compiles representative transfer calls and checks audit behavior under valid and stale source contracts.</summary>
    private static void ConfirmStaticCalls()
    {
        var trees = SamusBodyTransferClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path),
                path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Samus.Artwork;
            class SamusTransferConsumer {
                void Inspect(SamusBodyArtworkCatalog body, SuperMetroid.Core.Samus.Poses.SamusPoseId pose, ushort frame, SamusBodyHalf half, byte set,
                    byte position, int record) {
                    body.Frame(pose, frame);
                    body.Frame((SuperMetroid.Core.Samus.Poses.SamusPoseId)253, 0);
                    body.DefinitionRecord(SamusBodyHalf.Top, 0, 2);
                    body.DefinitionRecord(SamusBodyHalf.Top, 13, 0);
                    body.DefinitionRecord(SamusBodyHalf.Bottom, 11, 0);
                    body.DefinitionRecord(half, set, position);
                    body.DefinitionAt(record);
                    body.DefinitionAt(-1);
                }
            }
            """, path: "fixture-samus-transfer-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
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
        Require(before.UnresolvedCount == 8 && before.Consumers.Count == 8, "original unaccounted transfer boundaries" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 4 && after.UnresolvedCount == 4 && after.Consumers.Count == 8,
            "valid-domain transfer availability does not suppress known invalid pose, set or record constants" + " (" + after.Describe() + ")");
        var extra = CSharpSyntaxTree.ParseText("namespace SuperMetroid.Core.Samus.Artwork; public sealed partial class SamusBodyArtworkCatalog { }",
            path: "fixture-unreviewed-body-partial.cs");
        Require(Inspect(Compile(trees.Append(extra)), true).UnresolvedCount == 8, "unreviewed private-state access revokes transfer closure");
    }

    /// <summary>Creates a transparent indexed PNG stream for synthetic body artwork dependencies.</summary>
    private static MemoryStream Png(Extent16<ImagePixels> size)
    {
        var stream = new MemoryStream();
        IndexedPng.Write(stream, size, new byte[size.Area], [new Rgba32(0, 0, 0)]);
        stream.Position = 0;
        return stream;
    }

    /// <summary>Reports a failed transfer-contract assertion with its specific reason.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus transfer confirmation failed: " + reason);
    }
}
