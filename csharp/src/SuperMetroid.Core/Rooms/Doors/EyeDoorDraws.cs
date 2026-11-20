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

    /// <summary>The draw <paramref name="pose"/> shows for the eye door's body or bottom, <paramref name="part"/>.</summary>
    internal static RoomObjectDraw Of(EyeDoorPart part, EyeDoorPartPose pose) =>
        Family.Of(new Draw(EyeDoorPartLookDefinitions.Looks[part.BodyPart][pose], part.Orientation));

    /// <summary>The draw <paramref name="frame"/> shows for the eye of the door facing <paramref name="orientation"/>.</summary>
    internal static RoomObjectDraw Of(EyeDoorOrientation orientation, EyeDoorObjectFrame frame)
    {
        RoomObjectDraw Shown(EyeDoorLook look) => Family.Of(new Draw(look, orientation));
        return frame switch
        {
            EyeDoorObjectFrame.ClosedEye => Shown(EyeDoorLook.EyeClosed),
            EyeDoorObjectFrame.HalfOpenEye => Shown(EyeDoorLook.EyeHalfOpen),
            EyeDoorObjectFrame.OpenEye => Shown(EyeDoorLook.EyeOpen),
            EyeDoorObjectFrame.FlinchingEye => Shown(EyeDoorLook.EyeFlinching),
            EyeDoorObjectFrame.OpenSolidEye => Shown(EyeDoorLook.EyeOpenSolid),
            EyeDoorObjectFrame.OpeningClear => Shown(EyeDoorLook.Clear),
            // The eye flashes to the closed blue cap of its facing.
            EyeDoorObjectFrame.BlueCap => BlueDoorCaps.ClosedSolid(BlueCapFacing(orientation)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorObjectFrame)} {frame}."),
        };
    }

    /// <summary>The blue cap the eye of the door facing <paramref name="orientation"/> flashes to and turns into: the one facing its own side.</summary>
    /// <exception cref="InvalidOperationException">The orientation is undefined.</exception>
    internal static ColoredDoorOrientation BlueCapFacing(EyeDoorOrientation orientation) => orientation switch
    {
        EyeDoorOrientation.Left => ColoredDoorOrientation.Left,
        EyeDoorOrientation.Right => ColoredDoorOrientation.Right,
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
    };

    /// <summary>
    /// The component <paramref name="look"/> draws, or null for the clear, which clears the whole door's four blocks once
    /// the eye opens.
    /// </summary>
    /// <exception cref="InvalidOperationException">The look is undefined.</exception>
    internal static EyeDoorComponent? PartOf(EyeDoorLook look) => look switch
    {
        EyeDoorLook.Clear => null,
        EyeDoorLook.EyeClosed or EyeDoorLook.EyeHalfOpen or EyeDoorLook.EyeOpen or EyeDoorLook.EyeFlinching or EyeDoorLook.EyeOpenSolid => EyeDoorComponent.Eye,
        EyeDoorLook.MiddleResting or EyeDoorLook.MiddleSwelling or EyeDoorLook.MiddleSwollen => EyeDoorComponent.Door,
        EyeDoorLook.BottomResting or EyeDoorLook.BottomSwelling or EyeDoorLook.BottomSwollen => EyeDoorComponent.Bottom,
        _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorLook)} {look}."),
    };

    /// <summary>One eye-door draw: what it shows and its side.</summary>
    /// <param name="Look">What the draw shows, which names the component it writes.</param>
    /// <param name="Orientation">The way the door the draw belongs to faces; the two clears differ only in this.</param>
    internal readonly record struct Draw(EyeDoorLook Look, EyeDoorOrientation Orientation)
    {
        /// <summary>The component the draw writes, or null for the clear.</summary>
        internal EyeDoorComponent? Part => PartOf(Look);

        /// <summary>Whether the draw's blocks stand in a column: the eye's and the clear's.</summary>
        internal bool Vertical => Part is null or EyeDoorComponent.Eye;

        /// <summary>The draw's blocks from its first and how each collides.</summary>
        internal RoomBlockShape Blocks => Shapes[this];
    }

    /// <summary>
    /// Both eye doors' frames (#2034), pictured by the installed eye-door frames; the mirrored opening clear shows the
    /// shared clear mirrored.
    /// </summary>
    internal static RoomObjectDrawFamily<Draw> Family { get; } =
        new RoomObjectDrawFamily<Draw>("eye-door", frame => frame.Blocks);
}
