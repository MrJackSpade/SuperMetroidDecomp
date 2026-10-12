using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The Evir species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class EvirBehavior : IEnemySpecies
{
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyCollision _collision;

    internal EvirBehavior(EnemySlotTable slotTable, EnemyCollision collision)
    {
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Executes the Evir projectile's private animation instructions.</summary>
    private bool TryProcessEvirProjectileInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.EvirProjectile ||
            !Enum.IsDefined((EvirInstruction)word))
            return false;

        switch ((EvirInstruction)word)
        {
            case EvirInstruction.PlaySpitSFX:
                QueueEvirSpitSound();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case EvirInstruction.SetInitialRegenerationXOffset:
                SetInitialEvirRegenerationOffset(slot, RequireEvirState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case EvirInstruction.AdvanceRegenerationXOffset:
                AdvanceEvirRegenerationOffset(slot, RequireEvirState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case EvirInstruction.FinishRegeneration:
                FinishEvirRegeneration(RequireEvirState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Evir projectile does not own instruction ${word:X4}.");
        }
    }


    private const ushort EvirSpitSound = 0x005e;
    private const ushort EvirActivationDistance = 0x0080;
    private const ushort EvirProjectileSpeed = 4;
    private const ushort EvirFarOffscreenDistance = 0x0100;

    internal readonly EvirEnemyState?[] _evirStates =
        new EvirEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two spit sound requested during this enemy frame.</summary>
    public ushort? LastEvirSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Evir</c> at $A8:87E0.</summary>
    internal void InitializeEvir(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new EvirEnemyState(slot);
        _evirStates[slot.SlotIndex] = state;

        if (slot.Parameter1 != 0)
        {
            // The second record is not a second creature. Its nonzero parameter selects the
            // arms role, which always reads facing and position from the immediately prior
            // body record. Retail gives it layer four so the two maps compose correctly.
            PositionEvirArms(slot, state, initializing: true);
            slot.Layer = 4;
        }
        else
        {
            SetEvirBodyFacing(slot, state, samus);

            // Parameter two's low byte indexes two adjacent signed 16.16 entries in the
            // shared linear-speed table: downward first, then upward. The high byte is the
            // steady-state number of frames between direction changes.
            ushort speedOffset = unchecked((ushort)((byte)slot.Parameter2 * 8));
            (state.DownVelocity, state.DownSubvelocity) = EnemyMath.ReadLinearEnemySpeed(speedOffset);
            (state.UpVelocity, state.UpSubvelocity) = EnemyMath.ReadLinearEnemySpeed(
                unchecked((ushort)(speedOffset + 4)));

            // $A8:8811 indexes Enemy.init1+1 ($0FB7) with Y, the speed-table byte offset,
            // rather than X. The byte read is absolute enemy RAM chosen by the speed index
            // alone: in the 100% movie's Botwoon-hallway Evirs (speed 12) it is the high
            // byte of slot 2's palette word, so the first half-cycle is not always zero.
            state.MovementTimer = (ushort)(ReadAliasedEnemyRamByte(
                EvirInitDefinitions.AliasedTimerAddress + speedOffset) >> 1);
        }

        state.MovementDirection = 0;
        state.InstalledInstructionList = 0;
        state.Function = EvirAiFunction.HandleBodyOrArms;
    }

    /// <summary>Reads one byte of the 32-slot enemy RAM block at absolute WRAM <paramref name="address"/>.</summary>
    private byte ReadAliasedEnemyRamByte(int address)
    {
        int relative = address - EnemyRamDefinitions.FirstSlotAddress;
        int slotIndex = relative / EnemyRamDefinitions.SlotStride;
        if (relative < 0 || slotIndex >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidOperationException($"WRAM ${address:X4} lies outside enemy RAM.");
        int byteOffset = relative % EnemyRamDefinitions.SlotStride;
        ushort word = _slotTable._slots[slotIndex].ReadNativeWord(byteOffset & ~1);
        return (byteOffset & 1) == 0 ? (byte)word : SnesLittleEndian.HighByte(word);
    }

    /// <summary>Ports <c>InitAI_EvirProjectile</c> at $A8:88B0.</summary>
    internal void InitializeEvirProjectile(RoomEnemySlot slot)
    {
        var state = new EvirEnemyState(slot)
        {
            RequestedInstructionList = EvirInstructionProgramDefinitions.ProjectileNormal,
            InstalledInstructionList = 0,
            RegenerationFlag = 0,
            MovingFlag = 0,
            Function = EvirAiFunction.ProjectileIdle,
            RegenerationXOffset = 0,
        };
        _evirStates[slot.SlotIndex] = state;
        InstallEvirInstruction(slot, state);

        RoomEnemySlot body = RequireEvirRelativeSlot(slot, -2, EnemyDefinitionId.Evir, "projectile body", initializing: true);
        // The projectile definition has no graphics-set entry of its own. Native copies the
        // body's resolved palette/tile indexes so the projectile map addresses the Evir art.
        slot.PaletteIndex = body.PaletteIndex;
        slot.VramTilesIndex = body.VramTilesIndex;
        ResetEvirProjectilePosition(slot, state);

        // Native clears installed-list after having installed the normal list. That oddity
        // forces the first main-AI request to refresh the bytecode timer exactly once.
        state.InstalledInstructionList = 0;
    }

    private void RunEvirMain(RoomEnemySlot slot, EvirEnemyState state, SamusState? samus)
    {
        if (state.Function != EvirAiFunction.HandleBodyOrArms)
        {
            throw new InvalidDataException(
                $"Evir body/arms function $A8:{(ushort)state.Function:X4} is not translated.");
        }

        if (slot.Parameter1 != 0)
        {
            PositionEvirArms(slot, state);
            return;
        }

        RoomEnemySlot projectile = RequireEvirRelativeSlot(
            slot,
            2,
            EnemyDefinitionId.EvirProjectile,
            "body projectile");
        EvirEnemyState projectileState = RequireEvirState(projectile);

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (projectileState.RegenerationFlag == 0)
            SetEvirBodyFacing(slot, state, samus);

        int displacement = state.MovementDirection == 0
            ? ComposeEvirFixed(state.UpVelocity, state.UpSubvelocity)
            : ComposeEvirFixed(state.DownVelocity, state.DownSubvelocity);
        slot.Position = slot.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(displacement));

        // DEC/BPL means zero expires immediately into $FFFF. Every later reset uses the
        // unshifted high parameter byte, despite initialization's unrelated aliasing bug.
        state.MovementTimer = unchecked((ushort)(state.MovementTimer - 1));
        if ((state.MovementTimer & 0x8000) != 0)
        {
            state.MovementTimer = SnesLittleEndian.HighByte(slot.Parameter2);
            state.MovementDirection ^= 1;
        }
    }

    private void RunEvirProjectileMain(
        RoomEnemySlot slot,
        EvirEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (slot.FrozenTimer == 0)
        {
            if (state.MovingFlag != 0)
            {
                RequestEvirInstruction(
                    slot,
                    state,
                    EvirInstructionProgramDefinitions.ProjectileNormal);
            }
            else if (state.RegenerationFlag != 0)
            {
                RequestEvirInstruction(
                    slot,
                    state,
                    EvirInstructionProgramDefinitions.ProjectileRegenerating);
            }
            else
            {
                TryLaunchEvirProjectile(slot, state, samus);
            }
        }

        switch (state.Function)
        {
            case EvirAiFunction.ProjectileIdle:
                ResetEvirProjectilePosition(slot, state);
                break;
            case EvirAiFunction.ProjectileMoving:
                RunMovingEvirProjectile(slot, state, camera);
                break;
            case EvirAiFunction.ProjectileRegenerating:
                RunRegeneratingEvirProjectile(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Evir projectile function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void SetEvirBodyFacing(
        RoomEnemySlot body,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir facing AI requires the active Samus actor.");
        state.FacingDirection = unchecked((short)(activeSamus.Position.X.Whole - body.Position.X.Whole)) < 0
            ? (ushort)0
            : (ushort)1;
        RequestEvirInstruction(
            body,
            state,
            state.FacingDirection == 0
                ? EvirInstructionProgramDefinitions.BodyFacingLeft
                : EvirInstructionProgramDefinitions.BodyFacingRight);
    }

    private void PositionEvirArms(RoomEnemySlot arms, EvirEnemyState state, bool initializing = false)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(arms, -1, EnemyDefinitionId.Evir, "arms body", initializing);
        EvirEnemyState bodyState = RequireEvirState(body);
        state.FacingDirection = bodyState.FacingDirection;
        arms.Position = arms.Position.WithWhole(new(unchecked((ushort)(
            body.Position.X.Whole + (state.FacingDirection == 0 ? -4 : 4))), unchecked((ushort)(body.Position.Y.Whole + 10))));
        RequestEvirInstruction(
            arms,
            state,
            state.FacingDirection == 0
                ? EvirInstructionProgramDefinitions.ArmsFacingLeft
                : EvirInstructionProgramDefinitions.ArmsFacingRight);
    }

    private void ResetEvirProjectilePosition(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        EvirEnemyState bodyState = RequireEvirState(body);
        state.FacingDirection = bodyState.FacingDirection;
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(
            body.Position.X.Whole + (state.FacingDirection == 0 ? -4 : 4))), unchecked((ushort)(body.Position.Y.Whole + 18))));
    }

    private void TryLaunchEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir projectile aiming requires the active Samus actor.");
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (!EnemyMath.IsWithinStrictModularDistance(
                activeSamus.Position.X.Whole,
                body.Position.X.Whole,
                EvirActivationDistance))
        {
            return;
        }

        // CalculateAngleOfSamusFromEnemy returns zero up/clockwise. The two native angle
        // transforms below feed the buggy table multipliers and produce signed 16.16 speed.
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(activeSamus.Position.X.Whole - projectile.Position.X.Whole)), unchecked((short)(activeSamus.Position.Y.Whole - projectile.Position.Y.Whole))));
        byte transformedAngle = unchecked((byte)-(byte)(cartridgeAngle - 0x40));
        (short xWhole, ushort xFraction) = EnemyMath.ReadEightBitCosineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        (short yWhole, ushort yFraction) = EnemyMath.ReadEightBitNegativeSineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        state.ProjectileVelocity = new FixedVector<RoomPixels>(
            SnesSignedSixteenSixteen.FromParts(xWhole, xFraction),
            SnesSignedSixteenSixteen.FromParts(yWhole, yFraction));
        RequestEvirInstruction(
            projectile,
            state,
            EvirInstructionProgramDefinitions.ProjectileNormal);
        state.MovingFlag = 1;
        state.Function = EvirAiFunction.ProjectileMoving;
    }

    private void RunMovingEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, camera);
        projectile.Position = projectile.Position.Advance(state.ProjectileVelocity);
    }

    private void StartEvirRegenerationIfFarOffscreen(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        bool farOffscreen =
            EnemyMath.IsNegative16(projectile.Position.X.Whole + EvirFarOffscreenDistance - camera.X) ||
            EnemyMath.IsNegative16(camera.X + 256 + EvirFarOffscreenDistance - projectile.Position.X.Whole) ||
            EnemyMath.IsNegative16(projectile.Position.Y.Whole + EvirFarOffscreenDistance - camera.Y) ||
            EnemyMath.IsNegative16(camera.Y + 256 + EvirFarOffscreenDistance - projectile.Position.Y.Whole);
        if (!farOffscreen)
            return;

        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (body.FrozenTimer != 0)
            return;

        state.MovingFlag = 0;
        state.RegenerationFlag = 1;
        state.Function = EvirAiFunction.ProjectileRegenerating;
        RequestEvirInstruction(
            projectile,
            state,
            EvirInstructionProgramDefinitions.ProjectileRegenerating);
    }

    private void RunRegeneratingEvirProjectile(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (body.FrozenTimer != 0)
            return;

        if (state.RegenerationFlag == 0)
        {
            RequestEvirInstruction(
                projectile,
                state,
                EvirInstructionProgramDefinitions.ProjectileNormal);
            state.MovingFlag = 0;
            state.Function = EvirAiFunction.ProjectileIdle;
            return;
        }

        ResetEvirProjectilePosition(projectile, state);
        projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(
            projectile.Position.X.Whole + state.RegenerationXOffset)));
    }

    /// <summary>Instruction $A8:878F.</summary>
    private void QueueEvirSpitSound() => LastEvirSoundEffect = EvirSpitSound;

    /// <summary>Instruction $A8:879B.</summary>
    private void SetInitialEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationXOffset = EvirBodyFacesRight(slot)
            ? unchecked((ushort)-8)
            : (ushort)8;

    /// <summary>Instruction $A8:87B6.</summary>
    private void AdvanceEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationXOffset = unchecked((ushort)(
            state.RegenerationXOffset + (EvirBodyFacesRight(slot) ? 1 : -1)));

    /// <summary>
    /// Both regeneration instructions test <c>Evir.instList-$80,X</c>: the installed list
    /// of the body two slots before the projectile, not any facing word of its own.
    /// </summary>
    private bool EvirBodyFacesRight(RoomEnemySlot projectile)
    {
        EvirEnemyState body = _evirStates[projectile.SlotIndex - 2] ?? throw new InvalidDataException(
            $"Evir projectile slot {projectile.SlotIndex} has no body two slots before it.");
        return body.InstalledInstructionList == EvirInstructionProgramDefinitions.BodyFacingRight;
    }

    /// <summary>Instruction $A8:87CB.</summary>
    private static void FinishEvirRegeneration(EvirEnemyState state)
    {
        state.RegenerationFlag = 0;
        state.MovingFlag = 0;
        state.Function = EvirAiFunction.ProjectileIdle;
    }

    private static int ComposeEvirFixed(short whole, ushort fraction) =>
        unchecked((SnesSixteenSixteen.FromWhole(whole)) | fraction);


    private static void RequestEvirInstruction(
        RoomEnemySlot slot,
        EvirEnemyState state,
        ushort instructionList)
    {
        state.RequestedInstructionList = instructionList;
        InstallEvirInstruction(slot, state);
    }

    private static void InstallEvirInstruction(RoomEnemySlot slot, EvirEnemyState state)
    {
        if (state.RequestedInstructionList == state.InstalledInstructionList)
            return;
        slot.CurrentInstruction = state.RequestedInstructionList;
        state.InstalledInstructionList = state.RequestedInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot RequireEvirRelativeSlot(
        RoomEnemySlot owner,
        int relativeSlot,
        EnemyDefinitionId expectedDefinition,
        string relationship,
        bool initializing = false)
    {
        int targetIndex = owner.SlotIndex + relativeSlot;
        // Native links are physical offsets, not live-header references. Contact can
        // clear the body after the active list was built; its deleted arms and spit
        // still execute once and read the cleared common words. Validate authored
        // identities at initialization, then retain the established physical alias.
        if ((uint)targetIndex >= _slotTable._slots.Length ||
            (_slotTable._slots[targetIndex].EnemyDefinitionPointer != expectedDefinition &&
                (initializing || _evirStates[targetIndex] is null)))
        {
            throw new InvalidDataException(
                $"Evir {relationship} expected definition ${(int)expectedDefinition:X4} " +
                $"at relative slot {relativeSlot:+#;-#;0} from {owner.SlotIndex}.");
        }
        return _slotTable._slots[targetIndex];
    }

    private EvirEnemyState RequireEvirState(RoomEnemySlot slot) =>
        _evirStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Evir state.");

    /// <summary>
    /// Ports $A8:8B16 after common touch/shot/power-bomb damage. Native applies its +$40
    /// and +$80 physical-slot aliases relative to whichever $E63F record received the
    /// callback. Ordinary shots/touch can only reach the body because the arms carry
    /// property $0400, but the power-bomb walker deliberately ignores that property and can
    /// invoke the same code on the arms. Preserve that odd relative aliasing instead of
    /// replacing it with a body-only composite helper.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemySlot actor)
    {
        int firstRelativeIndex = actor.SlotIndex + 1;
        int secondRelativeIndex = actor.SlotIndex + 2;
        if ((uint)secondRelativeIndex >= _slotTable._slots.Length)
        {
            throw new InvalidDataException(
                $"Evir callback at slot {actor.SlotIndex} exceeds the native +$80 slot alias.");
        }
        RoomEnemySlot firstRelative = _slotTable._slots[firstRelativeIndex];
        RoomEnemySlot secondRelative = _slotTable._slots[secondRelativeIndex];

        if (actor.Health == 0)
        {
            firstRelative.Properties = firstRelative.Properties.With(EnemyProperties.Deleted);
            secondRelative.Properties = secondRelative.Properties.With(EnemyProperties.Deleted);
        }

        if (actor.FrozenTimer == 0)
            return;
        firstRelative.FrozenTimer = actor.FrozenTimer;
        firstRelative.AiHandlerBits = unchecked((ushort)(firstRelative.AiHandlerBits | 0x0004));
        if (secondRelative.VariableC != (ushort)EvirAiFunction.ProjectileMoving)
        {
            secondRelative.FrozenTimer = actor.FrozenTimer;
            secondRelative.AiHandlerBits = unchecked((ushort)(
                secondRelative.AiHandlerBits | 0x0004));
        }
    }

    internal void RegisterEvirBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Evir &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.EvirShot)
        {
            AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
        });
    }

    internal void RegisterEvirInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Evir, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeEvir(slot, samus);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_EvirProjectile, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.EvirProjectile, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeEvirProjectile(slot);
        });
    }

    internal void RegisterEvirMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Evir, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir, (slot, frame, nmiFrameCounter8) =>
        {
            RunEvirMain(slot, RequireEvirState(slot), frame.Samus);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_EvirProjectile, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.EvirProjectile, (slot, frame, nmiFrameCounter8) =>
        {
            RunEvirProjectileMain(
                slot,
                RequireEvirState(slot),
                frame.Samus,
                frame.Camera);
        });
    }

    internal void RegisterEvirOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessEvirProjectileInstruction(slot, word, ref cursor);
        });
    }

    internal void RegisterEvirPowerBombReactions(EnemyAiTable table)
    {
        // Touch and shot collision lists exclude the arms through property $0400, but $A0:A306's power-bomb walker
        // does not. Both body and arms therefore dispatch $A8:8B0C and its intentionally physical +$40/+80 follow-up
        // aliases.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Evir &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.EvirPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveEvirCombatAfterCommon(enemy),
        });
    }

    internal void RegisterEvirShotReactions(EnemyAiTable table)
    {
        // Evir's bank-$A8 shot tail runs after every accepted common shot, including the $FF ice result. That is
        // where the body copies the newly installed frozen timer to its arms and attached (idle) spit.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Evir &&
                                                     enemy.Parameter1 == 0 &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.EvirShot)
        {
            AfterCommon = (c, _) => ResolveEvirCombatAfterCommon(c.Enemy),
        });
    }

    internal void RegisterEvirTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir &&
                               slot.Parameter1 == 0 &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.EvirTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveEvirCombatAfterCommon(context.Slot);
            });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastEvirSoundEffect = null;
        Array.Clear(_evirStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastEvirSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastEvirSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
