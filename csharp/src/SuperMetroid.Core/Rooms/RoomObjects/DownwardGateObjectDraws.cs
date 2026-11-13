using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each downward gate draw is described: six vertical five-cell column frames that fill one more shootable row per
/// step, and four trigger colours on either side. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class DownwardGateObjectDraws
{
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

    /// <summary>Decoded column step or colored trigger shape; how each cell collides is calculated from this draw identity.</summary>
    internal readonly record struct Draw(DownwardGatePart Part, [property: IntegerQuantity("gate animation frame index")] int Frame)
    {
        /// <summary>Whether the list writes the gate column.</summary>
        internal bool Column => Part == DownwardGatePart.Column;

        /// <summary>Whether the list writes a right-hand trigger.</summary>
        internal bool Right => Part == DownwardGatePart.RightTrigger;

        /// <summary>Columns and right triggers occupy one run; left triggers include a separate neighboring block.</summary>
        internal int RunCount => Column || Right ? 1 : 2;
        /// <summary>Columns write five vertical cells; right triggers write two cells; each left-trigger run writes one.</summary>
        internal int BlockCount(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return Column ? 5 : Right ? 2 : 1;
        }

        /// <summary>Where a run starts, in blocks from the gate's origin: a left trigger's second run one block left of it.</summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return new(!Column && !Right && run == 1 ? -1 : 0, 0);
        }
        /// <summary>How a cell collides: the shootable column top and filled rows, the solid neighbour, or the shootable trigger.</summary>
        internal RoomCollisionType CollisionAt(int run, int cell)
        {
            if ((uint)cell >= BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            if (Column)
                return cell <= Frame || cell == 0 ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air;
            return run == 0 && cell == 0 ? RoomCollisionType.SolidBlock : RoomCollisionType.ShootableBlock;
        }
    }
}
