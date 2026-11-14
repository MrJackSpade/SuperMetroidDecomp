namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Chooses a Yapping Maw's attack bite by direction.</summary>
internal static class YappingMawBites
{
    /// <summary>The attack bite toward <paramref name="aim"/>, one of the eight directions a maw bites.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="aim"/> names no bite.</exception>
    internal static YappingMawSegment AttackForDirection(YappingMawAim aim) =>
        Ensure.Listed(YappingMawAnimationDefinitions.Attacks, aim);
}
