namespace SuperMetroid.Core.Game;

/// <summary>
/// Per-slot Kraid actor schedulers. The body owns the encounter phase, but the arm, three
/// belly lints, and foot remain real enemy records with independent visibility, bytecode,
/// timers, positions, and functions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Arm instruction-list address used for the normal phase between lunges.</summary>
    private const ushort KraidArmNormalInstruction =
        KraidArmInstructionProgramDefinitions.Normal;
    /// <summary>Arm instruction-list address for the retracted pose during lunge preparation.</summary>
    private const ushort KraidArmRetractedInstruction =
        KraidArmInstructionProgramDefinitions.DyingOrPreparingToLunge;
    /// <summary>Foot instruction-list address for the neutral pose between movement sequences.</summary>
    private const ushort KraidFootNeutralInstruction =
        KraidFootInstructionProgramDefinitions.Neutral;
    /// <summary>Foot instruction-list address that performs the first-phase forward lunge.</summary>
    private const ushort KraidFootLungeInstruction =
        KraidFootInstructionProgramDefinitions.LungeForward;
    /// <summary>Terminal foot instruction-list address signaling that the forward lunge animation finished.</summary>
    private const ushort KraidFootLungeFinishedInstruction =
        KraidFootInstructionProgramDefinitions.LungeForwardFinished;
    /// <summary>Foot instruction-list address that walks Kraid back toward the starting edge.</summary>
    private const ushort KraidFootWalkBackInstruction =
        KraidFootInstructionProgramDefinitions.WalkingBackward;
    /// <summary>Looping foot instruction-list address used to detect completion of a walk-back cycle.</summary>
    private const ushort KraidFootWalkBackLoopInstruction =
        KraidFootInstructionProgramDefinitions.WalkingBackwardLoop;

    /// <summary>Tracks the arm against the body anchor, culls it by viewport position, and holds its instruction timer while the mouth is closed.</summary>
    /// <param name="arm">Physical arm enemy slot.</param>
    /// <param name="cameraY">Current vertical camera origin used for viewport visibility.</param>
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

    /// <summary>Dispatches one belly lint's native production, charge, firing, or body-alignment phase.</summary>
    /// <param name="lint">Physical lint enemy slot whose function word selects the phase.</param>
    /// <param name="samus">Active actor for contact resolution and support carry; absent when no Samus state is available.</param>
    private void RunKraidLintMain(RoomEnemySlot lint, SamusState? samus)
    {
        KraidEnemyState state = RequireKraidState(lint);
        if (samus is not null)
            ResolveKraidLintContact(lint, samus);
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
                FireKraidLint(lint, part, samus);
                return;
            default:
                throw new InvalidDataException(
                    $"Kraid lint slot {lint.SlotIndex} function $A7:{lint.VariableA:X4} " +
                    "is not translated.");
        }
    }

    /// <summary>Returns an attached part to its body's horizontal anchor and advances its function timer.</summary>
    /// <param name="partSlot">Physical arm or lint slot to align.</param>
    /// <param name="part">Per-part timing and next-function state.</param>
    private void AlignKraidPart(RoomEnemySlot partSlot, KraidPartState part)
    {
        partSlot.XPosition = unchecked((ushort)(_slots[0].XPosition - partSlot.XRadius));
        TickKraidFunctionTimer(partSlot, part);
    }

    /// <summary>Moves a newly produced lint out from the body while counting its release distance.</summary>
    /// <param name="lint">Lint slot in the production phase.</param>
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

    /// <summary>Flashes a lint while its charge timer counts down, then selects firing and requests its sound.</summary>
    /// <param name="lint">Lint slot in the charge phase.</param>
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

    /// <summary>Moves a fired lint left, hides and recycles it at the body, and applies native support carry to Samus.</summary>
    /// <param name="lint">Lint slot in the firing phase.</param>
    /// <param name="part">Part state receiving the next production function.</param>
    /// <param name="samus">Active Samus actor to receive support displacement when standing on the lint.</param>
    private static void FireKraidLint(RoomEnemySlot lint, KraidPartState part, SamusState? samus)
    {
        AddSignedKraidHorizontalDisplacement(lint, KraidPlatformMovement.FiringDisplacement);
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

        // Native checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement dispatcher, rather than teleporting
        // her here. The clamp changes only the whole word; retain the fractional borrow.
        if (samus is not null && IsEnemyTouchingSamusFromBelow(lint, samus))
        {
            uint carry = ((uint)samus.Kinematics.ExtraXDisplacement << 16) |
                samus.Kinematics.ExtraXSubdisplacement;
            carry = unchecked(carry + (uint)KraidPlatformMovement.FiringDisplacement);
            short whole = unchecked((short)(carry >> 16));
            samus.Kinematics.ExtraXDisplacement = unchecked((ushort)(
                unchecked((short)(whole - KraidPlatformMovement.MinimumCarryWholePixels)) < 0
                    ? KraidPlatformMovement.MinimumCarryWholePixels : whole));
            samus.Kinematics.ExtraXSubdisplacement = unchecked((ushort)carry);
        }
    }

    /// <summary>Positions and culls the physical foot actor, then dispatches its current encounter-phase behavior.</summary>
    /// <param name="foot">Physical foot enemy slot.</param>
    /// <param name="cameraY">Current vertical camera origin used for viewport visibility.</param>
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

    /// <summary>Waits for the arm pause to finish, retracts the arm, and starts the first-phase foot lunge.</summary>
    /// <param name="body">Encounter body, checked for a pending growth transition.</param>
    /// <param name="foot">Foot slot whose lunge instruction and function are installed.</param>
    private void PrepareKraidFirstPhaseLunge(RoomEnemySlot body, RoomEnemySlot foot)
    {
        if (TryBeginKraidGrowth(body, RequireKraidState(body)))
            return;
        RoomEnemySlot arm = _slots[1];
        if (unchecked((short)(arm.CurrentInstruction -
            KraidArmInstructionProgramDefinitions.NormalPause)) < 0)
            return;
        arm.CurrentInstruction = KraidArmRetractedInstruction;
        arm.InstructionTimer = 1;
        foot.CurrentInstruction = KraidFootLungeInstruction;
        foot.InstructionTimer = 1;
        foot.VariableA = (ushort)KraidAiFunction.FootFirstPhaseLunge;
        foot.VariableF = 0;
    }

    /// <summary>Runs the first-phase lunge until its instruction sequence completes at the left boundary.</summary>
    /// <param name="body">Encounter body whose horizontal position is clamped during the lunge.</param>
    /// <param name="foot">Foot slot whose animation indicates lunge completion.</param>
    /// <param name="part">Foot timing state used to schedule the retreat transition.</param>
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

    /// <summary>Counts down the foot's one-step transition delay before installing the next phase function and walk-back list.</summary>
    /// <param name="foot">Foot slot carrying the transition timer and current function.</param>
    /// <param name="part">Part state containing the function to enter after the delay.</param>
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

    /// <summary>Moves the body back to its right boundary, then restores the arm and schedules another first-phase lunge.</summary>
    /// <param name="body">Encounter body whose horizontal position is clamped during retreat.</param>
    /// <param name="foot">Foot slot whose backward-walk loop marks retreat completion.</param>
    /// <param name="part">Foot state receiving the next lunge-preparation function.</param>
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

    /// <summary>Walks the foot back until the body reaches the stored second-phase starting coordinate.</summary>
    /// <param name="body">Encounter body carrying the target coordinate.</param>
    /// <param name="foot">Foot slot whose walk-back loop signals arrival.</param>
    /// <param name="part">Foot state receiving the initial second-phase think interval.</param>
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

    /// <summary>Counts down the second-phase think interval and chooses a new horizontal destination using cartridge RNG.</summary>
    /// <param name="body">Encounter body used to choose a direction and movement target.</param>
    /// <param name="foot">Foot slot switched to the selected walking instruction list.</param>
    /// <param name="part">Foot state carrying the think countdown and destination transition.</param>
    private void RunKraidSecondPhaseThinking(
        RoomEnemySlot body,
        RoomEnemySlot foot,
        KraidPartState part)
    {
        part.NextWord = unchecked((ushort)(part.NextWord - 1));
        if (part.NextWord != 0)
            return;

        (ushort targetX, ushort thinkTimer) = KraidMovementChoices.Select(body.XPosition, ReadKraidRandomNumber());
        if (unchecked((short)(targetX - body.XPosition)) >= 0)
            SetKraidWalkingRight(foot, part, targetX, thinkTimer);
        else
            SetKraidWalkingLeft(foot, part, targetX, thinkTimer);
    }

    /// <summary>Stores a rightward destination and starts the backward-walking animation used for that movement.</summary>
    /// <param name="foot">Foot slot receiving the walking function and instruction list.</param>
    /// <param name="part">Foot state receiving the subsequent think interval.</param>
    /// <param name="targetX">Body coordinate where the walk ends.</param>
    /// <param name="thinkTimer">Delay before the next destination choice.</param>
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

    /// <summary>Stores a leftward destination and starts the forward-walking animation used for that movement.</summary>
    /// <param name="foot">Foot slot receiving the walking function and instruction list.</param>
    /// <param name="part">Foot state receiving the subsequent think interval.</param>
    /// <param name="targetX">Body coordinate where the walk ends.</param>
    /// <param name="thinkTimer">Delay before the next destination choice.</param>
    private void SetKraidWalkingLeft(
        RoomEnemySlot foot,
        KraidPartState part,
        ushort targetX,
        ushort thinkTimer)
    {
        RequireKraidState(foot).TargetX = targetX;
        part.NextWord = thinkTimer;
        foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseWalkingLeft;
        foot.CurrentInstruction = KraidFootInstructionProgramDefinitions.WalkingForward;
        foot.InstructionTimer = 1;
    }

    /// <summary>Moves toward a rightward destination and returns to thinking after the backward-walk loop completes.</summary>
    /// <param name="body">Encounter body whose position follows the selected target.</param>
    /// <param name="foot">Foot slot whose instruction loop signals movement completion.</param>
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

    /// <summary>Moves toward a leftward destination and returns to thinking when the forward-walk animation completes.</summary>
    /// <param name="body">Encounter body whose position follows the selected target.</param>
    /// <param name="foot">Foot slot whose instruction sequence signals movement completion.</param>
    private void RunKraidSecondPhaseWalkingLeft(RoomEnemySlot body, RoomEnemySlot foot)
    {
        ushort targetX = RequireKraidState(body).TargetX;
        if (unchecked((short)(targetX - body.XPosition)) < 0)
        {
            if (foot.CurrentInstruction ==
                KraidFootInstructionProgramDefinitions.WalkingForwardFinished)
            {
                foot.CurrentInstruction =
                    KraidFootInstructionProgramDefinitions.WalkingForward;
                foot.InstructionTimer = 1;
            }
            return;
        }
        body.XPosition = targetX;
        if (foot.CurrentInstruction ==
            KraidFootInstructionProgramDefinitions.WalkingForwardFinished)
        {
            foot.VariableA = (ushort)KraidAiFunction.FootSecondPhaseThinking;
            foot.CurrentInstruction = KraidFootNeutralInstruction;
            foot.InstructionTimer = 1;
        }
    }

    /// <summary>Adds a signed fixed-point horizontal displacement to an enemy's whole and fractional X position.</summary>
    /// <param name="slot">Enemy actor whose X position is updated.</param>
    /// <param name="displacement">Signed 16.16 horizontal displacement.</param>
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
