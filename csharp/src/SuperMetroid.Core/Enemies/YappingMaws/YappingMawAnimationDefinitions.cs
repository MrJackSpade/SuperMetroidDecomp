namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Animation selection for the Yapping Maw.</summary>
internal static class YappingMawAnimationDefinitions
{
    /// <summary>The eight attack bites in the native clockwise octant order, starting facing up.</summary>
    private static readonly YappingMawProgram[] Attacks =
    [
        YappingMawProgram.AttackingUp, YappingMawProgram.AttackingUpRight, YappingMawProgram.AttackingRight,
        YappingMawProgram.AttackingDownRight, YappingMawProgram.AttackingDown, YappingMawProgram.AttackingDownLeft,
        YappingMawProgram.AttackingLeft, YappingMawProgram.AttackingUpLeft,
    ];

    /// <summary>The attack bite for octant <paramref name="direction"/>, zero facing up and counting clockwise.</summary>
    internal static YappingMawProgram AttackForDirection(int direction)
    {
        if ((uint)direction >= Attacks.Length)
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Yapping Maw direction must be zero through seven.");
        return Attacks[direction];
    }
}
