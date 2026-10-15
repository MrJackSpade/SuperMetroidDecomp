using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.RoomPlmSharedProgram, SuperMetroid.Core.Rooms.Plms.PlmNoDraw, SuperMetroid.Core.Rooms.Plms.PlmNoCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The shared one-instruction PLM programs (#1296), in place of their bank-$84 lists.</summary>
internal static class RoomPlmSharedProgramDefinitions
{
    /// <summary>Both shared programs.</summary>
    internal static AnimationProgramSet<RoomPlmSharedProgram, PlmNoDraw, PlmNoCue> Programs { get; } = new(
        new Dictionary<RoomPlmSharedProgram, Step[]>
        {
            [RoomPlmSharedProgram.Delete] = [Step.Stop],
            [RoomPlmSharedProgram.Sleep] = [Step.Hold],
        });
}
