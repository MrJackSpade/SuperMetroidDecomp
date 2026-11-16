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
    }

    /// <summary>Sets Crocomire up when the room spawns it.</summary>
    internal void InitializeCrocomire(RoomEnemyPlace place)
    {
        _roomStatus.Boss = AreaBoss.Crocomire;
        var state = new CrocomireEnemyState(place);
        place.SetSpeciesState(state);
        _access.Crocomire = state;
        // Crocomire shows his body frames and his death's melt images on his body (#1355, #1466, #1515), nothing at first.
        _access.CrocomireDeath = new CrocomireDeathState();
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Crocomire));

        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            // The dead-room state keeps only Crocomire's skeleton display actor. Property
            // mask and these radii/coordinates are literal writes.
            place.Contact = place.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
            state.DeathPhase = CrocomireDeathPhase.InertCorpse;
            CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.Dead);
            place.Position = place.Position.WithWhole(new(576, 144));
            place.Radius = new(40, 28);
            foreach (Position16<RoomScreens> screen in CrocomireCameraDefinitions.ArenaScreens)
                _room.Services.RoomScroll.Write(screen, RoomScrollState.OpenAlignedToTop);
            _access.PublishCrocomireObject(new Position16<RoomBlocks>(32, 3), CrocomireArenaMutation.ClearInvisibleWall);
            _access.PublishCrocomireObject(new Position16<RoomBlocks>(30, 3), CrocomireArenaMutation.ClearInvisibleWall);
            _access.PublishCrocomireObject(new Position16<RoomBlocks>(97, 11), CrocomireArenaMutation.ClearBridge);
            return;
        }

        state.DeathPhase = CrocomireDeathPhase.Fighting;
        // On the living branch only.
        _cameraDistance.Request(CameraDistanceMode.BossTracking);
        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, CrocomireBodySegment.Initial);
        place.CollidesByFrameHitboxes = true;
        foreach (Position16<RoomScreens> screen in CrocomireCameraDefinitions.BehindWallScreens)
            _room.Services.RoomScroll.Write(screen, RoomScrollState.Closed);

        // The wall's and the projectiles' colours his initializer loads are the import's shades of them, which his wall,
        // the bridge's fragments and his projectiles draw in (#1512).
    }

    /// <summary>Sets Crocomire's tongue up when the room spawns it.</summary>
    internal void InitializeCrocomireTongue(RoomEnemyPlace place)
    {
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            place.Processing = place.Processing.Without(EnemyProcessing.Animates);
            place.Presence = place.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }

        CrocomireEnemyState state = _access.Crocomire ??
            throw new InvalidOperationException("Crocomire's tongue initialized before its body.");
        state.Tongue = place;
        CrocomireBodyLayer.StartCrocomireTongueAnimation(state, CrocomireTongueSegment.Fight, 1);
        place.CollidesByFrameHitboxes = true;
        var tongueState = new CrocomireTongueState { BodyXOffset = CrocomireBehavior.CrocomireTongueBodyXOffset };
        place.SetSpeciesState(tongueState);
        place.Shading = state.Body.Shading;
    }

    /// <summary>Runs the artificial left-side wall.</summary>
    internal void HandleCrocomireInvisibleWall(
        RoomEnemyPlace body,
        CrocomireEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        if (state.DeathPhase != CrocomireDeathPhase.Fighting)
            return;
        int leftEdge = CoordinateAxis.Wrap(
            body.Position.X.Whole - body.Radius.X - samus.Kinematics.Radius.X);
        if (CoordinateAxis.Difference(leftEdge, samus.Position.X.Whole) >= 0)
            return;

        _collision.ResolveNormalEnemyTouch(body, samus, pseudoScrew);
        samus.Position = samus.Position.WithWrappedWholeX(leftEdge);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(-4, -1));
    }

    /// <summary>
    /// Runs the tongue's animation where the shared enemy animation runs, with the timer gate
    /// and new-frame flag.
    /// </summary>
    internal void AdvanceCrocomireTongueAnimation(RoomEnemyPlace tongue)
    {
        CrocomireEnemyState state = _access.Crocomire is { } owner && ReferenceEquals(owner.Tongue, tongue)
            ? owner
            : throw new InvalidOperationException("Crocomire's tongue has no body owner.");
        if (!tongue.AnimationClock.Expire())
            return;

        AnimationPlace place = state.TonguePlayback.Place;
        AnimationShown<CrocomireTongueSegment, EnemyFrameId> outcome = CrocomireAnimations.TongueSegments.PlayToNextFrame(
            state.TonguePlayback.Animation, ref place,
            cue => throw new InvalidOperationException($"A Crocomire tongue animation raised event {cue}."));
        state.TonguePlayback = state.TonguePlayback with { Place = place };
        // A hold rests the animation: the timer wraps from zero and the last frame stays.
        if (outcome is AnimationShown<CrocomireTongueSegment, EnemyFrameId>.Held)
            return;
        if (outcome is not AnimationShown<CrocomireTongueSegment, EnemyFrameId>.Frame shown)
            throw new InvalidOperationException($"Crocomire's tongue animation ended an update with {outcome}.");
        tongue.Frame = shown.Shown;
        tongue.AnimationClock.Show(shown.Duration);
    }

    /// <summary>
    /// Runs Crocomire's body animation where the shared enemy animation runs: the frame expires on the
    /// update its timer would, and a newly shown frame raises the new-frame flag exactly as an
    /// authored timed frame does.
    /// </summary>
    internal void AdvanceCrocomireBodyAnimation(RoomEnemyPlace body, SamusState samus, RoomLevelData level, int cameraX)
    {
        CrocomireEnemyState state = _access.RequireCrocomire(body);
        if (!body.AnimationClock.Expire())
            return;

        Position16<RoomPixels> placedAt = body.Position.Whole;
        Animation<CrocomireBodySegment> animation = state.BodyPlayback.Animation;
        AnimationPlace place = state.BodyPlayback.Place;
        // Each cue's reaction reads where the play stands just past the cue, which the play has written to these
        // locals by then: the fight AI compares the segment the body then rests in.
        AnimationShown<CrocomireBodySegment, EnemyFrameId> outcome = CrocomireBodyAnimations.Segments.PlayToNextFrame(
            ref animation, ref place,
            cue => ApplyCrocomireBodyCue(state, samus, level, cameraX, cue,
                    CrocomireEnemyState.ResidentSegment(new AnimationPlayback<CrocomireBodySegment>(animation, place))) is { } started
                ? new AnimationContinuation<CrocomireBodySegment>(CrocomireBodyAnimations.StartingWith(started), AnimationPlace.StartOfAnimation)
                : null);
        state.BodyPlayback = new AnimationPlayback<CrocomireBodySegment>(animation, place);
        // His steps move him after his update has placed his tongue, and his body's picture shows where they leave him:
        // the tongue's picture follows them so that it stays against the body's. The tongue itself, which Samus touches,
        // stays where it was placed until its next placement.
        if (state.Tongue is { } tongue)
            tongue.DrawOffset += body.Position.Whole - placedAt;
        // A hold rests the animation: the timer wraps from zero and the last frame stays. A held frame (#1627) shows its
        // frame for good, its timer left to wrap from zero as a hold's is. The body animations have no pauses or ends.
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
    /// <param name="continuation">The segment the body's animation rests in just past the cue, which the fight AI compares.</param>
    /// <returns>The segment whose animation starts, or null to continue after the cue.</returns>
    private CrocomireBodySegment? ApplyCrocomireBodyCue(
        CrocomireEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int cameraX,
        CrocomireBodyCue cue,
        CrocomireBodySegment continuation)
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
                return ApplyCrocomireBodySignal(state, samus, level, signal.Kind, continuation);
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyCue)} {cue}.");
        }
    }

    /// <summary>Whether a step under <paramref name="condition"/> moves the body now.</summary>
    private static bool StepMoves(CrocomireStepCondition condition, CrocomireEnemyState state, RoomEnemyPlace body, int cameraX) => condition switch
    {
        CrocomireStepCondition.Always => true,
        CrocomireStepCondition.UnlessHitPending => !state.DamageReactionPending,
        CrocomireStepCondition.WhileOnScreen => CrocomireBodyIsOnScreen(body, cameraX),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireStepCondition)} {condition}."),
    };

    /// <summary>Carries out one signalling body-animation cue; the fight AI and the spike-wall step may start another animation.</summary>
    private CrocomireBodySegment? ApplyCrocomireBodySignal(
        CrocomireEnemyState state,
        SamusState samus,
        RoomLevelData level,
        CrocomireBodyCueKind kind,
        CrocomireBodySegment continuation)
    {
        RoomEnemyPlace body = state.Body;
        switch (kind)
        {
            case CrocomireBodyCueKind.FightAi:
                return RunCrocomireFightCue(state, samus, continuation);

            case CrocomireBodyCueKind.MaybeStartProjectileAttack:
                if (RollsProjectileAttack(_access.ReadCrocomireRandom()))
                {
                    state.Phase = CrocomirePhase.ProjectileAttack;
                    state.ShotsSpat = 0;
                    return CrocomireBodySegment.ProjectileAttack;
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
                _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 5);
                _access.RequestCrocomireSound(CrocomireSoundDefinitions.ScreenShake);
                return null;

            case CrocomireBodyCueKind.StepTowardSpikeWall:
                if (MoveCrocomire(body, level, -4))
                {
                    state.Phase = CrocomirePhase.BackingOffSpikeWall;
                    return CrocomireBodySegment.BackOffFromSpikeWall;
                }
                int random = _access.ReadCrocomireRandom();
                int baseOffset = BossRandomRolls.InHalfFrom(random, threshold: 2048) ? -32 : 32;
                _access.SpawnCrocomireDust(state, baseOffset + CrocomireRandomDefinitions.SpikeWallDustSpread.Of(random));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyCueKind)} {kind}.");
        }
    }

    /// <summary>Whether Crocomire's one-in-four spit roll starts a spit attack where his walk offers one.</summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    private static bool RollsProjectileAttack(int sample) => CrocomireRandomDefinitions.ProjectileAttack.IsTakenBy(sample);

    /// <summary>Whether the body's left edge lies within 260 pixels right of the camera: the step-back moves' on-screen test.</summary>
    private static bool CrocomireBodyIsOnScreen(RoomEnemyPlace body, int cameraX) =>
        CoordinateAxis.Difference(body.Position.X.Whole - body.Radius.X - 260, cameraX) < 0;

    /// <summary>Runs Crocomire's fight AI for the phase his fight is in.</summary>
    private CrocomireBodySegment? RunCrocomireFightCue(
        CrocomireEnemyState state,
        SamusState samus,
        CrocomireBodySegment continuation)
    {
        switch (state.Phase)
        {
            case CrocomirePhase.Dormant:
                if (CoordinateAxis.Distance(
                        state.Body.Position.X.Whole, samus.Position.X.Whole) < 224)
                {
                    state.Phase = CrocomirePhase.WaitingForFirstDamage;
                    return CrocomireBodySegment.WaitForFirstSecondDamage;
                }
                return null;

            case CrocomirePhase.SteppingForward:
                if (state.DamageReactionPending)
                {
                    state.DamageReactionPending = false;
                    if (state.StepCounter != 0)
                    {
                        state.Phase = CrocomirePhase.SteppingBack;
                        return CrocomireBodySegment.StepBack;
                    }
                }
                if (CoordinateAxis.Difference(
                        state.Body.Position.X.Whole, CrocomireBehavior.CrocomireSpikeWallThreshold) < 0)
                {
                    state.Phase = CrocomirePhase.NearSpikeWallCharge;
                    return CrocomireBodySegment.NearSpikeWallCharge;
                }
                return !CrocomireBodyAnimationGroups.StepForwardInProgress.Contains(continuation)
                    ? CrocomireBodySegment.StepForward
                    : null;

            case CrocomirePhase.ProjectileAttack:
                if (state.DamageReactionPending)
                {
                    state.DamageReactionPending = false;
                    state.Phase = CrocomirePhase.SteppingBack;
                    return CrocomireBodySegment.StepBack;
                }
                if (state.ShotsSpat < CrocomireProjectileDefinitions.VolleyShotCount)
                {
                    state.ShotsSpat++;
                    SpawnCrocomireProjectile(state.Body, state.ShotsSpat);
                    _access.RequestCrocomireSound(CrocomireSoundDefinitions.ProjectileSpit);
                    return null;
                }
                state.Phase = CrocomirePhase.SteppingForward;
                return CrocomireBodySegment.StepForwardAfterDelay;

            case CrocomirePhase.NearSpikeWallCharge:
                if (state.DamageReactionPending)
                {
                    state.DamageReactionPending = false;
                    state.Phase = CrocomirePhase.SteppingBack;
                    return CrocomireBodySegment.StepBack;
                }
                return null;

            case CrocomirePhase.SteppingBack:
                if (state.StepCounter != 0)
                    state.StepCounter--;
                if (state.StepCounter != 0)
                    return CrocomireBodySegment.SteppingBack;
                state.Phase = CrocomirePhase.SteppingForward;
                return CrocomireBodySegment.StepForward;

            case CrocomirePhase.BackingOffSpikeWall:
                if (CoordinateAxis.Difference(
                        state.Body.Position.X.Whole, CrocomireBehavior.CrocomireSpikeWallThreshold) >= 0)
                {
                    state.Phase = CrocomirePhase.SteppingForward;
                    return CrocomireBodySegment.StepForward;
                }
                return null;

            case CrocomirePhase.WaitingForFirstDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    continuation,
                    CrocomirePhase.WaitingForSecondDamage);

            case CrocomirePhase.WaitingForSecondDamage:
                return RunCrocomireWaitingForDamage(
                    state,
                    continuation,
                    CrocomirePhase.SteppingBack);

            case CrocomirePhase.PowerBombCharge:
            {
                // The charge consumes one step and ends once fewer than two remain; a count that would drop below
                // zero ends it the same way, as the game's wrapped count reads negative.
                int remaining = state.StepCounter - 1;
                if (remaining < 2)
                {
                    state.StepCounter = 0;
                    state.Phase = CrocomirePhase.SteppingForward;
                    return CrocomireBodySegment.StepForward;
                }
                state.StepCounter = remaining;
                return null;
            }

            default:
                throw new InvalidDataException(
                    $"Crocomire fight has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>Holds the damage-reaction cue until its timer permits the next step.</summary>
    private static CrocomireBodySegment? RunCrocomireWaitingForDamage(
        CrocomireEnemyState state,
        CrocomireBodySegment continuation,
        CrocomirePhase damagedDestination)
    {
        if (state.DamageReactionPending)
        {
            state.DamageReactionPending = false;
            state.Phase = damagedDestination;
            return CrocomireBodySegment.StepBack;
        }
        return !CrocomireBodyAnimationGroups.RoarYetToCloseMouth.Contains(continuation)
            ? CrocomireBodySegment.Roar
            : null;
    }

    /// <summary>Moves Crocomire through ordinary ignore-slopes collision with 1/65536-pixel displacement.</summary>
    private bool MoveCrocomire(RoomEnemyPlace place, RoomLevelData level, int pixels) =>
        // The cues pass a whole ±4 pixels as a 1/65536-pixel displacement into the ordinary ignore-slopes
        // enemy mover; retaining that precision here preserves collision-edge alignment.
        place.DefinitionId == EnemyDefinitionId.Crocomire &&
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, SignedSixteenSixteen.FromWholePixels(pixels));

    /// <summary>Chooses a foot-dust offset from room RNG and spawns the matching effect.</summary>
    private void SpawnCrocomireRandomFootDust(CrocomireEnemyState state)
    {
        int random = _access.ReadCrocomireRandom();
        int offset = CrocomireRandomDefinitions.FootDustOffset.Of(random);
        if (BossRandomRolls.InHalfFrom(random, threshold: 4096))
            offset = -offset;
        _access.SpawnCrocomireDust(state, offset);
    }

    /// <summary>Spawns one of Crocomire's fight projectiles, the volley's <paramref name="shot"/>.</summary>
    internal void SpawnCrocomireProjectile(RoomEnemyPlace body, int shot)
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
        projectile.Velocity = new(SignedEightEight.FromPixels(-2), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(1));
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole - 32), CoordinateAxis.Wrap(body.Position.Y.Whole - 16)));
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.State = new CrocomireProjectileState(shot);
    }
}
