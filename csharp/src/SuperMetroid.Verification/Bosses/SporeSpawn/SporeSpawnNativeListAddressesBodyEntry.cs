using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>The program and entry of one spore spawn native list addresses body entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct SporeSpawnNativeListAddressesBodyEntry(SporeSpawnBodyProgram Program, ushort Entry);
