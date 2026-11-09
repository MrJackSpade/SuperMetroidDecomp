using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFakeDeathProgram, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainFakeDeathDraw, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectNoCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's fake-death room mutation programs (#1296), in place of their instruction lists at
/// Each draws its layout for one update, then deletes the PLM.
/// </summary>
internal static class MotherBrainFakeDeathObjectProgramDefinitions
{
    /// <summary>The layout each mutation program draws.</summary>
    internal static readonly Dictionary<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw> Layouts = new()
    {
        [MotherBrainFakeDeathProgram.FillWall] = MotherBrainFakeDeathDraw.FillWall,
        [MotherBrainFakeDeathProgram.EscapeDoor] = MotherBrainFakeDeathDraw.EscapeDoor,
        [MotherBrainFakeDeathProgram.MuralRow1] = MotherBrainFakeDeathDraw.MuralRow1,
        [MotherBrainFakeDeathProgram.MuralRow2] = MotherBrainFakeDeathDraw.MuralRow2,
        [MotherBrainFakeDeathProgram.MuralRow3] = MotherBrainFakeDeathDraw.MuralRow3,
        [MotherBrainFakeDeathProgram.MuralRow4] = MotherBrainFakeDeathDraw.MuralRow4,
        [MotherBrainFakeDeathProgram.MuralRow5] = MotherBrainFakeDeathDraw.MuralRow5,
        [MotherBrainFakeDeathProgram.MuralRow6] = MotherBrainFakeDeathDraw.MuralRow6,
        [MotherBrainFakeDeathProgram.MuralRow7] = MotherBrainFakeDeathDraw.MuralRow7,
        [MotherBrainFakeDeathProgram.MuralRow8] = MotherBrainFakeDeathDraw.MuralRow8,
        [MotherBrainFakeDeathProgram.MuralRow9] = MotherBrainFakeDeathDraw.MuralRow9,
        [MotherBrainFakeDeathProgram.MuralRow10] = MotherBrainFakeDeathDraw.MuralRow10,
        [MotherBrainFakeDeathProgram.MuralRow11] = MotherBrainFakeDeathDraw.MuralRow11,
        [MotherBrainFakeDeathProgram.MuralRow12] = MotherBrainFakeDeathDraw.MuralRow12,
        [MotherBrainFakeDeathProgram.ClearCeilingBlock] = MotherBrainFakeDeathDraw.ClearCeilingBlock,
        [MotherBrainFakeDeathProgram.ClearCeilingTube] = MotherBrainFakeDeathDraw.ClearCeilingTube,
        [MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube] = MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube,
        [MotherBrainFakeDeathProgram.ClearBottomMiddleTubes] = MotherBrainFakeDeathDraw.ClearBottomMiddleTubes,
        [MotherBrainFakeDeathProgram.ClearBottomLeftTube] = MotherBrainFakeDeathDraw.ClearBottomLeftTube,
        [MotherBrainFakeDeathProgram.ClearBottomRightTube] = MotherBrainFakeDeathDraw.ClearBottomRightTube,
    };

    /// <summary>Every mutation program.</summary>
    internal static AnimationProgramSet<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw, RoomObjectNoCue> Programs { get; } = new(
        Layouts.ToDictionary(pair => pair.Key, pair => DrawOnce(pair.Value)));

    /// <summary>Draws <paramref name="layout"/> for one update, then deletes the PLM.</summary>
    private static Step[] DrawOnce(MotherBrainFakeDeathDraw layout) => [Step.Show(layout, 1), Step.Stop];
}
