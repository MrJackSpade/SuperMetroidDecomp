using SuperMetroid.Core.Audio;
namespace SuperMetroid.Core.Game;

/// <summary>The Dragon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DragonBehavior : IEnemySpecies
{
    private readonly EnemyProjectileEngine _projectiles;
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyCollision _collision;

    internal DragonBehavior(EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision)
    {
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Executes Dragon's private animation instruction.</summary>
    private bool TryProcessDragonInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Dragon ||
            !Enum.IsDefined((DragonInstruction)word))
            return false;

        switch ((DragonInstruction)word)
        {
            case DragonInstruction.AttackFinishedCallback:
                FinishDragonAttackAnimation(slot);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Dragon does not own instruction ${word:X4}.");
        }
    }


    private const ushort DragonRiseOrSinkFrames = 0x0030;
    private const ushort DragonShotCount = 3;
    private const ushort DragonWaitBeforeSinkFrames = 0x0060;
    private const ushort DragonWaitBeforeRiseFrames = 0x0080;
    private const ushort DragonFireballSound = 0x0061;

    private readonly ushort[] _dragonRequestedInstructionLists = new ushort[RoomEnemySystem.MaximumEnemyCount];
    private readonly ushort[] _dragonInstalledInstructionLists = new ushort[RoomEnemySystem.MaximumEnemyCount];
    private readonly ushort[] _dragonAnimationFinishedFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    internal readonly DragonEnemyState?[] _dragonStates = new DragonEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two sound queued by a Dragon volley during the current frame.</summary>
    public ushort? LastDragonSoundEffect { get; private set; }

    internal void ResetDragonRoomState()
    {
        Array.Clear(_dragonRequestedInstructionLists);
        Array.Clear(_dragonInstalledInstructionLists);
        Array.Clear(_dragonAnimationFinishedFlags);
        Array.Clear(_dragonStates);
        LastDragonSoundEffect = null;
    }

    /// <summary>Ports <c>InitAI_Dragon</c> at $A2:E606.</summary>
    internal void InitializeDragon(RoomEnemySlot slot)
    {
        DragonEnemyState state = CreateDragonState(slot);
        state.AnimationFinished = false;

        if (state.IsWing)
        {
            // The wing begins on table index two (left-facing wings) and never runs body AI.
            // Property $0400 keeps the cosmetic record out of touch/shot collision passes.
            state.RequestedInstructionListIndex = DragonAnimationSelector.WingsFacingLeft;
            state.InstalledInstructionListIndex = DragonAnimationSelector.WingsFacingLeft;
            slot.CurrentInstruction = DragonAnimationDefinitions.InstructionList(
                DragonAnimationSelector.WingsFacingLeft);
            slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
            state.Function = DragonEnemyFunction.WingNoOp;
            return;
        }

        state.RequestedInstructionListIndex = DragonAnimationSelector.IdleFacingLeft;
        state.InstalledInstructionListIndex = DragonAnimationSelector.IdleFacingLeft;
        slot.CurrentInstruction = DragonAnimationDefinitions.InstructionList(
            DragonAnimationSelector.IdleFacingLeft);
        state.Function = DragonEnemyFunction.WaitToRise;
    }

    /// <summary>Ports <c>MainAI_Dragon</c> at $A2:E64E and its five body functions.</summary>
    private void RunDragonMain(RoomEnemySlot slot, DragonEnemyState state, SamusState? samus)
    {
        switch (state.Function)
        {
            case DragonEnemyFunction.WingNoOp:
                return;

            case DragonEnemyFunction.WaitToRise:
                RunDragonWaitToRise(slot, state, samus);
                return;

            case DragonEnemyFunction.Rising:
                RunDragonRising(slot, state);
                return;

            case DragonEnemyFunction.Attacking:
                RunDragonAttacking(slot, state);
                return;

            case DragonEnemyFunction.WaitToSink:
                RunDragonWaitToSink(slot, state);
                return;

            case DragonEnemyFunction.Sinking:
                RunDragonSinking(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dragon function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports Dragon function $A2:E654.</summary>
    internal void RunDragonWaitToRise(
        RoomEnemySlot body,
        DragonEnemyState bodyState,
        SamusState? samus)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (unchecked((short)bodyState.FunctionTimer) >= 0)
            return;
        if (samus is null)
            throw new InvalidOperationException("Dragon facing selection requires the active Samus actor.");

        RoomEnemySlot wing = GetDragonWing(body);
        DragonEnemyState wingState = RequireDragonState(wing);
        bodyState.FunctionTimer = DragonRiseOrSinkFrames;
        bodyState.Function = DragonEnemyFunction.Rising;

        // $E666-$E672 rotates the sign of (Samus X - body X) into bit 15 of variable A,
        // preserving its other fifteen bits. The following byte-sized list edits use that
        // sign to select even/odd left/right table entries for both physical records.
        bool facingLeft = unchecked((short)(samus.Position.X.Whole - body.Position.X.Whole)) < 0;
        bodyState.DirectionWord = unchecked((ushort)(
            (bodyState.DirectionWord & 0x7fff) | (facingLeft ? 0x8000 : 0)));
        bodyState.RequestedInstructionListIndex = DragonAnimationDefinitions.WithFacing(
            bodyState.RequestedInstructionListIndex,
            facingLeft);
        wingState.RequestedInstructionListIndex = DragonAnimationDefinitions.WithFacing(
            wingState.RequestedInstructionListIndex,
            facingLeft);
        InstallDragonInstructionList(body, bodyState);
        InstallDragonInstructionList(wing, wingState);
    }

    /// <summary>Ports Dragon function $A2:E6AD.</summary>
    internal void RunDragonRising(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (unchecked((short)bodyState.FunctionTimer) < 0)
        {
            bodyState.RequestedInstructionListIndex =
                DragonAnimationDefinitions.AttackingWithSameFacing(
                    bodyState.RequestedInstructionListIndex);
            bodyState.AttackCounter = DragonShotCount;
            bodyState.Function = DragonEnemyFunction.Attacking;
        }

        // The native 16.16 constant is exactly -$0001:0000 in the NTSC build, so both
        // subpositions remain untouched while body and wing rise one whole pixel.
        RoomEnemySlot wing = GetDragonWing(body);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 1)));
        wing.Position = wing.Position.WithWholeY(unchecked((ushort)(wing.Position.Y.Whole - 1)));
    }

    /// <summary>Ports Dragon function $A2:E6F1.</summary>
    internal void RunDragonAttacking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        InstallDragonInstructionList(body, bodyState);
        if (!bodyState.AnimationFinished)
            return;

        bodyState.AnimationFinished = false;

        // $FFFF forces the same attack list to reinstall after each animation reaches its
        // sleep opcode. That restart is what spaces the three fireballs by a full ROM list.
        bodyState.InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
        SpawnDragonFireball(body, bodyState);
        LastDragonSoundEffect = DragonFireballSound;
        bodyState.AttackCounter = unchecked((ushort)(bodyState.AttackCounter - 1));
        if (bodyState.AttackCounter != 0)
            return;

        bodyState.RequestedInstructionListIndex =
            DragonAnimationDefinitions.IdleWithSameFacing(
                bodyState.RequestedInstructionListIndex);
        bodyState.FunctionTimer = DragonWaitBeforeSinkFrames;
        bodyState.Function = DragonEnemyFunction.WaitToSink;
    }

    /// <summary>Ports Dragon function $A2:E734.</summary>
    private static void RunDragonWaitToSink(
        RoomEnemySlot body,
        DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (bodyState.FunctionTimer != 0)
            return;

        bodyState.FunctionTimer = DragonRiseOrSinkFrames;
        bodyState.Function = DragonEnemyFunction.Sinking;
        InstallDragonInstructionList(body, bodyState);
    }

    /// <summary>Ports Dragon function $A2:E749.</summary>
    private void RunDragonSinking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = unchecked((ushort)(bodyState.FunctionTimer - 1));
        if (unchecked((short)bodyState.FunctionTimer) < 0)
        {
            bodyState.FunctionTimer = DragonWaitBeforeRiseFrames;
            bodyState.Function = DragonEnemyFunction.WaitToRise;
        }

        // As with rising, the native regionalized 16.16 displacement is exactly one pixel.
        RoomEnemySlot wing = GetDragonWing(body);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        wing.Position = wing.Position.WithWholeY(unchecked((ushort)(wing.Position.Y.Whole + 1)));
    }

    /// <summary>Ports instruction opcode $A2:E5FB.</summary>
    private void FinishDragonAttackAnimation(RoomEnemySlot body)
    {
        DragonEnemyState state = RequireDragonState(body);
        if (state.IsWing)
        {
            throw new InvalidDataException(
                "Dragon wing reached the body-only attack-finished instruction.");
        }
        state.AnimationFinished = true;
    }

    /// <summary>
    /// Ports the shared tail at $A2:E7DA after touch, shot, or power-bomb common AI. A dead
    /// body deletes its wing; a surviving body mirrors every timer/handler word that affects
    /// visible hurt and freeze rendering into that cosmetic record.
    /// </summary>
    private void ResolveDragonCombatAfterCommon(RoomEnemySlot actor)
    {
        // The callback at $E7DA does not test init0. Normal touch/shot collision excludes
        // wings via property $0400, but bank $A0's power-bomb scan does not. Consequently a
        // power bomb may invoke this on a wing and copy into whatever physical record follows
        // it. Preserve that retail aliasing bug instead of silently normalizing to a body/wing
        // relationship; several dense populations place another live actor in that slot.
        if (actor.SlotIndex >= RoomEnemySystem.MaximumEnemyCount - 1)
        {
            throw new InvalidDataException(
                "Dragon combat callback cannot alias beyond the final physical enemy slot.");
        }
        RoomEnemySlot following = _slotTable._slots[actor.SlotIndex + 1];
        if (actor.Health == 0)
        {
            following.Properties = following.Properties.With(EnemyProperties.Deleted);
            return;
        }

        following.ShakeTimer = actor.ShakeTimer;
        following.InvincibilityTimer = actor.InvincibilityTimer;
        following.FlashTimer = actor.FlashTimer;
        following.FrozenTimer = actor.FrozenTimer;
        following.AiHandlerBits = actor.AiHandlerBits;
    }

    internal static void InstallDragonInstructionList(
        RoomEnemySlot slot,
        DragonEnemyState state)
    {
        if (state.RequestedInstructionListIndex == state.InstalledInstructionListIndex)
            return;
        state.InstalledInstructionListIndex = state.RequestedInstructionListIndex;
        slot.CurrentInstruction = DragonAnimationDefinitions.InstructionList(
            state.RequestedInstructionListIndex);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot GetDragonWing(RoomEnemySlot body)
    {
        if (body.SlotIndex >= RoomEnemySystem.MaximumEnemyCount - 1)
            throw new InvalidDataException("Dragon body occupies the final physical enemy slot.");

        RoomEnemySlot wing = _slotTable._slots[body.SlotIndex + 1];
        if (body.Parameter1 != 0 || wing.EnemyDefinitionPointer != EnemyDefinitionId.Dragon ||
            wing.Parameter1 == 0)
        {
            throw new InvalidDataException(
                $"Dragon body slot {body.SlotIndex} is not followed by its retail wing record.");
        }
        return wing;
    }

    private DragonEnemyState CreateDragonState(RoomEnemySlot slot)
    {
        var state = new DragonEnemyState(
            slot,
            _dragonRequestedInstructionLists,
            _dragonInstalledInstructionLists,
            _dragonAnimationFinishedFlags);
        _dragonStates[slot.SlotIndex] = state;
        return state;
    }

    private DragonEnemyState RequireDragonState(RoomEnemySlot slot) =>
        _dragonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Dragon state.");

    internal void RegisterDragonBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.DragonShot)
        {
            AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    internal static void RegisterDragonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunDragonFireballPreInstruction(context.Projectile, context.Camera.Y);
        }, EnemyProjectilePreInstruction.DragonFireball);
    }

    internal void RegisterDragonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Dragon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeDragon(slot);
        });
    }

    internal void RegisterDragonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Dragon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon, (slot, frame, nmiFrameCounter8) =>
        {
            RunDragonMain(slot, RequireDragonState(slot), frame.Samus);
        });
    }

    internal void RegisterDragonOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessDragonInstruction(slot, word, ref cursor);
        });
    }

    internal void RegisterDragonPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DragonPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveDragonCombatAfterCommon(enemy),
        });
    }

    internal void RegisterDragonShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DragonShot)
        {
            AfterCommon = (c, _) => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    internal void RegisterDragonTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.DragonTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveDragonCombatAfterCommon(context.Slot);
            });
    }
    private const ushort DragonFireballInitialYVelocity = 0xfc3f;
    private const ushort DragonFireballLeftXVelocity = 0xfd40;
    private const ushort DragonFireballRightXVelocity = 0x02c0;
    private const ushort DragonFireballGravity = 0x0020;

    /// <summary>Ports projectile initializer $86:B4EF.</summary>
    internal void SpawnDragonFireball(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.DragonFireball,
            unchecked((ushort)(body.PaletteIndex | body.VramTilesIndex)));
        projectile.Position = projectile.Position.WithY(new(unchecked((ushort)(body.Position.Y.Whole - 28)), 0));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(DragonFireballInitialYVelocity));

        if (unchecked((short)bodyState.DirectionWord) < 0)
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 12)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(DragonFireballLeftXVelocity));
            projectile.InstructionPointer =
                DragonFireballInstructionProgramDefinitions.RisingLeft;
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + 12)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(DragonFireballRightXVelocity));
            projectile.InstructionPointer =
                DragonFireballInstructionProgramDefinitions.RisingRight;
        }
        projectile.Position = projectile.Position.WithFractionX(0);
    }

    /// <summary>Ports projectile pre-instruction $86:B535.</summary>
    internal static void RunDragonFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());

        if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
        {
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
                projectile.Velocity.Y.RawValue + DragonFireballGravity))));
            if (unchecked((short)projectile.Velocity.Y.RawValue) < 0)
                return;

            // The exact zero crossing switches from the rising pair of two-frame loops to
            // the falling pair and forces an instruction tick on this same projectile pass.
            projectile.InstructionPointer = unchecked((short)projectile.Velocity.X.RawValue) < 0
                ? DragonFireballInstructionProgramDefinitions.FallingLeft
                : DragonFireballInstructionProgramDefinitions.FallingRight;
            projectile.InstructionTimer = 1;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue + DragonFireballGravity))));

        // $86:B5B9 retains fireballs above the viewport; horizontal distance is never consulted.
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetDragonRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastDragonSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastDragonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
