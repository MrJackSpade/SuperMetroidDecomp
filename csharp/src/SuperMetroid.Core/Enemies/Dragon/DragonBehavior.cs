using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>The Dragon species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DragonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dragon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DragonEntity(this, slot, common);

    /// <summary>Allocates Dragon's three-shot fireball volleys from the room projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Resolves the cosmetic wing in the physical enemy slot following its body.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Performs common Samus touch resolution before Dragon's body and wing synchronization.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Receives each fireball's sound in call order, as the cartridge queues it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Dragon body AI, paired-wing state, fireball spawning and its sound to room systems.</summary>
    internal DragonBehavior(EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision,
        EnemyAudioQueue audio)
    {
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
        _audio = audio;
    }

    /// <summary>Carries out Dragon's animation cue: the attack body marks its animation complete for main AI.</summary>
    private static DragonProgram? ApplyDragonCue(RoomEnemySlot slot, DragonCue cue)
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


    /// <summary>Ports <c>InitAI_Dragon</c> at $A2:E606.</summary>
    internal static void InitializeDragon(RoomEnemySlot slot)
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
                    $"Dragon function {state.Function?.ToString() ?? "none"} is not translated.");
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
        DragonEnemyState wingState = wing.RequireSpeciesState<DragonEnemyState>();
        bodyState.FunctionTimer = NativeCountdown.Start(DragonRiseOrSinkFrames);
        bodyState.Function = DragonEnemyFunction.Rising;

        // $E666-$E672 records the sign of (Samus X - body X) as the body's facing. The following byte-sized list
        // edits use that sign to select even/odd left/right table entries for both physical records.
        bool facingLeft = unchecked((short)(samus.Position.X.Whole - body.Position.X.Whole)) < 0;
        bodyState.FacingLeft = facingLeft;
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
        // $A2:E70F queues one sound per fireball; two Dragons attacking in one frame queue two.
        _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2,
            EnemySoundEffectDefinitions.DragonFireballSound), maximumQueued: 6);
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
    private static void FinishDragonAttackAnimation(RoomEnemySlot body)
    {
        DragonEnemyState state = body.RequireSpeciesState<DragonEnemyState>();
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
        RoomEnemySlot following = _slotTable.Slots[actor.SlotIndex + 1];
        if (actor.Health == 0)
        {
            following.Properties = following.Properties.With(EnemyProperties.Deleted);
            return;
        }

        following.ShakeTimer = actor.ShakeTimer;
        following.InvincibilityTimer = actor.InvincibilityTimer;
        following.FlashTimer = actor.FlashTimer;
        following.FrozenTimer = actor.FrozenTimer;
        following.AiStates = actor.AiStates;
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

        RoomEnemySlot wing = _slotTable.Slots[body.SlotIndex + 1];
        if (body.Spawn.Population.Parameter1 != 0 || wing.EnemyDefinitionPointer != EnemyDefinitionId.Dragon ||
            wing.Spawn.Population.Parameter1 == 0)
        {
            throw new InvalidDataException(
                $"Dragon body slot {body.SlotIndex} is not followed by its retail wing record.");
        }
        return wing;
    }

    /// <summary>Creates one Dragon slot's state, selecting body or wing from population parameter one.</summary>
    private static DragonEnemyState CreateDragonState(RoomEnemySlot slot)
    {
        var state = new DragonEnemyState { IsWing = slot.Spawn.Population.Parameter1 != 0 };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        return state;
    }

    /// <summary>Applies Dragon's paired-body combat tail after the common bomb-as-shot callback.</summary>
    internal void RegisterDragonBombReactions(EnemySharedReactions table)
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

    /// <summary>Ports Dragon's initialization AI.</summary>
    internal static void RunDragonInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeDragon(slot);
    }

    /// <summary>Ports Dragon's main AI.</summary>
    internal void RunDragonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDragonMain(slot, slot.RequireSpeciesState<DragonEnemyState>(), frame.Samus);
    }

    /// <summary>Advances Dragon's typed animation.</summary>
    internal static void AdvanceDragonTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, DragonAnimations.Programs, DragonFrameDefinitions.ShownFrames.Of,
            static (DragonEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyDragonCue(slot, cue));

    /// <summary>Synchronizes wing and body state after a Dragon power-bomb reaction.</summary>
    internal void RegisterDragonPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DragonPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveDragonCombatAfterCommon(enemy),
        });
    }

    /// <summary>Synchronizes wing and body state after common projectile damage resolution.</summary>
    internal void RegisterDragonShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DragonShot)
        {
            AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs ordinary enemy touch damage before mirroring Dragon's hurt and freeze state into its wing.</summary>
    internal void RegisterDragonTouch(EnemySharedReactions table)
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

        if (bodyState.FacingLeft)
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole - 12)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(DragonFireballLeftXVelocity));
            projectile.InstallAnimation(new DragonFireballAnimation(DragonFireballProgram.RisingLeft));
        }
        else
        {
            projectile.Position = projectile.Position.WithWholeX(unchecked((ushort)(body.Position.X.Whole + 12)));
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(DragonFireballRightXVelocity));
            projectile.InstallAnimation(new DragonFireballAnimation(DragonFireballProgram.RisingRight));
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
            projectile.StartAnimation(new DragonFireballAnimation(unchecked((short)projectile.Velocity.X.RawValue) < 0
                ? DragonFireballProgram.FallingLeft
                : DragonFireballProgram.FallingRight));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            projectile.Velocity.Y.RawValue + DragonFireballGravity))));

        // $86:B5B9 retains fireballs above the viewport; horizontal distance is never consulted.
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Dragon keeps no per-frame publication; its sounds are queued as they happen.
    }
}
