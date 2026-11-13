using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms that the enemies' terrain lookups, which address blocks by position (#1628), find the blocks the original's
/// level-data places address (<see cref="EnemyTerrainFixtureRoom"/>): an extension link adds its offset, or its offset
/// times the room's width, to the place, an enemy mover's place is counted within 32768 blocks and reads the terrain
/// beyond the room past the grid, and an enemy projectile's block scan steps its place by a row or by a block. Each check
/// compares every input of its fixtures' domain with that place model, including the row ends, the room's edges and the
/// lookups that fail.
/// </summary>
internal static class EnemyTerrainBlockAddressingVerification
{
    /// <summary>The blocks an enemy mover's scan counts before it continues from the room's first block.</summary>
    private const int ScanCycleBlocks = 32768;

    /// <summary>Room sizes for the extension-link and projectile-scan fixtures: single rows and columns, odd sizes and a screen-wide room.</summary>
    private static readonly (int Width, int Height)[] RoomSizes = [(1, 1), (1, 4), (4, 1), (3, 2), (5, 4), (16, 3)];

    /// <summary>The fills of each fixture room the link checks compare.</summary>
    private const int LinkFills = 48;

    /// <summary>The fills of each fixture room the edge-scan check compares, each over every edge, start and span.</summary>
    private const int ScanFills = 6;

    /// <summary>
    /// Compares the block an enemy projectile's extension links lead to, from every start in and around each fixture
    /// room, with the level-data place walk: a horizontal link past a row's end continues on the next row, a link
    /// outside the grid finds no block, and a cyclic chain fails.
    /// </summary>
    internal static void VerifyEnemyCollisionBlockResolution()
    {
        foreach ((int width, int height) in RoomSizes)
        for (int fill = 0; fill < LinkFills; fill++)
        {
            var room = EnemyTerrainFixtureRoom.Filled(width, height, beyondCount: 0, fill);
            for (int y = 0; y <= height + 2; y++)
            for (int x = 0; x <= width + 2; x++)
            {
                var start = Position16<RoomBlocks>.Of(x, y);
                string Context() => $"enemy projectile extension links from {start} in fill {fill} of a {width}x{height} room";
                EnemyTerrainResolvedPlace expected = room.ResolveCollisionPlace(start);
                if (expected.Failure is { } failure)
                {
                    AssertFailure(failure, () => EnemyTerrain.ResolveEnemyCollisionBlock(room.Level, start), Context());
                    continue;
                }

                RoomCollisionBlock? resolved = EnemyTerrain.ResolveEnemyCollisionBlock(room.Level, start);
                RoomCollisionBlock? block = expected.Place is { } place ? room.Level.GetCollisionBlock(BlockAtPlace(place, width)) : null;
                if (resolved != block)
                    AssertEqual(block, resolved, Context());
            }
        }

        Console.WriteLine("Enemy collision block resolution: every start's extension links lead to the block the level-data place walk reaches.");
    }

    /// <summary>
    /// Compares the block an enemy mover's lookup reads, from every block its scan can name, with the level-data place
    /// walk: links counted within 32768 blocks, the terrain beyond the room past the grid, and the reads that fail
    /// there or before the room's first block. The reached block's column is the one the non-square-slope move tests
    /// against the enemy's center, and the reached block is where a spike reaction breaks.
    /// </summary>
    internal static void VerifyEnemyMoverBlockResolution()
    {
        foreach ((int width, int height, int beyondCount) in new[] { (3, 2, 7), (5, 4, 10), (16, 3, 20) })
        for (int fill = 0; fill < LinkFills; fill++)
        {
            var room = EnemyTerrainFixtureRoom.Filled(width, height, beyondCount, fill);
            for (int place = 0; place < width * height + beyondCount + 2 * width; place++)
                VerifyMoverPlace(room, place, $"fill {fill} of a {width}x{height} room");
        }

        // The widest room, tall enough that its grid runs past the cycle, so links near the cycle's end continue from the
        // room's first block.
        for (int fill = 0; fill < 2; fill++)
        {
            var wide = EnemyTerrainFixtureRoom.SingleLinks(255, 129, fill);
            for (int place = 0; place < ScanCycleBlocks; place++)
                VerifyMoverPlace(wide, place, $"fill {fill} of a 255x129 room");
        }

        Console.WriteLine("Enemy mover block resolution: every scanned block's links lead to the block the level-data place walk reaches.");
    }

