namespace Engine.Collision;

/// <summary>What a collider is for.</summary>
public enum ColliderRole
{
    /// <summary>The shape its owner moves through terrain with.</summary>
    Body,

    /// <summary>Where its owner can be hit.</summary>
    Hurtbox,

    /// <summary>Where its owner hits others.</summary>
    Hitbox,

    /// <summary>A shape that blocks bodies and can be stood on.</summary>
    Solid,
}
