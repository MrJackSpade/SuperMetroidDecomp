using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// <summary>Checks physical-definition admission, the import's cross-group picture resolution, and static consumer classification.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = Body(shortFirstGroup: true); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "construction must reject a nonempty group missing one required physical definition");
        // Pose zero's frames select top group 0 position 0, top group 0 position 2, top group 1 position 0, and the
        // last top group's position 1 with bottom group 0 position 0: the import resolves each to the picture of the
        // physical definition record it lands on, across group and half boundaries.
        SamusBodyFrameSelection[] crossing = new SamusBodyFrameSelection[SamusBodySources.FrameCount];
        crossing[1] = new(0, 2, SamusBodyFrameSelection.NoBottomSet, 0);
        crossing[2] = new(1, 0, SamusBodyFrameSelection.NoBottomSet, 0);
        crossing[3] = new(SamusBodySources.TopSetCount - 1, 1, 0, 0);
        SamusBodyCharacterArtwork characters = Body(shortFirstGroup: false, crossing);
        InstalledSamusBodyArtwork body = characters.Body;
        Require(ReferenceEquals(characters.DefinitionAt(SamusBodySources.Record(body.Frame(0, 0).Top)), characters.TopSet(0)[0]),
            "a published frame resolves an admitted physical definition");
        Require(body.Frame(0, 1).Top == body.Frame(0, 2).Top && body.Frame(0, 3).Top == body.Frame(0, 3).Bottom,
            "native position arithmetic can cross set and half boundaries");
        SamusBodyFrameSelection[] invalidAdjacent = new SamusBodyFrameSelection[SamusBodySources.FrameCount];
        invalidAdjacent[1] = new(byte.MaxValue, 0, byte.MaxValue, 0);
        body = Body(shortFirstGroup: false, invalidAdjacent).Body;
        _ = body.Frame(0, 0);
        rejected = false;
        try { _ = body.Frame(0, 1); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "unselected backing bytes are accepted but invalid frame selection still fails loudly");
        ConfirmStaticCalls();
    }

    /// <summary>Builds a minimal body catalog and its characters, with a missing physical record when requested and the supplied frame selectors.</summary>
    private static SamusBodyCharacterArtwork Body(bool shortFirstGroup, SamusBodyFrameSelection[]? selectors = null)
    {
        // Twenty-four groups, with two records in the first and one in each other
        // group. The final group ends at the last record, just like import.
        int count = SamusBodySources.TopSetCount + SamusBodySources.BottomSetCount;
        int start = SamusBodySources.RecordCount - (count + 1);
        int[] starts = [.. Enumerable.Range(0, count).Select(index => start + (index == 0 ? 0 : index + 1))];
        SamusBodyTileTransfer[][] groups = Enumerable.Range(0, count).Select(index =>
            Enumerable.Range(0, index == 0 && !shortFirstGroup ? 2 : 1).Select(_ =>
                new SamusBodyTileTransfer(32, 0, new byte[32])).ToArray()).ToArray();
        var sprites = new SamusSpritemapSources(new ushort[InstalledSamusBodyArtwork.PoseCount],
            new ushort[InstalledSamusBodyArtwork.PoseCount], new SamusSpritemapId?[SamusSpritemapSources.EntryCount], []);
        var atmosphere = new SamusAtmosphericSourceArtwork(
            new SpritePart[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount],
            new SpritePart[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount]);
        RgbColor[][] Rows() => Enumerable.Range(0, SamusPaletteDefinitions.Death.PaletteCount)
            .Select(_ => new RgbColor[InstalledSamusDeathPalettes.ColorCount]).ToArray();
        var deathColors = new InstalledSamusDeathPalettes(
            Enumerable.Range(0, InstalledSamusDeathPalettes.SuitCount).Select(_ => Rows()).ToArray(), Rows(),
            new RgbColor[SamusPaletteDefinitions.Death.WhiteoutShadeCount], new ushort[SamusDeathExplosionTimingDefinitions.RecordCount]);
        using var deathPng = Png(new(SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height));
        SamusDeathTileAtlas deathTiles = SamusDeathTileAtlas.Load(deathPng);
        var cannonDocument = new SamusArmCannonArtworkDocument {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = new int[InstalledSamusBodyArtwork.PoseCount],
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            CoverAppearances = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
                .Select(_ => SpriteVisualPart.FromSpritePart(SpritePart.Single(0, 0, 0), SpriteArtwork.CharactersPerRow)).ToArray(),
            CoverTiles = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount).Select(_ =>
                new int[SamusArmCannonArtworkFormat.FramesPerDirection - 1]).ToArray() };
        using var cannonJson = new MemoryStream(SamusArmCannonSourceArtwork.Write(cannonDocument));
        using var cannonPng = Png(new(SamusArmCannonArtworkFormat.CoverTileCount * 8, 8));
        InstalledSamusArmCannonArtwork cannon = InstalledSamusArmCannonArtworkTooling.Load(cannonJson, cannonPng);
        SamusBodyFrameSelection[] frames = selectors ?? new SamusBodyFrameSelection[SamusBodySources.FrameCount];
        return SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = starts[..SamusBodySources.TopSetCount], BottomSetStarts = starts[SamusBodySources.TopSetCount..], PoseFirstFrames = new int[InstalledSamusBodyArtwork.PoseCount], GraphicsYOffsets = new int[InstalledSamusBodyArtwork.PoseCount], Frames = frames, Spritemaps = sprites, Atmosphere = atmosphere, DeathPalettes = deathColors, DeathTilesContentIdentity = deathTiles.ContentIdentity, ArmCannon = cannon, LandingYOffsets = new int[SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount], PostureYOffsets = new sbyte[SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount], DrainedYOffsets = new sbyte[SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount] },
            groups[..SamusBodySources.TopSetCount], groups[SamusBodySources.TopSetCount..]);
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
                void Inspect(InstalledSamusBodyArtwork body, SuperMetroid.Core.Samus.Poses.SamusPoseId pose, ushort frame) {
                    body.Frame(pose, frame);
                    body.Frame((SuperMetroid.Core.Samus.Poses.SamusPoseId)253, 0);
                }
            }
            """, path: "fixture-samus-transfer-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("SamusTransferFixture", sources,
            platforms.Append(typeof(InstalledSamusBodyArtwork).Assembly.Location).Distinct().Select(path => MetadataReference.CreateFromFile(path)),
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
        Require(before.UnresolvedCount == 2 && before.Consumers.Count == 2, "original unaccounted frame-picture boundaries" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 1 && after.UnresolvedCount == 1 && after.Consumers.Count == 2,
            "valid-domain frame-picture availability does not suppress a known invalid pose constant" + " (" + after.Describe() + ")");
        var extra = CSharpSyntaxTree.ParseText("namespace SuperMetroid.Core.Samus.Artwork; public abstract partial class InstalledSamusBodyArtwork { }",
            path: "fixture-unreviewed-body-partial.cs");
        Require(Inspect(Compile(trees.Append(extra)), true).UnresolvedCount == 2, "unreviewed private-state access revokes frame-picture closure");
    }

    /// <summary>Creates a transparent indexed PNG stream for synthetic body artwork dependencies.</summary>
    private static MemoryStream Png(Extent16<ImagePixels> size)
    {
        var stream = new MemoryStream();
        IndexedPng.Write(stream, size, new byte[size.Area], [new RgbaPixel(0, 0, 0)]);
        stream.Position = 0;
        return stream;
    }

    /// <summary>Reports a failed transfer-contract assertion with its specific reason.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus transfer confirmation failed: " + reason);
    }
}
