using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>The Dragon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DragonBehavior : IEnemySpecies
{
    /// <summary>Allocates Dragon's three-shot fireball volleys from the room projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Resolves the cosmetic wing in the physical enemy slot following its body.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Performs common Samus touch resolution before Dragon's body and wing synchronization.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Connects Dragon body AI, paired-wing state, and fireball spawning to room systems.</summary>
    internal DragonBehavior(EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision)
    {
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Carries out Dragon's animation cue: the attack body marks its animation complete for main AI.</summary>
    private DragonProgram? ApplyDragonCue(RoomEnemySlot slot, DragonCue cue)
    {
        switch (cue.Kind)
        {
            case DragonCueKind.FinishAttack:
                FinishDragonAttackAnimation(slot);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DragonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Frames spent in each body rise or sink movement phase.</summary>
    private const ushort DragonRiseOrSinkFrames = 48;
    /// <summary>Fireballs requested by one attacking volley.</summary>
    private const ushort DragonShotCount = 3;
    /// <summary>Pause after attack completion before the body begins sinking.</summary>
    private const ushort DragonWaitBeforeSinkFrames = 96;
    /// <summary>Pause beneath the platform before the next rise attempt.</summary>
    private const ushort DragonWaitBeforeRiseFrames = 128;

    /// <summary>Per-slot animation lists requested by Dragon AI before the instruction owner installs them.</summary>
    private readonly ushort[] _dragonRequestedInstructionLists = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot animation lists currently installed in the common enemy interpreter.</summary>
    private readonly ushort[] _dragonInstalledInstructionLists = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot callback flags signaling completion of Dragon's attack animation.</summary>
    private readonly ushort[] _dragonAnimationFinishedFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Body and wing state indexed by their native physical enemy slots.</summary>
    internal readonly DragonEnemyState?[] _dragonStates = new DragonEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two sound queued by a Dragon volley during the current frame.</summary>
    public ushort? LastDragonSoundEffect { get; private set; }

    /// <summary>Clears per-slot animation requests, completion flags, species state, and the pending fireball sound on room load.</summary>
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
            state.Program = AnimationProgramCursor<DragonProgram>.Start(DragonAnimationDefinitions.Program(
                DragonAnimationSelector.WingsFacingLeft));
            slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
            state.Function = DragonEnemyFunction.WingNoOp;
            return;
        }

        state.RequestedInstructionListIndex = DragonAnimationSelector.IdleFacingLeft;
        state.InstalledInstructionListIndex = DragonAnimationSelector.IdleFacingLeft;
        state.Program = AnimationProgramCursor<DragonProgram>.Start(DragonAnimationDefinitions.Program(
            DragonAnimationSelector.IdleFacingLeft));
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
        bodyState.FunctionTimer = bodyState.FunctionTimer.Decrement().Remaining;
        if (!bodyState.FunctionTimer.IsNegative)
            return;
        if (samus is null)
            throw new InvalidOperationException("Dragon facing selection requires the active Samus actor.");

        RoomEnemySlot wing = GetDragonWing(body);
        DragonEnemyState wingState = RequireDragonState(wing);
        bodyState.FunctionTimer = NativeCountdown.Start(DragonRiseOrSinkFrames);
        bodyState.Function = DragonEnemyFunction.Rising;

        // $E666-$E672 rotates the sign of (Samus X - body X) into bit 15 of variable A,
        // preserving its other fifteen bits. The following byte-sized list edits use that
        // sign to select even/odd left/right table entries for both physical records.
        bool facingLeft = unchecked((short)(samus.Position.X.Whole - body.Position.X.Whole)) < 0;
        bodyState.DirectionWord = unchecked((ushort)(
            (bodyState.DirectionWord & EnemyWordMasks.BelowSignBits) | (facingLeft ? EnemyWordMasks.SignBit : 0)));
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
        bodyState.FunctionTimer = bodyState.FunctionTimer.Decrement().Remaining;
        if (bodyState.FunctionTimer.IsNegative)
        {
            bodyState.RequestedInstructionListIndex =
                DragonAnimationDefinitions.AttackingWithSameFacing(
                    bodyState.RequestedInstructionListIndex);
            bodyState.AttackCounter = NativeCountdown.Start(DragonShotCount);
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
        LastDragonSoundEffect = EnemySoundEffectDefinitions.DragonFireballSound;
        bodyState.AttackCounter = bodyState.AttackCounter.Decrement().Remaining;
        if (!bodyState.AttackCounter.IsZero)
            return;

        bodyState.RequestedInstructionListIndex =
            DragonAnimationDefinitions.IdleWithSameFacing(
                bodyState.RequestedInstructionListIndex);
        bodyState.FunctionTimer = NativeCountdown.Start(DragonWaitBeforeSinkFrames);
        bodyState.Function = DragonEnemyFunction.WaitToSink;
    }

    /// <summary>Ports Dragon function $A2:E734.</summary>
    private static void RunDragonWaitToSink(
        RoomEnemySlot body,
        DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = bodyState.FunctionTimer.Decrement().Remaining;
        if (!bodyState.FunctionTimer.IsZero)
            return;

        bodyState.FunctionTimer = NativeCountdown.Start(DragonRiseOrSinkFrames);
        bodyState.Function = DragonEnemyFunction.Sinking;
        InstallDragonInstructionList(body, bodyState);
    }

    /// <summary>Ports Dragon function $A2:E749.</summary>
    private void RunDragonSinking(RoomEnemySlot body, DragonEnemyState bodyState)
    {
        bodyState.FunctionTimer = bodyState.FunctionTimer.Decrement().Remaining;
        if (bodyState.FunctionTimer.IsNegative)
        {
            bodyState.FunctionTimer = NativeCountdown.Start(DragonWaitBeforeRiseFrames);
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

    /// <summary>Installs a newly requested animation list once and resets its instruction and loop timers.</summary>
    internal static void InstallDragonInstructionList(
        RoomEnemySlot slot,
        DragonEnemyState state)
    {
        if (state.RequestedInstructionListIndex == state.InstalledInstructionListIndex)
            return;
        state.InstalledInstructionListIndex = state.RequestedInstructionListIndex;
        state.Program = AnimationProgramCursor<DragonProgram>.Start(DragonAnimationDefinitions.Program(
            state.RequestedInstructionListIndex));
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Requires Dragon's authored body/wing population pair in adjacent physical slots.</summary>
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

    /// <summary>Attaches one Dragon slot to the shared native animation-word arrays.</summary>
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

    /// <summary>Rejects a Dragon callback that arrives before its physical slot has initialized state.</summary>
    private DragonEnemyState RequireDragonState(RoomEnemySlot slot) =>
        _dragonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Dragon state.");

    /// <summary>Applies Dragon's paired-body combat tail after the common bomb-as-shot callback.</summary>
    internal void RegisterDragonBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.DragonShot)
        {
            AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Connects Dragon fireballs to their per-frame gravity and camera-boundary pre-instruction.</summary>
    internal static void RegisterDragonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunDragonFireballPreInstruction(context.Projectile, context.Camera.Y);
        }, EnemyProjectilePreInstruction.DragonFireball);
    }

    /// <summary>Registers body and wing initialization for Dragon definition slots.</summary>
    internal void RegisterDragonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Dragon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeDragon(slot);
        });
    }

    /// <summary>Dispatches Dragon's wait, rise, attack, and sink phases through the species state.</summary>
    internal void RegisterDragonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Dragon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon, (slot, frame, nmiFrameCounter8) =>
        {
            RunDragonMain(slot, RequireDragonState(slot), frame.Samus);
        });
    }

    /// <summary>Runs Dragon's body and wing animations as typed programs (#641).</summary>
    internal void RegisterDragonTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Dragon, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireDragonState(slot);
            AnimationProgramCursor<DragonProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, DragonAnimations.Programs, ref cursor, DragonFrameDefinitions.Spritemap,
                    (cue, _) => ApplyDragonCue(slot, cue));
            state.Program = cursor;
        });


    /// <summary>Synchronizes wing and body state after a Dragon power-bomb reaction.</summary>
    internal void RegisterDragonPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DragonPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveDragonCombatAfterCommon(enemy),
        });
    }

    /// <summary>Synchronizes wing and body state after common projectile damage resolution.</summary>
    internal void RegisterDragonShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DragonShot)
        {
            AfterCommon = (c, _) => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs ordinary enemy touch damage before mirroring Dragon's hurt and freeze state into its wing.</summary>
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
    /// <summary>Native upward launch velocity of each Dragon fireball, in signed eight-eight units.</summary>
    private const ushort DragonFireballInitialYVelocity = unchecked((ushort)-961);
    /// <summary>Native leftward eight-eight fireball velocity.</summary>
    private const ushort DragonFireballLeftXVelocity = unchecked((ushort)-704);
    /// <summary>Native rightward eight-eight fireball velocity.</summary>
    private const ushort DragonFireballRightXVelocity = 704;
    /// <summary>Eight-eight downward acceleration applied to a live fireball.</summary>
    private const ushort DragonFireballGravity = 32;

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
            projectile.InstructionTimer = NativeCountdown.Start(1);
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
