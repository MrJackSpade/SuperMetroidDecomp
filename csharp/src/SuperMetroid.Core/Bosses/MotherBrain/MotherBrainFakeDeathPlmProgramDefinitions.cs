using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFakeDeathProgram, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFakeDeathDraw, SuperMetroid.Core.Rooms.Plms.PlmNoCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room mutation programs (#1296), in place of their bank-$84 instruction lists at
/// $84:AC05..AC88: each draws its layout for one update, then deletes the PLM.
/// </summary>
internal static class MotherBrainFakeDeathPlmProgramDefinitions
{
    /// <summary>The layout each mutation program draws.</summary>
    internal static readonly Dictionary<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw> Layouts = new()
    {
        [MotherBrainFakeDeathProgram.FillWall] = MotherBrainFakeDeathDraw.FillWall,
        [MotherBrainFakeDeathProgram.EscapeDoor] = MotherBrainFakeDeathDraw.EscapeDoor,
        [MotherBrainFakeDeathProgram.BackgroundRow2] = MotherBrainFakeDeathDraw.BackgroundRow2,
        [MotherBrainFakeDeathProgram.BackgroundRow3] = MotherBrainFakeDeathDraw.BackgroundRow3,
        [MotherBrainFakeDeathProgram.BackgroundRow4] = MotherBrainFakeDeathDraw.BackgroundRow4,
        [MotherBrainFakeDeathProgram.BackgroundRow5] = MotherBrainFakeDeathDraw.BackgroundRow5,
        [MotherBrainFakeDeathProgram.BackgroundRow6] = MotherBrainFakeDeathDraw.BackgroundRow6,
        [MotherBrainFakeDeathProgram.BackgroundRow7] = MotherBrainFakeDeathDraw.BackgroundRow7,
        [MotherBrainFakeDeathProgram.BackgroundRow8] = MotherBrainFakeDeathDraw.BackgroundRow8,
        [MotherBrainFakeDeathProgram.BackgroundRow9] = MotherBrainFakeDeathDraw.BackgroundRow9,
        [MotherBrainFakeDeathProgram.BackgroundRowA] = MotherBrainFakeDeathDraw.BackgroundRowA,
        [MotherBrainFakeDeathProgram.BackgroundRowB] = MotherBrainFakeDeathDraw.BackgroundRowB,
        [MotherBrainFakeDeathProgram.BackgroundRowC] = MotherBrainFakeDeathDraw.BackgroundRowC,
        [MotherBrainFakeDeathProgram.BackgroundRowD] = MotherBrainFakeDeathDraw.BackgroundRowD,
        [MotherBrainFakeDeathProgram.BackgroundRowE] = MotherBrainFakeDeathDraw.BackgroundRowEUnused,
        [MotherBrainFakeDeathProgram.BackgroundRowF] = MotherBrainFakeDeathDraw.BackgroundRowFUnused,
        [MotherBrainFakeDeathProgram.ClearCeilingBlock] = MotherBrainFakeDeathDraw.ClearCeilingBlock,
        [MotherBrainFakeDeathProgram.ClearCeilingTube] = MotherBrainFakeDeathDraw.ClearCeilingTube,
        [MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube] = MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube,
        [MotherBrainFakeDeathProgram.ClearBottomMiddleTubes] = MotherBrainFakeDeathDraw.ClearBottomMiddleTubes,
        [MotherBrainFakeDeathProgram.ClearBottomLeftTube] = MotherBrainFakeDeathDraw.ClearBottomLeftTube,
        [MotherBrainFakeDeathProgram.ClearBottomRightTube] = MotherBrainFakeDeathDraw.ClearBottomRightTube,
    };

    /// <summary>Every mutation program.</summary>
    internal static AnimationProgramSet<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw, PlmNoCue> Programs { get; } = new(
        Layouts.ToDictionary(pair => pair.Key, pair => DrawOnce(pair.Value)));

    /// <summary>Draws <paramref name="layout"/> for one update, then deletes the PLM.</summary>
    private static Step[] DrawOnce(MotherBrainFakeDeathDraw layout) => [Step.Show(layout, 1), Step.Stop];
}
