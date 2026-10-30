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

    /// <summary>What each background block shows, row by row; nothing changes it after loading.</summary>
    private readonly RoomBlockAppearance[] background;

    /// <summary>The terrain past the grid, which room objects and probes past the room's last block reach.</summary>
    internal readonly TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior> BeyondRoom;

    /// <summary>The foreground plane last published for display, until the next visual write; derived, never saved.</summary>
    [field: NonSerialized]
    private RoomLevelPlane? PublishedForeground { get; set; }

    /// <summary>Copies a room's grid; an installed layout changes only what the blocks initially show.</summary>
    /// <param name="size">Room size in 16-pixel blocks: the width is the row-major stride, 1..255, and the height is positive.</param>
    /// <param name="planes">The foreground terrain, behaviour and background appearances, each exactly width times height entries in row-major order.</param>
    /// <param name="doors">Optional door list; null is suitable only for fixtures that never resolve a door block.</param>
    /// <param name="visualLayout">Optional matching-sized initial appearances of both planes; terrain, behaviours and the terrain beyond the room stay unchanged.</param>
    /// <param name="beyondRoom">
    /// The terrain past the grid, built for this room's lifetime from its installed level; a fixture that never reaches
    /// past its grid leaves it out, and has none.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Width is outside 1..255 or height is nonpositive.</exception>
    /// <exception cref="ArgumentException">A plane has the wrong size.</exception>
    /// <exception cref="InvalidDataException">The selected visual layout does not match the room dimensions.</exception>
    public RoomLevelData(
        Extent16<RoomBlocks> size,
        RoomLevelPlanes planes,
        IReadOnlyList<DoorListEntry>? doors = null,
        RoomLevelArtwork? visualLayout = null,
        TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior>? beyondRoom = null)
    {
        if (size.X is <= 0 or > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(size), "Room width must be 1..255 blocks.");
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
        beyondRoom ??= new TerrainBeyondRoom<RoomTerrainBlock, RoomBlockBehavior>(
            [], 0, RoomTerrainBlocks.EdgeSolid, RoomBlockBehaviorValues.None);
        BeyondRoom = beyondRoom;

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
    public RoomLevelPlane ForegroundPlane => PublishedForeground ??= new(Size, ShownForeground);

    /// <summary>The background plane as the room shows it; nothing changes the background after loading, so it is built once.</summary>
    [field: NonSerialized]
    public RoomLevelPlane BackgroundPlane => field ??= new(Size, background);

    /// <summary>Converts a block coordinate to its row-major index. Coordinates outside the room fail loudly.</summary>
    public int GetBlockIndex(Position16<RoomBlocks> block)
    {
        if (block.X >= (uint)Size.X)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block X {block.X} lies outside the {Size.X}-block room width.");
        if (block.Y >= (uint)Size.Y)
            throw new ArgumentOutOfRangeException(nameof(block), $"Block Y {block.Y} lies outside the {Size.Y}-block room height.");
        return Size.RowMajorIndex(block);
    }

    /// <summary>
    /// The row-major index of a room object's block, without the room's width or height bounds: a column past the
    /// width runs into the next row, and a row past the height lands in the terrain beyond the room.
    /// </summary>
    /// <remarks>
    /// This is intentionally distinct from <see cref="GetBlockIndex"/>. A room object that requires a block of the grid
    /// still uses the strict index and fails at that point; setup and draws reach the terrain beyond the room through
    /// this one, as the two shipped off-room coloured doors do.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">A coordinate is negative, or the block lies past the terrain beyond the room.</exception>
    internal int GetPlmBlockIndex(Position16<RoomBlocks> block)
    {
        if (block.X < 0 || block.Y < 0)
            throw new ArgumentOutOfRangeException(nameof(block), $"PLM block {block} has a negative coordinate.");
        int blockIndex = checked(block.Y * Size.X + block.X);
        if ((uint)blockIndex >= (uint)(Terrain.Length + BeyondRoom.Count))
        {
            throw new ArgumentOutOfRangeException(
                nameof(block),
                $"PLM block ({block.X},{block.Y}) resolves to index {blockIndex}, past the room's " +
                $"{Terrain.Length}-block grid and the {BeyondRoom.Count} blocks beyond it.");
        }

        return blockIndex;
    }

    /// <summary>A block past the room's grid, as a room object or enemy terrain read finds it.</summary>
    /// <param name="blockIndex">A row-major index past the grid.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is inside the grid.</exception>
    /// <exception cref="NotSupportedException">The index is past the terrain beyond the room.</exception>
    internal RoomCollisionBlock GetBlockBeyondRoom(int blockIndex)
    {
        if (IsLogicalBlockIndex(blockIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex), "The index is inside the room's grid.");
        int beyond = blockIndex - Terrain.Length;
        if (beyond < 0 || beyond >= BeyondRoom.Count)
        {
            throw new NotSupportedException(
                $"Block {blockIndex} lies past the room's grid and the {BeyondRoom.Count} blocks beyond it.");
        }
        return new RoomCollisionBlock(blockIndex, BeyondRoom.TerrainAt(beyond), BeyondRoom.BehaviorAt(beyond));
    }

    /// <summary>Whether a room object's block index belongs to the room's grid.</summary>
    internal bool IsLogicalBlockIndex(int blockIndex) =>
        (uint)blockIndex < (uint)Terrain.Length;

    /// <summary>Reads a room object's block, inside the grid or in the terrain beyond the room.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block is past the terrain beyond the room.</exception>
    internal RoomCollisionBlock GetPlmCollisionBlockByIndex(int blockIndex)
    {
        if (IsLogicalBlockIndex(blockIndex))
            return GetCollisionBlockByIndex(blockIndex);
        int beyond = BeyondIndex(blockIndex);
        return new RoomCollisionBlock(blockIndex, BeyondRoom.TerrainAt(beyond), BeyondRoom.BehaviorAt(beyond));
    }

    /// <summary>
    /// A room object's terrain write, showing the terrain's own picture. Writes inside the grid reach collision and the
    /// display; writes past it persist in the terrain beyond the room.
    /// </summary>
    internal void SetPlmTerrain(int blockIndex, RoomTerrainBlock block) =>
        SetPlmTerrain(blockIndex, block, block.Appearance);

    /// <summary>
    /// A room object's terrain write that shows <paramref name="shown"/> over the terrain instead of the terrain's own
    /// picture. Past the grid nothing is shown, so only the terrain persists.
    /// </summary>
    internal void SetPlmTerrain(int blockIndex, RoomTerrainBlock block, RoomBlockAppearance shown)
    {
        if (!IsLogicalBlockIndex(blockIndex))
        {
            BeyondRoom.SetTerrain(BeyondIndex(blockIndex), block);
            return;
        }

        Terrain[blockIndex] = block;
        ShownForeground[blockIndex] = shown;
        PublishedForeground = null;
    }

    /// <summary>A room object's behaviour write, inside the grid or in the terrain beyond the room.</summary>
    internal void SetPlmBehavior(int blockIndex, RoomBlockBehavior behavior)
    {
        if (!IsLogicalBlockIndex(blockIndex))
        {
            BeyondRoom.SetBehavior(BeyondIndex(blockIndex), behavior);
            return;
        }

        Behaviors[blockIndex] = behavior;
    }

    /// <summary>Returns the terrain and behaviour collision reads for one block of the grid.</summary>
    public RoomCollisionBlock GetCollisionBlock(Position16<RoomBlocks> block)
    {
        int index = GetBlockIndex(block);
        return GetCollisionBlockByIndex(index);
    }

    /// <summary>
    /// Reads a block of the grid, or the edge-solid block when a collision probe asks about a position outside the
    /// room's width or height (<see cref="TerrainBeyondRoom{TTerrain, TBehavior}.EdgeSolid"/>).
    /// </summary>
    internal RoomCollisionBlock GetCollisionBlockOrEdgeSolid(Position16<RoomBlocks> block) =>
        Size.Contains(block) ? GetCollisionBlock(block) : EdgeSolid();

    /// <summary>
    /// Applies the same edge rule after a collision extension has already reduced its target to a row-major index.
    /// </summary>
    internal RoomCollisionBlock GetCollisionBlockByIndexOrEdgeSolid(int blockIndex) =>
        IsLogicalBlockIndex(blockIndex) ? GetCollisionBlockByIndex(blockIndex) : EdgeSolid();

    /// <summary>Reads a block of the grid by its row-major index, as collision extensions reach it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the grid.</exception>
    public RoomCollisionBlock GetCollisionBlockByIndex(int blockIndex)
    {
        if (!IsLogicalBlockIndex(blockIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return new RoomCollisionBlock(blockIndex, Terrain[blockIndex], Behaviors[blockIndex]);
    }

    /// <summary>Reads the block containing a room pixel: sixteen pixels to a block.</summary>
    public RoomCollisionBlock GetCollisionBlockAtPixel(Position16<RoomPixels> position) =>
        GetCollisionBlock(position.ToRoomBlock());

    /// <summary>
    /// Resolves a door block's behaviour through the active room's door list, then publishes the normal-door transition
    /// request for the top-level dispatcher.
    /// </summary>
    public DoorListEntry ResolveDoorCollision(
        int behavior,
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
        int index = new RoomBlockBehavior(behavior).DoorListIndex;
        if (index >= doors.Count)
            throw new InvalidDataException($"Door index {index} is outside the room's {doors.Count}-entry door list.");
        DoorListEntry entry = doors[index];
        // The door handler normally publishes either a real transition or the elevator-contact flag. The runtime
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
    /// Replaces one foreground block's terrain, collision kind and picture together, and shows its picture.
    /// </summary>
    /// <remarks>
    /// Room-object draws write the whole block: breakable grapple frames alternate between grapple terrain and air, so
    /// a narrower "change the picture" helper would be incorrect here.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the grid.</exception>
    public void SetTerrain(int blockIndex, RoomTerrainBlock block)
    {
        if (!IsLogicalBlockIndex(blockIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        Terrain[blockIndex] = block;
        ShownForeground[blockIndex] = block.Appearance;
        PublishedForeground = null;
    }

    /// <summary>Replaces the behaviour of one foreground block.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the grid.</exception>
    public void SetBehavior(int blockIndex, RoomBlockBehavior behavior)
    {
        if (!IsLogicalBlockIndex(blockIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        Behaviors[blockIndex] = behavior;
    }

    /// <summary>The block a collision probe outside the room reads.</summary>
    private RoomCollisionBlock EdgeSolid() => new(Index: -1, BeyondRoom.EdgeSolid, BeyondRoom.NoBehavior);

    /// <summary>A row-major index past the grid, counted from the first block past it.</summary>
    private int BeyondIndex(int blockIndex) =>
        blockIndex >= Terrain.Length
            ? blockIndex - Terrain.Length
            : throw new ArgumentOutOfRangeException(nameof(blockIndex), blockIndex, "A room object's block index is negative.");
}
