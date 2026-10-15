namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres Baby's programs, with the operand its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The program a branch continues at.</param>
/// <param name="PaletteRow">The palette row an <see cref="CeresBabyCueKind.UpdateColors"/> cue loads.</param>
internal readonly record struct CeresBabyCue(CeresBabyCueKind Kind, CeresBabyProgram Target = default, int PaletteRow = 0);
