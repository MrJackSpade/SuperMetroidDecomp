namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive outlines the suit-pickup light beam takes across its bank-$88 stages.</summary>
public enum SamusSuitPickupBeamShape : byte
{
    /// <summary>No lit scanline: the setup calls before stage zero leave the beam window empty.</summary>
    None,
    /// <summary>Stage zero, <c>$88:E092</c>: the narrow beam between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/> grows inward from the top and bottom edges by <see cref="SamusSuitPickupBeam.Progress"/> scanlines each.</summary>
    GrowingFromEdges,
    /// <summary>Stage one, <c>$88:E0D7</c>: a uniform column from <see cref="SamusSuitPickupBeam.Left"/> to <see cref="SamusSuitPickupBeam.Right"/> on every scanline.</summary>
    Column,
    /// <summary>Stage two, <c>$88:E113</c>: the column widened on each scanline by the authored curve, widest at the vertical center.</summary>
    CurvedColumn,
    /// <summary>Stages three and four, <c>$88:E1BA</c>: the final curved column with <see cref="SamusSuitPickupBeam.Progress"/> scanlines wiped from the top and one fewer from the bottom.</summary>
    ClosingCurvedColumn,
    /// <summary>Stage five, <c>$88:E22B</c>: only the center scanline remains lit, between <see cref="SamusSuitPickupBeam.Left"/> and <see cref="SamusSuitPickupBeam.Right"/>.</summary>
    CenterLine,
}

/// <summary>
/// Shape and progress of the suit-pickup light beam, published by the simulation and rasterized
/// only on the render side.
/// </summary>
/// <param name="Shape">Current outline.</param>
/// <param name="Left">Inclusive left screen X of the column or center line; for the closing shape, the final curved column's left X.</param>
/// <param name="Right">Inclusive right screen X of the column or center line; a left greater than right lights nothing.</param>
/// <param name="Progress">Scanlines grown from each edge while growing, or wiped from the top while closing; unused by the other shapes.</param>
public readonly record struct SamusSuitPickupBeam(
    SamusSuitPickupBeamShape Shape,
    byte Left,
    byte Right,
    int Progress)
{
    /// <summary>Beam with no lit scanline.</summary>
    public static SamusSuitPickupBeam None => default;
}

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
