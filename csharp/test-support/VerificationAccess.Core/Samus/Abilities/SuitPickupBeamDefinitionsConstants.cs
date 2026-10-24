
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Cartridge values of <see cref="SuitPickupBeamDefinitions"/> that only verification reads.</summary>
internal static class SuitPickupBeamDefinitionsConstants
{
    /// <summary><c>$88:E3C9</c>, 128-byte symmetric light-beam curve.</summary>
    public const int BeamCurve = 0x88e3c9;

    /// <summary>Provides the native suit-pickup beam curve used to check Samus's special sequence.</summary>
    extension(SuitPickupBeamDefinitions)
    {
        /// <inheritdoc cref="SuitPickupBeamDefinitionsConstants.BeamCurve"/>
        internal static int BeamCurve => SuitPickupBeamDefinitionsConstants.BeamCurve;
    }
}
