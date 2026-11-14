using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>
/// Confirms that the room-terrain lookups Samus's collision, her shots, the grapple and the X-ray share address blocks by
/// position (#1628) and find the blocks the original's level-data places address: a block's place is its row times the
/// room's width plus its column, an extension link adds its offset, or its offset times the width, to the place, and the
/// block at a place is the place's remainder and quotient by the width. Each check compares every input of its domain
/// with that place model, including the row ends, the room's edges and the lookups that fail.
/// </summary>
internal static class RoomTerrainBlockAddressingVerification
{
    /// <summary>Room sizes: single rows and columns, odd sizes, a screen-wide room and the widest room.</summary>
    private static readonly (int Width, int Height)[] RoomSizes = [(1, 1), (1, 4), (4, 1), (3, 2), (5, 4), (16, 3), (255, 2)];

    /// <summary>The fills of each fixture room the extension checks compare.</summary>
    private const int Fills = 24;

    /// <summary>
    /// Compares where every read of a room's terrain says its block lies with the block it read, for every position in and
    /// around each fixture room: a read of the grid finds the block named, the edge-solid read outside the room lies at
    /// no block, and a room object's read past a row's end or past the grid finds the block at that place, beyond the
    /// grid's last row.
    /// </summary>
    internal static void VerifyCollisionReadPositions()
    {
        foreach ((int width, int height) in RoomSizes)
        {
            int beyondCount = 2 * width + 1;
            TerrainRoom room = TerrainRoom.Filled(width, height, fill: 0, beyondCount);
            for (int y = 0; y <= height + 4; y++)
            for (int x = 0; x <= width + 2; x++)
            {
                var block = Position16<RoomBlocks>.Of(x, y);
                string Context(string read) => $"the {read} read at {block} in a {width}x{height} room";
                bool inGrid = x < width && y < height;
                if (inGrid)
                {
                    AssertEqual(room.Read(y * width + x), room.Level.GetCollisionBlock(block), Context("grid"));
                    AssertEqual(room.Read(y * width + x), room.Level.GetCollisionBlockOrEdgeSolid(block), Context("edge-solid"));
                }
                else
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => room.Level.GetCollisionBlock(block), Context("grid"));
                    RoomCollisionBlock edge = room.Level.GetCollisionBlockOrEdgeSolid(block);
                    AssertTrue(edge.Position is null && edge.Terrain == RoomTerrainBlocks.EdgeSolid && edge.Behavior == RoomBlockBehavior.None,
                        Context("edge-solid"));
                }

                int place = y * width + x;
                if (place < width * height + beyondCount)
                    AssertEqual(room.Read(place), room.Level.GetObjectCollisionBlock(block), Context("room object's"));
                else
                    AssertThrows<ArgumentOutOfRangeException>(() => room.Level.GetObjectCollisionBlock(block), Context("room object's"));
            }
        }

        Console.WriteLine("Room terrain read positions: every read says it lies at the block the level-data place it read addresses.");
    }

    /// <summary>
    /// Compares every extension link step from every block of each fixture room, horizontal and vertical, for every offset
    /// a link can hold, with the place model: the step adds the offset, or the offset times the width, to the place; a
    /// place outside the grid finds no block, which the grapple's links read as the edge-solid block.
    /// </summary>
    internal static void VerifyExtensionLinkSteps()
    {
        foreach ((int width, int height) in RoomSizes)
        {
            TerrainRoom room = TerrainRoom.Filled(width, height, fill: 1, beyondCount: 0);
            for (int place = 0; place < width * height; place++)
            for (int offset = RoomBlockBehavior.MostBackwardExtension; offset <= RoomBlockBehavior.MostForwardExtension; offset++)
            foreach (bool vertical in new[] { false, true })
            {
                Position16<RoomBlocks> block = BlockAtPlace(place, width);
                int reached = place + (vertical ? offset * width : offset);
                Position16<RoomBlocks>? expected = reached >= 0 && reached < width * height ? BlockAtPlace(reached, width) : null;
                Offset16<RoomBlocks> step = vertical ? Offset16<RoomBlocks>.Of(0, offset) : Offset16<RoomBlocks>.Of(offset, 0);
                string context = $"a {(vertical ? "vertical" : "horizontal")} link of {offset} from {block} in a {width}x{height} room";
                Position16<RoomBlocks>? stepped = room.Level.GridBlockStepped(block, step);
                if (stepped != expected)
                    AssertEqual(expected, stepped, context);

                var extension = new RoomCollisionBlock(block,
                    RoomTerrainBlock.Of(0, BlockMirroring.None, vertical ? RoomCollisionType.VerticalExtension : RoomCollisionType.HorizontalExtension),
                    RoomBlockBehavior.Extending(offset));
                RoomCollisionBlock linked = room.Level.LinkedBlockOrEdgeSolid(extension);
                RoomCollisionBlock linkedExpected = expected is { } at
                    ? room.Level.GetCollisionBlock(at)
                    : new RoomCollisionBlock(null, RoomTerrainBlocks.EdgeSolid, RoomBlockBehavior.None);
                if (linked != linkedExpected)
                    AssertEqual(linkedExpected, linked, $"the grapple's read of {context}");
            }
        }

        Console.WriteLine("Room terrain extension links: every link step reaches the block the level-data place it replaced addresses.");
    }

    /// <summary>
    /// Compares Samus's collision extension resolution, from every block of each fixture room, with the place walk: each
    /// link adds its offset, or its offset times the width, to the place; an ending link reports no collision, any other
    /// block reports its own, a link outside the grid fails and a chain as long as the grid fails as cyclic.
    /// </summary>
    internal static void VerifySamusExtensionResolution()
    {
        foreach ((int width, int height) in RoomSizes)
        for (int fill = 0; fill < Fills; fill++)
        {
            TerrainRoom room = TerrainRoom.Filled(width, height, fill, beyondCount: 0);
            for (int start = 0; start < width * height; start++)
            {
                string context = $"Samus's extension resolution from {BlockAtPlace(start, width)} in fill {fill} of a {width}x{height} room";
                (int? reached, bool collides) = room.WalkSamusExtension(start);
                RoomCollisionBlock block = room.Level.GetCollisionBlock(BlockAtPlace(start, width));
                if (reached is not { } place)
                {
                    AssertThrows<InvalidDataException>(() => SamusBlockCollision.TryResolveExtension(room.Level, ref block), context);
                    continue;
                }

                bool resolved = SamusBlockCollision.TryResolveExtension(room.Level, ref block);
                if (resolved != collides || block != room.Read(place))
                {
                    AssertEqual(collides, resolved, $"{context}: whether it collides");
                    AssertEqual(room.Read(place), block, $"{context}: the block reached");
                }
            }
        }

        Console.WriteLine("Samus extension resolution: every chain ends at the block the level-data place walk reaches.");
    }

    /// <summary>
    /// Compares a shot's block reaction, from every block of each fixture room, horizontal and vertical, with the place
    /// walk of its extension links: an ending link lets the shot pass, a block of a solid kind stops it, a link outside the
    /// grid fails and a chain of sixteen blocks fails as unterminated.
    /// </summary>
    internal static void VerifyShotExtensionResolution()
    {
        foreach ((int width, int height) in RoomSizes)
        for (int fill = 0; fill < Fills; fill++)
        {
            TerrainRoom room = TerrainRoom.Filled(width, height, fill, beyondCount: 0);
            for (int start = 0; start < width * height; start++)
            foreach (bool vertical in new[] { false, true })
            {
                Position16<RoomBlocks> block = BlockAtPlace(start, width);
                string context = $"a {(vertical ? "vertical" : "horizontal")} shot's reaction at {block} in fill {fill} of a {width}x{height} room";
                (int? reached, bool stops) = room.WalkShotExtension(start);
                if (reached is null)
                {
                    AssertThrows<InvalidDataException>(() => ScanShot(room.Level, block, vertical), context);
                    continue;
                }

                bool stopped = ScanShot(room.Level, block, vertical);
                if (stopped != stops)
                    AssertEqual(stops, stopped, context);
            }
        }

        Console.WriteLine("Shot extension resolution: every shot reacts to the block the level-data place walk reaches.");
    }

    /// <summary>
    /// Compares the block every step of a Wave shot's horizontal scan starts in with the block at its place along the
    /// room's rows, for every top row a scan can name, columns at, before and past the room's width, and every step.
    /// </summary>
    internal static void VerifyWaveScanReadBlocks()
    {
        foreach (int width in new[] { 1, 2, 3, 16, 17, 200, 255 })
        {
            int[] columns = [0, 1, width - 1, width, width + 1, 255, 4095];
            for (int row = 0; row <= 4095; row++)
            foreach (int column in columns)
            for (int step = 0; step < 16; step++)
            {
                WaveScanRead read = WaveScanRead.At(Position16<RoomBlocks>.Of(column, row), width, step);
                Position16<RoomBlocks> expected = BlockAtPlace(read.Block, width);
                Position16<RoomBlocks> block = read.BlockIn(width);
                if (block != expected)
                    AssertEqual(expected, block, $"the Wave scan's step {step} from ({column},{row}) in a room {width} blocks wide");
            }
        }

        Console.WriteLine("Wave scan read blocks: every read starts in the block at its place along the room's rows.");
    }

    /// <summary>Runs a one-block shot's horizontal or vertical scan over <paramref name="block"/> and reports whether it stopped.</summary>
    private static bool ScanShot(RoomLevelData level, Position16<RoomBlocks> block, bool vertical)
    {
        var shot = new SamusProjectile(0)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(block.X * 16 + 8, block.Y * 16 + 8)),
            Radius = new(1, 1),
            Velocity = vertical
                ? EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 1)
                : EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(1, 0),
        };
        return vertical
            ? ProjectileTerrainCollision.ScanVerticalShotReactions(level, shot, roomObjects: null)
            : ProjectileTerrainCollision.ScanHorizontalShotReactions(level, shot, roomObjects: null);
    }

    /// <summary>The block a level-data place addresses: the place's remainder by the width is its column, its quotient its row.</summary>
    private static Position16<RoomBlocks> BlockAtPlace(int place, int width) => Position16<RoomBlocks>.Of(place % width, place / width);

    /// <summary>
    /// A room for the terrain addressing checks: a grid of air, walls, spike and grapple blocks and extension links leading
    /// up to a row's width or a column's height either way (or ending their chain), the terrain beyond it, and the
    /// level-data place walks of the extension resolutions compared.
    /// </summary>
    private sealed class TerrainRoom
    {
        /// <summary>Each grid block's terrain, by place.</summary>
        private readonly RoomTerrainBlock[] terrain;

        /// <summary>Each grid block's behaviour, by place.</summary>
        private readonly RoomBlockBehavior[] behaviors;

        /// <summary>The terrain beyond the grid, by its place past the grid's last block.</summary>
        private readonly RoomTerrainBlock[] beyond;

        /// <summary>The room's width in blocks.</summary>
        private readonly int width;

        /// <summary>Builds the room and its production level.</summary>
        private TerrainRoom(int width, int height, RoomTerrainBlock[] terrain, RoomBlockBehavior[] behaviors, RoomTerrainBlock[] beyond)
        {
            this.width = width;
            this.terrain = terrain;
            this.behaviors = behaviors;
            this.beyond = beyond;
            var size = new Extent16<RoomBlocks>(width, height);
            Level = new RoomLevelData(size,
                new RoomLevelPlanes(terrain, behaviors, [.. Enumerable.Repeat(RoomBlockAppearance.Of(0), size.Area)]),
                new TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior>(beyond, beyond.Length, RoomTerrainBlocks.EdgeSolid, RoomBlockBehavior.None));
        }

        /// <summary>The production level the lookups read.</summary>
        internal RoomLevelData Level { get; }

        /// <summary>
        /// A room filled deterministically from <paramref name="fill"/>, with <paramref name="beyondCount"/> blocks of
        /// walls and air beyond the grid.
        /// </summary>
        internal static TerrainRoom Filled(int width, int height, int fill, int beyondCount)
        {
            RoomCollisionType[] kinds =
            [
                RoomCollisionType.Air, RoomCollisionType.SolidBlock, RoomCollisionType.SpikeBlock, RoomCollisionType.GrappleBlock,
                RoomCollisionType.HorizontalExtension, RoomCollisionType.HorizontalExtension,
                RoomCollisionType.VerticalExtension, RoomCollisionType.VerticalExtension,
            ];
            int area = width * height;
            var terrain = new RoomTerrainBlock[area];
            var behaviors = new RoomBlockBehavior[area];
            for (int place = 0; place < area; place++)
            {
                RoomCollisionType kind = kinds[Mix(fill, place, 0) % kinds.Length];
                // Each block shows its own picture, so every block's terrain is distinct.
                terrain[place] = RoomTerrainBlock.Of(place % 1024, BlockMirroring.None, kind);
                int farthest = Math.Min(kind == RoomCollisionType.VerticalExtension ? height + 1 : width + 1, RoomBlockBehavior.MostForwardExtension);
                behaviors[place] = kind is RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension
                    ? RoomBlockBehavior.Extending(Mix(fill, place, 1) % (2 * farthest + 1) - farthest)
                    : RoomBlockBehavior.None;
            }

            RoomTerrainBlock[] beyond = [.. Enumerable.Range(0, beyondCount).Select(place =>
                RoomTerrainBlock.Of(place % 1024, BlockMirroring.None, place % 2 == 0 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air))];
            return new TerrainRoom(width, height, terrain, behaviors, beyond);
        }

        /// <summary>The block at <paramref name="place"/>, in the grid or beyond it, as a read there finds it.</summary>
        internal RoomCollisionBlock Read(int place) => place < terrain.Length
            ? new(BlockAtPlace(place, width), terrain[place], behaviors[place])
            : new(BlockAtPlace(place, width), beyond[place - terrain.Length], RoomBlockBehavior.None);

        /// <summary>
        /// The place walk of Samus's extension resolution from <paramref name="start"/>: the place the chain ends at and
        /// whether it collides there (an ending link does not), or null when a link leads outside the grid or the chain
        /// is as long as the grid.
        /// </summary>
        internal (int? Reached, bool Collides) WalkSamusExtension(int start)
        {
            int place = start;
            for (int hop = 0; hop < terrain.Length; hop++)
            {
                if (LinkFrom(place) is not { } offset)
                    return (place, !IsExtension(place));
                place += offset;
                if (place < 0 || place >= terrain.Length)
                    return (null, false);
            }
            return (null, false);
        }

        /// <summary>
        /// The place walk of a shot's extension resolution from <paramref name="start"/>: the place the chain ends at and
        /// whether the shot stops there (an ending link lets it pass, a solid kind stops it), or null when a link leads
        /// outside the grid or sixteen blocks are all links.
        /// </summary>
        internal (int? Reached, bool Stops) WalkShotExtension(int start)
        {
            int place = start;
            for (int link = 0; link < 16; link++)
            {
                if (LinkFrom(place) is not { } offset)
                    return (place, !IsExtension(place) && terrain[place].Collision.IsSolidKind());
                place += offset;
                if (place < 0 || place >= terrain.Length)
                    return (null, false);
            }
            return (null, false);
        }

        /// <summary>Whether the block at <paramref name="place"/> is an extension.</summary>
        private bool IsExtension(int place) =>
            terrain[place].Collision is RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension;

        /// <summary>The places an extension at <paramref name="place"/> leads on by, or null for an ending link or another block.</summary>
        private int? LinkFrom(int place)
        {
            int offset = behaviors[place].ExtensionOffset;
            if (!IsExtension(place) || offset == 0)
                return null;
            return terrain[place].Collision == RoomCollisionType.VerticalExtension ? offset * width : offset;
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

    /// <summary>Runs the focused room terrain block addressing checks.</summary>
    [VerificationCommand("--room-terrain-block-addressing")]
    internal static int RoomTerrainBlockAddressingCommand()
    {
        VerifyCollisionReadPositions();
        VerifyExtensionLinkSteps();
        VerifySamusExtensionResolution();
        VerifyShotExtensionResolution();
        VerifyWaveScanReadBlocks();
        return 0;
    }
}
