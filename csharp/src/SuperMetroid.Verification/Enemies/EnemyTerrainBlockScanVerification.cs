using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Checks which blocks the enemies' terrain movers read past a room's edges, through <see cref="BlockScanCycle"/> (#1593).</summary>
internal static class EnemyTerrainBlockScanVerification
{
    /// <summary>
    /// Confirms the movers read the blocks <see cref="BlockScanCycle"/> names: a horizontal move from the 257th row of a
    /// two-block-wide room meets the block at the top of its target column, while the same move from the second row passes;
    /// a vertical move into row 255 of a 200-block-wide room of 100 rows reads the block its scan reaches after counting
    /// past 32768 blocks, row 91 column 32; and extension links followed from that block lead on along the room's rows,
    /// one block forward for a horizontal link and one row back for a vertical one.
    /// </summary>
    internal static void VerifyEnemyTerrainBlockScan()
    {
        var terrain = new EnemyTerrain(breakableTerrain: null);
        SignedSixteenSixteen eightPixels = SignedSixteenSixteen.FromWholePixels(8);

        RoomLevelData tall = Room(2, 260, new EnemyTerrainScanBlock(1, 0, RoomCollisionType.SolidBlock, RoomBlockBehavior.None));
        RoomEnemySlot pastCycle = Enemy(8, 256 * 16 + 8);
        AssertTrue(terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(tall, pastCycle, eightPixels),
            "a horizontal move from the 257th row meets the block at the top of its target column");
        AssertEqual(12, pastCycle.Position.X.Whole, "the move stops against that block");
        RoomEnemySlot inCycle = Enemy(8, 24);
        AssertTrue(!terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(tall, inCycle, eightPixels),
            "the same move from the second row passes");
        AssertEqual(16, inCycle.Position.X.Whole, "the move from the second row completes");

        AssertVerticalMoveMeets(terrain, Room(200, 100, new EnemyTerrainScanBlock(32, 91, RoomCollisionType.SolidBlock, RoomBlockBehavior.None)),
            "a vertical move into row 255 reads row 91 column 32, counted past 32768 blocks");
        AssertVerticalMoveMeets(terrain, Room(200, 100,
                new(32, 91, RoomCollisionType.HorizontalExtension, RoomBlockBehavior.Extending(1)),
                new(33, 91, RoomCollisionType.SolidBlock, RoomBlockBehavior.None)),
            "a horizontal link from the counted block leads one block forward");
        AssertVerticalMoveMeets(terrain, Room(200, 100,
                new(32, 91, RoomCollisionType.VerticalExtension, RoomBlockBehavior.Extending(-1)),
                new(32, 90, RoomCollisionType.SolidBlock, RoomBlockBehavior.None)),
            "a vertical link from the counted block leads one row back");
        Console.WriteLine("Enemy terrain block scan: the movers read rows and blocks past the room's edges as the block scan cycle counts them.");
    }

    /// <summary>Moves an enemy in the room's first column down eight pixels into row 255 and confirms it collides there.</summary>
    private static void AssertVerticalMoveMeets(EnemyTerrain terrain, RoomLevelData level, string description)
    {
        RoomEnemySlot slot = Enemy(8, 255 * 16 - 4);
        AssertTrue(terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(8)), description);
        AssertEqual(255 * 16 - 4, slot.Position.Y.Whole, $"{description}: the move stops against that block");
    }

    /// <summary>An enemy eight pixels square centred on <paramref name="x"/>, <paramref name="y"/>.</summary>
    private static RoomEnemySlot Enemy(int x, int y) => new(0, EnemySlotFixtures.Registry)
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(x, y)),
        Radius = new(4, 4),
    };

    /// <summary>A room of air with the listed blocks, and nothing beyond its grid.</summary>
    private static RoomLevelData Room(int width, int height, params EnemyTerrainScanBlock[] blocks)
    {
        var size = new Extent16<RoomBlocks>(width, height);
        RoomTerrainBlock[] foreground = [.. Enumerable.Repeat(RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air), size.Area)];
        RoomBlockBehavior[] behaviors = [.. Enumerable.Repeat(RoomBlockBehavior.None, size.Area)];
        foreach (EnemyTerrainScanBlock block in blocks)
        {
            int index = block.Row * width + block.Column;
            foreground[index] = RoomTerrainBlock.Of(0, BlockMirroring.None, block.Collision);
            behaviors[index] = block.Behavior;
        }
        return new RoomLevelData(size, new RoomLevelPlanes(foreground, behaviors,
            [.. Enumerable.Repeat(RoomBlockAppearance.Of(0), size.Area)]), RoomLevelData.NothingBeyondGrid());
    }

    /// <summary>Runs the focused enemy terrain block scan check.</summary>
    [VerificationCommand("--enemy-terrain-block-scan")]
    internal static int EnemyTerrainBlockScanCommand()
    {
        VerifyEnemyTerrainBlockScan();
        return 0;
    }
}
