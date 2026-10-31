using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.SpeedBoosterEscapeProgram, SuperMetroid.Core.Rooms.Plms.PlmNoDraw, SuperMetroid.Core.Rooms.Plms.SpeedBoosterEscapeFrameBehavior>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The Speed Booster lavaquake's PLM program (#1296), in place of its bank-$84 list at $84:B88A: three phases, each
/// installing the pre-instruction that watches for its end and sleeping until that pre-instruction wakes it.
/// </summary>
internal static class SpeedBoosterEscapePlmProgramDefinitions
{
    /// <summary>The lavaquake phases; each cue installs the named pre-instruction.</summary>
    internal static AnimationProgramSet<SpeedBoosterEscapeProgram, PlmNoDraw, SpeedBoosterEscapeFrameBehavior> Programs { get; } = new(
        new Dictionary<SpeedBoosterEscapeProgram, Step[]>
        {
            [SpeedBoosterEscapeProgram.WaitForSpeedBooster] = [Step.Publish(SpeedBoosterEscapeFrameBehavior.WaitForSpeedBooster), Step.Hold],
            [SpeedBoosterEscapeProgram.WaitForSamusLeft] = [Step.Publish(SpeedBoosterEscapeFrameBehavior.WaitForSamusLeft), Step.Hold],
            [SpeedBoosterEscapeProgram.AdvanceLava] = [Step.Publish(SpeedBoosterEscapeFrameBehavior.AdvanceLava), Step.Hold],
        });
}
