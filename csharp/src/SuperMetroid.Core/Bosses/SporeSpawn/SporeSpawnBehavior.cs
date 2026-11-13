using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus.Movement;

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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SporeSpawnEntity(this, slot, common);

    /// <summary>Provides boss event state, installed colors, and the room RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates stalks, spore emitters, spores, and hardening dust effects.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns visual dust and dying explosions.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
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
        _spriteObjects = dependencies.SpriteObjects;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>Fixed horizontal arena center used for hardening and the death trajectory.</summary>
    private const int SporeSpawnDeathCenterX = 128;
    /// <summary>Fixed vertical arena center used for hardening and the death trajectory.</summary>
    private const int SporeSpawnDeathCenterY = 624;
    /// <summary>Horizontal sway amplitude in room pixels the descent leaves for the fight's first sway.</summary>
    private const int SporeSpawnDescentMaximumXRadius = 48;
    /// <summary>Sway amplitude in room pixels below which each closing widens the sway.</summary>
    private const int SporeSpawnWidenedRadiusLimit = 40;
    /// <summary>Room pixels each closing adds to a sway narrower than the limit.</summary>
    private const int SporeSpawnRadiusWidening = 8;
    /// <summary>Ceiling Y coordinate anchoring the four stalk projectiles.</summary>
    private const int SporeSpawnCeilingY = 560;

    /// <summary>Queues a sound from the boss or one of its animations as it makes it, after the update's earlier requests.</summary>
    private void RequestSporeSpawnSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>
    /// Runs the global scrolling-finished behaviour installed by live Spore Spawn.
    /// The camera invokes it after both ordinary camera axes have
    /// finished and before background streaming observes the final camera position.
    /// </summary>
    /// <param name="camera">Live room camera whose Y is raised to the arena minimum of 464 room pixels when the hook is active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="camera"/> is null.</exception>
    public void RunScrollingFinishedHook(ScrollBoundaryCamera camera)
    {
        Ensure.NotNull(camera);
        if (SporeSpawn?.ScrollClampHookActive != true)
            return;

        // The hook loads the arena minimum of 464, compares it with the camera's Y, and returns when it is below.
        // Equality deliberately performs the harmless store; a larger camera Y is
        // already below the arena ceiling and is therefore left untouched.
        if (SporeSpawnScrollingHooks.FightMinimumCameraY < camera.PixelPosition.Y)
            return;

        camera.SetCameraYFromScrollingFinishedHook(
            SporeSpawnScrollingHooks.FightMinimumCameraY);
    }

    /// <summary>Gets the single boss state and verifies it belongs to the supplied body slot.</summary>
    private SporeSpawnEnemyState RequireSporeSpawnState(RoomEnemySlot slot)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn main AI ran without an initialized boss extension.");
        if (!ReferenceEquals(state.Body, slot))
            throw new InvalidOperationException("Spore Spawn extension belongs to another enemy slot.");
        return state;
    }

    /// <summary>Sets Spore Spawn up when the room spawns it.</summary>
    internal void InitializeSporeSpawn(RoomEnemySlot body)
    {
        if (SporeSpawn is not null)
            throw new InvalidDataException("A room population contains more than one Spore Spawn body.");

        var state = new SporeSpawnEnemyState(body)
        {
            Movement = SporeSpawnMovement.Idle,
            StalkAnchor = body.Position.Whole + new Offset16<RoomPixels>(0, -72),
            MovementCenter = body.Position.Whole,
        };
        SporeSpawn = state;

        // His spores' colours, which he loads as he initializes, are the import's shade of them (#1512).
        body.Shading = state.Colours.Sprites;

        // The projectile spawner searches the eighteen-slot pool downward. Four calls therefore
        // occupy the pool's places 17..14, which the stalk update later reaches directly.
        for (int argument = 0; argument < 4; argument++)
            SpawnSporeSpawnStalk(body, argument);

        state.LoadedAsDefeated = _room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss);
        if (state.LoadedAsDefeated)
        {
            state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.InitialDead), AnimationPlace.StartOfAnimation);
            body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
            UpdateSporeSpawnStalks(state);
            PublishSporeSpawnObject(SporeSpawnCeilingMutation.Clear);
            return;
        }

        state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.InitialAlive), AnimationPlace.StartOfAnimation);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 128);

        // A full sweep is a global activity override, not an enemy property.
        // Preserve that distinction so all room actors use the same scan.
        _frameContext.RequestFullSweep();
        state.ScrollClampHookActive = true;
        for (int argument = 0; argument < 4; argument++)
            SpawnSporeSpawnSpawner(body, argument);
        UpdateSporeSpawnStalks(state);
    }

    /// <summary>Dispatches Spore Spawn's per-frame behaviour.</summary>
    private void RunSporeSpawnMain(
        RoomEnemySlot body,
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
    private void RunSporeSpawnDescent(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(state);
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
        if (CoordinateAxis.Difference(body.Position.Y.Whole, SporeSpawnDeathCenterY) >= 0)
        {
            state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.FightStarted), AnimationPlace.StartOfAnimation);
            body.AnimationClock.StepOnNextUpdate();
        }

        state.MaximumXRadius = SporeSpawnDescentMaximumXRadius;
        state.AngleDelta = 1;
        state.Angle = RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Runs Spore Spawn's table-backed ellipse movement.</summary>
    private void RunSporeSpawnMovement(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(state);
        body.Position = body.Position.WithWrappedWholeX(
            state.MovementCenter.X +
            EnemyMath.ReadEightBitCosineProduct(state.Angle.ToTrigonometryAngle(), state.MaximumXRadius));
        // The vertical sway turns twice as fast as the horizontal, from a quarter turn behind it, sixteen pixels
        // narrower. Spore Spawn only moves once the fight has started, which sets the radius to 48 or 64 first; the only
        // other way into movement is a hit on the exposed core, which only the open frames after that start carry. The
        // radius therefore never lies below sixteen when it is read.
        TrigonometryAngle swayBehind = (state.Angle - RotationAngle.QuarterTurn).ToTrigonometryAngle();
        body.Position = body.Position.WithWrappedWholeY(
            state.MovementCenter.Y +
            EnemyMath.ReadEightBitNegativeSineProduct(
                swayBehind.TurnedBy(swayBehind),
                state.MaximumXRadius - 16));

        // The phase advances by the signed delta and wraps around the turn.
        state.Angle = state.Angle.TurnedBySteps(state.AngleDelta);
    }

    /// <summary>Sets up Spore Spawn's death velocity.</summary>
    private static void SetupSporeSpawnDeathVelocity(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        TrigonometryAngle direction = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(SporeSpawnDeathCenterX, body.Position.X.Whole), CoordinateAxis.Difference(SporeSpawnDeathCenterY, body.Position.Y.Whole))).ToTableAngle();
        state.DeathAngle = direction.ToAngle();
        state.DeathVelocityMagnitude = new(
            EnemyMath.AbsoluteCosineSpeed(direction, 1).AsDisplacement(),
            EnemyMath.AbsoluteSineSpeed(direction, 1).AsDisplacement());
    }

    /// <summary>Runs Spore Spawn's dying sequence.</summary>
    private void RunSporeSpawnDying(
        RoomEnemySlot body,
        SporeSpawnEnemyState state)
    {
        SignedSixteenSixteen xDisplacement = EnemyMath.CosineIsNegative(state.DeathAngle.ToTrigonometryAngle())
            ? SignedSixteenSixteen.Zero - state.DeathVelocityMagnitude.X
            : state.DeathVelocityMagnitude.X;
        SignedSixteenSixteen yDisplacement = EnemyMath.SineIsNegative(state.DeathAngle.ToTrigonometryAngle())
            ? SignedSixteenSixteen.Zero - state.DeathVelocityMagnitude.Y
            : state.DeathVelocityMagnitude.Y;
        body.Position = body.Position.AdvanceX(xDisplacement);
        body.Position = body.Position.AdvanceY(yDisplacement);

        if (CoordinateAxis.Distance(body.Position.X.Whole, SporeSpawnDeathCenterX) < 8 &&
            CoordinateAxis.Distance(body.Position.Y.Whole, SporeSpawnDeathCenterY) < 8)
        {
            state.Movement = SporeSpawnMovement.Idle;
        }

        UpdateSporeSpawnStalks(state);
        // The cadence tests the frame counter that wraps at 65536, not the separate one that wraps at 256.
        if (SporeSpawnCadenceDefinitions.IdleDustEverySixteenthUpdate.IsDue(_frameContext.FrameCounter))
        {
            int random = _room.Random.Generate();
            _ = _spriteObjects.SpawnRoomSpriteObject(
                new(SporeSpawnRandomDefinitions.IdleDustX.Of(random) + 96, SporeSpawnRandomDefinitions.IdleDustY.Of(random) + 480),
                RoomSpriteObjectKind.DustCloud,
                graphics: default);
        }
    }

    /// <summary>Updates the stalk segments by raw slot interpolation between the body and the ceiling.</summary>
    private void UpdateSporeSpawnStalks(SporeSpawnEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        WriteSporeSpawnStalkAxis(
            firstPosition: SporeSpawnDeathCenterX,
            anchor: state.StalkAnchor.X,
            end: body.Position.X.Whole,
            axis: Axis.Horizontal);
        WriteSporeSpawnStalkAxis(
            firstPosition: SporeSpawnCeilingY,
            anchor: state.StalkAnchor.Y,
            end: CoordinateAxis.Wrap(body.Position.Y.Whole - 40),
            axis: Axis.Vertical);
    }

    /// <summary>Interpolates one axis of four physical stalk slots from the ceiling toward the body.</summary>
    private void WriteSporeSpawnStalkAxis(
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

        // The boss writes projectile places 14,15,16,17 in this order even though
        // its four initializer calls allocated them in reverse order.
        for (int segment = 0; segment < positions.Length; segment++)
        {
            EnemyProjectile stalk = _projectiles.EnemyProjectiles[14 + segment];
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
    internal void AddSporeSpawnBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
                                                       c.SelectedShotAi == EnemyReaction.SporeSpawnShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Binds spore movement, emitter timing, impact properties, and drop steps.</summary>
    internal void AddSporeSpawnProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunSporeSpawnSporeFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.SporeDrift);

        table.AddFrameBehavior(context =>
        {
            RunSporeSpawnSpawnerFrameBehavior(context.Projectile);
        }, EnemyProjectileFrameBehavior.SporeSpawnerCadence);

        table.AddShotAnimation(RoomEnemyProjectileKind.SporeSpawnSpore,
            () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SporeShot));
        table.AddShotAnimation(RoomEnemyProjectileKind.SporeSpawnSpawner,
            () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SpawnerClosed));
        table.AddShotAnimation(RoomEnemyProjectileKind.SporeSpawnStalk,
            () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Stalk));
        table.AddCue<SporeSpawnProjectileCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case SporeSpawnProjectileCue.SpawnSpore:
                    SpawnSporeSpawnSpore(projectile.Position.Whole);
                    return;
                case SporeSpawnProjectileCue.SetImpactProperties:
                    SetSporeSpawnImpactProperties(projectile);
                    return;
                case SporeSpawnProjectileCue.QueueKilledSound:
                    _deathsAndDrops.QueueEnemyKilledSound();
                    return;
                case SporeSpawnProjectileCue.RequestDrop:
                    RequestSporeSpawnSporeDrop(projectile);
                    return;
                case SporeSpawnProjectileCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SporeSpawnProjectileCue)} {cue}.");
            }
        });
    }

    /// <summary>Runs SporeSpawn's main AI.</summary>
    internal void RunSporeSpawnMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunSporeSpawnMain(slot, RequireSporeSpawnState(slot));
    }

    /// <summary>Plays Spore Spawn's body animation in place of the shared enemy animation.</summary>
    internal void AdvanceSporeSpawnAnimation(RoomEnemySlot body)
    {
        if (!body.AnimationClock.Expire())
            return;
        SporeSpawnEnemyState state = RequireSporeSpawnState(body);
        AnimationPlayback<SporeSpawnBodySegment> playback = state.BodyPlayback;
        // The open-pulse and death-burst repeats count the body's frame timer, as the animations' counted loops do.
        EnemyAnimationPlayback.Play(body, SporeSpawnAnimations.Segments, ref playback, SporeSpawnFrameDefinitions.ShownFrames.Of,
            cue =>
            {
                ApplySporeSpawnCue(body, state, cue);
                return null;
            });
        state.BodyPlayback = playback;
    }

    /// <summary>A bomb striking one of Spore Spawn's dud-shot hitboxes: the dud flash where the bomb lies and the dud sound.</summary>
    internal void CreateBombDudShot(EnemyBombContext context) => _projectiles.CreateBombDudShot(context.Bomb);

    /// <summary>Restricts core-hitbox damage to accepted projectile families and handles boss death.</summary>
    internal void AddSporeSpawnShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.SporeSpawnShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesFrameHitboxes)
                    throw new InvalidDataException("Spore Spawn requires its frames' hitboxes.");
                if (c.HitboxShotAi == EnemyReaction.DudShot)
                {
                    c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                    _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                    return true;
                }
                if (c.HitboxShotAi != EnemyReaction.SporeSpawnShot)
                {
                    throw new InvalidDataException(
                        $"Spore Spawn has no response to shot reaction {c.HitboxShotAi?.ToString() ?? "(unnamed)"}.");
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
        });
    }

    /// <summary>Routes vulnerable hitbox touch through common damage while excluding no-op hitboxes.</summary>
    internal void AddSporeSpawnTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.SporeSpawn &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.SporeSpawnTouch,
            context =>
            {
                if (!context.UsesFrameHitboxes)
                    throw new InvalidDataException("Spore Spawn requires its frames' hitboxes.");
                if (context.HitboxTouchAi == EnemyReaction.Ignore)
                    return;
                if (context.HitboxTouchAi != EnemyReaction.SporeSpawnTouch)
                {
                    throw new InvalidDataException(
                        $"Spore Spawn has no response to touch reaction {context.HitboxTouchAi?.ToString() ?? "(unnamed)"}.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                ResolveSporeSpawnDeathAfterCommon(context.Slot);
            });
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
    private void ResolveSporeSpawnShotAfterCommon(RoomEnemySlot body)
    {
        SporeSpawnEnemyState state = RequireSporeSpawnState(body);
        if (body.FlashTimer != 0)
        {
            state.Movement = SporeSpawnMovement.Moving;

            // Below 400 health the code doubles angular speed while preserving its
            // current sign. At exactly 400 it leaves the existing delta untouched.
            if (body.Health < 400)
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

                int healthFrame = body.Health >= 770
                    ? 0
                    : body.Health >= 410
                        ? 1
                        : body.Health >= 70
                            ? 2
                            : 3;
                if (body.Health != state.PreviousHealth)
                {
                    state.PreviousHealth = body.Health;
                    LoadSporeSpawnHealthPalette(healthFrame);
                }
            }
        }

        ResolveSporeSpawnDeathAfterCommon(body, state);
    }

    /// <summary>Resolves Spore Spawn's death after the common reaction.</summary>
    private void ResolveSporeSpawnDeathAfterCommon(
        RoomEnemySlot body,
        SporeSpawnEnemyState? suppliedState = null)
    {
        if (body.Health != 0)
            return;
        SporeSpawnEnemyState state = suppliedState ?? RequireSporeSpawnState(body);
        if (state.DeathStarted)
            return;

        state.DeathStarted = true;
        state.SporeGenerationSuppressed = false;
        body.InvincibilityTimer = 0;
        body.FlashTimer = 0;
        body.AiStates = EnemyAiStates.None;
        // The intangible property removes the dead body from the interactive-enemy list.
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

        // The clear walks the projectile slots downward from 13 to 0.
        // Slots 14..17 are the stalk and deliberately survive for the death animation.
        for (int projectileSlot = 0; projectileSlot <= 13; projectileSlot++)
            _projectiles.EnemyProjectiles[projectileSlot].Delete();

        state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.Death), AnimationPlace.StartOfAnimation);
        body.AnimationClock.StepOnNextUpdate();
        _room.Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);
        state.ScrollClampHookActive = false;
        PublishSporeSpawnObject(SporeSpawnCeilingMutation.Crumble);
    }

    /// <summary>Carries out one cue of Spore Spawn's body animation.</summary>
    internal void ApplySporeSpawnCue(RoomEnemySlot body, SporeSpawnEnemyState state, SporeSpawnCue cue)
    {
        switch (cue)
        {
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.IncreaseMaxXRadius }:
                if (state.MaximumXRadius < SporeSpawnWidenedRadiusLimit)
                    state.MaximumXRadius += SporeSpawnRadiusWidening;
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
                body.Position = body.Position.WithWhole(new(SporeSpawnDeathCenterX, SporeSpawnDeathCenterY));
                body.Contact = body.Contact.Replace(EnemyContact.IgnoresSamus, EnemyContact.SolidToSamus);
                body.Processing = body.Processing.With(EnemyProcessing.Animates);
                return;
            case SporeSpawnCue.QueueSound queueSound:
                RequestSporeSpawnSound(queueSound.Sound ?? throw new InvalidDataException("A Spore Spawn sound cue carries no sound."));
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.RequestDeathDrops }:
                RequestSporeSpawnDeathDrops();
                return;
            case SporeSpawnCue.SetMovement setMovement:
                state.Movement = setMovement.Movement;
                return;
            case SporeSpawnCue.LoadDeathPalette loadDeathPalette:
                LoadSporeSpawnDeathPalette(loadDeathPalette.DeathPaletteStep);
                return;
            case SporeSpawnCue.LoadDeathTargetPalette loadDeathTargetPalette:
                LoadSporeSpawnDeathPalette(loadDeathTargetPalette.DeathPaletteStep);
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.SpawnHardeningDust }:
                SpawnSporeSpawnHardeningDust(state);
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.SpawnDyingExplosion }:
                SpawnSporeSpawnDyingExplosion(state);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCueKind)} {cue}.");
        }
    }

    /// <summary>Shows health-stage body image <paramref name="frame"/>.</summary>
    internal void LoadSporeSpawnHealthPalette(int frame)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn's health colours show once Spore Spawn is loaded.");
        state.Colours.ShowHealth(Ensure.BetweenInclusive(0, SporeSpawnColorDefinitions.HealthFrameCount - 1, frame));
        state.Body.Shading = state.Colours.Sprites;
    }

    /// <summary>
    /// Shows step <paramref name="step"/> of his death fade over his body, the room's level and its background (#1512),
    /// as the import's shades of it. A defeated room's initialization loads it as the fade's target only, which the door's
    /// fade then brings in, so both copies show it.
    /// </summary>
    internal void LoadSporeSpawnDeathPalette(int step)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn palette cue ran without the boss state.");
        state.Colours.ShowDeath(step);
        state.Body.Shading = state.Colours.Sprites;
    }

    /// <summary>Draws one random number to place a hardening dust burst and request its sound.</summary>
    private void SpawnSporeSpawnHardeningDust(SporeSpawnEnemyState state)
    {
        int random = _room.Random.Generate();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + SporeSpawnRandomDefinitions.HardeningDustX.Of(random) - 64), CoordinateAxis.Wrap(state.Body.Position.Y.Whole + SporeSpawnRandomDefinitions.HardeningDustY.Of(random) - 64)),
            MiscDustAnimation.BigDustCloud);
        RequestSporeSpawnSound(SporeSpawnSoundDefinitions.HardeningDust);
    }

    /// <summary>Draws one random number to place a dying explosion sprite and request its sound.</summary>
    private void SpawnSporeSpawnDyingExplosion(SporeSpawnEnemyState state)
    {
        int random = _room.Random.Generate();
        _ = _spriteObjects.SpawnRoomSpriteObject(
            new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + SporeSpawnRandomDefinitions.DyingExplosionX.Of(random) - 64), CoordinateAxis.Wrap(state.Body.Position.Y.Whole + SporeSpawnRandomDefinitions.DyingExplosionY.Of(random) - 32)),
            RoomSpriteObjectKind.SporeSpawnDyingExplosion,
            graphics: default);
        RequestSporeSpawnSound(SporeSpawnSoundDefinitions.DyingExplosion);
    }

    /// <summary>Generates sixteen RNG-positioned boss drops.</summary>
    private void RequestSporeSpawnDeathDrops()
    {
        for (int index = 0; index < 16; index++)
        {
            int random = _room.Random.Generate();
            Position16<RoomPixels> drop = new(SporeSpawnRandomDefinitions.DropX.Of(random) + 64, SporeSpawnRandomDefinitions.DropY.Of(random) + 528);
            _deathsAndDrops.SpawnEnemyDropFromDefinition(drop, EnemyDefinitionId.SporeSpawn);
        }
    }
    /// <summary>Graphics bits assigned to newly spawned spores: his body's colours as they show now (#1512).</summary>
    private SpriteGraphics SporeSpawnGraphicsIndex => SpriteGraphics.SharedIn((SporeSpawn
        ?? throw new InvalidOperationException("Spore Spawn's spores spawn once Spore Spawn is loaded.")).Colours.Sprites);

    /// <summary>Spawns Spore Spawn's stalk projectile and runs its initializer.</summary>
    internal void SpawnSporeSpawnStalk(RoomEnemySlot body, int spawnArgument)
    {
        if (spawnArgument is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        EnemyProjectile? stalk = _projectiles.AllocateEnemyProjectile();
        if (stalk is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            stalk,
            RoomEnemyProjectileKind.SporeSpawnStalk,
            SpriteGraphics.SpawnedBy(body));
        stalk.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Stalk));
        stalk.Position = stalk.Position.WithWholeX(body.Position.X.Whole);
        stalk.Position = stalk.Position.WithWrappedWholeY(
            body.Position.Y.Whole +
            SporeSpawnProjectiles.StalkYOffset(spawnArgument));
    }

    /// <summary>Spawns Spore Spawn's spore-spawner projectile and runs its initializer.</summary>
    internal void SpawnSporeSpawnSpawner(RoomEnemySlot body, int spawnArgument)
    {
        if (spawnArgument is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        EnemyProjectile? spawner = _projectiles.AllocateEnemyProjectile();
        if (spawner is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spawner,
            RoomEnemyProjectileKind.SporeSpawnSpawner,
            SpriteGraphics.SpawnedBy(body));
        spawner.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SpawnerClosed));
        spawner.Position = spawner.Position.WithWholeX(SporeSpawnProjectiles.SpawnerX(spawnArgument));
        spawner.Position = spawner.Position.WithWholeY(520);
        spawner.State = new SporeSpawnEmitterState();
    }

    /// <summary>Spawns one spore projectile and runs its initializer.</summary>
    internal void SpawnSporeSpawnSpore(Position16<RoomPixels> position)
    {
        EnemyProjectile? spore = _projectiles.AllocateEnemyProjectile();
        if (spore is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spore,
            RoomEnemyProjectileKind.SporeSpawnSpore,
            SporeSpawnGraphicsIndex);
        spore.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Spore));
        spore.Position = spore.Position.WithWhole(position);

        // A spore from the right half of a screen drifts mirrored, so emitters at X 32/160 move one way and 96/224
        // the other.
        spore.State = new SporeSpawnSporeState(mirrored: RoomGridConversions.OffsetWithinScreen(position.X) >= 128);
    }

    /// <summary>Runs a spore's per-frame drift.</summary>
    internal static void RunSporeSpawnSporeFrameBehavior(EnemyProjectile spore)
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

    /// <summary>Runs a spore emitter's per-frame behaviour.</summary>
    internal void RunSporeSpawnSpawnerFrameBehavior(EnemyProjectile spawner)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "A Spore Spawn ceiling emitter survived without its owning boss state.");
        if (state.SporeGenerationSuppressed)
            return;

        SporeSpawnEmitterState emitter = spawner.StateOf<SporeSpawnEmitterState>();
        if (emitter.ReleaseWait.IsZero)
        {
            spawner.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.SpawnerRelease));
            emitter.ReleaseWait = WrappingCountdown.Start(_room.Random.Next(SporeSpawnRandomDefinitions.SporeReleaseWait));
        }
        emitter.ReleaseWait = emitter.ReleaseWait.Decrement().Remaining;
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

    /// <summary>Runs the spore's drop-request step.</summary>
    internal void RequestSporeSpawnSporeDrop(EnemyProjectile spore)
    {
        if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
        {
            throw new InvalidOperationException(
                $"Projectile {spore.Kind} reached Spore Spawn's spore drop step.");
        }
        _deathsAndDrops.SpawnEnemyDropFromDefinition(
            spore.Position.Whole,
            EnemyDefinitionId.SporeSpawnStalk);
    }

    /// <summary>Single active boss state, absent outside a room containing Spore Spawn.</summary>
    internal SporeSpawnEnemyState? SporeSpawn { get; set; }

    /// <summary>Publishes Spore Spawn's pending target palette writes, if the boss is present.</summary>

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindSporeSpawnHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
            SporeSpawnCollision.IsFrame(enemy.Frame)))
            return null;

        // These twelve frames are selected by Spore Spawn. Old visual
        // overrides may carry Draygon-prefixed keys, but every rectangle
        // and reaction comes from the compiled gameplay catalog.
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
