using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The camera position constrained by Super Metroid's directional scroll-zone checks.
/// </summary>
/// <remarks>
/// Each direction's travel stops at a closed screen or the room's edge; Samus's horizontal and vertical tracking supply
/// the proposed position, ideal position and camera speed they read.
/// </remarks>
[RoomLifetime]
public sealed partial class ScrollBoundaryCamera
{
    /// <summary>Creates a camera constrained by the supplied mutable room scroll grid.</summary>
    /// <param name="scrolls">Room-sized grid of directional scroll states.</param>
    public ScrollBoundaryCamera(RoomScrollGrid scrolls)
    {
        Scrolls = Ensure.NotNull(scrolls);
    }

    /// <summary>
    /// Gets the camera's position: whole pixels and fraction.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Gets the camera's integer position (its top-left room pixel).</summary>
    public Position16<RoomPixels> PixelPosition => Position.Whole;

    /// <summary>
    /// Gets the camera target calculated from Samus's position: horizontally from her facing
    /// context, vertically from the room's falling and rising camera offsets.
    /// </summary>
    internal Position16<RoomPixels> IdealPosition { get; set; }

    /// <summary>
    /// Gets the most recently calculated camera speed on each axis:
    /// the distance Samus moved, either way, plus one pixel.
    /// </summary>
    public FixedVector<RoomPixels> Speed { get; internal set; }

    /// <summary>
    /// Samus's previous position written at the end of scrolling, or that the camera has been placed since its last
    /// scrolling pass (<see cref="SamusScrollCheckpoint.SincePlacement"/>). Room-main movement occurs later and must
    /// remain visible to the next scroll update.
    /// </summary>
    public SamusScrollCheckpoint PreviousSamusPoint { get; private set; } = SamusScrollCheckpoint.SincePlacement;

    /// <summary>Publishes the previous-position checkpoint after both axes and the room's camera floor.</summary>
    public void FinishSamusScrolling(FixedPosition<RoomPixels> current) => PreviousSamusPoint = SamusScrollCheckpoint.At(current);

    /// <summary>
    /// The point a door's write starts from: the checkpoint's, or, for a camera placed since its last scroll, the room
    /// origin with no fraction of a pixel.
    /// </summary>
    private FixedPosition<RoomPixels> PointBeforeDoorWrite =>
        PreviousSamusPoint is SamusScrolledCheckpoint { Point: var point } ? point : FixedPosition<RoomPixels>.Zero;

    /// <summary>Door room replacement retains the global scrolling history and targets.</summary>
    internal void InheritTrackingState(ScrollBoundaryCamera previous)
    {
        IdealPosition = previous.IdealPosition;
        Speed = previous.Speed;
        PreviousSamusPoint = previous.PreviousSamusPoint;
    }

    /// <summary>Door placement writes both whole previous-position coordinates, retaining fractions.</summary>
    internal void PublishDoorSamusPosition(Position16<RoomPixels> samus) =>
        PreviousSamusPoint = SamusScrollCheckpoint.At(PointBeforeDoorWrite.WithWhole(samus));

    /// <summary>
    /// The door-transition scroll writes only the travel axis's whole previous position, retaining the
    /// other axis and both fractions.
    /// </summary>
    internal void PublishDoorSamusTravelAxis(Axis travel, Position16<RoomPixels> samus)
    {
        FixedPosition<RoomPixels> previous = PointBeforeDoorWrite;
        PreviousSamusPoint = SamusScrollCheckpoint.At(travel switch
        {
            Axis.Vertical => previous.WithY(previous.Y.WithWhole(samus.Y)),
            Axis.Horizontal => previous.WithX(previous.X.WithWhole(samus.X)),
            _ => throw new ArgumentOutOfRangeException(nameof(travel), travel, "Undefined door travel axis."),
        });
    }

    /// <summary>The room's scroll grid this camera consults.</summary>
    public RoomScrollGrid Scrolls { get; }

    /// <summary>
    /// Sets an entry/door camera position. This does only physical room-edge clamping;
    /// directional internal boundaries are evaluated when movement is attempted.
    /// </summary>
    public void SetPosition(Position16<RoomPixels> requested)
    {
        PreviousSamusPoint = SamusScrollCheckpoint.SincePlacement;
        int maxX = RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1);

