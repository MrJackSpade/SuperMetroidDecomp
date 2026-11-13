using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// A room for the enemy terrain block addressing checks (#1628): a grid filled deterministically with air, walls, slopes,
/// spike and grapple blocks and extension links, the terrain beyond it, and the original's level-data place model of the
/// enemy lookups the production lookups are compared with. A block's place is its row times the room's width plus its
/// column, and the block at a place is the place's remainder and quotient by the width.
/// </summary>
internal sealed class EnemyTerrainFixtureRoom
{
    /// <summary>The blocks an enemy mover's scan counts before it continues from the room's first block.</summary>
    private const int ScanCycleBlocks = 32768;

    /// <summary>Each grid block's terrain, by place.</summary>
    private readonly RoomTerrainBlock[] terrain;

    /// <summary>Each grid block's behaviour, by place.</summary>
    private readonly RoomBlockBehavior[] behaviors;

    /// <summary>The terrain beyond the grid, by its place past the grid's last block.</summary>
    private readonly RoomTerrainBlock[] beyond;

    /// <summary>Builds the room and its production level.</summary>
    private EnemyTerrainFixtureRoom(int width, int height, RoomTerrainBlock[] terrain, RoomBlockBehavior[] behaviors, RoomTerrainBlock[] beyond)
    {
        Width = width;
        Height = height;
        this.terrain = terrain;
        this.behaviors = behaviors;
        this.beyond = beyond;
        var size = new Extent16<RoomBlocks>(width, height);
        Level = new RoomLevelData(size,
            new RoomLevelPlanes(terrain, behaviors, [.. Enumerable.Repeat(RoomBlockAppearance.Of(0), size.Area)]),
            new TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior>(beyond, beyond.Length, RoomTerrainBlocks.EdgeSolid, RoomBlockBehavior.None));
    }

    /// <summary>The room's width in blocks.</summary>
    internal int Width { get; }

    /// <summary>The room's height in blocks.</summary>
    internal int Height { get; }

    /// <summary>The production level the lookups read.</summary>
    internal RoomLevelData Level { get; }

    /// <summary>The blocks of the grid.</summary>
    private int Area => Width * Height;

    /// <summary>
    /// A room whose every grid block is one of air, a wall, a slope, a spike block, a grapple block or a horizontal or
    /// vertical extension leading up to a row's width or a column's height either way (or ending its chain), and whose
    /// <paramref name="beyondCount"/> blocks beyond the grid are air, walls, slopes, extensions, spike or grapple blocks.
    /// </summary>
    /// <param name="width">The room's width in blocks.</param>
    /// <param name="height">The room's height in blocks.</param>
    /// <param name="beyondCount">The blocks beyond the grid.</param>
    /// <param name="fill">Which of the deterministic fills to use.</param>
    internal static EnemyTerrainFixtureRoom Filled(int width, int height, int beyondCount, int fill)
    {
        RoomCollisionType[] gridKinds =
        [
            RoomCollisionType.Air, RoomCollisionType.SolidBlock, RoomCollisionType.Slope, RoomCollisionType.SpikeBlock,
            RoomCollisionType.GrappleBlock, RoomCollisionType.HorizontalExtension, RoomCollisionType.HorizontalExtension,
            RoomCollisionType.VerticalExtension, RoomCollisionType.VerticalExtension,
        ];
        RoomCollisionType[] beyondKinds =
        [
            RoomCollisionType.Air, RoomCollisionType.SolidBlock, RoomCollisionType.Slope,
            RoomCollisionType.HorizontalExtension, RoomCollisionType.SpikeBlock, RoomCollisionType.GrappleBlock,
        ];
        int area = width * height;
        var terrain = new RoomTerrainBlock[area];
        var behaviors = new RoomBlockBehavior[area];
        for (int place = 0; place < area; place++)
        {
            RoomCollisionType kind = gridKinds[Roll(fill, place, 0, gridKinds.Length)];
            terrain[place] = Block(kind);
            behaviors[place] = kind == RoomCollisionType.HorizontalExtension
                ? RoomBlockBehavior.Extending(Link(fill, place, Math.Min(width + 1, RoomBlockBehavior.MostForwardExtension)))
                : kind == RoomCollisionType.VerticalExtension
                    ? RoomBlockBehavior.Extending(Link(fill, place, Math.Min(height + 1, RoomBlockBehavior.MostForwardExtension)))
                    : RoomBlockBehavior.None;
        }

        RoomTerrainBlock[] beyond = [.. Enumerable.Range(0, beyondCount).Select(place => Block(beyondKinds[Roll(fill, place, 2, beyondKinds.Length)]))];
        return new EnemyTerrainFixtureRoom(width, height, terrain, behaviors, beyond);
    }

