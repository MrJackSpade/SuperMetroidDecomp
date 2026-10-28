using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Checks ending artwork role mapping and decoded-content identity behavior.</summary>
internal static class EndingContentIdentityVerification
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Checks ending scene selection and original import metadata without a playthrough.</summary>
    internal static void VerifyEndingMode7RoleSelection()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mode7 metadata oracle revision");
        int Source(int instruction)
        {
            AssertEqual(0xa9, (int)rom.ReadCartridgeByte(instruction), "native source bank LDA");
            AssertEqual(0xa9, (int)rom.ReadCartridgeByte(instruction + 5), "native source offset LDA");
            return (int)new SnesAddress(rom.ReadCartridgeByte(instruction + 2), rom.ReadCartridgeWord(instruction + 6));
        }
        foreach (var native in new[]
        {
            new VerifyEndingMode7RoleSelectionCase(EndingFloorSceneId.EscapeA, 0x8bd4b4, 0x8bd4d6, "escape-a"),
            new VerifyEndingMode7RoleSelectionCase(EndingFloorSceneId.EscapeB, 0x8bd775, 0x8bd604, "escape-b"),
            new VerifyEndingMode7RoleSelectionCase(EndingFloorSceneId.PlanetExplosion, 0x8bd87b, 0x8bd615, "planet-explosion"),
        })
        {
            var sources = EndingArtworkSources.Floor(native.First);
            AssertEqual(Source(native.Second), sources.Characters, "original high-lane character source");
            AssertEqual(Source(native.Third), sources.PackedMap, "original packed-map source");
            AssertEqual($"ending-{native.Fourth}-mode7-characters.png", EndingFloorArtworkFormat.CharacterFileName(native.First), "published character filename");
            AssertEqual($"ending-{native.Fourth}-mode7-map.json", EndingFloorArtworkFormat.MapFileName(native.First), "published map filename");
        }
        static MemoryStream Map(int height, int seed) => new(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
            new EndingFloorMapDocument { Version = 1, Width = 128, Height = height,
                Tiles = Enumerable.Repeat(seed, 128 * height).ToArray() }, ArtworkJson.Options));
        static MemoryStream Png(int seed)
        {
            var png = new MemoryStream();
            IndexedPng.Write(png, new(128, 128), Enumerable.Repeat((byte)seed, 128 * 128).ToArray(),
                Enumerable.Range(0, 256).Select(i => new RgbaPixel((byte)i, 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }
        var supplied = new EndingFloorSceneArtwork[3];
        for (int i = 0; i < supplied.Length; i++)
        {
            using var map = Map(64, i);
            using var png = Png(i);
            supplied[i] = EndingFloorSceneCharacters.Read(map, png);
        }
        using var rewardMap = Map(128, 3);
        using var rewardPng = Png(3);
        EndingRewardIconArtwork reward = EndingRewardIconCharacters.Read(rewardMap, rewardPng);
        var catalog = new EndingFloorCharacters(supplied[0], supplied[1], supplied[2], reward);
        EndingFloorSceneId[] originalOrder = [EndingFloorSceneId.EscapeA, EndingFloorSceneId.EscapeB, EndingFloorSceneId.PlanetExplosion];
        for (int i = 0; i < originalOrder.Length; i++)
            AssertTrue(ReferenceEquals(supplied[i], catalog[originalOrder[i]]), "ending Mode7 supplied scene identity");
        AssertTrue(ReferenceEquals(reward, catalog.RewardIcon), "ending Mode7 reward identity");
        string originalHash = SelectedPresentationHash.Create(nameof(EndingFloorArtworkCatalog), content =>
        {
            content.Append("scene-count", supplied.Length);
            foreach (var scene in supplied)
            {
                content.Append("map", scene.Map.Span);
                content.Append("characters", scene.Characters.Span);
            }
            content.Append("reward-icon", reward.Bytes.Span);
        });
        AssertEqual(originalHash, catalog.ContentIdentity, "ending Mode7 original hash ordering");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[(EndingFloorSceneId)invalid], "ending Mode7 role bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingArtworkSources.Floor((EndingFloorSceneId)invalid), "Mode7 source bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingFloorArtworkFormat.CharacterFileName((EndingFloorSceneId)invalid), "Mode7 character filename bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingFloorArtworkFormat.MapFileName((EndingFloorSceneId)invalid), "Mode7 map filename bounds");
        }
    }

    /// <summary>Checks domain-specific digest changes, canonical frame order, and encoding invariance.</summary>
    internal static void VerifyEndingContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateEndingIdentityFixture();
        IReadOnlyDictionary<string, string> reordered = CreateEndingIdentityFixture(reverse: true);
        foreach ((string component, string digest) in baseline)
            AssertEqual(digest, reordered[component], component + " canonical frame lookup order");

        var edits = new List<EndingContentIdentityEdit>();
        foreach (string edit in new[]
        {
            "clouds", "explosion", "waiting", "shooting", "suitless", "fragment-0", "fragment-1",
            "fragment-2", "fragment-3", "fragment-order", "post-fragment-a", "post-fragment-b",
            "logo-tiles", "logo-map", "cloud-sprites", "explosion-sprites", "text-sprites",
            "reward-sprites", "logo-sprites", "part-x", "part-y", "part-column", "part-row",
            "part-size", "part-priority", "part-palette", "part-flip-x", "part-flip-y",
            "part-order", "part-count", "map-column", "map-row", "map-palette", "map-priority",
            "map-flip-x", "map-flip-y",
        }) edits.Add(new EndingContentIdentityEdit(edit, GameInstallationLayout.EndingObjectDirectoryName));
        foreach (string edit in new[]
        {
            "EscapeA-map", "EscapeA-pixels", "EscapeB-map", "EscapeB-pixels",
            "PlanetExplosion-map", "PlanetExplosion-pixels", "scene-order", "reward-map", "reward-pixels",
        }) edits.Add(new EndingContentIdentityEdit(edit, GameInstallationLayout.EndingMode7DirectoryName));
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
            edits.Add(new EndingContentIdentityEdit("palette-" + id, GameInstallationLayout.EndingPaletteDirectoryName));
        edits.Add(new EndingContentIdentityEdit("palette-order", GameInstallationLayout.EndingPaletteDirectoryName));

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
            typeof(EndingObjectArtworkCatalog), typeof(EndingFloorArtworkCatalog), typeof(EndingPaletteCatalog),
            typeof(EndingCloudSpritePresentation), typeof(EndingExplosionSpritePresentation),
            typeof(EndingCompletionTextSpritePresentation), typeof(EndingRewardSpritePresentation),
            typeof(EndingLogoSpritePresentation),
        })
            AssertTrue(!MetadataShape.DeclaresField(type, "<ContentIdentity>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                type.Name + " adds no serialized derived-identity field");
        Console.WriteLine($"  Ending content: {edits.Count} independent character/map/composition/palette edits, " +
            "ordered transfers and parts, canonical frames, encoding invariance and host warnings pass without a ROM.");
    }

    /// <summary>Builds ending catalogs with one controlled edit across objects, Mode 7 scenes, or palettes.</summary>
    private static IReadOnlyDictionary<string, string> CreateEndingIdentityFixture(
        string? edit = null, bool reverse = false)
    {
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            (edit == "json-indent" ? CamelCaseIndentedJson : CamelCaseJson)));

        RoomCharacterAtlas Characters(string name, int bytes, int seed = 0)
        {
            int tiles = bytes / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int rows = (tiles + columns - 1) / columns;
            var pixels = new byte[columns * rows * 64];
            pixels[0] = (byte)seed;
            if (edit == name) pixels[^1] = 1;
            using var png = new MemoryStream();
            IndexedPng.Write(png, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))), pixels, Enumerable.Range(0, 16)
                .Select(index => new RgbaPixel((byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return RoomCharacterAtlasPng.Read(png, bytes);
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
        var objects = new EndingObjectArtworkCatalog(new EndingObjectArtworkParts { Clouds = Characters("clouds", EndingObjectArtworkFormat.CloudByteCount), Explosion = Characters("explosion", EndingObjectArtworkFormat.ExplosionByteCount), Fragments = fragments, WaitingSamus = Characters("waiting", EndingObjectArtworkFormat.RewardByteCount), ShootingScreen = Characters("shooting", EndingObjectArtworkFormat.RewardByteCount), SuitlessSamus = Characters("suitless", EndingObjectArtworkFormat.RewardByteCount), WaitingTilemap = Map("waiting-map"), PostCreditsFragmentA = Characters("post-fragment-a", EndingObjectArtworkFormat.PostCreditsFragmentAByteCount), PostCreditsFragmentB = Characters("post-fragment-b", EndingObjectArtworkFormat.PostCreditsFragmentBByteCount), PostShotLogoTiles = Characters("logo-tiles", EndingObjectArtworkFormat.PostShotLogoTileByteCount), PostShotLogoMap = Map("logo-map"), CloudSprites = EndingCloudSpritePresentation.Load(Sprites("cloud", EndingCloudSpriteDefinitions.Frames.Select(frame => frame.Name))), ExplosionSprites = EndingExplosionSpritePresentation.Load(Sprites("explosion", EndingExplosionSpriteDefinitions.Frames.Select(frame => frame.Name))), CompletionTextSprites = EndingCompletionTextSpritePresentation.Load(Sprites("text", EndingCompletionTextSpriteDefinitions.Frames.Select(frame => frame.Name))), RewardSprites = EndingRewardSpritePresentation.Load(Sprites("reward", EndingRewardSpriteDefinitions.Frames.Select(frame => frame.Name))), LogoSprites = EndingLogoSpritePresentation.Load(Sprites("logo", EndingLogoSpriteDefinitions.Frames.Select(frame => frame.Name))) });

        MemoryStream Mode7Png(string name, int seed)
        {
            var pixels = new byte[EndingFloorArtworkFormat.CharacterByteCount];
            pixels[0] = (byte)seed;
            if (edit == name + "-pixels") pixels[^1] = 1;
            var png = new MemoryStream();
            IndexedPng.Write(png, new(EndingFloorArtworkFormat.CharacterWidth, EndingFloorArtworkFormat.CharacterHeight),
                pixels, Enumerable.Range(0, 256).Select(index => new RgbaPixel(
                    (byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }
        MemoryStream FloorMap(string name, int height)
        {
            var tiles = new int[EndingFloorArtworkFormat.MapWidth * height];
            if (edit == name + "-map") tiles[^1] = 1;
            return Json(new EndingFloorMapDocument
            {
                Version = EndingFloorArtworkFormat.Version, Width = EndingFloorArtworkFormat.MapWidth,
                Height = height, Tiles = tiles,
            });
        }
        EndingFloorSceneArtwork[] scenes = Enum.GetValues<EndingFloorSceneId>().Select(id =>
            EndingFloorSceneCharacters.Read(FloorMap(id.ToString(), EndingFloorArtworkFormat.MapHeight),
                Mode7Png(id.ToString(), (int)id))).ToArray();
        if (edit == "scene-order") Array.Reverse(scenes);
        var mode7 = new EndingFloorCharacters(scenes[0], scenes[1], scenes[2],
            EndingRewardIconCharacters.Read(FloorMap("reward", EndingRewardIconArtworkFormat.MapHeight), Mode7Png("reward", 0)));

        EndingPalette[] colors = Enum.GetValues<EndingPaletteId>().Select(id => EndingPaletteFile.Read(Json(new EndingPaletteDocument
        {
            Version = EndingPaletteDefinitions.Version,
            Colors = Enumerable.Range(0, EndingPaletteDefinitions.ColorCount(id)).Select(index => new PaletteRgb5
            {
                Red = ((int)id + index) % 32,
                Green = index == EndingPaletteDefinitions.ColorCount(id) - 1 && edit == "palette-" + id ? 1 : 0,
                Blue = 0,
            }).ToArray(),
        }), id)).ToArray();
        if (edit == "palette-order") {
    var held0 = colors[1];
    var held1 = colors[0];
    colors[0] = held0;
    colors[1] = held1;
}
        var palettes = new EndingPaletteCatalog(colors[0], colors[1], colors[2], colors[3], colors[4], colors[5], colors[6]);
        return EndingPresentationIdentity.Create(mode7, objects, palettes);
    }

    /// <summary>Runs the native ending Mode 7 source and artwork-role check.</summary>
    [VerificationCommand("--lookup-ending-mode7-roles")]
    internal static int LookupEndingMode7RolesCommand()
    {
        Suite(nameof(VerifyEndingMode7RoleSelection), () => VerifyEndingMode7RoleSelection());
        Console.WriteLine("Ending Mode7 roles: six original sources, filenames, supplied references, identity ordering and bounds pass.");
        return 0;
    }

    /// <summary>Runs the focused ending presentation identity check without a ROM.</summary>
    [VerificationCommand("--ending-content-identity")]
    internal static int EndingContentIdentityCommand()
    {
        Suite(nameof(VerifyEndingContentIdentity), () => VerifyEndingContentIdentity());
        return 0;
    }
}
