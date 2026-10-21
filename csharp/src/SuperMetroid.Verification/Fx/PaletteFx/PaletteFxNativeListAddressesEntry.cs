using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The program and entry of one palette fx native list addresses entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct PaletteFxNativeListAddressesEntry(PaletteFxProgram Program, ushort Entry);
