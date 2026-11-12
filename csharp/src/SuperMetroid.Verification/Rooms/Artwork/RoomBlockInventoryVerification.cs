using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Confirms #1636's room block sheets on the smallest synthetic fixtures, through the import's and the game's own code: the
/// import's inventory of a tileset holds the blocks its rooms place and the blocks of each writer whose start a room state
/// of it holds in its installed data, and no other block the tileset loads; a block picture's part that shows nothing is not installed, by frame or by name, and the
/// room reads its absence as nothing there; and a room showing a block its sheet has no picture of fails, naming the sheet
/// and the block.
/// </summary>
internal static class RoomBlockInventoryVerification
{
    /// <summary>The block the fixture room places.</summary>
    private const int PlacedBlock = 0x040;

    /// <summary>A block the fixture tileset loads that no room places and nothing writes.</summary>
    private const int UnplacedBlock = 0x041;

    /// <summary>The block the bomb blocks' writer writes.</summary>
    private const int BombWrittenBlock = 0x058;

    /// <summary>The block the writer of Mother Brain's glass writes.</summary>
    private const int GlassWrittenBlock = 0x060;

    /// <summary>
    /// Two room states of two tilesets, each placing one block. The first's terrain holds a bomb block and its population
    /// places Mother Brain's glass; the second's terrain holds a bombable block selecting no bomb-block reaction, and it
    /// places nothing. The first tileset's inventory holds its placed block and both writers' blocks; the second's holds its
    /// placed block alone, as neither writer's start is in its installed data, and neither holds a block it loads that
    /// nothing shows. A Ceres state holding both starts shows only what its level places.
    /// </summary>
    internal static void VerifyWriterCountsOnlyWhereHeld()
    {
        TilesetDefinition holding = RoomTilesets.Get(0);
        TilesetDefinition lacking = RoomTilesets.Get(1);
        InventoriedRoomState holder = State(holding, AreaId.Crateria, new(RoomCollisionType.BombableBlock, BlockReaction.Common(0)),
            RoomObjectHeaderId.MotherBrainGlass);
        InventoriedRoomState other = State(lacking, AreaId.Crateria, new(RoomCollisionType.BombableBlock, BlockReaction.Common(8)));
        RoomBlockWriter[] writers =
        [
            new(new HashSet<int> { BombWrittenBlock }, state => state.HoldsBombBlocks),
            new(new HashSet<int> { GlassWrittenBlock }, state => state.Headers.Contains(RoomObjectHeaderId.MotherBrainGlass)),
        ];
        RoomBlockInventory inventory = RoomBlockInventory.Of([holder, other], writers);
        IReadOnlySet<int> held = inventory.Shown(holding);
        AssertTrue(held.SetEquals([PlacedBlock, BombWrittenBlock, GlassWrittenBlock]),
            $"the holding tileset's inventory holds its placed block and the blocks of the writers its state holds, not {Blocks(held)}");
        IReadOnlySet<int> lacked = inventory.Shown(lacking);
        AssertTrue(lacked.SetEquals([PlacedBlock]),
            $"the other tileset's inventory holds only its placed block, as its state holds neither writer's start, not {Blocks(lacked)}");
        AssertTrue(!held.Contains(UnplacedBlock) && !lacked.Contains(UnplacedBlock),
            $"block {UnplacedBlock:X3}, which no room places and nothing writes, is not inventoried");

        InventoriedRoomState ceres = holder with { Area = AreaId.Ceres };
        AssertTrue(RoomBlockInventory.Of([ceres], writers).Shown(holding).SetEquals([PlacedBlock]),
            "a Ceres room's inventory holds only what its level places, as nothing writes blocks there");

        // The elevator shaft's screen is its turning floor alone, so a state running its room main shows none of the blocks
        // its level places (#1647), though its tileset is still inventoried.
        InventoriedRoomState shaft = ceres with { MainCallback = RoomMainCallback.CeresElevatorShaft };
        AssertTrue(RoomBlockInventory.Of([shaft], writers).Shown(holding).Count == 0,
            "a state showing the elevator shaft's floor shows none of its level's blocks");
        Console.WriteLine("Room block inventory: a writer's blocks count for the room states whose installed data holds its start, and for no other.");
    }

    /// <summary>A Zebes or Ceres room state of <paramref name="tileset"/> placing <see cref="PlacedBlock"/>, its terrain one block of <paramref name="terrain"/>, placing <paramref name="headers"/>.</summary>
    private static InventoriedRoomState State(TilesetDefinition tileset, AreaId area, RoomTerrainReaction terrain, params RoomObjectHeaderId[] headers) =>
        new(tileset, area, new HashSet<int> { PlacedBlock }, new HashSet<RoomTerrainReaction> { terrain }, headers.ToHashSet(),
            new HashSet<EnemyDefinitionId>(), RoomSetupCallback.None, RoomMainCallback.None, new HashSet<DoorDefinition>(), null);

    /// <summary>Names <paramref name="blocks"/> for a failure message.</summary>
    private static string Blocks(IReadOnlySet<int> blocks) =>
        string.Join(", ", blocks.Select(block => block.ToString("X3", CultureInfo.InvariantCulture)));

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
        Suite(nameof(VerifyWriterCountsOnlyWhereHeld), VerifyWriterCountsOnlyWhereHeld);
        Suite(nameof(VerifyPartThatShowsNothingIsNotInstalled), VerifyPartThatShowsNothingIsNotInstalled);
        Suite(nameof(VerifyUnpicturedBlockFailsLoudly), VerifyUnpicturedBlockFailsLoudly);
        return 0;
    }
}
