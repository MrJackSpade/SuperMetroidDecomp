using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Value-only render input for the suit pickup's windowed fixed-colour addition.</summary>
/// <param name="Beam">Outline and progress of the light beam.</param>
/// <param name="Color">The colour added inside the beam, as the accepted frame latched it.</param>
public readonly record struct SamusSuitPickupFlash(SamusSuitPickupBeam Beam, RgbColor Color);
