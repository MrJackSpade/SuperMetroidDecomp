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
            [P.MaridiaElevatube] = [Step.Show(F.MaridiaElevatube, 16), Step.Publish(new ScriptedRoomObjectCue.QueueSoundLibrary2Maximum6(Sound: Library2Sound.Elevatube)), Step.Stop],
            [P.OldTourianWallWait] = [Step.Publish(new ScriptedRoomObjectCue.InstallFrameBehavior(FrameBehavior: R.OldTourianWallWaitForSamus)), Step.Hold],
            [P.OldTourianWallOpen] = [Step.Publish(new ScriptedRoomObjectCue.Command(K.ClearFrameBehavior)), Step.Show(F.OldTourianWallOpen, 1), Step.Stop],
            [P.EscapeAnimalReaction] = [Step.Publish(new ScriptedRoomObjectCue.QueueSoundLibrary2Maximum6(Sound: Library2Sound.BlockCrumble)), Step.Show(F.EscapeAnimal0, 4), Step.Show(F.EscapeAnimal1, 4), Step.Show(F.EscapeAnimal2, 4), Step.Show(F.EscapeAnimal3, 1), Step.Publish(new ScriptedRoomObjectCue.Command(K.SetAnimalsEscapedEvent)), Step.Stop],
            [P.CrateriaPassage] = [Step.Show(F.CrateriaPassage, 1), Step.Publish(new ScriptedRoomObjectCue.Command(K.MoveRightFourBlocks)), Step.Show(F.CrateriaPassage, 1), Step.Stop],
            [P.MotherBrainEscapeGate] = [Step.Show(F.MotherBrainGateClosed, 6), Step.Stop],
            [P.MotherBrainEscapeGateClosing] = [Step.Show(F.MotherBrainGateOpen, 2), Step.Show(F.MotherBrainGateHalfClosed, 2), Step.Show(F.MotherBrainGateClosed, 2), Step.Stop],
            [P.EnemyBreakableTerrain] = [Step.Publish(new ScriptedRoomObjectCue.QueueSoundLibrary2Maximum3(Sound: Library2Sound.BlockCrumble)), Step.Show(F.SingleBreakup0, 4), Step.Show(F.SingleBreakup1, 4), Step.Show(F.SingleBreakup2, 4), Step.Show(F.SingleBreakup3, 1), Step.Stop],
            [P.ChozoCrumblePlug] = [Step.Show(F.SingleBreakup0, 4), Step.Show(F.SingleBreakup1, 4), Step.Show(F.SingleBreakup2, 4), Step.Show(F.SingleBreakup3, 1), Step.Stop],
            [P.LowerNorfairChozoHand] = [Step.Publish(new ScriptedRoomObjectCue.Command(K.ContinueIfLowerNorfairAcidLowered)), Step.Publish(new ScriptedRoomObjectCue.InstallFrameBehavior(FrameBehavior: R.LowerNorfairChozoHandWait)), Step.Hold],
            [P.LowerNorfairChozoHandLowered] = [Step.Publish(new ScriptedRoomObjectCue.Command(K.SetLoweredAcidHeight)), Step.Show(F.LowerNorfairChozoHand, 1), Step.Stop],
            [P.ChozoClearSlope] = [Step.Show(F.ChozoSlopeCleared, 1), Step.Publish(new ScriptedRoomObjectCue.Command(K.TransformSpikesToSlopes)), Step.Stop],
            [P.ChozoBlockSlope] = [Step.Show(F.ChozoSlopeBlocked, 1), Step.Publish(new ScriptedRoomObjectCue.Command(K.RevertSlopesToSpikes)), Step.Stop],
        });
}
