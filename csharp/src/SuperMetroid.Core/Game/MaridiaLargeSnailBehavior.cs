using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Maridia large snail species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MaridiaLargeSnailBehavior : IEnemySpecies
{
    /// <summary>Resolves floor, ledge, and wall contact during bounce and roll phases.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Runs common damaging touch behavior for selected shell hitboxes.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds terrain and contact owners for Oum's bounce and push behavior.</summary>
    internal MaridiaLargeSnailBehavior(EnemyTerrain terrain, EnemyCollision collision)
    {
        _terrain = terrain;
        _collision = collision;
    }

    /// <summary>Strict horizontal range in which Oum can push or carry Samus.</summary>
    private const ushort MaridiaLargeSnailPushingXDistance = 0x0018;
    /// <summary>Strict vertical range required for Samus to be in Oum's pushing band.</summary>
    private const ushort MaridiaLargeSnailPushingYDistance = 0x0020;
    /// <summary>Strict horizontal range used when a completed cooldown permits attack.</summary>
    private const ushort MaridiaLargeSnailAttackXDistance = 0x0020;
    /// <summary>Frames initially held before Oum may rotate into an attack.</summary>
    private const ushort MaridiaLargeSnailInitialAttackCooldown = 0x0080;
    /// <summary>Number of impacts in Oum's landing or ledge-fall settling sequence.</summary>
    private const ushort MaridiaLargeSnailInitialBounceCount = 3;
    /// <summary>Per-frame 8.8 index change in the quadratic vertical speed table.</summary>
    private const ushort MaridiaLargeSnailBounceAcceleration = 0x0180;
    /// <summary>Maximum falling speed-table index before the next impact.</summary>
    private const ushort MaridiaLargeSnailMaximumFallIndex = 0x4000;
    /// <summary>Speed-table index removed at each floor impact.</summary>
    private const ushort MaridiaLargeSnailImpactIndexLoss = 0x1000;
    /// <summary>Library-two splash sound emitted at the water collision.</summary>
    private const ushort MaridiaLargeSnailSplashSound = 0x000e;
    /// <summary>Library-two sound from the private tail of Oum shot handling.</summary>
    private const ushort MaridiaLargeSnailShotSound = 0x0057;

    /// <summary>Oum state indexed by physical enemy slot during the current room load.</summary>
    internal readonly MaridiaLargeSnailEnemyState?[] _maridiaLargeSnailStates =
        new MaridiaLargeSnailEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>
    /// Most recent library-two sound requested by Oum this frame. The outer audio mixer is
    /// still a frontend seam, so the enemy publishes the exact native sound identifier.
    /// </summary>
    public ushort? LastMaridiaLargeSnailSoundEffect { get; private set; }

    /// <summary>Clears physical-slot extensions and the frame's pending Oum sound.</summary>
    internal void ResetMaridiaLargeSnailRoomState()
    {
        Array.Clear(_maridiaLargeSnailStates);
        LastMaridiaLargeSnailSoundEffect = null;
    }

    /// <summary>Ports <c>InitAI_Oum</c> at $A2:CCD4.</summary>
    internal void InitializeMaridiaLargeSnail(RoomEnemySlot slot)
    {
        var state = new MaridiaLargeSnailEnemyState(slot)
        {
            RequestedInstructionListIndex = 0,
            InstalledInstructionListIndex = 0,
            AttackAnimationFinished = false,
            AttackAllowsRotation = false,
            SamusInPushingRange = false,
            SamusIsLeft = false,
            PreviousPosition = Position16<RoomPixels>.Zero,
            UnusedExtra0A = 0,
            StopMovementNextFrame = false,
            Function = MaridiaLargeSnailEnemyFunction.Idle,
            YSpeedTableIndex = 0,
            AttackCooldown = MaridiaLargeSnailInitialAttackCooldown,
            MovingLeft = false,
            RemainingBounces = MaridiaLargeSnailInitialBounceCount,
            BounceFunction = MaridiaLargeSnailBounceFunction.Falling,
        };
        _maridiaLargeSnailStates[slot.SlotIndex] = state;

        // The initial left-facing list intentionally disagrees with installed selector zero,
        // whose table entry is right-facing idle. Because SetOumInstList suppresses equal
        // selectors, this oddity is observable until Samus first selects a different index.
        slot.CurrentInstruction = MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle;
    }

    /// <summary>
    /// Ports <c>MainAI_Oum</c> and functions $CD23-$CFFF. Ordering is exact: proximity and
    /// previous positions, behavior, Samus carrying, then the input-derived stop flag.
    /// </summary>
    private void RunMaridiaLargeSnailMain(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Maridia Large Snail AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Maridia Large Snail AI requires room level data.");

        UpdateMaridiaLargeSnailSamusProximity(slot, state, samus);
        state.PreviousPosition = slot.Position.Whole;

        switch (state.Function)
        {
            case MaridiaLargeSnailEnemyFunction.Idle:
                RunMaridiaLargeSnailIdle(slot, state, samus, level);
                break;
            case MaridiaLargeSnailEnemyFunction.Rolling:
                RunMaridiaLargeSnailRolling(slot, state, samus, level);
                break;
            case MaridiaLargeSnailEnemyFunction.Attacking:
                RunMaridiaLargeSnailAttack(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail function $A2:{(ushort)state.Function:X4} is not translated.");
        }

        CarrySamusWithMaridiaLargeSnail(slot, state, samus);
        UpdateMaridiaLargeSnailInputStop(slot, state, controllerInput);
    }

    /// <summary>Completes landing bounces, faces Samus, then starts rolling within the pushing range.</summary>
    private void RunMaridiaLargeSnailIdle(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
            return;
        }

        state.RequestedInstructionListIndex = 0;
        if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
        {
            state.MovingLeft = true;
            state.RequestedInstructionListIndex = 1;
        }
        InstallMaridiaLargeSnailInstructionList(slot, state);

        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                slot.Position.X.Whole,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.RequestedInstructionListIndex = unchecked((ushort)(
            state.RequestedInstructionListIndex | 2));
        InstallMaridiaLargeSnailInstructionList(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    /// <summary>Handles ledges, cooldown and attack selection before horizontal terrain movement.</summary>
    private void RunMaridiaLargeSnailRolling(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.RemainingBounces != 0)
        {
            RunMaridiaLargeSnailBounce(slot, state, level);
        }
        else if (!_terrain.MoveEnemyVertically(level, slot, 1 << 16))
        {
            // No floor one pixel below means Oum walked off a ledge. Retail restarts the
            // complete three-impact settling sequence without skipping horizontal work.
            state.YSpeedTableIndex = 0;
            state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
            state.RemainingBounces = MaridiaLargeSnailInitialBounceCount;
        }

        // StopMovementNextFrame was produced at the end of the preceding main-AI call. The
        // current frame clears/recomputes it only after this behavior returns.
        if (state.StopMovementNextFrame)
            return;

        state.AttackCooldown = unchecked((ushort)(state.AttackCooldown - 1));
        bool cooldownExpired = unchecked((short)state.AttackCooldown) < 0;
        if (cooldownExpired)
        {
            state.AttackCooldown = 0;
            bool mayAttack = EnemyMath.IsWithinStrictModularDistance(
                    samus.Position.X.Whole,
                    slot.Position.X.Whole,
                    MaridiaLargeSnailAttackXDistance) &&
                state.AttackAllowsRotation &&
                state.RemainingBounces == 0;
            if (mayAttack)
            {
                state.RequestedInstructionListIndex = 0;
                state.MovingLeft = false;
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                {
                    state.RequestedInstructionListIndex = 1;
                    state.MovingLeft = true;
                }

                state.AttackCooldown = MaridiaLargeSnailInitialAttackCooldown;
                state.RequestedInstructionListIndex = unchecked((ushort)(
                    (state.RequestedInstructionListIndex & 1) | 4));
                InstallMaridiaLargeSnailInstructionList(slot, state);
                state.Function = MaridiaLargeSnailEnemyFunction.Attacking;
                return;
            }
        }

        MoveMaridiaLargeSnailHorizontally(slot, state, level);
    }

    /// <summary>Resumes the rolling list after the attack animation marks itself complete.</summary>
    private static void RunMaridiaLargeSnailAttack(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (!state.AttackAnimationFinished)
            return;

        state.AttackAnimationFinished = false;
        state.RequestedInstructionListIndex = unchecked((ushort)(
            state.RequestedInstructionListIndex - 2));
        InstallMaridiaLargeSnailInstructionList(slot, state);
        state.Function = MaridiaLargeSnailEnemyFunction.Rolling;
    }

    /// <summary>Alternates quadratic-speed fall and rise phases, reducing the index after each impact.</summary>
    private void RunMaridiaLargeSnailBounce(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        switch (state.BounceFunction)
        {
            case MaridiaLargeSnailBounceFunction.Falling:
                state.YSpeedTableIndex = unchecked((ushort)(
                    state.YSpeedTableIndex + MaridiaLargeSnailBounceAcceleration));
                if (unchecked((short)(
                        state.YSpeedTableIndex - MaridiaLargeSnailMaximumFallIndex)) >= 0)
                {
                    state.YSpeedTableIndex = MaridiaLargeSnailMaximumFallIndex;
                }

                if (!_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ReadQuadraticEnemySpeed(
                            unchecked((ushort)(SnesEightEight.WholeOf(state.YSpeedTableIndex))),
                            negative: false)))
                {
                    return;
                }

                state.RemainingBounces = unchecked((ushort)(state.RemainingBounces - 1));
                state.YSpeedTableIndex = unchecked((ushort)(
                    state.YSpeedTableIndex - MaridiaLargeSnailImpactIndexLoss));
                if (unchecked((short)state.YSpeedTableIndex) < 0)
                    state.RemainingBounces = 0;
                state.BounceFunction = MaridiaLargeSnailBounceFunction.Rising;
                return;

            case MaridiaLargeSnailBounceFunction.Rising:
                state.YSpeedTableIndex = unchecked((ushort)(
                    state.YSpeedTableIndex - MaridiaLargeSnailBounceAcceleration));
                if (unchecked((short)state.YSpeedTableIndex) < 0)
                {
                    state.YSpeedTableIndex = 0;
                    state.BounceFunction = MaridiaLargeSnailBounceFunction.Falling;
                    return;
                }

                // The rising half reads the negative member of the same quadratic row. The
                // native AND $7F00 is retained even though valid bounce indexes stay below
                // $4000; it documents the exact table-index formation seen in a debugger.
                ushort tableIndex = unchecked((ushort)(
                    SnesEightEight.WholeOf(state.YSpeedTableIndex & 0x7f00)));
                _terrain.MoveEnemyVertically(
                    level,
                    slot,
                    EnemyMath.ReadQuadraticEnemySpeed(tableIndex, negative: true));
                return;

            default:
                throw new InvalidDataException(
                    $"Maridia Large Snail bounce function " +
                    $"$A2:{(ushort)state.BounceFunction:X4} is not translated.");
        }
    }

    /// <summary>Temporarily extends the collision probe toward Samus, then retracts it after wall resolution.</summary>
    private void MoveMaridiaLargeSnailHorizontally(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        RoomLevelData level)
    {
        ushort byteOffset = state.MovingLeft ? (ushort)132 : (ushort)128;
        (short whole, ushort fraction) = EnemyMath.ReadLinearEnemySpeed(byteOffset);
        bool temporarilyReachTowardSamus = state.SamusInPushingRange &&
            state.MovingLeft == state.SamusIsLeft;
        if (temporarilyReachTowardSamus)
        {
            // Native temporarily extends the whole-pixel move sixteen pixels toward Samus,
            // performs collision, then retracts the center. Net velocity remains the table
            // speed while wall clipping naturally transfers the lost distance to Samus.
            whole = unchecked((short)(whole + (state.MovingLeft ? -16 : 16)));
        }

        int displacement = unchecked((SnesSixteenSixteen.FromWhole(whole)) | fraction);
        bool collided = _terrain.MoveEnemyHorizontallyTreatingSlopesAsWalls(
            level,
            slot,
            displacement);

        if (temporarilyReachTowardSamus)
        {
            slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
                slot.Position.X.Whole + (state.MovingLeft ? 16 : -16))));
        }

        if (!collided)
            return;

        state.MovingLeft = !state.MovingLeft;
        state.RequestedInstructionListIndex = unchecked((ushort)(
            state.RequestedInstructionListIndex ^ 4));
        InstallMaridiaLargeSnailInstructionList(slot, state);
    }

    /// <summary>Captures whether Samus is within both push ranges and on which side.</summary>
    private static void UpdateMaridiaLargeSnailSamusProximity(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.SamusInPushingRange = false;
        state.SamusIsLeft = false;
        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.Y.Whole,
                slot.Position.Y.Whole,
                MaridiaLargeSnailPushingYDistance) ||
            !EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                slot.Position.X.Whole,
                MaridiaLargeSnailPushingXDistance))
        {
            return;
        }

        state.SamusInPushingRange = true;
        state.SamusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
    }

    /// <summary>Publishes Oum's vertical platform motion and sideward push into Samus's extra displacement.</summary>
    private static void CarrySamusWithMaridiaLargeSnail(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        SamusState samus)
    {
        state.StopMovementNextFrame = false;
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
        {
            // Unlike most moving-platform producers, Oum assigns—not adds—the vertical
            // displacement word. This preserves the last touching Oum's native ownership.
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(unchecked((short)(
                slot.Position.Y.Whole - state.PreviousPosition.Y)));
        }

        if (!state.SamusInPushingRange)
            return;

        ushort deltaX = unchecked((ushort)(slot.Position.X.Whole - state.PreviousPosition.X));
        bool movedLeft = unchecked((short)deltaX) < 0;
        if (movedLeft != state.SamusIsLeft)
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(deltaX)), 0);
    }

    /// <summary>Records an opposing controller direction to stop the following frame's roll.</summary>
    private static void UpdateMaridiaLargeSnailInputStop(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state,
        ushort controllerInput)
    {
        if (!state.SamusInPushingRange)
            return;

        short deltaX = unchecked((short)(slot.Position.X.Whole - state.PreviousPosition.X));
        bool stop;
        if (deltaX == 0)
        {
            stop = state.SamusIsLeft
                ? state.MovingLeft &&
                    (controllerInput & (ushort)SnesButton.Right) != 0
                : !state.MovingLeft &&
                    (controllerInput & (ushort)SnesButton.Left) != 0;
        }
        else if (deltaX < 0)
        {
            stop = (controllerInput & (ushort)SnesButton.Right) != 0;
        }
        else
        {
            stop = (controllerInput & (ushort)SnesButton.Left) != 0;
        }

        if (!stop)
            return;

        state.StopMovementNextFrame = true;
        slot.Position = slot.Position.WithWholeX(state.PreviousPosition.X);
    }

    /// <summary>
    /// Exact $A0:ABE7 asymmetric "touching from below" predicate used for Oum carrying and
    /// its post-contact shove. It is intentionally not interchangeable with radius overlap.
    /// </summary>
    private static bool MaridiaLargeSnailTouchesSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        ushort xDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            samus.Position.X.Whole - slot.Position.X.Whole)));
        bool centerWithinSamusRadius = xDistance < samus.Kinematics.Radius.X;
        ushort remainingX = unchecked((ushort)(xDistance - samus.Kinematics.Radius.X));
        if (!centerWithinSamusRadius && remainingX >= slot.Radius.X)
            return false;

        ushort biasedSamusY = unchecked((ushort)(samus.Position.Y.Whole + 3));
        if (unchecked((short)(biasedSamusY - slot.Position.Y.Whole)) >= 0)
            return false;

        ushort verticalDistance = unchecked((ushort)(slot.Position.Y.Whole - biasedSamusY));
        ushort remainingY = unchecked((ushort)(verticalDistance - samus.Kinematics.Radius.Y));
        return verticalDistance < samus.Kinematics.Radius.Y ||
            remainingY == slot.Radius.Y ||
            remainingY < slot.Radius.Y;
    }

    /// <summary>Ports all four private no-operand instruction callbacks.</summary>
    private bool TryProcessMaridiaLargeSnailInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Oum)
            return false;

        MaridiaLargeSnailEnemyState state = RequireMaridiaLargeSnailState(slot);
        if (!Enum.IsDefined((MaridiaLargeSnailInstruction)opcode))
            return false;
        MaridiaLargeSnailInstruction instruction = (MaridiaLargeSnailInstruction)opcode;
        switch (instruction)
        {
            case MaridiaLargeSnailInstruction.PlaySplashedOutOfWaterSound:
                LastMaridiaLargeSnailSoundEffect = MaridiaLargeSnailSplashSound;
                break;
            case MaridiaLargeSnailInstruction.SetAnimationFinished:
                state.AttackAnimationFinished = true;
                break;
            case MaridiaLargeSnailInstruction.AllowAttackRotation:
                state.AttackAllowsRotation = true;
                break;
            case MaridiaLargeSnailInstruction.DisallowAttackRotation:
                state.AttackAllowsRotation = false;
                break;
            default:
                throw new InvalidOperationException($"Undefined MaridiaLargeSnailInstruction {instruction}.");
        }

        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Installs one of eight native Oum lists only when the requested index changes.</summary>
    internal static void InstallMaridiaLargeSnailInstructionList(
        RoomEnemySlot slot,
        MaridiaLargeSnailEnemyState state)
    {
        if (state.RequestedInstructionListIndex == state.InstalledInstructionListIndex)
            return;
        if (state.RequestedInstructionListIndex >= 8)
        {
            throw new InvalidDataException(
                $"Maridia Large Snail list index {state.RequestedInstructionListIndex} " +
                "is outside the retail eight-entry table.");
        }

        state.InstalledInstructionListIndex = state.RequestedInstructionListIndex;
        slot.CurrentInstruction = MaridiaLargeSnailInstructionDefinitions
            .InstructionPointer(state.RequestedInstructionListIndex);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>
    /// Runs the common/non-damaging Oum hitbox tail after the multibox dispatcher selected a
    /// concrete frame hitbox. Damaging hitboxes call common touch before entering this tail.
    /// </summary>
    private static void ResolveMaridiaLargeSnailTouchAfterCommon(
        RoomEnemySlot slot,
        SamusState samus)
    {
        if (MaridiaLargeSnailTouchesSamusFromBelow(slot, samus))
            return;

        ushort shove = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
            ? unchecked((ushort)-4)
            : (ushort)4;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(shove)), 0);
    }

    /// <summary>
    /// Ports the private tail of <c>EnemyShot_Oum</c> at $A2:D3B8. It runs after common shot
    /// AI for damaging, freezing, and zero-damage vulnerability results alike.
    /// </summary>
    private void ResolveMaridiaLargeSnailShotAfterCommon() =>
        LastMaridiaLargeSnailSoundEffect = MaridiaLargeSnailShotSound;

    /// <summary>Requires an initialized Oum extension for the physical enemy slot.</summary>
    private MaridiaLargeSnailEnemyState RequireMaridiaLargeSnailState(RoomEnemySlot slot) =>
        _maridiaLargeSnailStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Maridia Large Snail state.");

    /// <summary>Publishes Oum's native shot sound after common bomb damage.</summary>
    internal void RegisterMaridiaLargeSnailBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.MaridiaLargeSnailShot)
        {
            AfterCommon = _ => ResolveMaridiaLargeSnailShotAfterCommon(),
        });
    }

    /// <summary>Routes the Oum definition through its bounce-state initialization.</summary>
    internal void RegisterMaridiaLargeSnailInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Oum, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMaridiaLargeSnail(slot);
        });
    }

    /// <summary>Dispatches Oum's bounce, roll, and attack phases once per enemy frame.</summary>
    internal void RegisterMaridiaLargeSnailMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Oum, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum, (slot, frame, nmiFrameCounter8) =>
        {
            RunMaridiaLargeSnailMain(
                slot,
                RequireMaridiaLargeSnailState(slot),
                frame.Samus,
                frame.Level,
                frame.ControllerInput);
        });
    }

    /// <summary>Claims Oum's private animation and attack-rotation opcodes.</summary>
    internal void RegisterMaridiaLargeSnailOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessMaridiaLargeSnailInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Preserves protected-shell no-op hits and runs common damage only for vulnerable hitboxes.</summary>
    internal void RegisterMaridiaLargeSnailShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MaridiaLargeSnailShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Maridia Large Snail lost its required extended-spritemap property.");
                if (c.HitboxShotAi == EnemyInteractionCallback.NoOp)
                {
                    // Native multibox collision marks the projectile before calling the
                    // no-op. It does not run normal shot AI, create an impact, play $57,
                    // or consult vulnerability for this protected shell component.
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    return true;
                }
                if (c.HitboxShotAi != EnemyInteractionCallback.MaridiaLargeSnailShot)
                {
                    throw new InvalidDataException(
                        $"Maridia Large Snail hitbox shot AI " +
                        $"$A2:{c.HitboxShotAiPointer:X4} is not translated.");
                }
                return false;
            },
            AfterCommon = (_, _) => ResolveMaridiaLargeSnailShotAfterCommon(),
        });
    }

    /// <summary>Dispatches damaging and non-damaging Oum hitboxes with their distinct Samus pushes.</summary>
    internal void RegisterMaridiaLargeSnailTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Maridia Large Snail lost its required extended-spritemap property.");
                if (context.HitboxTouchAi is not (
                        EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch or
                        EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch))
                {
                    throw new InvalidDataException(
                        $"Maridia Large Snail hitbox touch AI " +
                        $"$A2:{context.HitboxTouchAiPointer:X4} is not translated.");
                }

                // $D388 enters common touch damage and falls directly through $D38C's
                // asymmetric shove. $D38C frames execute only the shove, which is why the
                // closed shell can carry/push Samus without inflicting the header's 100 HP.
                if (context.HitboxTouchAi == EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch)
                    _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveMaridiaLargeSnailTouchAfterCommon(context.Slot, context.Samus);
            });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetMaridiaLargeSnailRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastMaridiaLargeSnailSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastMaridiaLargeSnailSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
