using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.RoomObjects.SpeedBoosterEscapeProgram, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoDraw, SuperMetroid.Core.Rooms.RoomObjects.SpeedBoosterEscapeFrameBehavior>;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The Speed Booster lavaquake's PLM program (#1296), in place of its list: three phases, each
/// installing the pre-instruction that watches for its end and sleeping until that pre-instruction wakes it.
/// </summary>
internal static class SpeedBoosterEscapeObjectProgramDefinitions
{
    /// <summary>The lavaquake phases; each cue installs the named pre-instruction.</summary>
    internal static AnimationProgramSet<SpeedBoosterEscapeProgram, RoomObjectNoDraw, SpeedBoosterEscapeFrameBehavior> Programs { get; } = new(
        new Dictionary<SpeedBoosterEscapeProgram, Step[]>
        {
            [SpeedBoosterEscapeProgram.WaitForSpeedBooster] = [Step.Publish(SpeedBoosterEscapeFrameBehavior.WaitForSpeedBooster), Step.Hold],
            [SpeedBoosterEscapeProgram.WaitForSamusLeft] = [Step.Publish(SpeedBoosterEscapeFrameBehavior.WaitForSamusLeft), Step.Hold],
            [SpeedBoosterEscapeProgram.AdvanceLava] = [Step.Publish(SpeedBoosterEscapeFrameBehavior.AdvanceLava), Step.Hold],
        });
}
