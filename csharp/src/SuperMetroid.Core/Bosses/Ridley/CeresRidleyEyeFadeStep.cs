namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One Ceres Ridley eye-fade row selection or the terminal handoff.</summary>
/// <param name="IsComplete">Whether the schedule has ended and the body fade begins.</param>
/// <param name="PaletteRow">The eye palette row shown this step, zero through fifteen; zero at completion.</param>
internal readonly record struct CeresRidleyEyeFadeStep(bool IsComplete, int PaletteRow);
