using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Confirms #1636's room block sheets on the smallest synthetic fixtures, through the import's and the game's own code: the
/// import's inventory of a tileset holds the blocks its rooms place and the blocks a writer running in them writes, and no
/// other block the tileset loads; a block picture's part that shows nothing is not installed, by frame or by name, and the
/// room reads its absence as nothing there; and a room showing a block its sheet has no picture of fails, naming the sheet
/// and the block.
/// </summary>
internal static class RoomBlockInventoryVerification
{
    /// <summary>The block the fixture room places.</summary>
    private const int PlacedBlock = 0x040;

    /// <summary>A block the fixture tileset loads that no room places and nothing writes.</summary>
    private const int UnplacedBlock = 0x041;

    /// <summary>The block a writer running in the fixture room writes.</summary>
    private const int WrittenBlock = 0x052;

    /// <summary>The block a writer that does not run in the fixture room writes.</summary>
    private const int UnrunWrittenBlock = 0x060;

    /// <summary>
    /// A tileset whose one room places one block, with a writer running there that writes a second and a writer placed by no
    /// room of it writing a third: its inventory holds the placed and the written block alone, not the third nor a block it
    /// loads that nothing shows.
    /// </summary>
    internal static void VerifyInventoryHoldsPlacedAndWrittenBlocks()
    {
        TilesetDefinition tileset = default;
        var room = new InventoriedRoomState(tileset, AreaId.Crateria, new HashSet<int> { PlacedBlock },
            new HashSet<RoomObjectHeaderId>(), new HashSet<EnemyDefinitionId>(), RoomMainCallback.None, null);
        RoomBlockWriter[] writers =
        [
            new("writer running in every Zebes room", new HashSet<int> { WrittenBlock }, state => !state.InCeres),
            new("writer of a room object the room does not place", new HashSet<int> { UnrunWrittenBlock },
                state => state.Headers.Contains(RoomObjectHeaderId.MotherBrainGlass)),
        ];
        IReadOnlySet<int> shown = RoomBlockInventory.Of([room], writers).Shown(tileset);
        AssertTrue(shown.SetEquals([PlacedBlock, WrittenBlock]),
            $"the tileset's inventory holds the placed block {PlacedBlock:X3} and the written block {WrittenBlock:X3} alone, not {string.Join(", ", shown.Select(block => block.ToString("X3", CultureInfo.InvariantCulture)))}");
        AssertTrue(!shown.Contains(UnplacedBlock), $"block {UnplacedBlock:X3}, which no room places and nothing writes, is not inventoried");
        AssertTrue(!shown.Contains(UnrunWrittenBlock), $"block {UnrunWrittenBlock:X3}, written only by a room object the room does not place, is not inventoried");

        var ceres = room with { Area = AreaId.Ceres };
        AssertTrue(RoomBlockInventory.Of([ceres], writers).Shown(tileset).SetEquals([PlacedBlock]),
            "a Ceres room's inventory holds only what its level places, as no Zebes writer runs there");
        Console.WriteLine("Room block inventory: a tileset holds the blocks its rooms place and the blocks writers running in them write, and no other.");
    }

    /// <summary>
    /// Paints four blocks through the import's block painting and split sheet: one of back tiles only, one of front tiles
    /// only, one of both and one that inks nothing. The first installs no in-front part, the second no part behind, the
    /// third both parts, and the fourth is named once as showing nothing; no part that shows nothing is installed or named,
    /// and the room reads each absent part as nothing there.
    /// </summary>
    internal static void VerifyPartThatShowsNothingIsNotInstalled()
    {
        RoomBlockPictureTable table = RoomBlockFixtures.Pictures([
            .. Tiles(highPriority: false, false, false, false),
            .. Tiles(highPriority: true, true, true, true),
            .. Tiles(highPriority: false, true, false, true),
            .. Enumerable.Repeat(new BackgroundTile(ClearCharacter, 0, highPriority: false), 4),
        ], Characters(), Palette());
        SpriteSheet sheet = table.Sheet.Sheet;
        AssertParts(table, sheet, 0, behind: true, inFront: false);
        AssertParts(table, sheet, 1, behind: false, inFront: true);
        AssertParts(table, sheet, 2, behind: true, inFront: true);
        AssertParts(table, sheet, 3, behind: false, inFront: false);
        AssertTrue(sheet.blanks.SetEquals(["3"]), $"only the block that inks nothing is named as showing nothing, not {string.Join(", ", sheet.blanks)}");
        Console.WriteLine("Room block parts: a part that shows nothing is not installed, and the room reads it as nothing there.");
    }

