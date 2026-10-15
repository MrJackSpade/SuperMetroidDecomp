namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres steam vents' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The program a branch continues at.</param>
/// <param name="Alternate">The program a two-way branch continues at otherwise.</param>
internal readonly record struct CeresSteamCue(CeresSteamCueKind Kind, CeresSteamProgram Target = default, CeresSteamProgram Alternate = default);