        // The vertical maximum depends on whether the screen aligns the camera to its top. Starting with the
        // room-wide conservative lower-alignment allowance matches the boundary check's largest legal result;
        // the first vertical movement applies the screen-specific exact maximum.
        int maxY = (RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + SamusCameraDefinitions.LowerAlignment;
        var clamped = new Position16<RoomPixels>(
            Math.Clamp(requested.X, 0, maxX),
            Math.Clamp(requested.Y, 0, maxY));
        Position = FixedPosition<RoomPixels>.FromWhole(clamped);
        IdealPosition = clamped;
    }

    /// <summary>
    /// Keeps the fractions of the camera this one replaces.
    /// Room and game loads write only the integer positions, so the
    /// previous room's fractions remain.
    /// </summary>
    public void RetainFractions(ScrollBoundaryCamera previous) =>
        RetainFractions(previous.Position);

    /// <summary>
    /// Keeps the fractions of <paramref name="retained"/>, such as those that
    /// survived a released gameplay owner; the whole pixels are unchanged.
    /// </summary>
    public void RetainFractions(FixedPosition<RoomPixels> retained) =>
        Position = Position.WithFractionsOf(retained);

    /// <summary>
    /// Publishes the camera position the door-transition scroll owns, which may lie outside the room.
    /// </summary>
    /// <remarks>
    /// The vertical transitions begin a screen beyond the destination, outside the room: down starts a screen's height
    /// above the destination, wrapping as <see cref="CoordinateAxis"/> states, and up starts just below it. Every other
    /// placement of the camera is <see cref="SetPosition"/>'s, which keeps it within the room's edges.
    /// </remarks>
    public void SetDoorTransitionPosition(Position16<RoomPixels> position) =>
        // The door-transition setup and scroll write whole camera positions, leaving both fractions intact.
        Position = Position.WithWhole(position);

    /// <summary>
    /// Carries the retained camera fractions over when loading a door's new scroll grid:
    /// the whole pixels come from <paramref name="position"/>, the fractions from
    /// <paramref name="retainedFractions"/>.
    /// </summary>
    public void SetDoorTransitionPosition(Position16<RoomPixels> position, FixedPosition<RoomPixels> retainedFractions) =>
        Position = retainedFractions.WithWhole(position);

    /// <summary>
    /// Moves the camera across to where the Baby Metroid's AI places it. Unlike
    /// entry placement, the game does not clear the camera's fraction of a pixel or replace either ideal
    /// camera coordinate at this point in the gameplay frame.
    /// </summary>
    public void SetCameraXFromEnemyAi(int xPosition) => SetWholeX(xPosition);

    /// <summary>
    /// Moves the camera <paramref name="pixels"/> whole pixels right, as a room's main does (#2293): the fraction, the
    /// vertical axis and the ideal position keep what Samus's tracking left, and her next tracking starts from here.
    /// </summary>
    /// <param name="pixels">How far right the camera moves, in whole pixels.</param>
    internal void NudgeRight(int pixels) => SetWholeX(CameraPastRoomEdge.At(Position.X.Whole + pixels));

    /// <summary>
    /// Raises the camera to <paramref name="floor"/>'s lowest Y when it has tracked Samus above it, after both axes have
    /// tracked her and before her previous position is published. Only the whole Y changes: its fraction and the ideal
    /// position keep what the tracking left.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="floor"/> is null.</exception>
    public void ApplyFloor(RoomCameraFloor floor)
    {
        Ensure.NotNull(floor);
        if (floor.LowestY is not { } lowestY || lowestY <= PixelPosition.Y)
            return;
        SetWholeY(lowestY);
    }

