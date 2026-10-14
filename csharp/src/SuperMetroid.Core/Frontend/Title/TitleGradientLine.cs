namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Literal five-bit fixed color and CGADSUB control for a physical title scanline.</summary>
public readonly record struct TitleGradientLine(byte Red, byte Green, byte Blue, byte Control);
