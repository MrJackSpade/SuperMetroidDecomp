using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Layer-1 camera position constrained by Super Metroid's directional scroll-zone handlers.
/// </summary>
/// <remarks>
/// The four private handlers are direct ports of <c>$80:A641</c>, <c>$80:A6BB</c>,
/// <c>$80:A893</c>, and <c>$80:A936</c>. The public directional methods are host/debug
/// stimuli: genuine Samus tracking at <c>$90:95A0</c> will eventually supply the same
/// proposed position, ideal position, and fixed-point camera speed fields.
/// </remarks>
public sealed class ScrollBoundaryCamera
{

    /// <summary>Creates a camera constrained by the supplied mutable room scroll grid.</summary>
    /// <param name="scrolls">Room-sized grid of native directional scroll states.</param>
    public ScrollBoundaryCamera(RoomScrollGrid scrolls)
    {
        Scrolls = Ensure.NotNull(scrolls);
    }

    /// <summary>
    /// Gets the layer-one 16.16 position: the integer words <c>$0911/$0915</c> and the fraction
    /// words <c>$090F/$0913</c>.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Gets the layer-one integer position (the camera's top-left room pixel).</summary>
    public Position16<RoomPixels> PixelPosition => Position.Whole;

    /// <summary>
    /// Gets the camera target calculated from Samus's position: horizontally from her facing
    /// context, vertically from the scroller offsets (<c>$0939/$093B</c>).
    /// </summary>
    public Position16<RoomPixels> IdealPosition { get; internal set; }

    /// <summary>
    /// Gets the most recently calculated 16.16 camera speed on each axis (<c>$0DA0-$0DA6</c>):
    /// Samus's unsigned distance moved plus one pixel, held as its native word pattern.
    /// </summary>
    public FixedVector<RoomPixels> Speed { get; internal set; }

    /// <summary>
    /// Samus's four previous-position words written at the end of $90:94EC.
    /// Room-main movement occurs later and must remain visible to the next scroll call.
    /// Null represents a newly constructed camera without an accepted scrolling pass.
    /// </summary>
    public FixedPosition<RoomPixels>? PreviousSamusPoint { get; private set; }

    /// <summary>Publishes the native previous-position checkpoint after both axes and the scrolling hook.</summary>
    public void FinishSamusScrolling(FixedPosition<RoomPixels> current) => PreviousSamusPoint = current;

    /// <summary>Door room replacement retains the global scrolling history and targets.</summary>
    internal void InheritTrackingState(ScrollBoundaryCamera previous)
    {
        IdealPosition = previous.IdealPosition;
        Speed = previous.Speed;
        PreviousSamusPoint = previous.PreviousSamusPoint;
    }

    /// <summary>Door placement writes both whole previous-position words, retaining fractions.</summary>
    internal void PublishDoorSamusPosition(Position16<RoomPixels> samus) =>
        PreviousSamusPoint = (PreviousSamusPoint ?? default).WithWhole(samus);

    /// <summary>
    /// The door IRQ writes only the travel axis's whole previous-position word, retaining the
    /// other axis and both fractions.
    /// </summary>
    internal void PublishDoorSamusTravelAxis(bool vertical, Position16<RoomPixels> samus)
    {
        FixedPosition<RoomPixels> previous = PreviousSamusPoint ?? default;
        PreviousSamusPoint = vertical
            ? previous.WithY(previous.Y.WithWhole(samus.Y))
            : previous.WithX(previous.X.WithWhole(samus.X));
    }

    /// <summary>The mutable 50-byte scroll table consulted by this camera.</summary>
    public RoomScrollGrid Scrolls { get; }

    /// <summary>
    /// Sets an entry/door camera position. This does only physical room-edge clamping;
    /// directional internal boundaries are evaluated when movement is attempted.
    /// </summary>
    public void SetPosition(Position16<RoomPixels> requested)
    {
        PreviousSamusPoint = null;
        int maxX = RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1);