    /// <summary>
    /// Runs horizontal Samus tracking plus camera-speed calculation.
    /// Integer movement follows the facing-dependent camera target; an unchanged integer position goes on to
    /// autoscrolling (Samus moving within her pixel still affects speed).
    /// </summary>
    public void TrackMovedSamusHorizontally(
        FixedPosition<RoomPixels> previous,
        FixedPosition<RoomPixels> current,
        HorizontalCameraContext context,
        bool timeIsFrozen)
    {
        if (!Enum.IsDefined(context.CameraDistance))
            throw new ArgumentOutOfRangeException(nameof(context), $"Undefined camera distance {context.CameraDistance}.");

        Speed = Speed.WithX(CalculateDistanceMovedPlusOne(previous.X, current.X));

        // The camera compares the whole pixels only. This is intentionally after speed
        // calculation: even when they match, differing fractional samples can make
        // the integer X speed used by autoscrolling zero, one, or (on wrap) much larger.
        if (previous.X.Whole == current.X.Whole)
        {
            HandleHorizontalAutoscrolling(timeIsFrozen);
            return;
        }

        // Knockback, moonwalking, or a turn that keeps the old momentum reverses which facing target
        // applies; every facing but left selects the right-facing target.
        bool backwards = context.KnockedBack
            || context.MovementType == SamusMovementType.Moonwalking
            || context.TurningAgainstMomentum;
        HorizontalDirection targetFacing = backwards ^ !context.FacesLeft ? HorizontalDirection.Right : HorizontalDirection.Left;
        int targetOffset = HorizontalCameraTargets.Offset(context.CameraDistance, targetFacing);
        IdealPosition = IdealPosition.WithX(CameraPastRoomEdge.At(current.X.Whole - targetOffset));

        if (IdealPosition.X == Position.X.Whole)
            return;

        if (CameraPastRoomEdge.LiesBefore(IdealPosition.X, Position.X.Whole))
        {
            Position = Position.AdvanceX(-Speed.X);
            HandleScrollingLeft();
        }
        else
        {
            Position = Position.AdvanceX(Speed.X);
            HandleScrollingRight();
        }
    }

    /// <summary>
    /// The camera's scrolling during a fast grapple swing: the camera
    /// steps toward a dead zone around Samus by its slow-scroll step, then both autoscrollers run.
    /// The camera's scroll speeds are not recalculated. A negative Samus X skips both axes.
    /// </summary>
    public void TrackGrappleSlowScrolling(Position16<RoomPixels> samus, bool timeIsFrozen)
    {
        if (!CameraPastRoomEdge.LiesBeforeRoom(samus.X))
        {
            SetWholeX(StepTowardDeadZone(Position.X.Whole, samus.X,
                GrappleSlowScrollDefinitions.LeftEdgeX, GrappleSlowScrollDefinitions.RightEdgeX));
            if (!CameraPastRoomEdge.LiesBeforeRoom(samus.Y))
            {
                SetWholeY(StepTowardDeadZone(Position.Y.Whole, samus.Y,
                    GrappleSlowScrollDefinitions.TopEdgeY, GrappleSlowScrollDefinitions.BottomEdgeY));
            }
        }
        HandleHorizontalAutoscrolling(timeIsFrozen);
        HandleVerticalAutoscrolling(timeIsFrozen);
    }

    /// <summary>Moves the camera toward Samus until she lies within the grapple dead zone.</summary>
    /// <remarks>Samus left of or above the camera scrolls it back, as do screen offsets below the near edge; offsets at
    /// the far edge advance it.</remarks>
    private static int StepTowardDeadZone(int camera, int samus, int nearEdge, int farEdge)
    {
        if (samus < camera)
            return CameraPastRoomEdge.At(camera - GrappleSlowScrollDefinitions.Step);
        int onScreen = CameraPastRoomEdge.At(samus - camera);
        if (onScreen >= farEdge)
            return CameraPastRoomEdge.At(camera + GrappleSlowScrollDefinitions.Step);
        if (onScreen < nearEdge)
            return CameraPastRoomEdge.At(camera - GrappleSlowScrollDefinitions.Step);
        return camera;
    }

    /// <summary>
    /// Runs vertical Samus tracking plus distance calculation.
    /// An unchanged integer Y position runs the boundary check alone.
    /// </summary>
    public void TrackMovedSamusVertically(
        FixedPosition<RoomPixels> previous,
        FixedPosition<RoomPixels> current,
        VerticalCameraContext context,
        bool timeIsFrozen)
    {
        Speed = Speed.WithY(CalculateDistanceMovedPlusOne(previous.Y, current.Y));

        if (previous.Y.Whole == current.Y.Whole)
        {
            HandleVerticalAutoscrolling(timeIsFrozen);
            return;
        }

        // Moving up aims by the rising offset; moving any other way, by the falling one.
        int targetOffset = context.YDirection == SamusVerticalDirection.Up ? context.RisingCameraOffset : context.FallingCameraOffset;
        IdealPosition = IdealPosition.WithY(CameraPastRoomEdge.At(current.Y.Whole - targetOffset));

        if (IdealPosition.Y == Position.Y.Whole)
            return;

        if (CameraPastRoomEdge.LiesBefore(IdealPosition.Y, Position.Y.Whole))
        {
            Position = Position.AdvanceY(-Speed.Y);
            HandleScrollingUp();
        }
        else
        {
            Position = Position.AdvanceY(Speed.Y);
            HandleScrollingDown();
        }
    }

