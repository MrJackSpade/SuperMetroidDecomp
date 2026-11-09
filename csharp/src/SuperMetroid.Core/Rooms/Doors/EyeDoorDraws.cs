using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of both mirrored, three-component eye doors: the draw each typed eye-door frame shows, how their cells
/// collide from <see cref="EyeDoorObjectDrawDefinitions"/>, and the installed picture each alias shows.
/// </summary>
internal static class EyeDoorDraws
{
    /// <summary>The draw <paramref name="frame"/> shows for <paramref name="component"/> facing <paramref name="orientation"/>.</summary>
    internal static RoomObjectDraw DrawList(EyeDoorOrientation orientation, EyeDoorComponent component, EyeDoorObjectFrame frame)
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
            EyeDoorObjectFrame.Eye0 => RoomObjectDraw.Of(new Draw(Component.Eye, 0, left)),
            EyeDoorObjectFrame.Eye1 => RoomObjectDraw.Of(new Draw(Component.Eye, 1, left)),
            EyeDoorObjectFrame.Eye2 => RoomObjectDraw.Of(new Draw(Component.Eye, 2, left)),
            EyeDoorObjectFrame.Eye3 => RoomObjectDraw.Of(new Draw(Component.Eye, 3, left)),
            EyeDoorObjectFrame.Eye4 => RoomObjectDraw.Of(new Draw(Component.Eye, 4, left)),
            EyeDoorObjectFrame.OpeningClear => RoomObjectDraw.Of(new Draw(Component.Clear, 0, left)),
            // The eye flashes to the closed blue cap of its facing.
            EyeDoorObjectFrame.BlueCap => BlueDoorCaps.Alias(left ? ColoredDoorOrientation.Left : ColoredDoorOrientation.Right),
            EyeDoorObjectFrame.Pose0 => RoomObjectDraw.Of(new Draw(part, 0, left)),
            EyeDoorObjectFrame.Pose1 => RoomObjectDraw.Of(new Draw(part, 1, left)),
            EyeDoorObjectFrame.Pose2 => RoomObjectDraw.Of(new Draw(part, 2, left)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorObjectFrame)} {frame}."),
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
        internal int BlockCount => EyeDoorObjectDrawDefinitions.Collisions[this].Count;
        /// <summary>How <paramref name="cell"/>, in run order, collides.</summary>
        internal RoomCollisionType CollisionAt(int cell)
        {
            IReadOnlyList<RoomCollisionType> cells = EyeDoorObjectDrawDefinitions.Collisions[this];
            if ((uint)cell >= (uint)cells.Count) throw new ArgumentOutOfRangeException(nameof(cell));
            return cells[cell];
        }
    }

    /// <summary>Maps the mirrored opening clear to its shared editable visual source.</summary>
    internal static RoomObjectDraw VisualSource(RoomObjectDraw request) =>
        request == EyeDoorObjectDrawDefinitions.MirroredOpeningClear ? EyeDoorObjectDrawDefinitions.SharedClear : request;

    /// <summary>Accepts an eye-door draw whose frame lies within its component's animation.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw frame) => (uint)frame.Frame < (uint)EyeDoorObjectDrawDefinitions.FrameCounts[frame.Part], out draw);
}
