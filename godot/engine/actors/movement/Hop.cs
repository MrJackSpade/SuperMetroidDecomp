using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Jumping in arcs away from a surface and back to it: off a floor and down again, or off a ceiling and up again. The
/// mover leaps the way it faces with a set speed across, slows as it goes, turns back at the top of the hop or on
/// meeting something, and speeds up until it lands. How fast it rises and falls comes from gravity, or, when a speed
/// curve is given, from the curve.
/// </summary>
public sealed class Hop
{
    /// <summary>How far it travels across each update of a hop.</summary>
    public Pixels AcrossSpeed { get; set; } = Pixels.FromWhole(1);

    /// <summary>The speed away from its surface a hop starts with, when no curve is given.</summary>
    public Pixels UpSpeed { get; set; } = Pixels.FromWhole(3);

    /// <summary>The speed toward its surface gained each update, when no curve is given.</summary>
    public Pixels Gravity { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 8);

    /// <summary>The curve the hop's speeds away from and back to its surface come from; null for a hop under gravity.</summary>
    public SpeedCurve? Curve { get; set; }

    /// <summary>With a curve: how far along it the hop moves each update, in 256ths of a mark.</summary>
    public int CurveStep { get; set; } = SpeedCurve.PartsPerMark;

    /// <summary>With a curve: the furthest place along it the fall back reaches, in 256ths of a mark.</summary>
    public int FastestPlace { get; set; } = 64 * SpeedCurve.PartsPerMark;

    /// <summary>Whether the surface it hops from is above it: a ceiling.</summary>
    public bool FromCeiling { get; set; }

    /// <summary>How many updates it waits on its surface between hops.</summary>
    public int WaitUpdates { get; set; } = 30;

    /// <summary>The updates left before the next hop.</summary>
    private Countdown _wait = Countdown.Over;

    private int _place;
    private bool _returning;

    /// <summary>Whether it is away from its surface.</summary>
    public bool IsInAir { get; private set; }

    /// <summary>Whether it is on its way back to its surface.</summary>
    public bool IsReturning => IsInAir && _returning;

    /// <summary>
    /// Starts a hop now, the way the mover faces. With a curve, <paramref name="height"/> is how far from its surface
    /// the hop carries it.
    /// </summary>
    public void Launch(IMoves mover, Pixels height)
    {
        IsInAir = true;
        _returning = false;
        if (Curve is not null)
            _place = Curve.PlaceForClimb(height, CurveStep);
        else
            mover.Velocity = new PixelVelocity(mover.Facing.Toward(AcrossSpeed), FromCeiling ? UpSpeed : -UpSpeed);
    }

    /// <summary>Moves <paramref name="mover"/> for one update: waiting on its surface, or hopping.</summary>
    public MoveReport Step(IMoves mover)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        if (!IsInAir)
        {
            _wait = _wait.Stepped();
            if (_wait.IsRunning)
                return MoveReport.Nothing;
            Launch(mover, UpSpeed);
        }

        return Curve is null ? StepUnderGravity(mover, room, body) : StepAlongCurve(mover, room, body, Curve);
    }

    /// <summary>Moves one update of a hop whose speeds come from a curve.</summary>
    private MoveReport StepAlongCurve(IMoves mover, Mover room, Collider body, SpeedCurve curve)
    {
        // Each move is tried where it ends, by the edge it leads with: a hopper the room sets a little way into
        // the surface it hangs from or stands on moves away from it freely, and lands flush against it.
        MoveOptions air = MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.Wall };
        int away = FromCeiling ? 1 : -1;
        Pixels speed = curve.SpeedAt(_place);
        if (!_returning)
        {
            if (room.MoveDown(body, speed * away, air).Blocked)
            {
                // Meeting something on the way out turns the hop back, and it does not move across in that update.
                mover.Facing = mover.Facing.Opposite();
                _returning = true;
                return new MoveReport(false, FromCeiling, !FromCeiling, false, false);
            }

            if (room.MoveAcross(body, mover.Facing.Toward(AcrossSpeed), air).Blocked)
            {
                mover.Facing = mover.Facing.Opposite();
                _returning = true;
                return new MoveReport(true, false, false, false, false);
            }

            _place -= CurveStep;
            if (_place < 0)
            {
                _place = 0;
                _returning = true;
            }

            return MoveReport.Nothing;
        }

        if (room.MoveDown(body, speed * -away, air).Blocked)
        {
            IsInAir = false;
            _wait = Countdown.Of(WaitUpdates);
            return new MoveReport(false, !FromCeiling, FromCeiling, false, true);
        }

        bool hitWall = room.MoveAcross(body, mover.Facing.Toward(AcrossSpeed), air).Blocked;
        if (hitWall)
            mover.Facing = mover.Facing.Opposite();
        _place = Math.Min(_place + CurveStep, FastestPlace);
        return new MoveReport(hitWall, false, false, false, false);
    }

    /// <summary>Moves one update of a hop under gravity.</summary>
    private MoveReport StepUnderGravity(IMoves mover, Mover room, Collider body)
    {
        // Tried where each move ends, as a hop along a curve is.
        MoveOptions air = MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.Wall };
        bool hitWall = room.MoveAcross(body, mover.Velocity.X, air).Blocked;
        if (hitWall)
            mover.Velocity = mover.Velocity.WithX(Pixels.Zero);
        bool towardSurface = FromCeiling ? mover.Velocity.Y.IsNegative : !mover.Velocity.Y.IsNegative;
        bool stopped = room.MoveDown(body, mover.Velocity.Y, air).Blocked;
        mover.Velocity = mover.Velocity.WithY(mover.Velocity.Y + (FromCeiling ? -Gravity : Gravity));
        _returning = towardSurface;
        if (!stopped)
            return new MoveReport(hitWall, false, false, false, false);
        if (!towardSurface)
        {
            mover.Velocity = mover.Velocity.WithY(Pixels.Zero);
            return new MoveReport(hitWall, FromCeiling, !FromCeiling, false, false);
        }

        IsInAir = false;
        mover.Velocity = PixelVelocity.Zero;
        _wait = Countdown.Of(WaitUpdates);
        return new MoveReport(hitWall, !FromCeiling, FromCeiling, false, true);
    }
}
