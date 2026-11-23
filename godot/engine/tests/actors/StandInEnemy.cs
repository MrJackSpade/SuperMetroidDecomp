using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Engine.Tests.World;

namespace Engine.Tests.Actors;

/// <summary>
/// An enemy for the engine's own checks: 16 pixels wide and high around its place, with a body, a hurtbox and a solid
/// that is off until it is frozen. It moves at whatever speed it is given and counts its updates.
/// </summary>
public partial class StandInEnemy : Enemy
{
    /// <summary>How many updates the enemy has behaved for.</summary>
    public int UpdatesBehaved { get; private set; }

    /// <summary>A stand-in enemy with its colliders.</summary>
    public StandInEnemy()
    {
        var shape = new PixelRect(-8, -8, 16, 16);
        StandInWorld.AddCollider(this, ColliderRole.Body, shape);
        StandInWorld.AddCollider(this, ColliderRole.Hurtbox, shape);
        StandInWorld.AddCollider(this, ColliderRole.Solid, shape).Enabled = false;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        UpdatesBehaved++;
        if (!Velocity.X.IsZero)
            MoveAcross(MoveOptions.Plain);
        if (!Velocity.Y.IsZero)
            MoveDown(MoveOptions.Plain);
    }
}
