using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The Fake Kraid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class FakeKraidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.FakeKraid], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.FromSurroundings(new FakeKraidEntity(this, occupant, place, common));

    /// <summary>Provides the existing random number sampled for Fake Kraid's action decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Checks the walk cue's movement against room collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates spit, spike, and death effects from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Runs common Samus contact damage before Fake Kraid's specialized death.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns Fake Kraid's death explosion and the pickup scatter after lethal hits.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Receives Fake Kraid's cry and spike-launch sounds.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Fake Kraid's walk, projectiles, sounds and specialized death to room services.</summary>
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
    /// Carries out one cue of Fake Kraid's animations. The action choice continues at the step or spit animation it
    /// selects, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private FakeKraidSegment? ApplyFakeKraidCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, FakeKraidCue cue, SamusState samus, RoomLevelData level, Position16<RoomPixels> camera) =>
        cue.Match(ApplyFakeKraidCueCases, new ApplyFakeKraidCueContext(occupant, place, samus, level, camera, this));

    /// <summary>What <c>ApplyFakeKraidCue</c> does on each <see cref="FakeKraidCue"/>, built once (#2553).</summary>
    private static FakeKraidCueCases<ApplyFakeKraidCueContext, FakeKraidSegment?> ApplyFakeKraidCueCases { get; } = new(
            Spit: static (spit, context) =>
            {
                context.Owner.SpawnFakeKraidSpitPair(context.Occupant, context.Place, spit.Direction);
                return null;
            },
            ChooseAction: static (_, context) => context.Owner.SelectFakeKraidAction(context.Place.RequireSpeciesState<FakeKraidEnemyState>()),
            Move: static (_, context) =>
            {
                // Walk cue: advance one step unless its random reversal
                // clock expires, then turn to face Samus.
                context.Owner.ProcessFakeKraidWalkAnimation(context.Place, context.Place.RequireSpeciesState<FakeKraidEnemyState>(), context.Samus, context.Level);
                return null;
            },
            PlayCrySound: static (_, context) =>
            {
                // The spit step queues its sound only while Fake Kraid is in the view, its edges included.
                if (EnemyView.Contains(context.Place.Position.Whole, context.Camera))
                    context.Owner.RequestFakeKraidSound(KraidSoundDefinitions.FakeKraidCry);
                return null;
            });

    /// <summary>What <c>ApplyFakeKraidCue</c>'s cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Place">The <c>place</c> <c>ApplyFakeKraidCue</c> was handed.</param>
    /// <param name="Samus">The <c>samus</c> <c>ApplyFakeKraidCue</c> was handed.</param>
    /// <param name="Level">The <c>level</c> <c>ApplyFakeKraidCue</c> was handed.</param>
    /// <param name="Camera">The <c>camera</c> <c>ApplyFakeKraidCue</c> was handed.</param>
    /// <param name="Owner">The <see cref="FakeKraidBehavior"/> reacting.</param>
    private readonly record struct ApplyFakeKraidCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Place, SamusState Samus, RoomLevelData Level, Position16<RoomPixels> Camera, FakeKraidBehavior Owner);

    /// <summary>Queues one of Fake Kraid's sounds as it makes it, after the update's earlier requests.</summary>
    private void RequestFakeKraidSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Sets Fake Kraid up when the room spawns it.</summary>
    private void InitializeFakeKraid(RoomEnemyPlace place, SamusState samus)
    {
        var state = new FakeKraidEnemyState();
        place.SetSpeciesState(state);

        // The action choice reads the already-published random number; it does not advance
        // the RNG.
        int random = ReadFakeKraidRandomNumber();
        int seedDelay = KraidRandomDefinitions.FakeKraidStartDelay.Of(random) + KraidRandomDefinitions.FakeKraidStartDelayBase;
        state.WalkTurnWait.LastFor(seedDelay);
        state.SpitWait.LastFor(seedDelay);
        foreach ((FakeKraidSpikeRow row, int delay) in FakeKraidProjectileDefinitions.SpikeStartDelays)
            state.SpikeWaits[row] = BossCountdown.Lasting(seedDelay + delay);
        state.NextSpikeRow = FakeKraidSpikeRow.Top;

        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.Restart();
        HorizontalDirection towardSamus = SideOfSamus(place, samus);
        state.Walking = towardSamus;
        state.Facing = towardSamus;
        state.Playback = new AnimationPlayback<FakeKraidSegment>(FakeKraidAnimations.StartingWith(towardSamus == HorizontalDirection.Right
            ? FakeKraidSegment.StepForwardsFacingRight
            : FakeKraidSegment.StepForwardsFacingLeft), AnimationPlace.StartOfAnimation);
    }

    /// <summary>The side Samus stands on from Fake Kraid: right only when she stands right of him.</summary>
    private static HorizontalDirection SideOfSamus(RoomEnemyPlace place, SamusState samus) =>
        place.Position.X.Whole < samus.Position.X.Whole
            ? HorizontalDirection.Right
            : HorizontalDirection.Left;

    /// <summary>The port whose clock each port hands on to (#2553): top, middle and bottom in turn, then the top again.</summary>
    private static IdentityTable<FakeKraidSpikeRow, FakeKraidSpikeRow> SpikeRowAfter { get; } = new(new Dictionary<FakeKraidSpikeRow, FakeKraidSpikeRow>
    {
        [FakeKraidSpikeRow.Top] = FakeKraidSpikeRow.Middle,
        [FakeKraidSpikeRow.Middle] = FakeKraidSpikeRow.Bottom,
        [FakeKraidSpikeRow.Bottom] = FakeKraidSpikeRow.Top,
    });

    /// <summary>Runs Fake Kraid's main AI for one update.</summary>
    private void RunFakeKraidMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        FakeKraidEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Main AI moves on to the next port's clock before acting on this one.
        FakeKraidSpikeRow row = state.NextSpikeRow;
        state.NextSpikeRow = SpikeRowAfter[row];
        BossCountdown spikeWait = state.SpikeWaits[row];
        if (spikeWait.StepKeepsRunning())
            return;

        spikeWait.LastFor(
            KraidRandomDefinitions.FakeKraidSpikeReloadSpread.Of(ReadFakeKraidRandomNumber()) +
            KraidRandomDefinitions.FakeKraidSpikeReloadBase);
        if (SpawnFakeKraidSpike(occupant, place, state, row) &&
            EnemyView.Contains(place.Position.Whole, camera))
        {
            RequestFakeKraidSound(KraidSoundDefinitions.FakeKraidSpikeLaunch);
        }
    }

    /// <summary>Runs Fake Kraid's walk step.</summary>
    private void ProcessFakeKraidWalkAnimation(
        RoomEnemyPlace place,
        FakeKraidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        state.SpitWait.Step();

        // On the walk's last step he turns round without moving, and starts his next walk. Otherwise he attempts a
        // step and turns round only if it meets a wall.
        bool reverse = state.WalkTurnWait.StepRunsOut();
        if (reverse)
        {
            state.WalkTurnWait.LastFor(KraidRandomDefinitions.FakeKraidWalkStepSpread.Of(ReadFakeKraidRandomNumber()) +
                KraidRandomDefinitions.FakeKraidWalkStepBase);
        }
        else
        {
            SignedSixteenSixteen displacement =
                SignedSixteenSixteen.FromWholePixels(FakeKraidProjectileDefinitions.StepPixels).Toward(state.Walking);
            reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                displacement);
        }

        if (reverse)
            state.Walking = state.Walking.Reversed();

        state.Facing = SideOfSamus(place, samus);
    }

    /// <summary>Runs Fake Kraid's action choice and returns the animation it continues at.</summary>
    private FakeKraidSegment SelectFakeKraidAction(
        FakeKraidEnemyState state)
    {
        if (!state.SpitWait.HasRunOut)
        {
            if (state.Facing == HorizontalDirection.Left)
            {
                return state.Walking == HorizontalDirection.Right
                    ? FakeKraidSegment.StepBackwardsFacingLeft
                    : FakeKraidSegment.StepForwardsFacingLeft;
            }

            return state.Walking == HorizontalDirection.Left
                ? FakeKraidSegment.StepBackwardsFacingRight
                : FakeKraidSegment.StepForwardsFacingRight;
        }

        state.SpitWait.LastFor(KraidRandomDefinitions.FakeKraidSpitDecisionSpread.Of(ReadFakeKraidRandomNumber()) +
            KraidRandomDefinitions.FakeKraidSpitDecisionBase);
        return state.Facing == HorizontalDirection.Left
            ? FakeKraidSegment.FireSpitFacingLeft
            : FakeKraidSegment.FireSpitFacingRight;
    }

    /// <summary>Runs Fake Kraid's left and right spit steps.</summary>
    private void SpawnFakeKraidSpitPair(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        HorizontalDirection direction)
    {
        int xOffset = FakeKraidProjectileDefinitions.SpitAheadPixels.Toward(direction);
        foreach (FakeKraidSpit spit in FakeKraidProjectileDefinitions.SpitsInLaunchOrder)
            SpawnFakeKraidSpit(occupant, place, xOffset, FakeKraidProjectiles.SpitLaunch(direction, spit));
    }

    /// <summary>
    /// Allocates one spit projectile above and just ahead of Fake Kraid's body with its authored velocity; a full enemy
    /// projectile pool spawns none.
    /// </summary>
    private void SpawnFakeKraidSpit(
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        int xOffset,
        PixelVelocity<RoomPixels> velocity)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.FakeKraidSpit, SpitAndSpikes, SpriteGraphics.SpawnedBy(occupant, source.Shading));
        if (projectile is null)
            return;

        projectile.StartAnimation(new FakeKraidProjectileAnimation(FakeKraidProjectilePose.Spit));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + new Offset16<RoomPixels>(xOffset, FakeKraidProjectileDefinitions.SpitYOffset));
        projectile.Velocity = velocity;
    }

    /// <summary>Allocates a spike at an authored body row, flying the way Fake Kraid faces (#2278).</summary>
    /// <returns>False when the enemy projectile pool is full.</returns>
    private bool SpawnFakeKraidSpike(
        RoomEnemyOccupant occupant, RoomEnemyPlace source,
        FakeKraidEnemyState state,
        FakeKraidSpikeRow row)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.FakeKraidSpike, SpitAndSpikes, SpriteGraphics.SpawnedBy(occupant, source.Shading));
        if (projectile is null)
            return false;

        bool left = state.Facing == HorizontalDirection.Left;
        projectile.StartAnimation(new FakeKraidProjectileAnimation(left
            ? FakeKraidProjectilePose.SpikeLeft
            : FakeKraidProjectilePose.SpikeRight));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(
            source.Position.Whole + Offset16<RoomPixels>.Of(0, FakeKraidProjectiles.SpikeYOffset(row)));
        projectile.Velocity = new(FakeKraidProjectileDefinitions.SpikeSpeed.Toward(state.Facing), SignedEightEight.Zero);
        return true;
    }

    /// <summary>Runs Fake Kraid's spit per frame.</summary>
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

        // The fall speed stops at the spit's fastest fall: a spit falling within one step of it is set to it.
        SignedEightEight oldVelocity = projectile.Velocity.Y;
        SignedEightEight nextVelocity = oldVelocity + FakeKraidProjectileDefinitions.SpitGravity;
        if (!nextVelocity.IsNegative && oldVelocity >= FakeKraidProjectileDefinitions.SpitMaximumFallSpeed - FakeKraidProjectileDefinitions.SpitGravity)
            nextVelocity = FakeKraidProjectileDefinitions.SpitMaximumFallSpeed;
        projectile.Velocity = projectile.Velocity.WithY(nextVelocity);
    }

    /// <summary>Runs Fake Kraid's body spikes per frame.</summary>
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
    /// Fake Kraid's shared death reaction: the enemy's death spawns death
    /// explosion and clears the place, then the death drop
    /// scatters its pickups from where the enemy stood before the shot struck.
    /// The explosion therefore takes the first free projectile place.
    /// </summary>
    private void KillFakeKraid(RoomEnemyOccupant fakeKraid, SamusState samus)
    {
        RoomEnemyPlace place = fakeKraid.Place;
        Position16<RoomPixels> origin = place.Position.Whole;
        _deathsAndDrops.StartGenericEnemyDeath(fakeKraid, EnemyDeathExplosion.FakeKraid, samus);
        SpawnFakeKraidDeathDrops(origin, samus);
    }

    /// <summary>Scatters Fake Kraid's death pickups (<see cref="FakeKraidProjectileDefinitions.DeathDropCount"/>) around the position saved before death clears its place.</summary>
    private void SpawnFakeKraidDeathDrops(Position16<RoomPixels> origin, SamusState samus)
    {
        _deathsAndDrops.SpawnEnemyDropScatterAround(
            EnemyDefinitionId.FakeKraid,
            count: FakeKraidProjectileDefinitions.DeathDropCount,
            origin,
            samus);
    }

    /// <summary>Replaces generic bomb death with Fake Kraid's death explosion and drop sequence.</summary>
    internal EnemyBombReaction FakeKraidBombReaction => field ??= new()
    {
        // Fake Kraid keeps where it stood before the common damage, then dies in its own explosion and scatters its
        // drops there. It never dies the generic death, whatever reaction a hitbox selects.
        RunsGenericDeath = false,
        // The bomb's common damage shows no hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            // The bomb selects the fake Kraid's own shot reaction, which its definition names, rather than the empty
            // frame's ordinary one.
            if (c.SelectedShot == KraidReactions.FakeKraidShot && c.HealthBefore != 0 && c.Enemy.Health == 0)
                KillFakeKraid(c.Occupant, c.Samus);
        },
    };

    /// <summary>Fake Kraid's spit and spikes (#1760): their spawners start their animations.</summary>
    private SpitAndSpikeBehavior SpitAndSpikes { get; } = new();

    /// <summary>Fake Kraid's spit, with room collision then capped gravity, or a spike flying until it strikes a wall.</summary>
    internal sealed class SpitAndSpikeBehavior : EnemyProjectileBehavior
    {
        /// <summary>The flight each kind this behaviour is bound to starts in: the spit's and the spikes'.</summary>
        private readonly Dictionary<RoomEnemyProjectileKind, EnemyProjectileOwnedMotion> _startingMotions;

        /// <summary>The spit and the spikes fly from their spawn.</summary>
        internal SpitAndSpikeBehavior() => _startingMotions = new()
        {
            [RoomEnemyProjectileKind.FakeKraidSpit] = SpitFlying,
            [RoomEnemyProjectileKind.FakeKraidSpike] = SpikeFlying,
        };

        /// <summary>The spit flying through the room's blocks, then falling under capped gravity.</summary>
        private EnemyProjectileOwnedMotion SpitFlying { get; } = new(nameof(SpitFlying),
            context => FlyFakeKraidSpit(context.Projectile, context.Level));

        /// <summary>A spike flying along its heading until it strikes a wall.</summary>
        private EnemyProjectileOwnedMotion SpikeFlying { get; } = new(nameof(SpikeFlying),
            context => FlyFakeKraidSpike(context.Projectile, context.Level));

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">This behaviour is not bound to <paramref name="kind"/>.</exception>
        internal override EnemyProjectileMotion? InitialMotion(RoomEnemyProjectileKind kind) =>
            _startingMotions.TryGetValue(kind, out EnemyProjectileOwnedMotion? starting)
                ? starting
                : throw new InvalidOperationException($"{nameof(SpitAndSpikeBehavior)} is not bound to enemy projectile {kind}.");
    }

    /// <summary>Runs Fake Kraid's initialization AI.</summary>
    internal void SetUpFakeKraid(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        InitializeFakeKraid(place, surroundings.Samus);
    }

    /// <summary>Runs Fake Kraid's main AI.</summary>
    internal void RunFakeKraidMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunFakeKraidMain(
            occupant, place,
            place.RequireSpeciesState<FakeKraidEnemyState>(),
            frame.Camera);
    }

    /// <summary>Advances Fake Kraid's typed animation.</summary>
    internal void AdvanceFakeKraidTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, FakeKraidAnimations.Segments,
            static (FakeKraidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (_, cue) => ApplyFakeKraidCue(occupant, place, cue, context.Samus, context.Level, context.Camera) is { } action
                ? new AnimationContinuation<FakeKraidSegment>(FakeKraidAnimations.StartingWith(action), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Spawns Fake Kraid's own explosion and drops after a lethal power bomb.</summary>
    internal EnemyPowerBombReaction FakeKraidPowerBombReaction => field ??= new()
    {
        // Fake Kraid's private power-bomb reaction still owns its direct pickup explosions, spawned after its
        // death explosion.
        ReplacesGenericDeath = c => KillFakeKraid(c.Occupant, c.Samus),
    };

    /// <summary>Replaces ordinary projectile-shot death and preserves the pickup scatter on a lethal hit.</summary>
    internal EnemyShotReaction FakeKraidShotReaction => field ??= new()
    {
        // Fake Kraid's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        ReplacesGenericDeath = c =>
        {
            KillFakeKraid(c.Occupant, c.Samus);
            return true;
        },
        AfterCommon = c =>
        {
            if (!c.Froze && !c.Enemy.IsFree && c.HealthBefore != 0 && c.Enemy.Health == 0)
                SpawnFakeKraidDeathDrops(c.Enemy.Position.Whole, c.Samus);
        },
    };

    /// <summary>Runs common Samus contact and routes a resulting lethal hit to Fake Kraid's death sequence.</summary>
    internal void ReactToFakeKraidTouch(EnemyTouchContext context)
    {
        int healthBefore = context.Place.Health;
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        if (healthBefore != 0 && context.Place.Health == 0)
            KillFakeKraid(context.Occupant, context.Samus);
    }
}
