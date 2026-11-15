using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One family of linked block restorations: bomb blocks or Samus-contact crumble blocks. Each family has three
/// restoration draws, horizontal, vertical and square, that rebuild a parent block and its linked children. The families
/// differ only in the parent's collision kind; the horizontal- and vertical-extension children and their links are the
/// same. What each restoration shows is its installed picture (#1557); what it changes is its block shape (#1862). Each
/// family's draws are drawn by their own draw family (#2034).
/// </summary>
internal sealed class LinkedRestoreObjectFamily
{
    /// <summary>How a bomb-block restoration's parent collides.</summary>
    private const RoomCollisionType BombBlockParent = RoomCollisionType.BombableBlock;

    /// <summary>How a contact crumble restoration's parent collides.</summary>
    private const RoomCollisionType ContactCrumbleParent = RoomCollisionType.SpecialBlock;

    /// <summary>The blocks linked to the parent in each restoration: the one right of it, below it, or both and the one diagonally between.</summary>
    private static readonly Dictionary<RoomObjectRestoreShape, Offset16<RoomBlocks>[]> Children = new()
    {
        [RoomObjectRestoreShape.Horizontal] = [new(1, 0)],
        [RoomObjectRestoreShape.Vertical] = [new(0, 1)],
        [RoomObjectRestoreShape.Square] = [new(1, 0), new(0, 1), new(1, 1)],
    };

    /// <summary>The bomb blocks' restorations, pictured by the installed linked restorations.</summary>
    internal static RoomObjectDrawFamily<BombBlockRestoreDraw> BombBlockDraws { get; } =
        RoomObjectDrawFamily<BombBlockRestoreDraw>.Pictured<InstalledRoomObjectLinkedRestoreVisuals>("linked restoration",
            frame => RoomBlockShapes.Linked(BombBlockParent, Children[ShapeOf(frame)]), (visuals, draw) => visuals.Image(draw));

    /// <summary>The contact crumble blocks' restorations, pictured by the installed linked restorations.</summary>
    internal static RoomObjectDrawFamily<ContactCrumbleRestoreDraw> ContactCrumbleDraws { get; } =
        RoomObjectDrawFamily<ContactCrumbleRestoreDraw>.Pictured<InstalledRoomObjectLinkedRestoreVisuals>("linked restoration",
            frame => RoomBlockShapes.Linked(ContactCrumbleParent, Children[ShapeOf(frame)]), (visuals, draw) => visuals.Image(draw));

    /// <summary>Linked bomb blocks: a bombable parent.</summary>
    internal static LinkedRestoreObjectFamily BombBlock { get; } = new(
        BombBlockParent, new Dictionary<RoomObjectRestoreShape, RoomObjectDraw>
        {
            [RoomObjectRestoreShape.Horizontal] = BombBlockDraws.Of(BombBlockRestoreDraw.Horizontal),
            [RoomObjectRestoreShape.Vertical] = BombBlockDraws.Of(BombBlockRestoreDraw.Vertical),
            [RoomObjectRestoreShape.Square] = BombBlockDraws.Of(BombBlockRestoreDraw.Square),
        });

    /// <summary>Linked Samus-contact crumble blocks: a special-block parent.</summary>
    internal static LinkedRestoreObjectFamily ContactCrumble { get; } = new(
        ContactCrumbleParent, new Dictionary<RoomObjectRestoreShape, RoomObjectDraw>
        {
            [RoomObjectRestoreShape.Horizontal] = ContactCrumbleDraws.Of(ContactCrumbleRestoreDraw.Horizontal),
            [RoomObjectRestoreShape.Vertical] = ContactCrumbleDraws.Of(ContactCrumbleRestoreDraw.Vertical),
            [RoomObjectRestoreShape.Square] = ContactCrumbleDraws.Of(ContactCrumbleRestoreDraw.Square),
        });

    /// <summary>Both families, bomb blocks first, in their export order.</summary>
    internal static IReadOnlyList<LinkedRestoreObjectFamily> Families { get; } = [BombBlock, ContactCrumble];

    /// <summary>The block shape each restoration rebuilds: the family's parent with its children linked to it.</summary>
    private readonly Dictionary<RoomObjectRestoreShape, RoomBlockShape> _shapes;

    /// <summary>Defines a family by its parent collision kind and the restoration draw of each shape.</summary>
    private LinkedRestoreObjectFamily(RoomCollisionType parentCollision, IReadOnlyDictionary<RoomObjectRestoreShape, RoomObjectDraw> draws)
    {
        _shapes = Children.ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Linked(parentCollision, pair.Value));
        Draws = draws;
    }

    /// <summary>The restoration draw of each shape, horizontal, vertical and square.</summary>
    internal IReadOnlyDictionary<RoomObjectRestoreShape, RoomObjectDraw> Draws { get; }

    /// <summary>The blocks the family's restoration of <paramref name="shape"/> rebuilds and how each collides.</summary>
    internal RoomBlockShape Blocks(RoomObjectRestoreShape shape) => _shapes[shape];

    /// <summary>The family's restoration of <paramref name="shape"/>.</summary>
    internal Draw Describe(RoomObjectRestoreShape shape) => new(shape, this);

    /// <summary>The shape a bomb-block restoration rebuilds.</summary>
    private static RoomObjectRestoreShape ShapeOf(BombBlockRestoreDraw frame) => frame switch
    {
        BombBlockRestoreDraw.Horizontal => RoomObjectRestoreShape.Horizontal,
        BombBlockRestoreDraw.Vertical => RoomObjectRestoreShape.Vertical,
        BombBlockRestoreDraw.Square => RoomObjectRestoreShape.Square,
        _ => throw new InvalidOperationException($"Undefined {nameof(BombBlockRestoreDraw)} {frame}."),
    };

    /// <summary>The shape a contact crumble restoration rebuilds.</summary>
    private static RoomObjectRestoreShape ShapeOf(ContactCrumbleRestoreDraw frame) => frame switch
    {
        ContactCrumbleRestoreDraw.Horizontal => RoomObjectRestoreShape.Horizontal,
        ContactCrumbleRestoreDraw.Vertical => RoomObjectRestoreShape.Vertical,
        ContactCrumbleRestoreDraw.Square => RoomObjectRestoreShape.Square,
        _ => throw new InvalidOperationException($"Undefined {nameof(ContactCrumbleRestoreDraw)} {frame}."),
    };

    /// <summary>One restoration draw's geometry, from which its parent and linked children are reconstructed.</summary>
    /// <param name="Shape">Whether the blocks lie in a row, a column or a two-by-two square.</param>
    /// <param name="Family">The family whose parent collision kind the parent block carries.</param>
    internal readonly record struct Draw(RoomObjectRestoreShape Shape, LinkedRestoreObjectFamily Family)
    {
        /// <summary>
        /// The blocks the restoration rebuilds and how each collides: the top-left block carries the family's parent
        /// collision, and the others link to it.
        /// </summary>
        internal RoomBlockShape Blocks => Family.Blocks(Shape);
    }
}
