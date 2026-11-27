using Engine.Shared;

namespace Engine.World;

/// <summary>
/// The view onto a room. It follows the player by how far they moved, toward a place in the view the room keeps them
/// at, and is stopped by the edges of the room and by screens that are closed to it. It affects nothing in play by
/// itself; what is awake and what is seen follow where it stands. One per session.
/// </summary>
/// <remarks>
/// A room is a grid of screens, each <see cref="Screen"/> pixels each way, and each open to the view, closed to it, or
/// open with the view aligned to its top (<see cref="ScreenScroll"/>). The view never stands still inside a closed
/// screen: it drifts out of one, and back from one it would run into, by its speed and a little more each update.
/// The view's place has a fraction of a pixel, as the player's does; what is shown is its whole pixels.
/// </remarks>
public sealed class Camera
{
    /// <summary>The width and height of one screen of a room, in pixels.</summary>
    public const int Screen = 256;

    /// <summary>Half a screen: how far in from the view's corner the screen it checks across and down is taken.</summary>
    private const int HalfScreen = Screen / 2;

    private IScrollCells? _cells;

    /// <summary>What keeps how each screen lets the view in, once the camera has been given a room's.</summary>
    private IScrollCells Cells => _cells ?? throw new InvalidOperationException("The camera has been given no room's screens.");
    private int _columns = 1;
    private int _rows = 1;
    private PixelPosition? _tracked;
    private int? _trackedXWritten;
    private int? _trackedYWritten;

    /// <summary>How far down where the player is taken to have stood has been shifted since the view last tracked, where nothing was written outright.</summary>
    private int _shiftedY;

    /// <summary>A camera whose view is <paramref name="width"/> by <paramref name="height"/> pixels.</summary>
    public Camera(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
        LowerRest = Screen - height;
    }

    /// <summary>The view's width, in pixels.</summary>
    public int Width { get; }

    /// <summary>The view's height, in pixels.</summary>
    public int Height { get; }

    /// <summary>
    /// How far below a screen's top the view's top may rest, in a screen that does not align the view to its top. The
    /// plain answer is what brings the view's bottom to the screen's bottom.
    /// </summary>
    public int LowerRest { get; init; }

    /// <summary>How many pixels more than its speed the view drifts each update out of, or back from, a closed screen.</summary>
    public int ClosedScreenDrift { get; init; } = 2;

    /// <summary>Where the view's top-left corner stands in the room, to a fraction of a pixel.</summary>
    public PixelPosition Position { get; private set; }

    /// <summary>The view's left edge, in room pixels.</summary>
    public int Left => Position.X.Whole;

    /// <summary>The view's top edge, in room pixels.</summary>
    public int Top => Position.Y.Whole;

    /// <summary>The part of the room in view.</summary>
    public PixelRect View => new(Left, Top, Width, Height);

    /// <summary>Where the view's left edge is making for.</summary>
    public int IdealLeft { get; private set; }

    /// <summary>Where the view's top edge is making for.</summary>
    public int IdealTop { get; private set; }

    /// <summary>How far the view moves in an update across and down: how far the player last moved each way, and a pixel more.</summary>
    public PixelVelocity Speed { get; private set; }

    /// <summary>
    /// A player made anew is taken to have moved nothing across: the measure of the player's last move across is the
    /// one pixel more alone, until the view next follows a move. The measure down stays as it was.
    /// </summary>
    public void TakePlayerAsNew() => Speed = Speed.WithX(Pixels.FromWhole(1));

