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
    /// <summary>Checks the walk cue's movement against room collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates spit, spike, and death effects from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Runs common Samus contact damage before Mini-Kraid's specialized death.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns death animation three and the four-pickup scatter after lethal hits.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Receives Mini-Kraid's cry and spike-launch sounds.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Mini-Kraid's walk, projectiles, sounds and specialized death to room services.</summary>
    internal FakeKraidBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, EnemyAudioQueue audio)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _audio = audio;
    }

    /// <summary>
    /// Carries out one cue of Mini-Kraid's animations. The action choice continues at the step or spit animation it
    /// selects, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private FakeKraidProgram? ApplyFakeKraidCue(RoomEnemySlot slot, FakeKraidCue cue, SamusState? samus, RoomLevelData? level, Position16<RoomPixels> camera)
    {
        switch (cue.Kind)
        {
            case FakeKraidCueKind.Move:
                // Walk cue: advance one four-pixel step unless its random reversal
                // clock expires, then refresh the live facing marker from Samus.
                ProcessFakeKraidWalkInstruction(slot, slot.RequireSpeciesState<FakeKraidEnemyState>(), samus, level);
                return null;
            case FakeKraidCueKind.ChooseAction:
                return SelectFakeKraidAction(slot.RequireSpeciesState<FakeKraidEnemyState>());
            case FakeKraidCueKind.PlayCrySound:
                // The spit step queues its sound only while the actor origin is inside the
                // inclusive 256x256 screen rectangle.
                if (FakeKraidOriginIsOnScreen(slot, camera))
                    RequestFakeKraidSound(KraidSoundDefinitions.MiniKraidCry);
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

    /// <summary>The gravity Mini-Kraid's spit gains each update.</summary>
    private static readonly SignedEightEight SpitGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The fastest Mini-Kraid's spit falls: four pixels an update.</summary>
    private static readonly SignedEightEight SpitMaximumFallSpeed = SignedEightEight.FromPixels(4);

    /// <summary>Whole pixels per frame a body spike travels horizontally, toward the side Mini-Kraid faces.</summary>
    private const int SpikePixelsPerFrame = 2;

    /// <summary>Sub-pixel steps in one whole pixel of a projectile's 8.8 velocity.</summary>
    private const int SubpixelsPerPixel = 256;

    /// <summary>Queues one of Mini-Kraid's sounds as it makes it, after the update's earlier requests.</summary>
    private void RequestFakeKraidSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);


    /// <summary>Sets Mini-Kraid up when the room spawns it.</summary>
    internal void InitializeFakeKraid(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Fake Kraid initialization requires the active Samus position.");
        }

        var state = new FakeKraidEnemyState();
        slot.SetSpeciesState(state);

        // The routine reads the already-published global random word; it does not advance
        // the RNG. Use the read callback when available and reserve NextRandom for older
        // isolated callers that did not expose that seam.
        int random = ReadFakeKraidRandomNumber();
        int seedDelay = KraidRandomDefinitions.FakeKraidStartDelay.Of(random) + KraidRandomDefinitions.FakeKraidStartDelayBase;
        state.WalkStepTimer = seedDelay;
        state.SpitDecisionTimer = seedDelay;
        state.SpikeTimers[FakeKraidSpikeRow.Top] = seedDelay + 64;
        state.SpikeTimers[FakeKraidSpikeRow.Middle] = seedDelay + 96;
        state.SpikeTimers[FakeKraidSpikeRow.Bottom] = seedDelay + 48;
        state.NextSpikeRow = FakeKraidSpikeRow.Top;

        slot.Processing = slot.Processing.With(EnemyProcessing.AnimationScript);
        slot.AnimationClock.Restart();
        state.WalkDelta = -4;
        state.FacingDelta = -4;
        state.Playback = new AnimationPlayback<FakeKraidProgram>(FakeKraidAnimations.StartingWith(FakeKraidProgram.StepForwardsFacingLeft), AnimationPlace.StartOfAnimation);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0)
        {
            state.WalkDelta = 4;
            state.FacingDelta = 4;
            state.Playback = new AnimationPlayback<FakeKraidProgram>(FakeKraidAnimations.StartingWith(FakeKraidProgram.StepForwardsFacingRight), AnimationPlace.StartOfAnimation);
        }
    }

    /// <summary>Runs Mini-Kraid's per-frame behaviour.</summary>
    private void RunFakeKraidMain(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Main AI moves on to the next port's clock before acting on this one.
        FakeKraidSpikeRow row = state.NextSpikeRow;
        state.NextSpikeRow = EnumOrder.NextWrapping(row);
        int timer = state.SpikeTimers[row];
        if (timer != 0)
        {
            state.SpikeTimers[row] = timer - 1;
            return;
        }

        state.SpikeTimers[row] =
            KraidRandomDefinitions.FakeKraidSpikeReloadSpread.Of(ReadFakeKraidRandomNumber()) +
            KraidRandomDefinitions.FakeKraidSpikeReloadBase;
        if (SpawnFakeKraidSpike(slot, state, row) &&
            FakeKraidOriginIsOnScreen(slot, camera))
        {
            RequestFakeKraidSound(KraidSoundDefinitions.MiniKraidSpikeLaunch);
        }
    }

    /// <summary>Runs Mini-Kraid's walk step.</summary>
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

        state.SpitDecisionTimer = Math.Max(0, state.SpitDecisionTimer - 1);

        // The step expires when the decrement reaches zero: reload and reverse without moving. Otherwise attempt the signed four-pixel 16.16
        // displacement and reverse only if the room collision helper reports a wall.
        bool reverse = --state.WalkStepTimer == 0;
        if (reverse)
        {
            state.WalkStepTimer = KraidRandomDefinitions.FakeKraidWalkStepSpread.Of(ReadFakeKraidRandomNumber()) +
                KraidRandomDefinitions.FakeKraidWalkStepBase;
        }
        else
        {
            SignedSixteenSixteen displacement = SignedSixteenSixteen.FromWholePixels(state.WalkDelta);
            reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement);
        }

        if (reverse)
            state.WalkDelta = -state.WalkDelta;

        state.FacingDelta = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0
            ? 4
            : -4;
    }

    /// <summary>Runs Mini-Kraid's action choice and returns the animation it continues at.</summary>
    private FakeKraidProgram SelectFakeKraidAction(
        FakeKraidEnemyState state)
    {
        if (state.SpitDecisionTimer != 0)
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

        state.SpitDecisionTimer = KraidRandomDefinitions.FakeKraidSpitDecisionSpread.Of(ReadFakeKraidRandomNumber()) +
            KraidRandomDefinitions.FakeKraidSpitDecisionBase;
        return state.FacingDelta < 0
            ? FakeKraidProgram.FireSpitFacingLeft
            : FakeKraidProgram.FireSpitFacingRight;
    }

    /// <summary>Runs Mini-Kraid's left and right spit steps.</summary>
    internal void SpawnFakeKraidSpitPair(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        HorizontalDirection direction)
    {
        int xOffset = direction == HorizontalDirection.Right ? 4 : -4;
        for (int projectile = 0; projectile < 2; projectile++)
        {
            EightEightVector<RoomPixels> launch =
                FakeKraidProjectiles.SpitLaunch(direction == HorizontalDirection.Right, projectile);
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FakeKraidSpit,
            SpriteGraphics.SpawnedBy(source));
        projectile.StartAnimation(new FakeKraidProjectileAnimation(FakeKraidProjectilePose.Spit));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + new Offset16<RoomPixels>(CoordinateAxis.AsSigned(xOffset), -16));
        projectile.Velocity = velocity;
        return true;
    }

    /// <summary>Allocates a facing-specific spike at an authored body row and counts only successful spawns.</summary>
    /// <returns>False when the enemy projectile pool is full.</returns>
    internal bool SpawnFakeKraidSpike(
        RoomEnemySlot source,
        FakeKraidEnemyState state,
        FakeKraidSpikeRow row)
    {
        RoomEnemyProjectileKind kind = state.FacingDelta < 0
            ? RoomEnemyProjectileKind.FakeKraidSpikeLeft
            : RoomEnemyProjectileKind.FakeKraidSpikeRight;
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            SpriteGraphics.SpawnedBy(source));
        projectile.StartAnimation(new FakeKraidProjectileAnimation(kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? FakeKraidProjectilePose.SpikeLeft
            : FakeKraidProjectilePose.SpikeRight));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + Offset16<RoomPixels>.Of(0, FakeKraidProjectiles.SpikeYOffset(row)));
        int spikePixelsPerFrame = kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? -SpikePixelsPerFrame
            : SpikePixelsPerFrame;
        projectile.Velocity = new(SignedEightEight.FromPixels(spikePixelsPerFrame), SignedEightEight.Zero);
        state.SpawnedSpikeCount++;
        return true;
    }

    /// <summary>Runs Mini-Kraid's spit per frame.</summary>
    internal static void RunFakeKraidSpitFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Delete();
            return;
        }

        // The fall speed stops at four pixels an update: a spit falling within one step of it is set to it.
        SignedEightEight oldVelocity = projectile.Velocity.Y;
        SignedEightEight nextVelocity = oldVelocity + SpitGravity;
        if (!nextVelocity.IsNegative && oldVelocity >= SpitMaximumFallSpeed - SpitGravity)
            nextVelocity = SpitMaximumFallSpeed;
        projectile.Velocity = projectile.Velocity.WithY(nextVelocity);
    }

    /// <summary>Runs Mini-Kraid's body spikes per frame.</summary>
    internal static void RunFakeKraidSpikeFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            projectile.Delete();
    }

    /// <summary>Samples the room random word without advancing it for the action choice.</summary>
    private int ReadFakeKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>Tests the actor origin against the inclusive 256-pixel camera rectangle for its cry.</summary>
    private static bool FakeKraidOriginIsOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        CoordinateAxis.Difference(slot.Position.X.Whole, camera.X) >= 0 &&
        CoordinateAxis.AsSigned(camera.X + 256 - slot.Position.X.Whole) >= 0 &&
        CoordinateAxis.Difference(slot.Position.Y.Whole, camera.Y) >= 0 &&
        CoordinateAxis.AsSigned(camera.Y + 256 - slot.Position.Y.Whole) >= 0;

    /// <summary>
    /// Mini-Kraid's shared death reaction: the enemy's death spawns death
    /// explosion three and clears the slot, then the death drop
    /// scatters four pickups from the coordinates saved before the shot AI.
    /// The explosion therefore takes the first free projectile slot.
    /// </summary>
    private void KillFakeKraid(RoomEnemySlot slot)
    {
        Position16<RoomPixels> origin = slot.Position.Whole;
        _deathsAndDrops.StartGenericEnemyDeath(slot, deathAnimation: EnemyDeathExplosion.MiniKraid);
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
                if (c.SelectedShotAi == EnemyReaction.FakeKraidShot && c.HealthBefore != 0 && c.Enemy.Health == 0)
                    KillFakeKraid(c.Enemy);
            },
        });
    }

    /// <summary>Dispatches spit gravity and spike wall collision through their projectile frame behaviours.</summary>
    internal static void AddFakeKraidProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunFakeKraidSpitFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.FakeKraidSpitFlight);

        table.AddFrameBehavior(context =>
        {
            RunFakeKraidSpikeFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.FakeKraidSpikeFlight);
    }

    /// <summary>Runs Mini-Kraid's initialization AI.</summary>
    internal void RunFakeKraidInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeFakeKraid(slot, surroundings.Samus);
    }

    /// <summary>Runs Mini-Kraid's main AI.</summary>
    internal void RunFakeKraidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunFakeKraidMain(
            slot,
            slot.RequireSpeciesState<FakeKraidEnemyState>(),
            frame.Camera);
    }

    /// <summary>Advances Mini-Kraid's typed animation.</summary>
    internal void AdvanceFakeKraidTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, FakeKraidAnimations.Segments,
            static (FakeKraidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (_, cue) => ApplyFakeKraidCue(slot, cue, context.Samus, context.Level, context.Camera) is { } action
                ? new AnimationContinuation<FakeKraidProgram>(FakeKraidAnimations.StartingWith(action), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Spawns Mini-Kraid's own explosion and four drops after a lethal power bomb.</summary>
    internal void AddFakeKraidPowerBombReactions(EnemySharedReactions table)
    {
        // Mini Kraid's private power-bomb callback still owns its four direct pickup explosions, spawned after its
        // death explosion.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MiniKraid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.FakeKraidShot)
        {
            ReplacesGenericDeath = (enemy, _) => KillFakeKraid(enemy),
        });
    }

    /// <summary>Replaces ordinary projectile-shot death and preserves the pickup scatter on a lethal hit.</summary>
    internal void AddFakeKraidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MiniKraid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.FakeKraidShot)
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
                               EnemyCollision.TouchAi(slot) == EnemyReaction.FakeKraidTouch,
            context =>
            {
                int healthBefore = context.Slot.Health;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                if (healthBefore != 0 && context.Slot.Health == 0)
                    KillFakeKraid(context.Slot);
            });
    }
}