        // The vertical maximum depends on blue/green scroll alignment. Starting with the
        // room-wide conservative +$1F allowance matches $80:A731's largest legal result;
        // the first vertical movement applies the cell-specific exact maximum.
        int maxY = (RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + 0x1f;
        var clamped = new Position16<RoomPixels>(
            (ushort)Math.Clamp(requested.X, 0, maxX),
            (ushort)Math.Clamp(requested.Y, 0, maxY));
        Position = FixedPosition<RoomPixels>.FromWhole(clamped);
        IdealPosition = clamped;
    }

    /// <summary>
    /// Keeps the layer-one fraction words ($090F/$0913) of the camera this one replaces.
    /// Room and game loads write only the integer positions (e.g. <c>$80:C470</c>), so the
    /// previous room's fractions remain in RAM.
    /// </summary>
    public void RetainSubpositions(ScrollBoundaryCamera previous) =>
        RetainSubpositions(previous.Position);

    /// <summary>
    /// Keeps the layer-one fraction words of <paramref name="retained"/>, such as those that
    /// survived a released gameplay owner; the integer words are unchanged.
    /// </summary>
    public void RetainSubpositions(FixedPosition<RoomPixels> retained) =>
        Position = Position.WithFractionsOf(retained);

    /// <summary>
    /// Publishes the raw modular layer-one words owned by the door-opening IRQ.
    /// </summary>
    /// <remarks>
    /// The vertical transition routines deliberately begin outside the destination room:
    /// down starts at destination Y minus <c>$E0</c> (often <c>$FF20</c>) and up starts at
    /// destination Y plus <c>$FB</c>. These are valid 65C816 coordinates, not entry
    /// positions to clamp. Keeping this operation separate from <see cref="SetPosition"/>
    /// prevents ordinary loaders and debug callers from bypassing room-edge validation.
    /// </remarks>
    public void SetDoorTransitionPosition(Position16<RoomPixels> position) =>
        // Door IRQ/setup writes integer layer-one positions, leaving both fractions intact.
        Position = Position.WithWhole(position);

    /// <summary>
    /// Transfers the retained native camera fractions when loading a door's new scroll grid:
    /// the integer words come from <paramref name="position"/>, the fractions from
    /// <paramref name="retainedFractions"/>.
    /// </summary>
    public void SetDoorTransitionPosition(Position16<RoomPixels> position, FixedPosition<RoomPixels> retainedFractions) =>
        Position = retainedFractions.WithWhole(position);

    /// <summary>
    /// Applies the direct <c>layer1_x_pos</c> write used by bank-$A9 Shitroid AI. Unlike
    /// entry placement, the cartridge does not clear subposition or replace either ideal
    /// camera coordinate at this point in the gameplay frame.
    /// </summary>
    public void SetLayerOneXFromEnemyAi(ushort xPosition) => SetWholeX(xPosition);

    /// <summary>
    /// Applies a direct layer-one Y write made by a room-installed scrolling-finished
    /// hook. The cartridge changes only <c>Layer1YPosition</c> at this point: fractional
    /// and ideal-camera words retain the values produced by ordinary scrolling.
    /// </summary>
    public void SetLayerOneYFromScrollingFinishedHook(ushort yPosition) =>
        SetWholeY(yPosition);

