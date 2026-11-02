using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey door caps and the four shared cleared openings, and their physical cells from
/// <see cref="GreyDoorPlmDrawDefinitions"/>.
/// </summary>
internal static class GreyDoorCaps
{
    /// <summary>One of twenty twelve-byte records.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame)
    {
        /// <summary>Left and right caps use a vertical four-cell run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The terrain of <paramref name="cell"/>, one of the cap's four cells in run order.</summary>
        internal RoomTerrainBlock BlockAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return GreyDoorPlmDrawDefinitions.Cells[this][cell];
        }
    }

    /// <summary>Decodes a pointer on the twelve-byte grid into clear or animated cap orientation and frame.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= GreyDoorPlmDrawDefinitions.ClearFrame and < 4, out draw);
}
