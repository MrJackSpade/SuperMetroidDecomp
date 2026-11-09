using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectFrame;
using K = SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectProgram;
using R = SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectFrameBehavior;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectProgram, SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectFrame, SuperMetroid.Core.Rooms.RoomObjects.ScriptedRoomObjectCue>;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The scripted room PLMs' programs (#1296), step for step their lists.</summary>
internal static class ScriptedRoomObjectProgramDefinitions
{
    /// <summary>Every scripted room program.</summary>
    internal static AnimationProgramSet<P, F, ScriptedRoomObjectCue> Programs { get; } = new(
        new Dictionary<P, Step[]>
        {
            [P.ElevatorPlatform] = [Step.Show(F.ElevatorPlatform0, 4), Step.Show(F.ElevatorPlatform1, 4), Step.Show(F.ElevatorPlatform2, 4), Step.Show(F.ElevatorPlatform1, 4), Step.GoTo(P.ElevatorPlatform)],
            [P.ShaktoolRoomController] = [Step.Publish(new ScriptedRoomObjectCue.InstallFrameBehavior(FrameBehavior: R.ShaktoolRoomController)), Step.Hold],
            [P.MotherBrainEscapeGate] = [Step.Show(F.MotherBrainGateClosed, 6), Step.Stop],
            [P.MotherBrainEscapeGateClosing] = [Step.Show(F.MotherBrainGateOpen, 2), Step.Show(F.MotherBrainGateHalfClosed, 2), Step.Show(F.MotherBrainGateClosed, 2), Step.Stop],
        });
}
