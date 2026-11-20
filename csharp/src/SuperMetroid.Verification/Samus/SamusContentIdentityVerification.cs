using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;
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
    internal static SamusBodyCharacterArtwork CreateSamusIdentityCharacters() =>
        SamusBodyCharacterArtwork.Create(CreateSamusIdentityParts(0), new Dictionary<SamusBodyPicture, SamusBodyTileTransfer>
        {
            [IdentityTop] = new SamusBodyTileTransfer(32, 0, new byte[RoomCharacterAtlasFormat.BytesPerTile]),
            [IdentityBottom] = new SamusBodyTileTransfer(32, 0, new byte[RoomCharacterAtlasFormat.BytesPerTile]),
        });

    /// <summary>The minimal catalog's top half picture, which every frame selects.</summary>
    private static SamusBodyPicture IdentityTop => new("identity top");

    /// <summary>The minimal catalog's bottom half picture, which every frame selects.</summary>
    private static SamusBodyPicture IdentityBottom => new("identity bottom");

    /// <summary>
    /// A minimal Samus body: every pose's art origin <paramref name="artOrigin"/>, and every picture frame the installed
    /// analysis pictures placed at zero, selecting the same half pictures and showing the same composition in both halves;
    /// the explosion and atmosphere show that composition too.
    /// </summary>
    internal static SamusBodyArtworkParts CreateSamusIdentityParts(int artOrigin)
    {
        var shown = InstalledFixtures.SamusShownFrameAnalysis.Installed();
        var first = new SamusSpritemapId("identity 0");
        var frames = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose =>
        {
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            IEnumerable<SamusAnimationFrame> pictured = shown.PictureFrames.ContainsKey(pose) ? shown.Pictures(pose) : [];
            return (IReadOnlyDictionary<int, SamusBodyDecodedFrame>)pictured.Select(frame => SamusAnimationLayout.Offset(animation, frame)).Distinct()
                .ToDictionary(frame => frame, _ => new SamusBodyDecodedFrame(new SamusBodyFramePictures(IdentityTop, IdentityBottom), 0, first, first));
        });
        var halves = new Dictionary<SamusBodyHalfFrame, SamusSpritemapId?>();
        foreach ((SamusPoseId pose, IReadOnlyDictionary<int, SamusBodyDecodedFrame> decoded) in frames)
        {
            foreach (int frame in decoded.Keys)
            {
                halves.Add(new SamusBodyHalfFrame(pose, SamusBodyHalf.Top, frame), first);
                halves.Add(new SamusBodyHalfFrame(pose, SamusBodyHalf.Bottom, frame), first);
            }
        }
        var definitions = new Dictionary<SamusSpritemapId, SamusSpritemapDefinition>
        {
            [first] = new([SpritePart.Create(offset: Offset16<ScreenPixels>.Of(0, 0), large: false, character: 0, palette: 0, depth: SuperMetroid.Core.Assets.ActorDepth.BehindRoom)]),
            [new SamusSpritemapId("identity 1")] = new([SpritePart.Create(offset: Offset16<ScreenPixels>.Of(1, 0), large: false, character: 1, palette: 0, depth: SuperMetroid.Core.Assets.ActorDepth.BehindRoom)]),
        };
        var spritemaps = new SamusSpritemapSources(definitions, halves,
            SamusDeathSheetComposer.Frames.ToDictionary(name => name, _ => (SamusSpritemapId?)first),
            SamusAtmosphereSheetDefinitions.DivingSplash.Concat(SamusAtmosphereSheetDefinitions.Bubbles)
                .ToDictionary(SamusAtmosphereSheetDefinitions.FrameName, _ => (SamusSpritemapId?)first));
        var one = new SpritePart[SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount];
        var shared = new SpritePart[one.Length];
        var atmosphere = new SamusAtmosphericSourceArtwork(one, shared);
        ushort[][] Rows() => Enumerable.Range(0, SamusShadeDefinitions.Death.ExplosionShadeCount)
            .Select(_ => new ushort[InstalledSamusDeathPalettes.ColorCount]).ToArray();
        ushort[][][] suited = Enumerable.Range(0, InstalledSamusDeathPalettes.SuitCount).Select(_ => Rows()).ToArray();
        ushort[][] suitless = Rows();
        var whiteout = new ushort[SamusShadeDefinitions.Death.WhiteoutShadeCount];
        var explosion = new ushort[SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount];
        var deathPalettes = new InstalledSamusDeathPalettes(ToColors(suited), ToColors(suitless), ToColors(whiteout), explosion);
        var cannonDocument = new SamusArmCannonArtworkDocument
        {
            Version = SamusArmCannonArtworkFormat.Version,
            PoseDescriptors = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => 0),
            DrawingData = new int[SamusArmCannonArtworkFormat.DrawingDataByteCount],
            CoverAppearances = SamusProjectileDirectionCodec.InAimOrder.ToDictionary(direction => direction,
                _ => SpriteVisualPiece.FromSpritePart(SpritePart.Single(0, 0, 0), SpriteArtwork.CharactersPerRow)),
            CoverOrientations = SamusProjectileDirectionCodec.InAimOrder.ToDictionary(direction => direction,
                _ => SamusArmCannonCoverOrientation.Vertical),
        };
        SamusArmCannonSourceArtwork cannon = InstalledSamusArmCannonArtworkTooling.Load(
            new MemoryStream(SamusArmCannonSourceArtwork.Write(cannonDocument)));
        return new SamusBodyArtworkParts
        {
            ArtOrigins = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, _ => artOrigin),
            Frames = frames, Spritemaps = spritemaps, Atmosphere = atmosphere, DeathSteps = deathPalettes.Steps, ArmCannon = cannon,
        };
    }
}
