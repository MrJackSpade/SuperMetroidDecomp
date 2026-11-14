using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Verification.Frontend.Intro;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>Checks the canonical selected-content digest for the opening and Ceres presentation bundle.</summary>
internal static class IntroContentIdentityVerification
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Verifies source-identified opening/Ceres presentation coverage; no gameplay read discovery.</summary>
    internal static void VerifyIntroContentIdentity()
    {
        string baseline = CreateIntroIdentityFixture().ContentIdentity;
        // The compositions are import data (#1638): the installed bundle's identity leaves them out, and the import's
        // composition identity, which the sheets composed from them carry, covers each family.
        string spriteBaseline = CreateIntroSpritesIdentity(null, reverse: false);
        AssertEqual(spriteBaseline, CreateIntroSpritesIdentity(null, reverse: true), "opening sprite frame-key insertion order is canonical");
        string[] spriteEdits =
        [
            "caret-sprites", "next-page-arrow-sprites", "mother-brain-sprites", "mb-explosion-sprites", "rinka-sprites", "egg-sprites",
            "discovery-sprites", "scientist-sprites", "flight-sprites", "destruction-sprites",
        ];
        foreach (string edit in spriteEdits)
            AssertTrue(spriteBaseline != CreateIntroSpritesIdentity(edit, reverse: false), edit + " changes the opening's composition identity");
        var edits = new List<string>
        {
            "opening-backdrop", "flight-backdrop",
        };
        foreach ((string family, int count) in new[]
        {
            new VerifyIntroContentIdentityCase("flight", CeresFlightActorDefinitions.RearViewActorCount),
            new VerifyIntroContentIdentityCase("reveal", CeresDestructionActorDefinitions.RevealPlacementCount),
            new VerifyIntroContentIdentityCase("destruction", CeresDestructionActorDefinitions.InitialActorCount),
        })
        for (int index = 0; index < count; index++)
        {
            edits.Add($"{family}-actor-{index}-x");
            edits.Add($"{family}-actor-{index}-y");
        }
        foreach (string edit in edits)
        {
            string changed = CreateIntroIdentityFixture(edit).ContentIdentity;
            AssertTrue(baseline != changed, edit + " changes the whole selected opening bundle");
            var before = IntroIdentity(baseline);
            var after = IntroIdentity(changed);
            AssertTrue(before.CompositeSha256 != after.CompositeSha256, edit + " changes host identity");
            AssertTrue(before.GetCompatibilityWarnings(after.ToSnapshot(), "test").Single()
                .Contains(GameInstallationLayout.IntroCinematicDirectoryName, StringComparison.Ordinal),
                edit + " identifies the opening bundle in compatibility warnings");
        }
        foreach (string edit in new[] { "json-indent" })
            AssertEqual(baseline, CreateIntroIdentityFixture(edit).ContentIdentity,
                edit + " preserves equivalent selected content");
        foreach (Type type in new[]
        {
            typeof(IntroCinematicArtwork), typeof(IntroCinematicSourceArtwork), typeof(IntroEyeTilemapPresentation),
            typeof(CeresFlightArtwork), typeof(CeresFlightSourceArtwork), typeof(CeresFlightCharacters), typeof(CeresDestructionArtwork),
            typeof(CeresFlightActorPlacements), typeof(CeresDestructionActorPlacements), typeof(CeresRevealActorPlacements),
            typeof(IntroCaretSpritePresentation), typeof(IntroNextPageArrowSpritePresentation), typeof(IntroMotherBrainSpritePresentation),
            typeof(IntroMotherBrainExplosionSpritePresentation), typeof(IntroRinkaSpritePresentation),
            typeof(IntroEggEffectSpritePresentation), typeof(IntroDiscoveryActorSpritePresentation),
            typeof(IntroScientistSpritePresentation), typeof(CeresFlightSpritePresentation), typeof(CeresDestructionSpritePresentation),
        })
            AssertTrue(!MetadataShape.DeclaresField(type, "<ContentIdentity>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                type.Name + " adds no serialized derived-identity field");
        Console.WriteLine($"  Opening content: {edits.Count} independent bundle edits, {spriteEdits.Length} composition edits, all actor coordinates, " +
            "canonical frames, encoding invariance and host warnings pass without a ROM.");

        // Place an opening-bundle digest into a host identity for compatibility-warning checks.
        static GameContentIdentity IntroIdentity(string digest) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), Guid.Empty,
            [KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, digest)]);
    }

    /// <summary>
    /// The composition identity of a complete synthetic set of the opening's and the Ceres scenes' compositions with one
    /// selected edit, optionally reversing each family's key insertion: the opening's and the approach's together.
    /// </summary>
    private static string CreateIntroSpritesIdentity(string? edit, bool reverse)
    {
        var sprites = new IntroCinematicSprites(
            IntroCaretSpritePresentation.Load(Sprites("caret", IntroCaretSpriteDefinitions.Frames.Select(frame => frame.Name), IntroCaretSpriteVersion.Current.Revision)),
            IntroNextPageArrowSpritePresentation.Load(Sprites("next-page-arrow", IntroNextPageArrowSpriteDefinitions.Frames.Select(frame => frame.Name), IntroNextPageArrowSpriteFormat.Version)),
            IntroMotherBrainSpritePresentation.Load(Sprites("mother-brain", IntroMotherBrainSpriteDefinitions.Frames.Select(frame => frame.Name))),
            IntroMotherBrainExplosionSpritePresentation.Load(Sprites("mb-explosion", IntroMotherBrainExplosionSpriteDefinitions.Frames.Select(frame => frame.Name))),
            IntroRinkaSpritePresentation.Load(Sprites("rinka", IntroRinkaSpriteDefinitions.Frames.Select(frame => frame.Name))),
            IntroEggEffectSpritePresentation.Load(Sprites("egg", IntroEggEffectSpriteDefinitions.Frames.Select(frame => frame.Name))),
            IntroDiscoveryActorSpritePresentation.Load(Sprites("discovery", IntroDiscoveryActorSpriteDefinitions.Frames.Select(frame => frame.Name), IntroDiscoveryActorSpriteFormat.Version)),
            IntroScientistSpritePresentation.Load(Sprites("scientist", IntroScientistSpriteDefinitions.Frames.Select(frame => frame.Name), IntroScientistSpriteFormat.Version)),
            CeresDestructionSpritePresentation.Load(Sprites("destruction", CeresDestructionSpriteFrameDefinitions.Frames.Select(frame => frame.Name), CeresDestructionSpriteFormat.Version)));
        CeresFlightSpritePresentation flight = CeresFlightSpritePresentation.Load(Sprites("flight", CeresFlightSpriteDefinitions.Frames.Select(frame => frame.Name)));
        return sprites.ContentIdentity + flight.ContentIdentity;

        // Build semantic sprite frames, optionally reversing key insertion.
        MemoryStream Sprites(string family, IEnumerable<string> sourceNames, int version = 1)
        {
            var part = new SpriteVisualPart
            {
                OffsetX = 0, OffsetY = 0, TileColumn = 0, TileRow = 0, Size = 8,
                Priority = 1, Palette = null, FlipX = false, FlipY = false,
            };
            string[] names = sourceNames.ToArray();
            var frames = new Dictionary<string, SpriteVisualPart[]>();
            foreach (string name in reverse ? names.Reverse() : names)
                frames.Add(name, [part with { OffsetX = name == names[^1] && edit == family + "-sprites" ? 1 : 0 }]);
            return new(JsonSerializer.SerializeToUtf8Bytes(new { Version = version, Frames = frames }, CamelCaseJson));
        }
    }

    /// <summary>
    /// Builds a complete synthetic opening asset bundle of installed documents with one selected edit or encoding-only
    /// change; its characters, layouts, Mode 7 maps and compositions are import data no installed identity covers (#1636,
    /// #1638, #1693).
    /// </summary>
    private static IntroCinematicSourceArtwork CreateIntroIdentityFixture(string? edit = null)
    {
        // Serialize a fixture document, optionally changing only whitespace formatting.
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            (edit == "json-indent" ? CamelCaseIndentedJson : CamelCaseJson)));
        // A black backdrop with an optional red-channel edit; the colours behind the backdrops are the import's alone (#1544).
        RgbColor Backdrop(string name) => new FadeLevelColour(edit == name ? 1 : 0, 0, 0).ToColour();

        var flightActors = Enumerable.Range(0, CeresFlightActorDefinitions.RearViewActorCount).Select(index =>
            new CeresFlightActorPlacement
            {
                Id = CeresFlightActors.RearViewPlacementId(index), X = index + (edit == $"flight-actor-{index}-x" ? 1 : 0),
                Y = index + (edit == $"flight-actor-{index}-y" ? 1 : 0),
            }).ToArray();
        var flight = CeresFlightSourceArtwork.Read(
            Backdrop("flight-backdrop"),
            Json(new CeresFlightActorLayoutDocument { Version = CeresFlightActorLayoutFormat.Version, Actors = flightActors }));

        var revealActors = Enumerable.Range(0, CeresDestructionActorDefinitions.RevealPlacementCount).Select(index =>
            new CeresRevealActorPlacement
            {
                Id = CeresDestructionActors.ZebesPlacementId(index), X = index + (edit == $"reveal-actor-{index}-x" ? 1 : 0),
                Y = index + (edit == $"reveal-actor-{index}-y" ? 1 : 0),
            }).ToArray();
        var destructionActors = Enumerable.Range(0, CeresDestructionActorDefinitions.InitialActorCount).Select(index =>
            new CeresDestructionActorPlacement
            {
                Id = CeresDestructionActors.InitialPlacementId(index), X = index + (edit == $"destruction-actor-{index}-x" ? 1 : 0),
                Y = index + (edit == $"destruction-actor-{index}-y" ? 1 : 0),
            }).ToArray();
        var destruction = CeresDestructionArtworkFile.Read(
            Json(new CeresRevealActorLayoutDocument { Version = CeresRevealActorLayoutFormat.Version, Actors = revealActors }),
            Json(new CeresDestructionActorLayoutDocument { Version = CeresDestructionActorLayoutFormat.Version, Actors = destructionActors }));

        return new IntroCinematicSourceArtwork(new IntroCinematicSceneParts { Backdrop = Backdrop("opening-backdrop"), CeresFlight = flight, CeresDestruction = destruction });
    }

    /// <summary>Runs selected-content identity checks for opening and Ceres assets.</summary>
    [VerificationCommand("--intro-content-identity")]
    internal static int IntroContentIdentityCommand()
    {
        Suite(nameof(VerifyIntroContentIdentity), () => VerifyIntroContentIdentity());
        return 0;
    }
}
