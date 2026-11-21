using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's setup, its body and tongue animations and their cues, and the invisible wall that holds Samus back.</summary>
[RoomLifetime]
internal sealed class CrocomireFight
{
    /// <summary>Accesses the active room, enemy places, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves Crocomire against the arena's collision blocks.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the bridge and wall-break quake effects.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Resolves Crocomire body and tongue contact with Samus.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    private readonly RoomCameraDistance _cameraDistance;
    /// <summary>Updates boss completion and room-state flags.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>Crocomire's combat, whose projectile behaviour the mouth's shot is bound to.</summary>
    private readonly CrocomireCombat _combat;
    /// <summary>The room's enemy places, among which the tongue finds the body listed before it.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The encounters of the room's bosses, to which his body's setup attaches his (#2194).</summary>
    private readonly RoomBossEncounters _encounters;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireFight(CrocomireFightDependencies dependencies)
    {
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _presentation = dependencies.Presentation;
        _collision = dependencies.Collision;
        _cameraDistance = dependencies.CameraDistance;
        _roomStatus = dependencies.RoomStatus;
        _access = dependencies.Access;
        _combat = dependencies.Combat;
        _places = dependencies.Places;
        _encounters = dependencies.Encounters;
    }

    /// <summary>Sets Crocomire up when the room spawns it.</summary>
    internal void InitializeCrocomire(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        _roomStatus.Boss = AreaBoss.Crocomire;
        var state = new CrocomireEnemyState(occupant, place);
        place.SetSpeciesState(state);
        // Once attached, the room's shaking and his melt's display read his encounter from the room (#2194); his parts
        // each hold the encounter from their own setup.
        _encounters.Attach(state);
        // Crocomire shows his body frames and his death's melt images on his body (#1355, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Crocomire, state));
        _presentation.BossShadings.Show(BossShadingLayer.Crocomire, state.Colours);

        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            // In a room whose Crocomire is already dead only his skeleton is shown: it ignores Samus, and stands at a
            // fixed place with a fixed size.
            place.Contact = place.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
            state.RestAsCorpse();
            CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.Dead);
            place.Position = place.Position.WithWhole(CrocomireArenaDefinitions.CorpsePosition);
            place.Radius = CrocomireArenaDefinitions.CorpseRadius;
            foreach (Position16<RoomScreens> screen in CrocomireCameraDefinitions.ArenaScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.OpenAlignedToTop);
            _access.PublishCrocomireObject(CrocomireArenaDefinitions.FightWallBlock, CrocomireArenaMutation.ClearInvisibleWall);
            _access.PublishCrocomireObject(CrocomireArenaDefinitions.SpikeWallBlock, CrocomireArenaMutation.ClearInvisibleWall);
            _access.PublishCrocomireObject(CrocomireArenaDefinitions.BridgeFirstBlock, CrocomireArenaMutation.ClearBridge);
            return;
        }

        // On the living branch only.
        _cameraDistance.Request(CameraDistanceMode.BossTracking);
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.IdleSway);
        place.CollidesByFrameHitboxes = true;
        foreach (Position16<RoomScreens> screen in CrocomireCameraDefinitions.BehindWallScreens)
            _room.Services.RoomScroll.Write(screen, RoomScrollState.Closed);

        // His wall, the bridge's fragments and his projectiles draw in their own shades (#1512).
    }

    /// <summary>Sets Crocomire's tongue up when the room spawns it.</summary>
    internal void InitializeCrocomireTongue(RoomEnemyPlace place)
    {
        // The tongue joins the encounter its body has set up, in a defeated room too, where it is deleted at once (#2400).
        CrocomireEnemyState state = _places.Member(place, CrocomirePartListingDefinitions.Listing, EnemyDefinitionId.CrocomireTongue, EnemyDefinitionId.Crocomire)
            .RequireSpeciesState<CrocomireEnemyState>();
        state.JoinTongue(place);
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            place.Processing = place.Processing.Without(EnemyProcessing.Animates);
            place.Presence = place.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }

        CrocomireBodyLayer.StartCrocomireTongueAnimation(state, CrocomireTongueSegment.Fight);
        place.CollidesByFrameHitboxes = true;
        var tongueState = new CrocomireTongueState(state);
        place.SetSpeciesState(tongueState);
        place.Shading = state.Body.Shading;
    }

    /// <summary>Runs the artificial left-side wall.</summary>
    internal void HandleCrocomireInvisibleWall(
        RoomEnemyOccupant crocomire,
        CrocomireEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace body = crocomire.Place;
        if (state.DeathStage.Entry is not CrocomireDeathStage.Fighting)
            return;
        int leftEdge = body.Position.X.Whole - body.Radius.X - samus.Kinematics.Radius.X;
        if (leftEdge >= samus.Position.X.Whole)
            return;

        _collision.ResolveOrdinaryTouch(crocomire, samus, pseudoScrew);
        samus.Position = samus.Position.WithWholeX(leftEdge);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(CrocomireFightDefinitions.WallPush);
    }

    /// <summary>
    /// Runs the tongue's animation where the shared enemy animation runs: the frame changes when its timer
    /// runs out, and the clock notes each newly shown frame.
    /// </summary>
    internal static void AdvanceCrocomireTongueAnimation(RoomEnemyPlace tongue)
    {
        CrocomireEnemyState state = tongue.RequireSpeciesState<CrocomireTongueState>().Crocomire;
        if (!tongue.AnimationClock.Expire())
            return;

        AnimationPlace place = state.TonguePlayback.Place;
        AnimationShown<CrocomireTongueSegment, EnemyFrameId> outcome = CrocomireAnimations.TongueSegments.PlayToNextFrame(
            state.TonguePlayback.Animation, ref place);
        state.TonguePlayback = state.TonguePlayback with { Place = place };
        // A hold rests the animation: the last frame stays and the clock stops (#2454). Every outcome takes the shared
        // enemy animation's meaning.
        EnemyAnimationPlayback.Show(tongue, outcome, static frame => frame);
    }

    /// <summary>
    /// Runs Crocomire's body animation where the shared enemy animation runs: the frame expires on the
    /// update its timer would, and the clock notes each newly shown frame as it does an
    /// authored timed frame.
    /// </summary>
    internal void AdvanceCrocomireBodyAnimation(RoomEnemyPlace body, SamusState samus, RoomLevelData level, int cameraX)
    {
        CrocomireEnemyState state = body.RequireSpeciesState<CrocomireEnemyState>();
        if (!body.AnimationClock.Expire())
            return;

        Position16<RoomPixels> placedAt = body.Position.Whole;
        Animation<CrocomireBodySegment> animation = state.BodyPlayback.Animation;
        AnimationPlace place = state.BodyPlayback.Place;
        AnimationShown<CrocomireBodySegment, EnemyFrameId> outcome = CrocomireBodyAnimations.Segments.PlayToNextFrame(
            ref animation, ref place,
            cue => ApplyCrocomireBodyCue(state, samus, level, cameraX, cue) is { } started
                ? new AnimationContinuation<CrocomireBodySegment>(CrocomireBodyAnimations.StartingWith(started), AnimationPlace.StartOfAnimation)
                : null);
        state.BodyPlayback = new AnimationPlayback<CrocomireBodySegment>(animation, place);
        // His steps move him after his update has placed his tongue, and his body's picture shows where they leave him:
        // the tongue's picture follows them so that it stays against the body's. The tongue itself, which Samus touches,
        // stays where it was placed until its next placement.
        state.Tongue.DrawOffset += body.Position.Whole - placedAt;
        // A hold rests the animation: the last frame stays and the clock stops (#2454). A held frame (#1627) shows its
        // frame for good, its clock stopped as a hold's is. Every outcome takes the shared enemy animation's meaning.
        EnemyAnimationPlayback.Show(body, outcome, static (shownOn, frame, crocomire) => ShowCrocomireBodyFrame(crocomire, shownOn, frame), state);
    }

    /// <summary>Shows <paramref name="frame"/> on Crocomire's body.</summary>
    private static void ShowCrocomireBodyFrame(CrocomireEnemyState state, RoomEnemyPlace body, EnemyFrameId frame)
    {
        state.BodyFrame = frame;
        body.Frame = frame;
    }

    /// <summary>
    /// Carries out one body-animation cue. Movement, wall, sound, shake and dust cues keep their authored order;
    /// the fight AI and the spike-wall move may start another animation, which plays on at once.
    /// </summary>
    /// <param name="state">The Crocomire encounter whose body runs the animation.</param>
    /// <param name="samus">Samus, whom the fight AI measures against.</param>
    /// <param name="level">The room's collision level, which the movement cues require.</param>
    /// <param name="cameraX">The camera's X, which the on-screen movement tests use.</param>
    /// <param name="cue">The cue to perform.</param>
    /// <returns>The animation that starts, or null to continue after the cue.</returns>
    private CrocomireBodyAnimation? ApplyCrocomireBodyCue(
        CrocomireEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int cameraX,
        CrocomireBodyCue cue)
    {
        RoomEnemyPlace body = state.Body;
        return cue.Match(ApplyCrocomireBodyCueCases, new ApplyCrocomireBodyCueContext(state, body, cameraX, level, samus, this));
    }

    /// <summary>What <see cref="ApplyCrocomireBodyCue"/> does on each <see cref="CrocomireBodyCue"/>, built once (#2553).</summary>
    private static CrocomireBodyCueCases<ApplyCrocomireBodyCueContext, CrocomireBodyAnimation?> ApplyCrocomireBodyCueCases { get; } = new(
        Step: static (step, context) =>
        {
            if (step.Move.RaisesFootDust)
                context.Owner.SpawnCrocomireRandomFootDust(context.State);
            if (StepMoves(step.Move.Condition, context.State, context.Body, context.CameraX))
                context.Owner.MoveCrocomire(context.Body, context.Level, step.Move.HorizontalPixels);
            return null;
        },
        Dust: static (dust, context) =>
        {
            context.Owner._access.SpawnCrocomireDust(context.State, dust.Offset);
            return null;
        },
        Signal: static (signal, context) => context.Owner.ApplyCrocomireBodySignal(context.State, context.Samus, context.Level, signal.Kind));

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="State">The Crocomire encounter whose body runs the animation.</param>
    /// <param name="Body">The <c>body</c> <see cref="ApplyCrocomireBodyCue"/> works with.</param>
    /// <param name="CameraX">The camera's X, which the on-screen movement tests use.</param>
    /// <param name="Level">The room's collision level, which the movement cues require.</param>
    /// <param name="Samus">Samus, whom the fight AI measures against.</param>
    /// <param name="Owner">The <see cref="CrocomireFight"/> reacting.</param>
    private readonly record struct ApplyCrocomireBodyCueContext(CrocomireEnemyState State, RoomEnemyPlace Body, int CameraX, RoomLevelData Level, SamusState Samus, CrocomireFight Owner);

    /// <summary>Whether a step under <paramref name="condition"/> moves the body now.</summary>
    private static bool StepMoves(CrocomireStepCondition condition, CrocomireEnemyState state, RoomEnemyPlace body, int cameraX) => condition switch
    {
        CrocomireStepCondition.Always => true,
        CrocomireStepCondition.UnlessHitPending => !state.StepBack.IsAwaited,
        CrocomireStepCondition.WhileOnScreen => CrocomireBodyIsOnScreen(body, cameraX),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireStepCondition)} {condition}."),
    };

    /// <summary>What <see cref="ApplyCrocomireBodySignal"/> comes to for each <see cref="CrocomireBodyCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<CrocomireBodyCueKind, Func<CrocomireEnemyState, SamusState, RoomLevelData, RoomEnemyPlace, CrocomireBodyAnimation?>> ApplyCrocomireBodySignalForCrocomireBodyCueKind => field ??= new(new Dictionary<CrocomireBodyCueKind, Func<CrocomireEnemyState, SamusState, RoomLevelData, RoomEnemyPlace, CrocomireBodyAnimation?>>
    {
        [CrocomireBodyCueKind.FightAi] = (state, samus, level, body) => RunCrocomireFightCue(state, samus),
        [CrocomireBodyCueKind.MaybeStartProjectileAttack] = (state, samus, level, body) =>
        {
            if (RollsProjectileAttack(_access.ReadCrocomireRandom()))
            {
                state.Phase = CrocomirePhase.ProjectileAttack;
                state.ShotsSpat = 0;
                return CrocomireBodyAnimation.ProjectileAttack;
            }
            return null;
        },
        [CrocomireBodyCueKind.Cry] = (state, samus, level, body) =>
        {
            _access.RequestCrocomireSound(CrocomireSoundDefinitions.Cry);
            return null;
        },
        [CrocomireBodyCueKind.BigExplosionSound] = (state, samus, level, body) =>
        {
            _access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
            return null;
        },
        [CrocomireBodyCueKind.SkeletonCollapseSound] = (state, samus, level, body) =>
        {
            _access.RequestCrocomireSound(CrocomireSoundDefinitions.SkeletonCollapse);
            return null;
        },
        [CrocomireBodyCueKind.ShakeScreen] = (state, samus, level, body) =>
        {
            _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.RoomBlocks), CrocomireFightDefinitions.StepShakeDuration);
            _access.RequestCrocomireSound(CrocomireSoundDefinitions.ScreenShake);
            return null;
        },
        [CrocomireBodyCueKind.StepTowardSpikeWall] = (state, samus, level, body) =>
        {
            if (MoveCrocomire(body, level, CrocomireStepDefinitions.Advance.HorizontalPixels))
            {
                state.Phase = CrocomirePhase.BackingOffSpikeWall;
                return CrocomireBodyAnimation.BackOffFromSpikeWall;
            }
            int random = _access.ReadCrocomireRandom();
            int baseOffset = CrocomireFightDefinitions.SpikeWallDustSide.Toward(
                BossRandomRolls.InHalfFrom(random, CrocomireRandomDefinitions.SpikeWallDustLeftFrom) ? HorizontalDirection.Left : HorizontalDirection.Right);
            _access.SpawnCrocomireDust(state, baseOffset + CrocomireRandomDefinitions.SpikeWallDustSpread.Of(random));
            return null;
        },
        [CrocomireBodyCueKind.SkeletonCollapsed] = (state, samus, level, body) =>
        {
            state.MarkSkeletonCollapsed();
            return null;
        },
    });

    /// <summary>Carries out one signalling body-animation cue; the fight AI and the spike-wall step may start another animation.</summary>
    private CrocomireBodyAnimation? ApplyCrocomireBodySignal(
        CrocomireEnemyState state,
        SamusState samus,
        RoomLevelData level,
        CrocomireBodyCueKind kind)
    {
        RoomEnemyPlace body = state.Body;
        return ApplyCrocomireBodySignalForCrocomireBodyCueKind[kind](state, samus, level, body);
    }

    /// <summary>Whether Crocomire's one-in-four spit roll starts a spit attack where his walk offers one.</summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    private static bool RollsProjectileAttack(int sample) => CrocomireRandomDefinitions.ProjectileAttack.IsTakenBy(sample);

    /// <summary>Whether the body's left edge lies within the step back's reach right of the camera: the step-back moves' on-screen test.</summary>
    private static bool CrocomireBodyIsOnScreen(RoomEnemyPlace body, int cameraX) =>
        body.Position.X.Whole - body.Radius.X - CrocomireFightDefinitions.StepBackScreenReach < cameraX;

    /// <summary>What <see cref="RunCrocomireFightCue"/> comes to for each <see cref="CrocomirePhase"/> member, each member's own arm.</summary>
    private IdentityTable<CrocomirePhase, Func<CrocomireEnemyState, SamusState, CrocomireBodyAnimation?>> RunCrocomireFightCueForCrocomirePhase => field ??= new(new Dictionary<CrocomirePhase, Func<CrocomireEnemyState, SamusState, CrocomireBodyAnimation?>>
    {
        [CrocomirePhase.Dormant] = (state, samus) =>
        {
            if (Math.Abs(state.Body.Position.X.Whole - samus.Position.X.Whole) < CrocomireFightDefinitions.WakeDistance)
            {
                state.Phase = CrocomirePhase.WaitingForFirstDamage;
                return CrocomireBodyAnimation.AwaitingHit;
            }
            return null;
        },
        [CrocomirePhase.SteppingForward] = (state, samus) =>
        {
            if (state.StepBack.Take())
            {
                if (state.StepCounter != 0)
                {
                    state.Phase = CrocomirePhase.SteppingBack;
                    return CrocomireBodyAnimation.StepBack;
                }
            }
            if (state.Body.Position.X.Whole < CrocomireFightDefinitions.SpikeWallChargeX)
            {
                state.Phase = CrocomirePhase.NearSpikeWallCharge;
                return CrocomireBodyAnimation.NearSpikeWallCharge;
            }
            // The phase starts with his step forward, which loops on its own until another phase takes over.
            return null;
        },
        [CrocomirePhase.ProjectileAttack] = (state, samus) =>
        {
            if (StepBackOnHit(state, CrocomirePhase.SteppingBack) is { } stepBack)
                return stepBack;
            if (CrocomireVolley.TryNextGradient(state.ShotsSpat, _access.LastShotGradient, out int gradient))
            {
                state.ShotsSpat++;
                SpawnCrocomireProjectile(state.Body, gradient);
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.ProjectileSpit);
                return null;
            }
            state.Phase = CrocomirePhase.SteppingForward;
            return CrocomireBodyAnimation.StepForwardAfterDelay;
        },
        [CrocomirePhase.NearSpikeWallCharge] = (state, samus) => StepBackOnHit(state, CrocomirePhase.SteppingBack),
        [CrocomirePhase.SteppingBack] = (state, samus) =>
        {
            if (state.StepCounter != 0)
                state.StepCounter--;
            if (state.StepCounter != 0)
                return CrocomireBodyAnimation.SteppingBack;
            state.Phase = CrocomirePhase.SteppingForward;
            return CrocomireBodyAnimation.StepForward;
        },
        [CrocomirePhase.BackingOffSpikeWall] = (state, samus) =>
        {
            if (state.Body.Position.X.Whole >= CrocomireFightDefinitions.SpikeWallChargeX)
            {
                state.Phase = CrocomirePhase.SteppingForward;
                return CrocomireBodyAnimation.StepForward;
            }
            return null;
        },
        [CrocomirePhase.WaitingForFirstDamage] = (state, samus) => StepBackOnHit(state, CrocomirePhase.WaitingForSecondDamage),
        [CrocomirePhase.WaitingForSecondDamage] = (state, samus) => StepBackOnHit(state, CrocomirePhase.SteppingBack),
        [CrocomirePhase.PowerBombCharge] = (state, samus) =>
        {
            // The charge consumes one step and ends once fewer than two remain, as it does when none remained.
            int remaining = state.StepCounter - 1;
            if (remaining < 2)
            {
                state.StepCounter = 0;
                state.Phase = CrocomirePhase.SteppingForward;
                return CrocomireBodyAnimation.StepForward;
            }
            state.StepCounter = remaining;
            return null;
        },
    });

    /// <summary>
    /// Runs Crocomire's fight AI for the phase his fight is in. Each phase is entered as the animation it plays starts, so
    /// the phase alone says what his body is doing (#2180).
    /// </summary>
    private CrocomireBodyAnimation? RunCrocomireFightCue(
        CrocomireEnemyState state,
        SamusState samus)
    {
        return RunCrocomireFightCueForCrocomirePhase[state.Phase](state, samus);
    }

    /// <summary>
    /// Steps him back on a pending hit (#2557): a hit spends the pending step back and steps him back into
    /// <paramref name="damagedDestination"/>; otherwise his phase plays on. The damage waits, his spit volley and his charge
    /// near the spike wall all react to a hit so.
    /// </summary>
    private static CrocomireBodyAnimation? StepBackOnHit(
        CrocomireEnemyState state,
        CrocomirePhase damagedDestination)
    {
        if (!state.StepBack.Take())
            return null;
        state.Phase = damagedDestination;
        return CrocomireBodyAnimation.StepBack;
    }

    /// <summary>
    /// Moves Crocomire's body <paramref name="pixels"/> whole pixels sideways as an enemy that ignores non-square slopes moves,
    /// stopping flush against a wall.
    /// </summary>
    private bool MoveCrocomire(RoomEnemyPlace body, RoomLevelData level, int pixels) =>
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, body, SignedSixteenSixteen.FromWholePixels(pixels));

    /// <summary>Chooses a foot-dust offset from room RNG and spawns the matching effect.</summary>
    private void SpawnCrocomireRandomFootDust(CrocomireEnemyState state)
    {
        int random = _access.ReadCrocomireRandom();
        int offset = CrocomireRandomDefinitions.FootDustOffset.Of(random);
        if (BossRandomRolls.InHalfFrom(random, CrocomireRandomDefinitions.FootDustMirroredFrom))
            offset = -offset;
        _access.SpawnCrocomireDust(state, offset);
    }

    /// <summary>Spawns one of Crocomire's fight projectiles, fired along <paramref name="gradient"/>.</summary>
    private void SpawnCrocomireProjectile(RoomEnemyPlace body, int gradient)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.CrocomireProjectile, _combat.MouthShot, graphics: SpriteGraphics.SharedIn(CrocomireColours.ProjectileShading), new CrocomireProjectileAnimation(_combat.MouthShot, CrocomireProjectileSegment.Mouth));
        if (projectile is null)
            return;

        projectile.Velocity = CrocomireFightDefinitions.SpitVelocity;
        projectile.Position = projectile.Position.WithWhole(body.Position.Whole + CrocomireFightDefinitions.SpitOffset);
        projectile.Position = projectile.Position.WithoutFractions();
        _combat.MouthShot.Gradients.Start(projectile, new CrocomireProjectileState(gradient));
    }
}