    /// <summary>
    /// A room whose every fourth block is an extension leading, either way, to a block that is not one, and whose other
    /// blocks are air, walls, slopes, spike and grapple blocks: its chains end after one link, even where a link runs past
    /// the scan's cycle, so a room tall enough for its grid to run past the cycle is compared at every place.
    /// </summary>
    /// <param name="width">The room's width in blocks; links of whole rows never lead from one fourth block to another.</param>
    /// <param name="height">The room's height in blocks.</param>
    /// <param name="fill">Which of the deterministic fills to use.</param>
    internal static EnemyTerrainFixtureRoom SingleLinks(int width, int height, int fill)
    {
        RoomCollisionType[] plainKinds =
        [
            RoomCollisionType.Air, RoomCollisionType.SolidBlock, RoomCollisionType.Slope, RoomCollisionType.SpikeBlock,
            RoomCollisionType.GrappleBlock,
        ];
        int area = width * height;
        var terrain = new RoomTerrainBlock[area];
        var behaviors = new RoomBlockBehavior[area];
        for (int place = 0; place < area; place++)
        {
            terrain[place] = Block(plainKinds[Roll(fill, place, 0, plainKinds.Length)]);
            behaviors[place] = RoomBlockBehavior.None;
            if (place % 4 != 0)
                continue;
            bool vertical = Roll(fill, place, 3, 2) == 1;
            // A link that is no multiple of four blocks (or rows, when the width leaves one over four) leads to a plain block.
            int link = Link(fill, place, vertical ? 3 : RoomBlockBehavior.MostForwardExtension);
            if (link % 4 == 0)
                link = link < 0 ? link + 1 : link - 1;
            terrain[place] = Block(vertical ? RoomCollisionType.VerticalExtension : RoomCollisionType.HorizontalExtension);
            behaviors[place] = RoomBlockBehavior.Extending(link);
        }

        return new EnemyTerrainFixtureRoom(width, height, terrain, behaviors, []);
    }

    /// <summary>The terrain at <paramref name="place"/>, in the grid or beyond it.</summary>
    internal RoomTerrainBlock TerrainAt(int place) => place < Area ? terrain[place] : beyond[place - Area];

    /// <summary>The behaviour at <paramref name="place"/>; every block beyond the grid has none.</summary>
    internal RoomBlockBehavior BehaviorAt(int place) => place < Area ? behaviors[place] : RoomBlockBehavior.None;

    /// <summary>
    /// The place model of an enemy projectile's extension links: from a block of the grid, each link adds its offset, or
    /// its offset times the width, to the place; a start or link outside the grid finds no block, and a chain longer
    /// than the grid fails as cyclic.
    /// </summary>
    internal EnemyTerrainResolvedPlace ResolveCollisionPlace(Position16<RoomBlocks> start)
    {
        if (start.X >= Width || start.Y >= Height)
            return new(null, null);
        int place = start.Y * Width + start.X;
        for (int link = 0; link <= Area; link++)
        {
            if (place < 0 || place >= Area)
                return new(null, null);
            if (LinkFrom(place) is not { } offset)
                return new(place, null);
            place += offset;
        }
        return new(null, typeof(InvalidDataException));
    }

