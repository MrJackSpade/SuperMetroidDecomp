namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Value-only render input for the suit pickup's windowed fixed-color addition.</summary>
/// <param name="Beam">Outline and progress of the light beam.</param>
/// <param name="Color">The fixed color added inside the beam, as the NMI latched it, including each component's select bit.</param>
public readonly record struct SamusSuitPickupFlash(
    SamusSuitPickupBeam Beam,
    FixedColorRegisterSnapshot Color)
{
    /// <summary>Builds the flash from the three fixed-color bytes the NMI latched for the suit pickup.</summary>
    /// <param name="beam">Outline and progress of the light beam.</param>
    /// <param name="red">Latched red COLDATA write, including its select bit.</param>
    /// <param name="green">Latched green COLDATA write, including its select bit.</param>
    /// <param name="blue">Latched blue COLDATA write, including its select bit.</param>
    public SamusSuitPickupFlash(SamusSuitPickupBeam beam, int red, int green, int blue)
        : this(beam, new FixedColorRegisterSnapshot(red, green, blue))
    {
    }
}
