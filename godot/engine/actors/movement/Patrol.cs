using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Walking or flying back and forth: the mover travels the way it faces at a set speed and turns around at walls, and,
/// for one that walks, at the edges of the ground it is on.
/// </summary>
public sealed class Patrol
{
    /// <summary>How far it travels each update.</summary>
    public Pixels Speed { get; set; } = Pixels.FromWhole(1);

    /// <summary>Whether it walks: gravity holds it to the ground, and it turns at edges when <see cref="TurnsAtEdges"/> is set.</summary>
    public bool Walks { get; set; } = true;

    /// <summary>Whether a walker turns around at the edge of its ground; one that does not walks off.</summary>
    public bool TurnsAtEdges { get; set; } = true;

    /// <summary>The downward speed a walker gains each update while in the air.</summary>
    public Pixels Gravity { get; set; } = Pixels.FromParts(0, Pixels.FractionsPerPixel / 8);

    /// <summary>Moves <paramref name="mover"/> for one update.</summary>
    public MoveReport Step(IMoves mover)
    {
        Mover mover2 = mover.Room.Mover;
        Collider body = mover.MovingBody;
        bool hitFloor = false, reachedEdge = false;

        if (Walks)
        {
            if (mover2.IsOnGround(body, MoveOptions.AmongBlocks))
            {
                mover.Velocity = mover.Velocity.WithY(Pixels.Zero);
                if (TurnsAtEdges)
                {
                    // Look one body's width ahead for ground to step onto.
                    PixelRect rect = body.RoomRect;
                    int ahead = mover.Facing == Facing.Right ? rect.Width : -rect.Width;
                    if (mover2.IsFree(rect.MovedBy(ahead, 1), body.CollisionOwner, MoveOptions.AmongBlocks))
                    {
                        mover.Facing = mover.Facing.Opposite();
                        reachedEdge = true;
                    }
                }
            }
            else
            {
                mover.Velocity = mover.Velocity.WithY(mover.Velocity.Y + Gravity);
                hitFloor = mover2.MoveDown(body, mover.Velocity.Y, MoveOptions.AmongBlocks).Blocked;
            }
        }

        mover.Velocity = mover.Velocity.WithX(mover.Facing.Toward(Speed));
        MoveOptions options = Walks ? MoveOptions.AmongBlocks with { FollowGroundDown = 2 } : MoveOptions.AmongBlocks with { Slopes = SlopeTreatment.Wall };
        bool hitWall = mover2.MoveAcross(body, mover.Velocity.X, options).Blocked;
        if (hitWall)
            mover.Facing = mover.Facing.Opposite();
        return new MoveReport(hitWall, hitFloor, false, reachedEdge, false);
    }
}
