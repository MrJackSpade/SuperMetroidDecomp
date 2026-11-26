using Engine.Collision;
using Engine.Shared;
using Engine.World;

namespace Engine.Actors.Movement;

/// <summary>
/// Following the surface it is on, around corners: along floors, up walls, across ceilings and back down, always in
/// the same sense. Each update it presses toward its surface and, finding it there, moves along it. Meeting something
/// ahead it turns onto that; finding its surface gone it turns round the end of it. A crawler that keeps finding
/// nothing lets go and falls until it lands.
/// </summary>
public sealed class SurfaceCrawl
{
    /// <summary>The side of the mover the surface it clings to is on.</summary>
    public enum Clings
    {
        /// <summary>The surface is beneath it: a floor.</summary>
        Below,

        /// <summary>The surface is to its right: a wall.</summary>
        Right,

        /// <summary>The surface is above it: a ceiling.</summary>
        Above,

        /// <summary>The surface is to its left: a wall.</summary>
        Left,
    }

    /// <summary>How far it travels along its surface each update.</summary>
    public Pixels Speed { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 2);

    /// <summary>How many updates in a row it may find no surface before it lets go and falls.</summary>
    public int MissesBeforeFalling { get; set; } = 4;

    /// <summary>The downward speed a falling crawler gains each update.</summary>
    public Pixels FallGravity { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 2);

    /// <summary>The fastest a crawler falls.</summary>
    public Pixels FallSpeedLimit { get; set; } = Pixels.FromWhole(4);

    /// <summary>The speed along its surface a crawler takes on landing from a fall; null keeps the speed it had.</summary>
    public Pixels? SpeedOnLanding { get; set; }

    /// <summary>
    /// How far ahead of itself, along the way it crawls, it feels for its surface, in pixels: one that feels ahead
    /// finds the end of its surface, or the start of one in its way, that much sooner.
    /// </summary>
    public int Lookahead { get; set; }

    /// <summary>The way it presses across, as 1 for rightward or -1: toward the wall it clings to, or the way it crawls along a floor or ceiling.</summary>
    public int PressesAcross => _across;

    /// <summary>The way it presses down, as 1 for downward or -1: toward the floor or ceiling it clings to, or the way it crawls along a wall.</summary>
    public int PressesDown => _down;

    /// <summary>The way it presses across: toward the wall it clings to, or the way it crawls along a floor or ceiling.</summary>
    private int _across = 1;

    /// <summary>The way it presses down: toward the floor or ceiling it clings to, or the way it crawls along a wall.</summary>
    private int _down = 1;

    private int _misses;
    private Pixels _fallSpeed;
    private bool _onWallBeforeFall;

    /// <summary>The side the surface it clings to is on.</summary>
    public Clings Side { get; private set; } = Clings.Below;

    /// <summary>Whether it has let go and is falling.</summary>
    public bool IsFalling { get; private set; }

    /// <summary>
    /// Puts the crawler on a surface, going one way round: clockwise is rightward along a floor, down a wall on its
    /// left, leftward along a ceiling and up a wall on its right.
    /// </summary>
    public void Start(Clings side, bool clockwise)
    {
        Side = side;
        (_across, _down) = side switch
        {
            Clings.Below => (clockwise ? 1 : -1, 1),
            Clings.Above => (clockwise ? -1 : 1, -1),
            Clings.Left => (-1, clockwise ? 1 : -1),
            Clings.Right => (1, clockwise ? -1 : 1),
            _ => throw new InvalidOperationException($"Undefined side {side}."),
        };
        IsFalling = false;
        _misses = 0;
    }

    /// <summary>
    /// Puts the crawler on a surface pressing a given way across and down, whatever the surface. One that presses away
    /// from its surface finds nothing there and turns about, a face at a time, until it finds something to cling to.
    /// </summary>
    public void Start(Clings side, bool leftward, bool upward)
    {
        Side = side;
        _across = leftward ? -1 : 1;
        _down = upward ? -1 : 1;
        IsFalling = false;
        _misses = 0;
    }

