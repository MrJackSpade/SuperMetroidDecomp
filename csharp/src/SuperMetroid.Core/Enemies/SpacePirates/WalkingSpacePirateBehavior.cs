using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The walking space pirate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WalkingSpacePirateBehavior : IEnemySpecies, IEnemySpawnFactory<WalkingSpacePirateSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateRedWalking, EnemyDefinitionId.PirateSilverWalking];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, WalkingSpacePirateSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new WalkingSpacePirateEntity(this, occupant, place, common, settings));

    /// <summary>Supplies solid-ground and ledge probes for the patrol AI.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the shared Pirate/Mother Brain laser projectile.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Runs common Samus contact for the ordinary Pirate families.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Handles Pirate death animation and Gold Ninja's special pickup scatter.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the patrol collision owner and laser pool.</summary>
    internal WalkingSpacePirateBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _audio = audio;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Carries out one cue of the walking Space Pirates' animations. Choosing a movement continues at the walk or laser
    /// animation it selects, as the cue returns that animation; every other cue runs on.
    /// </summary>
    private WalkingSpacePirateSegment? ApplyWalkingSpacePirateCue(RoomEnemyOccupant occupant, WalkingSpacePirateSpawnSettings walkingSpacePirateSpawnSettings, RoomEnemyPlace place, WalkingSpacePirateEnemyState walkingSpacePirateEnemyState, WalkingSpacePirateCue cue, SamusState samus)
    {
        switch (cue)
        {
            case WalkingSpacePirateCue.FireLaser fireLaser:
                _ = SpawnSpacePirateLaser(occupant, fireLaser.Direction, fireLaser.LaserHeight,
                    walkingSpacePirateSpawnSettings.LaserSpeed);
                return null;
            case WalkingSpacePirateCue.Signal { Kind: WalkingSpacePirateCueKind.ChooseMovement }:
                return SelectWalkingSpacePirateMovement(place, samus);
            case WalkingSpacePirateCue.Begin begin:
                // The pirate animation does not jump to the stand or walk it begins: main AI runs it from the next
                // frame, as the animation runs on.
                RequireWalkingSpacePirateState(place, walkingSpacePirateEnemyState).Action = begin.Action;
                return null;
            case WalkingSpacePirateCue.BeginHeading begin:
                // As a plain begin, main AI runs the walk from the next frame, the cue's way (#2165).
                WalkingSpacePirateEnemyState walker = RequireWalkingSpacePirateState(place, walkingSpacePirateEnemyState);
                walker.Action = begin.Action;
                walker.Heading = begin.Heading;
                return null;
            default:
                throw new InvalidOperationException($"The walking Space Pirates' animations have no {cue} cue.");
        }
    }

    /// <summary>
    /// Queues <paramref name="sound"/> as a space pirate makes it, after the update's earlier requests: walking, wall and
    /// ninja pirates and Mother Brain's pirate laser all request their sounds through this.
    /// </summary>
    internal void RequestSpacePirateSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.Normal);

    /// <summary>Initializes the walking Space Pirate.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static WalkingSpacePirateEnemyState InitializeWalkingSpacePirate(RoomEnemyOccupant occupant, WalkingSpacePirateSpawnSettings settings, RoomEnemyPlace place)
    {
        var state = new WalkingSpacePirateEnemyState(WalkingSpacePirateAction.StandingStill);

        // The placement's facing alone selects the first walk.
        state.Playback = new(WalkingSpacePirateAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? WalkingSpacePirateSegment.WalkingRight
            : WalkingSpacePirateSegment.WalkingLeft), AnimationPlace.StartOfAnimation);

        // The posts wrap across the room's coordinate seam, as the game's additions do, so a modded patrol
        // reach can straddle it; the patrol compares positions by their signed difference, which still
        // sees each post on its own side of the pirate.
        state.RightPostX = OffRoomPlaces.Place(place.Position.X.Whole + settings.PatrolReach);
        state.LeftPostX = OffRoomPlaces.Place(place.Position.X.Whole - settings.PatrolReach);
        return state;
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    private void RunWalkingSpacePirateMain(
        RoomEnemyOccupant occupant, WalkingSpacePirateSpawnSettings walkingSpacePirateSpawnSettings, RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level,
        SamusProjectileSystem samusProjectiles)
    {
        switch (state.Action)
        {
            case WalkingSpacePirateAction.StandingStill:
                break;

            case WalkingSpacePirateAction.Walking when state.Heading == HorizontalDirection.Left:
                RunWalkingSpacePirateLeft(place, state, samus, level);
                break;

            case WalkingSpacePirateAction.Walking:
                RunWalkingSpacePirateRight(place, state, samus, level);
                break;

            default:
                throw new InvalidDataException(
                    $"Walking Space Pirate has no behaviour for action {state.Action}.");
        }

        // A pirate placed to flinch looks for Samus's shots after its normal AI, and a flinch may replace an animation
        // selected just above.
        if (walkingSpacePirateSpawnSettings.ShotWatch == WalkingSpacePirateShotWatch.FlinchesFromShots)
            TryTriggerWalkingSpacePirateFlinch(place, state, samus, samusProjectiles);
    }

    /// <summary>Runs the asymmetric left walk.</summary>
    private void RunWalkingSpacePirateLeft(
        RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(place, samus))
        {
            InstallWalkingSpacePirateAttack(place, state, samus);
            return;
        }

        // The first one-pixel downward move doubles as the grounded test. A move that hits
        // nothing means there was no floor, and the move itself is retained as falling.
        if (!_terrain.MoveEnemyVertically(level, place, SpacePirateMotionDefinitions.WalkingGroundProbeStep))
            return;

        // Left and right are not mirrors: walking left probes for floor a pixel further ahead than walking right, and its
        // step is a 65536th of a pixel longer.
        int originalX = place.Position.X.Whole;
        place.Position = place.Position.WithWholeX(
            place.Position.X.Whole - SpacePirateMotionDefinitions.WalkingLeftLedgeProbe);
        bool floorAhead = _terrain.MoveEnemyVertically(level, place, SpacePirateMotionDefinitions.WalkingGroundProbeStep);
        place.Position = place.Position.WithWholeX(originalX);

        if (floorAhead)
        {
            // With floor ahead, the Pirate steps left and walks on until a wall or its left post stops it.
            bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                displacement: SpacePirateMotionDefinitions.WalkingLeftStep);
            if (!hitWall &&
                !DistanceAcrossRoomEdge.LiesBefore(place.Position.X.Whole, state.LeftPostX))
            {
                return;
            }
        }

        StartWalkingSpacePirateAnimation(
            place,
            state,
            WalkingSpacePirateSegment.LookingFacingLeft);
    }

    /// <summary>Runs the right walk.</summary>
    private void RunWalkingSpacePirateRight(
        RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (WalkingSpacePirateIsVerticallyClose(place, samus))
        {
            InstallWalkingSpacePirateAttack(place, state, samus);
            return;
        }

        if (!_terrain.MoveEnemyVertically(level, place, SpacePirateMotionDefinitions.WalkingGroundProbeStep))
            return;

        int originalX = place.Position.X.Whole;
        place.Position = place.Position.WithWholeX(
            place.Position.X.Whole + SpacePirateMotionDefinitions.WalkingRightLedgeProbe);
        bool floorAhead = _terrain.MoveEnemyVertically(level, place, SpacePirateMotionDefinitions.WalkingGroundProbeStep);
        place.Position = place.Position.WithWholeX(originalX);

        if (floorAhead &&
            !_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                displacement: SpacePirateMotionDefinitions.WalkingRightStep) &&
            DistanceAcrossRoomEdge.LiesBefore(place.Position.X.Whole, state.RightPostX))
        {
            return;
        }

        StartWalkingSpacePirateAnimation(
            place,
            state,
            WalkingSpacePirateSegment.LookingFacingRight);
    }

    /// <summary>Flinches when the one shot the flinch tests (<see cref="PirateFlinchFromHighestShot"/>) is close on both axes.</summary>
    private static void TryTriggerWalkingSpacePirateFlinch(
        RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        SamusProjectileSystem projectiles)
    {
        // A walking pirate reacts to any shot by its kind, not its picture.
        SamusProjectile? shot = PirateFlinchFromHighestShot.Shot(projectiles.Shots, projectile => projectile.IsOccupied);
        if (shot is null ||
            Math.Abs(shot.Position.X.Whole - place.Position.X.Whole) >=
                SpacePirateMotionDefinitions.WalkingFlinchReach ||
            Math.Abs(shot.Position.Y.Whole - place.Position.Y.Whole) >=
                SpacePirateMotionDefinitions.WalkingFlinchReach)
        {
            return;
        }

        WalkingSpacePirateSegment flinch = place.Position.X.Whole < samus.Position.X.Whole
            ? WalkingSpacePirateSegment.FlinchFacingRight
            : WalkingSpacePirateSegment.FlinchFacingLeft;
        StartWalkingSpacePirateAnimation(place, state, flinch);
    }

    /// <summary>Runs the movement cue's direct animation selection.</summary>
    private static WalkingSpacePirateSegment SelectWalkingSpacePirateMovement(
        RoomEnemyPlace place,
        SamusState samus)
    {
        bool samusIsRightOrAligned =
            samus.Position.X.Whole >= place.Position.X.Whole;
        if (WalkingSpacePirateIsVerticallyClose(place, samus))
        {
            return samusIsRightOrAligned
                ? WalkingSpacePirateSegment.FireLasersRight
                : WalkingSpacePirateSegment.FireLasersLeft;
        }

        // When Samus is not in the firing band, the Pirate walks toward the opposite side:
        // Samus right selects leftward walking, Samus left selects rightward walking.
        return samusIsRightOrAligned
            ? WalkingSpacePirateSegment.WalkingLeft
            : WalkingSpacePirateSegment.WalkingRight;
    }

    /// <summary>Tests vertical separation against the firing band (<see cref="SpacePirateMotionDefinitions.WalkingFiringBand"/>).</summary>
    private static bool WalkingSpacePirateIsVerticallyClose(
        RoomEnemyPlace place,
        SamusState samus) =>
        Math.Abs(samus.Position.Y.Whole - place.Position.Y.Whole) <
            SpacePirateMotionDefinitions.WalkingFiringBand;

    /// <summary>Chooses a left or right laser animation from Samus's current side.</summary>
    private static void InstallWalkingSpacePirateAttack(
        RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus) =>
        StartWalkingSpacePirateAnimation(
            place,
            state,
            samus.Position.X.Whole >= place.Position.X.Whole
                ? WalkingSpacePirateSegment.FireLasersRight
                : WalkingSpacePirateSegment.FireLasersLeft);

    /// <summary>Arms a newly selected animation for processing on the next tick.</summary>
    private static void StartWalkingSpacePirateAnimation(
        RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        WalkingSpacePirateSegment segment)
    {
        state.Playback = new(WalkingSpacePirateAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// Runs the common laser setup. Wall and walking Pirates
    /// use different actor animation steps and muzzle offsets, but the allocated projectile is the
    /// same definition and therefore belongs in one shared implementation. The firing Pirate hands over the speed its
    /// placement names for its lasers (#2453).
    /// </summary>
    internal bool SpawnSpacePirateLaser(
        RoomEnemyOccupant source,
        HorizontalDirection direction,
        int laserHeight,
        PirateLaserSpeed speed)
    {
        RoomEnemyPlace place = Ensure.NotNull(source).Place;
        EnemyProjectile? projectile = SpawnPirateMotherBrainLaser(
            source,
            new(
                place.Position.X.Whole +
                    (SpacePirateMotionDefinitions.WalkingLaserMuzzleAhead.Toward(direction)),
                place.Position.Y.Whole - laserHeight),
            direction,
            speed);
        if (projectile is null)
            return false;

        RequestSpacePirateSound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound);
        return true;
    }

    /// <summary>
    /// Runs the common laser setup once the firing enemy has chosen
    /// its muzzle coordinate. Walking Pirates, wall Pirates, and Mother Brain enter through
    /// different actor animation steps, but the game deliberately funnels all of them through
    /// this definition so animation, speed flags, collision damage, and disposal agree.
    /// </summary>
    /// <param name="source">The enemy firing the laser, whose damage the laser takes.</param>
    /// <param name="muzzle">Where the laser starts.</param>
    /// <param name="direction">The way the laser travels.</param>
    /// <param name="speed">The laser's speed, which the firing enemy's placement names.</param>
    internal EnemyProjectile? SpawnPirateMotherBrainLaser(
        RoomEnemyOccupant source,
        Position16<RoomPixels> muzzle,
        HorizontalDirection direction,
        PirateLaserSpeed speed)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.PirateMotherBrainLaser, Laser, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return null;


        projectile.Position = FixedPosition<RoomPixels>.FromWhole(muzzle);
        projectile.StartAnimation(new SpacePirateProjectileAnimation(Laser, direction == HorizontalDirection.Right
            ? SpacePirateProjectileSegment.LaserRight
            : SpacePirateProjectileSegment.LaserLeft));

        // The laser holds still through its muzzle frames until its animation fires it, running its flight
        // at once.
        Laser.Lasers.Start(projectile, new PirateLaserState(speed, direction));

        // The setup replaces the projectile definition's properties with the enemy's damage and the
        // laser's own property. The laser does not take the Pirate's pictures or colours: it draws from the room's
        // shared sheet in its standard colours.
        projectile.Damage = Ensure.NotNull(source).Definition.Damage;
        projectile.CanDamageSamus = true;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return projectile;
    }

    /// <summary>Flies a laser one step its way, deleting it once it leaves the camera's view.</summary>
    private static void FlyPirateLaser(
        LaserBehavior owner,
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        PirateLaserState laser = owner.Lasers.Of(projectile);
        int pixels = laser.Speed == PirateLaserSpeed.Slow
            ? SpacePirateMotionDefinitions.WalkingSlowLaserStep
            : SpacePirateMotionDefinitions.WalkingFastLaserStep;
        // A laser flies through walls and leaves only through the view's edge. Flying left with the view at the room's left
        // edge, its last step takes it past the room's origin, where it continues from the space's far end, as
        // OffRoomPlaces places it, and the view test below deletes it.
        projectile.Position = projectile.Position.WithWholeX(laser.Direction == HorizontalDirection.Right
            ? projectile.Position.X.Whole + pixels
            : OffRoomPlaces.Place(projectile.Position.X.Whole - pixels));

        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }

    /// <summary>Requires the enemy to have been initialized as a walking pirate.</summary>
    internal static WalkingSpacePirateEnemyState RequireWalkingSpacePirateState(
        RoomEnemyPlace place, WalkingSpacePirateEnemyState walkingSpacePirateEnemyState) =>
        walkingSpacePirateEnemyState;

    /// <summary>
    /// The Pirates' and Mother Brain's laser (#1760): its spawner starts its animation and chooses its way, and the
    /// animation fires it.
    /// </summary>
    private LaserBehavior Laser { get; } = new();

    /// <summary>A laser: it flies its way until it leaves the camera, once its animation fires it (#2278).</summary>
    internal sealed class LaserBehavior()
        : EnemyProjectileMotionBehavior<SingleMotion>(initial: null), IEnemyProjectileCues<SpacePirateProjectileCue>
    {
        /// <summary>Each laser's speed and way, given as its firer aims it.</summary>
        internal EnemyProjectileStates<PirateLaserState> Lasers { get; } = new();

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            FlyPirateLaser(this, context.Projectile, context.Camera);

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// The cue is not a laser's firing, or fires the laser a way other than its firer chose.
        /// </exception>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
        {
            if (cue.Kind != SpacePirateProjectileCueKind.FireLaser)
                throw new InvalidOperationException($"A laser's animation published {cue}.");
            EnemyProjectile projectile = context.Projectile;
            HorizontalDirection chosen = Lasers.Of(projectile).Direction;
            if (cue.Direction != chosen)
                throw new InvalidOperationException($"A laser its firer aimed {chosen} played the animation that fires it {cue.Direction}.");

            // The laser flies from now on, and its first flight runs at once, so it moves on the update it fires and may
            // leave the screen then.
            projectile.StartMotion(this, SingleMotion.Moving);
            FlyPirateLaser(this, projectile, context.Camera);
        }
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    internal void RunWalkingSpacePirateMainAi(RoomEnemyOccupant occupant, WalkingSpacePirateSpawnSettings walkingSpacePirateSpawnSettings, RoomEnemyPlace place, WalkingSpacePirateEnemyState walkingSpacePirateEnemyState, IEnemyAiFrame frame)
    {
        RunWalkingSpacePirateMain(
            occupant, walkingSpacePirateSpawnSettings, place,
            RequireWalkingSpacePirateState(place, walkingSpacePirateEnemyState),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances the walking Space Pirate's typed animation.</summary>
    internal void AdvanceWalkingSpacePirateTypedAnimation(RoomEnemyOccupant occupant, WalkingSpacePirateSpawnSettings walkingSpacePirateSpawnSettings, RoomEnemyPlace place, WalkingSpacePirateEnemyState walkingSpacePirateEnemyState, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = RequireWalkingSpacePirateState(place, walkingSpacePirateEnemyState);
        AnimationPlayback<WalkingSpacePirateSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, WalkingSpacePirateAnimations.Segments, ref playback,
            cue => ApplyWalkingSpacePirateCue(occupant, walkingSpacePirateSpawnSettings, place, walkingSpacePirateEnemyState, cue, context.Samus) is { } continuesAt
                ? new AnimationContinuation<WalkingSpacePirateSegment>(WalkingSpacePirateAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>
    /// Runs the extended-hitbox shot reaction <paramref name="pirateAction"/> the Pirate's hitbox selected, including
    /// collision marking before rejection or reflection.
    /// </summary>
    internal bool ResolveSpacePirateHitboxShot(in EnemyShotContext c, PirateHitboxShotActions.PirateHitboxShotAction pirateAction)
    {
        if (pirateAction != PirateHitboxShotActions.PirateHitboxShotAction.Normal)
        {
            // The collision marks a non-plasma projectile as having entered an extended hitbox before the
            // selected reaction runs. Normal hits get the equivalent lifecycle transition from TryStartEnemyImpact;
            // rejected and reflected hits return before that shared path. An enemy that stops the Plasma Beam also
            // forces the mark.
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Ignore)
        {
            // The ninja's body ignores a just-fired Super Missile that has no link yet; the ignored shot is consumed
            // and touches no further hitbox.
            return true;
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Reflect)
        {
            c.Enemy.Invincibility.Restart(SpacePirateCollisionDefinitions.ReflectInvincibility);
            // A shot aimed straight left or right reflects up and away; every other aim reflects down.
            SamusProjectileDirection reflected = c.Projectile.Direction switch
            {
                SamusProjectileDirection.Left => SamusProjectileDirection.UpRight,
                SamusProjectileDirection.Right => SamusProjectileDirection.UpLeft,
                SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or SamusProjectileDirection.DownRight
                    or SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft
                    or SamusProjectileDirection.DownLeft or SamusProjectileDirection.UpLeft
                    or SamusProjectileDirection.UpFacingLeft => SamusProjectileDirection.DownFacingLeft,
                _ => throw new InvalidDataException($"Projectile aim {c.Projectile.Direction} decoded to an undefined direction."),
            };
            c.Projectile.Direction = reflected;
            c.Projectile.FlyOn();
            c.Projectiles.EnemyImpacts.ReflectFromEnemy(c.Samus,
                c.Shot);
            RequestSpacePirateSound(EnemySoundEffectDefinitions.WalkingPirateReflectSound);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the ordinary Pirates' bombs through the common bomb damage, showing the hit burst only where the reaction the
    /// bomb reaches shows it (<see cref="PirateHitboxShotActions.ShowsHitBurst"/>).
    /// </summary>
    internal static EnemyBombReaction SpacePirateBombReaction { get; } = new()
    {
        ShowsHitBurst = c => PirateHitboxShotActions.ShowsHitBurst(c.SelectedShot),
    };

    /// <summary>
    /// Runs the gold Ninja's bombs through the common bomb damage, showing the hit burst only where the reaction the bomb
    /// reaches shows it on the gold Ninja (<see cref="PirateHitboxShotActions.GoldNinjaShowsHitBurst"/>).
    /// </summary>
    internal static EnemyBombReaction GoldNinjaBombReaction { get; } = new()
    {
        ShowsHitBurst = c => PirateHitboxShotActions.GoldNinjaShowsHitBurst(c.SelectedShot),
    };

    /// <summary>Uses shared Pirate shot logic, including the hitbox quake and the big death explosion, on every Pirate but the gold Ninja.</summary>
    internal EnemyShotReaction SpacePirateShotReaction => field ??= new()
    {
        // Every Pirate but the gold Ninja goes on to its normal shot reaction from every hitbox, the Ninja's body reactions
        // included; the stage only refuses a reaction no Pirate hitbox names.
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (c.UsesFrameHitboxes)
                PirateHitboxShotActions.RequirePirateShotReaction(c.HitboxShot);
            return false;
        },
        ShowsHitBurst = c => PirateHitboxShotActions.ShowsHitBurst(c.HitboxShot),
        ReplacesGenericDeath = c =>
        {
            if (c.HitboxShot == EnemyReaction.OrdinaryShot)
                return false;
            StartPirateDeath(c);
            return true;
        },
    };

    /// <summary>The gold Ninja's shot reaction: its armour's reflections, the Pirate shot logic and the drops its death scatters.</summary>
    internal EnemyShotReaction GoldNinjaShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
            c.UsesFrameHitboxes && ResolveSpacePirateHitboxShot(c, PirateHitboxShotActions.SelectGoldNinjaHitboxShotAction(c.Occupant, c.Shot, c.HitboxShot)),
        ShowsHitBurst = c => PirateHitboxShotActions.GoldNinjaShowsHitBurst(c.HitboxShot),
        ReplacesGenericDeath = c =>
        {
            if (c.HitboxShot == EnemyReaction.OrdinaryShot)
                return false;
            // The Gold Ninja's death records its position as the drop origin before the enemy's death clears the
            // place; it then spawns the death explosion before the metal ninja Pirate's item drop spawns the drops.
            Position16<RoomPixels> dropOrigin = c.Enemy.Position.Whole;
            StartPirateDeath(c);
            _deathsAndDrops.SpawnEnemyDropScatterAround(EnemyDefinitionId.PirateGoldNinja, GoldNinjaSpacePirateDefinitions.DropScatterCount, dropOrigin, c.Samus);
            return true;
        },
    };

    /// <summary>Starts a Pirate's death by a shot: the big explosion its shot's death animation rule chooses.</summary>
    private void StartPirateDeath(EnemyShotContext c) =>
        _deathsAndDrops.StartGenericEnemyDeath(
            c.Occupant,
            EnemyDeathsAndDrops.ShotKillExplosion(c.Occupant, c.ProjectileType, rule: ShotDeathAnimationRule.PirateBigExplosion),
            c.Samus);

    /// <summary>Skips frozen contact and rejects unsupported extended hitbox reactions before normal touch damage.</summary>
    internal void ReactToSpacePirateTouch(EnemyTouchContext context)
    {
        // Every hitbox of every Space Pirate frame uses the shared Pirate touch reaction; a hitbox with any other
        // reaction is refused.
        if (context.UsesFrameHitboxes && context.HitboxTouch != SpacePirateReactions.Touch)
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to touch reaction {context.HitboxTouch}.");
        }
        // While the Pirate is frozen, touching it deals neither Samus nor the Pirate contact damage.
        if (context.Place.Freeze.IsRunning)
            return;
        _collision.ResolveOrdinaryTouch(context);
    }
}
