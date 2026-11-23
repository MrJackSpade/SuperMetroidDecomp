using Engine.Collision;
using Engine.Shared;
using Engine.World;

namespace Engine.Actors.Movement;

/// <summary>Something the shared ways of moving can move: it has a place, a speed, a facing and a body, in a room.</summary>
public interface IMoves
{
    /// <summary>Where it stands.</summary>
    PixelPosition Position { get; set; }

    /// <summary>Its speed.</summary>
    PixelVelocity Velocity { get; set; }

    /// <summary>The way it faces.</summary>
    Facing Facing { get; set; }

    /// <summary>The collider it moves through terrain with.</summary>
    Collider MovingBody { get; }

    /// <summary>The room it is in.</summary>
    Room Room { get; }
}
