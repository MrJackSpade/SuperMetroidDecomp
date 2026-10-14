namespace SuperMetroid.Core.Game;

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
