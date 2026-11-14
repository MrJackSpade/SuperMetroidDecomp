using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The camera position constrained by Super Metroid's directional scroll-zone handlers.
/// </summary>
/// <remarks>
/// The four directional handlers are the game's four scroll-boundary handlers. Samus's horizontal and vertical
/// tracking, run by the gameplay update's scrolling, supply the proposed position, ideal position and camera speed
/// they read.
/// </remarks>
[RoomLifetime]
public sealed class ScrollBoundaryCamera
{
    /// <summary>Half a 256-pixel screen: the camera's centre.</summary>
    private const int HalfScreen = 128;


    /// <summary>Creates a camera constrained by the supplied mutable room scroll grid.</summary>
    /// <param name="scrolls">Room-sized grid of directional scroll states.</param>
    public ScrollBoundaryCamera(RoomScrollGrid scrolls)
    {
        Scrolls = Ensure.NotNull(scrolls);
    }

    /// <summary>
    /// Gets the camera's 1/65536-pixel position: whole pixels and fraction.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Gets the camera's integer position (its top-left room pixel).</summary>
    public Position16<RoomPixels> PixelPosition => Position.Whole;

    /// <summary>
    /// Gets the camera target calculated from Samus's position: horizontally from her facing
    /// context, vertically from the scroller offsets.
    /// </summary>
    public Position16<RoomPixels> IdealPosition { get; internal set; }

    /// <summary>
    /// Gets the most recently calculated 1/65536-pixel camera speed on each axis:
    /// Samus's unsigned distance moved plus one pixel, held unsigned.
    /// </summary>
    public FixedVector<RoomPixels> Speed { get; internal set; }

    /// <summary>
    /// Samus's previous position written at the end of scrolling.
    /// Room-main movement occurs later and must remain visible to the next scroll call.
    /// Null represents a newly constructed camera without an accepted scrolling pass.
    /// </summary>
    public FixedPosition<RoomPixels>? PreviousSamusPoint { get; private set; }

    /// <summary>Publishes the previous-position checkpoint after both axes and the scrolling hook.</summary>
    public void FinishSamusScrolling(FixedPosition<RoomPixels> current) => PreviousSamusPoint = current;

    /// <summary>Door room replacement retains the global scrolling history and targets.</summary>
    internal void InheritTrackingState(ScrollBoundaryCamera previous)
    {
        IdealPosition = previous.IdealPosition;
        Speed = previous.Speed;
        PreviousSamusPoint = previous.PreviousSamusPoint;
    }

    /// <summary>Door placement writes both whole previous-position coordinates, retaining fractions.</summary>
    internal void PublishDoorSamusPosition(Position16<RoomPixels> samus) =>
        PreviousSamusPoint = (PreviousSamusPoint ?? default).WithWhole(samus);

    /// <summary>
    /// The door-transition scroll writes only the travel axis's whole previous position, retaining the
    /// other axis and both fractions.
    /// </summary>
    internal void PublishDoorSamusTravelAxis(bool vertical, Position16<RoomPixels> samus)
    {
        FixedPosition<RoomPixels> previous = PreviousSamusPoint ?? default;
        PreviousSamusPoint = vertical
            ? previous.WithY(previous.Y.WithWhole(samus.Y))
            : previous.WithX(previous.X.WithWhole(samus.X));
    }

    /// <summary>The room's scroll grid this camera consults.</summary>
    public RoomScrollGrid Scrolls { get; }

    /// <summary>
    /// Sets an entry/door camera position. This does only physical room-edge clamping;
    /// directional internal boundaries are evaluated when movement is attempted.
    /// </summary>
    public void SetPosition(Position16<RoomPixels> requested)
    {
        PreviousSamusPoint = null;
        int maxX = RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1);

        // The vertical maximum depends on whether the screen aligns the camera to its top. Starting with the
        // room-wide conservative +31 allowance matches the boundary check's largest legal result;
        // the first vertical movement applies the cell-specific exact maximum.
        int maxY = (RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + 31;
        var clamped = new Position16<RoomPixels>(
            CoordinateAxis.Wrap(Math.Clamp(requested.X, 0, maxX)),
            CoordinateAxis.Wrap(Math.Clamp(requested.Y, 0, maxY)));
        Position = FixedPosition<RoomPixels>.FromWhole(clamped);
        IdealPosition = clamped;
    }

