using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// A bullet from one of the turrets in Mother Brain's room. It flies straight and is gone without a trace once its
/// middle is in a block. It stops Samus's shots only on every other update, from its first.
/// </summary>
public partial class MotherBrainTurretBullet : Projectile
{
    private PixelVelocity _speed;

    /// <inheritdoc/>
    /// <remarks>
    /// The old bullet switches whether it stops shots each time it moves, before her shots meet it in that update: from
    /// stopping none it stops them after its first move, not after its second, and so on.
    /// </remarks>
    public override bool IsHitBy(HitSide side) => Age % 2 == 0 && base.IsHitBy(side);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        // It moves itself, so that it meets a block by its middle alone.
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Position += _speed.OverOneUpdate;
        if (room.Terrain.IsSolidIn(new PixelRect(Position.X.Whole, Position.Y.Whole, 1, 1), SlopeTreatment.Wall))
            Leave(room);
    }
}
