namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive native window endpoints; inverted endpoints hide the reveal on that line.</summary>
public readonly record struct XrayWindowLine(byte Left, byte Right);
