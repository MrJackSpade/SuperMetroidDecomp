namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>A run of consecutive palette colors the intro palette fader clears, fades in or fades out.</summary>
/// <param name="FirstColor">Index of the run's first color among the 256 palette colors.</param>
/// <param name="ColorCount">Number of colors in the run, at least one.</param>
public readonly record struct IntroPaletteSpan(int FirstColor, int ColorCount);