    /// <summary>Compares the mover's read from the block at <paramref name="place"/> with the place walk.</summary>
    private static void VerifyMoverPlace(EnemyTerrainFixtureRoom room, int place, string roomName)
    {
        Position16<RoomBlocks> start = BlockAtPlace(place, room.Width);
        EnemyTerrainResolvedPlace expected = room.ResolveMoverPlace(place);
        if (expected.Failure is { } failure)
        {
            AssertFailure(failure, () => EnemyTerrain.ResolveEnemyMoverBlock(room.Level, start, out _), $"an enemy mover's read from {start} in {roomName}");
            return;
        }

        int reachedPlace = expected.Place ?? throw new InvalidOperationException("A mover's read always reaches a block.");
        RoomCollisionBlock block = EnemyTerrain.ResolveEnemyMoverBlock(room.Level, start, out Position16<RoomBlocks> reached);
        Position16<RoomBlocks> expectedBlock = BlockAtPlace(reachedPlace, room.Width);
        if (reached != expectedBlock || block.Terrain != room.TerrainAt(reachedPlace) || block.Behavior != room.BehaviorAt(reachedPlace))
        {
            AssertEqual(expectedBlock, reached, $"the block an enemy mover's read from {start} reaches in {roomName}");
            AssertEqual(room.TerrainAt(reachedPlace), block.Terrain, $"the terrain an enemy mover's read from {start} finds in {roomName}");
            AssertEqual(room.BehaviorAt(reachedPlace), block.Behavior, $"the behaviour an enemy mover's read from {start} finds in {roomName}");
        }
    }

    /// <summary>
    /// Compares an enemy projectile's block scan along each edge, for every leading edge, span start and span length in
    /// and around each fixture room, with the level-data place scan: a scan along a row past its end continues on the
    /// next row, a first block outside the room is a wall, and a scan that runs past the room's last block fails. Pixels
    /// past the room coordinates' 65535 wrap to the room's first blocks, and a span ending before its start counts on to
    /// 4095 more blocks.
    /// </summary>
    internal static void VerifyEnemyProjectileEdgeScan()
    {
        foreach ((int width, int height) in RoomSizes)
        for (int fill = 0; fill < ScanFills; fill++)
        {
            var room = EnemyTerrainFixtureRoom.Filled(width, height, beyondCount: 0, fill);
            int reach = width + height + 2;
            foreach (Axis edge in new[] { Axis.Horizontal, Axis.Vertical })
            for (int fixedBlock = 0; fixedBlock <= reach; fixedBlock++)
            for (int startBlock = 0; startBlock <= reach; startBlock++)
            for (int extraBlocks = 0; extraBlocks <= width * height + 1; extraBlocks++)
            {
                // Pixels inside each block, so the edge, the span's start and its end round down to the blocks named.
                VerifyEdgeScan(room, edge, fixedBlock * 16 + 5, startBlock * 16 + 9, startBlock * 16 + extraBlocks * 16 + 3, $"fill {fill} of a {width}x{height} room");
            }
        }

        var wrapped = EnemyTerrainFixtureRoom.Filled(5, 4, beyondCount: 0, fill: 3);
        foreach (Axis edge in new[] { Axis.Horizontal, Axis.Vertical })
        foreach (int fixedPixel in new[] { -1, -16, 65536 + 5, 3 })
        foreach ((int spanStart, int spanEnd) in new[] { (-3, 7), (65536 + 2, 65536 + 40), (8, 7), (16, 15) })
            VerifyEdgeScan(wrapped, edge, fixedPixel, spanStart, spanEnd, "fill 3 of a 5x4 room");

        Console.WriteLine("Enemy projectile edge scan: every edge's scan tests the blocks the level-data place scan reaches.");
    }