    /// <summary>
    /// A room whose plane shows a block its sheet has no picture of, the import having found no room of the tileset that
    /// shows it, fails as the room resolves its pictures, naming the sheet and the block; the block the sheet pictures does
    /// not fail.
    /// </summary>
    internal static void VerifyUnpicturedBlockFailsLoudly()
    {
        RoomBlockPictureTable painted = RoomBlockFixtures.Pictures([.. Tiles(highPriority: false, false, false, false),
            .. Tiles(highPriority: false, false, false, false)], Characters(), Palette());
        var sheet = new RoomBlockSheet(default, painted.Sheet.Sheet, new RoomBlockAppearances([[], []], null, [0]));
        var resolver = new RoomBlockPictureResolver(sheet);
        var plane = new RoomLevelPlane(new Extent16<RoomBlocks>(2, 1), [RoomBlockAppearance.Of(0), RoomBlockAppearance.Of(1)]);
        InvalidDataException error = AssertThrows<InvalidDataException>(
            () => resolver.Resolve(plane, null, RoomBlockArtwork.Tileset, default, []),
            "a room showing a block its sheet does not picture fails");
        AssertTrue(error.Message.Contains(sheet.Sheet.Image.Name, StringComparison.Ordinal) && error.Message.Contains("block 001", StringComparison.Ordinal),
            $"the failure names the sheet {sheet.Sheet.Image.Name} and block 001: {error.Message}");
        AssertTrue(sheet.Inputs.Shows(0) && !sheet.Inputs.Shows(1), "the sheet's record shows the pictured block alone");
        Console.WriteLine("Unpictured block: a room showing a block its sheet does not picture fails, naming the sheet and the block.");
    }

    /// <summary>The character every pixel of which is the fixture palette's opaque colour.</summary>
    private const int InkedCharacter = 0;

    /// <summary>The character no pixel of which is inked.</summary>
    private const int ClearCharacter = 1;

    /// <summary>One block's four tiles of the inked character, top left, top right, bottom left, bottom right, each in front where its flag says so.</summary>
    private static BackgroundTile[] Tiles(bool highPriority, bool topRight, bool bottomLeft, bool bottomRight) =>
    [
        new(InkedCharacter, 0, highPriority),
        new(InkedCharacter, 0, topRight),
        new(InkedCharacter, 0, bottomLeft),
        new(InkedCharacter, 0, bottomRight),
    ];

    /// <summary>Two four-bit characters: the first inked in colour fifteen throughout, the second clear.</summary>
    private static byte[] Characters()
    {
        var characters = new byte[2 * RoomTileCanvas.CharacterBytes];
        characters.AsSpan(InkedCharacter * RoomTileCanvas.CharacterBytes, RoomTileCanvas.CharacterBytes).Fill(byte.MaxValue);
        return characters;
    }

    /// <summary>A palette whose first group's last colour, the inked character's, is white.</summary>
    private static RgbColor[] Palette()
    {
        var palette = new RgbColor[256];
        palette[15] = RgbColor.White;
        return palette;
    }

    /// <summary>Asserts which parts the fixture block <paramref name="block"/>'s picture installs, and that no part is named that shows nothing.</summary>
    private static void AssertParts(RoomBlockPictureTable table, SpriteSheet sheet, int block, bool behind, bool inFront)
    {
        string name = $"{block}";
        LayeredPicture picture = table.Picture(block) ?? throw new InvalidOperationException($"Verification failed: fixture block {block} has no picture.");
        AssertEqual(behind, picture.Behind is not null, $"block {block}'s picture has its part behind exactly when its back tiles ink a pixel");
        AssertEqual(inFront, picture.InFront is not null, $"block {block}'s picture has its part in front exactly when its front tiles ink a pixel");
        AssertEqual(behind, sheet.frames.ContainsKey(name), $"block {block}'s part behind is installed exactly when it shows something");
        AssertEqual(inFront, sheet.frames.ContainsKey(PictureParts.InFront(name)), $"block {block}'s part in front is installed exactly when it shows something");
        AssertTrue(!sheet.blanks.Contains(PictureParts.InFront(name)), $"block {block}'s part in front is never named as showing nothing");
    }

    /// <summary>Runs #1636's room block sheet confirmations.</summary>
    [VerificationCommand("--room-block-inventory")]
    internal static int RoomBlockInventoryCommand()
    {
        Suite(nameof(VerifyInventoryHoldsPlacedAndWrittenBlocks), VerifyInventoryHoldsPlacedAndWrittenBlocks);
        Suite(nameof(VerifyPartThatShowsNothingIsNotInstalled), VerifyPartThatShowsNothingIsNotInstalled);
        Suite(nameof(VerifyUnpicturedBlockFailsLoudly), VerifyUnpicturedBlockFailsLoudly);
        return 0;
    }
}
