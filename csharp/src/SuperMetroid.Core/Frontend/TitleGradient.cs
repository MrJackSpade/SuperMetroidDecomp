namespace SuperMetroid.Core.Frontend;

/// <summary>Literal five-bit fixed color and CGADSUB control for a physical title scanline.</summary>
/// <param name="Red">Red fixed-color component used by the scanline's color arithmetic.</param>
/// <param name="Green">Green fixed-color component used by the scanline's color arithmetic.</param>
/// <param name="Blue">Blue fixed-color component used by the scanline's color arithmetic.</param>
/// <param name="Control">Raw CGADSUB bits selecting affected layers and addition or subtraction.</param>
public readonly record struct TitleGradientLine(byte Red, byte Green, byte Blue, byte Control);