    /// <summary>
    /// Horizontal scroll-zone autoscrolling.
    /// </summary>
    /// <remarks>
    /// This step does not chase Samus. It gently pushes a camera that is sitting inside
    /// a closed scroll screen toward an open adjacent screen, or back from a closed screen on its
    /// right, by the camera's speed plus the closed screen's drift (<see cref="SamusCameraDefinitions.ClosedScreenDrift"/>).
    /// </remarks>
    private void HandleHorizontalAutoscrolling(bool timeIsFrozen)
    {
        // Frozen time stops autoscrolling. Keep this gate local to the two autoscrollers;
        // the directional moved-Samus checks themselves have no freeze check.
        if (timeIsFrozen)
            return;

        // The scroll records the pre-clamp position. Every proposed movement below starts from
        // this starting position, even if the live position is first clamped to a room edge.
        int proposed = Position.X.Whole;

        if (CameraPastRoomEdge.LiesBeforeRoom(Position.X.Whole))
            SetWholeX(0);

        int roomMaximum = RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1);
        if (Position.X.Whole > roomMaximum)
            SetWholeX(roomMaximum);

        Position16<RoomScreens> current = HorizontalCheckScreen();

        if (Scrolls.ReadCameraState(current) == RoomScrollState.Closed)
        {
            // In a closed current screen, drift right by the camera's speed plus the closed screen's drift toward its right edge.
            // If the screen right of the candidate is also closed, discard the fractional screen position and stay
            // on the candidate screen's left edge instead of allowing the viewport to enter it.
            DriftThroughClosedScreen(Axis.Horizontal, proposed, current);
            return;
        }

        if (Scrolls.ReadCameraState(RightOf(current)) != RoomScrollState.Closed)
            return;

