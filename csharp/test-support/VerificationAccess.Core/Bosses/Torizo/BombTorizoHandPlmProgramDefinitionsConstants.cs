using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Torizo.BombTorizoHandProgram, SuperMetroid.Core.Bosses.Torizo.BombTorizoHandDraw, SuperMetroid.Core.Bosses.Torizo.BombTorizoHandCue>;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="BombTorizoHandPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class BombTorizoHandPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:D33B</c>: the pre-instruction that wakes the hand once Samus has Bombs.</summary>
    internal const ushort WakeIfSamusHasBombsPreInstruction = 0xd33b;
}
