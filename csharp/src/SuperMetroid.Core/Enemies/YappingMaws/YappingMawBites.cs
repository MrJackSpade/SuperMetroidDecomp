namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Chooses a Yapping Maw's attack bite by direction.</summary>
internal static class YappingMawBites
{
    /// <summary>The attack bite for octant <paramref name="direction"/>, zero facing up and counting clockwise.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is outside zero through seven.</exception>
    internal static YappingMawProgram AttackForDirection(YappingMawAim aim) =>
        Ensure.Listed(YappingMawAnimationDefinitions.Attacks, aim);
}
