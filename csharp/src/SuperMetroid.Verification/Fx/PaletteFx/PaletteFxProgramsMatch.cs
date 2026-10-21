using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The key, address and position of one palette fx programs match.</summary>
/// <param name="Key">The key.</param>
/// <param name="Address">The address.</param>
/// <param name="Position">The position.</param>
internal readonly record struct PaletteFxProgramsMatch(PaletteFxProgram Key, ushort Address, int Position);
