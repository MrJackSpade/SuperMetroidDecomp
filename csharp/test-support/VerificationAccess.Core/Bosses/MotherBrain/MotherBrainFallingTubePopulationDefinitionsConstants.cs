
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="MotherBrainFallingTubePopulationDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainFallingTubePopulationDefinitionsConstants
{
    /// <summary>Bank $A9 containing the native tube-collapse placement records.</summary>
    public const int NativeBank = 0xa90000;

    /// <summary>Provides the native bank of falling tube population records for Mother Brain room checks.</summary>
    extension(MotherBrainFallingTubePopulationDefinitions)
    {
        /// <inheritdoc cref="MotherBrainFallingTubePopulationDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => MotherBrainFallingTubePopulationDefinitionsConstants.NativeBank;
    }
}
