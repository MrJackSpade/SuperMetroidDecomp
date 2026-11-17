using static SuperMetroid.Core.Bosses.Phantoon.PhantoonDeathExplosionDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's death explosions in the order they burst (#2315): the opening explosions, then the repeated ones, which
/// start again from their first after their last.
/// </summary>
public static class PhantoonDeathExplosions
{
    /// <summary>The explosions in the order they burst: the opening explosions, then the repeated ones once.</summary>
    public static IReadOnlyList<PhantoonDeathExplosionDefinition> Schedule { get; } = [.. Opening, .. Repeated];

    /// <summary>His first death explosion.</summary>
    public static ListStep<PhantoonDeathExplosionDefinition> First => ListStep<PhantoonDeathExplosionDefinition>.FirstOf(Schedule);

    /// <summary>The explosion the schedule starts again from after its last: the first of the repeated explosions.</summary>
    public static ListStep<PhantoonDeathExplosionDefinition> FirstRepeated => ListStep<PhantoonDeathExplosionDefinition>.At(Schedule, Opening.Length);
}
