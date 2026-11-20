using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each downward gate draw is described (#1862, #1994): the column open, lowering a row at a time and closed, five
/// blocks tall, and four trigger colours on either side. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class DownwardGateObjectDraws
{
    /// <summary>The blocks of the gate's column, down from its top block.</summary>
    private const int ColumnHeight = 5;

    /// <summary>The part and shape of one of the column's draws or one of the eight trigger draws.</summary>
    internal static Draw Describe(DownwardGateDraw gate) => Draws[gate];

    /// <summary>The gate's column and trigger frames (#2034), pictured by the installed downward gate frames.</summary>
    internal static RoomObjectDrawFamily<DownwardGateDraw> Family { get; } =
        new RoomObjectDrawFamily<DownwardGateDraw>("downward gate",
            gate => Describe(gate).Blocks);

    /// <summary>A right trigger: its solid block and the shootable trigger right of it.</summary>
    private static readonly RoomBlockShape RightTrigger = new(
        [new(new(0, 0), RoomCollisionType.SolidBlock), new(new(1, 0), RoomCollisionType.ShootableBlock)]);

    /// <summary>A left trigger: its solid block and the shootable trigger left of it.</summary>
    private static readonly RoomBlockShape LeftTrigger = new(
        [new(new(0, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.ShootableBlock)]);

    /// <summary>
    /// Each draw (#2553): the column's, filling its rows down from its top block (the open gate its top alone, each step of
    /// its lowering one row more, the fully lowered and closed gate all five), and each trigger's, on its side.
    /// </summary>
    private static IdentityTable<DownwardGateDraw, Draw> Draws => field ??= new(new Dictionary<DownwardGateDraw, Draw>
    {
        [DownwardGateDraw.Open] = ColumnDraw(DownwardGateDraw.Open, 1),
        [DownwardGateDraw.QuarterLowered] = ColumnDraw(DownwardGateDraw.QuarterLowered, 2),
        [DownwardGateDraw.HalfLowered] = ColumnDraw(DownwardGateDraw.HalfLowered, 3),
        [DownwardGateDraw.ThreeQuartersLowered] = ColumnDraw(DownwardGateDraw.ThreeQuartersLowered, 4),
        [DownwardGateDraw.FullyLowered] = ColumnDraw(DownwardGateDraw.FullyLowered, ColumnHeight),
        [DownwardGateDraw.Closed] = ColumnDraw(DownwardGateDraw.Closed, ColumnHeight),
        [DownwardGateDraw.TriggerBlueLeft] = new(DownwardGatePart.LeftTrigger, DownwardGateDraw.TriggerBlueLeft, LeftTrigger),
        [DownwardGateDraw.TriggerBlueRight] = new(DownwardGatePart.RightTrigger, DownwardGateDraw.TriggerBlueRight, RightTrigger),
        [DownwardGateDraw.TriggerRedLeft] = new(DownwardGatePart.LeftTrigger, DownwardGateDraw.TriggerRedLeft, LeftTrigger),
        [DownwardGateDraw.TriggerRedRight] = new(DownwardGatePart.RightTrigger, DownwardGateDraw.TriggerRedRight, RightTrigger),
        [DownwardGateDraw.TriggerGreenLeft] = new(DownwardGatePart.LeftTrigger, DownwardGateDraw.TriggerGreenLeft, LeftTrigger),
        [DownwardGateDraw.TriggerGreenRight] = new(DownwardGatePart.RightTrigger, DownwardGateDraw.TriggerGreenRight, RightTrigger),
        [DownwardGateDraw.TriggerYellowLeft] = new(DownwardGatePart.LeftTrigger, DownwardGateDraw.TriggerYellowLeft, LeftTrigger),
        [DownwardGateDraw.TriggerYellowRight] = new(DownwardGatePart.RightTrigger, DownwardGateDraw.TriggerYellowRight, RightTrigger),
    });

    /// <summary>The column draw <paramref name="gate"/>, filling <paramref name="rows"/> rows.</summary>
    private static Draw ColumnDraw(DownwardGateDraw gate, int rows) => new(DownwardGatePart.Column, gate, Columns[rows]);

    /// <summary>The column filling each count of rows, by the count: <see cref="ColumnHeight"/> down from its top block, the filled rows shootable and the rest air.</summary>
    private static readonly RoomBlockShape[] Columns = [.. Enumerable.Range(0, ColumnHeight + 1).Select(Column)];

    /// <summary>The column filling <paramref name="rows"/> rows: <see cref="ColumnHeight"/> down from its top block, the filled rows shootable and the rest air.</summary>
    private static RoomBlockShape Column(int rows) => new(
        [.. Enumerable.Range(0, ColumnHeight).Select(row => new RoomBlockShapeCell(new(0, row),
            row < rows ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air))]);

    /// <summary>One gate draw: the part it writes, which of the gate's draws it is, and the blocks it changes.</summary>
    /// <param name="Part">The column or the trigger on one side.</param>
    /// <param name="Gate">The draw.</param>
    /// <param name="Blocks">
    /// The blocks the draw changes and how each collides: the column's shootable top and filled rows, or the trigger's
    /// solid block and its shootable trigger beside it.
    /// </param>
    internal readonly record struct Draw(DownwardGatePart Part, DownwardGateDraw Gate, RoomBlockShape Blocks)
    {
        /// <summary>Whether the draw is the gate column's.</summary>
        internal bool Column => Part == DownwardGatePart.Column;
    }
}
