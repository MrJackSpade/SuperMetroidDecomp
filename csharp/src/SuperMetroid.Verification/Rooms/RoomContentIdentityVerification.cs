using SuperMetroid.Core.Assets;
using System.Text.Json;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Audio.AudioOverridesVerification;
using static SuperMetroid.Verification.Frontend.Recording.ControllerInputRecordingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks canonical content identities for selected room art and their compatibility warnings.</summary>
internal static class RoomContentIdentityVerification
{
    /// <summary>Direct selected-data checks: no cartridge, installation, or gameplay traversal.</summary>
    internal static void VerifyRoomContentIdentity()
    {
        RoomIdentityFixture baseline = CreateRoomIdentityFixture();
        IReadOnlyDictionary<string, string> hashes = baseline.Identity;
        RoomIdentityFixture reordered = CreateRoomIdentityFixture(reverse: true);
        AssertEqual(3, hashes.Count, "three independent room-art identities");
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
        // How a room's blocks look is their block sheet's identity (#1431), which covers the characters each block reads.
        AssertTrue(BlockSheetIdentity(firstCharacter: 0) != BlockSheetIdentity(firstCharacter: 1),
            "the characters a block reads are fingerprinted with its block sheet");

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
        Console.WriteLine("  Selected room content: three domains, CRE, block characters, canonical ordering, isolated edits, " +
            "aggregate drift, immutable snapshots and recording compatibility pass without a ROM.");
    }

    /// <summary>
    /// The content identity of a one-block sheet whose block reads the character run from <paramref name="firstCharacter"/>,
    /// pictured the same either way.
    /// </summary>
    private static string BlockSheetIdentity(int firstCharacter)
    {
        RoomBlockSheet pictured = RoomBlockFixtures.Pictures(new BackgroundTile[4], new byte[64], new RgbColor[256]).Sheet;
        var sheet = new RoomBlockSheet(default(TilesetDefinition), pictured.Sheet,
            new RoomBlockCharacters(default(TilesetDefinition).Blocks, [[new RoomBlockCharacterRun(firstCharacter, 1)]]));
        return new InstalledRoomArtwork(new Dictionary<TilesetDefinition, RoomBlockSheet> { [default] = sheet },
            new Dictionary<string, RoomBackgroundImage>(), new Dictionary<TilesetDefinition, TileWordSheet>(),
            new Dictionary<TilesetDefinition, TileWordSheet>(), new Dictionary<TilesetDefinition, MessageBoxSheet>(),
            new Dictionary<TilesetDefinition, FloorTileSheet>()).ContentIdentity;
    }

    /// <summary>Builds stock-shaped room catalogs with one optional visual edit and optional reversed insertion order.</summary>
    private static RoomIdentityFixture CreateRoomIdentityFixture(string? edited = null, bool reverse = false)
    {
        RoomCharacterAtlas Characters(bool change)
        {
            using var png = new MemoryStream();
            var pixels = new byte[64];
            pixels[0] = change ? (byte)1 : (byte)0;
            IndexedPng.Write(png, new(8, 8), pixels, [new RgbaPixel(0, 0, 0), new RgbaPixel(255, 255, 255)]);
            png.Position = 0;
            return RoomCharacterAtlasPng.Read(png, RoomCharacterAtlasFormat.BytesPerTile);
        }
        RoomStaticPalette Palette(bool change) => RoomStaticPaletteFile.Read(Json(new RoomStaticPaletteDocument
        {
            Version = RoomStaticPaletteFormat.Version,
            Colors = Enumerable.Range(0, RoomStaticPaletteFormat.ColorCount).Select(index =>
                new PaletteRgb5 { Red = change && index == 0 ? 1 : 0, Green = 0, Blue = 0 }).ToArray(),
        }));
        RoomCharacterAtlas character = Characters(edited == GameInstallationLayout.RoomCharacterDirectoryName);
        RoomStaticPalette palette = Palette(edited == GameInstallationLayout.RoomPaletteDirectoryName);
        var chars = new Dictionary<int, RoomCharacterAtlas>();
        var colors = new Dictionary<int, RoomStaticPalette>();
        IEnumerable<int> sets = Enumerable.Range(0, RoomTilesetDefinitions.Count);
        if (reverse) sets = sets.Reverse();
        foreach (int set in sets)
        {
            var definition = RoomTilesets.Get((byte)set);
            chars[definition.CharacterAddress] = character;
            colors[definition.PaletteAddress] = palette;
        }
        bool editLayout = edited == GameInstallationLayout.RoomVisualLayoutDirectoryName;
        RoomLevelId firstLevel = new("FixtureLevelA"), secondLevel = new("FixtureLevelB");
        var layouts = new Dictionary<RoomLevelId, RoomLevelArtwork>
        {
            [firstLevel] = new RoomLevelArtwork(firstLevel, new Extent16<RoomBlocks>(1, 2), [0, 1], [editLayout ? (ushort)2 : (ushort)0, 1]),
            [secondLevel] = new RoomLevelArtwork(secondLevel, new Extent16<RoomBlocks>(2, 1), [1, 0], [0, 1]),
        };
        if (reverse) layouts = layouts.Reverse().ToDictionary(pair => pair.Key, pair => pair.Value);
        return new RoomIdentityFixture(
            new RoomCharacterAtlases(Characters(edited == "cre-characters"), chars.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.CharacterSetAt(pair.Key), pair => pair.Value)),
            new RoomStaticPalettes(colors.ToDictionary(pair => SuperMetroid.Core.Rooms.RoomTilesetNative.PaletteSetAt(pair.Key), pair => pair.Value)),
            RoomLevelArtworks.FromLayoutsForVerification(layouts));

        static MemoryStream Json<T>(T document) => new(JsonSerializer.SerializeToUtf8Bytes(document));
    }

    /// <summary>Three independent room presentation catalogs used to measure selected-content identity.</summary>
    internal sealed record RoomIdentityFixture(RoomCharacterAtlases Characters,
        RoomStaticPalettes Palettes,
        RoomLevelArtworks Layouts)
    {
        /// <summary>Per-domain hashes for the fixture's selected room artwork.</summary>
        public IReadOnlyDictionary<string, string> Identity => RoomPresentationIdentity.Create(
            Characters, Palettes, Layouts);
    }

    /// <summary>Runs ROM-free room-art identity and aggregate compatibility checks.</summary>
    [VerificationCommand("--room-content-identity")]
    internal static int RoomContentIdentityCommand()
    {
        Suite(nameof(VerifyRoomContentIdentity), () => VerifyRoomContentIdentity());
        return 0;
    }
}
