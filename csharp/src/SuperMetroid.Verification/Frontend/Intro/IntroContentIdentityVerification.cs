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
        AssertEqual(baseline, CreateIntroIdentityFixture(reverse: true).ContentIdentity,
            "opening sprite frame-key insertion order is canonical");
        var edits = new List<string>
        {
            "caret-sprites", "next-page-arrow-sprites", "mother-brain-sprites", "mb-explosion-sprites", "rinka-sprites", "egg-sprites",
            "discovery-sprites", "scientist-sprites", "opening-backdrop",
            "flight-front-map", "flight-rear-map", "flight-backdrop", "flight-sprites",
            "destruction-map-0", "destruction-map-1", "destruction-map-2", "destruction-map-order",
            "destruction-sprites",
        };
        foreach ((string family, int count) in new[]
        {
            new VerifyIntroContentIdentityCase("flight", CeresFlightActorDefinitions.RearViewActorCount),
            new VerifyIntroContentIdentityCase("reveal", CeresDestructionActorDefinitions.ZebesActorCount),
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
        Console.WriteLine($"  Opening content: {edits.Count} independent bundle edits, all actor coordinates, " +
            "canonical frames, encoding invariance and host warnings pass without a ROM.");

        // Place an opening-bundle digest into a host identity for compatibility-warning checks.
        static GameContentIdentity IntroIdentity(string digest) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), Guid.Empty,
            [KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, digest)]);
    }

    /// <summary>
    /// Builds a complete synthetic opening asset bundle of installed documents with one selected edit or encoding-only
    /// change; its characters and layouts are import data no identity covers (#1636, #1638).
    /// </summary>
    private static IntroCinematicSourceArtwork CreateIntroIdentityFixture(string? edit = null, bool reverse = false)
    {
        // Serialize a fixture document, optionally changing only whitespace formatting.
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            (edit == "json-indent" ? CamelCaseIndentedJson : CamelCaseJson)));
        // A black backdrop with an optional red-channel edit; the colours behind the backdrops are the import's alone (#1544).
        RgbColor Backdrop(string name) => new FadeLevelColour(edit == name ? 1 : 0, 0, 0).ToColour();

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
            return Json(new { Version = version, Frames = frames });
        }

        var flightActors = Enumerable.Range(0, CeresFlightActorDefinitions.RearViewActorCount).Select(index =>
            new CeresFlightActorPlacement
            {
                Id = CeresFlightActors.RearViewPlacementId(index), X = index + (edit == $"flight-actor-{index}-x" ? 1 : 0),
                Y = index + (edit == $"flight-actor-{index}-y" ? 1 : 0),
            }).ToArray();
        // Build a Mode 7 flight view with an optional final-cell edit.
        int[] FlightMap(string name)
        {
            var cells = new int[CeresFlightArtworkFileFormat.MapCellsPerView];
            if (edit == name) cells[^1] = 1;
            return cells;
        }
        var flight = CeresFlightSourceArtwork.Read(
            Json(new CeresFlightMapDocument
            {
                Version = CeresFlightArtworkFileFormat.Version, Width = CeresFlightArtworkFileFormat.MapWidth,
                Height = CeresFlightArtworkFileFormat.MapHeight,
                FrontViewTiles = FlightMap("flight-front-map"), RearViewTiles = FlightMap("flight-rear-map"),
            }),
            Backdrop("flight-backdrop"),
            Sprites("flight", CeresFlightSpriteDefinitions.Frames.Select(frame => frame.Name)),
            Json(new CeresFlightActorLayoutDocument { Version = CeresFlightActorLayoutFormat.Version, Actors = flightActors }));

        var revealActors = Enumerable.Range(0, CeresDestructionActorDefinitions.ZebesActorCount).Select(index =>
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
        int[][] views = Enumerable.Range(0, CeresDestructionMapFormat.ViewCount).Select(view =>
        {
            var cells = new int[CeresDestructionMapFormat.CellsPerView];
            cells[0] = view;
            if (edit == "destruction-map-" + view) cells[^1] = 1;
            return cells;
        }).ToArray();
        if (edit == "destruction-map-order") Array.Reverse(views);
        var destructionMaps = CeresDestructionMaps.Load(Json(new CeresDestructionMapDocument
        {
            Version = CeresDestructionMapFormat.Version, Width = CeresDestructionMapFormat.MapWidth,
            Height = CeresDestructionMapFormat.MapHeight, Views = views,
        }));
        var destruction = CeresDestructionArtworkFile.Read(
            Json(new CeresRevealActorLayoutDocument { Version = CeresRevealActorLayoutFormat.Version, Actors = revealActors }),
            Json(new CeresDestructionActorLayoutDocument { Version = CeresDestructionActorLayoutFormat.Version, Actors = destructionActors }));

        return new IntroCinematicSourceArtwork(new IntroCinematicArtworkParts { CaretSprites = IntroCaretSpritePresentation.Load(Sprites("caret", IntroCaretSpriteDefinitions.Frames.Select(frame => frame.Name), IntroCaretSpriteVersion.Current.Revision)), NextPageArrowSprites = IntroNextPageArrowSpritePresentation.Load(Sprites("next-page-arrow", IntroNextPageArrowSpriteDefinitions.Frames.Select(frame => frame.Name), IntroNextPageArrowSpriteFormat.Version)), MotherBrainSprites = IntroMotherBrainSpritePresentation.Load(Sprites("mother-brain", IntroMotherBrainSpriteDefinitions.Frames.Select(frame => frame.Name))), MotherBrainExplosionSprites = IntroMotherBrainExplosionSpritePresentation.Load(Sprites("mb-explosion", IntroMotherBrainExplosionSpriteDefinitions.Frames.Select(frame => frame.Name))), RinkaSprites = IntroRinkaSpritePresentation.Load(Sprites("rinka", IntroRinkaSpriteDefinitions.Frames.Select(frame => frame.Name))), EggEffectSprites = IntroEggEffectSpritePresentation.Load(Sprites("egg", IntroEggEffectSpriteDefinitions.Frames.Select(frame => frame.Name))), DiscoveryActorSprites = IntroDiscoveryActorSpritePresentation.Load(Sprites("discovery", IntroDiscoveryActorSpriteDefinitions.Frames.Select(frame => frame.Name), IntroDiscoveryActorSpriteFormat.Version)), ScientistSprites = IntroScientistSpritePresentation.Load(Sprites("scientist", IntroScientistSpriteDefinitions.Frames.Select(frame => frame.Name), IntroScientistSpriteFormat.Version)), Backdrop = Backdrop("opening-backdrop"), CeresFlight = flight, CeresDestruction = destruction, CeresDestructionMaps = destructionMaps, CeresDestructionSprites = CeresDestructionSpritePresentation.Load(Sprites("destruction", CeresDestructionSpriteFrameDefinitions.Frames.Select(frame => frame.Name))) });
    }

    /// <summary>Runs selected-content identity checks for opening and Ceres assets.</summary>
    [VerificationCommand("--intro-content-identity")]
    internal static int IntroContentIdentityCommand()
    {
        Suite(nameof(VerifyIntroContentIdentity), () => VerifyIntroContentIdentity());
        return 0;
    }
}
