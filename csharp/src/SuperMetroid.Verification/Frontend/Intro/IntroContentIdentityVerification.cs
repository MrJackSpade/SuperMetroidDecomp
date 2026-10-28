using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
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
            "background", "intro-objects", "cinematic-objects", "page-0", "page-1", "page-2", "page-3",
            "page-order", "portrait", "narration-map", "divider", "eye-0", "eye-1", "eye-2", "eye-3",
            "caret-sprites", "mother-brain-sprites", "mb-explosion-sprites", "rinka-sprites", "egg-sprites",
            "discovery-sprites", "scientist-sprites", "intro-palette", "flight-mode7", "flight-objects",
            "flight-front-map", "flight-rear-map", "flight-palette", "flight-sprites",
            "destruction-map-0", "destruction-map-1", "destruction-map-2", "destruction-map-order",
            "zebes-map", "zebes-characters", "destruction-sprites",
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
        foreach (string edit in new[] { "json-indent", "png-colors", "legacy-caret" })
            AssertEqual(baseline, CreateIntroIdentityFixture(edit).ContentIdentity,
                edit + " preserves equivalent selected content");
        foreach (Type type in new[]
        {
            typeof(IntroCinematicArtwork), typeof(IntroCinematicSourceArtwork), typeof(IntroEyeTilemapPresentation),
            typeof(CeresFlightArtwork), typeof(CeresDestructionArtwork),
            typeof(CeresFlightActorPlacements), typeof(CeresDestructionActorPlacements), typeof(CeresRevealActorPlacements),
            typeof(IntroCaretSpritePresentation), typeof(IntroMotherBrainSpritePresentation),
            typeof(IntroMotherBrainExplosionSpritePresentation), typeof(IntroRinkaSpritePresentation),
            typeof(IntroEggEffectSpritePresentation), typeof(IntroDiscoveryActorSpritePresentation),
            typeof(IntroScientistSpritePresentation), typeof(CeresFlightSpritePresentation), typeof(CeresDestructionSpritePresentation),
        })
            AssertTrue(!MetadataShape.DeclaresField(type, "<ContentIdentity>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                type.Name + " adds no serialized derived-identity field");
        Console.WriteLine($"  Opening content: {edits.Count} independent bundle edits, all actor coordinates, " +
            "canonical frames, legacy caret selection, encoding invariance and host warnings pass without a ROM.");

        // Place an opening-bundle digest into a host identity for compatibility-warning checks.
        static GameContentIdentity IntroIdentity(string digest) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), Guid.Empty,
            [KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, digest)]);
    }

    /// <summary>Builds a complete synthetic opening asset bundle with one selected edit or encoding-only change.</summary>
    private static IntroCinematicArtwork CreateIntroIdentityFixture(string? edit = null, bool reverse = false)
    {
        // Serialize a fixture document, optionally changing only whitespace formatting.
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            (edit == "json-indent" ? CamelCaseIndentedJson : CamelCaseJson)));
        // Create indexed artwork with an optional last-pixel edit and separately variable display colors.
        MemoryStream Png(string name, int width, int height, int colors)
        {
            var pixels = new byte[width * height];
            if (edit == name) pixels[^1] = 1;
            var png = new MemoryStream();
            IndexedPng.Write(png, new(checked((ushort)width), checked((ushort)height)), pixels, Enumerable.Range(0, colors).Select(index =>
                new RgbaPixel((byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }
        // Size a synthetic character atlas to the requested SNES tile byte count.
        RoomCharacterAtlas Characters(string name, int bytes)
        {
            int tiles = bytes / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            return RoomCharacterAtlasPng.Read(Png(name, columns * 8, (tiles + columns - 1) / columns * 8, 16), bytes);
        }
        // Build a page of visual cells with an optional edit in its last tile column.
        RoomBackgroundTilemapCell[] Cells(string name, int count, int seed = 0) => Enumerable.Range(0, count)
            .Select(index => new RoomBackgroundTilemapCell
            {
                TileColumn = index == count - 1 && edit == name ? seed + 1 : seed,
                TileRow = 0, Palette = 0, Priority = false, FlipX = false, FlipY = false,
            }).ToArray();
        // Wrap visual cells in a one-page tilemap document.
        MemoryStream MapJson(string name, int seed = 0) => Json(new RoomBackgroundTilemapDocument
        {
            Version = RoomBackgroundTilemapFormat.Version,
            Pages = [new RoomBackgroundTilemapPage { Cells = Cells(name, RoomBackgroundTilemapFormat.CellsPerPage, seed) }],
        });
        // Load a generated page through the production background-tilemap codec.
        RoomBackgroundTilemapAtlas Map(string name, int seed = 0) => RoomBackgroundTilemapAtlas.Load(
            MapJson(name, seed), RoomBackgroundTilemapFormat.BytesPerPage);
        // Build a full black CGRAM image with an optional last-color red-channel edit.
        PaletteRgb5[] Colors(string name) => Enumerable.Range(0, SnesCgram.ColorCount).Select(index =>
            new PaletteRgb5 { Red = index == SnesCgram.ColorCount - 1 && edit == name ? 1 : 0, Green = 0, Blue = 0 }).ToArray();
        // Encode a complete editable palette document for one opening asset family.
        MemoryStream PaletteJson(string name) => Json(new { Version = 1, Colors = Colors(name) });

        // Build semantic sprite frames, optionally reversing key insertion or using the legacy caret selection.
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
            if (family == "caret" && edit == "legacy-caret")
            {
                // Only the first old frame is selected by the current loader. Unused blink-frame
                // leftovers must not change the identity of the visible caret selection.
                frames = IntroCaretSpriteDefinitions.PreviousFrameNames.Select((name, index) =>
                    KeyValuePair.Create(name, new[] { part with { OffsetX = index } })).ToDictionary();
                version = IntroCaretSpriteVersion.Previous.Revision;
            }
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
            var cells = new int[CeresFlightArtworkFormat.MapCellsPerView];
            if (edit == name) cells[^1] = 1;
            return cells;
        }
        var flight = CeresFlightCharacters.Read(
            Png("flight-mode7", CeresFlightArtworkFormat.FloorWidth, CeresFlightArtworkFormat.FloorHeight, 256),
            Json(new CeresFlightMapDocument
            {
                Version = CeresFlightArtworkFormat.Version, Width = CeresFlightArtworkFormat.MapWidth,
                Height = CeresFlightArtworkFormat.MapHeight,
                FrontTiles = FlightMap("flight-front-map"), RearTiles = FlightMap("flight-rear-map"),
            }),
            Png("flight-objects", CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight, 16),
            CeresFlightPaletteFile.Read(PaletteJson("flight-palette")),
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
        int[][] views = Enumerable.Range(0, CeresDestructionArtworkFormat.ViewCount).Select(view =>
        {
            var cells = new int[CeresDestructionArtworkFormat.CellsPerView];
            cells[0] = view;
            if (edit == "destruction-map-" + view) cells[^1] = 1;
            return cells;
        }).ToArray();
        if (edit == "destruction-map-order") Array.Reverse(views);
        var destruction = CeresDestructionArtwork.Load(Json(new CeresDestructionMapDocument
        {
            Version = CeresDestructionArtworkFormat.Version, Width = CeresDestructionArtworkFormat.MapWidth,
            Height = CeresDestructionArtworkFormat.MapHeight, Views = views,
        }), MapJson("zebes-map"),
            RoomCharacterAtlasPng.Read(Png("zebes-characters", 256, 128, 16), CeresDestructionArtworkFormat.ZebesCharacterByteCount),
            Json(new CeresRevealActorLayoutDocument { Version = CeresRevealActorLayoutFormat.Version, Actors = revealActors }),
            Json(new CeresDestructionActorLayoutDocument { Version = CeresDestructionActorLayoutFormat.Version, Actors = destructionActors }));

        RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount)
            .Select(index => Map("page-" + index, index)).ToArray();
        if (edit == "page-order") Array.Reverse(pages);
        return new IntroCinematicSourceArtwork(new IntroCinematicArtworkParts { BackgroundCharacters = Characters("background", IntroCinematicArtworkFormat.BackgroundByteCount), IntroObjectCharacters = Characters("intro-objects", IntroCinematicArtworkFormat.IntroObjectByteCount), CinematicObjectCharacters = Characters("cinematic-objects", IntroCinematicArtworkFormat.CinematicObjectByteCount), BackgroundPages = pages, PortraitTilemap = Map("portrait"), InitialNarrationTilemap = Map("narration-map"), FinalLine = IntroFinalLineTilemap.Load(Json(new IntroFinalLineTilemapDocument
            {
                Version = IntroFinalLineTilemapFormat.Version, Cells = Cells("divider", IntroFinalLineTilemapFormat.CellCount),
            })), EyeFrames = IntroEyeTilemapPresentation.Load(Json(new IntroEyeTilemapDocument
            {
                Version = IntroEyeTilemapFormat.Version,
                Frames = Enumerable.Range(0, IntroEyeTilemapFormat.FrameCount).Select(index => new IntroEyeTilemapFrame
                {
                    Id = IntroEyeTilemapFormat.FrameId(index), Cells = Cells("eye-" + index, IntroEyeTilemapFormat.CellsPerFrame),
                }).ToArray(),
            })), CaretSprites = IntroCaretSpritePresentation.Load(Sprites("caret", IntroCaretSpriteDefinitions.Frames.Select(frame => frame.Name), IntroCaretSpriteVersion.Current.Revision)), MotherBrainSprites = IntroMotherBrainSpritePresentation.Load(Sprites("mother-brain", IntroMotherBrainSpriteDefinitions.Frames.Select(frame => frame.Name))), MotherBrainExplosionSprites = IntroMotherBrainExplosionSpritePresentation.Load(Sprites("mb-explosion", IntroMotherBrainExplosionSpriteDefinitions.Frames.Select(frame => frame.Name))), RinkaSprites = IntroRinkaSpritePresentation.Load(Sprites("rinka", IntroRinkaSpriteDefinitions.Frames.Select(frame => frame.Name))), EggEffectSprites = IntroEggEffectSpritePresentation.Load(Sprites("egg", IntroEggEffectSpriteDefinitions.Frames.Select(frame => frame.Name))), DiscoveryActorSprites = IntroDiscoveryActorSpritePresentation.Load(Sprites("discovery", IntroDiscoveryActorSpriteDefinitions.Frames.Select(frame => frame.Name))), ScientistSprites = IntroScientistSpritePresentation.Load(Sprites("scientist", IntroScientistSpriteDefinitions.Frames.Select(frame => frame.Name))), Palette = IntroCinematicPaletteFile.Read(PaletteJson("intro-palette")), CeresFlight = flight, CeresDestruction = destruction, CeresDestructionSprites = CeresDestructionSpritePresentation.Load(Sprites("destruction", CeresDestructionSpriteDefinitions.Frames.Select(frame => frame.Name))) });
    }

    /// <summary>Runs selected-content identity checks for opening and Ceres assets.</summary>
    [VerificationCommand("--intro-content-identity")]
    internal static int IntroContentIdentityCommand()
    {
        Suite(nameof(VerifyIntroContentIdentity), () => VerifyIntroContentIdentity());
        return 0;
    }
}
