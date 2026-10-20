using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainTopTubeInstructionProgramDefinitionsTooling"/> that only verification reads.</summary>
internal static class MotherBrainTopTubeInstructionProgramDefinitionsToolingConstants
{
    /// <summary>Distinguishes compiled top-tube mechanics bytes from presentation data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => MotherBrainTopTubeInstructionProgramDefinitionsTooling.Layout.IsCompiledMechanicsByte(address);
}
