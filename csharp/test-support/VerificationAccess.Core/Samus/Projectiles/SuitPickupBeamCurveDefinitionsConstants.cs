
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="SuitPickupBeamCurveDefinitions"/> that only verification reads.</summary>
internal static class SuitPickupBeamCurveDefinitionsConstants
{
    /// <summary>Native source address of the 128-byte contour at <c>$88:E3C9</c>.</summary>
    public const int NativeCurveAddress = 0x88e3c9;

    /// <summary>Provides the native suit-pickup curve address for beam animation checks.</summary>
    extension(SuitPickupBeamCurveDefinitions)
    {
        /// <inheritdoc cref="SuitPickupBeamCurveDefinitionsConstants.NativeCurveAddress"/>
        internal static int NativeCurveAddress => SuitPickupBeamCurveDefinitionsConstants.NativeCurveAddress;
    }
}
