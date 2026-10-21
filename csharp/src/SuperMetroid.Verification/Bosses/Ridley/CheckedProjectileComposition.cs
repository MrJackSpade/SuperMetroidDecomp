namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The operand, direct and native of one checked projectile composition.</summary>
/// <param name="Operand">The operand.</param>
/// <param name="Direct">The direct.</param>
/// <param name="Native">The native.</param>
internal readonly record struct CheckedProjectileComposition(ushort Operand, ushort Direct, ushort Native);