    /// <summary>Makes the crawler let go of its surface and fall, as when the room shakes.</summary>
    public void LetGo()
    {
        if (IsFalling)
            return;
        _onWallBeforeFall = Side is Clings.Left or Clings.Right;
        IsFalling = true;
        _fallSpeed = Pixels.Zero;
    }

    /// <summary>
    /// How a crawler's moves are tried: only where each would end. Its leading edge alone is tried, so one that
    /// stands a little way into the terrain it clings to works free of it, and it rests against what stops it and
    /// never further back than it began. A move up or down meets sloped ground that holds things up by their middle
    /// under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.Plain with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsUnderSlopes = true };

    /// <summary>
    /// How a crawler's moves across are tried, feeling for a wall or going along a floor or ceiling: as its other
    /// moves, and sloped ground that holds things up by their middle is no part of them, neither stopping one nor
    /// putting the crawler anywhere. Such ground holds it afterwards, where it then is (<see cref="RestOnSlope"/>).
    /// So it crosses a slope on the picture of the floor or ceiling it is on, and turns only at a wall, a step or the
    /// end of its surface.
    /// </summary>
    private static readonly MoveOptions Across = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>
    /// Moves <paramref name="mover"/> for one update. With <paramref name="pursued"/> given, a crawler that is free to
    /// go either way along its surface goes toward that place.
    /// </summary>
    public MoveReport Step(IMoves mover, PixelPosition? pursued = null)
    {
        if (IsFalling)
            return StepFalling(mover);

        // With no speed it has no way to press but rightward and downward, whatever it was put on.
        if (Speed.IsZero)
            (_across, _down) = (1, 1);
        return Side is Clings.Left or Clings.Right ? StepOnWall(mover, pursued) : StepOnFloorOrCeiling(mover, pursued);
    }

    /// <summary>Crawls along a wall: presses across into it, then moves up or down along it.</summary>
    private MoveReport StepOnWall(IMoves mover, PixelPosition? pursued)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        // It feels for the wall from where it will be, and keeps however far that pressed it.
        var ahead = new PixelOffset(Pixels.Zero, Pixels.FromWhole(Lookahead * _down));
        mover.Position += ahead;
        bool found = room.MoveAcross(body, (Speed + Pixels.FromWhole(1)) * _across, Across).Blocked;
        mover.Position -= ahead;
        if (found)
        {
            _misses = 0;

            // Sloped ground holds it where it is, not where it felt from, before it moves along the wall.
            bool held = RestOnSlope(mover);
            if (room.MoveDown(body, Speed * _down, Moves).Blocked)
            {
                // An inside corner: what stopped it is its new surface, and it goes on along that away from the wall.
                _across = Turned(_across);
                Side = _down > 0 ? Clings.Below : Clings.Above;
                return new MoveReport(false, _down > 0, _down < 0, true, false) { MovedBySlope = held };
            }

            if (pursued is { } target && (target.Y.Whole >= mover.Position.Y.Whole) == (_down < 0))
                _down = Turned(_down);
            Show(mover);
            return MoveReport.Nothing with { MovedBySlope = held };
        }

        if (++_misses >= MissesBeforeFalling)
        {
            LetGo();
            return MoveReport.Nothing;
        }

