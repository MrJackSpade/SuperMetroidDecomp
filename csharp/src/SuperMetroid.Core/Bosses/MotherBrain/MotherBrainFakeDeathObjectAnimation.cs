using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A Mother Brain fake-death room mutation PLM's typed program.</summary>
internal sealed class MotherBrainFakeDeathObjectAnimation(MotherBrainFakeDeathProgram program)
    : RoomObjectCuelessProgramAnimation<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainFakeDeathProgram, MotherBrainFakeDeathDraw, RoomObjectNoCue> Programs =>
        MotherBrainFakeDeathObjectProgramDefinitions.Programs;

    /// <inheritdoc/>
    private protected override RoomObjectDraw DrawList(MotherBrainFakeDeathDraw frame) => RoomObjectDraw.Of(frame);
}
