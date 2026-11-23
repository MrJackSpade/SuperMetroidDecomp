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
internal sealed partial class SporeSpawnBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.SporeSpawn];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new SporeSpawnEntity(this, occupant, place, common));

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
        _passEvents = dependencies.PassEvents;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _effectActors = dependencies.EffectActors;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _presentation = dependencies.Presentation;
        _cameraFloor = dependencies.CameraFloor;
        Spore = new SporeSpawnSporeBehavior(dependencies.DeathsAndDrops);
        Spawner = new SporeSpawnSpawnerBehavior(dependencies.Room, dependencies.Projectiles, Spore);
    }

    /// <summary>The room camera's floor, which his live fight holds at his arena's top (#2243).</summary>
    private readonly RoomCameraFloor _cameraFloor;

    /// <summary>The room's presentation, which shows his colours (#2194).</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Queues a sound from the boss or one of its animations as it makes it, after the update's earlier requests.</summary>
    private void RequestSporeSpawnSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Sets Spore Spawn up when the room spawns it; his body holds his encounter from here on (#2231).</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal SporeSpawnEnemyState InitializeSporeSpawn(RoomEnemyOccupant occupant, RoomEnemyPlace body)
    {
        var state = new SporeSpawnEnemyState
        {
            Movement = SporeSpawnMovement.Idle,
            StalkAnchor = body.Position.Whole + SporeSpawnArenaDefinitions.StalkAnchorAboveBody,
            MovementCenter = body.Position.Whole,
        };
        _presentation.BossShadings.Show(BossShadingLayer.SporeSpawn, state.Colours);

        // His spores' colours, which he loads as he initializes, are the import's shade of them (#1512).
        body.Shading = state.Colours.Sprites;

        // He hangs from the ceiling by the stalk's links, the last he spawns highest (#1932).
        state.StalkLinks = [.. SporeSpawnProjectileDefinitions.StalkSegments.Select(segment => SpawnSporeSpawnStalk(occupant, body, segment)).OfType<SporeSpawnStalkLink>().Reverse()];

        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.Husk), AnimationPlace.StartOfAnimation);
            body.Contact = body.Contact.With(EnemyContact.SolidToSamus);
            UpdateSporeSpawnStalks(body, state);
            PublishSporeSpawnObject(SporeSpawnCeilingMutation.Clear);
            return state;
        }

        state.BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.AwaitingDescent), AnimationPlace.StartOfAnimation);
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole - SporeSpawnArenaDefinitions.FightStartRise);

        // His fight runs every enemy in the room each update, wherever the camera is.
        _frameContext.RequestFullSweep();
        _cameraFloor.Hold(SporeSpawnArenaDefinitions.LowestCameraY);
        foreach (SporeSpawnEmitter emitter in SporeSpawnProjectileDefinitions.Emitters)
            SpawnSporeSpawnSpawner(occupant, body, state, emitter);
        UpdateSporeSpawnStalks(body, state);
        return state;
    }

    /// <summary>What <see cref="RunSporeSpawnMain"/> does for each <see cref="SporeSpawnMovement"/> member, each member's own arm.</summary>
    private IdentityTable<SporeSpawnMovement, Action<RoomEnemyPlace, SporeSpawnEnemyState>> RunSporeSpawnMainForSporeSpawnMovement => field ??= new(new Dictionary<SporeSpawnMovement, Action<RoomEnemyPlace, SporeSpawnEnemyState>>
    {
        [SporeSpawnMovement.Idle] = (body, state) => { },
        [SporeSpawnMovement.Descending] = (body, state) => RunSporeSpawnDescent(body, state),
        [SporeSpawnMovement.Moving] = (body, state) => RunSporeSpawnMovement(body, state),
        [SporeSpawnMovement.SetupDeath] = (body, state) => SetupSporeSpawnDeathVelocity(body, state),
        [SporeSpawnMovement.Dying] = (body, state) => RunSporeSpawnDying(body, state),
    });

    /// <summary>Runs Spore Spawn's movement for one update.</summary>
    private void RunSporeSpawnMain(
        RoomEnemyPlace body,
        SporeSpawnEnemyState state)
    {
        RunSporeSpawnMainForSporeSpawnMovement[state.Movement](body, state);
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
        // The vertical sway turns twice as fast as the horizontal, from a quarter turn behind it, narrower by its
        // narrowing. Spore Spawn only moves once the fight has started, which sets the radius first; the only other way
        // into movement is a hit on the exposed core, which only the open frames after that start carry. The radius
        // therefore never lies below the narrowing when it is read.
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
        if (SporeSpawnCadenceDefinitions.IdleDust.IsDue(_frameContext.FrameCounter))
        {
            int random = _room.Random.Generate();
            _ = _effectActors.SpawnRoomEffectActor(
                new(SporeSpawnRandomDefinitions.IdleDustX.Of(random) + SporeSpawnArenaDefinitions.IdleDustLeft, SporeSpawnRandomDefinitions.IdleDustY.Of(random) + SporeSpawnArenaDefinitions.IdleDustTop),
                RoomEffectActorKind.BigDustCloud,
                graphics: SpriteGraphics.SharedStandard);
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
        IReadOnlyList<SporeSpawnStalkLink> links,
        int firstPosition,
        int anchor,
        int end,
        Axis axis)
    {
        // The links lie a quarter, a half and three quarters of the way along the distance from the anchor to the
        // body, the quarter and the half each rounded toward the anchor and the three quarters their sum.
        int delta = end - anchor;
        int quarter = delta / 4;
        int half = delta / 2;
        int threeQuarters = quarter + half;
        foreach ((SporeSpawnStalkPlace place, EnemyProjectile stalk) in links)
        {
            int position = place switch
            {
                SporeSpawnStalkPlace.Ceiling => firstPosition,
                SporeSpawnStalkPlace.Quarter => anchor + quarter,
                SporeSpawnStalkPlace.Half => anchor + half,
                SporeSpawnStalkPlace.ThreeQuarters => anchor + threeQuarters,
                _ => throw new InvalidDataException($"Undefined Spore Spawn stalk place {place}."),
            };
            if (axis == Axis.Horizontal)
                stalk.Position = stalk.Position.WithWholeX(position);
            else
                stalk.Position = stalk.Position.WithWholeY(position);
        }
    }

    /// <summary>Requests the arena ceiling room object at its fixed room block position.</summary>
    private void PublishSporeSpawnObject(SporeSpawnCeilingMutation mutation) =>
        _passEvents.Publish(new SporeSpawnCeilingChange(mutation));

    /// <summary>Routes accepted bomb hits on <paramref name="body"/> through his encounter's post-damage response.</summary>
    /// <param name="body">His body, whose encounter the response acts on.</param>
    internal static EnemyBombReaction SporeSpawnBombReactionOf(SporeSpawnEntity body)
    {
        Ensure.NotNull(body);
        return new()
        {
            RunsGenericDeath = false,
            AfterCommon = body.ResolveShotAfterCommon,
        };
    }

    /// <summary>A spore (#1760, #2410): it drifts from its spawn, and a shot one bursts, harmless, then drops and deletes itself.</summary>
    internal SporeSpawnSporeBehavior Spore { get; }

    /// <summary>A ceiling spore spawner (#1760, #2410): it releases spores on its random cadence, and a shot one restarts closed.</summary>
    internal SporeSpawnSpawnerBehavior Spawner { get; }

    /// <summary>The links of his stalk (#1760): each holds still, and a shot one restarts its pose.</summary>
    internal SporeSpawnStalkBehavior Stalk => field ??= new SporeSpawnStalkBehavior(Spawner);

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
            AfterSuperMissileQuake = ReactToSporeSpawnHitboxShot,
            Lethal = EnemyShotLethal.SpeciesOwned,
            AfterCommon = body.ResolveShotAfterCommon,
        };
    }

    /// <summary>
    /// Meets a shot with his shell's dud or his core's own reaction, after the Super Missile quake: a dud flashes, and his
    /// core rejects a weapon it does not accept before the common damage.
    /// </summary>
    /// <param name="c">The shot meeting his body.</param>
    /// <returns>Whether his own reaction took the shot.</returns>
    private bool ReactToSporeSpawnHitboxShot(in EnemyShotContext c)
    {
        // His frames' hitboxes meet a shot with his shell's dud or his core's own reaction.
        if (c.HitboxShot == EnemyReaction.DudShot)
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            _projectiles.CreateEnemyProjectileDudShot(c.Shot);
            return true;
        }
        if (!SporeSpawnAcceptsProjectile(c.ProjectileType))
        {
            // The shot has struck the vulnerable core, whose shot reaction rejects it before common damage.
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            return true;
        }
        return false;
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
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
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
    internal void ResolveSporeSpawnShotAfterCommon(RoomEnemyPlace body, SporeSpawnEnemyState state)
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
    internal void DeleteProjectilesButStalk(SporeSpawnEnemyState state)
    {
        foreach (EnemyProjectile projectile in _projectiles.EnemyProjectiles)
        {
            if (!state.StalkLinks.Any(link => link.Projectile == projectile))
                projectile.Delete();
        }
    }

    /// <summary>What <see cref="ApplySporeSpawnCue"/> does for each <see cref="SporeSpawnCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<SporeSpawnCueKind, Action<RoomEnemyPlace, SporeSpawnEnemyState, SamusState>> ApplySporeSpawnCueForSporeSpawnCueKind => field ??= new(new Dictionary<SporeSpawnCueKind, Action<RoomEnemyPlace, SporeSpawnEnemyState, SamusState>>
    {
        [SporeSpawnCueKind.WidenSway] = (body, state, samus) => state.SwayRadius = state.SwayRadius.Widened(),
        [SporeSpawnCueKind.ReopenToHits] = (body, state, samus) => state.Damaged = false,
        [SporeSpawnCueKind.Harden] = (body, state, samus) =>
        {
            body.Position = body.Position.WithWhole(new(SporeSpawnArenaDefinitions.CentreX, SporeSpawnArenaDefinitions.CentreY));
            body.Contact = body.Contact.Replace(EnemyContact.IgnoresSamus, EnemyContact.SolidToSamus);
            body.Processing = body.Processing.With(EnemyProcessing.Animates);
        },
        [SporeSpawnCueKind.RequestDeathDrops] = (body, state, samus) => RequestSporeSpawnDeathDrops(samus),
        [SporeSpawnCueKind.SpawnHardeningDust] = (body, state, samus) => SpawnSporeSpawnHardeningDust(body),
        [SporeSpawnCueKind.SpawnDyingExplosion] = (body, state, samus) => SpawnSporeSpawnDyingExplosion(body),
    });

    /// <summary>Carries out one cue of Spore Spawn's body animation.</summary>
    internal void ApplySporeSpawnCue(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnCue cue, SamusState samus)
    {
        cue.React(ApplySporeSpawnCueReactions, new ApplySporeSpawnCueContext(body, state, samus, this));
    }

    /// <summary>What <see cref="ApplySporeSpawnCue"/> does on each <see cref="SporeSpawnCue"/>, built once (#2553).</summary>
    private static SporeSpawnCueReactions<ApplySporeSpawnCueContext> ApplySporeSpawnCueReactions { get; } = new(
        ShowDeathShade: static (showDeathShade, context) => ShowSporeSpawnDeathShade(context.Body, context.State, showDeathShade.DeathShadeStep),
        QueueSound: static (queueSound, context) => context.Owner.RequestSporeSpawnSound(queueSound.Sound),
        SetMovement: static (setMovement, context) => context.State.Movement = setMovement.Movement,
        SetSway: static (setSway, context) =>
        {
            context.State.SwayRadius = setSway.Radius;
            context.State.SwayStep = setSway.Step;
        },
        SetSporeGeneration: static (setSporeGeneration, context) => context.State.SporeGenerationSuppressed = setSporeGeneration.SporeGenerationSuppressed,
        Signal: static (signal, context) =>
        {
            context.Owner.ApplySporeSpawnCueForSporeSpawnCueKind[signal.Kind](context.Body, context.State, context.Samus);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Body">The body the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Samus">The samus the cases read.</param>
    /// <param name="Owner">The <see cref="SporeSpawnBehavior"/> reacting.</param>
    private readonly partial record struct ApplySporeSpawnCueContext(RoomEnemyPlace Body, SporeSpawnEnemyState State, SamusState Samus, SporeSpawnBehavior Owner);

    /// <summary>Shows <paramref name="band"/>'s colours of <paramref name="state"/> on his <paramref name="body"/> and his stalk.</summary>
    internal static void ShowSporeSpawnHealthShade(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnHealthBand band)
    {
        state.Colours.ShowHealth(band);
        ShowSporeSpawnColours(body, state);
    }

    /// <summary>
    /// Shows his colours on his <paramref name="body"/> and the links of his stalk (#2628): the game draws his stalk in his
    /// body's colours, so his health bands and death fade recolour it with his body.
    /// </summary>
    private static void ShowSporeSpawnColours(RoomEnemyPlace body, SporeSpawnEnemyState state)
    {
        body.Shading = state.Colours.Sprites;
        foreach (SporeSpawnStalkLink link in state.StalkLinks)
            link.Projectile.Graphics = link.Projectile.Graphics.In(state.Colours.Sprites);
    }

    /// <summary>
    /// Shows step <paramref name="step"/> of his death fade over his body, the room's level and its background (#1512),
    /// as the import's shades of it.
    /// </summary>
    internal static void ShowSporeSpawnDeathShade(RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnDeathFadeStep step)
    {
        state.Colours.ShowDeath(step);
        ShowSporeSpawnColours(body, state);
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
            graphics: SpriteGraphics.SharedStandard);
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
    /// <summary>Spawns the stalk link <paramref name="link"/> from his body and returns it, or none when the pool is full (#1932).</summary>
    internal SporeSpawnStalkLink? SpawnSporeSpawnStalk(RoomEnemyOccupant occupant, RoomEnemyPlace body, SporeSpawnStalkLinkDefinition link)
    {
        if (_projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.SporeSpawnStalk, Stalk,
                SpriteGraphics.SpawnedBy(occupant, body.Shading), Stalk.Pose()) is not { } stalk)
            return null;

        stalk.Position = stalk.Position.WithWhole(body.Position.Whole + link.SpawnOffset);
        return new(link.Place, stalk);
    }

    /// <summary>Spawns the ceiling spore emitter at <paramref name="emitter"/> for <paramref name="state"/>'s fight and runs its setup.</summary>
    internal void SpawnSporeSpawnSpawner(RoomEnemyOccupant occupant, RoomEnemyPlace body, SporeSpawnEnemyState state, SporeSpawnEmitter emitter)
    {
        EnemyProjectile? spawner = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.SporeSpawnSpawner, Spawner, SpriteGraphics.SpawnedBy(occupant, body.Shading), new SporeSpawnCeilingAnimation(Spawner, SporeSpawnCeilingSegment.SpawnerClosed));
        if (spawner is null)
            return;

        spawner.Position = spawner.Position.WithWhole(emitter.Place);
        Spawner.Emitters.Start(spawner, new SporeSpawnEmitterState(state, emitter.Drift));
    }

    /// <summary>
    /// The first hitbox the probe overlaps on Spore Spawn's frame, whose reaction runs, or none; his own entity asks
    /// (#2301).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not one of his collision frames.</exception>
    internal static EnemyHitboxContact FindSporeSpawnHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        if (!SporeSpawnCollision.IsFrame(enemy.Frame))
            throw EnemyCollision.UncoveredFrame(enemy);

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
                return EnemyHitboxContact.Struck(probe.Contact == HitboxContactKind.Shot ? hitbox.Shot : hitbox.Touch);
            }
        }
        return EnemyHitboxContact.Missed;
    }
}
