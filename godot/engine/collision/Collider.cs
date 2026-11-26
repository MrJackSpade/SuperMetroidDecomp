using Engine.Shared;
using Godot;

namespace Engine.Collision;

/// <summary>
/// A rectangle with a role, belonging to the entity it is a child of. It is placed and sized relative to its owner's
/// origin and holds no behaviour: the collision world finds what it overlaps, and its owner decides what that means.
/// </summary>
[Tool]
public partial class Collider : Node2D
{
    /// <summary>The rectangle's top-left corner, in whole pixels from its owner's origin.</summary>
    [Export]
    public Vector2I TopLeft { get; set; }

    /// <summary>The rectangle's width and height, in whole pixels.</summary>
    [Export]
    public Vector2I Size { get; set; }

    /// <summary>What the collider is for.</summary>
    [Export]
    public ColliderRole Role { get; set; }

    /// <summary>Whether the collider takes part in collision at the moment.</summary>
    [Export]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether a solid collider stands as part of the room, as a wall does. A move tried only where it ends meets
    /// such a solid as it meets terrain: by the body's leading edge alone, and resting against it at the end of its
    /// pixel. It meets any other solid as a thing in its way. To a move that tries every pixel the two are alike.
    /// </summary>
    [Export]
    public bool StandsAsTerrain { get; set; }

    /// <summary>The entity this collider belongs to, once found.</summary>
    private ICollisionOwner? _owner;

    /// <summary>The rectangle relative to its owner's origin.</summary>
    public PixelRect Rect
    {
        get => new(TopLeft.X, TopLeft.Y, Size.X, Size.Y);
        set
        {
            TopLeft = new Vector2I(value.Left, value.Top);
            Size = new Vector2I(value.Width, value.Height);
        }
    }

    /// <summary>The entity this collider belongs to: the nearest node above it that owns colliders.</summary>
    /// <exception cref="InvalidOperationException">No node above it owns colliders.</exception>
    public ICollisionOwner CollisionOwner
    {
        get
        {
            if (_owner is not null)
                return _owner;
            for (Node? node = GetParent(); node is not null; node = node.GetParent())
            {
                if (node is ICollisionOwner owner)
                    return _owner = owner;
            }

            throw new InvalidOperationException($"The collider \"{Name}\" is not under anything that owns colliders.");
        }
    }

    /// <summary>The rectangle where it lies in the room right now.</summary>
    public PixelRect RoomRect => Rect.At(CollisionOwner.Position);

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!Godot.Engine.IsEditorHint())
            return;
        Color colour = Role switch
        {
            ColliderRole.Body => Colors.LimeGreen,
            ColliderRole.Hurtbox => Colors.Yellow,
            ColliderRole.Hitbox => Colors.Red,
            ColliderRole.Solid => Colors.CornflowerBlue,
            _ => Colors.White,
        };
        DrawRect(new Rect2(TopLeft, Size), colour, filled: false);
    }
}
