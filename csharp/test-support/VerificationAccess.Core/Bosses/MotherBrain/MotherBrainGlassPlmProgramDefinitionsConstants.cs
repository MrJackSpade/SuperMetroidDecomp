using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassPlmProgram, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassDraw, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassPlmCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainGlassPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainGlassPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:D1E6</c>: the pre-instruction that counts missile hits on the glass.</summary>
    internal const ushort HitPreInstruction = 0xd1e6;
}
