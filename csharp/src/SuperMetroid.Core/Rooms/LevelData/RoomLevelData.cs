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
    /// <param name="doors">Optional door list; null is suitable only for fixtures that never resolve a door block.</param>
    /// <param name="visualLayout">Optional matching-sized initial appearances of both planes; terrain, behaviours and the terrain beyond the room stay unchanged.</param>
    /// <exception cref="ArgumentOutOfRangeException">Width or height is nonpositive.</exception>
    /// <exception cref="ArgumentException">A plane has the wrong size.</exception>
    /// <exception cref="InvalidDataException">The selected visual layout does not match the room dimensions.</exception>
    public RoomLevelData(
        Extent16<RoomBlocks> size,
        RoomLevelPlanes planes,
        TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior> beyondRoom,
        IReadOnlyList<DoorListEntry>? doors = null,
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
        Doors = doors;
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
    /// Room size in 16-pixel blocks. The width is every row's index stride; the height comes from the room's level, which
    /// may hold more rows than the room's screens show.
    /// </summary>
    public Extent16<RoomBlocks> Size { get; }

    /// <summary>
    /// The entries this room's door blocks select by index. Synthetic collision fixtures deliberately leave it null;
    /// room loading always supplies it.
    /// </summary>
    public IReadOnlyList<DoorListEntry>? Doors { get; }

    /// <summary>The door definition selected by the most recent door-block collision, until the transition consumes it.</summary>
    public DoorDefinition? PendingDoorTransition { get; internal set; }

    /// <summary>
    /// Set when a door block resolves to the elevator's pseudo door for a standing pose. Enemies update before Samus's
    /// collision, so the elevator consumes this publication at the beginning of the following gameplay frame.
    /// </summary>
    public bool ElevatorDoorContactPending { get; internal set; }

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
    /// The block a room object placed at <paramref name="placed"/> sits on, as its own position: a placement whose column
    /// lies past the room's width is the block that many columns into the following rows, as the two shipped off-room
    /// coloured doors are placed, and a row past the room's height lands in the terrain beyond the room.
    /// </summary>
    /// <remarks>
    /// A room object that requires a block of the grid uses <see cref="GridBlock"/> and fails at that point; setup and
    /// draws reach the terrain beyond the room through this one, as the two shipped off-room coloured doors do.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The block lies past the terrain beyond the room.</exception>
    internal Position16<RoomBlocks> ObjectBlock(Position16<RoomBlocks> placed) =>
        Size.CellAtRowMajorIndex(ObjectPlace(placed));

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
    /// A room object's terrain write at its block that shows <paramref name="shown"/> over the terrain instead of the
    /// terrain's own picture. Past the grid nothing is shown, so only the terrain persists.
    /// </summary>
    internal void SetObjectTerrain(Position16<RoomBlocks> block, RoomTerrainBlock terrain, RoomBlockAppearance shown) =>
        WriteObjectTerrain(Size.RowMajorIndex(block), terrain, shown);

    /// <summary>A room object's behaviour write at its block, inside the grid or in the terrain beyond the room.</summary>
    internal void SetObjectBehavior(Position16<RoomBlocks> block, RoomBlockBehavior behavior)
    {
        int place = Size.RowMajorIndex(block);
        if (place >= Terrain.Length)
        {
            BeyondRoom.SetBehavior(BeyondPlace(place), behavior);
            return;
        }

        Behaviors[place] = behavior;
    }

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

    /// <summary>Replaces the behaviour of one foreground block; the block must lie in the grid.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is outside the grid.</exception>
    internal void SetBehavior(Position16<RoomBlocks> block, RoomBlockBehavior behavior) =>
        Behaviors[GridPlace(block)] = behavior;

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
    /// Resolves a door block's behaviour through the active room's door list, then publishes the normal-door transition
    /// request for the top-level game-state loop.
    /// </summary>
    public DoorListEntry ResolveDoorCollision(
        RoomBlockBehavior behavior,
        SamusCollisionPose samusPose,
        bool publishDoorSideEffects = true)
    {
        if (Doors is not { } doors)
        {
            // Every installed room supplies its door list. Its nullable form exists only so focused synthetic collision
            // fixtures need not fabricate door metadata when they contain no door blocks. Reaching a door in such a
            // fixture is therefore an invalid host composition, not missing door behavior.
            throw new InvalidOperationException(
                "Door collision requires the room's door list; this level has none.");
        }

        // Elevator and special-door behaviours share the door list lookup before the destination
        // decides whether collision is solid or starts the door transition.
        int index = behavior.DoorLink;
        if (index >= doors.Count)
            throw new InvalidDataException($"Door index {index} is outside the room's {doors.Count}-entry door list.");
        DoorListEntry entry = doors[index];
        // The door reaction normally publishes either a real transition or the elevator-contact flag. The runtime
        // collapses the outer transition states into one room-load call, however, so destination-side elevator return
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

    /// <summary>
    /// The place of a room object's block in the room's level space, the grid's planes and then the terrain beyond the
    /// room, without the room's width or height bounds: a column past the width runs into the next row, and a row past
    /// the height lands in the terrain beyond the room.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block lies past the terrain beyond the room.</exception>
    private int ObjectPlace(Position16<RoomBlocks> block)
    {
        int place = Size.RowMajorIndex(block);
        if (place >= Terrain.Length + BeyondRoom.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(block),
                $"Room-object block ({block.X},{block.Y}) lies at place {place}, past the room's " +
                $"{Terrain.Length}-block grid and the {BeyondRoom.Count} blocks beyond it.");
        }

        return place;
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
    private void WriteObjectTerrain(int place, RoomTerrainBlock block, RoomBlockAppearance shown)
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