    /// <summary>
    /// Keeps the fractions of the camera this one replaces.
    /// Room and game loads write only the integer positions, so the
    /// previous room's fractions remain.
    /// </summary>
    public void RetainSubpositions(ScrollBoundaryCamera previous) =>
        RetainSubpositions(previous.Position);

    /// <summary>
    /// Keeps the fractions of <paramref name="retained"/>, such as those that
    /// survived a released gameplay owner; the whole pixels are unchanged.
    /// </summary>
    public void RetainSubpositions(FixedPosition<RoomPixels> retained) =>
        Position = Position.WithFractionsOf(retained);

    /// <summary>
    /// Publishes the modular camera position the door-transition scroll owns.
    /// </summary>
    /// <remarks>
    /// The vertical transitions deliberately begin outside the destination room:
    /// down starts at destination Y minus 224 (often 65312) and up starts at
    /// destination Y plus 251. These are valid wrapped coordinates, not entry
    /// positions to clamp. Keeping this operation separate from <see cref="SetPosition"/>
    /// prevents ordinary loaders and debug callers from bypassing room-edge validation.
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
    /// Applies the direct camera X write the Shitroid's AI makes. Unlike
    /// entry placement, the game does not clear subposition or replace either ideal
    /// camera coordinate at this point in the gameplay frame.
    /// </summary>
    public void SetCameraXFromEnemyAi(int xPosition) => SetWholeX(xPosition);

    /// <summary>
    /// Applies a direct camera Y write made by a room-installed scrolling-finished
    /// hook. The game changes only the camera's whole Y at this point: fractional
    /// and ideal-camera values retain the values produced by ordinary scrolling.
    /// </summary>
    public void SetCameraYFromScrollingFinishedHook(int yPosition) =>
        SetWholeY(yPosition);

    /// <summary>
    /// Runs horizontal Samus tracking plus camera-speed calculation.
    /// Integer movement follows the facing-dependent camera target;
    /// an unchanged integer position dispatches to autoscrolling exactly
    /// as the game does (subpixel-only Samus movement still affects speed).
    /// </summary>
    public void TrackMovedSamusHorizontally(
        FixedPosition<RoomPixels> previous,
        FixedPosition<RoomPixels> current,
        HorizontalCameraContext context,
        bool timeIsFrozen = false)
    {
        if (!Enum.IsDefined(context.CameraDistance))
            throw new ArgumentOutOfRangeException(nameof(context), $"Undefined camera distance {context.CameraDistance}.");

        Speed = Speed.WithX(CalculateDistanceMovedPlusOne(previous.X, current.X));

        // Compares the whole pixels only. This is intentionally after speed
        // calculation: even when they match, differing fractional samples can make
        // the integer X speed used by autoscrolling zero, one, or (on wrap) much larger.
        if (previous.X.Whole == current.X.Whole)
        {
            HandleHorizontalAutoscrolling(timeIsFrozen);
            return;
        }

        // This XOR is a compact but exact projection of the camera target selection. Knockback,
        // moonwalking, or a turn that keeps the old momentum reverses which facing target
        // applies; every facing but left selects the right-facing target.
        bool backwards = context.KnockedBack
            || context.MovementType == SamusMovementType.Moonwalking
            || context.TurningAgainstMomentum;
        bool useFacingRightTarget = backwards ^ !context.FacesLeft;
        int targetOffset = HorizontalCameraTargets.Offset(context.CameraDistance, useFacingRightTarget);
        IdealPosition = IdealPosition.WithX(CoordinateAxis.Wrap(current.X.Whole - targetOffset));

        if (IdealPosition.X == Position.X.Whole)
            return;

        if (CoordinateAxis.Difference(IdealPosition.X, Position.X.Whole) < 0)
        {
            Position = Position.WithX((SignedSixteenSixteen.Zero - Speed.X).AddTo(Position.X.Whole, Position.X.Fraction));
            HandleScrollingLeft();
        }
        else
        {
            Position = Position.WithX(Speed.X.AddTo(Position.X.Whole, Position.X.Fraction));
            HandleScrollingRight();
        }
    }

