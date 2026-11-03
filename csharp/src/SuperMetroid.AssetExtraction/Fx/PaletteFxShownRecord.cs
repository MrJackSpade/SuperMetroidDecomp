using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// One record a palette-FX program can show, under the colour group its last cue named (#1512): the import pre-colours a
/// shade for each.
/// </summary>
/// <param name="Colours">The colour group the record writes.</param>
/// <param name="Record">The record shown.</param>
internal readonly record struct PaletteFxShownRecord(PaletteFxColours Colours, PaletteFxFrame Record);
