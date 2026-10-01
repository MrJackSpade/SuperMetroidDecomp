using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Direct, authored fixtures cover every selected Samus visual-data family.</summary>
    private static void VerifySamusContentIdentity()
    {
        SamusBodyArtworkCatalog baseline = CreateSamusIdentityFixture();
        AssertEqual(baseline.ContentIdentity, CreateSamusIdentityFixture(reverseMaps: true).ContentIdentity,
            "Samus identity ignores spritemap record insertion order");
        string[] edits =
        [
            "top-characters", "bottom-characters", "pose-pointer", "graphics-offset", "landing-offset",
            "posture-offset", "drained-offset", "frame-selector", "transfer-split", "body-source",
            "spritemap-top", "spritemap-bottom", "spritemap-pointer", "spritemap-part",
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

    private static SamusBodyArtworkCatalog CreateSamusIdentityFixture(string? edit = null, bool reverseMaps = false)
    {
        int firstPointer = SamusBodyDefinitionLayout.EndOffset -
            (SamusBodyArtworkCatalog.TopSetCount + SamusBodyArtworkCatalog.BottomSetCount) *
                SamusRenderingRomData.TileTransfers.DefinitionByteCount;
        ushort[] topPointers = Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount)
            .Select(index => (ushort)(firstPointer + index * SamusRenderingRomData.TileTransfers.DefinitionByteCount)).ToArray();
        ushort[] bottomPointers = Enumerable.Range(SamusBodyArtworkCatalog.TopSetCount, SamusBodyArtworkCatalog.BottomSetCount)
            .Select(index => (ushort)(firstPointer + index * SamusRenderingRomData.TileTransfers.DefinitionByteCount)).ToArray();
        ushort[] poses = Enumerable.Repeat((ushort)SamusBodyArtworkCatalog.FirstFrameOffset,
            SamusBodyArtworkCatalog.PoseCount).ToArray();
        if (edit == "pose-pointer") poses[0] += 4;
        var graphics = new sbyte[SamusBodyArtworkCatalog.PoseCount];
        if (edit == "graphics-offset") graphics[0] = -1;
        var landing = new ushort[SamusRenderingRomData.Body.LandingVerticalOffsetByteCount];
        if (edit == "landing-offset") landing[0] = 1;
        var posture = new sbyte[SamusRenderingRomData.Body.PostureTransitionVerticalOffsetByteCount];
        if (edit == "posture-offset") posture[0] = -1;
        var drained = new sbyte[SamusRenderingRomData.Body.DrainedVerticalOffsetByteCount];
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
        ushort[] pointers = Enumerable.Repeat((ushort)0x90ed, SamusSpritemapArtworkCatalog.PointerCount).ToArray();
        if (edit == "spritemap-pointer") pointers[0] = 0x90f4;
        SamusSpritemapDefinition[] definitions =
        [
            new(0x90ed, [new SamusSpritePart(0, edit == "spritemap-part" ? (byte)1 : (byte)0, 0)]),
            new(0x90f4, [new SamusSpritePart(1, 0, 1)]),
        ];
        if (reverseMaps) Array.Reverse(definitions);
        var spritemaps = new SamusSpritemapArtworkCatalog(topBases, bottomBases, pointers, definitions);
        var one = new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount];
        var shared = new ushort[one.Length];
        if (edit == "atmosphere-one") one[0] = 1;
        if (edit == "atmosphere-shared") shared[0] = 1;
        var atmosphere = new SamusAtmosphericArtworkCatalog(one, shared);
        ushort[][] Rows() => Enumerable.Range(0, SamusPaletteRomData.Death.PaletteCount)
            .Select(_ => new ushort[SamusDeathPaletteArtworkCatalog.ColorCount]).ToArray();
        ushort[][][] suited = Enumerable.Range(0, SamusDeathPaletteArtworkCatalog.SuitCount).Select(_ => Rows()).ToArray();
        ushort[][] suitless = Rows();
        var whiteout = new ushort[SamusPaletteRomData.Death.WhiteoutShadeCount];
        var explosion = new ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        if (edit == "death-suit") suited[0][0][0] = 1;
        if (edit == "death-suitless") suitless[0][0] = 1;
        if (edit == "death-whiteout") whiteout[0] = 1;
        if (edit == "death-selector") explosion[0] = 1;
        var deathPalettes = new SamusDeathPaletteArtworkCatalog(suited, suitless, whiteout, explosion);
        SamusDeathTileAtlas deathTiles = SamusDeathTileAtlas.Load(Png(SamusDeathTileAtlasFormat.Width,
            SamusDeathTileAtlasFormat.Height, edit == "death-tiles"));
        var cannonDocument = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PosePointers = Enumerable.Repeat((int)SamusArmCannonArtworkFormat.DrawingDataStart,
                SamusBodyArtworkCatalog.PoseCount).ToArray(),
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            SpriteAttributes = new int[SamusRenderingRomData.ArmCannon.DirectionCount],
            TileSources = Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount)
                .Select(_ => new[] { 0, (int)SamusArmCannonArtworkFormat.TileSourcePointers[0],
                    (int)SamusArmCannonArtworkFormat.TileSourcePointers[0],
                    (int)SamusArmCannonArtworkFormat.TileSourcePointers[0] }).ToArray(),
        };
        if (edit == "cannon-pose") cannonDocument.PosePointers[0]++;
        if (edit == "cannon-drawing") cannonDocument.DrawingData[0] = 1;
        if (edit == "cannon-attributes") cannonDocument.SpriteAttributes[0] = 1;
        if (edit == "cannon-sources") cannonDocument.TileSources[0][1] = SamusArmCannonArtworkFormat.TileSourcePointers[1];
        int cannonWidth = SamusArmCannonArtworkFormat.TileSourcePointers.Length * 8;
        SamusArmCannonArtworkCatalog cannon = SamusArmCannonArtworkCatalog.Load(
            new MemoryStream(SamusArmCannonArtworkCatalog.Write(cannonDocument)),
            Png(cannonWidth, 8, edit == "cannon-tiles"));
        return new SamusBodyArtworkCatalog(topPointers, bottomPointers, poses, graphics, frames,
            Half(topPointers.Length, "top-characters"), Half(bottomPointers.Length, "bottom-characters"),
            spritemaps, atmosphere, deathPalettes, deathTiles, cannon, landing, posture, drained);

        static MemoryStream Png(int width, int height, bool changed)
        {
            var stream = new MemoryStream();
            var pixels = new byte[width * height];
            if (changed) pixels[^1] = 1;
            IndexedPng.Write(stream, width, height, pixels, [new Rgba32(0, 0, 0), new Rgba32(255, 255, 255)]);
            stream.Position = 0;
            return stream;
        }
    }
}
