namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A hyper beam colour and the channels it supplies.</summary>
/// <param name="Value">The value.</param>
/// <param name="Mask">The mask.</param>
internal readonly record struct HyperBeamMaskedColor(RgbColor Value, int Mask);
