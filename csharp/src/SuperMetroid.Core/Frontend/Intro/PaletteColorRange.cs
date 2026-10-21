namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>A range of palette colours.</summary>
/// <param name="FirstColor">The first colour.</param>
/// <param name="ExclusiveEnd">The colour after the last.</param>
public readonly record struct PaletteColorRange(int FirstColor, int ExclusiveEnd);