    /// <summary>
    /// The fast-swing branch of the camera's scrolling: the camera
    /// steps three pixels toward a dead zone around Samus, then both autoscrollers run.
    /// The camera's scroll speeds are not recalculated. A negative Samus X skips both axes.
    /// </summary>
    public void TrackGrappleSlowScrolling(Position16<RoomPixels> samus, bool timeIsFrozen = false)
    {
        if (CoordinateAxis.AsSigned(samus.X) >= 0)
        {
            SetWholeX(StepTowardDeadZone(Position.X.Whole, samus.X,
                GrappleSlowScrollDefinitions.LeftEdgeX, GrappleSlowScrollDefinitions.RightEdgeX));
            if (CoordinateAxis.AsSigned(samus.Y) >= 0)
            {
                SetWholeY(StepTowardDeadZone(Position.Y.Whole, samus.Y,
                    GrappleSlowScrollDefinitions.TopEdgeY, GrappleSlowScrollDefinitions.BottomEdgeY));
            }
        }
        HandleHorizontalAutoscrolling(timeIsFrozen);
        HandleVerticalAutoscrolling(timeIsFrozen);
    }

    /// <summary>Moves the camera toward Samus until she lies within the grapple dead zone.</summary>
    /// <remarks>The subtraction is unsigned: Samus left of or above the camera borrows and scrolls back, as do screen
    /// offsets below the near edge; offsets at the far edge advance.</remarks>
    private static int StepTowardDeadZone(int camera, int samus, int nearEdge, int farEdge)
    {
        if (samus < camera)
            return CoordinateAxis.Wrap(camera - GrappleSlowScrollDefinitions.Step);
        int onScreen = CoordinateAxis.Wrap(samus - camera);
        if (onScreen >= farEdge)
            return CoordinateAxis.Wrap(camera + GrappleSlowScrollDefinitions.Step);
        if (onScreen < nearEdge)
            return CoordinateAxis.Wrap(camera - GrappleSlowScrollDefinitions.Step);
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
        bool timeIsFrozen = false)
    {
        Speed = Speed.WithY(CalculateDistanceMovedPlusOne(previous.Y, current.Y));

        if (previous.Y.Whole == current.Y.Whole)
        {
            HandleVerticalAutoscrolling(timeIsFrozen);
            return;
        }

        // Samus Y direction one means upward. The apparently reversed scroller choice is
        // literal: upward motion subtracts the down scroller, all else
        // subtracts the up scroller.
        int targetOffset = context.YDirection == SamusVerticalDirection.Up ? context.DownScroller : context.UpScroller;
        IdealPosition = IdealPosition.WithY(CoordinateAxis.Wrap(current.Y.Whole - targetOffset));

        if (IdealPosition.Y == Position.Y.Whole)
            return;

        if (CoordinateAxis.Difference(IdealPosition.Y, Position.Y.Whole) < 0)
        {
            Position = Position.WithY((SignedSixteenSixteen.Zero - Speed.Y).AddTo(Position.Y.Whole, Position.Y.Fraction));
            HandleScrollingUp();
        }
        else
        {
            Position = Position.WithY(Speed.Y.AddTo(Position.Y.Whole, Position.Y.Fraction));
            HandleScrollingDown();
        }
    }

    /// <summary>
    /// Horizontal scroll-zone autoscrolling.
    /// </summary>
    /// <remarks>
    /// This step does not chase Samus. It gently pushes a camera that is sitting inside
    /// a closed scroll cell toward a legal adjacent cell, or back from a closed cell on its
    /// right. The extra two pixels are the game's, not an invented smoothing rule.
    /// </remarks>
    private void HandleHorizontalAutoscrolling(bool timeIsFrozen)
    {
        // The game returns if either half of the time-freeze flag is set. Keep this gate local to the two autoscrollers;
        // the directional moved-Samus handlers themselves have no freeze check.
        if (timeIsFrozen)
            return;

        // The scroll records the pre-clamp position. Every proposed movement below starts from
        // this starting position, even if the live position is first clamped to a room edge.
        int proposed = Position.X.Whole;

        if (CoordinateAxis.AsSigned(Position.X.Whole) < 0)
            SetWholeX(0);

        int roomMaximum = CoordinateAxis.Wrap(RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1));
        if (Position.X.Whole > roomMaximum)
            SetWholeX(roomMaximum);

        int row = (EightEight.WholeOf(CoordinateAxis.Wrap(Position.Y.Whole + HalfScreen))) * Scrolls.Size.X;
        int currentCell = row + (EightEight.WholeOf(Position.X.Whole));

