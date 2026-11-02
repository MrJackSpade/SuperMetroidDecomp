using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of both mirrored, three-component eye doors: the draw each typed eye-door frame shows, their physical
/// cells from <see cref="EyeDoorPlmDrawDefinitions"/>, and the editable source of each alias.
/// </summary>
internal static class EyeDoorDraws
{
    /// <summary>The draw <paramref name="frame"/> shows for <paramref name="component"/> facing <paramref name="orientation"/>.</summary>
    internal static PlmDraw DrawList(EyeDoorOrientation orientation, EyeDoorComponent component, EyeDoorPlmFrame frame)
    {
        bool left = orientation switch
        {
            EyeDoorOrientation.Left => true,
            EyeDoorOrientation.Right => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        Component part = component switch
        {
            EyeDoorComponent.Door or EyeDoorComponent.Eye => Component.Middle,
            EyeDoorComponent.Bottom => Component.Bottom,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorComponent)} {component}."),
        };
        return frame switch
        {
            EyeDoorPlmFrame.Eye0 => PlmDraw.Of(new Draw(Component.Eye, 0, left)),
            EyeDoorPlmFrame.Eye1 => PlmDraw.Of(new Draw(Component.Eye, 1, left)),
            EyeDoorPlmFrame.Eye2 => PlmDraw.Of(new Draw(Component.Eye, 2, left)),
            EyeDoorPlmFrame.Eye3 => PlmDraw.Of(new Draw(Component.Eye, 3, left)),
            EyeDoorPlmFrame.Eye4 => PlmDraw.Of(new Draw(Component.Eye, 4, left)),
            EyeDoorPlmFrame.OpeningClear => PlmDraw.Of(new Draw(Component.Clear, 0, left)),
            // The eye flashes to the closed blue cap of its facing.
            EyeDoorPlmFrame.BlueCap => BlueDoorCaps.Alias(left ? ColoredDoorOrientation.Left : ColoredDoorOrientation.Right),
            EyeDoorPlmFrame.Pose0 => PlmDraw.Of(new Draw(part, 0, left)),
            EyeDoorPlmFrame.Pose1 => PlmDraw.Of(new Draw(part, 1, left)),
            EyeDoorPlmFrame.Pose2 => PlmDraw.Of(new Draw(part, 2, left)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorPlmFrame)} {frame}."),
        };
    }

    /// <summary>Physical segment affected by one eye-door draw.</summary>
    internal enum Component
    {
        /// <summary>Animated two-block eye with its vulnerable collision phase.</summary>
        Eye,
        /// <summary>Middle solid door component.</summary>
        Middle,
        /// <summary>Bottom solid door component.</summary>
        Bottom,
        /// <summary>Four-block clearing frame after the eye opens.</summary>
        Clear,
    }

    /// <summary>One eye-door draw: its component, animation frame and side.</summary>
    /// <param name="Part">The component the draw writes.</param>
    /// <param name="Frame">The component's animation frame.</param>
    /// <param name="Left">Whether the draw belongs to the left-facing door; the two clears differ only in this.</param>
    [IndependentFlag("which of the two mirrored eye doors a draw belongs to, a fact of the draw")]
    internal readonly record struct Draw(Component Part, [property: IntegerQuantity("eye animation frame index")] int Frame, bool Left)
    {
        /// <summary>Whether the run writes cells vertically rather than horizontally.</summary>
        internal bool Vertical => Part is Component.Eye or Component.Clear;
        /// <summary>Gets the cell count for an eye, middle, bottom, or clear component.</summary>
        internal int BlockCount => EyeDoorPlmDrawDefinitions.Cells[this].Count;
        /// <summary>The terrain of <paramref name="cell"/> in run order.</summary>
        internal RoomTerrainBlock BlockAt(int cell)
        {
            IReadOnlyList<RoomTerrainBlock> cells = EyeDoorPlmDrawDefinitions.Cells[this];
            if ((uint)cell >= (uint)cells.Count) throw new ArgumentOutOfRangeException(nameof(cell));
            return cells[cell];
        }
    }

    /// <summary>Maps the mirrored opening clear to its shared editable visual source.</summary>
    internal static PlmDraw VisualSource(PlmDraw request) =>
        request == EyeDoorPlmDrawDefinitions.MirroredOpeningClear ? EyeDoorPlmDrawDefinitions.SharedClear : request;

    /// <summary>Accepts an eye-door draw whose frame lies within its component's animation.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw frame) => (uint)frame.Frame < (uint)EyeDoorPlmDrawDefinitions.FrameCounts[frame.Part], out draw);
}
