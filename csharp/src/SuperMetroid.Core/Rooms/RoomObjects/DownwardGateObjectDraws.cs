using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each downward gate draw is described (#1862, #1994): the column open, lowering a row at a time and closed, five
/// blocks tall, and four trigger colours on either side. What each frame shows is its installed picture (#1557).
/// </summary>
internal static partial class DownwardGateObjectDraws
{
    /// <summary>The part and shape of one of the column's draws or one of the eight trigger draws.</summary>
    internal static Draw Describe(DownwardGateDraw gate) => new(PartOf(gate), gate);

    /// <summary>The gate's column and trigger frames (#2034), pictured by the installed downward gate frames.</summary>
    internal static RoomObjectDrawFamily<DownwardGateDraw> Family { get; } =
        new RoomObjectDrawFamily<DownwardGateDraw>("downward gate",
            gate => Describe(gate).Blocks);

    /// <summary>The part of the gate <paramref name="gate"/> writes.</summary>
    /// <exception cref="InvalidOperationException">The draw is undefined.</exception>
    private static DownwardGatePart PartOf(DownwardGateDraw gate) => gate switch
    {
        DownwardGateDraw.Open or DownwardGateDraw.QuarterLowered or DownwardGateDraw.HalfLowered or DownwardGateDraw.ThreeQuartersLowered
            or DownwardGateDraw.FullyLowered or DownwardGateDraw.Closed => DownwardGatePart.Column,
        DownwardGateDraw.TriggerBlueLeft or DownwardGateDraw.TriggerRedLeft or DownwardGateDraw.TriggerGreenLeft
            or DownwardGateDraw.TriggerYellowLeft => DownwardGatePart.LeftTrigger,
        DownwardGateDraw.TriggerBlueRight or DownwardGateDraw.TriggerRedRight or DownwardGateDraw.TriggerGreenRight
            or DownwardGateDraw.TriggerYellowRight => DownwardGatePart.RightTrigger,
        _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGateDraw)} {gate}."),
    };

    /// <summary>
    /// The rows of the column <paramref name="gate"/> fills, down from its top block: the open gate its top alone, each step
    /// of its lowering one row more, the fully lowered and closed gate all five.
    /// </summary>
    /// <exception cref="InvalidOperationException">The draw is not one of the column's.</exception>
    private static int FilledRows(DownwardGateDraw gate) => gate switch
    {
        DownwardGateDraw.Open => 1,
        DownwardGateDraw.QuarterLowered => 2,
        DownwardGateDraw.HalfLowered => 3,
        DownwardGateDraw.ThreeQuartersLowered => 4,
        DownwardGateDraw.FullyLowered or DownwardGateDraw.Closed => DownwardGateDefinitions.GateHeightInBlocks,
        DownwardGateDraw.TriggerBlueLeft or DownwardGateDraw.TriggerBlueRight or DownwardGateDraw.TriggerRedLeft or DownwardGateDraw.TriggerRedRight
            or DownwardGateDraw.TriggerGreenLeft or DownwardGateDraw.TriggerGreenRight or DownwardGateDraw.TriggerYellowLeft
            or DownwardGateDraw.TriggerYellowRight => throw new InvalidOperationException($"Downward gate draw {gate} is not one of the column's."),
        _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGateDraw)} {gate}."),
    };

    /// <summary>A right trigger: its solid block and the shootable trigger right of it.</summary>
    private static readonly RoomBlockShape RightTrigger = new(
        [new(new(0, 0), RoomCollisionType.SolidBlock), new(new(1, 0), RoomCollisionType.ShootableBlock)]);

    /// <summary>A left trigger: its solid block and the shootable trigger left of it.</summary>
    private static readonly RoomBlockShape LeftTrigger = new(
        [new(new(0, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.ShootableBlock)]);

    /// <summary>The column filling each count of rows, by the count: <see cref="DownwardGateDefinitions.GateHeightInBlocks"/> down from its top block, the filled rows shootable and the rest air.</summary>
    private static readonly RoomBlockShape[] Columns = [.. Enumerable.Range(0, DownwardGateDefinitions.GateHeightInBlocks + 1).Select(Column)];

    /// <summary>The column filling <paramref name="rows"/> rows: <see cref="DownwardGateDefinitions.GateHeightInBlocks"/> down from its top block, the filled rows shootable and the rest air.</summary>
    private static RoomBlockShape Column(int rows) => new(
        [.. Enumerable.Range(0, DownwardGateDefinitions.GateHeightInBlocks).Select(row => new RoomBlockShapeCell(new(0, row),
            row < rows ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air))]);

    /// <summary>One gate draw: the part it writes and which of the gate's draws it is.</summary>
    /// <param name="Part">The column or the trigger on one side.</param>
    /// <param name="Gate">The draw.</param>
    internal readonly partial record struct Draw(DownwardGatePart Part, DownwardGateDraw Gate)
    {

        /// <summary>
        /// The blocks the draw changes and how each collides: the column's shootable top and filled rows, or the trigger's
        /// solid block and its shootable trigger beside it.
        /// </summary>
        internal RoomBlockShape Blocks => Part switch
        {
            DownwardGatePart.Column => Columns[FilledRows(Gate)],
            DownwardGatePart.RightTrigger => RightTrigger,
            DownwardGatePart.LeftTrigger => LeftTrigger,
            _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGatePart)} {Part}."),
        };
    }
}
