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
    private readonly RoomScrollGrid _scrolls;

    public ScrollBoundaryCamera(RoomScrollGrid scrolls)
    {
        _scrolls = scrolls ?? throw new ArgumentNullException(nameof(scrolls));
    }

    public ushort XPosition { get; private set; }
    public ushort XSubposition { get; private set; }
    public ushort YPosition { get; private set; }
    public ushort YSubposition { get; private set; }
    public ushort IdealXPosition { get; private set; }
    public ushort IdealYPosition { get; private set; }
    public ushort CameraXSpeed { get; private set; }
    public ushort CameraXSubspeed { get; private set; }
    public ushort CameraYSpeed { get; private set; }
    public ushort CameraYSubspeed { get; private set; }

    /// <summary>The mutable 50-byte scroll table consulted by this camera.</summary>
    public RoomScrollGrid Scrolls => _scrolls;

    /// <summary>
    /// Sets an entry/door camera position. This does only physical room-edge clamping;
    /// directional internal boundaries are evaluated when movement is attempted.
    /// </summary>
    public void SetPosition(int x, int y)
    {
        int maxX = (_scrolls.WidthInScreens - 1) << 8;

        // The vertical maximum depends on blue/green scroll alignment. Starting with the
        // room-wide conservative +$1F allowance matches $80:A731's largest legal result;
        // the first vertical movement applies the cell-specific exact maximum.
        int maxY = ((_scrolls.HeightInScreens - 1) << 8) + 0x1f;
        XPosition = (ushort)Math.Clamp(x, 0, maxX);
        YPosition = (ushort)Math.Clamp(y, 0, maxY);
        XSubposition = 0;
        YSubposition = 0;
        IdealXPosition = XPosition;
        IdealYPosition = YPosition;
    }

    /// <summary>Applies a debug rightward stimulus, then runs <c>$80:A641</c>.</summary>
    public void MoveRight(ushort pixelDistance)
    {
        RequireDistance(pixelDistance);
        CameraXSpeed = pixelDistance;
        IdealXPosition = unchecked((ushort)(XPosition + pixelDistance));
        XPosition = IdealXPosition;
        HandleScrollingRight();
    }

    /// <summary>Applies a debug leftward stimulus, then runs <c>$80:A6BB</c>.</summary>
    public void MoveLeft(ushort pixelDistance)
    {
        RequireDistance(pixelDistance);
        CameraXSpeed = pixelDistance;
        IdealXPosition = unchecked((ushort)(XPosition - pixelDistance));
        XPosition = IdealXPosition;
        HandleScrollingLeft();
    }

    /// <summary>Applies a debug downward stimulus, then runs <c>$80:A893</c>.</summary>
    public void MoveDown(ushort pixelDistance)
    {
        RequireDistance(pixelDistance);
        CameraYSpeed = pixelDistance;
        IdealYPosition = unchecked((ushort)(YPosition + pixelDistance));
        YPosition = IdealYPosition;
        HandleScrollingDown();
    }

    /// <summary>Applies a debug upward stimulus, then runs <c>$80:A936</c>.</summary>
    public void MoveUp(ushort pixelDistance)
    {
        RequireDistance(pixelDistance);
        CameraYSpeed = pixelDistance;
        IdealYPosition = unchecked((ushort)(YPosition - pixelDistance));
        YPosition = IdealYPosition;
        HandleScrollingUp();
    }

    /// <summary>
    /// Ports horizontal Samus tracking at <c>$90:95A0</c> plus camera-speed calculation
    /// <c>$90:96C0</c>. Integer movement follows the facing-dependent camera target;
    /// an unchanged integer position dispatches to <c>$80:A528</c> autoscrolling exactly
    /// as the original routine does (subpixel-only Samus movement still affects speed).
    /// </summary>
    public void TrackMovedSamusHorizontally(
        SamusCameraPoint previous,
        SamusCameraPoint current,
        HorizontalCameraContext context,
        bool timeIsFrozen = false)
    {
        if (context.CameraDistanceIndex is not (0 or 2 or 4 or 6))
            throw new ArgumentOutOfRangeException(nameof(context), "Camera distance index must be byte offset 0, 2, 4, or 6.");

        (CameraXSpeed, CameraXSubspeed) = CalculateDistanceMovedPlusOne(
            previous.XPosition,
            previous.XSubposition,
            current.XPosition,
            current.XSubposition);

        // $90:95A8 compares the integer words only. This is intentionally after speed
        // calculation: even when those words match, differing fractional samples can make
        // the integer CameraXSpeed used by $80:A528 zero, one, or (on wrap) much larger.
        if (previous.XPosition == current.XPosition)
        {
            HandleHorizontalAutoscrolling(timeIsFrozen);
            return;
        }

        ReadOnlySpan<ushort> facingRightOffsets = [0x0060, 0x0040, 0x0020, 0x00e0];
        ReadOnlySpan<ushort> facingLeftOffsets = [0x00a0, 0x0050, 0x0020, 0x00e0];

        // This XOR is a compact but exact projection of $90:95B7-$90:95E5. Knockback,
        // moonwalk movement type $10, or acceleration mode one reverses which facing table
        // applies; pose X direction four is the ROM's right-facing value.
        bool backwards = context.KnockbackDirection != 0
            || context.MovementType == 0x10
            || context.XAccelerationMode == 1;
        bool useFacingRightTable = backwards ^ (context.PoseXDirection != 4);
        int distanceSlot = context.CameraDistanceIndex >> 1;
        ushort targetOffset = useFacingRightTable
            ? facingRightOffsets[distanceSlot]
            : facingLeftOffsets[distanceSlot];
        IdealXPosition = unchecked((ushort)(current.XPosition - targetOffset));

        if (IdealXPosition == XPosition)
            return;

        if (SignedDifference(IdealXPosition, XPosition) < 0)
        {
            SubtractFromX(CameraXSpeed, CameraXSubspeed);
            HandleScrollingLeft();
        }
        else
        {
            AddToX(CameraXSpeed, CameraXSubspeed);
            HandleScrollingRight();
        }
    }

    /// <summary>
    /// Ports vertical Samus tracking at <c>$90:964F</c> plus distance calculation
    /// <c>$90:96FF</c>. An unchanged integer Y position runs <c>$80:A731</c>.
    /// </summary>
    public void TrackMovedSamusVertically(
        SamusCameraPoint previous,
        SamusCameraPoint current,
        VerticalCameraContext context,
        bool timeIsFrozen = false)
    {
        (CameraYSpeed, CameraYSubspeed) = CalculateDistanceMovedPlusOne(
            previous.YPosition,
            previous.YSubposition,
            current.YPosition,
            current.YSubposition);

        if (previous.YPosition == current.YPosition)
        {
            HandleVerticalAutoscrolling(timeIsFrozen);
            return;
        }

        // Samus Y direction one means upward. The apparently reversed scroller choice is
        // literal $90:9666-$90:9681: upward motion subtracts down_scroller, all else
        // subtracts up_scroller.
        ushort targetOffset = context.YDirection == 1 ? context.DownScroller : context.UpScroller;
        IdealYPosition = unchecked((ushort)(current.YPosition - targetOffset));

        if (IdealYPosition == YPosition)
            return;

        if (SignedDifference(IdealYPosition, YPosition) < 0)
        {
            SubtractFromY(CameraYSpeed, CameraYSubspeed);
            HandleScrollingUp();
        }
        else
        {
            AddToY(CameraYSpeed, CameraYSubspeed);
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
        ushort proposed = XPosition;

        if ((short)XPosition < 0)
            XPosition = 0;

        ushort roomMaximum = (ushort)((_scrolls.WidthInScreens - 1) << 8);
        if (XPosition > roomMaximum)
            XPosition = roomMaximum;

        int row = ((ushort)(YPosition + 0x0080) >> 8) * _scrolls.WidthInScreens;
        int currentCell = row + (XPosition >> 8);

        if (_scrolls.ReadNativeStorage(currentCell) == 0)
        {
            // In a red current cell, drift right by camera speed + 2 toward its right edge.
            // If the next cell is also red, discard the fractional screen position and stay
            // on this cell's left edge instead of allowing the viewport to enter it.
            ushort rightBoundary = unchecked((ushort)((XPosition & 0xff00) + 0x0100));
            ushort candidate = unchecked((ushort)(proposed + CameraXSpeed + 2));
            if (candidate >= rightBoundary)
            {
                XPosition = rightBoundary;
                return;
            }

            proposed = candidate;
            int candidateRightCell = row + (proposed >> 8) + 1;
            XPosition = _scrolls.ReadNativeStorage(candidateRightCell) != 0
                ? proposed
                : (ushort)(proposed & 0xff00);
            return;
        }

        if (_scrolls.ReadNativeStorage(currentCell + 1) != 0)
            return;

        // A legal current cell with a red cell to its right is the mirror case: drift left
        // toward the current cell's left edge. If the candidate itself lands in red, round
        // up to the next screen boundary. Signed comparison reproduces BMI at $80:A5F9.
        ushort leftBoundary = (ushort)(XPosition & 0xff00);
        ushort leftCandidate = unchecked((ushort)(proposed - CameraXSpeed - 2));
        if (SignedDifference(leftCandidate, leftBoundary) < 0)
        {
            XPosition = leftBoundary;
            return;
        }

        proposed = leftCandidate;
        int candidateCell = row + (proposed >> 8);
        XPosition = _scrolls.ReadNativeStorage(candidateCell) != 0
            ? proposed
            : unchecked((ushort)((proposed & 0xff00) + 0x0100));
    }

    /// <summary>
    /// Direct port of vertical scroll-zone autoscrolling at <c>$80:A731-$80:A892</c>.
    /// </summary>
    private void HandleVerticalAutoscrolling(bool timeIsFrozen)
    {
        if (timeIsFrozen)
            return;

        int centeredXScreen = (ushort)(XPosition + 0x0080) >> 8;
        int preClampCell = (YPosition >> 8) * _scrolls.WidthInScreens + centeredXScreen;

        // Blue scroll cells align the 224-line gameplay viewport to the top of the screen;
        // red and green cells permit the ROM's $1F-pixel lower alignment. Crucially, the
        // alignment is selected before Y is clamped and remains fixed for this invocation.
        ushort verticalAlignment = _scrolls.ReadNativeStorage(preClampCell) == 1
            ? (ushort)0
            : (ushort)0x001f;
        ushort proposed = YPosition;

        if ((short)YPosition < 0)
            YPosition = 0;

        ushort roomMaximum = unchecked((ushort)(((_scrolls.HeightInScreens - 1) << 8) + verticalAlignment));
        if (YPosition > roomMaximum)
            YPosition = roomMaximum;

        int currentCell = (YPosition >> 8) * _scrolls.WidthInScreens + centeredXScreen;
        if (_scrolls.ReadNativeStorage(currentCell) == 0)
        {
            // A red current cell pushes downward toward its bottom boundary. As in the X
            // routine, a second red cell beyond the candidate causes screen-edge rounding.
            ushort bottomBoundary = unchecked((ushort)((YPosition & 0xff00) + 0x0100));
            ushort candidate = unchecked((ushort)(proposed + CameraYSpeed + 2));
            if (candidate >= bottomBoundary)
            {
                YPosition = bottomBoundary;
                return;
            }

            proposed = candidate;
            int belowCandidate = ((candidate >> 8) + 1) * _scrolls.WidthInScreens + centeredXScreen;
            YPosition = _scrolls.ReadNativeStorage(belowCandidate) != 0
                ? proposed
                : (ushort)(proposed & 0xff00);
            return;
        }

        if (_scrolls.ReadNativeStorage(currentCell + _scrolls.WidthInScreens) != 0)
            return;

        ushort topBoundary = unchecked((ushort)((YPosition & 0xff00) + verticalAlignment));

        // CMP/BCC at $80:A82D only pulls the camera upward when it is strictly below the
        // aligned boundary. Equality intentionally does nothing.
        if (topBoundary >= YPosition)
            return;

        ushort upwardCandidate = unchecked((ushort)(proposed - CameraYSpeed - 2));
        if (SignedDifference(upwardCandidate, topBoundary) < 0)
        {
            YPosition = topBoundary;
            return;
        }

        proposed = upwardCandidate;
        int candidateCell = (upwardCandidate >> 8) * _scrolls.WidthInScreens + centeredXScreen;
        YPosition = _scrolls.ReadNativeStorage(candidateCell) != 0
            ? proposed
            : unchecked((ushort)((proposed & 0xff00) + 0x0100));
    }

    private void HandleScrollingRight()
    {
        ushort proposed = XPosition;

        // CMP ideal,current / BPL in $80:A651: if movement overshot the ideal in signed
        // 16-bit space, snap to it and discard the fractional camera position.
        if (SignedDifference(IdealXPosition, XPosition) < 0)
        {
            XPosition = IdealXPosition;
            XSubposition = 0;
        }

        ushort roomMaximum = (ushort)((_scrolls.WidthInScreens - 1) << 8);
        if (XPosition > roomMaximum)
        {
            XPosition = roomMaximum;
            return;
        }

        // The horizontal routines sample the row containing the screen's vertical center
        // (Y+$80), then the screen immediately to the right of the camera's high X byte.
        int row = ((ushort)(YPosition + 0x0080) >> 8) * _scrolls.WidthInScreens;
        int rightCell = row + (XPosition >> 8) + 1;
        if (_scrolls.ReadNativeStorage(rightCell) != 0)
            return;

        ushort boundary = (ushort)(XPosition & 0xff00);
        ushort candidate = unchecked((ushort)(proposed - CameraXSpeed - 2));
        if (SignedDifference(candidate, boundary) < 0)
            candidate = boundary;
        XPosition = candidate;
    }

    private void HandleScrollingLeft()
    {
        ushort proposed = XPosition;
        if (SignedDifference(XPosition, IdealXPosition) < 0)
        {
            XPosition = IdealXPosition;
            XSubposition = 0;
        }

        if ((short)XPosition < 0)
        {
            XPosition = 0;
            return;
        }

        int row = ((ushort)(YPosition + 0x0080) >> 8) * _scrolls.WidthInScreens;
        int currentCell = row + (XPosition >> 8);
        if (_scrolls.ReadNativeStorage(currentCell) != 0)
            return;

        ushort boundary = unchecked((ushort)((XPosition & 0xff00) + 0x0100));
        ushort candidate = unchecked((ushort)(proposed + CameraXSpeed + 2));
        if (candidate >= boundary)
            candidate = boundary;
        XPosition = candidate;
    }

    private void HandleScrollingDown()
    {
        ushort proposed = YPosition;
        int currentCell = VerticalCellIndex();

        // Blue scrolls align the 224-line playfield at +0; red/green scrolls use +$1F.
        // This is the LDY #0 / CMP #1 / LDY #$1F sequence at $80:A8A8-$80:A8C8.
        ushort verticalAlignment = _scrolls.ReadNativeStorage(currentCell) == 1 ? (ushort)0 : (ushort)0x001f;

        if (SignedDifference(IdealYPosition, YPosition) < 0)
        {
            YPosition = IdealYPosition;
            YSubposition = 0;
        }

        ushort boundary = unchecked((ushort)(((_scrolls.HeightInScreens - 1) << 8) + verticalAlignment));
        bool beyondRoomBottom = boundary < YPosition;
        bool blockedBelow = false;
        if (!beyondRoomBottom)
        {
            // Preserve the assembly branch order: if the physical room maximum was already
            // exceeded, $80:A8F2 branches before reading the cell below. This also prevents
            // a legitimate bottom-row/right-edge camera from indexing beyond $7E:CD51.
            blockedBelow = _scrolls.ReadNativeStorage(currentCell + _scrolls.WidthInScreens) == 0;
            if (blockedBelow)
            {
                boundary = unchecked((ushort)((YPosition & 0xff00) + verticalAlignment));
                blockedBelow = boundary < YPosition;
            }
        }

        if (beyondRoomBottom || blockedBelow)
        {
            ushort candidate = unchecked((ushort)(proposed - CameraYSpeed - 2));
            if (SignedDifference(candidate, boundary) < 0)
                candidate = boundary;
            YPosition = candidate;
        }
    }

    private void HandleScrollingUp()
    {
        ushort proposed = YPosition;
        if (SignedDifference(YPosition, IdealYPosition) < 0)
        {
            YPosition = IdealYPosition;
            YSubposition = 0;
        }

        if ((short)YPosition < 0)
        {
            YPosition = 0;
            return;
        }

        int currentCell = VerticalCellIndex();
        if (_scrolls.ReadNativeStorage(currentCell) != 0)
            return;

        ushort boundary = unchecked((ushort)((YPosition & 0xff00) + 0x0100));
        ushort candidate = unchecked((ushort)(proposed + CameraYSpeed + 2));
        if (candidate >= boundary)
            candidate = boundary;
        YPosition = candidate;
    }

    private int VerticalCellIndex()
    {
        int xScreenAtCenter = (ushort)(XPosition + 0x0080) >> 8;
        int yScreen = YPosition >> 8;
        return yScreen * _scrolls.WidthInScreens + xScreenAtCenter;
    }

    private static int SignedDifference(ushort left, ushort right) => unchecked((short)(left - right));

    private static (ushort Speed, ushort Subspeed) CalculateDistanceMovedPlusOne(
        ushort previousPosition,
        ushort previousSubposition,
        ushort currentPosition,
        ushort currentSubposition)
    {
        uint previousFixed = ((uint)previousPosition << 16) | previousSubposition;
        uint currentFixed = ((uint)currentPosition << 16) | currentSubposition;

        // $90:96C3 compares only integer positions as signed 16-bit values. It then
        // subtracts the complete 16.16 pair in the selected direction and adds exactly
        // $0001:0000. Preserve modular uint arithmetic for the routine's noted subpixel
        // corner case where its result can still be less than 1.0.
        uint distance = SignedDifference(currentPosition, previousPosition) >= 0
            ? unchecked(currentFixed - previousFixed + 0x0001_0000u)
            : unchecked(previousFixed - currentFixed + 0x0001_0000u);
        return ((ushort)(distance >> 16), (ushort)distance);
    }

    private void AddToX(ushort speed, ushort subspeed)
    {
        uint fixedPosition = ((uint)XPosition << 16) | XSubposition;
        uint fixedSpeed = ((uint)speed << 16) | subspeed;
        fixedPosition = unchecked(fixedPosition + fixedSpeed);
        XPosition = (ushort)(fixedPosition >> 16);
        XSubposition = (ushort)fixedPosition;
    }

    private void SubtractFromX(ushort speed, ushort subspeed)
    {
        uint fixedPosition = ((uint)XPosition << 16) | XSubposition;
        uint fixedSpeed = ((uint)speed << 16) | subspeed;
        fixedPosition = unchecked(fixedPosition - fixedSpeed);
        XPosition = (ushort)(fixedPosition >> 16);
        XSubposition = (ushort)fixedPosition;
    }

    private void AddToY(ushort speed, ushort subspeed)
    {
        uint fixedPosition = ((uint)YPosition << 16) | YSubposition;
        uint fixedSpeed = ((uint)speed << 16) | subspeed;
        fixedPosition = unchecked(fixedPosition + fixedSpeed);
        YPosition = (ushort)(fixedPosition >> 16);
        YSubposition = (ushort)fixedPosition;
    }

    private void SubtractFromY(ushort speed, ushort subspeed)
    {
        uint fixedPosition = ((uint)YPosition << 16) | YSubposition;
        uint fixedSpeed = ((uint)speed << 16) | subspeed;
        fixedPosition = unchecked(fixedPosition - fixedSpeed);
        YPosition = (ushort)(fixedPosition >> 16);
        YSubposition = (ushort)fixedPosition;
    }

    private static void RequireDistance(ushort pixelDistance)
    {
        if (pixelDistance == 0 || pixelDistance >= 0x8000)
            throw new ArgumentOutOfRangeException(nameof(pixelDistance));
    }
}

/// <summary>One 16.16 Samus position sample consumed by the camera routines.</summary>
public readonly record struct SamusCameraPoint(
    ushort XPosition,
    ushort XSubposition,
    ushort YPosition,
    ushort YSubposition);

/// <summary>State branches used by horizontal camera target selection at <c>$90:95B7</c>.</summary>
public readonly record struct HorizontalCameraContext(
    ushort KnockbackDirection,
    byte MovementType,
    ushort XAccelerationMode,
    byte PoseXDirection,
    ushort CameraDistanceIndex);

/// <summary>Scroller distances used by vertical target selection at <c>$90:9666</c>.</summary>
public readonly record struct VerticalCameraContext(
    ushort YDirection,
    ushort UpScroller,
    ushort DownScroller);
