using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The address, definition and name of one torizo randomized projectile definitions definition.</summary>
/// <param name="Address">The address.</param>
/// <param name="Definition">The definition.</param>
/// <param name="Name">The name.</param>
internal readonly record struct TorizoRandomizedProjectileDefinitionsDefinition(int Address, TorizoRandomizedProjectileDefinition Definition, string Name);
