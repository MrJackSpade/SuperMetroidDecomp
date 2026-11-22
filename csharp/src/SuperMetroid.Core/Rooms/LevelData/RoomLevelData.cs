using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A room's block grid (#1469): each foreground block's terrain (what it shows and how it collides) and behaviour, what
/// each foreground block shows, and what each background block shows. How a block looks is its picture in the room
/// tileset's block sheet (#1431), selected by its appearance. What lies past the grid is
/// <see cref="TerrainBeyondRoom{TTerrain, TBehavior}"/>'s.
/// </summary>
/// <remarks>
/// Collision, room objects and the display read the same grid, so rendering and physics never see different revisions
/// of a room. The foreground shows its terrain's appearance unless an installed layout or a room object's draw shows
/// another picture over the same terrain.
/// <para>
/// Every block of a kind that reads a reaction carries its kind's reaction (#2554): the room's blocks, in its grid and past
/// it, are checked as the room loads, and a write that would leave a reacting block whose behaviour gives its kind no
/// reaction is refused. A write changing both how a block collides and what it does is one write, so no block passes
/// through such a state.
/// </para>
/// </remarks>
[RoomLifetime]
public sealed partial class RoomLevelData
{
    /// <summary>Each foreground block's terrain, row by row; collision and room objects read and change it.</summary>
    internal readonly RoomTerrainBlock[] terrain;

    /// <summary>Each foreground block's behaviour, row by row; its terrain's collision kind gives it meaning.</summary>
    internal readonly RoomBlockBehavior[] behaviors;

    /// <summary>
    /// Where each foreground block leads as a door block, row by row, or null where it leads nowhere (#2090). A block's exit
    /// is what its behaviour leads through, so a behaviour write takes it away.
    /// </summary>
    internal readonly Dictionary<int, DoorBlockExit> exits = [];

    /// <summary>What each foreground block shows, row by row: its terrain's appearance unless a layout or draw shows another.</summary>
    internal readonly RoomBlockAppearance[] shownForeground;

    /// <summary>
    /// The collectible picture each foreground block where a collectible showing its own picture lies shows over its
    /// terrain's appearance (#1538), by block; a write that changes what the block shows takes the picture away.
    /// </summary>
    private readonly Dictionary<int, CollectiblePicture> shownCollectibles = [];

    /// <summary>What each background block shows, row by row; nothing changes it after loading.</summary>
    private readonly RoomBlockAppearance[] background;

    /// <summary>The terrain past the grid, which room objects and probes past the room's last block reach.</summary>
    internal readonly TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior> BeyondRoom;

    /// <summary>The foreground plane last published for display, until the next visual write; derived, never saved.</summary>
    private RoomLevelPlane? PublishedForeground { get; set; }

    /// <summary>
    /// The background plane, built the first time the room shows it and kept: the block-picture resolver knows the plane it
    /// resolved by its identity, so a restored room's background is the plane its restored resolver holds (#2554).
    /// </summary>
    private RoomLevelPlane? PublishedBackground { get; set; }

