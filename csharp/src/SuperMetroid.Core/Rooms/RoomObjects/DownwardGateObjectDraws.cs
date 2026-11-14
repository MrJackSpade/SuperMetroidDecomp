using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each downward gate draw is described (#1862): six five-block column frames that fill one more shootable row per
/// step, and four trigger colours on either side. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class DownwardGateObjectDraws
{
    /// <summary>The blocks of the gate's column, down from its top block.</summary>
    private const int ColumnHeight = 5;

    /// <summary>The shape of one of the six column stages or eight trigger draws; a trigger's frame is its color's picture step.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw)
    {
        if (request is not RoomObjectFrameDraw<DownwardGateDraw> { Frame: var gate })
        {
            draw = default;
            return false;
        }
        draw = gate switch
        {
            DownwardGateDraw.Open => new(DownwardGatePart.Column, 0),
            DownwardGateDraw.Lowered1 => new(DownwardGatePart.Column, 1),
            DownwardGateDraw.Lowered2 => new(DownwardGatePart.Column, 2),
            DownwardGateDraw.Lowered3 => new(DownwardGatePart.Column, 3),
            DownwardGateDraw.Lowered4 => new(DownwardGatePart.Column, 4),
            DownwardGateDraw.Closed => new(DownwardGatePart.Column, 5),
            DownwardGateDraw.TriggerBlueLeft => new(DownwardGatePart.LeftTrigger, 0),
            DownwardGateDraw.TriggerBlueRight => new(DownwardGatePart.RightTrigger, 0),
            DownwardGateDraw.TriggerRedLeft => new(DownwardGatePart.LeftTrigger, 1),
            DownwardGateDraw.TriggerRedRight => new(DownwardGatePart.RightTrigger, 1),
            DownwardGateDraw.TriggerGreenLeft => new(DownwardGatePart.LeftTrigger, 2),
            DownwardGateDraw.TriggerGreenRight => new(DownwardGatePart.RightTrigger, 2),
            DownwardGateDraw.TriggerYellowLeft => new(DownwardGatePart.LeftTrigger, 3),
            DownwardGateDraw.TriggerYellowRight => new(DownwardGatePart.RightTrigger, 3),
            _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGateDraw)} {gate}."),
        };
        return true;
    }

    /// <summary>A right trigger: its solid block and the shootable trigger right of it.</summary>
    private static readonly RoomBlockShape RightTrigger = new(
        [new(new(0, 0), RoomCollisionType.SolidBlock), new(new(1, 0), RoomCollisionType.ShootableBlock)]);

    /// <summary>A left trigger: its solid block and the shootable trigger left of it.</summary>
    private static readonly RoomBlockShape LeftTrigger = new(
        [new(new(0, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.ShootableBlock)]);

    /// <summary>
    /// The column at each of its six stages: five blocks down from its top block, the top and the rows the stage has
    /// filled shootable and the rest air.
    /// </summary>
    private static readonly RoomBlockShape[] ColumnStages = [.. Enumerable.Range(0, ColumnHeight + 1).Select(stage => new RoomBlockShape(
        [.. Enumerable.Range(0, ColumnHeight).Select(row => new RoomBlockShapeCell(new(0, row),
            row <= stage ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air))]))];

    /// <summary>Decoded column step or colored trigger shape.</summary>
    internal readonly record struct Draw(DownwardGatePart Part, [property: IntegerQuantity("gate animation frame index")] int Frame)
    {
        /// <summary>Whether the draw is the gate column's.</summary>
        internal bool Column => Part == DownwardGatePart.Column;

        /// <summary>
        /// The blocks the draw changes and how each collides: the column's shootable top and filled rows, or the trigger's
        /// solid block and its shootable trigger beside it.
        /// </summary>
        internal RoomBlockShape Blocks => Part switch
        {
            DownwardGatePart.Column => ColumnStages[Frame],
            DownwardGatePart.RightTrigger => RightTrigger,
            DownwardGatePart.LeftTrigger => LeftTrigger,
            _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGatePart)} {Part}."),
        };
    }
}
