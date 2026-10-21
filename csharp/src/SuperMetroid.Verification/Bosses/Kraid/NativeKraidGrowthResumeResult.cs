namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The pointer and timer of one native kraid growth resume result.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Timer">The timer.</param>
internal readonly record struct NativeKraidGrowthResumeResult(ushort Pointer, ushort Timer);
