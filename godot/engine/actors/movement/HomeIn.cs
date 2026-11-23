using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Steering toward a target: each update the mover's speed along each axis is turned toward where the target lies, by
/// a limited amount, up to a top speed.
/// </summary>
public sealed class HomeIn
{
    /// <summary>The speed gained toward the target each update, along each axis.</summary>
    public Pixels Acceleration { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 16);

    /// <summary>The top speed along each axis.</summary>
    public Pixels TopSpeed { get; set; } = Pixels.FromWhole(2);

    /// <summary>Whether terrain stops it; one that is not stopped passes through walls.</summary>
    public bool StoppedByTerrain { get; set; } = true;

    /// <summary>Moves <paramref name="mover"/> for one update, with its target at <paramref name="target"/>.</summary>
    public MoveReport Step(IMoves mover, PixelPosition target)
    {
        PixelOffset toTarget = target - mover.Position;
        mover.Velocity = new PixelVelocity(Turn(mover.Velocity.X, toTarget.X), Turn(mover.Velocity.Y, toTarget.Y));
        if (!mover.Velocity.X.IsZero)
            mover.Facing = mover.Velocity.X.IsNegative ? Facing.Left : Facing.Right;

        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        MoveOptions air = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall, StoppedByTerrain = StoppedByTerrain, StoppedBySolids = StoppedByTerrain };
        bool hitWall = room.MoveAcross(body, mover.Velocity.X, air).Blocked;
        bool rising = mover.Velocity.Y.IsNegative;
        bool stopped = room.MoveDown(body, mover.Velocity.Y, air).Blocked;
        if (hitWall)
            mover.Velocity = mover.Velocity.WithX(Pixels.Zero);
        if (stopped)
            mover.Velocity = mover.Velocity.WithY(Pixels.Zero);
        return new MoveReport(hitWall, stopped && !rising, stopped && rising, false, false);
    }

    /// <summary>A speed along one axis, turned toward where the target lies along it.</summary>
    private Pixels Turn(Pixels speed, Pixels toTarget)
    {
        if (toTarget.IsZero)
            return speed;
        Pixels turned = toTarget.IsNegative ? speed - Acceleration : speed + Acceleration;
        return turned > TopSpeed ? TopSpeed : turned < -TopSpeed ? -TopSpeed : turned;
    }
}
