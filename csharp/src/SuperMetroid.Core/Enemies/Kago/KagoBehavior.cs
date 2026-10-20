using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The Kago species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class KagoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kago];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KagoEntity(slot, common);

    /// <summary>Provides the room random word sampled by bug spawns and jumps without advancing the generator.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Kago bugs from the ordinary enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Receives the short quake requested by each shell hit.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Resolves a bug's native parent index when enabling shot collision and homing jumps.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Creates the parent-header drop requested by a shot Kago bug.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Tracks the Kago death request in the room's defeated-enemy count.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Binds shell AI and bug projectile behavior to the shared room systems that own their effects.</summary>
    internal KagoBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemySlotTable slotTable, EnemyDeathsAndDrops deathsAndDrops, EnemyRoomStatus roomStatus)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _slotTable = slotTable;
        _deathsAndDrops = deathsAndDrops;
        _roomStatus = roomStatus;
    }

    /// <summary>Ports <c>Kago_Init</c> at <c>$A8:AB46</c>.</summary>
    internal static void InitializeKago(RoomEnemySlot slot)
    {
        var state = new KagoEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Kago's population word already carries the other authored property bits. Native
        // init ORs only $2000 so the common instruction interpreter animates the shell.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<KagoProgram>.Start(KagoProgram.Slow);
        state.Function = KagoEnemyFunction.InstallNoOp;
        state.DeathAnimationStarted = false;
        state.HitCounter = NativeCountdown.FromRaw(checked((ushort)slot.Spawn.Population.Parameter1));
    }

    /// <summary>Ports <c>Kago_Main</c> and its one useful indirect target.</summary>
    private static void RunKagoMain(KagoEnemyState state)
    {
        switch (state.Function)
        {
            case KagoEnemyFunction.InstallNoOp:
                state.Function = KagoEnemyFunction.NoOp;
                return;
            case KagoEnemyFunction.NoOp:
                return;
            default:
                throw new InvalidDataException(
                    $"Kago main function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Ports the Kago-owned tail of <c>Kago_Shot</c> at <c>$A8:AB83</c>. The caller has
    /// already executed common normal-enemy shot AI, exactly matching the leading JSR in
    /// the cartridge routine.
    /// </summary>
    internal void ResolveKagoShotAfterCommon(RoomEnemySlot slot, KagoEnemyState state)
    {
        _earthquake.EarthquakeType = 2;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(16);

        if (!state.UsesFastAnimation)
        {
            state.UsesFastAnimation = true;
            state.Program = AnimationProgramCursor<KagoProgram>.Start(KagoProgram.Fast);
            slot.InstructionTimer = NativeCountdown.Start(1);
        }

        state.HitCounter = state.HitCounter.Decrement().Remaining;
        if (state.HitCounter.IsNegative)
        {
            // Common shot AI can theoretically kill the 1600-health shell first with a
            // debug-edited projectile. Do not count the same native death request twice.
            if (!slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                _roomStatus.RecordKill();
            }
            state.DeathAnimationStarted = true;
        }

        // The ROM spawns even on the lethal eleventh callback, after requesting Kago's
        // death animation. Pool exhaustion is silent and therefore does not roll back the
        // counter, earthquake, or animation transition.
        if (SpawnKagoBug(slot))
            state.SpawnedBugCount++;
    }

    /// <summary>Routes bomb hits through common shot AI before applying Kago's quake, counter, and bug spawn.</summary>
    internal void RegisterKagoBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Kago &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.KagoShot)
        {
            AfterCommon = c => ResolveKagoShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<KagoEnemyState>()),
        });
    }

    /// <summary>Connects the bug's idle, rising, falling, jump, landing, and drop handlers to projectile dispatch.</summary>
    internal void RegisterKagoProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunKagoBugIdle(context.Projectile);
        }, EnemyProjectilePreInstruction.KagoBug_Idle);

        table.RegisterPreInstruction(context =>
        {
            RunKagoBugJumping(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KagoBug_Jumping);

        table.RegisterPreInstruction(context =>
        {
            RunKagoBugFalling(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KagoBug_Falling);

        table.RegisterCue<KagoBugProjectileCue>((context, cue) => ApplyKagoBugCue(context.Projectile, cue));
        // A shot bug runs its typed shot program, the list its definition names.
        table.RegisterShotAnimation(RoomEnemyProjectileKind.KagoBug, () => new KagoBugProjectileAnimation(KagoBugProjectileProgram.Shot));
    }

    /// <summary>Carries out one cue of the bugs' programs.</summary>
    private void ApplyKagoBugCue(RoomEnemyProjectileSlot projectile, KagoBugProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case KagoBugProjectileCueKind.StartJump:
                StartKagoBugJump(projectile);
                break;
            case KagoBugProjectileCueKind.StartIdle:
                StartKagoBugIdle(projectile);
                break;
            case KagoBugProjectileCueKind.SpawnDrop:
                RequestKagoBugDrop(projectile);
                break;
            case KagoBugProjectileCueKind.UsePaletteZero:
                projectile.GraphicsIndex = 0;
                break;
            case KagoBugProjectileCueKind.Delete:
                projectile.Clear();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(KagoBugProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Advances Kago's typed animation.</summary>
    internal static void AdvanceKagoTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        KagoEnemyState state = slot.RequireSpeciesState<KagoEnemyState>();
        AnimationProgramCursor<KagoProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, KagoAnimations.Programs, ref cursor, KagoFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Ports Kago's main AI.</summary>
    internal static void RunKagoMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunKagoMain(slot.RequireSpeciesState<KagoEnemyState>());
    }

    /// <summary>Applies Kago's shell response after the common projectile-shot callback has resolved damage.</summary>
    internal void RegisterKagoShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Kago &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.KagoShot)
        {
            AfterCommon = c => ResolveKagoShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<KagoEnemyState>()),
        });
    }
    /// <summary>Signed eight-eight acceleration added to a bug's vertical velocity each unobstructed airborne frame.</summary>
    private const ushort KagoBugGravity = 224;
    /// <summary>Signed eight-eight horizontal jump speed, negated for leftward launches.</summary>
    private const ushort KagoBugHorizontalSpeed = 512;
    /// <summary>Horizontal parent separation that enables the bug to block Samus's projectiles.</summary>
    private const ushort KagoBugSourceCollisionEnableDistance = 23;
    /// <summary>Beyond this parent separation, the next jump homes toward the shell.</summary>
    private const ushort KagoBugSourceHomingDistance = 48;

    /// <summary>Last library-two Kago bug sound request produced during this enemy frame.</summary>
    public SoundEffectId? LastKagoBugSoundEffect { get; private set; }

    /// <summary>Ports <c>EprojInit_KagosBugs</c> at <c>$86:D088</c>.</summary>
    internal bool SpawnKagoBug(RoomEnemySlot source)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.KagoBug,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));

        projectile.Variable1 = source.NativeIndex;
        projectile.Position = projectile.Position.WithWhole(source.Position.Whole);

        // Kago samples the existing random word and never advances it. SpawnEprojInner has
        // already cleared subpositions, velocities, flags, and collision scratch for us.
        ushort initialIdleTimer = unchecked((ushort)((ReadKagoRandomNumber() & 7) + 1));
        projectile.CollidedProjectileType = initialIdleTimer;
        projectile.Variable0 = unchecked((ushort)(initialIdleTimer + 4));
        projectile.PreInstruction =
            EnemyProjectilePreInstruction.KagoBug_Idle;
        projectile.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        return true;
    }

    /// <summary>Ports the library-two sound countdown shared by every live bug state.</summary>
    private void StepKagoBugSound(RoomEnemyProjectileSlot projectile)
    {
        NativeCountdown soundTimer = NativeCountdown.FromRaw(checked((ushort)projectile.Variable0));
        if (soundTimer.IsZero)
            return;

        soundTimer = soundTimer.DecrementToZero();
        projectile.Variable0 = soundTimer.RawValue;
        if (soundTimer.IsZero)
            LastKagoBugSoundEffect = EnemySoundEffectDefinitions.KagoBugSoundEffect;
    }

    /// <summary>
    /// Sets property $8000 once the bug is at least 23 pixels from its source Kago. That
    /// delay prevents the shot which opened the shell from immediately destroying the bug.
    /// </summary>
    private void EnableKagoBugShotCollisionWhenSeparated(RoomEnemyProjectileSlot projectile)
    {
        RoomEnemySlot source = _slotTable.SlotFromNativeIndex(checked((ushort)projectile.Variable1));
        int distance = Math.Abs(unchecked((short)(source.Position.X.Whole - projectile.Position.X.Whole)));
        if (distance >= KagoBugSourceCollisionEnableDistance)
            projectile.BlocksSamusProjectiles = true;
    }

    /// <summary>Ports idle pre-instruction <c>$86:D0CA</c>.</summary>
    internal void RunKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        // The transition occurs only when the counter was already zero on entry. A value
        // of one therefore decrements to zero and remains idle until the following frame.
        if (projectile.CollidedProjectileType != 0)
        {
            projectile.CollidedProjectileType = NativeWordCounter.Decrement(checked((ushort)projectile.CollidedProjectileType)).Value;
            return;
        }

        projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileProgram.JumpStart));
        projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_86D0EB;
    }

    /// <summary>Ports airborne rising pre-instruction <c>$86:D0EC</c>.</summary>
    internal void RunKagoBugJumping(RoomEnemyProjectileSlot projectile, RoomLevelData level)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            // A wall ends the rising phase immediately in the cartridge: both X velocity
            // and the upward component are replaced before selecting the falling list.
            projectile.Velocity = new(new SnesSignedEightEight(0), SnesSignedEightEight.FromSignedRaw(256));
            BeginKagoBugFall(projectile);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Velocity = projectile.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(256));
            BeginKagoBugFall(projectile);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + KagoBugGravity))));
        if (unchecked((short)projectile.Velocity.Y.RawValue) >= 0)
            BeginKagoBugFall(projectile);
    }

    /// <summary>Ports falling pre-instruction <c>$86:D128</c>.</summary>
    internal void RunKagoBugFalling(RoomEnemyProjectileSlot projectile, RoomLevelData level)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        // The native routine is an if/else-if chain. A wall consumes this frame, zeros X,
        // and deliberately postpones both vertical motion and gravity until the next frame.
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_86D0EB;
            projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileProgram.Landed));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + KagoBugGravity))));
    }

    /// <summary>Switches an airborne bug to the falling list when rising ends or a wall interrupts it.</summary>
    private static void BeginKagoBugFall(RoomEnemyProjectileSlot projectile)
    {
        projectile.PreInstruction =
            EnemyProjectilePreInstruction.KagoBug_Falling;
        projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileProgram.Falling));
    }

    /// <summary>Ports instruction <c>$86:D15C</c>, including its unusual RNG reuse.</summary>
    internal void StartKagoBugJump(RoomEnemyProjectileSlot projectile)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        ushort upwardMagnitude = unchecked((ushort)((ReadKagoRandomNumber() & KagoDefinitions.JumpHeightRandomBits) + 2048));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-upwardMagnitude)));

        RoomEnemySlot source = _slotTable.SlotFromNativeIndex(checked((ushort)projectile.Variable1));
        short sourceDelta = unchecked((short)(source.Position.X.Whole - projectile.Position.X.Whole));
        bool jumpLeft;
        if (Math.Abs((int)sourceDelta) >= KagoBugSourceHomingDistance)
        {
            // Outside the 48-pixel band the bug always jumps back toward its parent shell.
            jumpLeft = sourceDelta < 0;
        }
        else
        {
            // Inside that band the cartridge reuses bit $0100 of the chosen vertical speed
            // as a direction bit instead of requesting a second random sample.
            jumpLeft = (upwardMagnitude & KagoDefinitions.LeftJumpBit) != 0;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(jumpLeft
            ? unchecked((ushort)-KagoBugHorizontalSpeed)
            : KagoBugHorizontalSpeed));
        projectile.PreInstruction =
            EnemyProjectilePreInstruction.KagoBug_Jumping;
    }

    /// <summary>Ports landed-list instruction <c>$86:D1B6</c>.</summary>
    internal void StartKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        projectile.CollidedProjectileType = unchecked((ushort)(
            (ReadKagoRandomNumber() & 31) + 1));
        projectile.PreInstruction =
            EnemyProjectilePreInstruction.KagoBug_Idle;
    }

    /// <summary>Ports shot-list drop instruction <c>$86:D1CE</c>.</summary>
    internal void RequestKagoBugDrop(RoomEnemyProjectileSlot projectile)
    {
        _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
            projectile.Position.Whole,
            EnemyDefinitionId.Kago);
    }

    /// <summary>Samples the shared random word without consuming a new value, matching the bug routines' seed reuse.</summary>
    private int ReadKagoRandomNumber() =>
        _room.Random.Read();

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastKagoBugSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastKagoBugSoundEffect, maximumQueued: 6);
    }
}
