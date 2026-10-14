namespace SuperMetroid.Core.Game;

/// <summary>Value-only render input for the suit pickup's windowed fixed-color addition.</summary>
/// <param name="Beam">Outline and progress of the light beam.</param>
/// <param name="Red">Raw COLDATA red byte, including its component-enable bit.</param>
/// <param name="Green">Raw COLDATA green byte, including its component-enable bit.</param>
/// <param name="Blue">Raw COLDATA blue byte, including its component-enable bit.</param>
public readonly record struct SamusSuitPickupFlash(
    SamusSuitPickupBeam Beam,
    byte Red,
    byte Green,
    byte Blue);
