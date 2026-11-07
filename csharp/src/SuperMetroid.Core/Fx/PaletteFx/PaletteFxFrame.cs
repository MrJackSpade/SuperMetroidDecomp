namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One timed colour record; its colours are installed artwork keyed by <see cref="PaletteFxColorKey"/>.</summary>
/// <param name="Record">The record, in the order the programs show them.</param>
public readonly record struct PaletteFxFrame(int Record);
