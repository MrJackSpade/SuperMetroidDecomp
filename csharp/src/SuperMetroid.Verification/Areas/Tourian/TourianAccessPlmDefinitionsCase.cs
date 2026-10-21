namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>The clear and spawn operand of one tourian access plm definitions case.</summary>
/// <param name="Clear">The clear.</param>
/// <param name="SpawnOperand">The spawn operand.</param>
internal readonly record struct TourianAccessPlmDefinitionsCase(bool Clear, int SpawnOperand);
