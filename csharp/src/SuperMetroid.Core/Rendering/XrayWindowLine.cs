namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive native window endpoints; inverted endpoints hide the reveal on that line.</summary>
/// <param name="Left">Inclusive left screen coordinate of the beam reveal.</param>
/// <param name="Right">Inclusive right screen coordinate; an inverted pair hides this scanline.</param>
public readonly record struct XrayWindowLine(byte Left, byte Right);