        // An open current screen with a closed screen to its right is the mirror case: drift left toward the current
        // screen's left edge. If the candidate itself lands in a closed screen, round up to the next screen boundary;
        // which way the distance lies (CoordinateAxis.Difference) decides the drift.
        DriftBackFromClosedScreenBeyond(Axis.Horizontal, proposed, current, ScreenStart(Position.X.Whole));
    }

    /// <summary>
    /// Vertical scroll-zone autoscrolling.
    /// </summary>
    private void HandleVerticalAutoscrolling(bool timeIsFrozen)
    {
        if (timeIsFrozen)
            return;

        Position16<RoomScreens> preClamp = VerticalCheckScreen();

        // Screens open with the camera aligned to the top align the gameplay viewport to
        // the top of the screen; closed and open ones permit the game's lower alignment. Crucially, the
        // alignment is selected before Y is clamped and remains fixed for this update.
        int verticalAlignment = Scrolls.ReadCameraState(preClamp) == RoomScrollState.OpenAlignedToTop
            ? 0
            : SamusCameraDefinitions.LowerAlignment;
        int proposed = Position.Y.Whole;

        if (CameraPastRoomEdge.LiesBeforeRoom(Position.Y.Whole))
            SetWholeY(0);

        int roomMaximum = RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1) + verticalAlignment;
        if (Position.Y.Whole > roomMaximum)
            SetWholeY(roomMaximum);

        // Clamping moves the camera vertically only, so the column through its horizontal centre is unchanged.
        Position16<RoomScreens> current = VerticalCheckScreen();
        if (Scrolls.ReadCameraState(current) == RoomScrollState.Closed)
        {
            // A closed current screen pushes downward toward its bottom boundary. As in the X
            // step, a second closed screen beyond the candidate causes screen-edge rounding.
            DriftThroughClosedScreen(Axis.Vertical, proposed, current);
            return;
        }

        if (Scrolls.ReadCameraState(Below(current)) != RoomScrollState.Closed)
            return;

        int topBoundary = CameraPastRoomEdge.At(ScreenStart(Position.Y.Whole) + verticalAlignment);

        // The comparison only pulls the camera upward when it is strictly below the
        // aligned boundary. Equality intentionally does nothing.
        if (topBoundary >= Position.Y.Whole)
            return;

        DriftBackFromClosedScreenBeyond(Axis.Vertical, proposed, current, topBoundary);
    }

    /// <summary>Stops rightward travel at the ideal point, room edge, or closed screen to the right.</summary>
    internal void HandleScrollingRight()
    {
        int proposed = Position.X.Whole;

        // If the camera has moved past its ideal, which way the distance lies deciding it, it snaps to the ideal and
        // drops its fraction.
        if (CameraPastRoomEdge.LiesBefore(IdealPosition.X, Position.X.Whole))
            SnapToIdeal(Axis.Horizontal);

        int roomMaximum = RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1);
        if (Position.X.Whole > roomMaximum)
        {
            SetWholeX(roomMaximum);
            return;
        }

        // The check reads the screen immediately to the right of the camera's screen column.
        if (Scrolls.ReadCameraState(RightOf(HorizontalCheckScreen())) != RoomScrollState.Closed)
            return;

        DriftBack(Axis.Horizontal, proposed, ScreenStart(Position.X.Whole));
    }

    /// <summary>Stops leftward travel at the ideal point, room edge, or current closed screen.</summary>
    internal void HandleScrollingLeft() => StopBackwardTravel(Axis.Horizontal);

    /// <summary>Applies the top-aligned versus closed/open vertical alignment and blocks downward travel at room or scroll limits.</summary>
    internal void HandleScrollingDown()
    {
        int proposed = Position.Y.Whole;
        Position16<RoomScreens> current = VerticalCheckScreen();

        // Top-aligned screens align the playfield with no allowance; closed and open ones allow the lower alignment.
        int verticalAlignment = Scrolls.ReadCameraState(current) == RoomScrollState.OpenAlignedToTop
            ? 0
            : SamusCameraDefinitions.LowerAlignment;

        if (CameraPastRoomEdge.LiesBefore(IdealPosition.Y, Position.Y.Whole))
            SnapToIdeal(Axis.Vertical);

        int boundary = RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1) + verticalAlignment;
        bool beyondRoomBottom = boundary < Position.Y.Whole;
        bool blockedBelow = false;
        if (!beyondRoomBottom)
        {
            // A camera already past the room's bottom is not blocked by the screen below, which is not read, so a camera
            // on the bottom row or right edge never reads below the room's screens.
            blockedBelow = Scrolls.ReadCameraState(Below(current)) == RoomScrollState.Closed;
            if (blockedBelow)
            {
                boundary = CameraPastRoomEdge.At(ScreenStart(Position.Y.Whole) + verticalAlignment);
                blockedBelow = boundary < Position.Y.Whole;
            }
        }

        if (beyondRoomBottom || blockedBelow)
            DriftBack(Axis.Vertical, proposed, boundary);
    }

    /// <summary>Stops upward travel at the ideal point, room top, or current closed screen.</summary>
    internal void HandleScrollingUp() => StopBackwardTravel(Axis.Vertical);

    /// <summary>
    /// Stops travel back along <paramref name="axis"/> (leftward or upward) at the ideal point, the room's first edge, or a
    /// closed current screen (#2557): an overshoot snaps to the ideal position and drops its fraction, a camera before the
    /// room stands on its edge, and in a closed screen the camera drifts forward by its speed plus the closed screen's drift
    /// from where it began the update, no further than the next screen's start.
    /// </summary>
    private void StopBackwardTravel(Axis axis)
    {
        int proposed = WholeAlong(axis);
        if (CameraPastRoomEdge.LiesBefore(WholeAlong(axis), IdealAlong(axis)))
            SnapToIdeal(axis);

        if (CameraPastRoomEdge.LiesBeforeRoom(WholeAlong(axis)))
        {
            SetWholeAlong(axis, 0);
            return;
        }

        if (Scrolls.ReadCameraState(CheckScreen(axis)) != RoomScrollState.Closed)
            return;

        int boundary = NextScreenStart(WholeAlong(axis));
        int candidate = CameraPastRoomEdge.At(proposed + SpeedAlong(axis) + SamusCameraDefinitions.ClosedScreenDrift);
        SetWholeAlong(axis, candidate >= boundary ? boundary : candidate);
    }

    /// <summary>
    /// Drifts the camera back along <paramref name="axis"/> by its speed plus the closed screen's drift from
    /// <paramref name="proposed"/>, where it began the update, no further back than <paramref name="boundary"/>, which way
    /// the distance lies deciding it.
    /// </summary>
    private void DriftBack(Axis axis, int proposed, int boundary)
    {
        int candidate = CameraPastRoomEdge.At(proposed - SpeedAlong(axis) - SamusCameraDefinitions.ClosedScreenDrift);
        SetWholeAlong(axis, CameraPastRoomEdge.LiesBefore(candidate, boundary) ? boundary : candidate);
    }

    /// <summary>
    /// Autoscrolls a camera in the closed <paramref name="current"/> screen forward along <paramref name="axis"/> toward the
    /// screen's far edge by its speed plus the closed screen's drift from <paramref name="proposed"/> (#2557). It stops on the
    /// next screen's start; and if the screen beyond the one it lands in is also closed, it drops its place in that screen
    /// and stands on the screen's start instead of entering the closed one.
    /// </summary>
    private void DriftThroughClosedScreen(Axis axis, int proposed, Position16<RoomScreens> current)
    {
        int boundary = NextScreenStart(WholeAlong(axis));
        int candidate = CameraPastRoomEdge.At(proposed + SpeedAlong(axis) + SamusCameraDefinitions.ClosedScreenDrift);
        if (candidate >= boundary)
        {
            SetWholeAlong(axis, boundary);
            return;
        }

        SetWholeAlong(axis, Scrolls.ReadCameraState(Beyond(axis, ScreenAt(axis, current, candidate))) != RoomScrollState.Closed
            ? candidate
            : ScreenStart(candidate));
    }

    /// <summary>
    /// Autoscrolls a camera in an open <paramref name="current"/> screen with a closed screen beyond it back along
    /// <paramref name="axis"/> toward <paramref name="boundary"/> by its speed plus the closed screen's drift from
    /// <paramref name="proposed"/> (#2557): it stops on the boundary, and one that lands in a closed screen rounds up to the
    /// next screen's start.
    /// </summary>
    private void DriftBackFromClosedScreenBeyond(Axis axis, int proposed, Position16<RoomScreens> current, int boundary)
    {
        int candidate = CameraPastRoomEdge.At(proposed - SpeedAlong(axis) - SamusCameraDefinitions.ClosedScreenDrift);
        if (CameraPastRoomEdge.LiesBefore(candidate, boundary))
        {
            SetWholeAlong(axis, boundary);
            return;
        }

        SetWholeAlong(axis, Scrolls.ReadCameraState(ScreenAt(axis, current, candidate)) != RoomScrollState.Closed
            ? candidate
            : NextScreenStart(candidate));
    }

    /// <summary>The camera's whole position along <paramref name="axis"/>.</summary>
    private int WholeAlong(Axis axis) => axis switch
    {
        Axis.Horizontal => Position.X.Whole,
        Axis.Vertical => Position.Y.Whole,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>Writes only the camera's whole position along <paramref name="axis"/>, preserving its fraction and other axis.</summary>
    private void SetWholeAlong(Axis axis, int whole)
    {
        switch (axis)
        {
            case Axis.Horizontal:
                SetWholeX(whole);
                return;
            case Axis.Vertical:
                SetWholeY(whole);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }

    /// <summary>The camera's speed along <paramref name="axis"/>, in whole pixels.</summary>
    private int SpeedAlong(Axis axis) => axis switch
    {
        Axis.Horizontal => Speed.X.WholePixels,
        Axis.Vertical => Speed.Y.WholePixels,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>The camera's ideal position along <paramref name="axis"/>.</summary>
    private int IdealAlong(Axis axis) => axis switch
    {
        Axis.Horizontal => IdealPosition.X,
        Axis.Vertical => IdealPosition.Y,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>Snaps an overshoot along <paramref name="axis"/> to the ideal position, discarding the fractional camera position.</summary>
    private void SnapToIdeal(Axis axis) => Position = axis switch
    {
        Axis.Horizontal => Position.WithX(FixedCoordinate.FromWhole(IdealPosition.X)),
        Axis.Vertical => Position.WithY(FixedCoordinate.FromWhole(IdealPosition.Y)),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>The screen the checks along <paramref name="axis"/> read.</summary>
    private Position16<RoomScreens> CheckScreen(Axis axis) => axis switch
    {
        Axis.Horizontal => HorizontalCheckScreen(),
        Axis.Vertical => VerticalCheckScreen(),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>The screen beyond <paramref name="screen"/> along <paramref name="axis"/>: to its right, or below it.</summary>
    private static Position16<RoomScreens> Beyond(Axis axis, Position16<RoomScreens> screen) => axis switch
    {
        Axis.Horizontal => RightOf(screen),
        Axis.Vertical => Below(screen),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>The screen in <paramref name="screen"/>'s row or column that holds <paramref name="pixel"/> along <paramref name="axis"/>.</summary>
    private static Position16<RoomScreens> ScreenAt(Axis axis, Position16<RoomScreens> screen, int pixel) => axis switch
    {
        Axis.Horizontal => screen.WithX(RoomGridConversions.PixelToScreen(pixel)),
        Axis.Vertical => screen.WithY(RoomGridConversions.PixelToScreen(pixel)),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null),
    };

    /// <summary>
    /// The screen the horizontal checks read: the camera's screen column, on the row through the camera's vertical
    /// centre (its top plus half a screen).
    /// </summary>
    private Position16<RoomScreens> HorizontalCheckScreen() => new(
        RoomGridConversions.PixelToScreen(Position.X.Whole),
        RoomGridConversions.PixelToScreen(CameraPastRoomEdge.At(Position.Y.Whole + SamusCameraDefinitions.HalfScreen)));

    /// <summary>
    /// The screen the vertical checks read: the camera's screen row, in the column through the camera's horizontal
    /// centre (its left edge plus half a screen).
    /// </summary>
    private Position16<RoomScreens> VerticalCheckScreen() => new(
        RoomGridConversions.PixelToScreen(CameraPastRoomEdge.At(Position.X.Whole + SamusCameraDefinitions.HalfScreen)),
        RoomGridConversions.PixelToScreen(Position.Y.Whole));

    /// <summary>
    /// The screen to the right of <paramref name="screen"/>. Right of the room's last column the grid reads on into
    /// its next row's first screen, as the camera's checks do.
    /// </summary>
    private static Position16<RoomScreens> RightOf(Position16<RoomScreens> screen) => screen.WithX(screen.X + 1);

    /// <summary>The screen below <paramref name="screen"/>.</summary>
    private static Position16<RoomScreens> Below(Position16<RoomScreens> screen) => screen.WithY(screen.Y + 1);

    /// <summary>The first pixel of the 256-pixel scroll screen holding <paramref name="pixel"/> (the 256-pixel alignment).</summary>
    private static int ScreenStart(int pixel) => RoomGridConversions.ScreenStart(pixel);

    /// <summary>The first pixel of the following scroll screen, wrapping as <see cref="CoordinateAxis"/>
    /// states.</summary>
    private static int NextScreenStart(int pixel) => CameraPastRoomEdge.At(ScreenStart(pixel) + 2 * SamusCameraDefinitions.HalfScreen);

    /// <summary>
    /// The distance the camera moved from <paramref name="previous"/> to <paramref name="current"/>, in whichever
    /// direction the whole pixels moved (<see cref="CameraPastRoomEdge.LiesBefore"/>), plus one whole pixel. When the whole
    /// pixels match and the fraction moved back, the sum is less than one pixel.
    /// </summary>
    private static SignedSixteenSixteen CalculateDistanceMovedPlusOne(
        FixedCoordinate previous,
        FixedCoordinate current)
    {
        SignedSixteenSixteen distance = !CameraPastRoomEdge.LiesBefore(current.Whole, previous.Whole)
            ? current - previous
            : previous - current;
        return distance + SignedSixteenSixteen.OnePixel;
    }

    /// <summary>Writes only the horizontal whole camera position, preserving its fraction and vertical axis.</summary>
    private void SetWholeX(int x) => Position = Position.WithX(Position.X.WithWhole(x));

    /// <summary>Writes only the vertical whole camera position, preserving its fraction and horizontal axis.</summary>
    private void SetWholeY(int y) => Position = Position.WithY(Position.Y.WithWhole(y));
}
