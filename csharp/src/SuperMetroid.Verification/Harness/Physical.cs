namespace SuperMetroid.Verification.Harness;

/// <summary>The upper, set and pointer of one physical.</summary>
/// <param name="Upper">The upper.</param>
/// <param name="Set">The set.</param>
/// <param name="Pointer">The pointer.</param>
internal readonly record struct Physical(bool Upper, int Set, ushort Pointer);