    /// <summary>Takes up a room: its screens, and what says how each lets the view in.</summary>
    public void EnterRoom(IScrollCells cells, int columns, int rows)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        (_cells, _columns, _rows) = (cells, columns, rows);
    }

    /// <summary>
    /// Puts the view at a place, kept inside the room, at rest there, with no fraction unless it is to keep its own. Whatever point of the player it
    /// was tracking is forgotten: the next tracking starts from where the player stood as that update began.
    /// </summary>
    /// <param name="left">Where its left edge is put, in the room's pixels.</param>
    /// <param name="top">Where its top edge is put.</param>
    /// <param name="keepsFractions">Whether it keeps the parts of a pixel it had, as a view put somewhere as play begins does.</param>
    public void Place(int left, int top, bool keepsFractions = false)
    {
        left = Math.Clamp(left, 0, (_columns - 1) * Screen);
        top = Math.Clamp(top, 0, (_rows - 1) * Screen + LowerRest);
        Position = keepsFractions
            ? new PixelPosition(Pixels.FromParts(left, Position.X.Fraction), Pixels.FromParts(top, Position.Y.Fraction))
            : PixelPosition.FromWhole(left, top);
        (IdealLeft, IdealTop) = (left, top);
        (_tracked, _trackedXWritten, _trackedYWritten, _shiftedY) = (null, null, null, 0);
    }

    /// <summary>Moves the view's whole pixels to a place, keeping its fractions, with no regard to the room: what a scripted move of the view does.</summary>
    public void MoveTo(int left, int top) =>
        Position = new PixelPosition(Position.X.WithWhole(left), Position.Y.WithWhole(top));

    /// <summary>Moves the view's left edge to a place, keeping its fraction.</summary>
    public void MoveLeftEdgeTo(int left) => SetLeft(left);

    /// <summary>
    /// Says where across the player is to be taken to have stood when the view last tracked them, so that a move made
    /// for them (a carry, a placement) is not followed as if they had made it.
    /// </summary>
    public void TakePlayerToHaveStoodAtX(int x) => _trackedXWritten = x;

    /// <summary>The same, for how far down the player is taken to have stood.</summary>
    public void TakePlayerToHaveStoodAtY(int y) => (_trackedYWritten, _shiftedY) = (y, 0);

    /// <summary>
    /// Moves where the player is taken to have stood down by <paramref name="by"/> pixels, up when negative, whatever
    /// it was: for a change that moves the player's place without their having moved, made before they move on that
    /// update, as a change of form that keeps the feet where they were moves the middle. The rest of the update's way
    /// is followed, where saying outright where they stood (<see cref="TakePlayerToHaveStoodAtY"/>) would hide it.
    /// </summary>
    public void ShiftWherePlayerStoodDown(int by)
    {
        if (_trackedYWritten is { } written)
            _trackedYWritten = written + by;
        else
            _shiftedY += by;
    }

    /// <summary>
    /// Where, in whole pixels, the view takes the player to have stood when it last tracked them, with whatever it
    /// has been told of that since; null when it has tracked nothing since it was placed.
    /// </summary>
    public (int X, int Y)? PlayerTakenToHaveStoodAt =>
        _tracked is { } tracked ? (_trackedXWritten ?? tracked.X.Whole, _trackedYWritten ?? tracked.Y.Whole + _shiftedY) : null;

    /// <summary>
    /// Caps how fast the view follows across a player now at <paramref name="now"/>: where they stand more than
    /// <paramref name="most"/> whole pixels from where the view takes them to have stood, they are taken to have stood
    /// exactly that far off, so that the view follows by that and its one pixel more. For something that moves the
    /// player faster than the view should follow. A view that has tracked nothing since it was placed is left as it is.
    /// </summary>
    public void CapFollowAcross(Pixels now, int most)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(most);
        if (_tracked is not { } tracked)
            return;
        int moved = now.Whole - (_trackedXWritten ?? tracked.X.Whole);
        if (Math.Abs(moved) > most)
            _trackedXWritten = now.Whole - Math.Sign(moved) * most;
    }

    /// <summary>
    /// The same, up and down; with <paramref name="risingOnly"/>, only a move upward is capped.
    /// </summary>
    public void CapFollowUpOrDown(Pixels now, int most, bool risingOnly = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(most);
        if (_tracked is not { } tracked)
            return;
        int moved = now.Whole - (_trackedYWritten ?? tracked.Y.Whole + _shiftedY);
        if (moved < -most || (!risingOnly && moved > most))
            (_trackedYWritten, _shiftedY) = (now.Whole - Math.Sign(moved) * most, 0);
    }

    /// <summary>
    /// Follows the player for one update, across and then down.
    /// </summary>
    /// <param name="atUpdateStart">Where the player stood as this update began: what the view tracks from when it has tracked nothing since it was placed.</param>
    /// <param name="now">Where the player stands.</param>
    /// <param name="tracking">How the room keeps the player in view.</param>
    /// <param name="frozen">Whether time is frozen, which stops the view's own drifting.</param>
    public void Track(PixelPosition atUpdateStart, PixelPosition now, ViewTracking tracking, bool frozen)
    {
        PixelPosition before = _tracked ?? atUpdateStart;
        if (_trackedXWritten is { } x)
            before = new PixelPosition(before.X.WithWhole(x), before.Y);
        if (_trackedYWritten is { } y)
            before = new PixelPosition(before.X, before.Y.WithWhole(y));
        else if (_shiftedY != 0)
            before = new PixelPosition(before.X, before.Y.WithWhole(before.Y.Whole + _shiftedY));
        (_trackedXWritten, _trackedYWritten, _shiftedY) = (null, null, 0);

        TrackAcross(before, now, tracking, frozen);
        TrackDown(before, now, tracking, frozen);
        _tracked = now;
    }

    /// <summary>
    /// Follows the player slowly, a few pixels an update toward keeping them in the middle part of the view, with the
    /// speeds of the last ordinary tracking left as they are: what a fast swing on a line asks for.
    /// </summary>
    public void TrackSlowly(PixelPosition now, int step, PixelRect middle, bool frozen)
    {
        // Where the player was taken to have stood is used up by an update's tracking of either kind: it does not
        // last into the ordinary tracking that follows a swing.
        (_trackedXWritten, _trackedYWritten, _shiftedY) = (null, null, 0);
        if (now.X.Whole >= 0)
        {
            SetLeft(TowardMiddle(Left, now.X.Whole, middle.Left, middle.Right, step));
            if (now.Y.Whole >= 0)
                SetTop(TowardMiddle(Top, now.Y.Whole, middle.Top, middle.Bottom, step));
        }

        DriftAcross(frozen);
        DriftDown(frozen);
        _tracked = now;
    }

    /// <summary>Keeps the view's top from standing above <paramref name="lowestTop"/>, once it has tracked the player.</summary>
    public void KeepAtOrBelow(int lowestTop)
    {
        if (lowestTop > Top)
            SetTop(lowestTop);
    }

    private static int TowardMiddle(int view, int player, int nearEdge, int farEdge, int step)
    {
        if (player < view)
            return view - step;
        int inView = player - view;
        if (inView >= farEdge)
            return view + step;
        return inView < nearEdge ? view - step : view;
    }

    private void TrackAcross(PixelPosition before, PixelPosition now, ViewTracking tracking, bool frozen)
    {
        Speed = Speed.WithX(MovedAndOneMore(before.X, now.X));

        // Only whole pixels are compared, and after the speed is taken: a player who moved by a fraction leaves the
        // view to its own drifting, at a speed that fraction gave.
        if (before.X.Whole == now.X.Whole)
        {
            DriftAcross(frozen);
            return;
        }

        IdealLeft = now.X.Whole - tracking.PlaceAcross;
        if (IdealLeft == Left)
            return;
        if (IdealLeft < Left)
        {
            Position = new PixelPosition(Position.X - Speed.X, Position.Y);
            StopGoingBack(across: true);
        }
        else
        {
            Position = new PixelPosition(Position.X + Speed.X, Position.Y);
            ScrollRight();
        }
    }

    private void TrackDown(PixelPosition before, PixelPosition now, ViewTracking tracking, bool frozen)
    {
        Speed = Speed.WithY(MovedAndOneMore(before.Y, now.Y));
        if (before.Y.Whole == now.Y.Whole)
        {
            DriftDown(frozen);
            return;
        }

        IdealTop = now.Y.Whole - (tracking.Rising ? tracking.AboveWhileRising : tracking.AboveOtherwise);
        if (IdealTop == Top)
            return;
        if (IdealTop < Top)
        {
            Position = new PixelPosition(Position.X, Position.Y - Speed.Y);
            StopGoingBack(across: false);
        }
        else
        {
            Position = new PixelPosition(Position.X, Position.Y + Speed.Y);
            ScrollDown();
        }
    }

    /// <summary>What the view does across on an update the player did not move a whole pixel across: it keeps inside the room and drifts clear of closed screens.</summary>
    private void DriftAcross(bool frozen)
    {
        if (frozen)
            return;

        // Every move below is counted from where the view stood before it was kept inside the room.
        int from = Left;
        if (Left < 0)
            SetLeft(0);
        int last = (_columns - 1) * Screen;
        if (Left > last)
            SetLeft(last);
        (int column, int row) = ScreenCheckedAcross();
        if (Cells.Read(column, row) == ScreenScroll.Closed)
        {
            DriftThroughClosed(across: true, from, column, row);
            return;
        }

        if (Cells.Read(column + 1, row) != ScreenScroll.Closed)
            return;
        DriftBackFromClosedBeyond(across: true, from, column, row, ScreenStart(Left));
    }

    /// <summary>The same, down.</summary>
    private void DriftDown(bool frozen)
    {
        if (frozen)
            return;

        // How far below a screen's top the view may rest is chosen before the view is kept inside the room, and
        // holds for this update.
        (int firstColumn, int firstRow) = ScreenCheckedDown();
        int rest = Cells.Read(firstColumn, firstRow) == ScreenScroll.OpenAlignedToTop ? 0 : LowerRest;
        int from = Top;
        if (Top < 0)
            SetTop(0);
        int last = (_rows - 1) * Screen + rest;
        if (Top > last)
            SetTop(last);
        (int column, int row) = ScreenCheckedDown();
        if (Cells.Read(column, row) == ScreenScroll.Closed)
        {
            DriftThroughClosed(across: false, from, column, row);
            return;
        }

        if (Cells.Read(column, row + 1) != ScreenScroll.Closed)
            return;
        int boundary = ScreenStart(Top) + rest;

        // Only a view strictly below the boundary is drawn back up to it.
        if (boundary >= Top)
            return;
        DriftBackFromClosedBeyond(across: false, from, column, row, boundary);
    }

    private void ScrollRight()
    {
        int from = Left;

        // A view that has passed where it was making for stands there, with no fraction.
        if (IdealLeft < Left)
            SnapToIdeal(across: true);
        int last = (_columns - 1) * Screen;
        if (Left > last)
        {
            SetLeft(last);
            return;
        }

        (int column, int row) = ScreenCheckedAcross();
        if (Cells.Read(column + 1, row) != ScreenScroll.Closed)
            return;
        DriftBack(across: true, from, ScreenStart(Left));
    }

    private void ScrollDown()
    {
        int from = Top;
        (int column, int row) = ScreenCheckedDown();
        int rest = Cells.Read(column, row) == ScreenScroll.OpenAlignedToTop ? 0 : LowerRest;
        if (IdealTop < Top)
            SnapToIdeal(across: false);
        int boundary = (_rows - 1) * Screen + rest;
        bool pastRoomBottom = boundary < Top;
        bool stoppedBelow = false;
        if (!pastRoomBottom)
        {
            // A view already past the room's bottom is not asked about the screen below.
            stoppedBelow = Cells.Read(column, row + 1) == ScreenScroll.Closed;
            if (stoppedBelow)
            {
                boundary = ScreenStart(Top) + rest;
                stoppedBelow = boundary < Top;
            }
        }

        if (pastRoomBottom || stoppedBelow)
            DriftBack(across: false, from, boundary);
    }

    /// <summary>What ends a move left or up: the place it was making for, the room's edge, or a closed screen it has entered.</summary>
    private void StopGoingBack(bool across)
    {
        int from = Whole(across);
        if (Whole(across) < Ideal(across))
            SnapToIdeal(across);
        if (Whole(across) < 0)
        {
            SetWhole(across, 0);
            return;
        }

        (int column, int row) = across ? ScreenCheckedAcross() : ScreenCheckedDown();
        if (Cells.Read(column, row) != ScreenScroll.Closed)
            return;
        int boundary = ScreenStart(Whole(across)) + Screen;
        int candidate = from + SpeedAlong(across) + ClosedScreenDrift;
        SetWhole(across, candidate >= boundary ? boundary : candidate);
    }

    private void DriftBack(bool across, int from, int boundary)
    {
        int candidate = from - SpeedAlong(across) - ClosedScreenDrift;
        SetWhole(across, candidate < boundary ? boundary : candidate);
    }

    private void DriftThroughClosed(bool across, int from, int column, int row)
    {
        int boundary = ScreenStart(Whole(across)) + Screen;
        int candidate = from + SpeedAlong(across) + ClosedScreenDrift;

        // A view that began above or left of the room and has not drifted back into it goes to the boundary: the
        // cartridge keeps the view's place as a number that cannot be below nothing, and one counted back from
        // nothing reads as far past any boundary.
        if (candidate < 0 || candidate >= boundary)
        {
            SetWhole(across, boundary);
            return;
        }

        // Should the screen past the one it lands in be closed too, it stays at that screen's near edge.
        (int landsColumn, int landsRow) = across ? (ScreenOf(candidate), row) : (column, ScreenOf(candidate));
        bool beyondOpen = Cells.Read(across ? landsColumn + 1 : landsColumn, across ? landsRow : landsRow + 1) != ScreenScroll.Closed;
        SetWhole(across, beyondOpen ? candidate : ScreenStart(candidate));
    }

    private void DriftBackFromClosedBeyond(bool across, int from, int column, int row, int boundary)
    {
        int candidate = from - SpeedAlong(across) - ClosedScreenDrift;
        if (candidate < boundary)
        {
            SetWhole(across, boundary);
            return;
        }

        (int landsColumn, int landsRow) = across ? (ScreenOf(candidate), row) : (column, ScreenOf(candidate));
        SetWhole(across, Cells.Read(landsColumn, landsRow) != ScreenScroll.Closed ? candidate : ScreenStart(candidate) + Screen);
    }

    /// <summary>The screen whose state decides the view's way across: the one its left edge is in, half a screen down from its top.</summary>
    private (int Column, int Row) ScreenCheckedAcross() => (ScreenOf(Left), ScreenOf(Top + HalfScreen));

    /// <summary>The screen whose state decides the view's way down: the one its top edge is in, half a screen in from its left.</summary>
    private (int Column, int Row) ScreenCheckedDown() => (ScreenOf(Left + HalfScreen), ScreenOf(Top));

    private static int ScreenOf(int pixel) => Math.DivRem(pixel, Screen, out int within) - (within < 0 ? 1 : 0);

    private static int ScreenStart(int pixel) => ScreenOf(pixel) * Screen;

    /// <summary>How far the player moved along one axis, whichever way, and one pixel more.</summary>
    private static Pixels MovedAndOneMore(Pixels before, Pixels now) =>
        (now.Whole >= before.Whole ? now - before : before - now) + Pixels.FromWhole(1);

    private int Whole(bool across) => across ? Left : Top;

    private int Ideal(bool across) => across ? IdealLeft : IdealTop;

    private int SpeedAlong(bool across) => across ? Speed.X.Whole : Speed.Y.Whole;

    private void SetWhole(bool across, int whole)
    {
        if (across)
            SetLeft(whole);
        else
            SetTop(whole);
    }

    private void SnapToIdeal(bool across) => Position = across
        ? new PixelPosition(Pixels.FromWhole(IdealLeft), Position.Y)
        : new PixelPosition(Position.X, Pixels.FromWhole(IdealTop));

    private void SetLeft(int left) => Position = new PixelPosition(Position.X.WithWhole(left), Position.Y);

    private void SetTop(int top) => Position = new PixelPosition(Position.X, Position.Y.WithWhole(top));
}
