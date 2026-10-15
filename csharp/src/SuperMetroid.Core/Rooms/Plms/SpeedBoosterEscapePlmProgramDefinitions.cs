using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.SpeedBoosterEscapeProgram, SuperMetroid.Core.Rooms.Plms.PlmNoDraw, SuperMetroid.Core.Rooms.Plms.SpeedBoosterEscapePreInstruction>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The Speed Booster lavaquake's PLM program (#1296), in place of its bank-$84 list at $84:B88A: three phases, each
/// installing the pre-instruction that watches for its end and sleeping until that pre-instruction wakes it.
/// </summary>
internal static class SpeedBoosterEscapePlmProgramDefinitions
{
    /// <summary>The lavaquake phases; each cue installs the named pre-instruction.</summary>
    internal static AnimationProgramSet<SpeedBoosterEscapeProgram, PlmNoDraw, SpeedBoosterEscapePreInstruction> Programs { get; } = new(
        new Dictionary<SpeedBoosterEscapeProgram, Step[]>
        {
            [SpeedBoosterEscapeProgram.WaitForSpeedBooster] = [Step.Publish(SpeedBoosterEscapePreInstruction.WaitForSpeedBooster), Step.Hold],
            [SpeedBoosterEscapeProgram.WaitForSamusLeft] = [Step.Publish(SpeedBoosterEscapePreInstruction.WaitForSamusLeft), Step.Hold],
            [SpeedBoosterEscapeProgram.AdvanceLava] = [Step.Publish(SpeedBoosterEscapePreInstruction.AdvanceLava), Step.Hold],
        });

    /// <summary>The phase a wake past <paramref name="phase"/>'s sleep continues with; the last phase never wakes.</summary>
    internal static SpeedBoosterEscapeProgram? PhaseAfter(SpeedBoosterEscapeProgram phase) => phase switch
    {
        SpeedBoosterEscapeProgram.WaitForSpeedBooster => SpeedBoosterEscapeProgram.WaitForSamusLeft,
        SpeedBoosterEscapeProgram.WaitForSamusLeft => SpeedBoosterEscapeProgram.AdvanceLava,
        SpeedBoosterEscapeProgram.AdvanceLava => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterEscapeProgram)} {phase}."),
    };
}
