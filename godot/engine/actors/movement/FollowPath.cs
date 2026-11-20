using Engine.Shared;
using Engine.World;

namespace Engine.Actors.Movement;

/// <summary>
/// Moving along a set path, placed on it each update. Nothing stops it: a thing on a path goes where the path goes.
/// Platforms and carriers follow paths the same way.
/// </summary>
public sealed class FollowPath
{
    private readonly PathFollower _path;

    /// <summary>A follower of <paramref name="points"/>, moving <paramref name="speed"/> each update.</summary>
    public FollowPath(IReadOnlyList<PixelPosition> points, Pixels speed, PathFollower.AtEnd atEnd) =>
        _path = new PathFollower(points, speed, atEnd);

    /// <summary>Moves <paramref name="mover"/> for one update along its path.</summary>
    public MoveReport Step(IMoves mover)
    {
        PixelOffset moved = _path.Step();
        mover.Position = _path.Position;
        mover.Velocity = new PixelVelocity(moved.X, moved.Y);
        if (!moved.X.IsZero)
            mover.Facing = moved.X.IsNegative ? Facing.Left : Facing.Right;
        return new MoveReport(false, false, false, false, _path.IsFinished);
    }
}
