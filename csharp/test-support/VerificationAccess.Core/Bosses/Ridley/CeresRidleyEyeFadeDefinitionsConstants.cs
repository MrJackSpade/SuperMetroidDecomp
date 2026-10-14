
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Cartridge values of <see cref="CeresRidleyEyeFadeDefinitions"/> that only verification reads.</summary>
internal static class CeresRidleyEyeFadeDefinitionsConstants
{
    /// <summary>
    /// <c>RidleyFadeIn_EyeGlowIntensity</c> at $A6:E269: 64 palette-row selectors
    /// followed by the $FF completion marker at $A6:E2A9.
    /// </summary>
    public const int NativeScheduleAddress = 0xa6e269;

    /// <summary>Provides the source schedule address for checking Ridley's Ceres eye fade.</summary>
    extension(CeresRidleyEyeFadeDefinitions)
    {
        /// <inheritdoc cref="CeresRidleyEyeFadeDefinitionsConstants.NativeScheduleAddress"/>
        internal static int NativeScheduleAddress => CeresRidleyEyeFadeDefinitionsConstants.NativeScheduleAddress;
    }
}
