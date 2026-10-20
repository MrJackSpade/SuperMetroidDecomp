using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmFrame;
using K = SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmCueKind;
using P = SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmProgram;
using R = SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmPreInstruction;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmProgram, SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmFrame, SuperMetroid.Core.Rooms.Plms.ScriptedRoomPlmCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The scripted room PLMs' programs (#1296), step for step their bank-$84 lists.</summary>
internal static class ScriptedRoomPlmProgramDefinitions
{
    /// <summary>Every scripted room program.</summary>
    internal static AnimationProgramSet<P, F, ScriptedRoomPlmCue> Programs { get; } = new(
        new Dictionary<P, Step[]>
        {
            [P.ElevatorPlatform] = [Step.Show(F.ElevatorPlatform0, 4), Step.Show(F.ElevatorPlatform1, 4), Step.Show(F.ElevatorPlatform2, 4), Step.Show(F.ElevatorPlatform1, 4), Step.GoTo(P.ElevatorPlatform)],
            [P.ShaktoolRoomController] = [Step.Publish(new ScriptedRoomPlmCue.InstallPreInstruction(PreInstruction: R.ShaktoolRoomController)), Step.Hold],
            [P.MaridiaElevatube] = [Step.Show(F.MaridiaElevatube, 16), Step.Publish(new ScriptedRoomPlmCue.QueueSoundLibrary2Maximum6(SoundId: 0x15)), Step.Stop],
            [P.OldTourianWallWait] = [Step.Publish(new ScriptedRoomPlmCue.InstallPreInstruction(PreInstruction: R.OldTourianWallWaitForSamus)), Step.Hold],
            [P.OldTourianWallOpen] = [Step.Publish(new ScriptedRoomPlmCue.Command(K.ClearPreInstruction)), Step.Show(F.OldTourianWallOpen, 1), Step.Stop],
            [P.EscapeAnimalReaction] = [Step.Publish(new ScriptedRoomPlmCue.QueueSoundLibrary2Maximum6(SoundId: 0x0a)), Step.Show(F.EscapeAnimal0, 4), Step.Show(F.EscapeAnimal1, 4), Step.Show(F.EscapeAnimal2, 4), Step.Show(F.EscapeAnimal3, 1), Step.Publish(new ScriptedRoomPlmCue.Command(K.SetAnimalsEscapedEvent)), Step.Stop],
            [P.CrateriaPassage] = [Step.Show(F.CrateriaPassage, 1), Step.Publish(new ScriptedRoomPlmCue.Command(K.MoveRightFourBlocks)), Step.Show(F.CrateriaPassage, 1), Step.Stop],
            [P.MotherBrainEscapeGate] = [Step.Show(F.MotherBrainGateClosed, 6), Step.Stop],
            [P.MotherBrainEscapeGateClosing] = [Step.Show(F.MotherBrainGateOpen, 2), Step.Show(F.MotherBrainGateHalfClosed, 2), Step.Show(F.MotherBrainGateClosed, 2), Step.Stop],
            [P.EnemyBreakableTerrain] = [Step.Publish(new ScriptedRoomPlmCue.QueueSoundLibrary2Maximum3(SoundId: 0x0a)), Step.Show(F.SingleBreakup0, 4), Step.Show(F.SingleBreakup1, 4), Step.Show(F.SingleBreakup2, 4), Step.Show(F.SingleBreakup3, 1), Step.Stop],
            [P.ChozoCrumblePlug] = [Step.Show(F.SingleBreakup0, 4), Step.Show(F.SingleBreakup1, 4), Step.Show(F.SingleBreakup2, 4), Step.Show(F.SingleBreakup3, 1), Step.Stop],
            [P.LowerNorfairChozoHand] = [Step.Publish(new ScriptedRoomPlmCue.Command(K.ContinueIfLowerNorfairAcidLowered)), Step.Publish(new ScriptedRoomPlmCue.InstallPreInstruction(PreInstruction: R.LowerNorfairChozoHandWait)), Step.Hold],
            [P.LowerNorfairChozoHandLowered] = [Step.Publish(new ScriptedRoomPlmCue.Command(K.SetLoweredAcidHeight)), Step.Show(F.LowerNorfairChozoHand, 1), Step.Stop],
            [P.ChozoClearSlope] = [Step.Show(F.ChozoSlopeCleared, 1), Step.Publish(new ScriptedRoomPlmCue.Command(K.TransformSpikesToSlopes)), Step.Stop],
            [P.ChozoBlockSlope] = [Step.Show(F.ChozoSlopeBlocked, 1), Step.Publish(new ScriptedRoomPlmCue.Command(K.RevertSlopesToSpikes)), Step.Stop],
        });
}
