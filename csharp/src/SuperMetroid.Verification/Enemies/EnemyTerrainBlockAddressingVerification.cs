using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.Projectiles;
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
    /// Compares the blocks an enemy projectile's block scan names (<see cref="EnemyProjectileBlockScan"/>) with the
    /// original's arithmetic, for every first row 0 through 1023 and 4095 (an edge above the room's top), first columns at,
    /// before and past the room's width and 4095, and steps across a row's end and past the 32768-block cycle: the first
    /// row's low eight bits times the width plus the column, stepped by the width down a column or by one block along a
    /// row, counted within 32768 blocks. The scan's first block and count are compared, for every pixel within each
    /// block and spans of every length up to two blocks either side of a block boundary, with the sixteen-bit span count.
    /// </summary>
    internal static void VerifyEnemyProjectileScanBlocks()
    {
        int[] widths = [1, 2, 3, 16, 17, 129, 200, 255];
        int[] steps = [0, 1, 2, 3, 15, 16, 17, 255, 4095];
        foreach (int width in widths)
        {
            int[] columns = [0, 1, width - 1, width, width + 1, 2 * width, 255, 4095];
            foreach (int row in Enumerable.Range(0, ScannedFirstRows).Append(4095))
            foreach (int column in columns)
            foreach (Axis edge in new[] { Axis.Horizontal, Axis.Vertical })
            {
                var scan = new EnemyProjectileBlockScan(Position16<RoomBlocks>.Of(column, row), edge, ExtraBlocks: 0);
                int first = (row & 0xff) * width + column;
                foreach (int step in steps)
                {
                    int place = (first + step * (edge == Axis.Vertical ? 1 : width)) % ScanCycleBlocks;
                    Position16<RoomBlocks> expected = BlockAtPlace(place, width);
                    Position16<RoomBlocks> scanned = scan.Block(step, width);
                    if (scanned != expected)
                        AssertEqual(expected, scanned, $"the {edge} scan's block {step} from ({column},{row}) in a room {width} blocks wide");
                }
            }
        }

        foreach (Axis edge in new[] { Axis.Horizontal, Axis.Vertical })
        foreach (int edgePixel in new[] { 0, 5, 15, 16, 4095, 16383, 65535, -1, 65536 + 17 })
        for (int spanStart = -40; spanStart <= 40; spanStart++)
        for (int spanLength = -33; spanLength <= 33; spanLength++)
        {
            int spanEnd = spanStart + spanLength;
            foreach (int shift in new[] { 0, 16368, 65536 - 48 })
            {
                var scan = EnemyProjectileBlockScan.Across(edgePixel, spanStart + shift, spanEnd + shift, edge);
                int edgeLine = (edgePixel & 0xffff) >> 4;
                int startLine = ((spanStart + shift) & 0xffff) >> 4;
                int extraBlocks = (((spanEnd + shift) & 0xffff) - (((spanStart + shift) & 0xffff) & 0xfff0) & 0xffff) >> 4;
                var first = edge == Axis.Vertical ? Position16<RoomBlocks>.Of(startLine, edgeLine) : Position16<RoomBlocks>.Of(edgeLine, startLine);
                if (scan != new EnemyProjectileBlockScan(first, edge, extraBlocks))
                    AssertEqual(new EnemyProjectileBlockScan(first, edge, extraBlocks), scan,
                        $"the {edge} scan across edge pixel {edgePixel} over pixels {spanStart + shift}..{spanEnd + shift}");
            }
        }

        Console.WriteLine("Enemy projectile scan blocks: every first row, column and step names the block the original's place arithmetic does.");
    }

    /// <summary>
    /// Compares both enemy projectile block scans, the eye door projectiles' edge scan and the shared axis move, for every
    /// first row 0 through 1023 and every first column up to one past the room's width, with the original's place scan:
    /// the first row's low eight bits times the width plus the column, so a row past the 256th scans the rows from the
    /// room's top again, and a room wider than 128 blocks carries rows near the cycle's end past the 32768-block cycle
    /// into its first rows. The scans span one, two and three blocks.
    /// </summary>
    internal static void VerifyEnemyProjectileScanRows()
    {
        foreach ((int width, int height) in new[] { (5, 4), (16, 3), (200, 4) })
        for (int fill = 0; fill < 2; fill++)
        {
            ScanRowsRoom room = ScanRowsRoom.Filled(width, height, fill);
            for (int row = 0; row < ScannedFirstRows; row++)
            for (int column = 0; column <= width + 1; column++)
            for (int extraBlocks = 0; extraBlocks <= 2; extraBlocks++)
            foreach (Axis edge in new[] { Axis.Horizontal, Axis.Vertical })
            {
                string Context(string scan) => $"the {scan} {edge} scan from column {column}, row {row} over {extraBlocks + 1} blocks in fill {fill} of a {width}x{height} room";
                int edgeBlock = edge == Axis.Vertical ? row : column;
                int spanBlock = edge == Axis.Vertical ? column : row;
                EnemyTerrainScanOutcome expected = room.Scan(edgeBlock, spanBlock, extraBlocks, edge, EyeDoorPassable);
                if (expected.Failure is { } failure)
                {
                    AssertFailure(failure, () => EyeDoorBehavior.EyeDoorEffectEdgeIsSolid(room.Level, edgeBlock * 16 + 5, spanBlock * 16 + 2, spanBlock * 16 + extraBlocks * 16 + 9, edge), Context("eye door"));
                }
                else
                {
                    bool solid = EyeDoorBehavior.EyeDoorEffectEdgeIsSolid(room.Level, edgeBlock * 16 + 5, spanBlock * 16 + 2, spanBlock * 16 + extraBlocks * 16 + 9, edge);
                    if (solid != expected.Solid)
                        AssertEqual(expected.Solid, solid, Context("eye door"));
                }

                EnemyTerrainScanOutcome moved = room.Scan(edgeBlock, spanBlock, extraBlocks, edge, AxisMovePassable, laterPlacesAreWalls: true);
                if (moved.Failure is { } moveFailure)
                {
                    AssertFailure(moveFailure, () => MoveScannedProjectile(room.Level, edge, edgeBlock, spanBlock, extraBlocks), Context("axis move"));
                    continue;
                }
                bool collided = MoveScannedProjectile(room.Level, edge, edgeBlock, spanBlock, extraBlocks);
                if (collided != moved.Solid)
                    AssertEqual(moved.Solid, collided, Context("axis move"));
            }
        }

        Console.WriteLine("Enemy projectile scan rows: both block scans test the blocks the original's place scan reaches from every first row.");
    }

    /// <summary>The first rows the scan checks compare: four cycles of 256.</summary>
    private const int ScannedFirstRows = 1024;

    /// <summary>The kinds the eye door projectiles' edge scan passes through, as its scan tests them.</summary>
    private static bool EyeDoorPassable(RoomCollisionType kind) => kind is
        RoomCollisionType.Air or RoomCollisionType.SpikeAir or RoomCollisionType.SpecialAir or RoomCollisionType.ShootableAir or
        RoomCollisionType.UnrevealedAir or RoomCollisionType.BombableAir or RoomCollisionType.HorizontalExtension or
        RoomCollisionType.VerticalExtension or RoomCollisionType.GrappleBlock;

    /// <summary>The kinds the shared axis move passes through: the kinds its unconditional block reactions do not stop.</summary>
    private static bool AxisMovePassable(RoomCollisionType kind) => kind is not (
        RoomCollisionType.Slope or RoomCollisionType.HorizontalExtension or RoomCollisionType.SolidBlock or
        RoomCollisionType.DoorBlock or RoomCollisionType.SpikeBlock or RoomCollisionType.SpecialBlock or
        RoomCollisionType.ShootableBlock or RoomCollisionType.VerticalExtension or RoomCollisionType.BombableBlock);

    /// <summary>
    /// Moves a projectile one pixel along <paramref name="edge"/>'s axis so its leading edge lies in the edge block and its
    /// span across the move starts in the span block and covers <paramref name="extraBlocks"/> more, and reports whether it
    /// collided.
    /// </summary>
    private static bool MoveScannedProjectile(RoomLevelData level, Axis edge, int edgeBlock, int spanBlock, int extraBlocks)
    {
        // A span radius of 2, 8 or 16 from two pixels into the span block covers one, two or three blocks.
        int spanRadius = extraBlocks switch { 0 => 2, 1 => 8, _ => 16 };
        int spanCenter = spanBlock * 16 + 2 + spanRadius;
        // A radius of one and a move of one pixel put the leading edge five pixels into the edge block.
        int moveCenter = edgeBlock * 16 + 4;
        ScannedProjectile.Position = edge == Axis.Horizontal
            ? FixedPosition<RoomPixels>.FromWhole(new(moveCenter, spanCenter))
            : FixedPosition<RoomPixels>.FromWhole(new(spanCenter, moveCenter));
        ScannedProjectile.Radius = edge == Axis.Horizontal ? new(1, spanRadius) : new(spanRadius, 1);
        ScannedProjectile.Velocity = edge == Axis.Horizontal
            ? EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(256, 0)
            : EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 256);
        return EnemyProjectileEngine.MoveProjectileAxis(ScannedProjectile, level, edge);
    }

    /// <summary>The projectile the scan row check moves, placed afresh for each move.</summary>
    private static readonly EnemyProjectile ScannedProjectile = new(0, default);

    /// <summary>
    /// A room for the scan row checks: air, walls, spike and grapple blocks and extension links leading up to a row's
    /// width or a column's height either way (or ending their chain), and the original's place scan of an enemy
    /// projectile's block scan over it.
    /// </summary>
    private sealed class ScanRowsRoom
    {
        /// <summary>Each block's terrain, by place.</summary>
        private readonly RoomTerrainBlock[] terrain;

        /// <summary>Each block's behaviour, by place.</summary>
        private readonly RoomBlockBehavior[] behaviors;

        /// <summary>The room's width in blocks.</summary>
        private readonly int width;

        /// <summary>Builds the room and its production level.</summary>
        private ScanRowsRoom(int width, int height, RoomTerrainBlock[] terrain, RoomBlockBehavior[] behaviors)
        {
            this.width = width;
            this.terrain = terrain;
            this.behaviors = behaviors;
            var size = new Extent16<RoomBlocks>(width, height);
            Level = new RoomLevelData(size,
                new RoomLevelPlanes(terrain, behaviors, [.. Enumerable.Repeat(RoomBlockAppearance.Of(0), size.Area)]),
                RoomLevelData.NothingBeyondGrid());
        }

        /// <summary>The production level the scans read.</summary>
        internal RoomLevelData Level { get; }

        /// <summary>A room filled deterministically from <paramref name="fill"/>.</summary>
        internal static ScanRowsRoom Filled(int width, int height, int fill)
        {
            RoomCollisionType[] kinds =
            [
                RoomCollisionType.Air, RoomCollisionType.Air, RoomCollisionType.SolidBlock, RoomCollisionType.SpikeBlock,
                RoomCollisionType.GrappleBlock, RoomCollisionType.HorizontalExtension, RoomCollisionType.VerticalExtension,
            ];
            int area = width * height;
            var terrain = new RoomTerrainBlock[area];
            var behaviors = new RoomBlockBehavior[area];
            for (int place = 0; place < area; place++)
            {
                RoomCollisionType kind = kinds[Mix(fill, place, 0) % kinds.Length];
                terrain[place] = RoomTerrainBlock.Of(0, BlockMirroring.None, kind);
                int farthest = Math.Min(kind == RoomCollisionType.VerticalExtension ? height : width, RoomBlockBehavior.MostForwardExtension);
                behaviors[place] = kind is RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension
                    ? RoomBlockBehavior.Extending(Mix(fill, place, 1) % (2 * farthest + 1) - farthest)
                    : RoomBlockBehavior.None;
            }
            return new ScanRowsRoom(width, height, terrain, behaviors);
        }

        /// <summary>
        /// The original's place scan: the first place is the first row's low eight bits times the width plus the first
        /// column, each later place one block on (a vertical edge) or one row on (a horizontal edge), within 32768 blocks.
        /// A first place past the grid is a wall; a later one fails, or is a wall when <paramref name="laterPlacesAreWalls"/>.
        /// Each block's links add their offsets, or their offsets times the width, to its place; a link outside the grid
        /// finds no block and is a wall, and a chain longer than the grid fails as cyclic.
        /// </summary>
        internal EnemyTerrainScanOutcome Scan(int edgeBlock, int spanBlock, int extraBlocks, Axis edge, Func<RoomCollisionType, bool> passable, bool laterPlacesAreWalls = false)
        {
            int row = edge == Axis.Vertical ? edgeBlock : spanBlock;
            int column = edge == Axis.Vertical ? spanBlock : edgeBlock;
            int first = (row & 0xff) * width + column;
            int step = edge == Axis.Vertical ? 1 : width;
            for (int tested = 0; tested <= extraBlocks; tested++)
            {
                int place = (first + tested * step) % ScanCycleBlocks;
                if (place >= terrain.Length)
                    return tested > 0 && !laterPlacesAreWalls ? new(false, typeof(InvalidDataException)) : new(true, null);
                EnemyTerrainScanOutcome? reached = Resolve(place, passable);
                if (reached is { } outcome)
                    return outcome;
            }
            return new(false, null);
        }

        /// <summary>
        /// Follows the links from <paramref name="place"/>: null when the block reached passes the scan on, a wall when it
        /// stops it or a link finds no block, and a failure for a cyclic chain.
        /// </summary>
        private EnemyTerrainScanOutcome? Resolve(int place, Func<RoomCollisionType, bool> passable)
        {
            for (int link = 0; link <= terrain.Length; link++)
            {
                if (place < 0 || place >= terrain.Length)
                    return new(true, null);
                RoomCollisionType kind = terrain[place].Collision;
                int offset = behaviors[place].ExtensionOffset;
                if (kind is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension) || offset == 0)
                    return passable(kind) ? null : new(true, null);
                place += kind == RoomCollisionType.VerticalExtension ? offset * width : offset;
            }
            return new(false, typeof(InvalidDataException));
        }

        /// <summary>A deterministic mix of the fill, the place and the purpose, zero or more.</summary>
        private static int Mix(int fill, int place, int purpose)
        {
            uint mixed = unchecked((uint)(fill * 73856093) ^ (uint)(place * 19349663) ^ (uint)(purpose * 83492791));
            mixed ^= mixed >> 13;
            mixed = unchecked(mixed * 1540483477u);
            mixed ^= mixed >> 15;
            return (int)(mixed % 1000003u);
        }
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
        behaviors[size.RowMajorIndex(broken)] = RoomBlockBehavior.Reacting(SpikeReaction.EnemyBreakable);
        var level = new RoomLevelData(size, new RoomLevelPlanes(terrain, behaviors,
            [.. Enumerable.Repeat(RoomBlockAppearance.Of(0), size.Area)]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        var mover = new EnemyTerrain(roomObjects.BreakableTerrain);
        var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(84, 24)),
            Radius = new(4, 4),
        };
        bool collided = mover.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, SignedSixteenSixteen.FromWholePixels(8));

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
        VerifyEnemyProjectileScanBlocks();
        VerifyEnemyProjectileScanRows();
        VerifyEnemySpikeBreakPastRowEnd();
        return 0;
    }
}
