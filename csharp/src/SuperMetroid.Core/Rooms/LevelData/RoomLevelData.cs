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
/// </remarks>
[RoomLifetime]
public sealed class RoomLevelData
{
    /// <summary>Each foreground block's terrain, row by row; collision and room objects read and change it.</summary>
    internal readonly RoomTerrainBlock[] Terrain;

    /// <summary>Each foreground block's behaviour, row by row; its terrain's collision kind gives it meaning.</summary>
    internal readonly RoomBlockBehavior[] Behaviors;

    /// <summary>
    /// Where each foreground block leads as a door block, row by row, or null where it leads nowhere (#2090). A block's exit
    /// is what its behaviour leads through, so a behaviour write takes it away.
    /// </summary>
    internal readonly DoorBlockExit?[] Exits;

    /// <summary>What each foreground block shows, row by row: its terrain's appearance unless a layout or draw shows another.</summary>
    internal readonly RoomBlockAppearance[] ShownForeground;

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
    /// <exception cref="InvalidDataException">The selected visual layout does not match the room dimensions.</exception>
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
        Exits = new DoorBlockExit?[expectedBlocks];
        foreach (RoomDoorBlock door in Ensure.NotNull(doorBlocks))
        {
            if (!size.Contains(door.Block))
                throw new ArgumentException($"Door block {door.Block} lies outside the {size.X}x{size.Y}-block grid.", nameof(doorBlocks));
            int place = size.RowMajorIndex(door.Block);
            if (Exits[place] is not null)
                throw new ArgumentException($"Door block {door.Block} is listed twice.", nameof(doorBlocks));
            Exits[place] = door.Exit;
        }
        Terrain = planes.Foreground.ToArray();
        Behaviors = planes.Behavior.ToArray();
        BeyondRoom = Ensure.NotNull(beyondRoom);

