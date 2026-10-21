namespace SuperMetroid.Core.Rendering;

/// <summary>A sampled background pixel's colour and priority.</summary>
/// <param name="Color">The color.</param>
/// <param name="High">The high.</param>
internal readonly record struct BackgroundPixelSample(Rgba32 Color, bool High);