    /// <summary>
    /// The place model of an enemy mover's read: each link adds its offset, or its offset times the width, to the place,
    /// counted within 32768 blocks; a place past the grid reads the terrain beyond it, which fails past its last block
    /// or for a block whose behaviour there is unbounded; a link before the room's first block fails, and a chain longer
    /// than the grid fails as cyclic.
    /// </summary>
    internal EnemyTerrainResolvedPlace ResolveMoverPlace(int start)
    {
        int place = start;
        for (int link = 0; link <= Area; link++)
        {
            if (place >= Area)
            {
                if (place - Area >= beyond.Length)
                    return new(null, typeof(NotSupportedException));
                return beyond[place - Area].Collision is RoomCollisionType.Slope or RoomCollisionType.HorizontalExtension
                    or RoomCollisionType.VerticalExtension or RoomCollisionType.SpikeBlock
                    ? new(null, typeof(NotSupportedException))
                    : new(place, null);
            }
            if (LinkFrom(place) is not { } offset)
                return new(place, null);
            if (place + offset < 0)
                return new(null, typeof(ArgumentOutOfRangeException));
            place = (place + offset) % ScanCycleBlocks;
        }
        return new(null, typeof(InvalidDataException));
    }

    /// <summary>
    /// The place model of an enemy projectile's block scan along one edge, the original's arithmetic: the first row taken
    /// within 256 (its low eight bits) times the width plus the first column is the first place; a scan down the column
    /// steps the place by the width and a scan along the row by one block, each place counted within 32768 blocks. A
    /// first place past the grid is a wall, and a later one fails. A block whose links find none, or of a kind other than
    /// air, an extension or a grapple block, stops the scan as a wall.
    /// </summary>
    internal EnemyTerrainScanOutcome ScanEdge(int fixedBlock, int startBlock, int extraBlocks, Axis edge)
    {
        int row = edge == Axis.Vertical ? fixedBlock : startBlock;
        int column = edge == Axis.Vertical ? startBlock : fixedBlock;
        int first = (row & 0xff) * Width + column;
        int step = edge == Axis.Vertical ? 1 : Width;
        for (int tested = 0; tested <= extraBlocks; tested++)
        {
            int place = (first + tested * step) % ScanCycleBlocks;
            if (place >= Area)
                return tested > 0 ? new(false, typeof(InvalidDataException)) : new(true, null);
            EnemyTerrainResolvedPlace resolved = ResolveCollisionPlace(Position16<RoomBlocks>.Of(place % Width, place / Width));
            if (resolved.Failure is { } failure)
                return new(false, failure);
            if (resolved.Place is not { } reached || terrain[reached].Collision is not (
                    RoomCollisionType.Air or RoomCollisionType.SpikeAir or RoomCollisionType.SpecialAir or
                    RoomCollisionType.ShootableAir or RoomCollisionType.UnusedAir or RoomCollisionType.BombableAir or
                    RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension or RoomCollisionType.GrappleBlock))
                return new(true, null);
        }
        return new(false, null);
    }

    /// <summary>The places an extension at <paramref name="place"/> leads on by: its offset, or its offset times the width.</summary>
    private int? LinkFrom(int place)
    {
        int offset = behaviors[place].ExtensionOffset;
        RoomCollisionType kind = terrain[place].Collision;
        if (offset == 0 || kind is not (RoomCollisionType.HorizontalExtension or RoomCollisionType.VerticalExtension))
            return null;
        return kind == RoomCollisionType.VerticalExtension ? offset * Width : offset;
    }

    /// <summary>A block of the kind, with the first picture and no mirroring.</summary>
    private static RoomTerrainBlock Block(RoomCollisionType kind) => RoomTerrainBlock.Of(0, BlockMirroring.None, kind);

    /// <summary>A link from <paramref name="place"/> of up to <paramref name="farthest"/> either way, zero included.</summary>
    private static int Link(int fill, int place, int farthest) => Roll(fill, place, 1, 2 * farthest + 1) - farthest;

    /// <summary>One of <paramref name="outcomes"/> outcomes, mixed from the fill, the place and the purpose.</summary>
    private static int Roll(int fill, int place, int purpose, int outcomes)
    {
        uint mixed = unchecked((uint)(fill * 73856093) ^ (uint)(place * 19349663) ^ (uint)(purpose * 83492791));
        mixed ^= mixed >> 13;
        mixed = unchecked(mixed * 1540483477u);
        mixed ^= mixed >> 15;
        return (int)(mixed % (uint)outcomes);
    }
}
