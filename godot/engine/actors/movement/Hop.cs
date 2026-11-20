using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Jumping in arcs: the mover waits on the ground, then leaps the way it faces with a set speed across and up, falls
/// under gravity, lands, and waits again.
/// </summary>
public sealed class Hop
{
    /// <summary>How far it travels across each update of a hop.</summary>
    public Pixels AcrossSpeed { get; set; } = Pixels.FromWhole(1);

    /// <summary>The upward speed a hop starts with.</summary>
    public Pixels UpSpeed { get; set; } = Pixels.FromWhole(3);

    /// <summary>The downward speed gained each update in the air.</summary>
    public Pixels Gravity { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 8);

    /// <summary>How many updates it waits on the ground between hops.</summary>
    public int WaitUpdates { get; set; } = 30;

    /// <summary>The updates left before the next hop.</summary>
    private Countdown _wait = Countdown.Over;

    /// <summary>Whether it is in the air.</summary>
    public bool IsInAir { get; private set; }

    /// <summary>Moves <paramref name="mover"/> for one update.</summary>
    public MoveReport Step(IMoves mover)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        if (!IsInAir)
        {
            if (!room.IsOnGround(body, MoveOptions.Plain))
            {
                IsInAir = true;
                mover.Velocity = PixelVelocity.Zero;
            }
            else
            {
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return MoveReport.Nothing;
                IsInAir = true;
                mover.Velocity = new PixelVelocity(mover.Facing.Toward(AcrossSpeed), -UpSpeed);
            }
        }

        MoveOptions air = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        bool hitWall = room.MoveAcross(body, mover.Velocity.X, air).Blocked;
        if (hitWall)
            mover.Velocity = mover.Velocity.WithX(Pixels.Zero);
        bool rising = mover.Velocity.Y.IsNegative;
        bool stopped = room.MoveDown(body, mover.Velocity.Y, air).Blocked;
        mover.Velocity = mover.Velocity.WithY(mover.Velocity.Y + Gravity);
        if (!stopped)
            return new MoveReport(hitWall, false, false, false, false);
        if (rising)
        {
            mover.Velocity = mover.Velocity.WithY(Pixels.Zero);
            return new MoveReport(hitWall, false, true, false, false);
        }

        IsInAir = false;
        mover.Velocity = PixelVelocity.Zero;
        _wait = Countdown.Of(WaitUpdates);
        return new MoveReport(hitWall, true, false, false, true);
    }
}
