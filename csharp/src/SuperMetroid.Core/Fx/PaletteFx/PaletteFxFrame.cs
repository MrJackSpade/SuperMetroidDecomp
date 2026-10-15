namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One timed colour record; its layout and installed colours are confined to <see cref="Artwork.PaletteFxRecordDefinitions"/>.</summary>
/// <param name="Record">The record, in the order the programs show them.</param>
public readonly record struct PaletteFxFrame(int Record);
