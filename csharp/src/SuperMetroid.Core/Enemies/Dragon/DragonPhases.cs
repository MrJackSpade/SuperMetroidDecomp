namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon's animations turned to a side: the body's idle and attack and the wing's flapping, facing left or right.</summary>
internal static class DragonPhases
{
    /// <summary>The body's idle pose, facing <paramref name="facing"/>.</summary>
    internal static DragonSegment Idle(HorizontalDirection facing) => facing == HorizontalDirection.Left ? DragonSegment.IdleFacingLeft : DragonSegment.IdleFacingRight;

    /// <summary>The wing's flapping, facing <paramref name="facing"/>.</summary>
    internal static DragonSegment Wings(HorizontalDirection facing) => facing == HorizontalDirection.Left ? DragonSegment.WingsFacingLeft : DragonSegment.WingsFacingRight;

    /// <summary>The body's attack, facing <paramref name="facing"/>.</summary>
    internal static DragonSegment Attacking(HorizontalDirection facing) => facing == HorizontalDirection.Left ? DragonSegment.AttackingFacingLeft : DragonSegment.AttackingFacingRight;
}
