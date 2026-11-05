using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>A minimal, ROM-free Samus body catalog that checks bind Samus to when they need no retail artwork.</summary>
internal static class SamusContentIdentityVerification
{
    /// <summary>
    /// Builds a minimal Samus art catalog; its characters are registered, so bodies built from its tables compose their
    /// sheets from them.
    /// </summary>
    internal static InstalledSamusBodyArtwork CreateSamusIdentityFixture() =>
        SamusBodySheetFixtures.Register(CreateSamusIdentityCharacters());

    /// <summary>Builds a minimal Samus art catalog and its characters.</summary>
    internal static SamusBodyCharacterArtwork CreateSamusIdentityCharacters()
    {
        int firstRecord = SamusBodySources.RecordCount -
            (SamusBodySources.TopSetCount + SamusBodySources.BottomSetCount);
        int[] topStarts = [.. Enumerable.Range(0, SamusBodySources.TopSetCount).Select(index => firstRecord + index)];
        int[] bottomStarts = [.. Enumerable.Range(SamusBodySources.TopSetCount, SamusBodySources.BottomSetCount)
            .Select(index => firstRecord + index)];
        var poses = new int[InstalledSamusBodyArtwork.PoseCount];
        var graphics = new int[InstalledSamusBodyArtwork.PoseCount];
        var landing = new int[SamusDrawingDefinitions.BodyOffsets.LandingVerticalOffsetByteCount];
        var posture = new sbyte[SamusDrawingDefinitions.BodyOffsets.PostureTransitionVerticalOffsetByteCount];
        var drained = new sbyte[SamusDrawingDefinitions.BodyOffsets.DrainedVerticalOffsetByteCount];
        var frames = new SamusBodyFrameSelection[SamusBodySources.FrameCount];
        SamusBodyTileTransfer[][] Half(int count) => Enumerable.Range(0, count)
            .Select(_ => new[] { new SamusBodyTileTransfer(32, 0, new byte[RoomCharacterAtlasFormat.BytesPerTile]) }).ToArray();

        var topBases = new ushort[InstalledSamusBodyArtwork.PoseCount];
        var bottomBases = new ushort[InstalledSamusBodyArtwork.PoseCount];
        SamusSpritemapId?[] entries = [.. Enumerable.Repeat<SamusSpritemapId?>(new SamusSpritemapId(0), SamusSpritemapSources.EntryCount)];
        SamusSpritemapDefinition[] definitions =
        [
            new([SpritePart.Create(offset: Offset16<ScreenPixels>.Of(0, 0), large: false, character: 0, palette: 0, priority: 0)]),
            new([SpritePart.Create(offset: Offset16<ScreenPixels>.Of(1, 0), large: false, character: 1, palette: 0, priority: 0)]),
        ];
        var spritemaps = new SamusSpritemapSources(topBases, bottomBases, entries, definitions);
        var one = new SpritePart[SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount];
        var shared = new SpritePart[one.Length];
        var atmosphere = new SamusAtmosphericSourceArtwork(one, shared);
        ushort[][] Rows() => Enumerable.Range(0, SamusPaletteDefinitions.Death.PaletteCount)
            .Select(_ => new ushort[InstalledSamusDeathPalettes.ColorCount]).ToArray();
        ushort[][][] suited = Enumerable.Range(0, InstalledSamusDeathPalettes.SuitCount).Select(_ => Rows()).ToArray();
        ushort[][] suitless = Rows();
        var whiteout = new ushort[SamusPaletteDefinitions.Death.WhiteoutShadeCount];
        var explosion = new ushort[SamusDeathExplosionTimingDefinitions.RecordCount];
        var deathPalettes = new InstalledSamusDeathPalettes(ToColors(suited), ToColors(suitless), ToColors(whiteout), explosion);
        SamusDeathTileAtlas deathTiles = SamusDeathTileAtlas.Load(Png(SamusDeathTileAtlasFormat.Width,
            SamusDeathTileAtlasFormat.Height));
        var cannonDocument = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = new int[InstalledSamusBodyArtwork.PoseCount],
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            CoverAppearances = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
                .Select(_ => SpriteVisualPart.FromSpritePart(SpritePart.Single(0, 0, 0), SpriteArtwork.CharactersPerRow)).ToArray(),
            CoverTiles = Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
                .Select(_ => new int[SamusArmCannonArtworkFormat.FramesPerDirection - 1]).ToArray(),
        };
        int cannonWidth = SamusArmCannonArtworkFormat.CoverTileCount * 8;
        InstalledSamusArmCannonArtwork cannon = InstalledSamusArmCannonArtworkTooling.Load(
            new MemoryStream(SamusArmCannonSourceArtwork.Write(cannonDocument)),
            Png(cannonWidth, 8));
        return SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = topStarts, BottomSetStarts = bottomStarts, PoseFirstFrames = poses, GraphicsYOffsets = graphics, Frames = frames, Spritemaps = spritemaps, Atmosphere = atmosphere, DeathPalettes = deathPalettes, DeathTilesContentIdentity = deathTiles.ContentIdentity, ArmCannon = cannon, LandingYOffsets = landing, PostureYOffsets = posture, DrainedYOffsets = drained }, Half(topStarts.Length), Half(bottomStarts.Length));

        static MemoryStream Png(int width, int height)
        {
            var stream = new MemoryStream();
            IndexedPng.Write(stream, new(checked((ushort)width), checked((ushort)height)), new byte[width * height], [new RgbaPixel(0, 0, 0), new RgbaPixel(255, 255, 255)]);
            stream.Position = 0;
            return stream;
        }
    }
}