        // An outside corner: the wall has ended, and it turns round the end onto the face beyond.
        _down = Turned(_down);
        Side = _down > 0 ? Clings.Below : Clings.Above;
        return new MoveReport(false, false, false, true, false);
    }

    /// <summary>Crawls along a floor or ceiling: presses down or up into it, then moves across along it.</summary>
    private MoveReport StepOnFloorOrCeiling(IMoves mover, PixelPosition? pursued)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        var ahead = new PixelOffset(Pixels.FromWhole(Lookahead * _across), Pixels.Zero);
        mover.Position += ahead;
        bool found = room.MoveDown(body, (Speed + Pixels.FromWhole(1)) * _down, Moves).Blocked;
        mover.Position -= ahead;
        if (found)
        {
            _misses = 0;
            if (room.MoveAcross(body, Along(mover), Across).Blocked)
            {
                // An inside corner: it turns onto what stopped it from where the stop left it, and no slope holds it.
                _down = Turned(_down);
                Side = _across > 0 ? Clings.Right : Clings.Left;
                return new MoveReport(true, false, false, true, false);
            }

            // Having gone along, sloped ground holds it where it now is.
            bool held = RestOnSlope(mover);
            if (pursued is { } target && (target.X.Whole >= mover.Position.X.Whole) == (_across < 0))
                _across = Turned(_across);
            Show(mover);
            return MoveReport.Nothing with { MovedBySlope = held };
        }

        if (++_misses >= MissesBeforeFalling)
        {
            LetGo();
            return MoveReport.Nothing;
        }

        _across = Turned(_across);
        Side = _across > 0 ? Clings.Right : Clings.Left;
        return new MoveReport(false, false, false, true, false);
    }

    /// <summary>
    /// Puts the crawler where sloped ground that holds things up by their middle has it under its middle, where it is
    /// now: up out of a floor's slope its feet are in, then down from a ceiling's its head is in. Returns whether that
    /// moved it; lying exactly against such ground it is held and not moved.
    /// </summary>
    private static bool RestOnSlope(IMoves mover)
    {
        Pixels before = mover.Position.Y;

        // A move across of no distance meets nothing, and ends as every move across that climbs such ground does.
        mover.Room.Mover.MoveAcross(mover.MovingBody, Pixels.Zero, Moves);
        return mover.Position.Y != before;
    }

    /// <summary>
    /// How far it goes across this update along a floor or ceiling: by its speed, of which sloped ground that holds
    /// things by their middle lets only its share through. The ground asked is what lies at the middle of the face it
    /// presses with, where it is after pressing: the bottom row of its body on a floor, the top row on a ceiling.
    /// </summary>
    private Pixels Along(IMoves mover)
    {
        PixelRect here = mover.MovingBody.RoomRect;
        int share = mover.Room.Terrain.SlopeShareAt(here.Left + here.Width / 2, _down < 0 ? here.Top : here.Bottom - 1);
        return Speed.Share(share, SlopeShape.WholeShare) * _across;
    }

    /// <summary>
    /// A way of pressing turned about. One with no speed cannot be turned: its speed either way is nothing, which
    /// presses rightward and downward, so on a ceiling or against a wall on its left it finds nothing and lets go.
    /// </summary>
    private int Turned(int way) => Speed.IsZero ? 1 : -way;

    /// <summary>Falls until something stops it, and then crawls on from where it landed.</summary>
    private MoveReport StepFalling(IMoves mover)
    {
        MoveOptions fall = Moves;
        if (!mover.Room.Mover.MoveDown(mover.MovingBody, _fallSpeed, fall).Blocked)
        {
            if (_fallSpeed < FallSpeedLimit)
                _fallSpeed += FallGravity;
            return MoveReport.Nothing;
        }

        // One given its speed as it lands goes rightward and downward with it, whatever way it pressed before.
        if (SpeedOnLanding is { } speed)
            (Speed, _across, _down) = (speed, 1, 1);
        _fallSpeed = Pixels.Zero;
        _misses = 0;
        IsFalling = false;
        // It goes on as it was before it let go: a crawler that fell off a wall presses toward that wall again.
        Side = _onWallBeforeFall ? (_across > 0 ? Clings.Right : Clings.Left) : (_down > 0 ? Clings.Below : Clings.Above);
        return new MoveReport(false, true, false, false, true);
    }

    /// <summary>Gives the mover the facing and speed its crawl has, for whoever shows it.</summary>
    private void Show(IMoves mover)
    {
        mover.Velocity = new PixelVelocity(Speed * _across, Speed * _down);
        mover.Facing = _across < 0 ? Facing.Left : Facing.Right;
    }
}