        if (Scrolls.ReadCameraState(currentCell) == RoomScrollState.Closed)
        {
            // In a red current cell, drift right by camera speed + 2 toward its right edge.
            // If the next cell is also red, discard the fractional screen position and stay
            // on this cell's left edge instead of allowing the viewport to enter it.
            int rightBoundary = NextScreenStart(Position.X.Whole);
            int candidate = CoordinateAxis.Wrap(proposed + Speed.X.WholePixels + 2);
            if (candidate >= rightBoundary)
            {
                SetWholeX(rightBoundary);
                return;
            }

            proposed = candidate;
            int candidateRightCell = row + RoomGridConversions.PixelToScreen(proposed) + 1;
            SetWholeX(Scrolls.ReadCameraState(candidateRightCell) != RoomScrollState.Closed
                ? proposed
                : ScreenStart(proposed));
            return;
        }

        if (Scrolls.ReadCameraState(currentCell + 1) != RoomScrollState.Closed)
            return;

        // A legal current cell with a closed cell to its right is the mirror case: drift left
        // toward the current cell's left edge. If the candidate itself lands in a closed cell, round
        // up to the next screen boundary. The comparison is signed.
        int leftBoundary = ScreenStart(Position.X.Whole);
        int leftCandidate = CoordinateAxis.Wrap(proposed - Speed.X.WholePixels - 2);
        if (CoordinateAxis.Difference(leftCandidate, leftBoundary) < 0)
        {
            SetWholeX(leftBoundary);
            return;
        }

