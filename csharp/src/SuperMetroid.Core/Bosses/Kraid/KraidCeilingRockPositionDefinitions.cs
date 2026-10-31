using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled ceiling-break projectile placement data.</summary>
public static class KraidCeilingRockPositionDefinitions
{
    /// <summary>
    /// $A7:ACB3: the room X of each ceiling rock, in the order Kraid's growth drops them. The rocks alternate between
    /// the gaps at block columns 2..6 and 10..13, taking each gap's right then left remaining edge inward, at each
    /// block's center (16 * column + 8).
    /// </summary>
    public static ImmutableArray<int> Xs { get; } = [104, 216, 40, 168, 88, 200, 56, 184, 72];
}
