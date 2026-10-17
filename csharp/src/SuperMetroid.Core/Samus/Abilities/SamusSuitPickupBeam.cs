namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Shape and progress of the suit-pickup light beam, published by the simulation and rasterized
/// only on the render side.
/// </summary>
/// <param name="Shape">Current outline.</param>
/// <param name="Left">Inclusive left screen column of the column or center line, 0 through 255; only X is meaningful. For the closing shape, the final curved column's left edge.</param>
/// <param name="Right">Inclusive right screen column, 0 through 255; only X is meaningful. A left greater than right lights nothing.</param>
/// <param name="Progress">Scanlines grown from each edge while growing, or wiped from the top while closing; unused by the other shapes.</param>
public readonly record struct SamusSuitPickupBeam(
    SamusSuitPickupBeamShape Shape,
    Position16<ScreenPixels> Left,
    Position16<ScreenPixels> Right,
    int Progress)
{
    /// <summary>Beam with no lit scanline.</summary>
    public static SamusSuitPickupBeam None => default;
}
