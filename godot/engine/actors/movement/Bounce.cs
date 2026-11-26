using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Travelling in a straight line and reflecting off what it meets: a wall reverses its speed across, a floor or ceiling
/// its speed up and down.
/// </summary>
public sealed class Bounce
{
    /// <summary>Whether gravity acts on it between bounces.</summary>
    public bool Falls { get; set; }

    /// <summary>The downward speed gained each update, for one that falls.</summary>
    public Pixels Gravity { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 8);

    /// <summary>Moves <paramref name="mover"/> for one update at its own speed.</summary>
    public MoveReport Step(IMoves mover)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        MoveOptions air = MoveOptions.AmongBlocks with { Slopes = SlopeTreatment.Wall };

        bool hitWall = room.MoveAcross(body, mover.Velocity.X, air).Blocked;
        if (hitWall)
        {
            mover.Velocity = mover.Velocity.WithX(-mover.Velocity.X);
            mover.Facing = mover.Facing.Opposite();
        }

        bool rising = mover.Velocity.Y.IsNegative;
        bool stopped = room.MoveDown(body, mover.Velocity.Y, air).Blocked;
        if (stopped)
            mover.Velocity = mover.Velocity.WithY(-mover.Velocity.Y);
        else if (Falls)
            mover.Velocity = mover.Velocity.WithY(mover.Velocity.Y + Gravity);
        return new MoveReport(hitWall, stopped && !rising, stopped && rising, false, false);
    }
}
