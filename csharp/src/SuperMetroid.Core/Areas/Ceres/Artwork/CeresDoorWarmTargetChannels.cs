namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>The seven shared paint channels the warm Ceres targets calculate from.</summary>
/// <param name="HighlightBlue">The white highlight's blue.</param>
/// <param name="AmberRed">The amber target's red.</param>
/// <param name="AmberGreen">The amber target's green.</param>
/// <param name="Red">The red target's red.</param>
/// <param name="GoldRed">The brightest gold's red.</param>
/// <param name="GoldGreen">The brightest gold's green.</param>
/// <param name="GoldBlue">The brightest gold's blue.</param>
internal readonly record struct CeresDoorWarmTargetChannels(int HighlightBlue, int AmberRed, int AmberGreen, int Red,
    int GoldRed, int GoldGreen, int GoldBlue);
