using SuperMetroid.Core.Enemies.Metroids;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>The loop and cries of one verify metroid behavior case.</summary>
/// <param name="Loop">The loop.</param>
/// <param name="Cries">The cries.</param>
internal readonly record struct VerifyMetroidBehaviorCase(MetroidProgram Loop, int Cries);
