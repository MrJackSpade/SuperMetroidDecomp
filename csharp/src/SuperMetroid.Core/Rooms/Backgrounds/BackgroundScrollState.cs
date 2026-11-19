using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The layers' positions and the scrolls the screen shows them at (#1555): the room pixel the foreground's view starts at
/// and the background pixel the background's starts at, both in the layer's own pixels with no offset of their own.
/// </summary>
/// <remarks>
/// This reproduces how the game derives the background's position from the camera, and when each scroll follows its
/// position: every unfrozen gameplay update, once at room entry, and at the steps a door's opening scroll recalculates
/// them. Backgrounds render from level data and installed images through these scrolls (#1346), with no camera
/// interpolation. Every calculation below computes its two axes independently, so whole-value
/// updates preserve its results.
/// </remarks>
public sealed class BackgroundScrollState
{
    /// <summary>World-space foreground/camera position.</summary>
    public Position16<RoomPixels> ForegroundPosition { get; set; }

    /// <summary>Parallax-derived background position.</summary>
    public Position16<BackgroundPixels> BackgroundPosition { get; set; }

    /// <summary>
    /// The room state's per-axis background scroll. Before a room state sets it, both axes copy the foreground and scroll in,
    /// as the game does before any room state chooses.
    /// </summary>
    public BackgroundScrollModes BackgroundScrollModes { get; set; } = BackgroundScrollModes.FollowsForeground;

    /// <summary>
    /// The room pixel the foreground's view starts at: its position at the last scroll calculation, or a room main's shift,
    /// as a signed position (#2384): a view above or left of the room's origin, as the Crocomire rumble's from a camera at the
    /// room's top, is negative.
    /// </summary>
    public Offset16<RoomPixels> ForegroundScroll { get; private set; }

    /// <summary>
    /// The background pixel the background's view starts at: its position at the last scroll calculation, as a signed
    /// position (#2305, #2498): a view left of or above the background's origin, as a door's opening view or a fixed axis
    /// held there, is negative.
    /// </summary>
    /// <remarks>
    /// The position's wrap past the origin is not load-bearing: nothing reads the background's position but its views and the
    /// door's opening scroll, which moves it a few pixels at a time, and it stays within a room's few screens of the origin,
    /// so a position that has wrapped past the background's top or left edge lies a small negative distance from the origin.
    /// The view takes it signed once, where it is set.
    /// </remarks>
    public Offset16<BackgroundPixels> BackgroundScroll { get; private set; }

    /// <summary>
    /// Shifts the foreground's view <paramref name="rows"/> rows down from the camera's row without moving the camera (a room
    /// main's rumble): a plain offset from the camera's signed row, so a rumble above the room's top leaves a negative row.
    /// </summary>
    public void ShiftForegroundViewFromCamera(int rows) =>
        ForegroundScroll = ForegroundScroll.WithY(SignedView(ForegroundPosition).Y + rows);

    /// <summary>
    /// Begins a door's opening scroll with the destination room's views at <paramref name="openingForeground"/>, the
    /// destination pixel the source room's screen will lie at, before the setup's first step: the screen shows the source
    /// room's screen there and the destination scrolls in around it.
    /// </summary>
    public void BeginDoorOpeningViews(Position16<RoomPixels> openingForeground)
    {
        ForegroundScroll = SignedView(openingForeground);
        BackgroundScroll = CameraPastRoomEdge.SignedScroll<BackgroundPixels>(openingForeground.X, openingForeground.Y);
    }

    /// <summary>
    /// Implements the two background position calculations made by each directional door
    /// setup after the door installs the destination camera coordinates. A fixed background
    /// keeps its old background position, as the game's does.
    /// </summary>
    /// <param name="destination">The foreground destination published to <see cref="ForegroundPosition"/>.</param>
    /// <param name="parallaxBasis">
    /// The foreground position the parallax calculation reads. An upward door calculates background from
    /// the destination plus 31 before the scroll's foreground endpoint becomes the destination plus 32.
    /// </param>
    public void PrepareDoorOpeningDestination(Position16<RoomPixels> destination, Position16<RoomPixels> parallaxBasis)
    {
        if (BackgroundScrollModes.Horizontal.BackgroundPositionFor(parallaxBasis.X) is int backgroundX)
            BackgroundPosition = BackgroundPosition.WithX(backgroundX);
        if (BackgroundScrollModes.Vertical.BackgroundPositionFor(parallaxBasis.Y) is int backgroundY)
            BackgroundPosition = BackgroundPosition.WithY(backgroundY);
        ForegroundPosition = destination;
    }

    /// <summary>
    /// Steps one gameplay update's scrolling: the layer scrolls and the background parallax follow
    /// the foreground. A frozen frame changes nothing.
    /// </summary>
    public void StepScrolling(bool timeIsFrozen)
    {
        if (timeIsFrozen)
            return;
        CalculateScrolls();
    }

    /// <summary>
    /// The foreground's view follows its position, and each background axis its scroll mode, keeping a fixed axis's view.
    /// Room entry runs it once before the first frame; gameplay runs it every unfrozen frame.
    /// </summary>
    public void CalculateScrolls()
    {
        ForegroundScroll = SignedView(ForegroundPosition);

        // A fixed axis keeps both the background's position and its view on that axis.
        if (BackgroundScrollModes.Horizontal.BackgroundPositionFor(ForegroundPosition.X) is int backgroundX)
        {
            BackgroundPosition = BackgroundPosition.WithX(backgroundX);
            BackgroundScroll = BackgroundScroll.WithX(CameraPastRoomEdge.Signed(backgroundX));
        }

        if (BackgroundScrollModes.Vertical.BackgroundPositionFor(ForegroundPosition.Y) is int backgroundY)
        {
            BackgroundPosition = BackgroundPosition.WithY(backgroundY);
            BackgroundScroll = BackgroundScroll.WithY(CameraPastRoomEdge.Signed(backgroundY));
        }
    }

    /// <summary>
    /// Both views follow their layers' positions, for door transitions that already own the layer positions, without
    /// changing background parallax state.
    /// </summary>
    public void CalculateLayerScrolls()
    {
        ForegroundScroll = SignedView(ForegroundPosition);
        BackgroundScroll = CameraPastRoomEdge.SignedScroll<BackgroundPixels>(BackgroundPosition.X, BackgroundPosition.Y);
    }

    /// <summary>
    /// The foreground's view at a room position, a wrapping position taken as a signed view once (#2384): a position that has wrapped
    /// past the room's top or left edge, as a door's opening view does, lies a small negative distance from the origin.
    /// </summary>
    private static Offset16<RoomPixels> SignedView(Position16<RoomPixels> position) =>
        CameraPastRoomEdge.SignedScroll<RoomPixels>(position.X, position.Y);
}
