namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>The operand and source of one original tourian frames result.</summary>
/// <param name="Operand">The operand.</param>
/// <param name="Source">The source.</param>
internal readonly record struct OriginalTourianFramesResult(ushort Operand, int Source);
