using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The Kago species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class KagoBehavior : IEnemySpecies
{
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

    /// <summary>Holds shell hit counters and animation state at the native enemy slot indices.</summary>
    internal readonly KagoEnemyState?[] _kagoStates =
        new KagoEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>Kago_Init</c> at <c>$A8:AB46</c>.</summary>
    internal void InitializeKago(RoomEnemySlot slot)
    {
        var state = new KagoEnemyState(slot);
        _kagoStates[slot.SlotIndex] = state;

        // Kago's population word already carries the other authored property bits. Native
        // init ORs only $2000 so the common instruction interpreter animates the shell.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.VariableE = 0;
        state.Program = AnimationProgramCursor<KagoProgram>.Start(KagoProgram.Slow);
        state.Function = KagoEnemyFunction.InstallNoOp;
        state.DeathAnimationStarted = false;
        state.HitCounter = NativeCountdown.FromRaw(slot.Parameter1);
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
                    $"Kago main function $A8:{(ushort)state.Function:X4} is not translated.");
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
                _roomStatus.EnemiesKilled = unchecked((ushort)(_roomStatus.EnemiesKilled + 1));
            }
            state.DeathAnimationStarted = true;
        }

        // The ROM spawns even on the lethal eleventh callback, after requesting Kago's
        // death animation. Pool exhaustion is silent and therefore does not roll back the
        // counter, earthquake, or animation transition.
        if (SpawnKagoBug(slot))
            state.SpawnedBugCount++;
    }

    /// <summary>Rejects a shot or main callback that arrived before the shell initialized its slot state.</summary>
    private KagoEnemyState RequireKagoState(RoomEnemySlot slot) =>
        _kagoStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Kago state.");

    /// <summary>Routes bomb hits through common shot AI before applying Kago's quake, counter, and bug spawn.</summary>
    internal void RegisterKagoBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Kago &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.KagoShot)
        {
            AfterCommon = c => ResolveKagoShotAfterCommon(c.Enemy, RequireKagoState(c.Enemy)),
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

        table.RegisterInstruction(EnemyProjectileInstruction.KagoBugStartJumpInstruction, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                StartKagoBugJump(projectile);
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });

        table.RegisterInstruction(EnemyProjectileInstruction.KagoBugStartIdleInstruction, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                StartKagoBugIdle(projectile);
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });

        table.RegisterInstruction(EnemyProjectileInstruction.KagoBugSpawnDropInstruction, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                RequestKagoBugDrop(projectile);
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }

    /// <summary>Runs Kago's shell animation as a typed program (#641).</summary>
    internal void RegisterKagoTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kago, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            KagoEnemyState state = RequireKagoState(slot);
            AnimationProgramCursor<KagoProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, KagoAnimations.Programs, ref cursor, KagoFrameDefinitions.Spritemap);
            state.Program = cursor;
        });

    /// <summary>Runs Kago shell initialization only for slots bearing the Kago definition.</summary>
    internal void RegisterKagoInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Kago, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kago, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeKago(slot);
        });
    }

    /// <summary>Dispatches the shell's install-once and no-op main phases for Kago slots.</summary>
    internal void RegisterKagoMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Kago, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kago, (slot, frame, nmiFrameCounter8) =>
        {
            RunKagoMain(RequireKagoState(slot));
        });
    }

    /// <summary>Applies Kago's shell response after the common projectile-shot callback has resolved damage.</summary>
    internal void RegisterKagoShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Kago &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.KagoShot)
        {
            AfterCommon = (c, _) => ResolveKagoShotAfterCommon(c.Enemy, RequireKagoState(c.Enemy)),
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
    public ushort? LastKagoBugSoundEffect { get; private set; }

    /// <summary>Last Kago bug enemy-drop request produced during this enemy frame.</summary>
    public KagoBugDropRequest? LastKagoBugDropRequest { get; private set; }

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
            (ushort)EnemyProjectilePreInstruction.KagoBug_Idle;
        projectile.StartAnimation(new KraidRockAnimation(KraidRockProgram.SharedPose));
        return true;
    }

    /// <summary>Ports the library-two sound countdown shared by every live bug state.</summary>
    private void StepKagoBugSound(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.Variable0 == 0)
            return;

        projectile.Variable0 = unchecked((ushort)(projectile.Variable0 - 1));
        if (projectile.Variable0 == 0)
            LastKagoBugSoundEffect = EnemySoundEffectDefinitions.KagoBugSoundEffect;
    }

    /// <summary>
    /// Sets property $8000 once the bug is at least 23 pixels from its source Kago. That
    /// delay prevents the shot which opened the shell from immediately destroying the bug.
    /// </summary>
    private void EnableKagoBugShotCollisionWhenSeparated(RoomEnemyProjectileSlot projectile)
    {
        RoomEnemySlot source = _slotTable.SlotFromNativeIndex(projectile.Variable1);
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
            projectile.CollidedProjectileType = NativeWordCounter.Decrement(projectile.CollidedProjectileType).Value;
            return;
        }

        projectile.InstructionPointer =
            KagoBugProjectileInstructionProgramDefinitions.JumpStart;
        projectile.InstructionTimer = NativeCountdown.Start(1);
        projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86D0EB;
    }

    /// <summary>Ports airborne rising pre-instruction <c>$86:D0EC</c>.</summary>
    internal void RunKagoBugJumping(RoomEnemyProjectileSlot projectile, RoomLevelData level)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            // A wall ends the rising phase immediately in the cartridge: both X velocity
            // and the upward component are replaced before selecting the falling list.
            projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0x0100));
            BeginKagoBugFall(projectile);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(0x0100));
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
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
        {
            projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(0));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86D0EB;
            projectile.InstructionPointer =
                KagoBugProjectileInstructionProgramDefinitions.Landed;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + KagoBugGravity))));
    }

    /// <summary>Switches an airborne bug to the falling list when rising ends or a wall interrupts it.</summary>
    private static void BeginKagoBugFall(RoomEnemyProjectileSlot projectile)
    {
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Falling;
        projectile.InstructionPointer = KagoBugProjectileInstructionProgramDefinitions.Falling;
        projectile.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Ports instruction <c>$86:D15C</c>, including its unusual RNG reuse.</summary>
    internal void StartKagoBugJump(RoomEnemyProjectileSlot projectile)
    {
        StepKagoBugSound(projectile);
        EnableKagoBugShotCollisionWhenSeparated(projectile);

        ushort upwardMagnitude = unchecked((ushort)((ReadKagoRandomNumber() & 0x0300) + 2048));
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-upwardMagnitude)));

        RoomEnemySlot source = _slotTable.SlotFromNativeIndex(projectile.Variable1);
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
            jumpLeft = (upwardMagnitude & 0x0100) != 0;
        }

        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(jumpLeft
            ? unchecked((ushort)-KagoBugHorizontalSpeed)
            : KagoBugHorizontalSpeed));
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Jumping;
    }

    /// <summary>Ports landed-list instruction <c>$86:D1B6</c>.</summary>
    internal void StartKagoBugIdle(RoomEnemyProjectileSlot projectile)
    {
        projectile.CollidedProjectileType = unchecked((ushort)(
            (ReadKagoRandomNumber() & 0x001f) + 1));
        projectile.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.KagoBug_Idle;
    }

    /// <summary>Ports shot-list drop instruction <c>$86:D1CE</c>.</summary>
    internal void RequestKagoBugDrop(RoomEnemyProjectileSlot projectile)
    {
        LastKagoBugDropRequest = new KagoBugDropRequest();
        _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
            projectile.Position.Whole,
            EnemyDefinitionId.Kago);
    }

    /// <summary>Samples the shared random word without consuming a new value, matching the bug routines' seed reuse.</summary>
    private ushort ReadKagoRandomNumber() =>
        _room.Random.Read();

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastKagoBugSoundEffect = null;
        LastKagoBugDropRequest = null;
        Array.Clear(_kagoStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastKagoBugSoundEffect = null;
        LastKagoBugDropRequest = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastKagoBugSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
