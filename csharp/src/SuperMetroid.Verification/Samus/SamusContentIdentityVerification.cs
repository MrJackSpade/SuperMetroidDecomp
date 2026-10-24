using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that each selected Samus artwork family contributes to the installed content identity.</summary>
internal static class SamusContentIdentityVerification
{
    /// <summary>Direct, authored fixtures cover every selected Samus visual-data family.</summary>
    internal static void VerifySamusContentIdentity()
    {
        SamusBodyArtworkCatalog baseline = CreateSamusIdentityFixture();
        string[] edits =
        [
            "top-characters", "bottom-characters", "pose-pointer", "graphics-offset", "landing-offset",
            "posture-offset", "drained-offset", "frame-selector", "transfer-split", "body-source",
            "spritemap-top", "spritemap-bottom", "spritemap-entry", "spritemap-part",
            "atmosphere-one", "atmosphere-shared", "death-suit", "death-suitless", "death-whiteout",
            "death-selector", "death-tiles", "cannon-pose", "cannon-drawing", "cannon-attributes",
            "cannon-sources", "cannon-tiles",
        ];
        foreach (string edit in edits)
        {
            SamusBodyArtworkCatalog changed = CreateSamusIdentityFixture(edit);
            AssertTrue(changed.ContentIdentity != baseline.ContentIdentity, $"{edit} invalidates Samus identity");
        }
        Guid build = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        GameContentIdentity Identity(SamusBodyArtworkCatalog samus) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), build,
            new Dictionary<string, string> { [GameInstallationLayout.SamusBodyDirectoryName] = samus.ContentIdentity });
        GameContentIdentity first = Identity(baseline);
        GameContentIdentity second = Identity(CreateSamusIdentityFixture("cannon-tiles"));
        AssertTrue(first.CompositeSha256 != second.CompositeSha256, "Samus edit changes host aggregate");
        AssertTrue(first.GetCompatibilityWarnings(second.ToSnapshot(), "test").Single()
            .Contains(GameInstallationLayout.SamusBodyDirectoryName, StringComparison.Ordinal),
            "Samus drift receives a component-specific warning");
        Console.WriteLine($"  Samus content identity: {edits.Length} independent visual edits, " +
            "canonical spritemap order and host aggregate/warnings pass without a ROM.");
    }

    /// <summary>Builds a minimal Samus art catalog, optionally changing one visual family or spritemap order.</summary>
    internal static SamusBodyArtworkCatalog CreateSamusIdentityFixture(string? edit = null)
    {
        int firstRecord = SamusBodyDefinitionLayout.RecordCount -
            (SamusBodyArtworkCatalog.TopSetCount + SamusBodyArtworkCatalog.BottomSetCount);
        int[] topStarts = [.. Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => firstRecord + index)];
        int[] bottomStarts = [.. Enumerable.Range(SamusBodyArtworkCatalog.TopSetCount, SamusBodyArtworkCatalog.BottomSetCount)
            .Select(index => firstRecord + index)];
        var poses = new int[SamusBodyArtworkCatalog.PoseCount];
        if (edit == "pose-pointer") poses[0] += 1;
        var graphics = new sbyte[SamusBodyArtworkCatalog.PoseCount];
        if (edit == "graphics-offset") graphics[0] = -1;
        var landing = new ushort[SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount];
        if (edit == "landing-offset") landing[0] = 1;
        var posture = new sbyte[SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount];
        if (edit == "posture-offset") posture[0] = -1;
        var drained = new sbyte[SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount];
        if (edit == "drained-offset") drained[0] = 1;
        var frames = new SamusBodyFrameSelection[SamusBodyArtworkCatalog.FrameCount];
        if (edit == "frame-selector") frames[0] = new SamusBodyFrameSelection(1, 0, 0, 0);
        SamusBodyTileDefinition[][] Half(int count, string family) => Enumerable.Range(0, count)
            .Select(index =>
            {
                var bytes = new byte[RoomCharacterAtlasFormat.BytesPerTile];
                if (edit == family && index == 0) bytes[0] = 1;
                return new[] { new SamusBodyTileDefinition(edit == "body-source" ? 0x9a9000 : 0x9a8000,
                    edit == "transfer-split" ? (ushort)16 : (ushort)32,
                    edit == "transfer-split" ? (ushort)16 : (ushort)0, bytes) };
            }).ToArray();

        var topBases = new ushort[SamusBodyArtworkCatalog.PoseCount];
        var bottomBases = new ushort[SamusBodyArtworkCatalog.PoseCount];
        if (edit == "spritemap-top") topBases[0] = 1;
        if (edit == "spritemap-bottom") bottomBases[0] = 1;
        SamusSpritemapId?[] entries = [.. Enumerable.Repeat<SamusSpritemapId?>(new SamusSpritemapId(0), SamusSpritemapArtworkCatalog.PointerCount)];
        if (edit == "spritemap-entry") entries[0] = new SamusSpritemapId(1);
        SamusSpritemapDefinition[] definitions =
        [
            new([new SamusSpritePart(new SnesSpritemapXWord(0), new SnesSpritemapYByte(edit == "spritemap-part" ? (byte)1 : (byte)0), new SnesObjAttributeWord(0))]),
            new([new SamusSpritePart(new SnesSpritemapXWord(1), new SnesSpritemapYByte(0), new SnesObjAttributeWord(1))]),
        ];
        var spritemaps = new SamusSpritemapArtworkCatalog(topBases, bottomBases, entries, definitions);
        var one = new ushort[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount];
        var shared = new ushort[one.Length];
        if (edit == "atmosphere-one") one[0] = 1;
        if (edit == "atmosphere-shared") shared[0] = 1;
        var atmosphere = new SamusAtmosphericArtworkCatalog(one, shared);
        ushort[][] Rows() => Enumerable.Range(0, SamusPaletteDefinitions.Death.PaletteCount)
            .Select(_ => new ushort[SamusDeathPaletteArtworkCatalog.ColorCount]).ToArray();
        ushort[][][] suited = Enumerable.Range(0, SamusDeathPaletteArtworkCatalog.SuitCount).Select(_ => Rows()).ToArray();
        ushort[][] suitless = Rows();
        var whiteout = new ushort[SamusPaletteDefinitions.Death.WhiteoutShadeCount];
        var explosion = new ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        if (edit == "death-suit") suited[0][0][0] = 1;
        if (edit == "death-suitless") suitless[0][0] = 1;
        if (edit == "death-whiteout") whiteout[0] = 1;
        if (edit == "death-selector") explosion[0] = 1;
        var deathPalettes = new SamusDeathPaletteArtworkCatalog(ToColors(suited), ToColors(suitless), ToColors(whiteout), explosion);
        SamusDeathTileAtlas deathTiles = SamusDeathTileAtlas.Load(Png(SamusDeathTileAtlasFormat.Width,
            SamusDeathTileAtlasFormat.Height, edit == "death-tiles"));
        var cannonDocument = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = new int[SamusBodyArtworkCatalog.PoseCount],
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            SpriteAttributes = new int[SamusDrawingDefinitions.ArmCannon.DirectionCount],
            CoverTiles = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
                .Select(_ => new int[SamusArmCannonArtworkFormat.FramesPerDirection - 1]).ToArray(),
        };
        if (edit == "cannon-pose") cannonDocument.PoseDescriptors[0]++;
        if (edit == "cannon-drawing") cannonDocument.DrawingData[0] = 1;
        if (edit == "cannon-attributes") cannonDocument.SpriteAttributes[0] = 1;
        if (edit == "cannon-sources") cannonDocument.CoverTiles[0][0] = 1;
        int cannonWidth = SamusArmCannonArtworkFormat.CoverTileCount * 8;
        SamusArmCannonArtworkCatalog cannon = SamusArmCannonArtworkCatalogTooling.Load(
            new MemoryStream(SamusArmCannonArtworkCatalog.Write(cannonDocument)),
            Png(cannonWidth, 8, edit == "cannon-tiles"));
        return new SamusBodyArtworkCatalog(new SamusBodyArtworkParts { TopSetStarts = topStarts, BottomSetStarts = bottomStarts, PoseFirstFrames = poses, GraphicsYOffsets = graphics, Frames = frames, Top = Half(topStarts.Length, "top-characters"), Bottom = Half(bottomStarts.Length, "bottom-characters"), Spritemaps = spritemaps, Atmosphere = atmosphere, DeathPalettes = deathPalettes, DeathTiles = deathTiles, ArmCannon = cannon, LandingYOffsets = landing, PostureYOffsets = posture, DrainedYOffsets = drained });

        static MemoryStream Png(int width, int height, bool changed)
        {
            var stream = new MemoryStream();
            var pixels = new byte[width * height];
            if (changed) pixels[^1] = 1;
            IndexedPng.Write(stream, new(checked((ushort)width), checked((ushort)height)), pixels, [new Rgba32(0, 0, 0), new Rgba32(255, 255, 255)]);
            stream.Position = 0;
            return stream;
        }
    }

    /// <summary>Runs the focused Samus artwork identity check.</summary>
    [VerificationCommand("--samus-content-identity")]
    internal static int SamusContentIdentityCommand()
    {
        Suite(nameof(VerifySamusContentIdentity), () => VerifySamusContentIdentity());
        return 0;
    }
}
