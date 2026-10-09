using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Direct selected-data checks: no cartridge, installation, or gameplay traversal.</summary>
    private static void VerifyRoomContentIdentity()
    {
        RoomIdentityFixture baseline = CreateRoomIdentityFixture();
        IReadOnlyDictionary<string, string> hashes = baseline.Identity;
        RoomIdentityFixture reordered = CreateRoomIdentityFixture(reverse: true);
        AssertEqual(6, hashes.Count, "six independent room-art identities");
        foreach ((string name, string digest) in hashes)
            AssertEqual(digest, reordered.Identity[name], $"{name} identity ignores insertion order");

        foreach (string domain in hashes.Keys)
        {
            IReadOnlyDictionary<string, string> changed = CreateRoomIdentityFixture(domain).Identity;
            AssertTrue(hashes[domain] != changed[domain], $"{domain} selected edit changes its hash");
            foreach (string other in hashes.Keys.Where(name => name != domain))
                AssertEqual(hashes[other], changed[other], $"{domain} edit preserves unrelated {other}");
        }

        // CRE is not keyed by a graphics-set source; it must not escape the identity.
        AssertTrue(baseline.Characters.ContentIdentity != CreateRoomIdentityFixture("cre-characters").Characters.ContentIdentity,
            "shared CRE character edits are fingerprinted");
        AssertTrue(baseline.Metatiles.ContentIdentity != CreateRoomIdentityFixture("cre-blocks").Metatiles.ContentIdentity,
            "shared CRE metatile edits are fingerprinted");

        var callerOwned = new Dictionary<string, string>(hashes);
        Guid build = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        GameContentIdentity aggregate = GameContentIdentity.Create(new string('A', 64),
            new string('B', 64), new string('C', 64), build, callerOwned);
        string first = callerOwned.Keys.First();
        callerOwned[first] = new string('D', 64);
        AssertEqual(hashes[first], aggregate.AdditionalContentSha256[first], "aggregate owns an immutable selection snapshot");
        var changedIdentity = GameContentIdentity.Create(new string('A', 64),
            new string('B', 64), new string('C', 64), build, callerOwned);
        AssertTrue(aggregate.CompositeSha256 != changedIdentity.CompositeSha256,
            "room-art edit invalidates aggregate identity");
        AssertTrue(aggregate.GetCompatibilityWarnings(changedIdentity.ToSnapshot(), "test").Single()
            .Contains(first, StringComparison.Ordinal), "room-art drift is explained by its domain");
        Suite(nameof(VerifyGameContentIdentityComposition), () => VerifyGameContentIdentityComposition());
        Suite(nameof(VerifyControllerInputRecording), () => VerifyControllerInputRecording());
        Console.WriteLine("  Selected room content: six domains, CRE, canonical ordering, isolated edits, " +
            "aggregate drift, immutable snapshots and recording compatibility pass without a ROM.");
    }

    /// <summary>Builds small room-art catalogs for checking canonical, domain-isolated content hashes.</summary>
    /// <param name="edited">Optional domain name whose fixture data should differ from the baseline.</param>
    /// <param name="reverse">Whether catalog entries should be inserted in reverse order to test order independence.</param>
    /// <returns>Catalogs containing shared fixture assets and their computed content identities.</returns>
    private static RoomIdentityFixture CreateRoomIdentityFixture(string? edited = null, bool reverse = false)
    {
        RoomCharacterAtlas Characters(bool change)
        {
            using var png = new MemoryStream();
            var pixels = new byte[64];
            pixels[0] = change ? (byte)1 : (byte)0;
            IndexedPng.Write(png, 8, 8, pixels, [new Rgba32(0, 0, 0), new Rgba32(255, 255, 255)]);
            png.Position = 0;
            return RoomCharacterAtlas.Load(png, RoomCharacterAtlasFormat.BytesPerTile);
        }
        RoomStaticPalette Palette(bool change) => RoomStaticPalette.Load(Json(new RoomStaticPaletteDocument
        {
            Version = RoomStaticPaletteFormat.Version,
            Colors = Enumerable.Range(0, RoomStaticPaletteFormat.ColorCount).Select(index =>
                new PaletteRgb5 { Red = change && index == 0 ? 1 : 0, Green = 0, Blue = 0 }).ToArray(),
        }));
        RoomMetatileAtlas Blocks(bool change)
        {
            var cell = new RoomMetatileCell
            {
                TileColumn = 0, TileRow = 0, Palette = 0, Priority = change, FlipX = false, FlipY = false,
            };
            return RoomMetatileAtlas.Load(Json(new RoomMetatileDocument
            {
                Version = RoomMetatileFormat.Version,
                Blocks = [new RoomMetatileDefinition
                {
                    TopLeft = cell, TopRight = cell, BottomLeft = cell, BottomRight = cell,
                }],
            }), RoomMetatileFormat.BytesPerBlock);
        }
        RoomBackgroundTilemapAtlas Page(bool change) => RoomBackgroundTilemapAtlas.Load(
            Json(new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = [new RoomBackgroundTilemapPage
                {
                    Cells = Enumerable.Range(0, RoomBackgroundTilemapFormat.CellsPerPage).Select(index =>
                        new RoomBackgroundTilemapCell
                        {
                            TileColumn = 0, TileRow = 0, Palette = 0,
                            Priority = false, FlipX = change && index == 0, FlipY = false,
                        }).ToArray(),
                }],
            }), RoomBackgroundTilemapFormat.BytesPerPage);

        RoomCharacterAtlas character = Characters(edited == GameInstallationLayout.RoomCharacterDirectoryName);
        RoomStaticPalette palette = Palette(edited == GameInstallationLayout.RoomPaletteDirectoryName);
        RoomMetatileAtlas blocks = Blocks(edited == GameInstallationLayout.RoomMetatileDirectoryName);
        var chars = new Dictionary<int, RoomCharacterAtlas>();
        var colors = new Dictionary<int, RoomStaticPalette>();
        var metatiles = new Dictionary<int, RoomMetatileAtlas>();
        IEnumerable<int> sets = Enumerable.Range(0, RoomTilesetDefinitions.Count);
        if (reverse) sets = sets.Reverse();
        foreach (int set in sets)
        {
            var definition = RoomTilesetDefinitions.Get((byte)set);
            chars[definition.CharacterAddress] = character;
            colors[definition.PaletteAddress] = palette;
            metatiles[definition.BlockDefinitionsAddress] = blocks;
        }
        var backgrounds = new Dictionary<int, RoomBackgroundTilemapAtlas>();
        IEnumerable<int> pages = RoomBackgroundTilemapSources.All;
        if (reverse) pages = pages.Reverse();
        foreach (int page in pages)
            backgrounds[page] = Page(edited == GameInstallationLayout.RoomBackgroundTilemapDirectoryName &&
                page == RoomBackgroundTilemapSources.All[0]);
        RoomBackgroundTilemapAtlas[] skies = Enumerable.Range(0, RoomSkyTilemapFormat.PageCount)
            .Select(page => Page(edited == nameof(RoomSkyTilemapCatalog) && page == 0)).ToArray();
        bool editLayout = edited == GameInstallationLayout.RoomVisualLayoutDirectoryName;
        var layouts = new Dictionary<int, RoomVisualLayout>
        {
            [1] = new RoomVisualLayout(1, 1, 2, [0, 1], [editLayout ? (ushort)2 : (ushort)0, 1]),
            [2] = new RoomVisualLayout(2, 2, 1, [1, 0], [0, 1]),
        };
        if (reverse) layouts = layouts.Reverse().ToDictionary(pair => pair.Key, pair => pair.Value);
        return new RoomIdentityFixture(
            new RoomCharacterAtlasCatalog(Characters(edited == "cre-characters"), chars),
            new RoomStaticPaletteCatalog(colors),
            new RoomMetatileCatalog(Blocks(edited == "cre-blocks"), metatiles),
            new RoomBackgroundTilemapCatalog(backgrounds), new RoomSkyTilemapCatalog(skies),
            RoomVisualLayoutCatalog.FromLayoutsForVerification(layouts));

        static MemoryStream Json<T>(T document) => new(JsonSerializer.SerializeToUtf8Bytes(document));
    }

    /// <summary>Holds the six selected room-art catalogs used to verify independent content identities.</summary>
    /// <param name="Characters">Room character atlases, including the shared CRE character source.</param>
    /// <param name="Palettes">Static room palettes selected across the fixture's tilesets.</param>
    /// <param name="Metatiles">Metatile atlases, including the shared CRE block source.</param>
    /// <param name="Backgrounds">Background tilemap pages selected by their source addresses.</param>
    /// <param name="Skies">Sky tilemap pages available to room presentation.</param>
    /// <param name="Layouts">Visual layouts whose ordered layer data contributes to room identity.</param>
    private sealed record RoomIdentityFixture(RoomCharacterAtlasCatalog Characters,
        RoomStaticPaletteCatalog Palettes, RoomMetatileCatalog Metatiles,
        RoomBackgroundTilemapCatalog Backgrounds, RoomSkyTilemapCatalog Skies,
        RoomVisualLayoutCatalog Layouts)
    {
        /// <summary>Gets the canonical hash for each independently selected room-art domain.</summary>
        public IReadOnlyDictionary<string, string> Identity => RoomPresentationIdentity.Create(
            Characters, Palettes, Metatiles, Backgrounds, Skies, Layouts);
    }
}
