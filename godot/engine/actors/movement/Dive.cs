using Engine.Collision;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// Waiting, then dropping at a target: the mover holds still until its target comes within reach across, then dives at
/// a set speed until something stops it. It then either stays where it landed or climbs back to where it waited.
/// </summary>
public sealed class Dive
{
    /// <summary>How near across the target must come, in pixels, to set off the dive.</summary>
    public int Reach { get; set; } = 48;

    /// <summary>How far it travels down each update of the dive.</summary>
    public Pixels DiveSpeed { get; set; } = Pixels.FromWhole(3);

    /// <summary>How far it travels across each update of the dive, toward where the target was.</summary>
    public Pixels AcrossSpeed { get; set; }

    /// <summary>Whether it climbs back to where it waited after a dive.</summary>
    public bool Returns { get; set; }

    /// <summary>How far it travels each update while climbing back.</summary>
    public Pixels ReturnSpeed { get; set; } = Pixels.FromWhole(1);

    private enum Doing
    {
        Waiting,
        Diving,
        Returning,
        Done,
    }

    private Doing _doing;
    private PixelPosition _waitedAt;
    private Facing _toward;

    /// <summary>Moves <paramref name="mover"/> for one update, with its target at <paramref name="target"/>.</summary>
    public MoveReport Step(IMoves mover, PixelPosition target)
    {
        Mover room = mover.Room.Mover;
        Collider body = mover.MovingBody;
        MoveOptions air = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        switch (_doing)
        {
            case Doing.Waiting:
                if ((target.X - mover.Position.X).Abs() > Pixels.FromWhole(Reach))
                    return MoveReport.Nothing;
                _waitedAt = mover.Position;
                _toward = target.X >= mover.Position.X ? Facing.Right : Facing.Left;
                mover.Facing = _toward;
                _doing = Doing.Diving;
                return MoveReport.Nothing;

            case Doing.Diving:
                bool hitWall = room.MoveAcross(body, _toward.Toward(AcrossSpeed), air).Blocked;
                bool hitFloor = room.MoveDown(body, DiveSpeed, air).Blocked;
                mover.Velocity = new PixelVelocity(_toward.Toward(AcrossSpeed), DiveSpeed);
                if (!hitFloor)
                    return new MoveReport(hitWall, false, false, false, false);
                mover.Velocity = PixelVelocity.Zero;
                _doing = Returns ? Doing.Returning : Doing.Done;
                return new MoveReport(hitWall, true, false, false, !Returns);

            case Doing.Returning:
                Pixels up = mover.Position.Y - _waitedAt.Y;
                Pixels step = up < ReturnSpeed ? up : ReturnSpeed;
                bool hitCeiling = room.MoveDown(body, -step, air).Blocked;
                if (!hitCeiling && mover.Position.Y > _waitedAt.Y)
                    return MoveReport.Nothing;
                _doing = Doing.Waiting;
                return new MoveReport(false, false, hitCeiling, false, true);

            case Doing.Done:
                return MoveReport.Nothing;

            default:
                throw new InvalidOperationException($"Undefined dive stage {_doing}.");
        }
    }
}
