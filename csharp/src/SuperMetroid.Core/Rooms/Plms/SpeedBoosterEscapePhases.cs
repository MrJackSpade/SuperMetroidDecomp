using static SuperMetroid.Core.Rooms.Plms.SpeedBoosterEscapePlmProgramDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The phase of the Speed Booster escape that follows each phase.</summary>
internal static class SpeedBoosterEscapePhases
{
    /// <summary>The phase a wake past <paramref name="phase"/>'s sleep continues with; the last phase never wakes.</summary>
    internal static SpeedBoosterEscapeProgram? PhaseAfter(SpeedBoosterEscapeProgram phase) => phase switch
    {
        SpeedBoosterEscapeProgram.WaitForSpeedBooster => SpeedBoosterEscapeProgram.WaitForSamusLeft,
        SpeedBoosterEscapeProgram.WaitForSamusLeft => SpeedBoosterEscapeProgram.AdvanceLava,
        SpeedBoosterEscapeProgram.AdvanceLava => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterEscapeProgram)} {phase}."),
    };
}
