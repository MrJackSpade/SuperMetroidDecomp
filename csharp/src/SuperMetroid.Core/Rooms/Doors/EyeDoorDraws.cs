using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of both mirrored, three-component eye doors: the draw each typed eye-door frame shows, the block shape each
/// draws from <see cref="EyeDoorObjectDrawDefinitions"/> (#1862), and the installed picture each alias shows.
/// </summary>
internal static class EyeDoorDraws
{
    /// <summary>The block shape of each eye-door draw: its blocks down its column or along its row.</summary>
    private static readonly Dictionary<Draw, RoomBlockShape> Shapes = EyeDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

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
            EyeDoorObjectFrame.ClosedEye => RoomObjectDraw.Of(new Draw(Component.Eye, 0, left)),
            EyeDoorObjectFrame.HalfOpenEye => RoomObjectDraw.Of(new Draw(Component.Eye, 1, left)),
            EyeDoorObjectFrame.OpenEye => RoomObjectDraw.Of(new Draw(Component.Eye, 2, left)),
            EyeDoorObjectFrame.FlinchingEye => RoomObjectDraw.Of(new Draw(Component.Eye, 3, left)),
            EyeDoorObjectFrame.ScrewedShutEye => RoomObjectDraw.Of(new Draw(Component.Eye, 4, left)),
            EyeDoorObjectFrame.OpeningClear => RoomObjectDraw.Of(new Draw(Component.Clear, 0, left)),
            // The eye flashes to the closed blue cap of its facing.
            EyeDoorObjectFrame.BlueCap => BlueDoorCaps.Closed(left ? ColoredDoorOrientation.Left : ColoredDoorOrientation.Right),
            EyeDoorObjectFrame.PartResting => RoomObjectDraw.Of(new Draw(part, 0, left)),
            EyeDoorObjectFrame.PartSwelling => RoomObjectDraw.Of(new Draw(part, 1, left)),
            EyeDoorObjectFrame.PartSwollen => RoomObjectDraw.Of(new Draw(part, 2, left)),
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
        /// <summary>Whether the draw's blocks stand in a column: the eye's and the clear's.</summary>
        internal bool Vertical => Part is Component.Eye or Component.Clear;

        /// <summary>The draw's blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>Maps the mirrored opening clear to its shared editable visual source.</summary>
    internal static RoomObjectDraw VisualSource(RoomObjectDraw request) =>
        request == EyeDoorObjectDrawDefinitions.MirroredOpeningClear ? EyeDoorObjectDrawDefinitions.SharedClear : request;

    /// <summary>Accepts an eye-door draw whose frame lies within its component's animation.</summary>
    internal static bool TryDescribe(RoomObjectDraw request, out Draw draw) =>
        RoomObjectDraw.TryFrame(request, static (Draw frame) => (uint)frame.Frame < (uint)EyeDoorObjectDrawDefinitions.FrameCounts[frame.Part], out draw);
}
