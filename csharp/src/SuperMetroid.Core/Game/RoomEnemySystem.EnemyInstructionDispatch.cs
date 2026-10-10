using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Routing for the generic enemy instruction interpreter. A negative instruction word is
/// either one of the <see cref="CommonEnemyInstruction"/> commands every bank shares, or a
/// private opcode of the slot's owner. Each owner handler accepts a word only for its own
/// enemy definitions and only from its own closed instruction set, so a word no handler
/// accepts is an untranslated opcode for that owner.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Executes one common enemy instruction. <paramref name="yieldInterpreter"/> reports that
    /// the instruction ended this frame's interpretation (delete, sleep, or wait).
    /// </summary>
    private void ProcessCommonEnemyInstruction(
        RoomEnemySlot slot,
        CommonEnemyInstruction instruction,
        ref ushort cursor,
        out bool yieldInterpreter)
    {
        yieldInterpreter = false;
        switch (instruction)
        {
            case CommonEnemyInstruction.StopScript:
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                yieldInterpreter = true;
                return;
            case CommonEnemyInstruction.Goto:
                cursor = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                return;
            case CommonEnemyInstruction.Sleep:
                slot.CurrentInstruction = cursor;
                yieldInterpreter = true;
                return;
            case CommonEnemyInstruction.EnableOffScreenProcessing:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                cursor = unchecked((ushort)(cursor + 2));
                return;
            case CommonEnemyInstruction.DisableOffScreenProcessing:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                cursor = unchecked((ushort)(cursor + 2));
                return;
            case CommonEnemyInstruction.DecrementTimerAndGoto:
            case CommonEnemyInstruction.DecrementTimerAndGotoDuplicate:
                slot.Timer = unchecked((ushort)(slot.Timer - 1));
                cursor = slot.Timer != 0
                    ? ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)))
                    : unchecked((ushort)(cursor + 4));
                return;
            case CommonEnemyInstruction.SetTimer:
                slot.Timer = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return;
            case CommonEnemyInstruction.WaitFrames:
                slot.InstructionTimer = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                yieldInterpreter = true;
                return;
            case CommonEnemyInstruction.CopyToVram:
                ApplyEnemyInstructionVramTransfer(slot, cursor);

                // The descriptor is seven bytes rather than words. The next command is
                // therefore at opcode+2+7, an odd bank address used intentionally by
                // the Torizo crumbling-statue stream.
                cursor = unchecked((ushort)(cursor + 9));
                return;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(CommonEnemyInstruction)} {(int)instruction:X4}.");
        }
    }

    /// <summary>
    /// Offers a non-common instruction word to the owner handlers. The owner predicates are
    /// disjoint, so at most one handler can accept the word; the order mirrors the original
    /// single-switch interpreter. Returns false when no owner translates the word.
    /// </summary>
    private bool TryProcessOwnedEnemyInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort word,
        ref ushort cursor,
        ushort cameraX,
        ushort cameraY,
        ushort controllerInput,
        out bool yieldInterpreter)
    {
        yieldInterpreter = false;
        if (TryProcessBabyMetroidCutsceneInstruction(slot, word, ref cursor) ||
            TryProcessBoyonInstruction(slot, word, ref cursor) ||
            TryProcessStokeInstruction(slot, word, ref cursor) ||
            TryProcessBabyTurtleInstruction(slot, samus, level, word, ref cursor) ||
            TryProcessMamaTurtleInstruction(slot, word, ref cursor) ||
            TryProcessDragonInstruction(slot, word, ref cursor) ||
            TryProcessKraidArmInstruction(slot, word, ref cursor) ||
            TryProcessKraidFootInstruction(slot, level, word, ref cursor))
        {
            return true;
        }

        if (TryProcessPhantoonPartInstruction(slot, word, ref cursor, out yieldInterpreter))
            return true;

        if (TryProcessMotherBrainInstruction(slot, samus, word, ref cursor) ||
            TryProcessDraygonInstruction(slot, samus, word, ref cursor) ||
            TryProcessWallSpacePirateInstruction(slot, level, word, ref cursor) ||
            TryProcessNinjaSpacePirateInstruction(slot, samus, word, ref cursor) ||
            TryProcessWorkRobotInstruction(slot, samus, level, word, ref cursor, cameraX, cameraY) ||
            TryProcessBeetomInstruction(slot, word, ref cursor) ||
            TryProcessYappingMawInstruction(slot, word, ref cursor) ||
            TryProcessCacatacInstruction(slot, word, ref cursor) ||
            TryProcessOwtchInstruction(slot, word, ref cursor) ||
            TryProcessFuneNamiheInstruction(slot, word, ref cursor) ||
            TryProcessEvirProjectileInstruction(slot, word, ref cursor) ||
            TryProcessYardInstruction(slot, word, ref cursor) ||
            TryProcessHopperInstruction(slot, word, ref cursor) ||
            TryProcessZoaInstruction(slot, word, ref cursor) ||
            TryProcessMetroidInstruction(slot, word, ref cursor) ||
            TryProcessSharedCrawlerInstruction(slot, word, ref cursor) ||
            TryProcessHZoomerInstruction(slot, word, ref cursor) ||
            TryProcessSkreeInstruction(slot, word, ref cursor) ||
            TryProcessWaverInstruction(slot, word, ref cursor) ||
            TryProcessMetareeInstruction(slot, word, ref cursor) ||
            TryProcessSkulteraInstruction(slot, word, ref cursor) ||
            TryProcessPlatformInstruction(slot, word, ref cursor) ||
            TryProcessAlcoonInstruction(slot, level, word, ref cursor) ||
            TryProcessKiHunterInstruction(slot, word, ref cursor) ||
            TryProcessSparkInstruction(slot, word, ref cursor) ||
            TryProcessHibashiInstruction(slot, word, ref cursor) ||
            TryProcessFakeKraidInstruction(slot, samus, level, word, ref cursor, cameraX, cameraY) ||
            TryProcessWalkingSpacePirateInstruction(slot, samus, word, ref cursor) ||
            TryProcessRidleyInstruction(slot, samus, word, ref cursor) ||
            TryProcessCeresSteamInstruction(slot, word, ref cursor) ||
            TryProcessCeresDoorInstruction(slot, samus, word, ref cursor) ||
            TryProcessDeadSidehopperInstruction(slot, word, ref cursor) ||
            TryProcessShitroidInstruction(slot, word, ref cursor) ||
            TryProcessSporeSpawnInstruction(slot, word, ref cursor) ||
            TryProcessCrocomireInstruction(slot, samus, level, word, ref cursor, cameraX) ||
            TryProcessRinkaInstruction(slot, word, ref cursor) ||
            TryProcessRioInstruction(slot, word, ref cursor) ||
            TryProcessNorfairLavaJumpingEnemyInstruction(slot, word, ref cursor) ||
            TryProcessNorfairRioInstruction(slot, word, ref cursor) ||
            TryProcessLowerNorfairRioInstruction(slot, word, ref cursor) ||
            TryProcessMaridiaLargeSnailInstruction(slot, word, ref cursor) ||
            TryProcessMagdolliteInstruction(slot, word, ref cursor, cameraX, cameraY) ||
            TryProcessBotwoonInstruction(slot, word, ref cursor) ||
            TryProcessShaktoolInstruction(slot, word, ref cursor) ||
            TryProcessChozoStatueInstruction(slot, samus, level, word, ref cursor) ||
            TryProcessEscapeAnimalInstruction(slot, samus, word, ref cursor))
        {
            return true;
        }

        return TryProcessBombTorizoInstruction(
            slot,
            samus,
            level,
            word,
            ref cursor,
            controllerInput,
            unchecked((byte)_enemyFrameNmiFrameCounter),
            out yieldInterpreter);
    }
}
