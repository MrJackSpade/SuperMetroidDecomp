using SuperMetroid.Core.Assets;
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

    /// <summary>
    /// Confirms the base copy and four-piece reveals place whole 16x16 blocks (#1431): each cell compares the block index
    /// and arrangement the reveal shows, where the 8x8 reveal compared the four tile words a block wrote.
    /// </summary>
    internal static void VerifyXrayTilemap()
    {
        // Every BG1 block shows its own index, mirrored each of the four ways in turn, so a copied cell names the one
        // location it was read from and how the level mirrors it.
        LevelBlockFlipFlags[] mirrorings =
        [
            LevelBlockFlipFlags.None, LevelBlockFlipFlags.Horizontal, LevelBlockFlipFlags.Vertical,
            LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
        ];
        RevealedBlockArrangement[] arrangements =
        [
            RevealedBlockArrangement.AsDefined, RevealedBlockArrangement.MirroredHorizontally,
            RevealedBlockArrangement.MirroredVertically, RevealedBlockArrangement.MirroredBoth,
        ];
        ushort[] foreground = [.. Enumerable.Range(0, RoomSide * RoomSide)
            .Select(block => RoomLevelWord.Create(block, mirrorings[block % mirrorings.Length], RoomCollisionType.SolidBlock).Raw)];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(RoomSide, RoomSide),
            new RoomLevelPlanes(foreground, new byte[RoomSide * RoomSide], new ushort[RoomSide * RoomSide]));
        RevealedBlock Copied(int x, int y)
        {
            int block = y * RoomSide + x;
            return new(block, arrangements[block % arrangements.Length]);
        }
        RevealedBlock Defined(int block) => new(block, RevealedBlockArrangement.AsDefined);
        // Layer one, and BG1 with it, half a block into the room's second block: the camera's block is block 33.
        Position16<RoomPixels> layer1 = new(24, 24);
        RevealedBlock?[] Build(AreaId area = AreaId.Brinstar) => XrayRevealMap.Build(level, layer1, default, area);
        RevealedBlock? Cell(RevealedBlock?[] map, int column, int row) => map[row * XrayRevealLayout.MetatileColumns + column];
        // Changes a block's collision, leaving the block BG1 shows, so a cell no reveal writes keeps its copy.
        void Set(int index, RoomCollisionType type, byte bts)
        {
            level.SetPlmForegroundEntry(index, (int)type << 12, new RoomLevelWord(foreground[index]).VisualWord);
            level.SetPlmBehavior(index, bts);
        }
        RevealedBlock?[] basis = Build();
        AssertEqual(Copied(1, 1), Cell(basis, 0, 0), "base copy starts at the block holding the screen's top-left corner, mirrored as the level mirrors it");
        AssertEqual(Copied(2, 1), Cell(basis, 1, 0), "base copy's next column is the next block, mirrored its own way");
        AssertEqual(Copied(16, 1), Cell(basis, 15, 0), "base copy crosses BG1's 256-pixel screen boundary block for block");
        AssertEqual(Copied(17, 1), Cell(basis, 16, 0), "the fine-scroll column copies the block past the first screen");
        AssertEqual(Copied(1, 16), Cell(basis, 0, 15), "base copy continues past BG1's 256-pixel height into the room's next rows");
        AssertTrue(Enumerable.Range(0, XrayRevealLayout.Cells).All(cell => basis[cell] ==
                Copied(1 + cell % XrayRevealLayout.MetatileColumns, 1 + cell / XrayRevealLayout.MetatileColumns)),
            "every cell copies the BG1 block at its scrolled position");
        // With layer one at the room's right edge the fine-scroll column lies beyond the room, where BG1 shows nothing.
        layer1 = new(RoomSide * 16 - 256 + 8, 24);
        RevealedBlock?[] rightEdge = Build();
        AssertEqual(Copied(RoomSide - 1, 1), Cell(rightEdge, 15, 0), "the room's last column is copied");
        AssertEqual(null, Cell(rightEdge, 16, 0), "a cell beyond the room's right edge shows nothing");
        layer1 = new(24, 24);
        Set(33, RoomCollisionType.ShootableBlock, 3);
        Set(34, RoomCollisionType.HorizontalExtension, 0xff);
        Set(65, RoomCollisionType.VerticalExtension, 0xff);
        Set(66, RoomCollisionType.HorizontalExtension, 0xff);
        var square = Build();
        AssertEqual(Defined(0x99), Cell(square, 0, 0), "square top left");
        AssertEqual(Defined(0x9a), Cell(square, 1, 0), "square top right survives extension lookup");
        AssertEqual(Defined(0xb9), Cell(square, 0, 1), "square lower left");
        AssertEqual(Defined(0xba), Cell(square, 1, 1), "square lower right is placed whole");
        Set(32, RoomCollisionType.ShootableBlock, 3);
        Set(33, RoomCollisionType.HorizontalExtension, 0xff);
        var left = Build();
        AssertEqual(Defined(0x9a), Cell(left, 0, 0), "offscreen predecessor contributes its right half at the viewport edge");
        AssertEqual(Defined(0xba), Cell(left, 0, 1), "offscreen predecessor contributes its lower right half");
        Set(48, RoomCollisionType.ShootableBlock, 1);
        Set(49, RoomCollisionType.HorizontalExtension, 0xff);
        var edge = Build();
        AssertEqual(Defined(0x96), Cell(edge, 15, 0), "last first-screen column receives wide reveal's left half");
        AssertEqual(Cell(basis, 16, 0), Cell(edge, 16, 0), "last first-screen column does not write into the second screen");
        AssertEqual(Cell(left, 0, 1), Cell(edge, 0, 1), "last first-screen column does not write into the next row's first block");
        Set(40, RoomCollisionType.SpecialBlock, 0x82);
        AssertEqual(Defined(0xb6), Cell(Build(), 7, 0), "Brinstar-only reveal is applied in Brinstar");
        AssertEqual(Cell(basis, 7, 0), Cell(Build(AreaId.Crateria), 7, 0), "Brinstar-only reveal preserves copied art elsewhere");
        // A square at the bottom row writes its lower blocks past the first screen: the first column's lands in the
        // second screen's first row, which shows as the fine-scroll column's top cell; the others are lost.
        Set(33 + 15 * RoomSide, RoomCollisionType.ShootableBlock, 3);
        var bottom = Build();
        AssertEqual(Defined(0xb9), Cell(bottom, 16, 0), "a bottom-row square's lower left lands in the second screen's top row");
        AssertEqual(Defined(0x99), Cell(bottom, 0, 15), "a bottom-row square's top left is placed at the bottom row");
        Console.WriteLine("  X-ray tilemap: whole-block base copy across BG1's screen edges, four-piece reveals, left predecessor, right-edge suppression, bottom-row spill and area gate agree.");
    }
}
