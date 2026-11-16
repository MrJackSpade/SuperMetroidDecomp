using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Mini-Kraid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FakeKraidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.MiniKraid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new FakeKraidEntity(this, place, common);

    /// <summary>Provides the existing random number sampled for Mini-Kraid's action decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Checks the walk cue's movement against room collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates spit, spike, and death effects from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Runs common Samus contact damage before Mini-Kraid's specialized death.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns Mini-Kraid's death explosion and the four-pickup scatter after lethal hits.</summary>
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
    private FakeKraidSegment? ApplyFakeKraidCue(RoomEnemyPlace place, FakeKraidCue cue, SamusState samus, RoomLevelData level, Position16<RoomPixels> camera)
    {
        switch (cue.Kind)
        {
            case FakeKraidCueKind.Move:
                // Walk cue: advance one four-pixel step unless its random reversal
                // clock expires, then refresh the live facing marker from Samus.
                ProcessFakeKraidWalkAnimation(place, place.RequireSpeciesState<FakeKraidEnemyState>(), samus, level);
                return null;
            case FakeKraidCueKind.ChooseAction:
                return SelectFakeKraidAction(place.RequireSpeciesState<FakeKraidEnemyState>());
            case FakeKraidCueKind.PlayCrySound:
                // The spit step queues its sound only while Mini-Kraid is in the view, its edges included.
                if (EnemyView.Contains(place.Position.Whole, camera))
                    RequestFakeKraidSound(KraidSoundDefinitions.MiniKraidCry);
                return null;
            case FakeKraidCueKind.SpitLeft:
                SpawnFakeKraidSpitPair(place, direction: HorizontalDirection.Left);
                return null;
            case FakeKraidCueKind.SpitRight:
                SpawnFakeKraidSpitPair(place, direction: HorizontalDirection.Right);
                return null;
            default:
                throw new InvalidOperationException($"Mini-Kraid's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>The gravity Mini-Kraid's spit gains each update.</summary>
    private static readonly SignedEightEight SpitGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>The fastest Mini-Kraid's spit falls: four pixels an update.</summary>
    private static readonly SignedEightEight SpitMaximumFallSpeed = SignedEightEight.FromPixels(4);

    /// <summary>The speed a body spike travels horizontally, toward the side Mini-Kraid faces: two pixels an update.</summary>
    private static readonly SignedEightEight SpikeSpeed = SignedEightEight.FromPixels(2);

    /// <summary>Queues one of Mini-Kraid's sounds as it makes it, after the update's earlier requests.</summary>
    private void RequestFakeKraidSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);


    /// <summary>Sets Mini-Kraid up when the room spawns it.</summary>
    internal void InitializeFakeKraid(RoomEnemyPlace place, SamusState samus)
    {
        var state = new FakeKraidEnemyState();
        place.SetSpeciesState(state);

        // The action choice reads the already-published random number; it does not advance
        // the RNG.
        int random = ReadFakeKraidRandomNumber();
        int seedDelay = KraidRandomDefinitions.FakeKraidStartDelay.Of(random) + KraidRandomDefinitions.FakeKraidStartDelayBase;
        state.WalkStepTimer = seedDelay;
        state.SpitDecisionTimer = seedDelay;
        state.SpikeTimers[FakeKraidSpikeRow.Top] = seedDelay + 64;
        state.SpikeTimers[FakeKraidSpikeRow.Middle] = seedDelay + 96;
        state.SpikeTimers[FakeKraidSpikeRow.Bottom] = seedDelay + 48;
        state.NextSpikeRow = FakeKraidSpikeRow.Top;

        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.Restart();
        state.WalkDelta = -4;
        state.FacingDelta = -4;
        state.Playback = new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(FakeKraidSegment.StepForwardsFacingLeft), AnimationPlace.StartOfAnimation);
        if (CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0)
        {
            state.WalkDelta = 4;
            state.FacingDelta = 4;
            state.Playback = new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(FakeKraidSegment.StepForwardsFacingRight), AnimationPlace.StartOfAnimation);
        }
    }

    /// <summary>Runs Mini-Kraid's main AI for one update.</summary>
    private void RunFakeKraidMain(
        RoomEnemyPlace place,
        FakeKraidEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Main AI moves on to the next port's clock before acting on this one.
        FakeKraidSpikeRow row = state.NextSpikeRow;
        state.NextSpikeRow = row switch
        {
            FakeKraidSpikeRow.Top => FakeKraidSpikeRow.Middle,
            FakeKraidSpikeRow.Middle => FakeKraidSpikeRow.Bottom,
            FakeKraidSpikeRow.Bottom => FakeKraidSpikeRow.Top,
            _ => throw new InvalidOperationException($"Undefined {nameof(FakeKraidSpikeRow)} {row}."),
        };
        int timer = state.SpikeTimers[row];
        if (timer != 0)
        {
            state.SpikeTimers[row] = timer - 1;
            return;
        }

        state.SpikeTimers[row] =
            KraidRandomDefinitions.FakeKraidSpikeReloadSpread.Of(ReadFakeKraidRandomNumber()) +
            KraidRandomDefinitions.FakeKraidSpikeReloadBase;
        if (SpawnFakeKraidSpike(place, state, row) &&
            EnemyView.Contains(place.Position.Whole, camera))
        {
            RequestFakeKraidSound(KraidSoundDefinitions.MiniKraidSpikeLaunch);
        }
    }

    /// <summary>Runs Mini-Kraid's walk step.</summary>
    private void ProcessFakeKraidWalkAnimation(
        RoomEnemyPlace place,
        FakeKraidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        state.SpitDecisionTimer = Math.Max(0, state.SpitDecisionTimer - 1);

        // The step expires when the decrement reaches zero: reload and reverse without moving. Otherwise attempt the four-pixel
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
                place,
                displacement);
        }

        if (reverse)
            state.WalkDelta = -state.WalkDelta;

        state.FacingDelta = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0
            ? 4
            : -4;
    }

    /// <summary>Runs Mini-Kraid's action choice and returns the animation it continues at.</summary>
    private FakeKraidSegment SelectFakeKraidAction(
        FakeKraidEnemyState state)
    {
        if (state.SpitDecisionTimer != 0)
        {
            if (state.FacingDelta < 0)
            {
                return state.WalkDelta >= 0
                    ? FakeKraidSegment.StepBackwardsFacingLeft
                    : FakeKraidSegment.StepForwardsFacingLeft;
            }

            return state.WalkDelta < 0
                ? FakeKraidSegment.StepBackwardsFacingRight
                : FakeKraidSegment.StepForwardsFacingRight;
        }

        state.SpitDecisionTimer = KraidRandomDefinitions.FakeKraidSpitDecisionSpread.Of(ReadFakeKraidRandomNumber()) +
            KraidRandomDefinitions.FakeKraidSpitDecisionBase;
        return state.FacingDelta < 0
            ? FakeKraidSegment.FireSpitFacingLeft
            : FakeKraidSegment.FireSpitFacingRight;
    }

    /// <summary>Runs Mini-Kraid's left and right spit steps.</summary>
    internal void SpawnFakeKraidSpitPair(
        RoomEnemyPlace place,
        HorizontalDirection direction)
    {
        int xOffset = direction == HorizontalDirection.Right ? 4 : -4;
        for (int projectile = 0; projectile < 2; projectile++)
        {
            EightEightVector<RoomPixels> launch =
                FakeKraidProjectiles.SpitLaunch(direction == HorizontalDirection.Right, projectile);
            SpawnFakeKraidSpit(place, xOffset, launch);
        }
    }

    /// <summary>
    /// Allocates one spit projectile above and just ahead of Mini-Kraid's body with its authored velocity; a full enemy
    /// projectile pool spawns none.
    /// </summary>
    internal void SpawnFakeKraidSpit(
        RoomEnemyPlace source,
        int xOffset,
        EightEightVector<RoomPixels> velocity)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FakeKraidSpit,
            SpitAndSpikes,
            SpriteGraphics.SpawnedBy(source));
        projectile.StartAnimation(new FakeKraidProjectileAnimation(FakeKraidProjectilePose.Spit));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + new Offset16<RoomPixels>(CoordinateAxis.AsSigned(xOffset), -16));
        projectile.Velocity = velocity;
    }

    /// <summary>Allocates a facing-specific spike at an authored body row.</summary>
    /// <returns>False when the enemy projectile pool is full.</returns>
    internal bool SpawnFakeKraidSpike(
        RoomEnemyPlace source,
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
            SpitAndSpikes,
            SpriteGraphics.SpawnedBy(source));
        projectile.StartAnimation(new FakeKraidProjectileAnimation(kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? FakeKraidProjectilePose.SpikeLeft
            : FakeKraidProjectilePose.SpikeRight));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + Offset16<RoomPixels>.Of(0, FakeKraidProjectiles.SpikeYOffset(row)));
        projectile.Velocity = new(kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft ? -SpikeSpeed : SpikeSpeed, SignedEightEight.Zero);
        return true;
    }

    /// <summary>Runs Mini-Kraid's spit per frame.</summary>
    internal static void FlyFakeKraidSpit(
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
    internal static void FlyFakeKraidSpike(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            projectile.Delete();
    }

    /// <summary>Samples the room's random number without advancing it for the action choice.</summary>
    private int ReadFakeKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>
    /// Mini-Kraid's shared death reaction: the enemy's death spawns death
    /// explosion and clears the place, then the death drop
    /// scatters four pickups from the coordinates saved before the shot AI.
    /// The explosion therefore takes the first free projectile slot.
    /// </summary>
    private void KillFakeKraid(RoomEnemyPlace place, SamusState samus)
    {
        Position16<RoomPixels> origin = place.Position.Whole;
        _deathsAndDrops.StartGenericEnemyDeath(place, EnemyDeathExplosion.MiniKraid, samus);
        SpawnFakeKraidDeathDrops(origin, samus);
    }

    /// <summary>Scatters four Mini-Kraid pickups around the position saved before death clears its place.</summary>
    private void SpawnFakeKraidDeathDrops(Position16<RoomPixels> origin, SamusState samus)
    {
        _deathsAndDrops.SpawnEnemyDropScatterAround(
            EnemyDefinitionId.MiniKraid,
            count: 4,
            origin,
            samus);
    }

    /// <summary>Replaces generic bomb death with Mini-Kraid's death explosion and four-drop sequence.</summary>
    internal EnemyBombReaction FakeKraidBombReaction => field ??= new()
    {
        // Fake Kraid saves its coordinates before common no-death AI, then requests death variant three and the
        // Mini-Kraid drop. Its definition never runs the generic death, whatever reaction a hitbox selects.
        RunsGenericDeath = false,
        // Mini Kraid's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            // The bomb selects the fake Kraid's own shot reaction, which its definition leaves to it, rather than the
            // empty frame's ordinary one.
            if (c.SelectedShot is null && c.HealthBefore != 0 && c.Enemy.Health == 0)
                KillFakeKraid(c.Enemy, c.Samus);
        },
    };

    /// <summary>Fake Kraid's spit and spikes (#1760): their spawners start their animations.</summary>
    internal static readonly EnemyProjectileBehavior SpitAndSpikes = new SpitAndSpikeBehavior();

    /// <summary>Fake Kraid's spit, with room collision then capped gravity, or a spike flying until it strikes a wall.</summary>
    private sealed class SpitAndSpikeBehavior() : EnemyProjectileMotionBehavior<FakeKraidProjectileMotion>(StartingMotions)
    {
        /// <summary>The spit and the spikes fly from their spawn.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, FakeKraidProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.FakeKraidSpit] = FakeKraidProjectileMotion.SpitFlying,
            [RoomEnemyProjectileKind.FakeKraidSpikeLeft] = FakeKraidProjectileMotion.SpikeFlying,
            [RoomEnemyProjectileKind.FakeKraidSpikeRight] = FakeKraidProjectileMotion.SpikeFlying,
        };

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, FakeKraidProjectileMotion motion)
        {
            switch (motion)
            {
                case FakeKraidProjectileMotion.SpitFlying:
                    FlyFakeKraidSpit(context.Projectile, context.Level);
                    break;
                case FakeKraidProjectileMotion.SpikeFlying:
                    FlyFakeKraidSpike(context.Projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(FakeKraidProjectileMotion)} {motion}.");
            }
        }
    }

    /// <summary>Runs Mini-Kraid's initialization AI.</summary>
    internal void RunFakeKraidInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeFakeKraid(place, surroundings.IntroducedSamus);
    }

    /// <summary>Runs Mini-Kraid's main AI.</summary>
    internal void RunFakeKraidMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunFakeKraidMain(
            place,
            place.RequireSpeciesState<FakeKraidEnemyState>(),
            frame.Camera);
    }

    /// <summary>Advances Mini-Kraid's typed animation.</summary>
    internal void AdvanceFakeKraidTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, FakeKraidAnimations.Segments,
            static (FakeKraidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (_, cue) => ApplyFakeKraidCue(place, cue, context.Samus, context.Level, context.Camera) is { } action
                ? new AnimationContinuation<FakeKraidSegment>(FakeKraidAnimations.StartingWith(action), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Spawns Mini-Kraid's own explosion and four drops after a lethal power bomb.</summary>
    internal EnemyPowerBombReaction FakeKraidPowerBombReaction => field ??= new()
    {
        // Mini Kraid's private power-bomb reaction still owns its four direct pickup explosions, spawned after its
        // death explosion.
        ReplacesGenericDeath = c => KillFakeKraid(c.Enemy, c.Samus),
    };

    /// <summary>Replaces ordinary projectile-shot death and preserves the pickup scatter on a lethal hit.</summary>
    internal EnemyShotReaction FakeKraidShotReaction => field ??= new()
    {
        // Mini Kraid's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        ReplacesGenericDeath = c =>
        {
            KillFakeKraid(c.Enemy, c.Samus);
            return true;
        },
        AfterCommon = c =>
        {
            if (!c.Froze && !c.Enemy.IsFree && c.HealthBefore != 0 && c.Enemy.Health == 0)
                SpawnFakeKraidDeathDrops(c.Enemy.Position.Whole, c.Samus);
        },
    };

    /// <summary>Runs common Samus contact and routes a resulting lethal hit to Mini-Kraid's death sequence.</summary>
    internal void ReactToFakeKraidTouch(EnemyTouchContext context)
    {
        int healthBefore = context.Place.Health;
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
        if (healthBefore != 0 && context.Place.Health == 0)
            KillFakeKraid(context.Place, context.Samus);
    }
}
