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
    internal CrocomireEnemyState InitializeCrocomire(RoomEnemyPlace place)
    {
        _roomStatus.Boss = AreaBoss.Crocomire;
        var state = new CrocomireEnemyState(place, _places.ListedAfter(place, 1));
        // The room's shaking main and his melt's display read him from the room from here (#2194); his parts each hold
        // the encounter from their own setup.
        _encounters.Attach(state);
        // Crocomire shows his body frames and his death's melt images on his body (#1355, #1466, #1515), nothing at first.
        _presentation.AttachBossBody(state.BodyParts);
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
            return state;
        }

        // On the living branch only.
        _cameraDistance.Request(CameraDistanceMode.BossTracking);
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodyAnimation.IdleSway);
        place.CollidesByFrameHitboxes = true;
        foreach (Position16<RoomScreens> screen in CrocomireCameraDefinitions.BehindWallScreens)
            _room.Services.RoomScroll.Write(screen, RoomScrollState.Closed);

        // The wall's and the projectiles' colours his setup loads are the import's shades of them, which his wall,
        // the bridge's fragments and his projectiles draw in (#1512).
        return state;
    }

    /// <summary>Whether Crocomire is already defeated, so his room's tongue is deleted before any setup.</summary>
    internal bool TongueIsDeletedAtOnce => _room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss);

    /// <summary>Deletes a defeated room's tongue as it is placed, before any setup: a defeated Crocomire's tongue never sets up (#2400, #2554).</summary>
    /// <param name="place">The tongue's place.</param>
    internal static void DeleteDefeatedTongue(RoomEnemyPlace place)
    {
        place.Processing = place.Processing.Without(EnemyProcessing.Animates);
        place.Presence = place.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
    }

    /// <summary>Sets a living room's Crocomire tongue up when the room spawns it.</summary>
    internal CrocomireTongueState InitializeCrocomireTongue(RoomEnemyPlace place)
    {
        // The room lists the tongue right after its body, which has set the encounter up and holds the tongue's values.
        CrocomireEnemyState state = EnemyPartners.Listed<CrocomireBodyEntity>(_places.ListedBefore(place, 1)).State;
        CrocomireTongueState tongueState = state.TongueState;
        tongueState.BodyXOffset = CrocomireFightDefinitions.TongueBodyXOffset;

        CrocomireBodyLayer.StartCrocomireTongueAnimation(state, CrocomireTongueSegment.Fight);
        place.CollidesByFrameHitboxes = true;
        place.Shading = state.Body.Shading;
        return tongueState;
    }

    /// <summary>Runs the artificial left-side wall.</summary>
    internal void HandleCrocomireInvisibleWall(
        RoomEnemyPlace body,
        CrocomireEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        if (state.DeathStage.Entry is not CrocomireDeathStage.Fighting)
            return;
        int leftEdge = body.Position.X.Whole - body.Radius.X - samus.Kinematics.Radius.X;
        if (leftEdge >= samus.Position.X.Whole)
            return;

        _collision.ResolveOrdinaryTouch(body, samus, pseudoScrew);
        samus.Position = samus.Position.WithWholeX(leftEdge);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(CrocomireFightDefinitions.WallPush);
    }

    /// <summary>
    /// Runs the tongue's animation where the shared enemy animation runs: the frame changes when its timer
    /// runs out, and the clock notes each newly shown frame.
    /// </summary>
    internal static void AdvanceCrocomireTongueAnimation(RoomEnemyPlace tongue, CrocomireTongueState crocomireTongueState)
    {
        CrocomireEnemyState state = crocomireTongueState.Crocomire;
        if (!tongue.AnimationClock.Expire())
            return;

        AnimationPlace place = state.TonguePlayback.Place;
        AnimationShown<CrocomireTongueSegment, EnemyFrameId> outcome = CrocomireAnimations.TongueSegments.PlayToNextFrame(
            state.TonguePlayback.Animation, ref place,
            cue => throw new InvalidOperationException($"A Crocomire tongue animation raised event {cue}."));
        state.TonguePlayback = state.TonguePlayback with { Place = place };
        // A hold rests the animation: the last frame stays and the clock stops (#2454).
        if (outcome is AnimationShown<CrocomireTongueSegment, EnemyFrameId>.Held)
        {
            tongue.AnimationClock.Stop();
            return;
        }
        if (outcome is not AnimationShown<CrocomireTongueSegment, EnemyFrameId>.Frame shown)
            throw new InvalidOperationException($"Crocomire's tongue animation ended an update with {outcome}.");
        tongue.Frame = shown.Shown;
        tongue.AnimationClock.Show(shown.Duration);
    }

    /// <summary>
    /// Runs Crocomire's body animation where the shared enemy animation runs: the frame expires on the
    /// update its timer would, and the clock notes each newly shown frame as it does an
    /// authored timed frame.
    /// </summary>
    internal void AdvanceCrocomireBodyAnimation(RoomEnemyPlace body, CrocomireEnemyState state, SamusState samus, RoomLevelData level, int cameraX)
    {
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
        // frame for good, its clock stopped as a hold's is. The body animations have no pauses or ends.
        switch (outcome)
        {
            case AnimationShown<CrocomireBodySegment, EnemyFrameId>.Frame shown:
                ShowCrocomireBodyFrame(state, body, shown.Shown);
                body.AnimationClock.Show(shown.Duration);
                return;
            case AnimationShown<CrocomireBodySegment, EnemyFrameId>.HeldFrame held:
                ShowCrocomireBodyFrame(state, body, held.Shown);
                body.AnimationClock.ShowForGood();
                return;
            case AnimationShown<CrocomireBodySegment, EnemyFrameId>.Held:
                body.AnimationClock.Stop();
                return;
            default:
                throw new InvalidOperationException($"Crocomire's body animation ended an update with {outcome}.");
        }
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
        switch (cue)
        {
            case CrocomireBodyCue.Step step:
                if (step.Move.RaisesFootDust)
                    SpawnCrocomireRandomFootDust(state);
                if (StepMoves(step.Move.Condition, state, body, cameraX))
                    MoveCrocomire(body, level, step.Move.HorizontalPixels);
                return null;
            case CrocomireBodyCue.Dust dust:
                _access.SpawnCrocomireDust(state, dust.Offset);
                return null;
            case CrocomireBodyCue.Signal signal:
                return ApplyCrocomireBodySignal(state, samus, level, signal.Kind);
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyCue)} {cue}.");
        }
    }

    /// <summary>Whether a step under <paramref name="condition"/> moves the body now.</summary>
    private static bool StepMoves(CrocomireStepCondition condition, CrocomireEnemyState state, RoomEnemyPlace body, int cameraX) => condition switch
    {
        CrocomireStepCondition.Always => true,
        CrocomireStepCondition.UnlessHitPending => !state.StepBack.IsAwaited,
        CrocomireStepCondition.WhileOnScreen => CrocomireBodyIsOnScreen(body, cameraX),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireStepCondition)} {condition}."),
    };

    /// <summary>Carries out one signalling body-animation cue; the fight AI and the spike-wall step may start another animation.</summary>
    private CrocomireBodyAnimation? ApplyCrocomireBodySignal(
        CrocomireEnemyState state,
        SamusState samus,
        RoomLevelData level,
        CrocomireBodyCueKind kind)
    {
        RoomEnemyPlace body = state.Body;
        switch (kind)
        {
            case CrocomireBodyCueKind.FightAi:
                return RunCrocomireFightCue(state, samus);

            case CrocomireBodyCueKind.MaybeStartProjectileAttack:
                if (RollsProjectileAttack(_access.ReadCrocomireRandom()))
                {
                    state.Phase = CrocomirePhase.ProjectileAttack;
                    state.ShotsSpat = 0;
                    return CrocomireBodyAnimation.ProjectileAttack;
                }
                return null;

            case CrocomireBodyCueKind.Cry:
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.Cry);
                return null;
            case CrocomireBodyCueKind.BigExplosionSound:
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
                return null;
            case CrocomireBodyCueKind.SkeletonCollapseSound:
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.SkeletonCollapse);
                return null;
            case CrocomireBodyCueKind.ShakeScreen:
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), CrocomireFightDefinitions.StepShakeDuration);
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.ScreenShake);
                return null;

            case CrocomireBodyCueKind.StepTowardSpikeWall:
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
            case CrocomireBodyCueKind.SkeletonCollapsed:
                state.MarkSkeletonCollapsed();
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyCueKind)} {kind}.");
        }
    }

    /// <summary>Whether Crocomire's one-in-four spit roll starts a spit attack where his walk offers one.</summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    private static bool RollsProjectileAttack(int sample) => CrocomireRandomDefinitions.ProjectileAttack.IsTakenBy(sample);

    /// <summary>Whether the body's left edge lies within the step back's reach right of the camera: the step-back moves' on-screen test.</summary>
    private static bool CrocomireBodyIsOnScreen(RoomEnemyPlace body, int cameraX) =>
        body.Position.X.Whole - body.Radius.X - CrocomireFightDefinitions.StepBackScreenReach < cameraX;

    /// <summary>
    /// Runs Crocomire's fight AI for the phase his fight is in. Each phase is entered as the animation it plays starts, so
    /// the phase alone says what his body is doing (#2180).
    /// </summary>
    private CrocomireBodyAnimation? RunCrocomireFightCue(
        CrocomireEnemyState state,
        SamusState samus)
    {
        switch (state.Phase)
        {
            case CrocomirePhase.Dormant:
                if (Math.Abs(state.Body.Position.X.Whole - samus.Position.X.Whole) < CrocomireFightDefinitions.WakeDistance)
                {
                    state.Phase = CrocomirePhase.WaitingForFirstDamage;
                    return CrocomireBodyAnimation.AwaitingHit;
                }
                return null;

            case CrocomirePhase.SteppingForward:
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

            case CrocomirePhase.ProjectileAttack:
                if (state.StepBack.Take())
                {
                    state.Phase = CrocomirePhase.SteppingBack;
                    return CrocomireBodyAnimation.StepBack;
                }
                if (state.ShotsSpat < CrocomireProjectileDefinitions.VolleyShotCount)
                {
                    int gradient = CrocomireVolley.GradientOf(state.ShotsSpat, _access.LastShotGradient);
                    state.ShotsSpat++;
                    SpawnCrocomireProjectile(state.Body, gradient);
                    _access.RequestCrocomireSound(CrocomireSoundDefinitions.ProjectileSpit);
                    return null;
                }
                state.Phase = CrocomirePhase.SteppingForward;
                return CrocomireBodyAnimation.StepForwardAfterDelay;

            case CrocomirePhase.NearSpikeWallCharge:
                if (state.StepBack.Take())
                {
                    state.Phase = CrocomirePhase.SteppingBack;
                    return CrocomireBodyAnimation.StepBack;
                }
                return null;

            case CrocomirePhase.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                if (state.StepCounter != 0)
                    return CrocomireBodyAnimation.SteppingBack;
                state.Phase = CrocomirePhase.SteppingForward;
                return CrocomireBodyAnimation.StepForward;

            case CrocomirePhase.BackingOffSpikeWall:
                if (state.Body.Position.X.Whole >= CrocomireFightDefinitions.SpikeWallChargeX)
                {
                    state.Phase = CrocomirePhase.SteppingForward;
                    return CrocomireBodyAnimation.StepForward;
                }
                return null;

            case CrocomirePhase.WaitingForFirstDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    CrocomirePhase.WaitingForSecondDamage);

            case CrocomirePhase.WaitingForSecondDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    CrocomirePhase.SteppingBack);

            case CrocomirePhase.PowerBombCharge:
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
            }

            default:
                throw new InvalidDataException(
                    $"Crocomire fight has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>
    /// Waits for a hit while the damage wait roars: a hit steps him back into <paramref name="damagedDestination"/>;
    /// otherwise his roar plays on, the damage wait's animation roaring again each time his mouth has closed.
    /// </summary>
    private static CrocomireBodyAnimation? RunCrocomireWaitingForDamage(
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
    internal void SpawnCrocomireProjectile(RoomEnemyPlace body, int gradient)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CrocomireProjectile,
            _combat.MouthShot,
            graphics: SpriteGraphics.SharedIn(CrocomireColours.ProjectileShading));
        projectile.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileSegment.Mouth));
        projectile.Velocity = CrocomireFightDefinitions.SpitVelocity;
        projectile.Position = projectile.Position.WithWhole(body.Position.Whole + CrocomireFightDefinitions.SpitOffset);
        projectile.Position = projectile.Position.WithoutFractions();
        projectile.State = new CrocomireProjectileState(gradient);
    }
}
