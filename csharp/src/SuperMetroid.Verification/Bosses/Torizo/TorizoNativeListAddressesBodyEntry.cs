using SuperMetroid.Core.Bosses.Torizo;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The program and entry of one torizo native list addresses body entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct TorizoNativeListAddressesBodyEntry(TorizoBodyProgram Program, ushort Entry);
