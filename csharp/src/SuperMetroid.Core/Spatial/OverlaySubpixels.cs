namespace SuperMetroid.Core.Spatial;

/// <summary>
/// BG3 room-FX texture drift in 1/256 pixels: the 8.8 rain, fog and spores accumulators whose
/// signed high byte is a whole <see cref="OverlayPixels"/> displacement.
/// </summary>
public sealed class OverlaySubpixels : ISpatialSpace;
