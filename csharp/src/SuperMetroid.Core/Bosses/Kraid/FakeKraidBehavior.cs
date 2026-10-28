using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Mini-Kraid species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FakeKraidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MiniKraid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FakeKraidEntity(this, slot, common);

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

    /// <summary>
    /// Carries out one cue of Mini-Kraid's animations. The action choice continues at the step or spit animation it
    /// selects, as the native opcode returns that list; every other cue runs on.
    /// </summary>
    private FakeKraidProgram? ApplyFakeKraidCue(RoomEnemySlot slot, FakeKraidCue cue, SamusState? samus, RoomLevelData? level, Position16<RoomPixels> camera)
    {
        switch (cue.Kind)
        {
            case FakeKraidCueKind.Move:
                // Walk opcode: advance one four-pixel step unless its random reversal
                // clock expires, then refresh the live facing marker from Samus.
                ProcessFakeKraidWalkInstruction(slot, slot.RequireSpeciesState<FakeKraidEnemyState>(), samus, level);
                return null;
            case FakeKraidCueKind.ChooseAction:
                return SelectFakeKraidAction(slot.RequireSpeciesState<FakeKraidEnemyState>());
            case FakeKraidCueKind.PlayCrySound:
                // `$A6:9BB2` queues sound $16 only while the actor origin is inside the
                // inclusive 256x256 native screen rectangle.
                if (FakeKraidOriginIsOnScreen(slot, camera))
                    LastFakeKraidSoundEffect = KraidSoundDefinitions.MiniKraidCry;
                return null;
            case FakeKraidCueKind.SpitLeft:
                SpawnFakeKraidSpitPair(slot, slot.RequireSpeciesState<FakeKraidEnemyState>(), direction: HorizontalDirection.Left);
                return null;
            case FakeKraidCueKind.SpitRight:
                SpawnFakeKraidSpitPair(slot, slot.RequireSpeciesState<FakeKraidEnemyState>(), direction: HorizontalDirection.Right);
                return null;
            default:
                throw new InvalidOperationException($"Mini-Kraid's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Fewest frames a spike clock waits after it fires before it can fire again.</summary>
    private const int MinimumSpikeReloadFrames = 16;

    /// <summary>Number of distinct random extra frames (zero through sixty-three) added to a spike clock's reload.</summary>
    private const int SpikeReloadRandomSpread = 64;

    /// <summary>Whole pixels per frame a body spike travels horizontally, toward the side Mini-Kraid faces.</summary>
    private const int SpikePixelsPerFrame = 2;

    /// <summary>Sub-pixel steps in one whole pixel of a projectile's 8.8 velocity.</summary>
    private const int SubpixelsPerPixel = 256;

    /// <summary>Last sound requested by Fake Kraid during this enemy frame, or null when it requested none.</summary>
    public SoundEffectId? LastFakeKraidSoundEffect { get; private set; }


    /// <summary>Ports <c>FakeKraid_Init</c> at <c>$A6:9A58</c>.</summary>
    internal void InitializeFakeKraid(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Fake Kraid initialization requires the active Samus position.");
        }

        var state = new FakeKraidEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The routine reads the already-published global random word; it does not advance
        // the RNG. Use the read callback when available and reserve NextRandom for older
        // isolated callers that did not expose that native seam.
        int random = ReadFakeKraidRandomNumber();
        int seedDelay = (random & 3) + 2;
        state.WalkStepTimer = WrappingCountdown.FromRaw(seedDelay);
        state.SpitDecisionTimer = WrappingCountdown.FromRaw(seedDelay);
        state.SpikeTimers[0] = WrappingCountdown.Start(seedDelay + 64);
        state.SpikeTimers[1] = WrappingCountdown.Start(seedDelay + 96);
        state.SpikeTimers[2] = WrappingCountdown.Start(seedDelay + 48);
        state.SpikeTimerIndex = 0;

        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.WalkDelta = -4;
        state.FacingDelta = -4;
        state.Program = AnimationProgramCursor<FakeKraidProgram>.Start(FakeKraidProgram.StepForwardsFacingLeft);
        if (unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0)
        {
            state.WalkDelta = 4;
            state.FacingDelta = 4;
            state.Program = AnimationProgramCursor<FakeKraidProgram>.Start(FakeKraidProgram.StepForwardsFacingRight);
        }
    }

    /// <summary>Ports <c>FakeKraid_Main</c> at <c>$A6:9AC2</c>.</summary>
    private void RunFakeKraidMain(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Main AI stores the next selector before acting on the old value. The native byte offsets zero/two/four
        // into scratch RAM alias the three adjacent clocks above rather than three enemy slots.
        int timerIndex = state.SpikeTimerIndex;
        state.SpikeTimerIndex = timerIndex >= state.SpikeTimers.Length - 1 ? 0 : timerIndex + 1;
        WrappingCountdown timer = state.SpikeTimers[timerIndex];
        if (!timer.IsZero)
        {
            state.SpikeTimers[timerIndex] = timer.DecrementToZero();
            return;
        }

        // The reload keeps the random word's low six bits: sixteen to seventy-nine frames.
        state.SpikeTimers[timerIndex] = WrappingCountdown.Start(
            ReadFakeKraidRandomNumber() % SpikeReloadRandomSpread + MinimumSpikeReloadFrames);
        if (SpawnFakeKraidSpike(slot, state, timerIndex) &&
            FakeKraidOriginIsOnScreen(slot, camera))
        {
            LastFakeKraidSoundEffect = KraidSoundDefinitions.MiniKraidSpikeLaunch;
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

        // The step expires when the decrement reaches zero: reload seven through ten and
        // reverse without moving. Otherwise attempt the signed four-pixel 16.16
        // displacement and reverse only if the room collision helper reports a wall. A
        // cleared timer wraps rather than expiring, so it still performs the attempted step.
        CountdownStep walkStep = state.WalkStepTimer.Decrement();
        state.WalkStepTimer = walkStep.Remaining;
        bool reverse = walkStep.IsZero;
        if (reverse)
        {
            state.WalkStepTimer = WrappingCountdown.Start(
                (ReadFakeKraidRandomNumber() & 3) + 7);
        }
        else
        {
            int displacement = SixteenSixteen.FromWhole(state.WalkDelta);
            reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement);
        }

        if (reverse)
            state.WalkDelta = -state.WalkDelta;

        state.FacingDelta = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) < 0
            ? 4
            : -4;
    }

    /// <summary>Ports animation opcode <c>$A6:9B74</c> and returns the animation it continues at.</summary>
    private FakeKraidProgram SelectFakeKraidAction(
        FakeKraidEnemyState state)
    {
        if (!state.SpitDecisionTimer.IsZero)
        {
            if (state.FacingDelta < 0)
            {
                return state.WalkDelta >= 0
                    ? FakeKraidProgram.StepBackwardsFacingLeft
                    : FakeKraidProgram.StepForwardsFacingLeft;
            }

            return state.WalkDelta < 0
                ? FakeKraidProgram.StepBackwardsFacingRight
                : FakeKraidProgram.StepForwardsFacingRight;
        }

        state.SpitDecisionTimer = WrappingCountdown.Start((ReadFakeKraidRandomNumber() & 3) + 3);
        return state.FacingDelta < 0
            ? FakeKraidProgram.FireSpitFacingLeft
            : FakeKraidProgram.FireSpitFacingRight;
    }

    /// <summary>Ports animation opcodes <c>$A6:9BC4</c>/<c>$A6:9C02</c>.</summary>
    internal void SpawnFakeKraidSpitPair(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        HorizontalDirection direction)
    {
        int xOffset = direction == HorizontalDirection.Right ? 4 : -4;
        for (int projectile = 0; projectile < 2; projectile++)
        {
            EightEightVector<RoomPixels> launch =
                FakeKraidProjectileDefinitions.SpitLaunch(direction == HorizontalDirection.Right, projectile);
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
        int xOffset,
        EightEightVector<RoomPixels> velocity)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FakeKraidSpit,
            source.Graphics);
        projectile.StartAnimation(new FakeKraidProjectileAnimation(FakeKraidProjectilePose.Spit));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + new Offset16<RoomPixels>((short)xOffset, -16));
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
            source.Graphics);
        projectile.StartAnimation(new FakeKraidProjectileAnimation(kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? FakeKraidProjectilePose.SpikeLeft
            : FakeKraidProjectilePose.SpikeRight));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + new Offset16<RoomPixels>(0, FakeKraidProjectileDefinitions.SpikeYOffset(row)));
        int spikePixelsPerFrame = kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? -SpikePixelsPerFrame
            : SpikePixelsPerFrame;
        projectile.Velocity = new(SignedEightEight.FromSignedRaw(checked((short)(spikePixelsPerFrame * SubpixelsPerPixel))),
            new SignedEightEight(0));
        state.SpawnedSpikeCount++;
        return true;
    }

    /// <summary>Ports <c>EprojPreInit_MiniKraidSpit</c> at <c>$86:9E1E</c>.</summary>
    internal static void RunFakeKraidSpitPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Clear();
            return;
        }

        int oldVelocity = projectile.Velocity.Y.SignedRawValue;
        int nextVelocity = unchecked((short)(oldVelocity + 64));
        if (nextVelocity >= 0 && oldVelocity >= 960)
            nextVelocity = 1024;
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(unchecked((ushort)nextVelocity)));
    }

    /// <summary>Ports <c>EprojPreInstr_MiniKraidSpikes</c> at <c>$86:9E83</c>.</summary>
    internal static void RunFakeKraidSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            projectile.Clear();
    }

    /// <summary>Samples the room random word without advancing it for the native action choice.</summary>
    private int ReadFakeKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>Tests the actor origin against the inclusive native 256-pixel camera rectangle for its cry.</summary>
    private static bool FakeKraidOriginIsOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(slot.Position.X.Whole - camera.X)) >= 0 &&
        unchecked((short)(camera.X + 256 - slot.Position.X.Whole)) >= 0 &&
        unchecked((short)(slot.Position.Y.Whole - camera.Y)) >= 0 &&
        unchecked((short)(camera.Y + 256 - slot.Position.Y.Whole)) >= 0;

    /// <summary>
    /// Ports <c>Reaction_MiniKraid_Common</c> ($A6:9C50): <c>EnemyDeath</c> spawns death
    /// explosion three and clears the slot, then <c>MiniKraidDeathItemDropRoutine</c>
    /// ($A0:B8EE) scatters four pickups from the coordinates saved before the shot AI.
    /// The explosion therefore takes the first free projectile slot.
    /// </summary>
    private void KillFakeKraid(RoomEnemySlot slot)
    {
        Position16<RoomPixels> origin = slot.Position.Whole;
        _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: 3);
        SpawnFakeKraidDeathDrops(origin);
    }

    /// <summary>Scatters four Mini-Kraid pickups around the position saved before death clears its slot.</summary>
    private void SpawnFakeKraidDeathDrops(Position16<RoomPixels> origin)
    {
        _deathsAndDrops.SpawnEnemyDropScatterAround(
            EnemyDefinitionId.MiniKraid,
            count: 4,
            origin);
    }

    /// <summary>Replaces generic bomb death with Mini-Kraid's explosion-three and four-drop sequence.</summary>
    internal void AddFakeKraidBombReactions(EnemySharedReactions table)
    {
        // Fake Kraid saves its coordinates before common no-death AI, then requests death variant three and the
        // Mini-Kraid drop. Its definition never runs the generic death, whatever callback a hitbox selects.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.MiniKraid)
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
    internal static void AddFakeKraidProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunFakeKraidSpitPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.MiniKraidSpit);

        table.AddPreInstruction(context =>
        {
            RunFakeKraidSpikePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.MiniKraidSpikes);
    }

    /// <summary>Ports FakeKraid's initialization AI.</summary>
    internal void RunFakeKraidInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeFakeKraid(slot, surroundings.Samus);
    }

    /// <summary>Ports FakeKraid's main AI.</summary>
    internal void RunFakeKraidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunFakeKraidMain(
            slot,
            slot.RequireSpeciesState<FakeKraidEnemyState>(),
            frame.Camera);
    }

    /// <summary>Advances FakeKraid's typed animation.</summary>
    internal void AdvanceFakeKraidTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, FakeKraidAnimations.Programs, FakeKraidFrameDefinitions.ShownFrames.Of,
            static (FakeKraidEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyFakeKraidCue(slot, cue, context.Samus, context.Level, context.Camera));

    /// <summary>Spawns Mini-Kraid's own explosion and four drops after a lethal power bomb.</summary>
    internal void AddFakeKraidPowerBombReactions(EnemySharedReactions table)
    {
        // Mini Kraid's private power-bomb callback still owns its four direct pickup explosions, spawned after its
        // death explosion.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MiniKraid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.FakeKraidShot)
        {
            ReplacesGenericDeath = (enemy, _) => KillFakeKraid(enemy),
        });
    }

    /// <summary>Replaces ordinary projectile-shot death and preserves the pickup scatter on a lethal hit.</summary>
    internal void AddFakeKraidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MiniKraid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.FakeKraidShot)
        {
            ReplacesGenericDeath = c =>
            {
                KillFakeKraid(c.Enemy);
                return true;
            },
            AfterCommon = c =>
            {
                if (!c.Froze && c.Enemy.DefinitionId != EnemyDefinitionId.None && c.HealthBefore != 0 && c.Enemy.Health == 0)
                    SpawnFakeKraidDeathDrops(c.Enemy.Position.Whole);
            },
        });
    }

    /// <summary>Runs common Samus contact and routes a resulting lethal hit to Mini-Kraid's death sequence.</summary>
    internal void AddFakeKraidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.MiniKraid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.FakeKraidTouch,
            context =>
            {
                int healthBefore = context.Slot.Health;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                if (healthBefore != 0 && context.Slot.Health == 0)
                    KillFakeKraid(context.Slot);
            });
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastFakeKraidSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastFakeKraidSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}
