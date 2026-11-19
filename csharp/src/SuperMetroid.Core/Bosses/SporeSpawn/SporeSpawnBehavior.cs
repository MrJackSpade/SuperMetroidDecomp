using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
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
            StalkAnchor = body.Position.Whole + SporeSpawnArenaDefinitions.StalkAnchorAboveBody,
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
            state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.Husk), AnimationPlace.StartOfAnimation);
            body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
            UpdateSporeSpawnStalks(body, state);
            PublishSporeSpawnObject(SporeSpawnCeilingMutation.Clear);
            return;
        }

        state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.AwaitingDescent), AnimationPlace.StartOfAnimation);
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole - SporeSpawnArenaDefinitions.FightStartRise);

        // His fight runs every enemy in the room each update, wherever the camera is.
        _frameContext.RequestFullSweep();
        _cameraFloor.Hold(SporeSpawnArenaDefinitions.LowestCameraY);
        foreach (SporeSpawnEmitter emitter in SporeSpawnProjectileDefinitions.Emitters)
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
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + SporeSpawnArenaDefinitions.DescentSpeed);
        if (body.Position.Y.Whole >= SporeSpawnArenaDefinitions.CentreY)
        {
            StartSporeSpawnAnimation(body, state, SporeSpawnBodySegment.FightStarted);
        }

        state.SwayRadius = SporeSpawnArenaDefinitions.DescentSwayRadius;
        state.SwayStep = SporeSpawnArenaDefinitions.SwayStep;
        state.Angle = RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Sways Spore Spawn round his ellipse about his movement's centre.</summary>
    private static void RunSporeSpawnMovement(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(body, state);
        body.Position = body.Position.WithWholeX(
            state.MovementCenter.X +
            EnemyMath.CircleCosinePixels(state.Angle.ToTrigonometryAngle(), state.SwayRadius.Reach.X));
        // The vertical sway turns twice as fast as the horizontal, from a quarter turn behind it, sixteen pixels
        // narrower. Spore Spawn only moves once the fight has started, which sets the radius to 48 or 64 first; the only
        // other way into movement is a hit on the exposed core, which only the open frames after that start carry. The
        // radius therefore never lies below sixteen when it is read.
        TrigonometryAngle swayBehind = (state.Angle - RotationAngle.QuarterTurn).ToTrigonometryAngle();
        body.Position = body.Position.WithWholeY(
            state.MovementCenter.Y +
            EnemyMath.CircleNegativeSinePixels(
                swayBehind.TurnedBy(swayBehind),
                state.SwayRadius.Reach.Y));

        // The phase turns by his sway's step and wraps around the turn.
        state.Angle = state.SwayStep.Turn(state.Angle);
    }

    /// <summary>Aims Spore Spawn's death drift at the arena's centre: a pixel an update along his bearing to it.</summary>
    private static void SetupSporeSpawnDeathVelocity(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        TrigonometryAngle direction = EnemyMath.CalculateAimAngle(
            new(SporeSpawnArenaDefinitions.CentreX - body.Position.X.Whole, SporeSpawnArenaDefinitions.CentreY - body.Position.Y.Whole));
        state.DeathVelocity = EnemyMath.VelocityAlong(direction, 1);
    }

    /// <summary>Runs Spore Spawn's dying sequence.</summary>
    private void RunSporeSpawnDying(
        RoomEnemyPlace body,
        SporeSpawnEnemyState state)
    {
        body.Position = body.Position.AdvanceX(state.DeathVelocity.X);
        body.Position = body.Position.AdvanceY(state.DeathVelocity.Y);

        if (Math.Abs(body.Position.X.Whole - SporeSpawnArenaDefinitions.CentreX) < SporeSpawnArenaDefinitions.DeathDriftSettleDistance &&
            Math.Abs(body.Position.Y.Whole - SporeSpawnArenaDefinitions.CentreY) < SporeSpawnArenaDefinitions.DeathDriftSettleDistance)
        {
            state.Movement = SporeSpawnMovement.Idle;
        }

        UpdateSporeSpawnStalks(body, state);
        // The dust follows the game's frame counter.
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
            end: body.Position.Y.Whole - SporeSpawnArenaDefinitions.StalkBodyGap,
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
        // The links lie a quarter, a half and three quarters of the way along the distance from the anchor to the
        // body, each fraction of the distance rounded down.
        int delta = end - anchor;
        bool negative = delta < 0;
        int magnitude = Math.Abs(delta);
        int quarter = magnitude / 4;
        int half = magnitude / 2;
        int threeQuarters = quarter + half;
        Span<int> positions = stackalloc int[4]
        {
            firstPosition,
            anchor + (negative ? -quarter : quarter),
            anchor + (negative ? -half : half),
            anchor + (negative ? -threeQuarters : threeQuarters),
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

    /// <summary>Routes accepted bomb hits on <paramref name="body"/> through his encounter's post-damage response.</summary>
    /// <param name="body">His body, whose encounter the response acts on.</param>
    internal EnemyBombReaction SporeSpawnBombReactionOf(SporeSpawnEntity body)
    {
        Ensure.NotNull(body);
        return new()
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy, body.Encounter),
        };
    }

    /// <summary>A spore (#1760, #2410): it drifts from its spawn, and a shot one bursts, harmless, then drops and deletes itself.</summary>
    private SporeSpawnSporeBehavior Spore => field ??= new SporeSpawnSporeBehavior(_deathsAndDrops);

    /// <summary>A ceiling spore spawner (#1760, #2410): it releases spores on its random cadence, and a shot one restarts closed.</summary>
    private SporeSpawnSpawnerBehavior Spawner => field ??= new SporeSpawnSpawnerBehavior(_room, _projectiles, Spore);

    /// <summary>A link of Spore Spawn's stalk (#1760): it holds still, and a shot one restarts its pose.</summary>
    private EnemyProjectileBehavior Stalk => field ??= new EnemyProjectileBehavior(shot: () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment.Stalk));

    /// <summary>Runs SporeSpawn's main AI with the encounter his <paramref name="place"/> holds.</summary>
    internal void RunSporeSpawnMainAi(RoomEnemyPlace place, SporeSpawnEnemyState state) => RunSporeSpawnMain(place, state);

    /// <summary>
    /// Starts his body's animation at <paramref name="segment"/> (#2335): its first step plays on the next update, from the
    /// animation's start.
    /// </summary>
    private static void StartSporeSpawnAnimation(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnBodySegment segment) =>
        state.BodyPlayback = EnemyAnimationPlayback.Start(body, SporeSpawnAnimations.StartingWith(segment));

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

    /// <summary>Restricts core-hitbox damage on <paramref name="body"/> to accepted projectile families and handles his death.</summary>
    /// <param name="body">His body, whose encounter the rest of an accepted shot acts on.</param>
    internal EnemyShotReaction SporeSpawnShotReactionOf(SporeSpawnEntity body)
    {
        Ensure.NotNull(body);
        return new()
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                // His frames' hitboxes meet a shot with his shell's dud or his core's own reaction.
                if (c.HitboxShot == EnemyReaction.DudShot)
                {
                    c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                    _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                    return true;
                }
                if (!SporeSpawnAcceptsProjectile(c.ProjectileType))
                {
                    // The shot has struck the vulnerable core, whose shot reaction rejects it before common damage.
                    c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                    return true;
                }
                return false;
            },
            ExemptFromGenericDeath = true,
            AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy, body.Encounter),
        };
    }

    /// <summary>
    /// Routes vulnerable hitbox touch on his body through common damage, then resolves <paramref name="state"/>'s death; a
    /// touch on a hitbox that does nothing does nothing.
    /// </summary>
    /// <param name="context">The touch.</param>
    /// <param name="state">The encounter his body holds.</param>
    internal void ReactToSporeSpawnTouch(EnemyTouchContext context, SporeSpawnEnemyState state)
    {
        // His frames' hitboxes meet a touch with his hardened husk's shell, which does nothing, or his own reaction.
        if (context.HitboxTouch == EnemyReaction.Ignore)
            return;
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
        ResolveSporeSpawnDeathAfterCommon(context.Place, state);
    }

    /// <summary>
    /// Whether the vulnerable core accepts the projectile: a charged beam, or any family but an
    /// uncharged beam and a missile explosion.
    /// </summary>
    private static bool SporeSpawnAcceptsProjectile(SamusProjectileType projectileType) =>
        !projectileType.IsFamily(SamusProjectileFamily.Beam) && !projectileType.IsFamily(SamusProjectileFamily.MissileExplosion) ||
        projectileType.IsChargedBeam;

    /// <summary>
    /// Resolves the rest of a shot or bomb on Spore Spawn's <paramref name="body"/>, of encounter <paramref name="state"/>,
    /// after the common reaction: while his hurt flash shows he reacts as to a hit.
    /// </summary>
    private void ResolveSporeSpawnShotAfterCommon(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        // He reacts to whatever strikes his core while he flashes hurt, by this hit or by an earlier one whose flash still
        // shows: a bomb, which does him no damage, still sets him swaying inside a missile's flash.
        bool flashingHurt = body.HurtFlash.IsRunning;
        if (flashingHurt)
        {
            state.Movement = SporeSpawnMovement.Moving;

            // Low on health, a hit doubles his angle step while keeping its direction.
            if (SporeSpawnHealthBands.SwingsFast(body.Health))
            {
                state.SwayStep = state.SwayStep.By(SporeSpawnArenaDefinitions.FastSwayStep);
            }

            // Only the first damaging hit during an open phase reverses direction and
            // closes the head. The open-phase step clears this guard when the animation permits it.
            if (!state.Damaged)
            {
                state.SwayStep = state.SwayStep.Reversed();
                state.Damaged = true;
                StartSporeSpawnAnimation(body, state, SporeSpawnBodySegment.CloseAndMove);
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
        body.Invincibility.End();
        body.HurtFlash.End();
        body.AiStates = EnemyAiStates.None;
        // The dead body stops touching Samus.
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);

        DeleteProjectilesButStalk(state);

        StartSporeSpawnAnimation(body, state, SporeSpawnBodySegment.Death);
        _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaMiniBoss);
        _cameraFloor.Release();
        PublishSporeSpawnObject(SporeSpawnCeilingMutation.Crumble);
    }

    /// <summary>
    /// Deletes every enemy projectile but his stalk's links, as his death does: the stalk survives for the death animation
    /// (#1932).
    /// </summary>
    private void DeleteProjectilesButStalk(SporeSpawnEnemyState state)
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
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.WidenSway }:
                state.SwayRadius = state.SwayRadius.Widened();
                return;
            case SporeSpawnCue.Signal { Kind: SporeSpawnCueKind.ReopenToHits }:
                state.Damaged = false;
                return;
            case SporeSpawnCue.SetSway setSway:
                state.SwayRadius = setSway.Radius;
                state.SwayStep = setSway.Step;
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
    private static void ShowSporeSpawnHealthShade(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnHealthBand band)
    {
        state.Colours.ShowHealth(band);
        body.Shading = state.Colours.Sprites;
    }

    /// <summary>
    /// Shows step <paramref name="step"/> of his death fade over his body, the room's level and its background (#1512),
    /// as the import's shades of it.
    /// </summary>
    private static void ShowSporeSpawnDeathShade(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnDeathFadeStep step)
    {
        state.Colours.ShowDeath(step);
        body.Shading = state.Colours.Sprites;
    }

    /// <summary>Draws one random number to place a hardening dust burst and request its sound.</summary>
    private void SpawnSporeSpawnHardeningDust(RoomEnemyPlace body)
    {
        int random = _room.Random.Generate();
        _projectiles.SpawnDustOrExplosion(
            new(body.Position.X.Whole + SporeSpawnRandomDefinitions.HardeningDustX.Of(random) - SporeSpawnArenaDefinitions.HardeningDustCentring.X, body.Position.Y.Whole + SporeSpawnRandomDefinitions.HardeningDustY.Of(random) - SporeSpawnArenaDefinitions.HardeningDustCentring.Y),
            DustExplosionAnimation.BigDustCloud);
        RequestSporeSpawnSound(SporeSpawnSoundDefinitions.HardeningDust);
    }

    /// <summary>Draws one random number to place a dying explosion sprite and request its sound.</summary>
    private void SpawnSporeSpawnDyingExplosion(RoomEnemyPlace body)
    {
        int random = _room.Random.Generate();
        _ = _effectActors.SpawnRoomEffectActor(
            new(body.Position.X.Whole + SporeSpawnRandomDefinitions.DyingExplosionX.Of(random) - SporeSpawnArenaDefinitions.DyingExplosionCentring.X, body.Position.Y.Whole + SporeSpawnRandomDefinitions.DyingExplosionY.Of(random) - SporeSpawnArenaDefinitions.DyingExplosionCentring.Y),
            RoomEffectActorKind.SmallExplosion,
            graphics: default);
        RequestSporeSpawnSound(SporeSpawnSoundDefinitions.DyingExplosion);
    }

    /// <summary>
    /// Scatters his death's drops over their stretch of his room, as every scattered death drops them (#2335): each drop's
    /// place is drawn from one random number before it spawns.
    /// </summary>
    private void RequestSporeSpawnDeathDrops(SamusState samus) =>
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.SporeSpawn,
            count: SporeSpawnArenaDefinitions.DeathDrops,
            xBase: SporeSpawnArenaDefinitions.DeathDropCorner.X,
            xScatter: SporeSpawnRandomDefinitions.DropX,
            yBase: SporeSpawnArenaDefinitions.DeathDropCorner.Y,
            yScatter: SporeSpawnRandomDefinitions.DropY,
            samus: samus);
    /// <summary>Spawns the stalk link at <paramref name="segment"/> from his body and returns it (#1932).</summary>
    /// <exception cref="InvalidOperationException">No projectile place is free: he spawns his stalk into his room's empty places.</exception>
    private EnemyProjectile SpawnSporeSpawnStalk(RoomEnemyPlace body, Offset16<RoomPixels> segment)
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

    /// <summary>Spawns the ceiling spore emitter at <paramref name="emitter"/> for <paramref name="state"/>'s fight and runs its setup.</summary>
    private void SpawnSporeSpawnSpawner(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnEmitter emitter)
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
        spawner.Position = spawner.Position.WithWhole(emitter.Place);
        spawner.State = new SporeSpawnEmitterState(state, emitter.Drift);
    }

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on Spore Spawn's frame, or null when the frame is not one of his
    /// collision frames; his own entity asks (#2301).
    /// </summary>
    internal static bool? FindSporeSpawnHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!SporeSpawnCollision.IsFrame(enemy.Frame))
            return null;

        // Every rectangle and reaction of these eleven frames comes from Spore Spawn's collision catalog.
        foreach (EnemyCollisionComponent component in
                 SporeSpawnCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        componentOrigin.X + hitbox.Left,
                        componentOrigin.Y + hitbox.Top,
                        componentOrigin.X + hitbox.Right,
                        componentOrigin.Y + hitbox.Bottom))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }
}
