using Engine.Shared;

namespace Engine.Collision;

/// <summary>Something that has colliders: it stands at an exact place in a room, and its colliders stand relative to it.</summary>
public interface ICollisionOwner
{
    /// <summary>Where it stands in the room.</summary>
    PixelPosition Position { get; set; }
}
