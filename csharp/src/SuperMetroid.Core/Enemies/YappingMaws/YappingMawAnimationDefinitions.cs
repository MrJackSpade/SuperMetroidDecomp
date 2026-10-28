namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Animation selection for the Yapping Maw.</summary>
internal static class YappingMawAnimationDefinitions
{
    /// <summary>The eight attack bites in the native clockwise octant order, starting facing up.</summary>
    internal static readonly YappingMawProgram[] Attacks =
    [
        YappingMawProgram.AttackingUp, YappingMawProgram.AttackingUpRight, YappingMawProgram.AttackingRight,
        YappingMawProgram.AttackingDownRight, YappingMawProgram.AttackingDown, YappingMawProgram.AttackingDownLeft,
        YappingMawProgram.AttackingLeft, YappingMawProgram.AttackingUpLeft,
    ];
}