    /// <summary>Copies a room's grid; an installed layout changes only what the blocks initially show.</summary>
    /// <param name="size">Room size in 16-pixel blocks: the width is the row-major stride and both are positive (#1627).</param>
    /// <param name="planes">The foreground terrain, behaviour and background appearances, each exactly width times height entries in row-major order.</param>
    /// <param name="beyondRoom">
    /// The terrain past the grid, built for this room's lifetime: from its installed level for a room load, or
    /// <see cref="NothingBeyondGrid"/> for a room whose level space ends at its grid.
    /// </param>
    /// <param name="doorBlocks">Where each of the grid's door blocks leads; a room without doors has none.</param>
    /// <param name="visualLayout">Optional matching-sized initial appearances of both planes; terrain, behaviours and the terrain beyond the room stay unchanged.</param>
    /// <exception cref="ArgumentOutOfRangeException">Width or height is nonpositive.</exception>
    /// <exception cref="ArgumentException">A plane has the wrong size, or a door block lies outside the grid or is listed twice.</exception>
    /// <exception cref="InvalidDataException">
    /// The selected visual layout does not match the room dimensions, or a block of a reacting kind, in the grid or past it,
    /// has a behaviour that gives its kind no reaction.
    /// </exception>
    public RoomLevelData(
        Extent16<RoomBlocks> size,
        RoomLevelPlanes planes,
        TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior> beyondRoom,
        IReadOnlyCollection<RoomDoorBlock> doorBlocks,
        RoomLevelArtwork? visualLayout = null)
    {
        if (size.X <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Room width must be positive.");
        if (size.Y <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Room height must be positive.");

        int expectedBlocks = size.Area;
        if (planes.Foreground.Length != expectedBlocks)
            throw new ArgumentException("The foreground must hold exactly one terrain block per room block.", nameof(planes));
        if (planes.Behavior.Length != expectedBlocks)
            throw new ArgumentException("The behaviours must hold exactly one behaviour per room block.", nameof(planes));
        if (planes.Background.Length != expectedBlocks)
            throw new ArgumentException("The background must hold exactly one appearance per room block.", nameof(planes));

        Size = size;
        foreach (RoomDoorBlock door in Ensure.NotNull(doorBlocks))
        {
            if (!size.Contains(door.Block))
                throw new ArgumentException($"Door block {door.Block} lies outside the {size.X}x{size.Y}-block grid.", nameof(doorBlocks));
            int place = size.RowMajorIndex(door.Block);
            if (!exits.TryAdd(place, door.Exit))
                throw new ArgumentException($"Door block {door.Block} is listed twice.", nameof(doorBlocks));
        }
        terrain = planes.Foreground.ToArray();
        behaviors = planes.Behavior.ToArray();
        BeyondRoom = Ensure.NotNull(beyondRoom);
        for (int place = 0; place < terrain.Length; place++)
        {
            if (!CarriesItsReaction(terrain[place].Collision, behaviors[place]))
                throw new InvalidDataException($"Room block {size.CellAtRowMajorIndex(place)} {NoReaction(terrain[place].Collision, behaviors[place])}.");
        }
        for (int beyond = 0; beyond < BeyondRoom.Count; beyond++)
        {
            if (!CarriesItsReaction(BeyondRoom.TerrainAt(beyond).Collision, BeyondRoom.BehaviorAt(beyond)))
                throw new InvalidDataException($"Block {beyond} past the room's grid {NoReaction(BeyondRoom.TerrainAt(beyond).Collision, BeyondRoom.BehaviorAt(beyond))}.");
        }

        if (visualLayout is null)
        {
            shownForeground = [.. terrain.Select(block => block.Appearance)];
            background = planes.Background.ToArray();
            return;
        }
        if (visualLayout.Size != size)
            throw new InvalidDataException("Installed room layout dimensions do not match the room's grid.");
        shownForeground = visualLayout.Foreground.ToArray();
        background = visualLayout.Background.ToArray();
    }

    /// <summary>
    /// Terrain past a grid whose level space ends with it: no block lies past the room's last block, and a probe outside
    /// the room reads solid terrain with no behaviour. The intro's collision rooms, which have no level space of their
    /// own, are built with it.
    /// </summary>
    internal static TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior> NothingBeyondGrid() =>
        new([], 0, RoomTerrainBlocks.EdgeSolid, RoomBlockBehavior.None);

    /// <summary>
    /// A room whose blocks only collide (#2088): each block, row by row, collides as <paramref name="collision"/> gives,
    /// shows nothing and has no behaviour; nothing lies past the grid and the room has no doors. The opening's collision
    /// rooms, which it draws as pictures of its own, are built with it.
    /// </summary>
    /// <param name="size">Room size in 16-pixel blocks; both are positive.</param>
    /// <param name="collision">How each block collides, exactly width times height entries in row-major order.</param>
    /// <exception cref="ArgumentException"><paramref name="collision"/> does not hold one entry per room block.</exception>
    public static RoomLevelData CollisionOnly(Extent16<RoomBlocks> size, ReadOnlySpan<RoomCollisionType> collision)
    {
        var terrain = new RoomTerrainBlock[collision.Length];
        for (int block = 0; block < terrain.Length; block++)
            terrain[block] = new RoomTerrainBlock(RoomBlockAppearance.Nothing, collision[block]);
        var behaviors = new RoomBlockBehavior[collision.Length];
        behaviors.AsSpan().Fill(RoomBlockBehavior.None);
        var background = new RoomBlockAppearance[collision.Length];
        background.AsSpan().Fill(RoomBlockAppearance.Nothing);
        return new(size, new RoomLevelPlanes(terrain, behaviors, background), NothingBeyondGrid(), []);
    }

    /// <summary>
    /// Room size in 16-pixel blocks. The width is every row's index stride; the height comes from the room's level, which
    /// may hold more rows than the room's screens show.
    /// </summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>
    /// Whether the room's heavy spike blocks damage Samus (#2397): Wrecked Ship's are harmless until Phantoon is defeated,
    /// every other area's always damage. Samus's input phase samples it from the room's area and its boss before her
    /// movement reaches a spike, so a boss defeated later in the update leaves it as sampled.
    /// </summary>
    internal bool HeavySpikesDamage { get; set; } = true;

    /// <summary>
    /// The foreground as the room shows it (#1346). The same immutable plane is returned until a block write, so frames
    /// that change nothing share it.
    /// </summary>
    public RoomLevelPlane ForegroundPlane => PublishedForeground ??= new(Size, shownForeground, shownCollectibles);

    /// <summary>The background plane as the room shows it; nothing changes the background after loading, so it is built once.</summary>
    public RoomLevelPlane BackgroundPlane => PublishedBackground ??= new(Size, background);

    /// <summary>
    /// <paramref name="block"/>, which must be a block of the room's grid, as a room object that sits on the grid is
    /// placed. Coordinates outside the room fail loudly.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies outside the room's width or height.</exception>
    internal Position16<RoomBlocks> GridBlock(Position16<RoomBlocks> block)
    {
        RequireInGrid(block);
        return block;
    }

    /// <summary>
    /// <paramref name="block"/>, which must lie off the room's grid below its last row, in one of its columns, as the two
    /// shipped off-room coloured doors are placed (#2034, #2572); nothing reads or writes terrain there for them.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies in the grid or past the room's width.</exception>
    internal Position16<RoomBlocks> BlockOffTheGrid(Position16<RoomBlocks> block)
    {
        if (block.X >= Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block X {block.X} lies outside the {Size.X}-block room width.");
        if (block.Y < Size.Y)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block {block} lies in the room's {Size.Y}-row grid, not off it.");
        return block;
    }

    /// <summary>
    /// The block <paramref name="step"/> blocks from a room object's <paramref name="block"/>, as the game's room objects
    /// reach their neighbours and move their origins: the room's blocks run in reading order, so a column step past a
    /// row's last block continues on the next row, one before a row's first block continues on the row above, and a row
    /// step past the grid lands in the terrain beyond the room.
    /// </summary>
    /// <remarks>
    /// The reading-order reach is kept because the game observes it where it happens: an object at a row's edge whose
    /// neighbour step crosses it writes and finds the neighbouring row's block. <see cref="RoomReadingOrder"/> computes
    /// that reach; a purely vertical step never crosses a row's end and is the position plus its offset.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The step reaches before the room's first block.</exception>
    internal Position16<RoomBlocks> ObjectBlockStep(Position16<RoomBlocks> block, Offset16<RoomBlocks> step)
    {
        int reached = RoomReadingOrder.PlaceStepped(block, step, Size.X);
        return reached >= 0 ? RoomReadingOrder.BlockAt(reached, Size.X) : throw new ArgumentOutOfRangeException(nameof(step),
            $"A step of {step} from room-object block {block} reaches before the room's first block.");
    }

    /// <summary>
    /// Finds the block of the room's grid <paramref name="step"/> blocks from <paramref name="block"/> in the room's reading
    /// order (<see cref="RoomReadingOrder"/>), as Samus's collision extensions, her shots' and the grapple's extension
    /// links and a Wave shot's straddled read reach it: a column step past a row's end continues on the next row; false
    /// when the step leads before the room's first block or past its grid.
    /// </summary>
    /// <param name="block">The block stepped from.</param>
    /// <param name="step">The columns and rows stepped.</param>
    /// <param name="reached">The block reached.</param>
    internal bool TryGridBlockStepped(Position16<RoomBlocks> block, Offset16<RoomBlocks> step, out Position16<RoomBlocks> reached)
    {
        int place = RoomReadingOrder.PlaceStepped(block, step, Size.X);
        reached = place >= 0 ? RoomReadingOrder.BlockAt(place, Size.X) : default;
        return place >= 0 && Size.Contains(reached);
    }

    /// <summary>
    /// The block <paramref name="extension"/>'s link leads to (<see cref="RoomCollisionBlock.ExtensionLink"/>), or the
    /// edge-solid block a probe outside the room reads when it leads outside the room's grid, as the grapple's extension
    /// links reach it.
    /// </summary>
    /// <param name="extension">An extension block the read found in the grid.</param>
    internal RoomCollisionBlock LinkedBlockOrEdgeSolid(RoomCollisionBlock extension) =>
        TryGridBlockStepped(extension.At, extension.ExtensionLink, out Position16<RoomBlocks> reached) ? GetCollisionBlock(reached) : EdgeSolid(reached);

    /// <summary>
    /// The whole block <paramref name="block"/> stands for, as Samus's body, her shots, missiles and bombs collide with and
    /// react to it (<see cref="ContinuedBlocks"/>): the block itself, or the block a continuing cell's links lead to; null
    /// when the cell behaves as air.
    /// </summary>
    /// <param name="block">A block the read found in the grid.</param>
    /// <exception cref="InvalidDataException">A link leads outside the room's grid, or the chain is cyclic.</exception>
    internal StoodForBlock? BlockStoodFor(RoomCollisionBlock block) =>
        ContinuedBlocks.StoodFor(block, Size.Area, AsWholeBlock, cell => cell.Behavior.EndsExtension, LinkedBlock);

    /// <summary>
    /// The block <paramref name="block"/> stands for (<see cref="BlockStoodFor"/>) with how it collides, which is no
    /// extension (#2553); null when the cell behaves as air.
    /// </summary>
    /// <param name="block">A block the read found in the grid.</param>
    /// <exception cref="InvalidDataException">A link leads outside the room's grid, or the chain is cyclic.</exception>
    internal StandingBlock? StandingBlockFor(RoomCollisionBlock block) =>
        ContinuedBlocks.StoodFor(block, Size.Area,
            cell => StandingTypes[cell.CollisionType] is { } type ? new StandingBlock(cell, type) : (StandingBlock?)null,
            cell => cell.Behavior.EndsExtension,
            LinkedBlock);

    /// <summary>The block <paramref name="extension"/>'s link leads to (<see cref="RoomCollisionBlock.ExtensionLink"/>).</summary>
    /// <param name="extension">An extension block the read found in the grid.</param>
    /// <exception cref="InvalidDataException">The link leads outside the room's grid.</exception>
    private RoomCollisionBlock LinkedBlock(RoomCollisionBlock extension) =>
        GetCollisionBlock(TryGridBlockStepped(extension.At, extension.ExtensionLink, out Position16<RoomBlocks> reached) ? reached
            : throw new InvalidDataException(
                $"Continuing block {extension.Position} type {extension.CollisionType}/behaviour ({extension.Behavior}) leads outside the room's grid."));

    /// <summary>How a block of each collision family collides once a read stands on it; none for the two extensions, which continue another block.</summary>
    private static IdentityTable<RoomCollisionType, StandingCollisionType?> StandingTypes { get; } = new(new Dictionary<RoomCollisionType, StandingCollisionType?>
    {
        [RoomCollisionType.Air] = StandingCollisionType.Air,
        [RoomCollisionType.Slope] = StandingCollisionType.Slope,
        [RoomCollisionType.SpikeAir] = StandingCollisionType.SpikeAir,
        [RoomCollisionType.SpecialAir] = StandingCollisionType.SpecialAir,
        [RoomCollisionType.ShootableAir] = StandingCollisionType.ShootableAir,
        [RoomCollisionType.HorizontalExtension] = null,
        [RoomCollisionType.UnrevealedAir] = StandingCollisionType.UnrevealedAir,
        [RoomCollisionType.BombableAir] = StandingCollisionType.BombableAir,
        [RoomCollisionType.SolidBlock] = StandingCollisionType.SolidBlock,
        [RoomCollisionType.DoorBlock] = StandingCollisionType.DoorBlock,
        [RoomCollisionType.SpikeBlock] = StandingCollisionType.SpikeBlock,
        [RoomCollisionType.SpecialBlock] = StandingCollisionType.SpecialBlock,
        [RoomCollisionType.ShootableBlock] = StandingCollisionType.ShootableBlock,
        [RoomCollisionType.VerticalExtension] = null,
        [RoomCollisionType.GrappleBlock] = StandingCollisionType.GrappleBlock,
        [RoomCollisionType.BombableBlock] = StandingCollisionType.BombableBlock,
    });

    /// <summary>
    /// Replaces <paramref name="block"/> with the whole block it stands for (<see cref="BlockStoodFor"/>), returning false
    /// when it behaves as air.
    /// </summary>
    /// <param name="block">A block the read found in the grid; the block it stands for once the call returns true.</param>
    /// <exception cref="InvalidDataException">A link leads outside the room's grid, or the chain is cyclic.</exception>
    internal bool TryReachBlockStoodFor(ref RoomCollisionBlock block)
    {
        if (BlockStoodFor(block) is not { } stoodFor)
            return false;
        block = stoodFor.Block;
        return true;
    }

    /// <summary>
    /// The whole block <paramref name="block"/> stands for as the grapple beam's checks read it (<see cref="ContinuedBlocks"/>):
    /// a link that leads outside the room's grid reads the edge-solid block a probe outside the room reads; null when the
    /// cell behaves as air.
    /// </summary>
    /// <param name="block">A block the read found in the grid.</param>
    /// <exception cref="InvalidDataException">The chain is cyclic.</exception>
    internal StoodForBlock? GrappleBlockStoodFor(RoomCollisionBlock block) =>
        ContinuedBlocks.StoodFor(block, Size.Area, AsWholeBlock, cell => cell.Behavior.EndsExtension, LinkedBlockOrEdgeSolid);

    /// <summary>
    /// <paramref name="block"/> as a whole block, with its kind as one; none when it continues a block of several cells,
    /// its link leading to the block it stands for.
    /// </summary>
    private static StoodForBlock? AsWholeBlock(RoomCollisionBlock block) =>
        block.Kind.AsWholeBlock is { } whole ? new StoodForBlock(block, whole) : null;

    /// <summary>Reads a room object's block, inside the grid or in the terrain beyond the room.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the terrain beyond the room.</exception>
    internal RoomCollisionBlock GetObjectCollisionBlock(Position16<RoomBlocks> block) =>
        ObjectCollisionBlockAt(Size.RowMajorIndex(block));

    /// <summary>
    /// A room object's terrain write at its block, showing the terrain's own picture. Writes inside the grid reach
    /// collision and the display; writes past it persist in the terrain beyond the room.
    /// </summary>
    /// <exception cref="ArgumentException">The terrain is of a reacting kind and the block's behaviour gives it no reaction.</exception>
    internal void SetObjectTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain)
    {
        int place = Size.RowMajorIndex(block);
        RequireItsReaction(terrain.Collision, BehaviorAt(place), nameof(terrain));
        WriteObjectTerrain(place, terrain, terrain.Appearance);
    }

    /// <summary>
    /// A room object's change of only how a block collides and what it does, inside the grid or in the terrain beyond
    /// the room (#2496): the block keeps its picture and what it shows. A block of the grid no longer leads where its
    /// earlier behaviour did.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the terrain beyond the room.</exception>
    /// <exception cref="ArgumentException">The collision kind reads a reaction the behaviour does not give it.</exception>
    internal void SetObjectCollisionAndBehavior(Position16<RoomBlocks> block, RoomCollisionType collision, RoomBlockBehavior behavior)
    {
        int place = Size.RowMajorIndex(block);
        RequireItsReaction(collision, behavior, nameof(behavior));
        if (place >= terrain.Length)
        {
            int beyond = BeyondPlace(place);
            BeyondRoom.SetTerrain(beyond, BeyondRoom.TerrainAt(beyond).WithCollision(collision));
            BeyondRoom.SetBehavior(beyond, behavior);
            return;
        }

        terrain[place] = terrain[place].WithCollision(collision);
        WriteBehavior(place, behavior);
    }

    /// <summary>
    /// Makes a block of the grid lead through <paramref name="exit"/> as a door block, as Mother Brain's escape door is set
    /// up (#2090); the block's collision kind and behaviour are written first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    internal void SetDoorExit(Position16<RoomBlocks> block, DoorBlockExit exit) => exits[GridPlace(block)] = exit;

    /// <summary>
    /// Shows <paramref name="picture"/> at a foreground block over its terrain's appearance, as a collectible showing its
    /// own picture does where it lies (#1538), until a write changes what the block shows. Past the grid nothing is shown,
    /// as for a room object's terrain write there.
    /// </summary>
    internal void ShowCollectible(Position16<RoomBlocks> block, CollectiblePicture picture)
    {
        int place = Size.RowMajorIndex(block);
        if (place >= terrain.Length)
            return;
        shownCollectibles[place] = picture;
        PublishedForeground = null;
    }

    /// <summary>
    /// Replaces one foreground block's terrain, collision kind and picture together, and shows its picture; the block must
    /// lie in the grid.
    /// </summary>
    /// <remarks>
    /// Room-object draws write the whole block: breakable grapple frames alternate between grapple terrain and air, so
    /// a narrower "change the picture" helper would be incorrect here.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    /// <exception cref="ArgumentException">The terrain is of a reacting kind and the block's behaviour gives it no reaction.</exception>
    internal void SetTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain)
    {
        int place = GridPlace(block);
        RequireItsReaction(terrain.Collision, behaviors[place], nameof(terrain));
        WriteObjectTerrain(place, terrain, terrain.Appearance);
    }

    /// <summary>
    /// Replaces one foreground block's terrain and behaviour together, and shows its picture, as a room object that writes
    /// a block's collision kind, picture and behaviour at once does; the block must lie in the grid and no longer leads where
    /// its earlier behaviour did.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    /// <exception cref="ArgumentException">The terrain is of a reacting kind and the behaviour gives it no reaction.</exception>
    internal void SetTerrainAndBehavior(Position16<RoomBlocks> block, RoomTerrainBlock terrain, RoomBlockBehavior behavior)
    {
        int place = GridPlace(block);
        RequireItsReaction(terrain.Collision, behavior, nameof(behavior));
        WriteObjectTerrain(place, terrain, terrain.Appearance);
        WriteBehavior(place, behavior);
    }

    /// <summary>
    /// Changes only how one foreground block collides (#2494): the block keeps its picture, what it shows, its
    /// behaviour and where it leads; the block must lie in the grid.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    /// <exception cref="ArgumentException">The kind reads a reaction the block's behaviour does not give it.</exception>
    internal void SetCollision(Position16<RoomBlocks> block, RoomCollisionType collision)
    {
        int place = GridPlace(block);
        RequireItsReaction(collision, behaviors[place], nameof(collision));
        terrain[place] = terrain[place].WithCollision(collision);
    }

    /// <summary>
    /// Changes how one foreground block collides and what it does together (#2554): the block keeps its picture and what it
    /// shows, and no longer leads where its earlier behaviour did; the block must lie in the grid.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    /// <exception cref="ArgumentException">The kind reads a reaction the behaviour does not give it.</exception>
    internal void SetCollisionAndBehavior(Position16<RoomBlocks> block, RoomCollisionType collision, RoomBlockBehavior behavior)
    {
        int place = GridPlace(block);
        RequireItsReaction(collision, behavior, nameof(behavior));
        terrain[place] = terrain[place].WithCollision(collision);
        WriteBehavior(place, behavior);
    }

    /// <summary>
    /// The shared deactivating setup at one foreground block (#2502): a block of a solid kind becomes a plain solid block
    /// and any other becomes air (<see cref="RoomTerrainBlock.Deactivated"/>). Only its collision kind changes: it keeps
    /// its picture, what it shows, its behaviour and where it leads; the block must lie in the grid.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    internal void Deactivate(Position16<RoomBlocks> block)
    {
        int place = GridPlace(block);
        RoomTerrainBlock deactivated = terrain[place].Deactivated();
        RequireItsReaction(deactivated.Collision, behaviors[place], nameof(block));
        terrain[place] = deactivated;
    }

    /// <summary>
    /// Replaces the behaviour of one foreground block, which no longer leads where its earlier behaviour did; the block must
    /// lie in the grid.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    /// <exception cref="ArgumentException">The block's kind reads a reaction the behaviour does not give it.</exception>
    internal void SetBehavior(Position16<RoomBlocks> block, RoomBlockBehavior behavior)
    {
        int place = GridPlace(block);
        RequireItsReaction(terrain[place].Collision, behavior, nameof(behavior));
        WriteBehavior(place, behavior);
    }

    /// <summary>A block past the room's grid, as an enemy terrain read finds it.</summary>
    /// <param name="block">A block in the room's columns whose row lies past the room's height.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block is inside the grid, or its column lies past the room's width.</exception>
    /// <exception cref="NotSupportedException">The block is past the terrain beyond the room.</exception>
    internal RoomCollisionBlock GetBlockBeyondRoom(Position16<RoomBlocks> block)
    {
        if (block.X >= Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block {block} lies outside the {Size.X}-block room width.");
        if (Size.Contains(block))
            throw new ArgumentOutOfRangeException(nameof(block), "The block is inside the room's grid.");
        int beyond = Size.RowMajorIndex(block) - terrain.Length;
        if (beyond >= BeyondRoom.Count)
        {
            throw new NotSupportedException(
                $"Block {block} lies past the room's grid and the {BeyondRoom.Count} blocks beyond it.");
        }
        return new RoomCollisionBlock(block, BeyondRoom.TerrainAt(beyond), BeyondRoom.BehaviorAt(beyond));
    }

    /// <summary>Returns the terrain and behaviour collision reads for one block of the grid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    public RoomCollisionBlock GetCollisionBlock(Position16<RoomBlocks> block) =>
        GridCollisionBlockAt(GridPlace(block));

    /// <summary>
    /// Reads a block of the grid, or the edge-solid block when a collision probe asks about a position outside the
    /// room's width or height (<see cref="TerrainBeyondRoom{TTerrain, TBehavior}.EdgeSolid"/>).
    /// </summary>
    internal RoomCollisionBlock GetCollisionBlockOrEdgeSolid(Position16<RoomBlocks> block) =>
        Size.Contains(block) ? GetCollisionBlock(block) : EdgeSolid(block);

    /// <summary>Reads the block containing a room pixel: sixteen pixels to a block.</summary>
    public RoomCollisionBlock GetCollisionBlockAtPixel(Position16<RoomPixels> position) =>
        GetCollisionBlock(position.ToRoomBlock());

    /// <summary>
    /// Where a door block of the grid leads: through a door, or to the elevator's door. Elevator and physical door blocks
    /// are found alike; what Samus's collision with the block does is the contact she makes with it
    /// (<see cref="DoorContact"/>, #2545).
    /// </summary>
    /// <param name="doorBlock">The door block collision found in the grid.</param>
    /// <exception cref="InvalidDataException">
    /// The block leads nowhere: neither the installed level nor a room object's write made it lead through a door.
    /// </exception>
    public DoorBlockExit DoorExitAt(RoomCollisionBlock doorBlock)
    {
        Position16<RoomBlocks> block = doorBlock.At;
        return exits.TryGetValue(GridPlace(block), out DoorBlockExit? exit) ? exit : throw new InvalidDataException($"Door block {block} leads nowhere.");
    }

    /// <summary>
    /// The place of <paramref name="block"/>, which must be a block of the room's grid, in the row-by-row planes that hold
    /// the grid. Coordinates outside the room fail loudly.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies outside the room's width or height.</exception>
    private int GridPlace(Position16<RoomBlocks> block)
    {
        RequireInGrid(block);
        return Size.RowMajorIndex(block);
    }

    /// <summary>Requires <paramref name="block"/> to lie in the room's grid; coordinates outside the room fail loudly.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies outside the room's width or height.</exception>
    private void RequireInGrid(Position16<RoomBlocks> block)
    {
        if (block.X >= Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block X {block.X} lies outside the {Size.X}-block room width.");
        if (block.Y >= Size.Y)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block Y {block.Y} lies outside the {Size.Y}-block room height.");
    }

    /// <summary>Writes the behaviour of the grid's block at <paramref name="place"/>, which no longer leads where it did.</summary>
    internal void WriteBehavior(int place, RoomBlockBehavior behavior)
    {
        behaviors[place] = behavior;
        exits.Remove(place);
    }

    /// <summary>The block of the grid at <paramref name="place"/> in its planes, as collision reads it.</summary>
    private RoomCollisionBlock GridCollisionBlockAt(int place) =>
        new(Size.CellAtRowMajorIndex(place), terrain[place], behaviors[place]);

    /// <summary>A room object's block at <paramref name="place"/> in the level space, inside the grid or beyond it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is past the terrain beyond the room.</exception>
    private RoomCollisionBlock ObjectCollisionBlockAt(int place)
    {
        if (place < terrain.Length)
            return GridCollisionBlockAt(place);
        int beyond = BeyondPlace(place);
        return new RoomCollisionBlock(Size.CellAtRowMajorIndex(place), BeyondRoom.TerrainAt(beyond), BeyondRoom.BehaviorAt(beyond));
    }

    /// <summary>
    /// A terrain write at <paramref name="place"/> in the level space that shows <paramref name="shown"/>. Writes inside
    /// the grid reach collision and the display; writes past it persist in the terrain beyond the room.
    /// </summary>
    internal void WriteObjectTerrain(int place, RoomTerrainBlock block, RoomBlockAppearance shown)
    {
        if (place >= terrain.Length)
        {
            BeyondRoom.SetTerrain(BeyondPlace(place), block);
            return;
        }

        terrain[place] = block;
        if (shownForeground[place] != shown)
            shownCollectibles.Remove(place);
        shownForeground[place] = shown;
        PublishedForeground = null;
    }

    /// <summary>The behaviour of the block at <paramref name="place"/> in the level space, inside the grid or beyond it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is past the terrain beyond the room.</exception>
    private RoomBlockBehavior BehaviorAt(int place) =>
        place < terrain.Length ? behaviors[place] : BeyondRoom.BehaviorAt(BeyondPlace(place));

    /// <summary>
    /// Whether a block of <paramref name="collision"/> with <paramref name="behavior"/> carries the reaction its kind reads
    /// (#2554): a kind that reads none always does, and a reacting kind does when its behaviour gives its family a reaction.
    /// </summary>
    private static bool CarriesItsReaction(RoomCollisionType collision, RoomBlockBehavior behavior) =>
        RoomBlockKind.TryOf(collision, behavior, out _);

    /// <summary>Names a reacting kind's block whose behaviour gives its kind no reaction, for a refusal.</summary>
    private static string NoReaction(RoomCollisionType collision, RoomBlockBehavior behavior) =>
        $"is a {collision} block whose behaviour ({behavior}) gives its kind no reaction";

    /// <summary>Refuses a write that would leave a block of a reacting kind without its reaction (<see cref="CarriesItsReaction"/>).</summary>
    /// <param name="collision">How the block collides once written.</param>
    /// <param name="behavior">The block's behaviour once written.</param>
    /// <param name="parameter">The written argument that would leave it.</param>
    /// <exception cref="ArgumentException">The block would be of a reacting kind without its reaction.</exception>
    private static void RequireItsReaction(RoomCollisionType collision, RoomBlockBehavior behavior, string parameter)
    {
        if (!CarriesItsReaction(collision, behavior))
            throw new ArgumentException($"The write would leave a block that {NoReaction(collision, behavior)}.", parameter);
    }

    /// <summary>The block a collision probe outside the room reads.</summary>
    /// <param name="probed">The block the probe named, outside the room.</param>
    private RoomCollisionBlock EdgeSolid(Position16<RoomBlocks> probed) => RoomCollisionBlock.OutsideRoom(probed, BeyondRoom.EdgeSolid, BeyondRoom.NoBehavior);

    /// <summary>A place in the level space past the grid, counted from the first block past it.</summary>
    internal int BeyondPlace(int place) => place - terrain.Length;
}
