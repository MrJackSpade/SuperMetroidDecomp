namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon's animations turned to a side: the body's idle and attack and the wing's flapping, facing left or right.</summary>
internal static class DragonPhases
{
    /// <summary>The body's idle pose, facing left or right.</summary>
    internal static DragonSegment Idle(bool facingLeft) => facingLeft ? DragonSegment.IdleFacingLeft : DragonSegment.IdleFacingRight;

    /// <summary>The wing's flapping, facing left or right.</summary>
    internal static DragonSegment Wings(bool facingLeft) => facingLeft ? DragonSegment.WingsFacingLeft : DragonSegment.WingsFacingRight;

    /// <summary>The body's attack, facing left or right.</summary>
    internal static DragonSegment Attacking(bool facingLeft) => facingLeft ? DragonSegment.AttackingFacingLeft : DragonSegment.AttackingFacingRight;
}
