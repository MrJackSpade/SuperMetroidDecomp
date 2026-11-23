using Engine.Collision;
using Engine.Shared;
using Godot;

namespace Engine.Tests.Collision;

/// <summary>Something with a place and colliders, for the engine's own collision checks.</summary>
public partial class StandInBody : Node2D, ICollisionOwner
{
    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>Adds a collider of <paramref name="role"/> covering <paramref name="rect"/> relative to this body's origin.</summary>
    public Collider AddCollider(ColliderRole role, PixelRect rect)
    {
        var collider = new Collider { Role = role, Rect = rect };
        AddChild(collider);
        return collider;
    }
}
