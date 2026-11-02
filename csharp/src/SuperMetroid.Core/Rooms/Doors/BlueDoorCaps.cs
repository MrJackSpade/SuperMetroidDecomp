using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue-door caps: the closed alias and the four opening frames of each facing, and their physical cells from
/// <see cref="BlueDoorPlmDrawDefinitions"/>.
/// </summary>
internal static class BlueDoorCaps
{
    /// <summary>One of the twenty blue-cap draws: four facings, each with the alias and four opening frames.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, int Frame)
    {
        /// <summary>Whether this cap's four blocks are laid out as a vertical run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;

        /// <summary>The terrain of <paramref name="cell"/>, one of the cap's four cells in run order.</summary>
        internal RoomTerrainBlock BlockAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return BlueDoorPlmDrawDefinitions.Cells[this][cell];
        }
    }

    /// <summary>The closed cap facing <paramref name="orientation"/>, the alias that shows frame zero's artwork.</summary>
    internal static PlmDraw Alias(ColoredDoorOrientation orientation) =>
        PlmDraw.Of(new Draw(orientation, BlueDoorPlmDrawDefinitions.AliasFrame));

    /// <summary>Accepts a blue-cap draw: the alias or one of the four opening frames.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= BlueDoorPlmDrawDefinitions.AliasFrame and < 4, out draw);

    /// <summary>Maps a physical closed-cap alias to the matching editable frame-zero artwork.</summary>
    internal static PlmDraw VisualSource(PlmDraw request) =>
        request is PlmFrameDraw<Draw> { Frame: { Frame: BlueDoorPlmDrawDefinitions.AliasFrame } alias } ? PlmDraw.Of(alias with { Frame = 0 }) : request;
}
