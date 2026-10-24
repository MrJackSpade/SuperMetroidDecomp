
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="BeamComboDefinitions"/> that only verification reads.</summary>
internal static class BeamComboDefinitionsConstants
{
    /// <summary>IcePlasmaSBAProjectileOriginAngles at $90:CD08.</summary>
    public const int OriginAngles = 0x90cd08;
    /// <summary>SineCosineTables_8bitSine_SignExtended at $A0:B443, positive half-wave words.</summary>
    public const int PositiveSine = 0xa0b443;

    /// <summary>Provides combo origin angles and positive sine values for Samus combo trajectory checks.</summary>
    extension(BeamComboDefinitions)
    {
        /// <inheritdoc cref="BeamComboDefinitionsConstants.OriginAngles"/>
        internal static int OriginAngles => BeamComboDefinitionsConstants.OriginAngles;
        /// <inheritdoc cref="BeamComboDefinitionsConstants.PositiveSine"/>
        internal static int PositiveSine => BeamComboDefinitionsConstants.PositiveSine;
    }
}
