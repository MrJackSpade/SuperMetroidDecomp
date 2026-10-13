using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Mini-Kraid species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FakeKraidBehavior : IEnemySpecies
{
    /// <summary>Provides the existing random word sampled for Mini-Kraid's action decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Checks the walk opcode's movement against room collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates spit, spike, and death effects from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Runs common Samus contact damage before Mini-Kraid's specialized death.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns death animation three and the four-pickup scatter after lethal hits.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects Mini-Kraid's walk, projectiles, and specialized death to room services.</summary>
    internal FakeKraidBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Executes Mini-Kraid's private animation instructions.</summary>
    private bool TryProcessFakeKraidInstruction(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level, ushort word, ref ushort cursor, Position16<RoomPixels> camera)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.MiniKraid ||
            !Enum.IsDefined((FakeKraidInstruction)word))
            return false;

        switch ((FakeKraidInstruction)word)
        {
            case FakeKraidInstruction.Move:
                // Walk opcode: advance one four-pixel step unless its random reversal
                // clock expires, then refresh the live facing marker from Samus.
                ProcessFakeKraidWalkInstruction(slot, RequireFakeKraidState(slot), samus, level);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FakeKraidInstruction.ChooseAction:
                // Decision opcode returns a direct same-bank address. Several targets
                // deliberately begin two bytes inside a named list to skip this opcode.
                cursor = SelectFakeKraidInstruction(RequireFakeKraidState(slot));
                return true;
            case FakeKraidInstruction.PlayCrySFX:
                // `$A6:9BB2` queues sound $16 only while the actor origin is inside the
                // inclusive 256x256 native screen rectangle.
                if (FakeKraidOriginIsOnScreen(slot, camera))
                    LastFakeKraidSoundEffect = FakeKraidSpitSound;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FakeKraidInstruction.FireSpitLeft:
                SpawnFakeKraidSpitPair(slot, RequireFakeKraidState(slot), movingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case FakeKraidInstruction.FireSpitRight:
                SpawnFakeKraidSpitPair(slot, RequireFakeKraidState(slot), movingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Mini-Kraid does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Library-two cry requested only when Mini-Kraid's origin is on screen.</summary>
    private const ushort FakeKraidSpitSound = 0x0016;
    /// <summary>Library-two cue used by Mini-Kraid's spike launch.</summary>
    private const ushort FakeKraidSpikeSound = 0x003f;

    /// <summary>Per-slot movement direction, spit timer, and spawned-projectile counts.</summary>
    internal readonly FakeKraidEnemyState?[] _fakeKraidStates =
        new FakeKraidEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two sound requested by Fake Kraid during this enemy frame.</summary>
    public ushort? LastFakeKraidSoundEffect { get; private set; }

    /// <summary>Last specialized Mini-Kraid death/drop request.</summary>
    public FakeKraidDropRequest? LastFakeKraidDropRequest { get; private set; }

    /// <summary>Ports <c>FakeKraid_Init</c> at <c>$A6:9A58</c>.</summary>
    internal void InitializeFakeKraid(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Fake Kraid initialization requires the active Samus position.");
        }

        var state = new FakeKraidEnemyState(slot);
        _fakeKraidStates[slot.SlotIndex] = state;

        // The routine reads the already-published global random word; it does not advance
        // the RNG. Use the read callback when available and reserve NextRandom for older
        // isolated callers that did not expose that native seam.
        ushort random = ReadFakeKraidRandomNumber();
        ushort seedDelay = unchecked((ushort)((random & 3) + 2));
        state.WalkStepTimer = seedDelay;
        state.SpitDecisionTimer = NativeCountdown.FromRaw(seedDelay);
        state.SpikeTimers[0] = unchecked((ushort)(seedDelay + 64));
        state.SpikeTimers[1] = unchecked((ushort)(seedDelay + 96));
        state.SpikeTimers[2] = unchecked((ushort)(seedDelay + 48));
        state.SpikeTimerByteOffset = 0;

        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.WalkDelta = -4;
        state.FacingDelta = -4;
        slot.CurrentInstruction =
            FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft;
        if (unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0)
        {
            state.WalkDelta = 4;
            state.FacingDelta = 4;
            slot.CurrentInstruction =
                FakeKraidInstructionProgramDefinitions.StepForwardsFacingRight;
        }
    }

    /// <summary>Ports <c>FakeKraid_Main</c> at <c>$A6:9AC2</c>.</summary>
    private void RunFakeKraidMain(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Main AI stores the next selector before acting on the old value. Because the old
        // value is passed as a byte offset into scratch RAM, offsets zero/two/four alias the
        // three adjacent clocks above rather than three enemy slots.
        ushort selectedByteOffset = state.SpikeTimerByteOffset;
        ushort nextByteOffset = unchecked((ushort)(selectedByteOffset + 2));
        if (selectedByteOffset >= 4)
            nextByteOffset = 0;
        state.SpikeTimerByteOffset = nextByteOffset;

        int timerIndex = selectedByteOffset >> 1;
        ushort timer = state.SpikeTimers[timerIndex];
        if (timer != 0)
        {
            state.SpikeTimers[timerIndex] = unchecked((ushort)(timer - 1));
            return;
        }

        state.SpikeTimers[timerIndex] = unchecked((ushort)(
            (ReadFakeKraidRandomNumber() & 0x003f) + 16));
        state.LastSpikeRow = timerIndex;
        if (SpawnFakeKraidSpike(slot, state, timerIndex) &&
            FakeKraidOriginIsOnScreen(slot, camera))
        {
            LastFakeKraidSoundEffect = FakeKraidSpikeSound;
        }
    }

    /// <summary>Ports animation opcode <c>$A6:9B26</c>.</summary>
    private void ProcessFakeKraidWalkInstruction(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null || level is null)
        {
            throw new InvalidOperationException(
                "Fake Kraid walk bytecode requires Samus and room collision data.");
        }

        state.SpitDecisionTimer = state.SpitDecisionTimer.DecrementToZero();

        // C's post-decrement comparison is exact here: when the old value is one, reload
        // seven through ten and reverse without moving. Otherwise attempt the signed four-
        // pixel 16.16 displacement and reverse only if the room collision helper reports a
        // wall. A wrapped zero timer therefore still performs the attempted step.
        ushort oldWalkTimer = state.WalkStepTimer;
        state.WalkStepTimer = unchecked((ushort)(oldWalkTimer - 1));
        bool reverse = oldWalkTimer == 1;
        if (reverse)
        {
            state.WalkStepTimer = unchecked((ushort)(
                (ReadFakeKraidRandomNumber() & 3) + 7));
        }
        else
        {
            int displacement = SnesSixteenSixteen.FromWhole(state.WalkDelta);
            reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement);
        }

        if (reverse)
            state.WalkDelta = unchecked((short)-state.WalkDelta);

        state.FacingDelta = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
            ? (short)4
            : (short)-4;
    }

    /// <summary>Ports animation opcode <c>$A6:9B74</c> and returns its direct list target.</summary>
    private ushort SelectFakeKraidInstruction(
        FakeKraidEnemyState state)
    {
        if (!state.SpitDecisionTimer.IsZero)
        {
            if (state.FacingDelta < 0)
            {
                return state.WalkDelta >= 0
                    ? FakeKraidInstructionProgramDefinitions.StepBackwardsFacingLeft
                    : FakeKraidInstructionProgramDefinitions.StepForwardsFacingLeft;
            }

            return state.WalkDelta < 0
                ? FakeKraidInstructionProgramDefinitions.StepBackwardsFacingRight
                : FakeKraidInstructionProgramDefinitions.StepForwardsFacingRight;
        }

        state.SpitDecisionTimer = NativeCountdown.FromRaw(unchecked((ushort)(
            (ReadFakeKraidRandomNumber() & 3) + 3)));
        return state.FacingDelta < 0
            ? FakeKraidInstructionProgramDefinitions.FireSpitFacingLeft
            : FakeKraidInstructionProgramDefinitions.FireSpitFacingRight;
    }

    /// <summary>Ports animation opcodes <c>$A6:9BC4</c>/<c>$A6:9C02</c>.</summary>
    internal void SpawnFakeKraidSpitPair(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        bool movingRight)
    {
        short xOffset = movingRight ? (short)4 : (short)-4;
        for (int projectile = 0; projectile < 2; projectile++)
        {
            EightEightVector<RoomPixels> launch =
                FakeKraidProjectileDefinitions.SpitLaunch(movingRight, projectile);
            if (SpawnFakeKraidSpit(
                    slot,
                    xOffset,
                    launch))
                state.SpawnedSpitCount++;
        }
    }

    /// <summary>Allocates one spit projectile above and just ahead of Mini-Kraid's body with its authored velocity.</summary>
    /// <returns>False when the enemy projectile pool is full.</returns>
    internal bool SpawnFakeKraidSpit(
        RoomEnemySlot source,
        short xOffset,
        EightEightVector<RoomPixels> velocity)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FakeKraidSpit,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + new Offset16<RoomPixels>(xOffset, -16));
        projectile.Velocity = velocity;
        return true;
    }

    /// <summary>Allocates a facing-specific spike at an authored body row and counts only successful spawns.</summary>
    /// <returns>False when the enemy projectile pool is full.</returns>
    internal bool SpawnFakeKraidSpike(
        RoomEnemySlot source,
        FakeKraidEnemyState state,
        int row)
    {
        RoomEnemyProjectileKind kind = state.FacingDelta < 0
            ? RoomEnemyProjectileKind.FakeKraidSpikeLeft
            : RoomEnemyProjectileKind.FakeKraidSpikeRight;
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        short yOffset = FakeKraidProjectileDefinitions.SpikeYOffset(row);
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(new(source.Position.X.Whole, unchecked((ushort)(source.Position.Y.Whole + yOffset))));
        projectile.Velocity = new(new SnesSignedEightEight(kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? unchecked((ushort)-0x0200)
            : (ushort)0x0200), new SnesSignedEightEight(0));
        state.SpawnedSpikeCount++;
        return true;
    }

    /// <summary>Ports <c>EprojPreInit_MiniKraidSpit</c> at <c>$86:9E1E</c>.</summary>
    internal static void RunFakeKraidSpitPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
            return;
        }

        short oldVelocity = unchecked((short)projectile.Velocity.Y.RawValue);
        short nextVelocity = unchecked((short)(oldVelocity + 64));
        if (nextVelocity >= 0 && oldVelocity >= 960)
            nextVelocity = 1024;
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)nextVelocity)));
    }

    /// <summary>Ports <c>EprojPreInstr_MiniKraidSpikes</c> at <c>$86:9E83</c>.</summary>
    internal static void RunFakeKraidSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true))
            projectile.Clear();
    }

    /// <summary>Samples the room random word without advancing it for the native action choice.</summary>
    private ushort ReadFakeKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>Tests the actor origin against the inclusive native 256-pixel camera rectangle for its cry.</summary>
    private static bool FakeKraidOriginIsOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(slot.Position.X.Whole - camera.X)) >= 0 &&
        unchecked((short)(camera.X + 256 - slot.Position.X.Whole)) >= 0 &&
        unchecked((short)(slot.Position.Y.Whole - camera.Y)) >= 0 &&
        unchecked((short)(camera.Y + 256 - slot.Position.Y.Whole)) >= 0;

    /// <summary>Rejects a callback for a Mini-Kraid slot whose species state has not initialized.</summary>
    private FakeKraidEnemyState RequireFakeKraidState(RoomEnemySlot slot) =>
        _fakeKraidStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Fake Kraid state.");

    /// <summary>
    /// Ports <c>Reaction_MiniKraid_Common</c> ($A6:9C50): <c>EnemyDeath</c> spawns death
    /// explosion three and clears the slot, then <c>MiniKraidDeathItemDropRoutine</c>
    /// ($A0:B8EE) scatters four pickups from the coordinates saved before the shot AI.
    /// The explosion therefore takes the first free projectile slot.
    /// </summary>
    private void KillFakeKraid(RoomEnemySlot slot)
    {
        Position16<RoomPixels> origin = slot.Position.Whole;
        ushort chancePointer = slot.Definition.ItemDropChancesPointer;
        _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: 3);
        SpawnFakeKraidDeathDrops(origin);
    }

    /// <summary>Scatters four Mini-Kraid pickups around the position saved before death clears its slot.</summary>
    private void SpawnFakeKraidDeathDrops(Position16<RoomPixels> origin)
    {
        LastFakeKraidDropRequest = new FakeKraidDropRequest();
        _deathsAndDrops.SpawnEnemyDropScatterAround(
            EnemyDefinitionId.MiniKraid,
            count: 4,
            origin);
    }

    /// <summary>Replaces generic bomb death with Mini-Kraid's explosion-three and four-drop sequence.</summary>
    internal void RegisterFakeKraidBombReactions(EnemyAiTable table)
    {
        // Fake Kraid saves its coordinates before common no-death AI, then requests death variant three and the
        // Mini-Kraid drop. Its definition never runs the generic death, whatever callback a hitbox selects.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid)
        {
            RunsGenericDeath = false,
            AfterCommon = c =>
            {
                if (c.SelectedShotAi == EnemyInteractionCallback.FakeKraidShot && c.HealthBefore != 0 && c.Enemy.Health == 0)
                    KillFakeKraid(c.Enemy);
            },
        });
    }

    /// <summary>Dispatches spit gravity and spike wall collision through their projectile pre-instructions.</summary>
    internal static void RegisterFakeKraidProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunFakeKraidSpitPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.MiniKraidSpit);

        table.RegisterPreInstruction(context =>
        {
            RunFakeKraidSpikePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.MiniKraidSpikes);
    }

    /// <summary>Initializes Mini-Kraid's facing and action state from its populated enemy slot.</summary>
    internal void RegisterFakeKraidInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_MiniKraid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeFakeKraid(slot, samus);
        });
    }

    /// <summary>Runs the Mini-Kraid phase dispatcher with the current camera.</summary>
    internal void RegisterFakeKraidMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_MiniKraid, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid, (slot, frame, nmiFrameCounter8) =>
        {
            RunFakeKraidMain(
                slot,
                RequireFakeKraidState(slot),
                frame.Camera);
        });
    }

    /// <summary>Claims Mini-Kraid's walk, action choice, cry, and spit opcodes before common animation dispatch.</summary>
    internal void RegisterFakeKraidOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessFakeKraidInstruction(slot, context.Samus, context.Level, word, ref cursor, context.Camera);
        });
    }

    /// <summary>Spawns Mini-Kraid's own explosion and four drops after a lethal power bomb.</summary>
    internal void RegisterFakeKraidPowerBombReactions(EnemyAiTable table)
    {
        // Mini Kraid's private power-bomb callback still owns its four direct pickup explosions, spawned after its
        // death explosion.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.FakeKraidShot)
        {
            ReplacesGenericDeath = (enemy, _) => KillFakeKraid(enemy),
        });
    }

    /// <summary>Replaces ordinary projectile-shot death and preserves the pickup scatter on a lethal hit.</summary>
    internal void RegisterFakeKraidShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.FakeKraidShot)
        {
            ReplacesGenericDeath = c =>
            {
                KillFakeKraid(c.Enemy);
                return true;
            },
            AfterCommon = (c, froze) =>
            {
                if (!froze && c.Enemy.EnemyDefinitionPointer != 0 && c.HealthBefore != 0 && c.Enemy.Health == 0)
                    SpawnFakeKraidDeathDrops(c.Enemy.Position.Whole);
            },
        });
    }

    /// <summary>Runs common Samus contact and routes a resulting lethal hit to Mini-Kraid's death sequence.</summary>
    internal void RegisterFakeKraidTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.FakeKraidTouch,
            context =>
            {
                ushort healthBefore = context.Slot.Health;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
                if (healthBefore != 0 && context.Slot.Health == 0)
                    KillFakeKraid(context.Slot);
            });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastFakeKraidSoundEffect = null;
        LastFakeKraidDropRequest = null;
        Array.Clear(_fakeKraidStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastFakeKraidSoundEffect = null;
        LastFakeKraidDropRequest = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastFakeKraidSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
