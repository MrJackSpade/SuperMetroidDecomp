using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies source-identified opening/Ceres presentation coverage; no gameplay read discovery.</summary>
    private static void VerifyIntroContentIdentity()
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
            ("flight", CeresFlightActorDefinitions.RearViewActorCount),
            ("reveal", CeresDestructionActorDefinitions.ZebesActorCount),
            ("destruction", CeresDestructionActorDefinitions.InitialActorCount),
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
            typeof(IntroCinematicArtworkCatalog), typeof(IntroEyeTilemapPresentation),
            typeof(CeresFlightArtworkCatalog), typeof(CeresDestructionArtworkCatalog),
            typeof(CeresFlightActorLayout), typeof(CeresDestructionActorLayout), typeof(CeresRevealActorLayout),
            typeof(IntroCaretSpritePresentation), typeof(IntroMotherBrainSpritePresentation),
            typeof(IntroMotherBrainExplosionSpritePresentation), typeof(IntroRinkaSpritePresentation),
            typeof(IntroEggEffectSpritePresentation), typeof(IntroDiscoveryActorSpritePresentation),
            typeof(IntroScientistSpritePresentation), typeof(CeresFlightSpritePresentation), typeof(CeresDestructionSpritePresentation),
        })
            AssertTrue(type.GetField("<ContentIdentity>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) is null,
                type.Name + " adds no serialized derived-identity field");
        Console.WriteLine($"  Opening content: {edits.Count} independent bundle edits, all actor coordinates, " +
            "canonical frames, legacy caret selection, encoding invariance and host warnings pass without a ROM.");

        static GameContentIdentity IntroIdentity(string digest) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), Guid.Empty,
            [KeyValuePair.Create(GameInstallationLayout.IntroCinematicDirectoryName, digest)]);
    }

    /// <summary>Creates an in-memory intro and Ceres artwork catalog for content-identity checks, with an optional resource variation or reversed sprite-frame insertion order.</summary>
    /// <param name="edit">Optional fixture variation name that changes one resource or selects an equivalent encoding variant.</param>
    /// <param name="reverse">When <see langword="true"/>, inserts sprite frames in reverse order to verify canonical identity.</param>
    /// <returns>The assembled catalog loaded from the generated in-memory artwork resources.</returns>
    private static IntroCinematicArtworkCatalog CreateIntroIdentityFixture(string? edit = null, bool reverse = false)
    {
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = edit == "json-indent" }));
        MemoryStream Png(string name, int width, int height, int colors)
        {
            var pixels = new byte[width * height];
            if (edit == name) pixels[^1] = 1;
            var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, Enumerable.Range(0, colors).Select(index =>
                new Rgba32((byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }
        RoomCharacterAtlas Characters(string name, int bytes)
        {
            int tiles = bytes / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            return RoomCharacterAtlas.Load(Png(name, columns * 8, (tiles + columns - 1) / columns * 8, 16), bytes);
        }
        RoomBackgroundTilemapCell[] Cells(string name, int count, int seed = 0) => Enumerable.Range(0, count)
            .Select(index => new RoomBackgroundTilemapCell
            {
                TileColumn = index == count - 1 && edit == name ? seed + 1 : seed,
                TileRow = 0, Palette = 0, Priority = false, FlipX = false, FlipY = false,
            }).ToArray();
        MemoryStream MapJson(string name, int seed = 0) => Json(new RoomBackgroundTilemapDocument
        {
            Version = RoomBackgroundTilemapFormat.Version,
            Pages = [new RoomBackgroundTilemapPage { Cells = Cells(name, RoomBackgroundTilemapFormat.CellsPerPage, seed) }],
        });
        RoomBackgroundTilemapAtlas Map(string name, int seed = 0) => RoomBackgroundTilemapAtlas.Load(
            MapJson(name, seed), RoomBackgroundTilemapFormat.BytesPerPage);
        PaletteRgb5[] Colors(string name) => Enumerable.Range(0, SnesCgram.ColorCount).Select(index =>
            new PaletteRgb5 { Red = index == SnesCgram.ColorCount - 1 && edit == name ? 1 : 0, Green = 0, Blue = 0 }).ToArray();
        MemoryStream PaletteJson(string name) => Json(new { Version = 1, Colors = Colors(name) });

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
                version = IntroCaretSpriteFormat.PreviousVersion;
            }
            return Json(new { Version = version, Frames = frames });
        }

        var flightActors = Enumerable.Range(0, CeresFlightActorDefinitions.RearViewActorCount).Select(index =>
            new CeresFlightActorPlacement
            {
                Id = CeresFlightActorDefinitions.RearViewPlacementSource(index).Id, X = index + (edit == $"flight-actor-{index}-x" ? 1 : 0),
                Y = index + (edit == $"flight-actor-{index}-y" ? 1 : 0),
            }).ToArray();
        int[] FlightMap(string name)
        {
            var cells = new int[CeresFlightArtworkFormat.MapCellsPerView];
            if (edit == name) cells[^1] = 1;
            return cells;
        }
        var flight = CeresFlightArtworkCatalog.Load(
            Png("flight-mode7", CeresFlightArtworkFormat.Mode7Width, CeresFlightArtworkFormat.Mode7Height, 256),
            Json(new CeresFlightMapDocument
            {
                Version = CeresFlightArtworkFormat.Version, Width = CeresFlightArtworkFormat.MapWidth,
                Height = CeresFlightArtworkFormat.MapHeight,
                FrontTiles = FlightMap("flight-front-map"), RearTiles = FlightMap("flight-rear-map"),
            }),
            Png("flight-objects", CeresFlightArtworkFormat.ObjectWidth, CeresFlightArtworkFormat.ObjectHeight, 16),
            PaletteJson("flight-palette"),
            Sprites("flight", CeresFlightSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name)),
            Json(new CeresFlightActorLayoutDocument { Version = CeresFlightActorLayoutFormat.Version, Actors = flightActors }));

        var revealActors = Enumerable.Range(0, CeresDestructionActorDefinitions.ZebesActorCount).Select(index =>
            new CeresRevealActorPlacement
            {
                Id = CeresDestructionActorDefinitions.ZebesPlacementSource(index).Id, X = index + (edit == $"reveal-actor-{index}-x" ? 1 : 0),
                Y = index + (edit == $"reveal-actor-{index}-y" ? 1 : 0),
            }).ToArray();
        var destructionActors = Enumerable.Range(0, CeresDestructionActorDefinitions.InitialActorCount).Select(index =>
            new CeresDestructionActorPlacement
            {
                Id = CeresDestructionActorDefinitions.InitialPlacementId(index), X = index + (edit == $"destruction-actor-{index}-x" ? 1 : 0),
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
        var destruction = CeresDestructionArtworkCatalog.Load(Json(new CeresDestructionMapDocument
        {
            Version = CeresDestructionArtworkFormat.Version, Width = CeresDestructionArtworkFormat.MapWidth,
            Height = CeresDestructionArtworkFormat.MapHeight, Views = views,
        }), MapJson("zebes-map"),
            Png("zebes-characters", 256, 128, 16),
            Sprites("destruction", CeresDestructionSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name)),
            Json(new CeresRevealActorLayoutDocument { Version = CeresRevealActorLayoutFormat.Version, Actors = revealActors }),
            Json(new CeresDestructionActorLayoutDocument { Version = CeresDestructionActorLayoutFormat.Version, Actors = destructionActors }));

        RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, IntroCinematicArtworkFormat.BackgroundPageCount)
            .Select(index => Map("page-" + index, index)).ToArray();
        if (edit == "page-order") Array.Reverse(pages);
        return new IntroCinematicArtworkCatalog(
            Characters("background", IntroCinematicArtworkFormat.BackgroundByteCount),
            Characters("intro-objects", IntroCinematicArtworkFormat.IntroObjectByteCount),
            Characters("cinematic-objects", IntroCinematicArtworkFormat.CinematicObjectByteCount), pages,
            Map("portrait"), Map("narration-map"),
            IntroFinalLineTilemap.Load(Json(new IntroFinalLineTilemapDocument
            {
                Version = IntroFinalLineTilemapFormat.Version, Cells = Cells("divider", IntroFinalLineTilemapFormat.CellCount),
            })),
            IntroEyeTilemapPresentation.Load(Json(new IntroEyeTilemapDocument
            {
                Version = IntroEyeTilemapFormat.Version,
                Frames = Enumerable.Range(0, IntroEyeTilemapFormat.FrameCount).Select(index => new IntroEyeTilemapFrame
                {
                    Id = IntroEyeTilemapFormat.FrameId(index), Cells = Cells("eye-" + index, IntroEyeTilemapFormat.CellsPerFrame),
                }).ToArray(),
            })),
            IntroCaretSpritePresentation.Load(Sprites("caret", IntroCaretSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name), IntroCaretSpriteFormat.Version)),
            IntroMotherBrainSpritePresentation.Load(Sprites("mother-brain", IntroMotherBrainSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            IntroMotherBrainExplosionSpritePresentation.Load(Sprites("mb-explosion", IntroMotherBrainExplosionSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            IntroRinkaSpritePresentation.Load(Sprites("rinka", IntroRinkaSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            IntroEggEffectSpritePresentation.Load(Sprites("egg", IntroEggEffectSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            IntroDiscoveryActorSpritePresentation.Load(Sprites("discovery", IntroDiscoveryActorSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            IntroScientistSpritePresentation.Load(Sprites("scientist", IntroScientistSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            IntroCinematicPalette.Load(PaletteJson("intro-palette")), flight, destruction);
    }
}
