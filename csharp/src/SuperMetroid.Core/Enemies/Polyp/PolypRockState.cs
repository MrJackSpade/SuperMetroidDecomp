using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>What a Polyp's rock keeps: whether it is still rising or has passed the top of its arc.</summary>
internal sealed class PolypRockState : EnemyProjectileState
{
    /// <summary>The part of its arc the rock is in; it starts rising.</summary>
    internal PolypRockFunction Phase { get; set; } = PolypRockFunction.Rising;
}
