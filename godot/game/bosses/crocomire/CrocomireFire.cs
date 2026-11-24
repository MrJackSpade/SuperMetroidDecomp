using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// A ball of Crocomire's fire. Its first move is straight across at the speed it is launched at, stopping short of a
/// block but carrying on all the same; from its next update it flies at the speed Crocomire aims it at, across and then
/// down, and is gone without a trace where a block stops it either way.
/// </summary>
public partial class CrocomireFire : Projectile
{
    private PixelVelocity _speed;
    private bool _moved;

    /// <summary>Gives it the speed it flies at from its next move on.</summary>
    public void Aim(PixelVelocity aimed) => _speed = aimed;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        // It moves itself, meeting terrain an axis at a time.
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_moved)
        {
            // The old fire's first move does not ask whether a block stopped it.
            _moved = true;
            MoveAcrossMeetingTerrain(room, _speed.X);
            return;
        }

        if (MoveAcrossMeetingTerrain(room, _speed.X) || MoveDownMeetingTerrain(room, _speed.Y))
            Leave(room);
    }
}
