namespace SuperMetroid.Core.Game;

/// <summary>
/// Per-slot Kraid actor schedulers. The body owns the encounter phase, but the arm, three
/// belly lints, and foot remain real enemy records with independent visibility, bytecode,
/// timers, positions, and functions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort KraidArmNormalInstruction = KraidInstructionLists.Ilist_89F3;
    private const ushort KraidArmRetractedInstruction = KraidInstructionLists.ArmRetracted;
    private const ushort KraidFootNeutralInstruction = KraidInstructionLists.Ilist_86ED;
    private const ushort KraidFootLungeInstruction = KraidInstructionLists.FootLunge;
    private const ushort KraidFootLungeFinishedInstruction =
        KraidInstructionLists.FootLungeFinished;
    private const ushort KraidFootWalkBackInstruction = KraidInstructionLists.FootWalkBack;
    private const ushort KraidFootWalkBackLoopInstruction =
        KraidInstructionLists.FootWalkBackLoop;

    private void RunKraidArmMain(RoomEnemySlot arm, ushort cameraY)
    {
        KraidEnemyState state = RequireKraidState(arm);
        RoomEnemySlot body = _slots[0];
        arm.YPosition = unchecked((ushort)(body.YPosition - 44));
        arm.XPosition = body.XPosition;

        // `$B7D3-$B7F0` hides only the arm when its anchor leaves the 224-line gameplay
        // viewport. This is actor visibility, not BG priority, so preserve property $0100.
        bool onScreen = unchecked((short)(arm.YPosition - cameraY)) >= 0 &&
            unchecked((short)(arm.YPosition - cameraY - 224)) < 0;
        arm.Properties = onScreen
            ? arm.Properties.Without(EnemyProperties.Invisible)
            : arm.Properties.With(EnemyProperties.Invisible);

        // Mouth reopen attempts are encoded in the high byte. Incrementing the timer here
        // cancels the common interpreter's later decrement and freezes the selected arm map.
        if ((state.MouthFlags & 0xff00) != 0)
            arm.InstructionTimer = unchecked((ushort)(arm.InstructionTimer + 1));
    }

    private void RunKraidLintMain(RoomEnemySlot lint)
    {
        KraidEnemyState state = RequireKraidState(lint);
        lint.InstructionTimer = 0x7fff;
        KraidPartState part = state.Parts[lint.SlotIndex];
        switch ((KraidAiFunction)lint.VariableA)
        {
            case KraidAiFunction.LintInactive:
                return;
            case KraidAiFunction.AlignPartToKraid:
                AlignKraidPart(lint, part);
                return;
            case KraidAiFunction.LintProduce:
                ProduceKraidLint(lint);
                return;
            case KraidAiFunction.LintCharge:
                ChargeKraidLint(lint);
                return;
            case KraidAiFunction.LintFire:
                FireKraidLint(lint, part);
                return;
            default:
                throw new InvalidDataException(
                    $"Kraid lint slot {lint.SlotIndex} function $A7:{lint.VariableA:X4} " +
                    "is not translated.");
        }
    }

    private void AlignKraidPart(RoomEnemySlot partSlot, KraidPartState part)
    {
        partSlot.XPosition = unchecked((ushort)(_slots[0].XPosition - partSlot.XRadius));
        TickKraidFunctionTimer(partSlot, part);
    }

    private void ProduceKraidLint(RoomEnemySlot lint)
    {
        lint.Properties = lint.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        lint.XPosition = unchecked((ushort)(
            lint.VariableC + _slots[0].XPosition - lint.VariableB));
        lint.VariableB = unchecked((ushort)(lint.VariableB + 1));
        if (unchecked((short)(lint.VariableB - 32)) >= 0)
        {
            lint.VariableA = (ushort)KraidAiFunction.LintCharge;
            lint.VariableF = 30;
        }
    }

    private void ChargeKraidLint(RoomEnemySlot lint)
    {
        lint.PaletteIndex = (lint.VariableF & 1) != 0 ? (ushort)3584 : (ushort)0;
        lint.XPosition = unchecked((ushort)(
            lint.VariableC + _slots[0].XPosition - lint.VariableB));
        ushort oldTimer = lint.VariableF;
        lint.VariableF = unchecked((ushort)(lint.VariableF - 1));
        if (oldTimer == 1)
        {
            lint.VariableA = (ushort)KraidAiFunction.LintFire;
            LastKraidSoundEffect = new KraidSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x001f));
        }
    }

    private static void FireKraidLint(RoomEnemySlot lint, KraidPartState part)
    {
        AddSignedKraidHorizontalDisplacement(lint, -0x00038000);
        if (unchecked((short)(lint.XPosition - 56)) < 0)
            lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (unchecked((short)(lint.XPosition - 32)) < 0)
        {
            lint.Properties = lint.Properties.With(EnemyProperties.Invisible);
            lint.VariableA = (ushort)KraidAiFunction.AlignPartToKraid;
            lint.VariableF = 300;
            part.NextFunction = KraidAiFunction.LintProduce;
            lint.VariableB = 0;
        }
    }

    private void RunKraidFootMain(RoomEnemySlot foot, ushort cameraY)
    {
        KraidEnemyState state = RequireKraidState(foot);
        RoomEnemySlot body = _slots[0];
        foot.XPosition = body.XPosition;
        foot.YPosition = unchecked((ushort)(body.YPosition + 100));
        bool onScreen = unchecked((short)(foot.YPosition - cameraY)) >= 0 &&
            unchecked((short)(foot.YPosition - 224 - cameraY)) < 0;
        foot.Properties = onScreen
            ? foot.Properties.Without(EnemyProperties.Invisible)
            : foot.Properties.With(EnemyProperties.Invisible);

        KraidPartState part = state.Parts[5];
        switch ((KraidAiFunction)foot.VariableA)
        {
            case KraidAiFunction.NoOperation:
                return;
            case KraidAiFunction.FootFirstPhaseThinking:
                if (TryBeginKraidGrowth(body, state))
                    return;
                TickKraidFunctionTimer(foot, part);
                return;
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
                PrepareKraidFirstPhaseLunge(body, foot);
                return;
            case KraidAiFunction.FootFirstPhaseLunge:
                RunKraidFirstPhaseLunge(body, foot, part);
                return;
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
                TickKraidFootTransitionTimer(foot, part);
                return;
            case KraidAiFunction.HandleFunctionTimer:
                TickKraidFunctionTimer(foot, part);
                return;
            case KraidAiFunction.FootFirstPhaseRetreat:
                RunKraidFirstPhaseRetreat(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseWalkToStart:
                RunKraidSecondPhaseWalkToStart(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseInitialize:
                SetKraidWalkingRight(foot, part, targetX: 352, thinkTimer: 180);
                return;
            case KraidAiFunction.FootSecondPhaseThinking:
                RunKraidSecondPhaseThinking(body, foot, part);
                return;
            case KraidAiFunction.FootSecondPhaseWalkingRight:
                RunKraidSecondPhaseWalkingRight(body, foot);
                return;
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
                RunKraidSecondPhaseWalkingLeft(body, foot);
                return;
            default:
                throw new InvalidDataException(
                    $"Kraid foot function $A7:{foot.VariableA:X4} is not translated.");
        }
    }

    private void PrepareKraidFirstPhaseLunge(RoomEnemySlot body, RoomEnemySlot foot)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        RoomEnemySlot arm = _slots[1];
        if (unchecked((short)(arm.CurrentInstruction - KraidInstructionLists.ArmListLowerBound)) < 0)
            return;
        arm.CurrentInstruction = KraidArmRetractedInstruction;
        arm.InstructionTimer = 1;
        foot.CurrentInstruction = KraidFootLungeInstruction;
        foot.InstructionTimer = 1;
        foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseLunge;
        foot.VariableF = 0;
    }

    private void RunKraidFirstPhaseLunge(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        if (unchecked((short)(body.XPosition - 92)) < 0)
            body.XPosition = 92;
        if (foot.CurrentInstruction != KraidFootLungeFinishedInstruction)
            return;
        if (body.XPosition == 92)
        {
            part.NextFunction = KraidAiFunction.FootFirstPhaseRetreat;
            foot.VariableA = (ushort)KraidAiFunction.DecrementFunctionTimerAndStartWalk;
            foot.VariableF = 1;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
        }
        else
        {
            foot.CurrentInstruction = KraidFootLungeInstruction;
        }
        foot.InstructionTimer = 1;
    }

    private static void TickKraidFootTransitionTimer(RoomEnemySlot foot, KraidPartState part)
    {
        if (foot.VariableF == 0)
            return;
        ushort oldTimer = foot.VariableF;
        foot.VariableF = unchecked((ushort)(foot.VariableF - 1));
        if (oldTimer == 1)
        {
            foot.VariableA = (ushort)part.NextFunction;
            foot.CurrentInstruction = KraidFootWalkBackInstruction;
            foot.InstructionTimer = 1;
        }
    }

    private void RunKraidFirstPhaseRetreat(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        if (unchecked((short)(body.XPosition - 176)) >= 0)
            body.XPosition = 176;
        if (unchecked((short)(foot.CurrentInstruction - KraidFootWalkBackLoopInstruction)) < 0)
            return;
        if (body.XPosition == 176)
        {
            RoomEnemySlot arm = _slots[1];
            arm.CurrentInstruction = KraidArmNormalInstruction;
            arm.InstructionTimer = 1;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
            foot.InstructionTimer = 1;
            foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseThinking;
            foot.VariableF = 300;
            part.NextFunction = KraidAiFunction.FootPrepareFirstPhaseLunge;
        }
        else
        {
            foot.CurrentInstruction = KraidFootWalkBackInstruction;
            foot.InstructionTimer = 1;
        }
    }

    private void RunKraidSecondPhaseWalkToStart(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (targetX != body.XPosition)
        {
            if (unchecked((short)(targetX - body.XPosition)) >= 0)
                return;
            body.XPosition = targetX;
        }
        if (unchecked((short)(foot.CurrentInstruction - KraidFootWalkBackLoopInstruction)) < 0)
            return;
        foot.VariableA = (ushort)KraidAiFunction.HandleFunctionTimer;
        foot.VariableF = 180;
        part.NextFunction = KraidAiFunction.FootSecondPhaseInitialize;
        foot.CurrentInstruction = KraidFootNeutralInstruction;
        foot.InstructionTimer = 1;
    }

    private void RunKraidSecondPhaseThinking(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        part.NextWord = unchecked((ushort)(part.NextWord - 1));
        if (part.NextWord != 0)
            return;

        int recordOffset = 0;
        for (; recordOffset < 24; recordOffset += 4)
        {
            if (body.XPosition == ReadWord(_bus!, 0xa7ba7d + recordOffset))
                break;
        }
        if (recordOffset >= 24)
            recordOffset = 4;
        ushort randomOffset = unchecked((ushort)(ReadKraidRandomNumber() & 0x001c));
        if (randomOffset >= 16)
            randomOffset = 16;
        ushort choiceTable = ReadWord(_bus!, 0xa7ba7f + recordOffset);
        ushort targetX = ReadWord(_bus!, 0xa70000 | unchecked((ushort)(choiceTable + randomOffset)));
        ushort thinkTimer = ReadWord(
            _bus!, 0xa70000 | unchecked((ushort)(choiceTable + randomOffset + 2)));
        if (unchecked((short)(targetX - body.XPosition)) >= 0)
            SetKraidWalkingRight(foot, part, targetX, thinkTimer);
        else
            SetKraidWalkingLeft(foot, part, targetX, thinkTimer);
    }

    private void SetKraidWalkingRight(
        RoomEnemySlot foot,
        KraidPartState part,
        ushort targetX,
        ushort thinkTimer)
    {
        RequireKraidState(foot).TargetX = targetX;
        part.NextWord = thinkTimer;
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkingRight;
        foot.CurrentInstruction = KraidFootWalkBackInstruction;
        foot.InstructionTimer = 1;
    }

    private void SetKraidWalkingLeft(
        RoomEnemySlot foot,
        KraidPartState part,
        ushort targetX,
        ushort thinkTimer)
    {
        RequireKraidState(foot).TargetX = targetX;
        part.NextWord = thinkTimer;
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkingLeft;
        foot.CurrentInstruction = KraidInstructionLists.Ilist_86F3;
        foot.InstructionTimer = 1;
    }

    private void RunKraidSecondPhaseWalkingRight(RoomEnemySlot body, RoomEnemySlot foot)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (targetX != body.XPosition)
        {
            if (unchecked((short)(targetX - body.XPosition)) >= 0)
                return;
            body.XPosition = targetX;
        }
        if (unchecked((short)(foot.CurrentInstruction - KraidFootWalkBackLoopInstruction)) >= 0)
        {
            foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseThinking;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
            foot.InstructionTimer = 1;
        }
    }

    private void RunKraidSecondPhaseWalkingLeft(RoomEnemySlot body, RoomEnemySlot foot)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (unchecked((short)(targetX - body.XPosition)) < 0)
        {
            if (foot.CurrentInstruction == KraidInstructionLists.Ilist_87BB)
            {
                foot.CurrentInstruction = KraidInstructionLists.Ilist_86F3;
                foot.InstructionTimer = 1;
            }
            return;
        }
        body.XPosition = targetX;
        if (foot.CurrentInstruction == KraidInstructionLists.Ilist_87BB)
        {
            foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseThinking;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
            foot.InstructionTimer = 1;
        }
    }

    private static void AddSignedKraidHorizontalDisplacement(
        RoomEnemySlot slot,
        int displacement)
    {
        uint position = ((uint)slot.XPosition << 16) | slot.XSubposition;
        position = unchecked(position + (uint)displacement);
        slot.XPosition = unchecked((ushort)(position >> 16));
        slot.XSubposition = unchecked((ushort)position);
    }
}
