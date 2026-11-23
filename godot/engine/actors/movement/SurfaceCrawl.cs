using Engine.Collision;
using Engine.Shared;

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
    /// Moves <paramref name="mover"/> for one update. With <paramref name="pursued"/> given, a crawler that is free to
    /// go either way along its surface goes toward that place.
    /// </summary>
    public MoveReport Step(IMoves mover, PixelPosition? pursued = null)
    {
        if (IsFalling)
            return StepFalling(mover);
        return Side is Clings.Left or Clings.Right ? StepOnWall(mover, pursued) : StepOnFloorOrCeiling(mover, pursued);
    }

    /// <summary>Crawls along a wall: presses across into it, then moves up or down along it.</summary>
    private MoveReport StepOnWall(IMoves mover, PixelPosition? pursued)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        MoveOptions crawl = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        if (room.MoveAcross(body, (Speed + Pixels.FromWhole(1)) * _across, crawl).Blocked)
        {
            _misses = 0;
            if (room.MoveDown(body, Speed * _down, crawl).Blocked)
            {
                // An inside corner: what stopped it is its new surface, and it goes on along that away from the wall.
                _across = -_across;
                Side = _down > 0 ? Clings.Below : Clings.Above;
                return new MoveReport(false, _down > 0, _down < 0, true, false);
            }

            if (pursued is { } target && (target.Y.Whole >= mover.Position.Y.Whole) == (_down < 0))
                _down = -_down;
            Show(mover);
            return MoveReport.Nothing;
        }

        if (++_misses >= MissesBeforeFalling)
        {
            LetGo();
            return MoveReport.Nothing;
        }

        // An outside corner: the wall has ended, and it turns round the end onto the face beyond.
        _down = -_down;
        Side = _down > 0 ? Clings.Below : Clings.Above;
        return new MoveReport(false, false, false, true, false);
    }

    /// <summary>Crawls along a floor or ceiling: presses down or up into it, then moves across along it.</summary>
    private MoveReport StepOnFloorOrCeiling(IMoves mover, PixelPosition? pursued)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        MoveOptions press = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        // Along a floor it climbs and follows slopes as anything walking does.
        MoveOptions along = Side == Clings.Below ? MoveOptions.Plain with { ClimbPerPixel = 2, FollowGroundDown = 2 } : press;
        if (room.MoveDown(body, (Speed + Pixels.FromWhole(1)) * _down, press).Blocked)
        {
            _misses = 0;
            if (room.MoveAcross(body, Speed * _across, along).Blocked)
            {
                _down = -_down;
                Side = _across > 0 ? Clings.Right : Clings.Left;
                return new MoveReport(true, false, false, true, false);
            }

            if (pursued is { } target && (target.X.Whole >= mover.Position.X.Whole) == (_across < 0))
                _across = -_across;
            Show(mover);
            return MoveReport.Nothing;
        }

        if (++_misses >= MissesBeforeFalling)
        {
            LetGo();
            return MoveReport.Nothing;
        }

        _across = -_across;
        Side = _across > 0 ? Clings.Right : Clings.Left;
        return new MoveReport(false, false, false, true, false);
    }

    /// <summary>Falls until something stops it, and then crawls on from where it landed.</summary>
    private MoveReport StepFalling(IMoves mover)
    {
        MoveOptions fall = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        if (!mover.Room.Mover.MoveDown(mover.MovingBody, _fallSpeed, fall).Blocked)
        {
            if (_fallSpeed < FallSpeedLimit)
                _fallSpeed += FallGravity;
            return MoveReport.Nothing;
        }

        if (SpeedOnLanding is { } speed)
            Speed = speed;
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
