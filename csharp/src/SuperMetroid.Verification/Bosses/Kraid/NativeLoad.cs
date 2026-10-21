using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The source and instruction of one native load.</summary>
/// <param name="Source">The source.</param>
/// <param name="Instruction">The instruction.</param>
internal readonly record struct NativeLoad(KraidPaletteSource Source, int Instruction);
