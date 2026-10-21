using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The program and entry of one torizo native list addresses projectile entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct TorizoNativeListAddressesProjectileEntry(TorizoProjectileProgram Program, ushort Entry);
