namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One Ceres Ridley eye-fade row selection or the terminal handoff.</summary>
internal readonly record struct CeresRidleyEyeFadeStep(bool IsComplete, byte PaletteRow);
