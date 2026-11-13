using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks the X-ray reveal's base copy, extension blocks, screen edges, and area-specific overlays in whole blocks.</summary>
internal static class XrayTilemapVerification
{
    /// <summary>The fixture room's width and height in blocks.</summary>
    private const int RoomSide = 32;

    /// <summary>The first block the fixture's reveal pictures show, beyond every block the revealed area copies from the room.</summary>
    private const int RevealBlockBase = 900;

    /// <summary>
    /// Confirms the base copy and four-piece reveals place whole 16x16 blocks (#1431): each cell compares the block index
    /// and arrangement the reveal shows, where the 8x8 reveal compared the four tile words a block wrote.
    /// </summary>
    internal static void VerifyXrayTilemap()
    {
        // Every BG1 block shows its own index, mirrored each of the four ways in turn, so a copied cell names the one
        // location it was read from and how the level mirrors it.
        BlockMirroring[] mirrorings =
        [
            BlockMirroring.None, BlockMirroring.Horizontal, BlockMirroring.Vertical,
            BlockMirroring.Horizontal | BlockMirroring.Vertical,
        ];
        RevealedBlockArrangement[] arrangements =
        [
            RevealedBlockArrangement.AsDefined, RevealedBlockArrangement.MirroredHorizontally,
            RevealedBlockArrangement.MirroredVertically, RevealedBlockArrangement.MirroredBoth,
        ];
        RoomTerrainBlock[] foreground = [.. Enumerable.Range(0, RoomSide * RoomSide)
            .Select(block => RoomTerrainBlock.Of(block, mirrorings[block % mirrorings.Length], RoomCollisionType.SolidBlock))];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(RoomSide, RoomSide),
            new RoomLevelPlanes(foreground, new RoomBlockBehavior[RoomSide * RoomSide], new RoomBlockAppearance[RoomSide * RoomSide]), RoomLevelData.NothingBeyondGrid());
        RevealedBlock Copied(int x, int y)
        {
            int block = y * RoomSide + x;
            return new(block, arrangements[block % arrangements.Length]);
        }
        RevealedBlock Defined(int block) => new(block, RevealedBlockArrangement.AsDefined);
        // Each copy command shows its own four installed blocks, so a revealed cell names the command and the quarter
        // that wrote it (#1596).
        static XrayRevealVisualBlocks Shown(XrayRevealShape command)
        {
            int first = RevealBlockBase + (int)command * 4;
            return new(first, first + 1, first + 2, first + 3);
        }
        var visuals = XrayRevealFixtures.Complete(Shown);
        XrayRevealVisualBlocks squareBlocks = Shown(XrayRevealShape.CopySquare);
        // Layer one, and BG1 with it, half a block into the room's second block: the camera's block is block 33.
        Position16<RoomPixels> cameraPosition = new(24, 24);
        RevealedBlock?[] Build(AreaId area = AreaId.Brinstar) => XrayRevealMap.Build(level, cameraPosition, area, visuals);
        RevealedBlock? Cell(RevealedBlock?[] map, int column, int row) => map[row * XrayRevealArea.Columns + column];
        // Changes a block's collision, leaving the block BG1 shows, so a cell no reveal writes keeps its copy.
        void Set(int index, RoomCollisionType type, int behavior)
        {
            level.SetObjectTerrain(index, RoomTerrainBlock.Of(0, BlockMirroring.None, type), foreground[index].Appearance);
            level.SetObjectBehavior(index, RoomBlockBehaviorCodec.Decode(behavior));
        }
        RevealedBlock?[] basis = Build();
        AssertEqual(Copied(1, 1), Cell(basis, 0, 0), "base copy starts at the block holding the screen's top-left corner, mirrored as the level mirrors it");
        AssertEqual(Copied(2, 1), Cell(basis, 1, 0), "base copy's next column is the next block, mirrored its own way");
        AssertEqual(Copied(16, 1), Cell(basis, 15, 0), "base copy crosses BG1's 256-pixel screen boundary block for block");
        AssertEqual(Copied(17, 1), Cell(basis, 16, 0), "the seventeenth column copies the block a scrolled screen also shows");
        AssertEqual(Copied(1, 16), Cell(basis, 0, 15), "base copy continues past BG1's 256-pixel height into the room's next rows");
        AssertTrue(Enumerable.Range(0, XrayRevealArea.BlockCount).All(cell => basis[cell] ==
                Copied(1 + cell % XrayRevealArea.Columns, 1 + cell / XrayRevealArea.Columns)),
            "every cell copies the BG1 block at its scrolled position");
        // With layer one at the room's right edge the seventeenth column lies beyond the room, where the foreground shows nothing.
        cameraPosition = new(RoomSide * 16 - 256 + 8, 24);
        RevealedBlock?[] rightEdge = Build();
        AssertEqual(Copied(RoomSide - 1, 1), Cell(rightEdge, 15, 0), "the room's last column is copied");
        AssertEqual(null, Cell(rightEdge, 16, 0), "a cell beyond the room's right edge shows nothing");
        cameraPosition = new(24, 24);
        Set(33, RoomCollisionType.ShootableBlock, 3);
        Set(34, RoomCollisionType.HorizontalExtension, 0xff);
        Set(65, RoomCollisionType.VerticalExtension, 0xff);
        Set(66, RoomCollisionType.HorizontalExtension, 0xff);
        var square = Build();
        AssertEqual(Defined(squareBlocks.TopLeft), Cell(square, 0, 0), "square top left");
        AssertEqual(Defined(squareBlocks.TopRight), Cell(square, 1, 0), "square top right survives extension lookup");
        AssertEqual(Defined(squareBlocks.BottomLeft), Cell(square, 0, 1), "square lower left");
        AssertEqual(Defined(squareBlocks.BottomRight), Cell(square, 1, 1), "square lower right is placed whole");
        Set(32, RoomCollisionType.ShootableBlock, 3);
        Set(33, RoomCollisionType.HorizontalExtension, 0xff);
        var left = Build();
        AssertEqual(Defined(squareBlocks.TopRight), Cell(left, 0, 0), "offscreen predecessor contributes its right half at the viewport edge");
        AssertEqual(Defined(squareBlocks.BottomRight), Cell(left, 0, 1), "offscreen predecessor contributes its lower right half");
        Set(48, RoomCollisionType.ShootableBlock, 1);
        Set(49, RoomCollisionType.HorizontalExtension, 0xff);
        var edge = Build();
        AssertEqual(Defined(Shown(XrayRevealShape.CopyWide).TopLeft), Cell(edge, 15, 0), "last first-screen column receives wide reveal's left half");
        AssertEqual(Cell(left, 0, 1), Cell(edge, 0, 1), "last first-screen column does not write into the next row's first block");
        Set(40, RoomCollisionType.SpecialBlock, 0x82);
        AssertEqual(Defined(Shown(XrayRevealShape.CopyBrinstar).TopLeft), Cell(Build(), 7, 0), "Brinstar-only reveal is applied in Brinstar");
        AssertEqual(Cell(basis, 7, 0), Cell(Build(AreaId.Crateria), 7, 0), "Brinstar-only reveal preserves copied art elsewhere");
        // A square at the bottom row writes only its upper blocks: its lower ones lie below the revealed area.
        Set(33 + 15 * RoomSide, RoomCollisionType.ShootableBlock, 3);
        var bottom = Build();
        AssertEqual(Cell(edge, 16, 0), Cell(bottom, 16, 0), "a bottom-row square's lower blocks reach no other revealed block");
        AssertEqual(Defined(squareBlocks.TopLeft), Cell(bottom, 0, 15), "a bottom-row square's top left is placed at the bottom row");
        Console.WriteLine("  X-ray reveal: whole-block base copy across the foreground's screen edges, four-piece reveals, left predecessor, the revealed area's edges and area gate agree.");
    }
}
