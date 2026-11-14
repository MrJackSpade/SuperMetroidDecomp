using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>A minimal, ROM-free Samus body catalog that checks bind Samus to when they need no retail artwork.</summary>
internal static class SamusContentIdentityVerification
{
    /// <summary>
    /// Builds a minimal Samus art catalog; its characters are registered, so bodies built from its tables compose their
    /// sheets from them.
    /// </summary>
    internal static SamusBodySources CreateSamusIdentityFixture() =>
        SamusBodySheetFixtures.Register(CreateSamusIdentityCharacters());

    /// <summary>Builds a minimal Samus art catalog and its characters.</summary>
    internal static SamusBodyCharacterArtwork CreateSamusIdentityCharacters()
    {
        int firstRecord = SamusBodySources.RecordCount -
            (SamusBodySources.TopSetCount + SamusBodySources.BottomSetCount);
        int[] topStarts = [.. Enumerable.Range(0, SamusBodySources.TopSetCount).Select(index => firstRecord + index)];
        int[] bottomStarts = [.. Enumerable.Range(SamusBodySources.TopSetCount, SamusBodySources.BottomSetCount)
            .Select(index => firstRecord + index)];
        Dictionary<SamusPoseId, int> poses = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => 0);
        var graphics = new int[SamusBodySources.PoseCount];
        Dictionary<SamusPoseId, SamusBodyPosePlacement> placements = SamusBodyPlacementDecoding.Decode(graphics,
            new int[SamusBodyPlacementSourceDefinitions.LandingByteCount], new int[SamusBodyPlacementSourceDefinitions.PostureByteCount],
            new int[SamusBodyPlacementSourceDefinitions.DrainedByteCount]);
        var frames = new SamusBodyFrameSelection[SamusBodySources.FrameCount];
        SamusBodyTileTransfer[][] Half(int count) => Enumerable.Range(0, count)
            .Select(_ => new[] { new SamusBodyTileTransfer(32, 0, new byte[RoomCharacterAtlasFormat.BytesPerTile]) }).ToArray();

        Dictionary<SamusPoseId, int> topBases = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => 0);
        Dictionary<SamusPoseId, int> bottomBases = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => 0);
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
        var explosion = new ushort[SamusDeathExplosionTimingDefinitions.StepCount];
        var deathPalettes = new InstalledSamusDeathPalettes(ToColors(suited), ToColors(suitless), ToColors(whiteout), explosion);
        var cannonDocument = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => 0),
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            CoverAppearances = Enumerable.Range(0, SamusProjectileDirectionCodec.Count)
                .Select(_ => SpriteVisualPart.FromSpritePart(SpritePart.Single(0, 0, 0), SpriteArtwork.CharactersPerRow)).ToArray(),
            CoverTiles = Enumerable.Range(0, SamusProjectileDirectionCodec.Count)
                .Select(_ => new int[SamusArmCannonArtworkFormat.FramesPerDirection - 1]).ToArray(),
        };
        SamusArmCannonSourceArtwork cannon = InstalledSamusArmCannonArtworkTooling.Load(
            new MemoryStream(SamusArmCannonSourceArtwork.Write(cannonDocument)));
        return SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = topStarts, BottomSetStarts = bottomStarts, PoseFirstFrames = poses, Placements = placements, Frames = frames, Spritemaps = spritemaps, Atmosphere = atmosphere, DeathSteps = deathPalettes.Steps, ArmCannon = cannon }, Half(topStarts.Length), Half(bottomStarts.Length));
    }
}
