namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Viewport positions in 1/256 pixels: unsigned 8.8 words whose high byte is the whole
/// <see cref="ScreenPixels"/> coordinate (the escape timer's on-screen position).
/// </summary>
public sealed class ScreenSubpixels : ISpatialSpace;
