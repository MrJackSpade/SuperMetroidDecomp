using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Ending;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

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
        EndingIdentityFixture baselineFixture = CreateEndingIdentityFixture();
        EndingIdentityFixture reorderedFixture = CreateEndingIdentityFixture(reverse: true);
        IReadOnlyDictionary<string, string> baseline = baselineFixture.Installed;
        AssertEqual(baselineFixture.Objects, reorderedFixture.Objects, "ending objects canonical frame lookup order");
        foreach ((string component, string digest) in baseline)
            AssertEqual(digest, reorderedFixture.Installed[component], component + " canonical frame lookup order");

        // The ending's maps and compositions are import data (#1638): each edit changes the identity of the objects the
        // ending's sheets are composed from, which the sheets carry, and none of the installed content's identity.
        string[] objectEdits =
        [
            "fragment-0", "fragment-1",
            "fragment-2", "fragment-3", "fragment-order", "post-fragment-b",
            "logo-map", "cloud-sprites", "explosion-sprites", "text-sprites",
            "reward-sprites", "logo-sprites", "part-x", "part-y", "part-column", "part-row",
            "part-size", "part-priority", "part-palette", "part-flip-x", "part-flip-y",
            "part-order", "part-count", "map-column", "map-row", "map-palette", "map-priority",
            "map-flip-x", "map-flip-y",
        ];
        foreach (string edit in objectEdits)
        {
            EndingIdentityFixture changed = CreateEndingIdentityFixture(edit);
            AssertTrue(baselineFixture.Objects != changed.Objects, edit + " invalidates the ending objects' identity");
            foreach ((string component, string digest) in baseline)
                AssertEqual(digest, changed.Installed[component], edit + " preserves installed " + component);
        }

        var edits = new List<EndingContentIdentityEdit>();
        foreach (string edit in BackdropScenes)
            edits.Add(new EndingContentIdentityEdit("backdrop-" + edit, GameInstallationLayout.EndingBackdropDirectoryName));
        edits.Add(new EndingContentIdentityEdit("backdrop-order", GameInstallationLayout.EndingBackdropDirectoryName));
        edits.Add(new EndingContentIdentityEdit("backdrop-space", GameInstallationLayout.EndingBackdropDirectoryName));
        edits.Add(new EndingContentIdentityEdit("backdrop-space-order", GameInstallationLayout.EndingBackdropDirectoryName));

        foreach ((string edit, string component) in edits)
        {
            IReadOnlyDictionary<string, string> changed = CreateEndingIdentityFixture(edit).Installed;
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
        foreach (string cosmeticEdit in new[] { "json-indent" })
        {
            AssertEqual(baselineFixture.Objects, CreateEndingIdentityFixture(cosmeticEdit).Objects, cosmeticEdit + " preserves decoded ending objects");
            foreach ((string component, string digest) in baseline)
                AssertEqual(digest, CreateEndingIdentityFixture(cosmeticEdit).Installed[component],
                    cosmeticEdit + " preserves decoded " + component);
        }

        // These identities are derived from existing installed fields. Persisting an extra cached
        // digest would needlessly change the exact debugger-object layout of older save states.
        foreach (Type type in new[]
        {
            typeof(EndingObjectSourceArtwork), typeof(EndingFloorCharacters), typeof(EndingBackdrops),
            typeof(EndingCloudSpritePresentation), typeof(EndingExplosionSpritePresentation),
            typeof(EndingCompletionTextSpritePresentation), typeof(EndingRewardSpritePresentation),
            typeof(EndingLogoSpritePresentation),
        })
            AssertTrue(!MetadataShape.DeclaresField(type, "<ContentIdentity>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                type.Name + " adds no serialized derived-identity field");
        Console.WriteLine($"  Ending content: {objectEdits.Length} map/composition edits and {edits.Count} backdrop edits, " +
            "ordered transfers and parts, canonical frames, encoding invariance and host warnings pass without a ROM.");
    }

    /// <summary>
    /// The identities of a synthetic ending with one controlled edit: of its object maps and compositions, which are import
    /// data, and of the installed content the host identity covers.
    /// </summary>
    /// <param name="Objects">The object maps' and compositions' identity.</param>
    /// <param name="Installed">The installed domains' identities.</param>
    private sealed record EndingIdentityFixture(string Objects, IReadOnlyDictionary<string, string> Installed);

    /// <summary>Builds ending catalogs with one controlled edit across objects, Mode 7 scenes, or palettes.</summary>
    private static EndingIdentityFixture CreateEndingIdentityFixture(
        string? edit = null, bool reverse = false)
    {
        MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            (edit == "json-indent" ? CamelCaseIndentedJson : CamelCaseJson)));


        RoomBackgroundTilemapAtlas Map(string name, int seed = 0)
        {
            RoomBackgroundTilemapCell Cell(int index)
            {
                bool last = index == RoomBackgroundTilemapFormat.CellsPerPage - 1;
                return new()
                {
                    TileColumn = last && (edit == name || name == "waiting-map" && edit == "map-column") ? 1 : index == 0 ? seed : 0,
                    TileRow = last && name == "waiting-map" && edit == "map-row" ? 1 : 0,
                    Palette = last && name == "waiting-map" && edit == "map-palette" ? 1 : 0,
                    Priority = last && name == "waiting-map" && edit == "map-priority",
                    FlipX = last && name == "waiting-map" && edit == "map-flip-x",
                    FlipY = last && name == "waiting-map" && edit == "map-flip-y",
                };
            }
            return RoomBackgroundTilemapAtlas.Load(Json(new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = [new RoomBackgroundTilemapPage
                {
                    Cells = Enumerable.Range(0, RoomBackgroundTilemapFormat.CellsPerPage)
                        .Select(Cell).ToArray(),
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
            .Select(index => Map("fragment-" + index, index)).ToArray();
        if (edit == "fragment-order") Array.Reverse(fragments);
        var objects = new EndingObjectSourceArtwork(new EndingObjectArtworkParts { Fragments = fragments, WaitingTilemap = Map("waiting-map"), PostCreditsFragmentB = Map("post-fragment-b"), PostShotLogoMap = Map("logo-map"), CloudSprites = EndingCloudSpritePresentation.Load(Sprites("cloud", EndingCloudSpriteDefinitions.Frames.Select(frame => frame.Name))), ExplosionSprites = EndingExplosionSpritePresentation.Load(Sprites("explosion", EndingExplosionSpriteDefinitions.Frames.Select(frame => frame.Name))), CompletionTextSprites = EndingCompletionTextSpritePresentation.Load(Sprites("text", EndingCompletionTextSpriteDefinitions.Frames.Select(frame => frame.Name))), RewardSprites = EndingRewardSpritePresentation.Load(Sprites("reward", EndingRewardSpriteDefinitions.Frames.Select(frame => frame.Name))), LogoSprites = EndingLogoSpritePresentation.Load(Sprites("logo", EndingLogoSpriteDefinitions.Frames.Select(frame => frame.Name))) });

        RgbColor[] backdrops = [.. BackdropScenes.Select((scene, index) => new FadeLevelColour(index + 1, edit == "backdrop-" + scene ? 1 : 0, 0).ToColour())];
        if (edit == "backdrop-order")
            backdrops = [backdrops[1], backdrops[0], .. backdrops[2..]];
        // The space's backdrops are distinct greys by frame, so swapping two frames changes what each frame shows.
        RgbColor[] space = [.. Enumerable.Range(0, EndingSpaceBackdropColours.FrameCount)
            .Select(frame => new FadeLevelColour(frame, frame, edit == "backdrop-space" && frame == 0 ? 1 : frame).ToColour())];
        if (edit == "backdrop-space-order")
            space = [space[1], space[0], .. space[2..]];
        return new EndingIdentityFixture(objects.ContentIdentity, EndingPresentationIdentity.Create(new EndingBackdrops(backdrops[0], backdrops[1],
            backdrops[2], backdrops[3], EndingSpaceBackdropColours.FromFrameOrder(space))));
    }

    /// <summary>The scenes whose backdrops the identity fixture edits, in the backdrops' order.</summary>
    private static readonly string[] BackdropScenes = ["Escape", "Explosion", "Credits", "PostCredits"];

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