        proposed = leftCandidate;
        int candidateCell = row + RoomGridConversions.PixelToScreen(proposed);
        SetWholeX(Scrolls.ReadCameraState(candidateCell) != RoomScrollState.Closed
            ? proposed
            : NextScreenStart(proposed));
    }

    /// <summary>
    /// Vertical scroll-zone autoscrolling.
    /// </summary>
    private void HandleVerticalAutoscrolling(bool timeIsFrozen)
    {
        if (timeIsFrozen)
            return;

        int centeredXScreen = EightEight.WholeOf(CoordinateAxis.Wrap(Position.X.Whole + HalfScreen));
        int preClampCell = (EightEight.WholeOf(Position.Y.Whole)) * Scrolls.Size.X + centeredXScreen;

        // Screens open with the camera aligned to the top align the 224-line gameplay viewport to
        // the top of the screen; closed and open ones permit the game's 31-pixel lower alignment. Crucially, the
        // alignment is selected before Y is clamped and remains fixed for this invocation.
        int verticalAlignment = Scrolls.ReadCameraState(preClampCell) == RoomScrollState.OpenAlignedToTop
            ? 0
            : 31;
        int proposed = Position.Y.Whole;

        if (CoordinateAxis.AsSigned(Position.Y.Whole) < 0)
            SetWholeY(0);

        int roomMaximum = CoordinateAxis.Wrap((RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + verticalAlignment);
        if (Position.Y.Whole > roomMaximum)
            SetWholeY(roomMaximum);

        int currentCell = (EightEight.WholeOf(Position.Y.Whole)) * Scrolls.Size.X + centeredXScreen;
        if (Scrolls.ReadCameraState(currentCell) == RoomScrollState.Closed)
        {
            // A closed current cell pushes downward toward its bottom boundary. As in the X
            // step, a second closed cell beyond the candidate causes screen-edge rounding.
            int bottomBoundary = NextScreenStart(Position.Y.Whole);
            int candidate = CoordinateAxis.Wrap(proposed + Speed.Y.WholePixels + 2);
            if (candidate >= bottomBoundary)
            {
                SetWholeY(bottomBoundary);
                return;
            }

            proposed = candidate;
            int belowCandidate = (RoomGridConversions.PixelToScreen(candidate) + 1) * Scrolls.Size.X + centeredXScreen;
            SetWholeY(Scrolls.ReadCameraState(belowCandidate) != RoomScrollState.Closed
                ? proposed
                : ScreenStart(proposed));
            return;
        }

        if (Scrolls.ReadCameraState(currentCell + Scrolls.Size.X) !=
            RoomScrollState.Closed)
            return;

        int topBoundary = CoordinateAxis.Wrap(ScreenStart(Position.Y.Whole) + verticalAlignment);

        // The comparison only pulls the camera upward when it is strictly below the
        // aligned boundary. Equality intentionally does nothing.
        if (topBoundary >= Position.Y.Whole)
            return;

        int upwardCandidate = CoordinateAxis.Wrap(proposed - Speed.Y.WholePixels - 2);
        if (CoordinateAxis.Difference(upwardCandidate, topBoundary) < 0)
        {
            SetWholeY(topBoundary);
            return;
        }

        proposed = upwardCandidate;
        int candidateCell = RoomGridConversions.PixelToScreen(upwardCandidate) * Scrolls.Size.X + centeredXScreen;
        SetWholeY(Scrolls.ReadCameraState(candidateCell) != RoomScrollState.Closed
            ? proposed
            : NextScreenStart(proposed));
    }

    /// <summary>Stops rightward travel at the ideal point, room edge, or closed cell to the right.</summary>
    internal void HandleScrollingRight()
    {
        int proposed = Position.X.Whole;

        // The signed comparison of ideal against current: if movement overshot the ideal in signed
        // wrapped space, snap to it and discard the fractional camera position.
        if (CoordinateAxis.Difference(IdealPosition.X, Position.X.Whole) < 0)
        {
            // Overshoot snaps to the ideal position and discards the fractional camera position.
            Position = Position.WithX(new FixedCoordinate(IdealPosition.X, 0));
        }

        int roomMaximum = CoordinateAxis.Wrap(RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1));
        if (Position.X.Whole > roomMaximum)
        {
            SetWholeX(roomMaximum);
            return;
        }

        // The horizontal checks sample the row containing the screen's vertical center
        // (Y plus half a screen), then the screen immediately to the right of the camera's screen column.
        int row = (EightEight.WholeOf(CoordinateAxis.Wrap(Position.Y.Whole + HalfScreen))) * Scrolls.Size.X;
        int rightCell = row + (EightEight.WholeOf(Position.X.Whole)) + 1;
        if (Scrolls.ReadCameraState(rightCell) != RoomScrollState.Closed)
            return;

        int boundary = ScreenStart(Position.X.Whole);
        int candidate = CoordinateAxis.Wrap(proposed - Speed.X.WholePixels - 2);
        if (CoordinateAxis.Difference(candidate, boundary) < 0)
            candidate = boundary;
        SetWholeX(candidate);
    }

    /// <summary>Stops leftward travel at the ideal point, room edge, or current closed cell.</summary>
    internal void HandleScrollingLeft()
    {
        int proposed = Position.X.Whole;
        if (CoordinateAxis.Difference(Position.X.Whole, IdealPosition.X) < 0)
        {
            // Overshoot snaps to the ideal position and discards the fractional camera position.
            Position = Position.WithX(new FixedCoordinate(IdealPosition.X, 0));
        }

        if (CoordinateAxis.AsSigned(Position.X.Whole) < 0)
        {
            SetWholeX(0);
            return;
        }

        int row = (EightEight.WholeOf(CoordinateAxis.Wrap(Position.Y.Whole + HalfScreen))) * Scrolls.Size.X;
        int currentCell = row + (EightEight.WholeOf(Position.X.Whole));
        if (Scrolls.ReadCameraState(currentCell) != RoomScrollState.Closed)
            return;

        int boundary = NextScreenStart(Position.X.Whole);
        int candidate = CoordinateAxis.Wrap(proposed + Speed.X.WholePixels + 2);
        if (candidate >= boundary)
            candidate = boundary;
        SetWholeX(candidate);
    }

    /// <summary>Applies the top-aligned versus closed/open vertical alignment and blocks downward travel at room or scroll limits.</summary>
    internal void HandleScrollingDown()
    {
        int proposed = Position.Y.Whole;
        int currentCell = VerticalCellIndex();

        // Top-aligned screens align the 224-line playfield at +0; closed and open ones use +31.
        int verticalAlignment = Scrolls.ReadCameraState(currentCell) == RoomScrollState.OpenAlignedToTop
            ? 0
            : 31;

        if (CoordinateAxis.Difference(IdealPosition.Y, Position.Y.Whole) < 0)
        {
            // Overshoot snaps to the ideal position and discards the fractional camera position.
            Position = Position.WithY(new FixedCoordinate(IdealPosition.Y, 0));
        }

        int boundary = CoordinateAxis.Wrap((RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + verticalAlignment);
        bool beyondRoomBottom = boundary < Position.Y.Whole;
        bool blockedBelow = false;
        if (!beyondRoomBottom)
        {
            // Preserve the game's branch order: if the physical room maximum was already
            // exceeded, the boundary check branches before reading the cell below. This also prevents
            // a legitimate bottom-row/right-edge camera from indexing beyond the scroll grid.
            blockedBelow = Scrolls.ReadCameraState(currentCell + Scrolls.Size.X) ==
                RoomScrollState.Closed;
            if (blockedBelow)
            {
                boundary = CoordinateAxis.Wrap(ScreenStart(Position.Y.Whole) + verticalAlignment);
                blockedBelow = boundary < Position.Y.Whole;
            }
        }

        if (beyondRoomBottom || blockedBelow)
        {
            int candidate = CoordinateAxis.Wrap(proposed - Speed.Y.WholePixels - 2);
            if (CoordinateAxis.Difference(candidate, boundary) < 0)
                candidate = boundary;
            SetWholeY(candidate);
        }
    }

    /// <summary>Stops upward travel at the ideal point, room top, or current closed cell.</summary>
    internal void HandleScrollingUp()
    {
        int proposed = Position.Y.Whole;
        if (CoordinateAxis.Difference(Position.Y.Whole, IdealPosition.Y) < 0)
        {
            // Overshoot snaps to the ideal position and discards the fractional camera position.
            Position = Position.WithY(new FixedCoordinate(IdealPosition.Y, 0));
        }

        if (CoordinateAxis.AsSigned(Position.Y.Whole) < 0)
        {
            SetWholeY(0);
            return;
        }

        int currentCell = VerticalCellIndex();
        if (Scrolls.ReadCameraState(currentCell) != RoomScrollState.Closed)
            return;

        int boundary = NextScreenStart(Position.Y.Whole);
        int candidate = CoordinateAxis.Wrap(proposed + Speed.Y.WholePixels + 2);
        if (candidate >= boundary)
            candidate = boundary;
        SetWholeY(candidate);
    }

    /// <summary>Samples the scroll cell at the camera's horizontal midpoint for vertical boundaries.</summary>
    private int VerticalCellIndex()
    {
        int xScreenAtCenter = EightEight.WholeOf(CoordinateAxis.Wrap(Position.X.Whole + HalfScreen));
        int yScreen = EightEight.WholeOf(Position.Y.Whole);
        return yScreen * Scrolls.Size.X + xScreenAtCenter;
    }

    /// <summary>The first pixel of the 256-pixel scroll screen holding <paramref name="pixel"/> (the 256-pixel alignment).</summary>
    private static int ScreenStart(int pixel) => RoomGridConversions.ScreenStart(pixel);

    /// <summary>The first pixel of the following scroll screen, wrapping at sixteen bits.</summary>
    private static int NextScreenStart(int pixel) => CoordinateAxis.Wrap(ScreenStart(pixel) + 2 * HalfScreen);

    /// <summary>
    /// Compares only the whole pixels as signed wrapped values, then
    /// subtracts the complete 1/65536-pixel pair in the selected direction and adds exactly
    /// one whole pixel. Modular arithmetic keeps the game's noted subpixel corner case, where
    /// the result can still be less than 1.0.
    /// </summary>
    private static SignedSixteenSixteen CalculateDistanceMovedPlusOne(
        FixedCoordinate previous,
        FixedCoordinate current)
    {
        SignedSixteenSixteen distance = CoordinateAxis.Difference(current.Whole, previous.Whole) >= 0
            ? current - previous
            : previous - current;
        return distance + OnePixel;
    }

    /// <summary>The whole pixel the distance moved gains.</summary>
    private static readonly SignedSixteenSixteen OnePixel = SignedSixteenSixteen.FromParts(1, 0);

    /// <summary>Writes only the horizontal whole camera position, preserving its fraction and vertical axis.</summary>
    private void SetWholeX(int x) => Position = Position.WithX(Position.X.WithWhole(x));

    /// <summary>Writes only the vertical whole camera position, preserving its fraction and horizontal axis.</summary>
    private void SetWholeY(int y) => Position = Position.WithY(Position.Y.WithWhole(y));
}
