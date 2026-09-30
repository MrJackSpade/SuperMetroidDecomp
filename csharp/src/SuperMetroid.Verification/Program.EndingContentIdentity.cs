using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks statically identified ending presentation domains without a cartridge or playthrough.</summary>
    private static void VerifyEndingContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateEndingIdentityFixture();
        IReadOnlyDictionary<string, string> reordered = CreateEndingIdentityFixture(reverse: true);
        foreach ((string component, string digest) in baseline)
            AssertEqual(digest, reordered[component], component + " canonical frame lookup order");

        var edits = new List<(string Edit, string Component)>();
        foreach (string edit in new[]
        {
            "clouds", "explosion", "waiting", "shooting", "suitless", "fragment-0", "fragment-1",
            "fragment-2", "fragment-3", "fragment-order", "post-fragment-a", "post-fragment-b",
            "logo-tiles", "logo-map", "cloud-sprites", "explosion-sprites", "text-sprites",
            "reward-sprites", "logo-sprites", "part-x", "part-y", "part-column", "part-row",
            "part-size", "part-priority", "part-palette", "part-flip-x", "part-flip-y",
            "part-order", "part-count", "map-column", "map-row", "map-palette", "map-priority",
            "map-flip-x", "map-flip-y",
        }) edits.Add((edit, GameInstallationLayout.EndingObjectDirectoryName));
        foreach (string edit in new[]
        {
            "EscapeA-map", "EscapeA-pixels", "EscapeB-map", "EscapeB-pixels",
            "PlanetExplosion-map", "PlanetExplosion-pixels", "scene-order", "reward-map", "reward-pixels",
        }) edits.Add((edit, GameInstallationLayout.EndingMode7DirectoryName));
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
            edits.Add(("palette-" + id, GameInstallationLayout.EndingPaletteDirectoryName));
        edits.Add(("palette-order", GameInstallationLayout.EndingPaletteDirectoryName));

        foreach ((string edit, string component) in edits)
        {
            IReadOnlyDictionary<string, string> changed = CreateEndingIdentityFixture(edit);
            AssertTrue(baseline[component] != changed[component], edit + " invalidates selected content");
            foreach (string other in baseline.Keys.Where(name => name != component))
                AssertEqual(baseline[other], changed[other], edit + " preserves unrelated " + other);
            var before = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, baseline);
            var after = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, changed);
            AssertTrue(before.CompositeSha256 != after.CompositeSha256, edit + " changes host identity");
            AssertTrue(before.GetCompatibilityWarnings(after.ToSnapshot(), "test").Single()
                .Contains(component, StringComparison.Ordinal), edit + " reports its specific domain");
        }
        foreach (string cosmeticEdit in new[] { "json-indent", "png-colors" })
        foreach ((string component, string digest) in baseline)
            AssertEqual(digest, CreateEndingIdentityFixture(cosmeticEdit)[component],
                cosmeticEdit + " preserves decoded " + component);

        // These identities are derived from existing installed fields. Persisting an extra cached
        // digest would needlessly change the exact debugger-object layout of older save states.
        foreach (Type type in new[]
        {
            typeof(EndingObjectArtworkCatalog), typeof(EndingMode7ArtworkCatalog), typeof(EndingPaletteCatalog),
            typeof(EndingCloudSpritePresentation), typeof(EndingExplosionSpritePresentation),
            typeof(EndingCompletionTextSpritePresentation), typeof(EndingRewardSpritePresentation),
            typeof(EndingLogoSpritePresentation),
        })
            AssertTrue(type.GetField("<ContentIdentity>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) is null,
                type.Name + " adds no serialized derived-identity field");
        Console.WriteLine($"  Ending content: {edits.Count} independent character/map/composition/palette edits, " +
            "ordered transfers and parts, canonical frames, encoding invariance and host warnings pass without a ROM.");
    }

    private static IReadOnlyDictionary<string, string> CreateEndingIdentityFixture(
        string? edit = null, bool reverse = false)
    {
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = edit == "json-indent",
            }));

        RoomCharacterAtlas Characters(string name, int bytes, int seed = 0)
        {
            int tiles = bytes / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int rows = (tiles + columns - 1) / columns;
            var pixels = new byte[columns * rows * 64];
            pixels[0] = (byte)seed;
            if (edit == name) pixels[^1] = 1;
            using var png = new MemoryStream();
            IndexedPng.Write(png, columns * 8, rows * 8, pixels, Enumerable.Range(0, 16)
                .Select(index => new Rgba32((byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return RoomCharacterAtlas.Load(png, bytes);
        }

        RoomBackgroundTilemapAtlas Map(string name)
        {
            RoomBackgroundTilemapCell Cell(bool last) => new()
            {
                TileColumn = last && (edit == name || name == "waiting-map" && edit == "map-column") ? 1 : 0,
                TileRow = last && name == "waiting-map" && edit == "map-row" ? 1 : 0,
                Palette = last && name == "waiting-map" && edit == "map-palette" ? 1 : 0,
                Priority = last && name == "waiting-map" && edit == "map-priority",
                FlipX = last && name == "waiting-map" && edit == "map-flip-x",
                FlipY = last && name == "waiting-map" && edit == "map-flip-y",
            };
            return RoomBackgroundTilemapAtlas.Load(Json(new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = [new RoomBackgroundTilemapPage
                {
                    Cells = Enumerable.Range(0, RoomBackgroundTilemapFormat.CellsPerPage)
                        .Select(index => Cell(index == RoomBackgroundTilemapFormat.CellsPerPage - 1)).ToArray(),
                }],
            }), RoomBackgroundTilemapFormat.BytesPerPage);
        }

        MemoryStream Sprites(string family, IEnumerable<string> names)
        {
            var first = new SpriteVisualPart
            {
                OffsetX = 0, OffsetY = 0, TileColumn = 0, TileRow = 0, Size = 8,
                Priority = 1, Palette = null, FlipX = false, FlipY = false,
            };
            var second = first with { OffsetX = 8, OffsetY = -8, TileColumn = 1, TileRow = 1, Palette = 3 };
            string[] keys = names.ToArray();
            var frames = new Dictionary<string, SpriteVisualPart[]>();
            foreach (string key in reverse ? keys.Reverse() : keys)
            {
                SpriteVisualPart a = first;
                if (key == keys[^1])
                {
                    if (edit == family + "-sprites") a = a with { OffsetX = 1 };
                    if (family == "cloud") a = a with
                    {
                        OffsetX = edit == "part-x" ? -1 : a.OffsetX,
                        OffsetY = edit == "part-y" ? -1 : a.OffsetY,
                        TileColumn = edit == "part-column" ? 1 : a.TileColumn,
                        TileRow = edit == "part-row" ? 1 : a.TileRow,
                        Size = edit == "part-size" ? 16 : a.Size,
                        Priority = edit == "part-priority" ? 2 : a.Priority,
                        // Explicit zero and inherited zero have identical packed attributes but
                        // behave differently when the drawing owner's palette changes.
                        Palette = edit == "part-palette" ? 0 : a.Palette,
                        FlipX = edit == "part-flip-x",
                        FlipY = edit == "part-flip-y",
                    };
                }
                SpriteVisualPart[] parts = [a, second];
                if (family == "cloud" && key == keys[^1])
                {
                    if (edit == "part-order") Array.Reverse(parts);
                    if (edit == "part-count") parts = [a];
                }
                frames.Add(key, parts);
            }
            return Json(new { Version = 1, Frames = frames });
        }

        var fragments = Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
            .Select(index => Characters("fragment-" + index, EndingObjectArtworkFormat.FragmentByteCount, index)).ToArray();
        if (edit == "fragment-order") Array.Reverse(fragments);
        var objects = new EndingObjectArtworkCatalog(
            Characters("clouds", EndingObjectArtworkFormat.CloudByteCount),
            Characters("explosion", EndingObjectArtworkFormat.ExplosionByteCount), fragments,
            Characters("waiting", EndingObjectArtworkFormat.RewardByteCount),
            Characters("shooting", EndingObjectArtworkFormat.RewardByteCount),
            Characters("suitless", EndingObjectArtworkFormat.RewardByteCount), Map("waiting-map"),
            Characters("post-fragment-a", EndingObjectArtworkFormat.PostCreditsFragmentAByteCount),
            Characters("post-fragment-b", EndingObjectArtworkFormat.PostCreditsFragmentBByteCount),
            Characters("logo-tiles", EndingObjectArtworkFormat.PostShotLogoTileByteCount), Map("logo-map"),
            EndingCloudSpritePresentation.Load(Sprites("cloud", EndingCloudSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            EndingExplosionSpritePresentation.Load(Sprites("explosion", EndingExplosionSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            EndingCompletionTextSpritePresentation.Load(Sprites("text", EndingCompletionTextSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            EndingRewardSpritePresentation.Load(Sprites("reward", EndingRewardSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))),
            EndingLogoSpritePresentation.Load(Sprites("logo", EndingLogoSpriteDefinitions.Frames.ToArray().Select(frame => frame.Name))));

        MemoryStream Mode7Png(string name, int seed)
        {
            var pixels = new byte[EndingMode7ArtworkFormat.CharacterByteCount];
            pixels[0] = (byte)seed;
            if (edit == name + "-pixels") pixels[^1] = 1;
            var png = new MemoryStream();
            IndexedPng.Write(png, EndingMode7ArtworkFormat.CharacterWidth, EndingMode7ArtworkFormat.CharacterHeight,
                pixels, Enumerable.Range(0, 256).Select(index => new Rgba32(
                    (byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }
        MemoryStream Mode7Map(string name, int height)
        {
            var tiles = new int[EndingMode7ArtworkFormat.MapWidth * height];
            if (edit == name + "-map") tiles[^1] = 1;
            return Json(new EndingMode7MapDocument
            {
                Version = EndingMode7ArtworkFormat.Version, Width = EndingMode7ArtworkFormat.MapWidth,
                Height = height, Tiles = tiles,
            });
        }
        EndingMode7SceneArtwork[] scenes = Enum.GetValues<EndingMode7SceneId>().Select(id =>
            EndingMode7SceneArtwork.Load(Mode7Map(id.ToString(), EndingMode7ArtworkFormat.MapHeight),
                Mode7Png(id.ToString(), (int)id))).ToArray();
        if (edit == "scene-order") Array.Reverse(scenes);
        var mode7 = new EndingMode7ArtworkCatalog(scenes[0], scenes[1], scenes[2],
            EndingRewardIconArtwork.Load(Mode7Map("reward", EndingRewardIconArtworkFormat.MapHeight), Mode7Png("reward", 0)));

        EndingPalette[] colors = Enum.GetValues<EndingPaletteId>().Select(id => EndingPalette.Load(Json(new EndingPaletteDocument
        {
            Version = EndingPaletteDefinitions.Version,
            Colors = Enumerable.Range(0, EndingPaletteDefinitions.ColorCount(id)).Select(index => new PaletteRgb5
            {
                Red = ((int)id + index) % 32,
                Green = index == EndingPaletteDefinitions.ColorCount(id) - 1 && edit == "palette-" + id ? 1 : 0,
                Blue = 0,
            }).ToArray(),
        }), id)).ToArray();
        if (edit == "palette-order") (colors[0], colors[1]) = (colors[1], colors[0]);
        var palettes = new EndingPaletteCatalog(colors[0], colors[1], colors[2], colors[3], colors[4], colors[5], colors[6]);
        return EndingPresentationIdentity.Create(mode7, objects, palettes);
    }
}
