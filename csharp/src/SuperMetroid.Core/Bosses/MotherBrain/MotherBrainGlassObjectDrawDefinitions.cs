using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The block shape of each of the eleven frames of Mother Brain's glass (#1608): the cells each frame covers around the
/// glass's origin block, which sits at the glass's right edge with the pane reaching up to three blocks left of it and
/// three below, and how each cell collides. The glass's typed object owns damage thresholds, shards and timing, and what
/// each frame shows is its installed picture (#1557).
/// </summary>
internal static class MotherBrainGlassObjectDrawDefinitions
{
    /// <summary>The intact glass: the shootable glass block alone.</summary>
    private static readonly RoomBlockShape Intact = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
    ]);

    /// <summary>
    /// A damaged pane: the shootable glass block over two vertical extensions and a horizontal extension at the foot of its
    /// column, and two vertical extensions in the middle of the column to its left.
    /// </summary>
    private static readonly RoomBlockShape DamagedPane = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(0, 1), RoomCollisionType.VerticalExtension),
        new(new(0, 2), RoomCollisionType.VerticalExtension),
        new(new(0, 3), RoomCollisionType.HorizontalExtension),
        new(new(-1, 1), RoomCollisionType.VerticalExtension),
        new(new(-1, 2), RoomCollisionType.VerticalExtension),
    ]);

    /// <summary>The pane giving way: the shootable glass block over two empty blocks.</summary>
    private static readonly RoomBlockShape GivingWayPane = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
    ]);

    /// <summary>
    /// The pane at its first shift: the shootable glass block, a solid four-block column three blocks left of it, and the
    /// two solid middle blocks of the column beside that.
    /// </summary>
    private static readonly RoomBlockShape FirstShiftedPane = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(-3, 0), RoomCollisionType.SolidBlock),
        new(new(-3, 1), RoomCollisionType.SolidBlock),
        new(new(-3, 2), RoomCollisionType.SolidBlock),
        new(new(-3, 3), RoomCollisionType.SolidBlock),
        new(new(-2, 1), RoomCollisionType.SolidBlock),
        new(new(-2, 2), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>
    /// The pane at its second shift: the shootable glass block, the lower three blocks of the column three blocks left of
    /// it solid, and the two solid middle blocks of the column beside that.
    /// </summary>
    private static readonly RoomBlockShape SecondShiftedPane = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(-3, 1), RoomCollisionType.SolidBlock),
        new(new(-3, 2), RoomCollisionType.SolidBlock),
        new(new(-3, 3), RoomCollisionType.SolidBlock),
        new(new(-2, 1), RoomCollisionType.SolidBlock),
        new(new(-2, 2), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The shifted pane emptying: the shootable glass block, and the two middle blocks of the far column empty.</summary>
    private static readonly RoomBlockShape EmptyingShiftedPane = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(-3, 1), RoomCollisionType.Air),
        new(new(-3, 2), RoomCollisionType.Air),
    ]);

    /// <summary>
    /// The glass starting to shatter: its right column is the shootable glass block over two empty blocks and a horizontal
    /// extension, its left column a solid block over two empty blocks and a solid foot, and the two middle columns' middle
    /// blocks are vertical extensions.
    /// </summary>
    private static readonly RoomBlockShape FirstShatter = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.HorizontalExtension),
        new(new(-3, 0), RoomCollisionType.SolidBlock),
        new(new(-3, 1), RoomCollisionType.Air),
        new(new(-3, 2), RoomCollisionType.Air),
        new(new(-3, 3), RoomCollisionType.SolidBlock),
        new(new(-2, 1), RoomCollisionType.VerticalExtension),
        new(new(-2, 2), RoomCollisionType.VerticalExtension),
        new(new(-1, 1), RoomCollisionType.VerticalExtension),
        new(new(-1, 2), RoomCollisionType.VerticalExtension),
    ]);

    /// <summary>
    /// The glass further shattered: its outer columns as at the first shatter, and each middle column a horizontal
    /// extension over three vertical extensions.
    /// </summary>
    private static readonly RoomBlockShape SecondShatter = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock),
        new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air),
        new(new(0, 3), RoomCollisionType.HorizontalExtension),
        new(new(-3, 0), RoomCollisionType.SolidBlock),
        new(new(-3, 1), RoomCollisionType.Air),
        new(new(-3, 2), RoomCollisionType.Air),
        new(new(-3, 3), RoomCollisionType.SolidBlock),
        new(new(-2, 0), RoomCollisionType.HorizontalExtension),
        new(new(-2, 1), RoomCollisionType.VerticalExtension),
        new(new(-2, 2), RoomCollisionType.VerticalExtension),
        new(new(-2, 3), RoomCollisionType.VerticalExtension),
        new(new(-1, 0), RoomCollisionType.HorizontalExtension),
        new(new(-1, 1), RoomCollisionType.VerticalExtension),
        new(new(-1, 2), RoomCollisionType.VerticalExtension),
        new(new(-1, 3), RoomCollisionType.VerticalExtension),
    ]);

    /// <summary>The glass gone: its whole four-by-four square, from three blocks left of the origin, empty.</summary>
    private static readonly RoomBlockShape Gone = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(0, 1), RoomCollisionType.Air),
        new(new(0, 2), RoomCollisionType.Air), new(new(0, 3), RoomCollisionType.Air),
        new(new(-3, 0), RoomCollisionType.Air), new(new(-3, 1), RoomCollisionType.Air),
        new(new(-3, 2), RoomCollisionType.Air), new(new(-3, 3), RoomCollisionType.Air),
        new(new(-2, 0), RoomCollisionType.Air), new(new(-2, 1), RoomCollisionType.Air),
        new(new(-2, 2), RoomCollisionType.Air), new(new(-2, 3), RoomCollisionType.Air),
        new(new(-1, 0), RoomCollisionType.Air), new(new(-1, 1), RoomCollisionType.Air),
        new(new(-1, 2), RoomCollisionType.Air), new(new(-1, 3), RoomCollisionType.Air),
    ]);

    /// <summary>The block shape each of the eleven glass frames draws.</summary>
    internal static readonly Dictionary<MotherBrainGlassDraw, RoomBlockShape> Shapes = new()
    {
        [MotherBrainGlassDraw.Initial] = Intact,
        [MotherBrainGlassDraw.PaneDamage1] = DamagedPane,
        [MotherBrainGlassDraw.PaneDamage2] = DamagedPane,
        [MotherBrainGlassDraw.PaneTransition] = GivingWayPane,
        [MotherBrainGlassDraw.ShiftedPane1] = FirstShiftedPane,
        [MotherBrainGlassDraw.ShiftedPane2] = SecondShiftedPane,
        [MotherBrainGlassDraw.ShiftedPane3] = EmptyingShiftedPane,
        [MotherBrainGlassDraw.Shatter1] = FirstShatter,
        [MotherBrainGlassDraw.Shatter2] = SecondShatter,
        [MotherBrainGlassDraw.Shatter3] = Gone,
        [MotherBrainGlassDraw.Cleared] = Gone,
    };
}