        if (visualLayout is null)
        {
            ShownForeground = [.. Terrain.Select(block => block.Appearance)];
            background = planes.Background.ToArray();
            return;
        }
        if (visualLayout.Size != size)
            throw new InvalidDataException("Installed room layout dimensions do not match the room's grid.");
        ShownForeground = visualLayout.Foreground.ToArray();
        background = visualLayout.Background.ToArray();
    }

    /// <summary>
    /// Terrain past a grid whose level space ends with it: no block lies past the room's last block, and a probe outside
    /// the room reads solid terrain with no behaviour. The intro's collision rooms, which have no level space of their
    /// own, are built with it.
    /// </summary>
    public static TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior> NothingBeyondGrid() =>
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

    /// <summary>The door definition selected by the most recent door-block collision, until the transition consumes it.</summary>
    public DoorDefinition? PendingDoorTransition { get; private set; }

    /// <summary>
    /// Set when a door block resolves to the elevator's pseudo door for a standing pose. Enemies update before Samus's
    /// collision, so the elevator consumes this publication at the beginning of the following gameplay frame.
    /// </summary>
    public bool ElevatorDoorContactPending { get; private set; }

    /// <summary>
    /// The foreground as the room shows it (#1346). The same immutable plane is returned until a block write, so frames
    /// that change nothing share it.
    /// </summary>
    public RoomLevelPlane ForegroundPlane => PublishedForeground ??= new(Size, ShownForeground, shownCollectibles);

    /// <summary>The background plane as the room shows it; nothing changes the background after loading, so it is built once.</summary>
    public RoomLevelPlane BackgroundPlane => field ??= new(Size, background);

    /// <summary>
    /// <paramref name="block"/>, which must be a block of the room's grid, as a room object that sits on the grid is
    /// placed. Coordinates outside the room fail loudly.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies outside the room's width or height.</exception>
    internal Position16<RoomBlocks> GridBlock(Position16<RoomBlocks> block)
    {
        _ = GridPlace(block);
        return block;
    }

    /// <summary>
    /// <paramref name="block"/>, which must be a block of the terrain beyond the room: a column of the room's width in a row
    /// past its height that the terrain beyond the room reaches, as the two shipped off-room coloured doors are placed
    /// (#2034).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies in the grid, past the room's width, or past the terrain beyond the room.</exception>
    internal Position16<RoomBlocks> BlockBeyondRoom(Position16<RoomBlocks> block)
    {
        if (block.X >= Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block X {block.X} lies outside the {Size.X}-block room width.");
        if (block.Y < Size.Y)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block {block} lies in the room's {Size.Y}-row grid, not beyond it.");
        if (BeyondPlace(Size.RowMajorIndex(block)) >= BeyondRoom.Count)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block {block} lies past the {BeyondRoom.Count} blocks beyond the room.");
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
    internal Position16<RoomBlocks> ObjectBlockStep(Position16<RoomBlocks> block, Offset16<RoomBlocks> step) =>
        RoomReadingOrder.Step(block, step, Size.X) ??
        throw new ArgumentOutOfRangeException(nameof(step), $"A step of {step} from room-object block {block} reaches before the room's first block.");

    /// <summary>
    /// The block of the room's grid <paramref name="step"/> blocks from <paramref name="block"/> in the room's reading
    /// order (<see cref="RoomReadingOrder"/>), as Samus's collision extensions, her shots' and the grapple's extension
    /// links and a Wave shot's straddled read reach it: a column step past a row's end continues on the next row.
    /// </summary>
    /// <param name="block">The block stepped from.</param>
    /// <param name="step">The columns and rows stepped.</param>
    /// <returns>The block reached, or null when the step leads before the room's first block or past its grid.</returns>
    internal Position16<RoomBlocks>? GridBlockStepped(Position16<RoomBlocks> block, Offset16<RoomBlocks> step) =>
        RoomReadingOrder.Step(block, step, Size.X) is { } reached && Size.Contains(reached) ? reached : null;

    /// <summary>
    /// The block <paramref name="extension"/>'s link leads to (<see cref="RoomCollisionBlock.ExtensionLink"/>), or the
    /// edge-solid block a probe outside the room reads when it leads outside the room's grid, as the grapple's extension
    /// links reach it.
    /// </summary>
    /// <param name="extension">An extension block the read found in the grid.</param>
    internal RoomCollisionBlock LinkedBlockOrEdgeSolid(RoomCollisionBlock extension) =>
        GridBlockStepped(extension.At, extension.ExtensionLink) is { } reached ? GetCollisionBlock(reached) : EdgeSolid();

    /// <summary>Reads a room object's block, inside the grid or in the terrain beyond the room.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the terrain beyond the room.</exception>
    internal RoomCollisionBlock GetObjectCollisionBlock(Position16<RoomBlocks> block) =>
        ObjectCollisionBlockAt(Size.RowMajorIndex(block));

    /// <summary>
    /// A room object's terrain write at its block, showing the terrain's own picture. Writes inside the grid reach
    /// collision and the display; writes past it persist in the terrain beyond the room.
    /// </summary>
    internal void SetObjectTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain) =>
        WriteObjectTerrain(Size.RowMajorIndex(block), terrain, terrain.Appearance);

    /// <summary>
    /// A room object's behaviour write at its block, inside the grid or in the terrain beyond the room. A block of the grid
    /// no longer leads where its earlier behaviour did.
    /// </summary>
    internal void SetObjectBehavior(Position16<RoomBlocks> block, RoomBlockBehavior behavior)
    {
        int place = Size.RowMajorIndex(block);
        if (place >= Terrain.Length)
        {
            BeyondRoom.SetBehavior(BeyondPlace(place), behavior);
            return;
        }

        WriteBehavior(place, behavior);
    }

    /// <summary>
    /// Makes a block of the grid lead through <paramref name="exit"/> as a door block, as Mother Brain's escape door is set
    /// up (#2090); the block's collision kind and behaviour are written first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    internal void SetDoorExit(Position16<RoomBlocks> block, DoorBlockExit exit) => Exits[GridPlace(block)] = exit;

    /// <summary>
    /// Shows <paramref name="picture"/> at a foreground block over its terrain's appearance, as a collectible showing its
    /// own picture does where it lies (#1538), until a write changes what the block shows. Past the grid nothing is shown,
    /// as for a room object's terrain write there.
    /// </summary>
    internal void ShowCollectible(Position16<RoomBlocks> block, CollectiblePicture picture)
    {
        int place = Size.RowMajorIndex(block);
        if (place >= Terrain.Length)
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
    internal void SetTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain) =>
        WriteObjectTerrain(GridPlace(block), terrain, terrain.Appearance);

    /// <summary>
    /// Replaces the behaviour of one foreground block, which no longer leads where its earlier behaviour did; the block must
    /// lie in the grid.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    internal void SetBehavior(Position16<RoomBlocks> block, RoomBlockBehavior behavior) =>
        WriteBehavior(GridPlace(block), behavior);

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
        int beyond = Size.RowMajorIndex(block) - Terrain.Length;
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
        Size.Contains(block) ? GetCollisionBlock(block) : EdgeSolid();

    /// <summary>Reads the block containing a room pixel: sixteen pixels to a block.</summary>
    public RoomCollisionBlock GetCollisionBlockAtPixel(Position16<RoomPixels> position) =>
        GetCollisionBlock(position.ToRoomBlock());

    /// <summary>
    /// Finds where a door block of the grid Samus's collision met leads, then publishes the normal-door transition request
    /// for the top-level game-state loop.
    /// </summary>
    /// <param name="doorBlock">The door block collision found in the grid.</param>
    /// <param name="samusPose">Samus's collision pose, which decides whether the elevator pseudo-door arms the elevator.</param>
    /// <param name="publishDoorSideEffects">Whether the transition request or elevator contact is published.</param>
    /// <exception cref="InvalidDataException">
    /// The block leads nowhere: neither the installed level nor a room object's write made it lead through a door.
    /// </exception>
    public DoorBlockExit ResolveDoorCollision(
        RoomCollisionBlock doorBlock,
        SamusCollisionPose samusPose,
        bool publishDoorSideEffects)
    {
        // Elevator and physical door blocks are found alike before the destination decides whether collision is solid or
        // starts the door transition.
        Position16<RoomBlocks> block = doorBlock.At;
        DoorBlockExit entry = Exits[GridPlace(block)] ?? throw new InvalidDataException($"Door block {block} leads nowhere.");
        // The door reaction normally publishes either a real transition or the elevator-contact flag. The runtime
        // collapses the outer transition states into one room-load update, however, so destination-side elevator return
        // scans must inspect the same door block without republishing either side effect. Keep the door lookup and
        // solid/passable classification intact while gating only the writes.
        if (publishDoorSideEffects && entry.Door is { } door)
            PendingDoorTransition ??= door;
        // The pseudo destination is solid for every pose, but publishes the elevator contact only for standing poses.
        // This prevents running, aerial, morph, and damage poses that merely brush the block from arming the actor.
        else if (publishDoorSideEffects && entry.IsElevatorPseudoDoor &&
                 samusPose is SamusCollisionPose.StandingNormal or SamusCollisionPose.StandingOther)
            ElevatorDoorContactPending = true;
        return entry;
    }

    /// <summary>Consumes the pending door transition once.</summary>
    public DoorDefinition? ConsumePendingDoorTransition()
    {
        DoorDefinition? pending = PendingDoorTransition;
        PendingDoorTransition = null;
        return pending;
    }

    /// <summary>Consumes the one-frame elevator pseudo-door collision publication.</summary>
    public bool ConsumeElevatorDoorContact()
    {
        bool pending = ElevatorDoorContactPending;
        ElevatorDoorContactPending = false;
        return pending;
    }

    /// <summary>
    /// The place of <paramref name="block"/>, which must be a block of the room's grid, in the row-by-row planes that hold
    /// the grid. Coordinates outside the room fail loudly.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies outside the room's width or height.</exception>
    private int GridPlace(Position16<RoomBlocks> block)
    {
        if (block.X >= Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block X {block.X} lies outside the {Size.X}-block room width.");
        if (block.Y >= Size.Y)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block Y {block.Y} lies outside the {Size.Y}-block room height.");
        return Size.RowMajorIndex(block);
    }

    /// <summary>Writes the behaviour of the grid's block at <paramref name="place"/>, which no longer leads where it did.</summary>
    private void WriteBehavior(int place, RoomBlockBehavior behavior)
    {
        Behaviors[place] = behavior;
        Exits[place] = null;
    }

    /// <summary>The block of the grid at <paramref name="place"/> in its planes, as collision reads it.</summary>
    private RoomCollisionBlock GridCollisionBlockAt(int place) =>
        new(Size.CellAtRowMajorIndex(place), Terrain[place], Behaviors[place]);

    /// <summary>A room object's block at <paramref name="place"/> in the level space, inside the grid or beyond it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is past the terrain beyond the room.</exception>
    private RoomCollisionBlock ObjectCollisionBlockAt(int place)
    {
        if (place < Terrain.Length)
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
        if (place >= Terrain.Length)
        {
            BeyondRoom.SetTerrain(BeyondPlace(place), block);
            return;
        }

        Terrain[place] = block;
        if (ShownForeground[place] != shown)
            shownCollectibles.Remove(place);
        ShownForeground[place] = shown;
        PublishedForeground = null;
    }

    /// <summary>The block a collision probe outside the room reads.</summary>
    private RoomCollisionBlock EdgeSolid() => new(Position: null, BeyondRoom.EdgeSolid, BeyondRoom.NoBehavior);

    /// <summary>A place in the level space past the grid, counted from the first block past it.</summary>
    private int BeyondPlace(int place) => place - Terrain.Length;
}
