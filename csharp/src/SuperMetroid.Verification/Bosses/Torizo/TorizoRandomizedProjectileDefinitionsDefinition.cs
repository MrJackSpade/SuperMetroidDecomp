using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The address, definition and name of one torizo randomized projectile definitions definition.</summary>
/// <typeparam name="TSegment">The segments of the launched projectile's animation family.</typeparam>
/// <param name="Address">The address.</param>
/// <param name="Definition">The definition.</param>
/// <param name="Name">The name.</param>
internal readonly record struct TorizoRandomizedProjectileDefinitionsDefinition<TSegment>(int Address, TorizoRandomizedProjectileDefinition<TSegment> Definition, string Name)
    where TSegment : struct;