    /// <summary>
    /// Ports horizontal Samus tracking at <c>$90:95A0</c> plus camera-speed calculation
    /// <c>$90:96C0</c>. Integer movement follows the facing-dependent camera target;
    /// an unchanged integer position dispatches to <c>$80:A528</c> autoscrolling exactly
    /// as the original routine does (subpixel-only Samus movement still affects speed).
    /// </summary>
    public void TrackMovedSamusHorizontally(
        FixedPosition<RoomPixels> previous,
        FixedPosition<RoomPixels> current,
        HorizontalCameraContext context,
        bool timeIsFrozen = false)
    {
        if (!Enum.IsDefined(context.CameraDistanceIndex))
            throw new ArgumentOutOfRangeException(nameof(context), "Camera distance index must be byte offset 0, 2, 4, or 6.");

        Speed = Speed.WithX(CalculateDistanceMovedPlusOne(previous.X, current.X));

        // $90:95A8 compares the integer words only. This is intentionally after speed
        // calculation: even when those words match, differing fractional samples can make
        // the integer X speed used by $80:A528 zero, one, or (on wrap) much larger.
        if (previous.X.Whole == current.X.Whole)
        {
            HandleHorizontalAutoscrolling(timeIsFrozen);
            return;
        }

        // This XOR is a compact but exact projection of $90:95B7-$90:95E5. Knockback,
        // moonwalk movement type $10, or acceleration mode one reverses which facing target
        // applies; pose X direction four is the ROM's right-facing value.
        bool backwards = context.KnockbackDirection != 0
            || context.MovementType == SamusMovementType.Moonwalking
            || context.XAccelerationMode == 1;
        bool useFacingRightTarget = backwards ^ (context.PoseXDirection != 4);
        ushort targetOffset = HorizontalCameraTargetDefinitions.Offset(context.CameraDistanceIndex, useFacingRightTarget);
        IdealPosition = IdealPosition.WithX(unchecked((ushort)(current.X.Whole - targetOffset)));

        if (IdealPosition.X == Position.X.Whole)
            return;

        if (SignedDifference(IdealPosition.X, Position.X.Whole) < 0)
        {
            Position = Position.WithX((SnesSignedSixteenSixteen.Zero - Speed.X).AddTo(Position.X.Whole, Position.X.Fraction));
            HandleScrollingLeft();
        }
        else
        {
            Position = Position.WithX(Speed.X.AddTo(Position.X.Whole, Position.X.Fraction));
            HandleScrollingRight();
        }
    }

    /// <summary>
    /// The fast-swing branch of <c>Main_Scrolling_Routine</c> ($90:94F7-$9555): layer 1
    /// steps three pixels toward a dead zone around Samus, then both autoscrollers run.
    /// The camera speed words are not recalculated. A negative Samus X skips both axes.
    /// </summary>
    public void TrackGrappleSlowScrolling(Position16<RoomPixels> samus, bool timeIsFrozen = false)
    {
        if (unchecked((short)samus.X) >= 0)
        {
            SetWholeX(StepTowardDeadZone(Position.X.Whole, samus.X,
                GrappleSlowScrollDefinitions.LeftEdgeX, GrappleSlowScrollDefinitions.RightEdgeX));
            if (unchecked((short)samus.Y) >= 0)
            {
                SetWholeY(StepTowardDeadZone(Position.Y.Whole, samus.Y,
                    GrappleSlowScrollDefinitions.TopEdgeY, GrappleSlowScrollDefinitions.BottomEdgeY));
            }
        }
        HandleHorizontalAutoscrolling(timeIsFrozen);
        HandleVerticalAutoscrolling(timeIsFrozen);
    }

    // The subtraction is unsigned: Samus left of or above the camera borrows and scrolls
    // back, as do screen offsets below the near edge; offsets at the far edge advance.
    private static ushort StepTowardDeadZone(ushort camera, ushort samus, ushort nearEdge, ushort farEdge)
    {
        if (samus < camera)
            return unchecked((ushort)(camera - GrappleSlowScrollDefinitions.Step));
        ushort onScreen = unchecked((ushort)(samus - camera));
        if (onScreen >= farEdge)
            return unchecked((ushort)(camera + GrappleSlowScrollDefinitions.Step));
        if (onScreen < nearEdge)
            return unchecked((ushort)(camera - GrappleSlowScrollDefinitions.Step));
        return camera;
    }

