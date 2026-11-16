using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The Spore Spawn species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SporeSpawnBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.SporeSpawn];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new SporeSpawnEntity(this, place, common);

    /// <summary>Provides boss event state, installed colors, and the room RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates stalks, spore emitters, spores, and hardening dust effects.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns visual dust and dying explosions.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Exposes the enemy-frame counter and process-all-enemies override.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves core-hitbox touch damage through shared enemy collision.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Creates item drops from Spore Spawn and its spores.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Receives Spore Spawn's sounds.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Captures the room and combat systems used by boss and spore behavior.</summary>
    internal SporeSpawnBehavior(SporeSpawnBehaviorDependencies dependencies)
    {
        _audio = dependencies.Audio;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _effectActors = dependencies.EffectActors;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _presentation = dependencies.Presentation;
        _cameraFloor = dependencies.CameraFloor;
    }

    /// <summary>The room camera's floor, which his live fight holds at his arena's top (#2243).</summary>
    private readonly RoomCameraFloor _cameraFloor;

    /// <summary>The room's presentation, which shows his colours (#2194).</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Queues a sound from the boss or one of its animations as it makes it, after the update's earlier requests.</summary>
    private void RequestSporeSpawnSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Sets Spore Spawn up when the room spawns it; his body holds his encounter from here on (#2231).</summary>
    internal void InitializeSporeSpawn(RoomEnemyPlace body)
    {
        var state = new SporeSpawnEnemyState
        {
            Movement = SporeSpawnMovement.Idle,
            StalkAnchor = body.Position.Whole + new Offset16<RoomPixels>(0, -72),
            MovementCenter = body.Position.Whole,
        };
        body.SetSpeciesState(state);
        _presentation.BossShadings.Show(BossShadingLayer.SporeSpawn, state.Colours);

        // His spores' colours, which he loads as he initializes, are the import's shade of them (#1512).
        body.Shading = state.Colours.Sprites;

        // He hangs from the ceiling by the stalk's links, the last he spawns highest (#1932).
        state.StalkLinks = [.. SporeSpawnProjectileDefinitions.StalkSegments.Select(segment => SpawnSporeSpawnStalk(body, segment)).Reverse()];

        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.InitialDead), AnimationPlace.StartOfAnimation);
            body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
            UpdateSporeSpawnStalks(body, state);
            PublishSporeSpawnObject(SporeSpawnCeilingMutation.Clear);
            return;
        }

        state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.InitialAlive), AnimationPlace.StartOfAnimation);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - SporeSpawnArenaDefinitions.FightStartRise);

        // A full sweep is a global activity override, not an enemy property.
        // Preserve that distinction so all room actors use the same scan.
        _frameContext.RequestFullSweep();
        _cameraFloor.Hold(SporeSpawnArenaDefinitions.LowestCameraY);
        foreach (Position16<RoomPixels> emitter in SporeSpawnProjectileDefinitions.Emitters)
            SpawnSporeSpawnSpawner(body, state, emitter);
        UpdateSporeSpawnStalks(body, state);
    }

    /// <summary>Runs Spore Spawn's movement for one update.</summary>
    private void RunSporeSpawnMain(
        RoomEnemyPlace body,
        SporeSpawnEnemyState state)
    {
        switch (state.Movement)
        {
            case SporeSpawnMovement.Idle:
                return;

            case SporeSpawnMovement.Descending:
                RunSporeSpawnDescent(body, state);
                return;

            case SporeSpawnMovement.Moving:
                RunSporeSpawnMovement(body, state);
                return;

            case SporeSpawnMovement.SetupDeath:
                SetupSporeSpawnDeathVelocity(body, state);
                return;

            case SporeSpawnMovement.Dying:
                RunSporeSpawnDying(body, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Spore Spawn has no behaviour for movement {state.Movement}.");
        }
    }

    /// <summary>Runs Spore Spawn's descent into the room.</summary>
    private static void RunSporeSpawnDescent(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(body, state);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
        if (CoordinateAxis.Difference(body.Position.Y.Whole, SporeSpawnArenaDefinitions.CentreY) >= 0)
        {
            state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.FightStarted), AnimationPlace.StartOfAnimation);
            body.AnimationClock.StepOnNextUpdate();
        }

        state.MaximumXRadius = SporeSpawnArenaDefinitions.DescentSwayRadius;
        state.AngleDelta = 1;
        state.Angle = RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Runs Spore Spawn's table-backed ellipse movement.</summary>
    private static void RunSporeSpawnMovement(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(body, state);
        body.Position = body.Position.WithWrappedWholeX(
            state.MovementCenter.X +
            EnemyMath.CircleCosinePixels(state.Angle.ToTrigonometryAngle(), state.MaximumXRadius));
        // The vertical sway turns twice as fast as the horizontal, from a quarter turn behind it, sixteen pixels
        // narrower. Spore Spawn only moves once the fight has started, which sets the radius to 48 or 64 first; the only
        // other way into movement is a hit on the exposed core, which only the open frames after that start carry. The
        // radius therefore never lies below sixteen when it is read.
        TrigonometryAngle swayBehind = (state.Angle - RotationAngle.QuarterTurn).ToTrigonometryAngle();
        body.Position = body.Position.WithWrappedWholeY(
            state.MovementCenter.Y +
            EnemyMath.CircleNegativeSinePixels(
                swayBehind.TurnedBy(swayBehind),
                state.MaximumXRadius - SporeSpawnArenaDefinitions.VerticalSwayNarrowing));

        // The phase advances by the signed delta and wraps around the turn.
        state.Angle = state.Angle.TurnedBySteps(state.AngleDelta);
    }

    /// <summary>Aims Spore Spawn's death drift at the arena's centre: each axis's share of one pixel per update, signed toward it.</summary>
    private static void SetupSporeSpawnDeathVelocity(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        TrigonometryAngle direction = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(SporeSpawnArenaDefinitions.CentreX, body.Position.X.Whole), CoordinateAxis.Difference(SporeSpawnArenaDefinitions.CentreY, body.Position.Y.Whole))).ToTrigonometryAngle();
        SignedSixteenSixteen across = EnemyMath.AbsoluteCosineSpeed(direction, 1).AsDisplacement();
        SignedSixteenSixteen down = EnemyMath.AbsoluteSineSpeed(direction, 1).AsDisplacement();
        state.DeathVelocity = new(
            EnemyMath.CosineIsNegative(direction) ? -across : across,
            EnemyMath.SineIsNegative(direction) ? -down : down);
    }

    /// <summary>Runs Spore Spawn's dying sequence.</summary>
    private void RunSporeSpawnDying(
        RoomEnemyPlace body,
        SporeSpawnEnemyState state)
    {
        body.Position = body.Position.AdvanceX(state.DeathVelocity.X);
        body.Position = body.Position.AdvanceY(state.DeathVelocity.Y);

        if (CoordinateAxis.Distance(body.Position.X.Whole, SporeSpawnArenaDefinitions.CentreX) < SporeSpawnArenaDefinitions.DeathDriftSettleDistance &&
            CoordinateAxis.Distance(body.Position.Y.Whole, SporeSpawnArenaDefinitions.CentreY) < SporeSpawnArenaDefinitions.DeathDriftSettleDistance)
        {
            state.Movement = SporeSpawnMovement.Idle;
        }

        UpdateSporeSpawnStalks(body, state);
        // The cadence tests the frame counter that wraps at 65536, not the separate one that wraps at 256.
        if (SporeSpawnCadenceDefinitions.IdleDustEverySixteenthUpdate.IsDue(_frameContext.FrameCounter))
        {
            int random = _room.Random.Generate();
            _ = _effectActors.SpawnRoomEffectActor(
                new(SporeSpawnRandomDefinitions.IdleDustX.Of(random) + SporeSpawnArenaDefinitions.IdleDustLeft, SporeSpawnRandomDefinitions.IdleDustY.Of(random) + SporeSpawnArenaDefinitions.IdleDustTop),
                RoomEffectActorKind.BigDustCloud,
                graphics: default);
        }
    }

    /// <summary>Places his stalk's links between the ceiling and his <paramref name="body"/>.</summary>
    private static void UpdateSporeSpawnStalks(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        WriteSporeSpawnStalkAxis(
            state.StalkLinks,
            firstPosition: SporeSpawnArenaDefinitions.CentreX,
            anchor: state.StalkAnchor.X,
            end: body.Position.X.Whole,
            axis: Axis.Horizontal);
        WriteSporeSpawnStalkAxis(
            state.StalkLinks,
            firstPosition: SporeSpawnArenaDefinitions.CeilingY,
            anchor: state.StalkAnchor.Y,
            end: CoordinateAxis.Wrap(body.Position.Y.Whole - SporeSpawnArenaDefinitions.StalkBodyGap),
            axis: Axis.Vertical);
    }

    /// <summary>
    /// Places one axis of the stalk's four links: the first at the ceiling, the others a quarter, a half and three
    /// quarters of the way from the stalk's anchor toward his body.
    /// </summary>
    private static void WriteSporeSpawnStalkAxis(
        IReadOnlyList<EnemyProjectile> links,
        int firstPosition,
        int anchor,
        int end,
        Axis axis)
    {
        // The signed distance from the anchor to the body is at most 32768 either way, so its
        // quarters never wrap; only the interpolated coordinates keep sixteen bits.
        int delta = CoordinateAxis.Difference(end, anchor);
        bool negative = delta < 0;
        int magnitude = Math.Abs(delta);
        int quarter = magnitude >> 2;
        int half = magnitude >> 1;
        int threeQuarters = quarter + half;
        Span<int> positions = stackalloc int[4]
        {
            firstPosition,
            CoordinateAxis.Wrap(anchor + (negative ? -quarter : quarter)),
            CoordinateAxis.Wrap(anchor + (negative ? -half : half)),
            CoordinateAxis.Wrap(anchor + (negative ? -threeQuarters : threeQuarters)),
        };

        for (int segment = 0; segment < positions.Length; segment++)
        {
            EnemyProjectile stalk = links[segment];
            if (axis == Axis.Horizontal)
                stalk.Position = stalk.Position.WithWholeX(positions[segment]);
            else
                stalk.Position = stalk.Position.WithWholeY(positions[segment]);
        }
    }

    /// <summary>Requests the arena ceiling room object at its fixed room block position.</summary>
    private void PublishSporeSpawnObject(SporeSpawnCeilingMutation mutation) =>
        _frameResults.SporeSpawnCeiling = mutation;

    /// <summary>Routes accepted bomb hits through the boss-specific post-damage response.</summary>
    internal EnemyBombReaction SporeSpawnBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy),
    };

    /// <summary>A spore (#1760): its spawner starts its animation, and a shot spore runs its typed shot animation.</summary>
    internal SporeSpawnProjectileBehavior Spore => field ??= new SporeSpawnProjectileBehavior(_room, _projectiles, _deathsAndDrops,
        spores: null, shot: () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SporeShot));

    /// <summary>A ceiling spore spawner (#1760): its spawner starts its animation, and a shot one restarts it closed.</summary>
    internal EnemyProjectileBehavior Spawner => field ??= new SporeSpawnProjectileBehavior(_room, _projectiles, _deathsAndDrops,
        spores: Spore, shot: () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SpawnerClosed));

    /// <summary>A link of Spore Spawn's stalk (#1760): its spawner starts its animation, and a shot one restarts it.</summary>
    internal EnemyProjectileBehavior Stalk => field ??= new SporeSpawnProjectileBehavior(_room, _projectiles, _deathsAndDrops,
        spores: Spore, shot: () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Stalk));

    /// <summary>A spore, spawner or stalk link: the spores' drift and the spawners' cadence, and their animations' cues.</summary>
    /// <param name="room">The loaded room, whose random numbers time the spawners' releases.</param>
    /// <param name="projectiles">The enemy projectiles, which hold the spores a spawner releases.</param>
    /// <param name="deathsAndDrops">The killed sound and the drops a shot spore leaves.</param>
    /// <param name="spores">The spores' behaviour, which the spores a spawner releases run; null for the spores' own.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    internal sealed class SporeSpawnProjectileBehavior(
        EnemyRoomBinding room,
        EnemyProjectileEngine projectiles,
        EnemyDeathsAndDrops deathsAndDrops,
        SporeSpawnProjectileBehavior? spores,
        Func<EnemyProjectileAnimation> shot)
        : EnemyProjectileMotionBehavior<SporeSpawnProjectileMotion>(StartingMotions, shot), IEnemyProjectileCues<SporeSpawnProjectileCue>
    {
        /// <summary>The behaviour the spores a spawner releases run: the spores' own.</summary>
        private SporeSpawnProjectileBehavior Spores => spores ?? this;

        /// <summary>A spore drifts and a spawner releases spores from its spawn; a stalk link holds still.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, SporeSpawnProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.SporeSpawnSpore] = SporeSpawnProjectileMotion.SporeDrifting,
            [RoomEnemyProjectileKind.SporeSpawnSpawner] = SporeSpawnProjectileMotion.SpawnerReleasing,
            [RoomEnemyProjectileKind.SporeSpawnStalk] = null,
        };

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, SporeSpawnProjectileMotion motion)
        {
            switch (motion)
            {
                case SporeSpawnProjectileMotion.SporeDrifting:
                    DriftSpore(context.Projectile);
                    break;
                case SporeSpawnProjectileMotion.SpawnerReleasing:
                    ReleaseSpores(context.Projectile);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SporeSpawnProjectileMotion)} {motion}.");
            }
        }

        /// <summary>Carries out a cue of the spores' and spawners' animations: spores, impact properties, the killed sound, drops and deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, SporeSpawnProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case SporeSpawnProjectileCue.SpawnSpore:
                    // Only a ceiling spawner's release spawns a spore, which takes the colours of the Spore Spawn that set the spawner up.
                    SpawnSporeSpawnSpore(projectile.Position.Whole, projectile.StateOf<SporeSpawnEmitterState>().SporeSpawn);
                    return;
                case SporeSpawnProjectileCue.SetImpactProperties:
                    SetSporeSpawnImpactProperties(projectile);
                    return;
                case SporeSpawnProjectileCue.QueueKilledSound:
                    deathsAndDrops.QueueEnemyKilledSound();
                    return;
                case SporeSpawnProjectileCue.RequestDrop:
                    RequestSporeSpawnSporeDrop(projectile, context.Samus);
                    return;
                case SporeSpawnProjectileCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SporeSpawnProjectileCue)} {cue}.");
            }
        }

        /// <summary>
        /// Spawns one spore projectile and runs its initializer. The spore shares <paramref name="sporeSpawn"/>'s body's
        /// colours as they show now (#1512).
        /// </summary>
        internal void SpawnSporeSpawnSpore(Position16<RoomPixels> position, SporeSpawnEnemyState sporeSpawn)
        {
            EnemyProjectile? spore = projectiles.AllocateEnemyProjectile();
            if (spore is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                spore,
                RoomEnemyProjectileKind.SporeSpawnSpore,
                Spores,
                SpriteGraphics.SharedIn(sporeSpawn.Colours.Sprites));
            spore.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Spore));
            spore.Position = spore.Position.WithWhole(position);

            // A spore from the right half of a screen drifts mirrored, so emitters at X 32/160 move one way and 96/224
            // the other.
            spore.State = new SporeSpawnSporeState(mirrored: RoomGridConversions.OffsetWithinScreen(position.X) >= 128);
        }

        /// <summary>Releases spores from a ceiling spawner on its random cadence while Spore Spawn lets it.</summary>
        private void ReleaseSpores(EnemyProjectile spawner)
        {
            SporeSpawnEmitterState emitter = spawner.StateOf<SporeSpawnEmitterState>();
            if (emitter.SporeSpawn.SporeGenerationSuppressed)
                return;

            if (emitter.ReleaseWait.IsZero)
            {
                spawner.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SpawnerRelease));
                emitter.ReleaseWait = WrappingCountdown.Start(room.Random.Next(SporeSpawnRandomDefinitions.SporeReleaseWait));
            }
            emitter.ReleaseWait = emitter.ReleaseWait.Decrement().Remaining;
        }

        /// <summary>Runs the spore's drop-request step.</summary>
        private void RequestSporeSpawnSporeDrop(EnemyProjectile spore, SamusState samus)
        {
            if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
            {
                throw new InvalidOperationException(
                    $"Projectile {spore.Kind} reached Spore Spawn's spore drop step.");
            }
            deathsAndDrops.SpawnEnemyDropFromChanceTable(
                spore.Position.Whole,
                SporeSpawnProjectileDefinitions.SporeDropChances,
                samus);
        }
    }

    /// <summary>Runs SporeSpawn's main AI with the encounter his <paramref name="place"/> holds.</summary>
    internal void RunSporeSpawnMainAi(RoomEnemyPlace place, SporeSpawnEnemyState state) => RunSporeSpawnMain(place, state);

    /// <summary>Plays Spore Spawn's body animation in place of the shared enemy animation.</summary>
    internal void AdvanceSporeSpawnAnimation(RoomEnemyPlace body, SporeSpawnEnemyState state, SamusState samus)
    {
        if (!body.AnimationClock.Expire())
            return;
        AnimationPlayback<SporeSpawnBodySegment> playback = state.BodyPlayback;
        // The open-pulse and death-burst repeats count the body's frame timer, as the animations' counted loops do.
        EnemyAnimationPlayback.Play(body, SporeSpawnAnimations.Segments, ref playback,
            cue =>
            {
                ApplySporeSpawnCue(body, state, cue, samus);
                return null;
            });
        state.BodyPlayback = playback;
    }

    /// <summary>A bomb striking one of Spore Spawn's dud-shot hitboxes: the dud flash where the bomb lies and the dud sound.</summary>
    internal void CreateBombDudShot(EnemyBombContext context) => _projectiles.CreateBombDudShot(context.Bomb);

    /// <summary>Restricts core-hitbox damage to accepted projectile families and handles boss death.</summary>
    internal EnemyShotReaction SporeSpawnShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes)
                throw new InvalidDataException("Spore Spawn requires its frames' hitboxes.");
            if (c.HitboxShot == EnemyReaction.DudShot)
            {
                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                return true;
            }
            if (c.HitboxShot != SporeSpawnReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Spore Spawn has no response to shot reaction {c.HitboxShot?.ToString() ?? "(unnamed)"}.");
            }
            if (!SporeSpawnAcceptsProjectile(c.ProjectileType))
            {
                // The extended collision walker has already selected the vulnerable
                // core. The core's shot reaction rejects this projectile family before common damage.
                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                return true;
            }
            return false;
        },
        ExemptFromGenericDeath = true,
        AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy),
    };

    /// <summary>Routes vulnerable hitbox touch through common damage while excluding no-op hitboxes.</summary>
    internal void ReactToSporeSpawnTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Spore Spawn requires its frames' hitboxes.");
        if (context.HitboxTouch == EnemyReaction.Ignore)
            return;
        if (context.HitboxTouch != SporeSpawnReactions.Touch)
        {
            throw new InvalidDataException(
                $"Spore Spawn has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
        ResolveSporeSpawnDeathAfterCommon(context.Place, SporeSpawnEntity.EncounterOf(context.Place));
    }
    /// <summary>
    /// Whether the vulnerable core accepts the projectile: a charged beam, or any family but an
    /// uncharged beam and a missile explosion. Of the projectile families, only those two leave the tested part of the
    /// family clear.
    /// </summary>
    private static bool SporeSpawnAcceptsProjectile(SamusProjectileType projectileType) =>
        !projectileType.IsFamily(SamusProjectileFamily.Beam) && !projectileType.IsFamily(SamusProjectileFamily.MissileExplosion) ||
        projectileType.IsChargedBeam;

    /// <summary>Resolves the rest of a shot on Spore Spawn after the common reaction.</summary>
    private void ResolveSporeSpawnShotAfterCommon(RoomEnemyPlace body)
    {
        SporeSpawnEnemyState state = SporeSpawnEntity.EncounterOf(body);
        if (body.FlashTimer != 0)
        {
            state.Movement = SporeSpawnMovement.Moving;

            // Low on health, a hit doubles his angle step while keeping its direction.
            if (SporeSpawnHealthBands.SwingsFast(body.Health))
            {
                state.AngleDelta = state.AngleDelta < 0 ? -2 : 2;
            }

            // Only the first damaging hit during an open phase reverses direction and
            // closes the head. The open-phase step clears this guard when the animation permits it.
            if (!state.Damaged)
            {
                state.AngleDelta = -state.AngleDelta;
                state.Damaged = true;
                state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.CloseAndMove), AnimationPlace.StartOfAnimation);
                body.AnimationClock.StepOnNextUpdate();
                ShowSporeSpawnHealthShade(body, state, SporeSpawnHealthBand.Of(body.Health));
            }
        }

        ResolveSporeSpawnDeathAfterCommon(body, state);
    }

    /// <summary>Resolves Spore Spawn's death after the common reaction.</summary>
    private void ResolveSporeSpawnDeathAfterCommon(
        RoomEnemyPlace body,
        SporeSpawnEnemyState state)
    {
        if (body.Health != 0)
            return;
        if (state.DeathStarted)
            return;

        state.DeathStarted = true;
        state.SporeGenerationSuppressed = false;
        body.InvincibilityTimer = 0;
        body.FlashTimer = 0;
        body.AiStates = EnemyAiStates.None;
        // The intangible property removes the dead body from the interactive-enemy list.
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

        DeleteProjectilesButStalk(state);

        state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.Death), AnimationPlace.StartOfAnimation);
        body.AnimationClock.StepOnNextUpdate();
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaMiniBoss);
        _cameraFloor.Release();
        PublishSporeSpawnObject(SporeSpawnCeilingMutation.Crumble);
    }

    /// <summary>
    /// Deletes every enemy projectile but his stalk's links, as his death does: the stalk survives for the death animation
    /// (#1932).
    /// </summary>
    internal void DeleteProjectilesButStalk(SporeSpawnEnemyState state)
    {
        foreach (EnemyProjectile projectile in _projectiles.EnemyProjectiles)
        {
            if (!state.StalkLinks.Contains(projectile))
                projectile.Delete();
        }
    }

    /// <summary>Carries out one cue of Spore Spawn's body animation.</summary>
    internal void ApplySporeSpawnCue(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnCue cue, SamusState samus)
    {
        switch (cue)
        {
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.IncreaseMaxXRadius }:
                if (state.MaximumXRadius < SporeSpawnArenaDefinitions.WidenedSwayLimit)
                    state.MaximumXRadius += SporeSpawnArenaDefinitions.SwayWidening;
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.ClearDamagedFlag }:
                state.Damaged = false;
                return;
            case SporeSpawnCue.SetMaxXRadiusAndAngleDelta setMaxXRadiusAndAngleDelta:
                state.MaximumXRadius = setMaxXRadiusAndAngleDelta.MaximumXRadius;
                state.AngleDelta = setMaxXRadiusAndAngleDelta.AngleDelta;
                return;
            case SporeSpawnCue.SetSporeGeneration setSporeGeneration:
                state.SporeGenerationSuppressed = setSporeGeneration.SporeGenerationSuppressed;
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.Harden }:
                body.Position = body.Position.WithWhole(new(SporeSpawnArenaDefinitions.CentreX, SporeSpawnArenaDefinitions.CentreY));
                body.Contact = body.Contact.Replace(EnemyContact.IgnoresSamus, EnemyContact.SolidToSamus);
                body.Processing = body.Processing.With(EnemyProcessing.Animates);
                return;
            case SporeSpawnCue.QueueSound queueSound:
                RequestSporeSpawnSound(queueSound.Sound);
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.RequestDeathDrops }:
                RequestSporeSpawnDeathDrops(samus);
                return;
            case SporeSpawnCue.SetMovement setMovement:
                state.Movement = setMovement.Movement;
                return;
            case SporeSpawnCue.ShowDeathShade showDeathShade:
                ShowSporeSpawnDeathShade(body, state, showDeathShade.DeathShadeStep);
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.SpawnHardeningDust }:
                SpawnSporeSpawnHardeningDust(body);
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.SpawnDyingExplosion }:
                SpawnSporeSpawnDyingExplosion(body);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCueKind)} {cue}.");
        }
    }

    /// <summary>Shows <paramref name="band"/>'s colours of <paramref name="state"/> on his <paramref name="body"/>.</summary>
    internal static void ShowSporeSpawnHealthShade(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnHealthBand band)
    {
        state.Colours.ShowHealth(band);
        body.Shading = state.Colours.Sprites;
    }

    /// <summary>
    /// Shows step <paramref name="step"/> of his death fade over his body, the room's level and its background (#1512),
    /// as the import's shades of it.
    /// </summary>
    internal static void ShowSporeSpawnDeathShade(RoomEnemyPlace body, SporeSpawnEnemyState state, int step)
    {
        state.Colours.ShowDeath(step);
        body.Shading = state.Colours.Sprites;
    }

    /// <summary>Draws one random number to place a hardening dust burst and request its sound.</summary>
    private void SpawnSporeSpawnHardeningDust(RoomEnemyPlace body)
    {
        int random = _room.Random.Generate();
        _projectiles.SpawnDustOrExplosion(
            new(CoordinateAxis.Wrap(body.Position.X.Whole + SporeSpawnRandomDefinitions.HardeningDustX.Of(random) - 64), CoordinateAxis.Wrap(body.Position.Y.Whole + SporeSpawnRandomDefinitions.HardeningDustY.Of(random) - 64)),
            MiscDustAnimation.BigDustCloud);
        RequestSporeSpawnSound(SporeSpawnSoundDefinitions.HardeningDust);
    }

    /// <summary>Draws one random number to place a dying explosion sprite and request its sound.</summary>
    private void SpawnSporeSpawnDyingExplosion(RoomEnemyPlace body)
    {
        int random = _room.Random.Generate();
        _ = _effectActors.SpawnRoomEffectActor(
            new(CoordinateAxis.Wrap(body.Position.X.Whole + SporeSpawnRandomDefinitions.DyingExplosionX.Of(random) - 64), CoordinateAxis.Wrap(body.Position.Y.Whole + SporeSpawnRandomDefinitions.DyingExplosionY.Of(random) - 32)),
            RoomEffectActorKind.SmallExplosion,
            graphics: default);
        RequestSporeSpawnSound(SporeSpawnSoundDefinitions.DyingExplosion);
    }

    /// <summary>Generates sixteen RNG-positioned boss drops.</summary>
    private void RequestSporeSpawnDeathDrops(SamusState samus)
    {
        for (int index = 0; index < 16; index++)
        {
            int random = _room.Random.Generate();
            Position16<RoomPixels> drop = new(SporeSpawnRandomDefinitions.DropX.Of(random) + 64, SporeSpawnRandomDefinitions.DropY.Of(random) + 528);
            _deathsAndDrops.SpawnEnemyDropFromDefinition(drop, EnemyDefinitionId.SporeSpawn, samus);
        }
    }
    /// <summary>Spawns the stalk link at <paramref name="segment"/> from his body and returns it (#1932).</summary>
    /// <exception cref="InvalidOperationException">No projectile place is free: he spawns his stalk into his room's empty places.</exception>
    internal EnemyProjectile SpawnSporeSpawnStalk(RoomEnemyPlace body, Offset16<RoomPixels> segment)
    {
        EnemyProjectile stalk = _projectiles.AllocateEnemyProjectile() ?? throw new InvalidOperationException(
            "Spore Spawn's stalk found no free projectile place; he spawns it as his room loads, into its empty places.");

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            stalk,
            RoomEnemyProjectileKind.SporeSpawnStalk,
            Stalk,
            SpriteGraphics.SpawnedBy(body));
        stalk.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Stalk));
        stalk.Position = stalk.Position.WithWhole(body.Position.Whole + segment);
        return stalk;
    }

    /// <summary>Spawns the ceiling spore emitter at <paramref name="emitter"/> for <paramref name="state"/>'s fight and runs its initializer.</summary>
    internal void SpawnSporeSpawnSpawner(RoomEnemyPlace body, SporeSpawnEnemyState state, Position16<RoomPixels> emitter)
    {
        EnemyProjectile? spawner = _projectiles.AllocateEnemyProjectile();
        if (spawner is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spawner,
            RoomEnemyProjectileKind.SporeSpawnSpawner,
            Spawner,
            SpriteGraphics.SpawnedBy(body));
        spawner.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SpawnerClosed));
        spawner.Position = spawner.Position.WithWhole(emitter);
        spawner.State = new SporeSpawnEmitterState(state);
    }

    /// <summary>Drifts a spore one step along its path, deleting it once it falls below the room.</summary>
    internal static void DriftSpore(EnemyProjectile spore)
    {
        SporeSpawnSporeState drift = spore.StateOf<SporeSpawnSporeState>();
        Offset16<RoomPixels> movement = SporeSpawnProjectiles.DriftAt(drift.PathStep);
        int xDelta = movement.X;
        if (drift.Mirrored)
            xDelta = -xDelta;
        spore.Position = spore.Position.WithWrappedWholeX(spore.Position.X.Whole + xDelta);

        int yDelta = movement.Y;
        spore.Position = spore.Position.WithWrappedWholeY(spore.Position.Y.Whole + yDelta + yDelta);
        if (CoordinateAxis.Difference(spore.Position.Y.Whole, 768) >= 0)
            spore.Delete();

        drift.AdvancePath();
    }

    /// <summary>Runs the spore's impact step, which replaces its whole property set.</summary>
    internal static void SetSporeSpawnImpactProperties(EnemyProjectile spore)
    {
        if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
        {
            throw new InvalidOperationException(
                $"Projectile {spore.Kind} reached Spore Spawn's spore impact step.");
        }

        // The new properties: damage zero, high priority, Samus collision enabled, projectile
        // collision disabled, and deletion on contact. Drawing ignores the priority,
        // but every collision bit is retained explicitly.
        spore.Damage = 0;
        spore.CanDamageSamus = true;
        spore.PersistsOnSamusContact = false;
        spore.BlocksSamusProjectiles = false;
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindSporeSpawnHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
            SporeSpawnCollision.IsFrame(enemy.Frame)))
            return null;

        // Every rectangle and reaction of these twelve frames comes from Spore Spawn's collision catalog.
        foreach (EnemyCollisionComponent component in
                 SporeSpawnCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top),
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom)))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }
}
