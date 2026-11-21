namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>The program and entry of one spore spawn native list addresses projectile entry.</summary>
/// <typeparam name="TSegment">The program's family of segments.</typeparam>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct SporeSpawnNativeListAddressesProjectileEntry<TSegment>(TSegment Program, ushort Entry);