    /// <summary>
    /// Ports vertical Samus tracking at <c>$90:964F</c> plus distance calculation
    /// <c>$90:96FF</c>. An unchanged integer Y position runs <c>$80:A731</c>.
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
        // literal $90:9666-$90:9681: upward motion subtracts down_scroller, all else
        // subtracts up_scroller.
        ushort targetOffset = context.YDirection == SamusVerticalDirection.Up ? context.DownScroller : context.UpScroller;
        IdealPosition = IdealPosition.WithY(unchecked((ushort)(current.Y.Whole - targetOffset)));

        if (IdealPosition.Y == Position.Y.Whole)
            return;

        if (SignedDifference(IdealPosition.Y, Position.Y.Whole) < 0)
        {
            Position = Position.WithY((SnesSignedSixteenSixteen.Zero - Speed.Y).AddTo(Position.Y.Whole, Position.Y.Fraction));
            HandleScrollingUp();
        }
        else
        {
            Position = Position.WithY(Speed.Y.AddTo(Position.Y.Whole, Position.Y.Fraction));
            HandleScrollingDown();
        }
    }

    /// <summary>
    /// Direct port of horizontal scroll-zone autoscrolling at <c>$80:A528-$80:A640</c>.
    /// </summary>
    /// <remarks>
    /// This routine does not chase Samus. It gently pushes a camera that is sitting inside
    /// a red (zero) scroll cell toward a legal adjacent cell, or back from a red cell on its
    /// right. The extra two pixels are present in the ROM and are not a host smoothing rule.
    /// </remarks>
    private void HandleHorizontalAutoscrolling(bool timeIsFrozen)
    {
        // The original reads both bytes of $0A78 while the accumulator is eight-bit and
        // returns if either is nonzero. Keep this gate local to the two autoscrollers;
        // the directional moved-Samus handlers themselves have no freeze check.
        if (timeIsFrozen)
            return;

        // $0939 captures the pre-clamp position. Every proposed movement below starts from
        // this original word, even if the live position is first clamped to a room edge.
        ushort proposed = Position.X.Whole;

        if ((short)Position.X.Whole < 0)
            SetWholeX(0);

        ushort roomMaximum = (ushort)(RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1));
        if (Position.X.Whole > roomMaximum)
            SetWholeX(roomMaximum);

        int row = (SnesEightEight.WholeOf((ushort)(Position.Y.Whole + 0x0080))) * Scrolls.Size.X;
        int currentCell = row + (SnesEightEight.WholeOf(Position.X.Whole));

        if (Scrolls.ReadNativeState(currentCell) == RoomScrollState.RedBoundary)
        {
            // In a red current cell, drift right by camera speed + 2 toward its right edge.
            // If the next cell is also red, discard the fractional screen position and stay
            // on this cell's left edge instead of allowing the viewport to enter it.
            ushort rightBoundary = NextScreenStart(Position.X.Whole);
            ushort candidate = unchecked((ushort)(proposed + Speed.X.WholeWord + 2));
            if (candidate >= rightBoundary)
            {
                SetWholeX(rightBoundary);
                return;
            }

            proposed = candidate;
            int candidateRightCell = row + (SnesLittleEndian.HighByte(proposed)) + 1;
            SetWholeX(Scrolls.ReadNativeState(candidateRightCell) != RoomScrollState.RedBoundary
                ? proposed
                : ScreenStart(proposed));
            return;
        }

        if (Scrolls.ReadNativeState(currentCell + 1) != RoomScrollState.RedBoundary)
            return;

        // A legal current cell with a red cell to its right is the mirror case: drift left
        // toward the current cell's left edge. If the candidate itself lands in red, round
        // up to the next screen boundary. Signed comparison reproduces BMI at $80:A5F9.
        ushort leftBoundary = ScreenStart(Position.X.Whole);
        ushort leftCandidate = unchecked((ushort)(proposed - Speed.X.WholeWord - 2));
        if (SignedDifference(leftCandidate, leftBoundary) < 0)
        {
            SetWholeX(leftBoundary);
            return;
        }

        proposed = leftCandidate;
        int candidateCell = row + (SnesLittleEndian.HighByte(proposed));
        SetWholeX(Scrolls.ReadNativeState(candidateCell) != RoomScrollState.RedBoundary
            ? proposed
            : NextScreenStart(proposed));
    }

    /// <summary>
    /// Direct port of vertical scroll-zone autoscrolling at <c>$80:A731-$80:A892</c>.
    /// </summary>
    private void HandleVerticalAutoscrolling(bool timeIsFrozen)
    {
        if (timeIsFrozen)
            return;

        int centeredXScreen = SnesEightEight.WholeOf((ushort)(Position.X.Whole + 0x0080));
        int preClampCell = (SnesEightEight.WholeOf(Position.Y.Whole)) * Scrolls.Size.X + centeredXScreen;

        // Blue scroll cells align the 224-line gameplay viewport to the top of the screen;
        // red and green cells permit the ROM's $1F-pixel lower alignment. Crucially, the
        // alignment is selected before Y is clamped and remains fixed for this invocation.
        ushort verticalAlignment = Scrolls.ReadNativeState(preClampCell) == RoomScrollState.Blue
            ? (ushort)0
            : (ushort)0x001f;
        ushort proposed = Position.Y.Whole;

        if ((short)Position.Y.Whole < 0)
            SetWholeY(0);

        ushort roomMaximum = unchecked((ushort)((RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + verticalAlignment));
        if (Position.Y.Whole > roomMaximum)
            SetWholeY(roomMaximum);

        int currentCell = (SnesEightEight.WholeOf(Position.Y.Whole)) * Scrolls.Size.X + centeredXScreen;
        if (Scrolls.ReadNativeState(currentCell) == RoomScrollState.RedBoundary)
        {
            // A red current cell pushes downward toward its bottom boundary. As in the X
            // routine, a second red cell beyond the candidate causes screen-edge rounding.
            ushort bottomBoundary = NextScreenStart(Position.Y.Whole);
            ushort candidate = unchecked((ushort)(proposed + Speed.Y.WholeWord + 2));
            if (candidate >= bottomBoundary)
            {
                SetWholeY(bottomBoundary);
                return;
            }

            proposed = candidate;
            int belowCandidate = ((SnesLittleEndian.HighByte(candidate)) + 1) * Scrolls.Size.X + centeredXScreen;
            SetWholeY(Scrolls.ReadNativeState(belowCandidate) != RoomScrollState.RedBoundary
                ? proposed
                : ScreenStart(proposed));
            return;
        }

        if (Scrolls.ReadNativeState(currentCell + Scrolls.Size.X) !=
            RoomScrollState.RedBoundary)
            return;

        ushort topBoundary = unchecked((ushort)(ScreenStart(Position.Y.Whole) + verticalAlignment));

        // CMP/BCC at $80:A82D only pulls the camera upward when it is strictly below the
        // aligned boundary. Equality intentionally does nothing.
        if (topBoundary >= Position.Y.Whole)
            return;

        ushort upwardCandidate = unchecked((ushort)(proposed - Speed.Y.WholeWord - 2));
        if (SignedDifference(upwardCandidate, topBoundary) < 0)
        {
            SetWholeY(topBoundary);
            return;
        }

        proposed = upwardCandidate;
        int candidateCell = (SnesLittleEndian.HighByte(upwardCandidate)) * Scrolls.Size.X + centeredXScreen;
        SetWholeY(Scrolls.ReadNativeState(candidateCell) != RoomScrollState.RedBoundary
            ? proposed
            : NextScreenStart(proposed));
    }

    internal void HandleScrollingRight()
    {
        ushort proposed = Position.X.Whole;

        // CMP ideal,current / BPL in $80:A651: if movement overshot the ideal in signed
        // 16-bit space, snap to it and discard the fractional camera position.
        if (SignedDifference(IdealPosition.X, Position.X.Whole) < 0)
        {
            // Overshoot snaps to the ideal word and discards the fractional camera position.
            Position = Position.WithX(new SnesFixedPosition(IdealPosition.X, 0));
        }

        ushort roomMaximum = (ushort)(RoomGridConversions.ScreenToPixels(Scrolls.Size.X - 1));
        if (Position.X.Whole > roomMaximum)
        {
            SetWholeX(roomMaximum);
            return;
        }

        // The horizontal routines sample the row containing the screen's vertical center
        // (Y+$80), then the screen immediately to the right of the camera's high X byte.
        int row = (SnesEightEight.WholeOf((ushort)(Position.Y.Whole + 0x0080))) * Scrolls.Size.X;
        int rightCell = row + (SnesEightEight.WholeOf(Position.X.Whole)) + 1;
        if (Scrolls.ReadNativeState(rightCell) != RoomScrollState.RedBoundary)
            return;

        ushort boundary = ScreenStart(Position.X.Whole);
        ushort candidate = unchecked((ushort)(proposed - Speed.X.WholeWord - 2));
        if (SignedDifference(candidate, boundary) < 0)
            candidate = boundary;
        SetWholeX(candidate);
    }

    internal void HandleScrollingLeft()
    {
        ushort proposed = Position.X.Whole;
        if (SignedDifference(Position.X.Whole, IdealPosition.X) < 0)
        {
            // Overshoot snaps to the ideal word and discards the fractional camera position.
            Position = Position.WithX(new SnesFixedPosition(IdealPosition.X, 0));
        }

        if ((short)Position.X.Whole < 0)
        {
            SetWholeX(0);
            return;
        }

        int row = (SnesEightEight.WholeOf((ushort)(Position.Y.Whole + 0x0080))) * Scrolls.Size.X;
        int currentCell = row + (SnesEightEight.WholeOf(Position.X.Whole));
        if (Scrolls.ReadNativeState(currentCell) != RoomScrollState.RedBoundary)
            return;

        ushort boundary = NextScreenStart(Position.X.Whole);
        ushort candidate = unchecked((ushort)(proposed + Speed.X.WholeWord + 2));
        if (candidate >= boundary)
            candidate = boundary;
        SetWholeX(candidate);
    }

    internal void HandleScrollingDown()
    {
        ushort proposed = Position.Y.Whole;
        int currentCell = VerticalCellIndex();

        // Blue scrolls align the 224-line playfield at +0; red/green scrolls use +$1F.
        // This is the LDY #0 / CMP #1 / LDY #$1F sequence at $80:A8A8-$80:A8C8.
        ushort verticalAlignment = Scrolls.ReadNativeState(currentCell) == RoomScrollState.Blue
            ? (ushort)0
            : (ushort)0x001f;

        if (SignedDifference(IdealPosition.Y, Position.Y.Whole) < 0)
        {
            // Overshoot snaps to the ideal word and discards the fractional camera position.
            Position = Position.WithY(new SnesFixedPosition(IdealPosition.Y, 0));
        }

        ushort boundary = unchecked((ushort)((RoomGridConversions.ScreenToPixels(Scrolls.Size.Y - 1)) + verticalAlignment));
        bool beyondRoomBottom = boundary < Position.Y.Whole;
        bool blockedBelow = false;
        if (!beyondRoomBottom)
        {
            // Preserve the assembly branch order: if the physical room maximum was already
            // exceeded, $80:A8F2 branches before reading the cell below. This also prevents
            // a legitimate bottom-row/right-edge camera from indexing beyond $7E:CD51.
            blockedBelow = Scrolls.ReadNativeState(currentCell + Scrolls.Size.X) ==
                RoomScrollState.RedBoundary;
            if (blockedBelow)
            {
                boundary = unchecked((ushort)(ScreenStart(Position.Y.Whole) + verticalAlignment));
                blockedBelow = boundary < Position.Y.Whole;
            }
        }

        if (beyondRoomBottom || blockedBelow)
        {
            ushort candidate = unchecked((ushort)(proposed - Speed.Y.WholeWord - 2));
            if (SignedDifference(candidate, boundary) < 0)
                candidate = boundary;
            SetWholeY(candidate);
        }
    }

    internal void HandleScrollingUp()
    {
        ushort proposed = Position.Y.Whole;
        if (SignedDifference(Position.Y.Whole, IdealPosition.Y) < 0)
        {
            // Overshoot snaps to the ideal word and discards the fractional camera position.
            Position = Position.WithY(new SnesFixedPosition(IdealPosition.Y, 0));
        }

        if ((short)Position.Y.Whole < 0)
        {
            SetWholeY(0);
            return;
        }

        int currentCell = VerticalCellIndex();
        if (Scrolls.ReadNativeState(currentCell) != RoomScrollState.RedBoundary)
            return;

        ushort boundary = NextScreenStart(Position.Y.Whole);
        ushort candidate = unchecked((ushort)(proposed + Speed.Y.WholeWord + 2));
        if (candidate >= boundary)
            candidate = boundary;
        SetWholeY(candidate);
    }

    private int VerticalCellIndex()
    {
        int xScreenAtCenter = SnesEightEight.WholeOf((ushort)(Position.X.Whole + 0x0080));
        int yScreen = SnesEightEight.WholeOf(Position.Y.Whole);
        return yScreen * Scrolls.Size.X + xScreenAtCenter;
    }

    private static int SignedDifference(ushort left, ushort right) => unchecked((short)(left - right));

    /// <summary>The first pixel of the 256-pixel scroll screen holding <paramref name="pixel"/> (the AND #$FF00 alignment).</summary>
    private static ushort ScreenStart(ushort pixel) => (ushort)(SnesLittleEndian.HighByteInPlace(pixel));

    /// <summary>The first pixel of the following scroll screen, wrapping at sixteen bits.</summary>
    private static ushort NextScreenStart(ushort pixel) => unchecked((ushort)(ScreenStart(pixel) + 0x0100));

    /// <summary>
    /// $90:96C3/$90:9702: compares only the integer words as signed 16-bit values, then
    /// subtracts the complete 16.16 pair in the selected direction and adds exactly
    /// $0001:0000. Modular arithmetic keeps the routine's noted subpixel corner case, where
    /// the result can still be less than 1.0.
    /// </summary>
    private static SnesSignedSixteenSixteen CalculateDistanceMovedPlusOne(
        SnesFixedPosition previous,
        SnesFixedPosition current)
    {
        uint distance = SignedDifference(current.Whole, previous.Whole) >= 0
            ? unchecked(current.RawValue - previous.RawValue + 0x0001_0000u)
            : unchecked(previous.RawValue - current.RawValue + 0x0001_0000u);
        return SnesSignedSixteenSixteen.FromRaw(unchecked((int)distance));
    }

    // The scroll handlers are per-axis ports: each writes one integer layer-one word, leaving
    // the other axis and (unless stated) both fraction words untouched.
    private void SetWholeX(ushort x) => Position = Position.WithX(Position.X.WithWhole(x));

    private void SetWholeY(ushort y) => Position = Position.WithY(Position.Y.WithWhole(y));
}

/// <summary>State branches used by horizontal camera target selection at <c>$90:95B7</c>.</summary>
public readonly record struct HorizontalCameraContext(
    ushort KnockbackDirection,
    SamusMovementType MovementType,
    ushort XAccelerationMode,
    byte PoseXDirection,
    CameraDistanceMode CameraDistanceIndex);

/// <summary>Scroller distances used by vertical target selection at <c>$90:9666</c>.</summary>
public readonly record struct VerticalCameraContext(
    SamusVerticalDirection YDirection,
    ushort UpScroller,
    ushort DownScroller);
