using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The draws of both mirrored, three-component eye doors: the draw each typed eye-door frame shows, the block shape each
/// draws from <see cref="EyeDoorObjectDrawDefinitions"/> (#1862, #1994), and the installed picture the mirrored clear shows.
/// </summary>
internal static class EyeDoorDraws
{
    /// <summary>
    /// The block shape of each eye-door draw: its blocks down its column or along its row. Built on first use: the eye
    /// doors' definitions make their two clears through <see cref="Family"/>, so the shapes cannot be read while they load.
    /// </summary>
    private static Dictionary<Draw, RoomBlockShape> Shapes => field ??= EyeDoorObjectDrawDefinitions.Collisions
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Line(pair.Key.Vertical, pair.Value));

    /// <summary>The draw <paramref name="frame"/> shows for <paramref name="component"/> facing <paramref name="orientation"/>.</summary>
    internal static RoomObjectDraw Of(EyeDoorOrientation orientation, EyeDoorComponent component, EyeDoorObjectFrame frame)
    {
        bool left = orientation switch
        {
            EyeDoorOrientation.Left => true,
            EyeDoorOrientation.Right => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        bool bottom = component switch
        {
            EyeDoorComponent.Door or EyeDoorComponent.Eye => false,
            EyeDoorComponent.Bottom => true,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorComponent)} {component}."),
        };
        RoomObjectDraw Shown(EyeDoorLook look) => Family.Of(new Draw(look, left));
        return frame switch
        {
            EyeDoorObjectFrame.ClosedEye => Shown(EyeDoorLook.EyeClosed),
            EyeDoorObjectFrame.HalfOpenEye => Shown(EyeDoorLook.EyeHalfOpen),
            EyeDoorObjectFrame.OpenEye => Shown(EyeDoorLook.EyeOpen),
            EyeDoorObjectFrame.FlinchingEye => Shown(EyeDoorLook.EyeFlinching),
            EyeDoorObjectFrame.OpenSolidEye => Shown(EyeDoorLook.EyeOpenSolid),
            EyeDoorObjectFrame.OpeningClear => Shown(EyeDoorLook.Clear),
            // The eye flashes to the closed blue cap of its facing.
            EyeDoorObjectFrame.BlueCap => BlueDoorCaps.ClosedSolid(left ? ColoredDoorOrientation.Left : ColoredDoorOrientation.Right),
            EyeDoorObjectFrame.PartResting => Shown(bottom ? EyeDoorLook.BottomResting : EyeDoorLook.MiddleResting),
            EyeDoorObjectFrame.PartSwelling => Shown(bottom ? EyeDoorLook.BottomSwelling : EyeDoorLook.MiddleSwelling),
            EyeDoorObjectFrame.PartSwollen => Shown(bottom ? EyeDoorLook.BottomSwollen : EyeDoorLook.MiddleSwollen),
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

    /// <summary>The segment <paramref name="look"/> draws.</summary>
    /// <exception cref="InvalidOperationException">The look is undefined.</exception>
    internal static Component PartOf(EyeDoorLook look) => look switch
    {
        EyeDoorLook.Clear => Component.Clear,
        EyeDoorLook.EyeClosed or EyeDoorLook.EyeHalfOpen or EyeDoorLook.EyeOpen or EyeDoorLook.EyeFlinching or EyeDoorLook.EyeOpenSolid => Component.Eye,
        EyeDoorLook.MiddleResting or EyeDoorLook.MiddleSwelling or EyeDoorLook.MiddleSwollen => Component.Middle,
        EyeDoorLook.BottomResting or EyeDoorLook.BottomSwelling or EyeDoorLook.BottomSwollen => Component.Bottom,
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorLook)} {look}."),
    };

    /// <summary>One eye-door draw: what it shows and its side.</summary>
    /// <param name="Look">What the draw shows, which names the component it writes.</param>
    /// <param name="Left">Whether the draw belongs to the left-facing door; the two clears differ only in this.</param>
    [IndependentFlag("which of the two mirrored eye doors a draw belongs to, a fact of the draw")]
    internal readonly record struct Draw(EyeDoorLook Look, bool Left)
    {
        /// <summary>The component the draw writes.</summary>
        internal Component Part => PartOf(Look);

        /// <summary>Whether the draw's blocks stand in a column: the eye's and the clear's.</summary>
        internal bool Vertical => Part is Component.Eye or Component.Clear;

        /// <summary>The draw's blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>
    /// Both eye doors' frames (#2034), pictured by the installed eye-door frames; the mirrored opening clear shows the
    /// shared clear mirrored.
    /// </summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        RoomObjectDrawFamily<Draw>.Pictured<InstalledRoomObjectEyeDoorVisuals>("eye-door", frame => frame.Blocks,
            (visuals, draw) => visuals.Image(draw));
}