    /// <summary>Compares one edge scan with the place scan of the blocks its pixels name.</summary>
    private static void VerifyEdgeScan(EnemyTerrainFixtureRoom room, Axis edge, int fixedPixel, int spanStart, int spanEnd, string roomName)
    {
        int fixedBlock = CoordinateAxis.Wrap(fixedPixel) >> 4;
        int startBlock = CoordinateAxis.Wrap(spanStart) >> 4;
        int extraBlocks = CoordinateAxis.Wrap(spanEnd - (spanStart & ~15)) >> 4;
        string Context() => $"an enemy projectile's {edge} edge scan at pixel {fixedPixel} over pixels {spanStart}..{spanEnd} in {roomName}";
        EnemyTerrainScanOutcome expected = room.ScanEdge(fixedBlock, startBlock, extraBlocks, edge);
        if (expected.Failure is { } failure)
        {
            AssertFailure(failure, () => EyeDoorBehavior.EyeDoorEffectEdgeIsSolid(room.Level, fixedPixel, spanStart, spanEnd, edge), Context());
            return;
        }

        bool solid = EyeDoorBehavior.EyeDoorEffectEdgeIsSolid(room.Level, fixedPixel, spanStart, spanEnd, edge);
        if (solid != expected.Solid)
            AssertEqual(expected.Solid, solid, Context());
    }

    /// <summary>
    /// Confirms an enemy mover's spike reaction breaks the block its lookup reached past a row's end: a horizontal move
    /// whose leading edge lies past the room's width reads the block that many columns into the next row, and an
    /// enemy-breakable spike block there spawns its break at that block and becomes air.
    /// </summary>
    internal static void VerifyEnemySpikeBreakPastRowEnd()
    {
        var size = new Extent16<RoomBlocks>(4, 4);
        RoomTerrainBlock[] terrain = [.. Enumerable.Repeat(RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air), size.Area)];
        RoomBlockBehavior[] behaviors = [.. Enumerable.Repeat(RoomBlockBehavior.None, size.Area)];
        // The leading edge's column five of row one, in a room four blocks wide, is the block one column into row two.
        var broken = Position16<RoomBlocks>.Of(1, 2);
        terrain[size.RowMajorIndex(broken)] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpikeBlock);
        behaviors[size.RowMajorIndex(broken)] = RoomBlockBehavior.Reacting(BlockReaction.Common(EnemyBreakableTerrainDefinitions.BreakableReaction));
        var level = new RoomLevelData(size, new RoomLevelPlanes(terrain, behaviors,
            [.. Enumerable.Repeat(RoomBlockAppearance.Of(0), size.Area)]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        var mover = new EnemyTerrain();
        var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(84, 24)),
            Radius = new(4, 4),
        };
        bool collided;
        using (new EnemyTerrain.EnemyTerrainScope(mover, roomObjects.BreakableTerrain))
            collided = mover.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, SignedSixteenSixteen.FromWholePixels(8));

        AssertTrue(!collided, "the enemy breaks the spike block rather than meeting it");
        AssertTrue(roomObjects.PopulationSlots.Single() is { Block: var block } && block == broken,
            "the break is spawned at the block one column into the next row");
        AssertEqual(RoomCollisionType.Air, level.GetCollisionBlock(broken).CollisionType, "the broken block becomes air");
        Console.WriteLine("Enemy spike break past a row's end: the break is spawned at the block the mover's lookup reached on the next row.");
    }

    /// <summary>Fails unless <paramref name="lookup"/> fails with exactly <paramref name="failure"/>.</summary>
    private static void AssertFailure(Type failure, Func<object?> lookup, string context)
    {
        Exception thrown = AssertThrows<Exception>(lookup, context);
        if (thrown.GetType() != failure)
            AssertEqual(failure, thrown.GetType(), $"{context}: the failure, {thrown.Message}");
    }

    /// <summary>The block a level-data place addresses: the place's remainder by the width is its column, its quotient its row.</summary>
    private static Position16<RoomBlocks> BlockAtPlace(int place, int width) => Position16<RoomBlocks>.Of(place % width, place / width);

    /// <summary>Runs the focused enemy terrain block addressing checks.</summary>
    [VerificationCommand("--enemy-terrain-block-addressing")]
    internal static int EnemyTerrainBlockAddressingCommand()
    {
        VerifyEnemyCollisionBlockResolution();
        VerifyEnemyMoverBlockResolution();
        VerifyEnemyProjectileEdgeScan();
        VerifyEnemySpikeBreakPastRowEnd();
        return 0;
    }
}
