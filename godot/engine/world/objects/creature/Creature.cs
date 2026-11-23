using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Shared;

namespace Engine.World.Objects;

/// <summary>
/// A harmless animal that moves by its own behaviour. It cannot be hurt and hurts nothing. A particular creature
/// supplies only its behaviour.
/// </summary>
public abstract partial class Creature : RoomObject, IMoves
{
    /// <inheritdoc/>
    public PixelVelocity Velocity { get; set; }

    /// <inheritdoc/>
    public Facing Facing { get; set; }

    /// <inheritdoc/>
    public Collider MovingBody =>
        EntityNode.ColliderOf(this, ColliderRole.Body) ?? throw new InvalidOperationException($"The creature \"{Name}\" has no body collider.");

    /// <inheritdoc/>
    protected override void OnStep() => Behave();

    /// <summary>This creature's own behaviour for one update.</summary>
    protected abstract void Behave();
}
