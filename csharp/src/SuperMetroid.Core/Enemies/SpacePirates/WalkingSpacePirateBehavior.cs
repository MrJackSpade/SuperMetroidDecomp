using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The walking space pirate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WalkingSpacePirateBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateRedWalking, EnemyDefinitionId.PirateSilverWalking], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new WalkingSpacePirateEntity(this, occupant, place, common));

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

    /// <summary>The way a shot aimed each way leaves a Space Pirate that reflects it.</summary>
    private static IdentityTable<SamusProjectileDirection, SamusProjectileDirection> Reflections => field ??= new(new Dictionary<SamusProjectileDirection, SamusProjectileDirection>
    {
        [SamusProjectileDirection.Left] = SamusProjectileDirection.UpRight,
        [SamusProjectileDirection.Right] = SamusProjectileDirection.UpLeft,
        [SamusProjectileDirection.UpFacingRight] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.UpRight] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.DownRight] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.DownFacingRight] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.DownFacingLeft] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.DownLeft] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.UpLeft] = SamusProjectileDirection.DownFacingLeft,
        [SamusProjectileDirection.UpFacingLeft] = SamusProjectileDirection.DownFacingLeft,
    });

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
    private WalkingSpacePirateSegment? ApplyWalkingSpacePirateCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, WalkingSpacePirateCue cue, SamusState samus) =>
        cue.Match(ApplyWalkingSpacePirateCueCases, new ApplyWalkingSpacePirateCueContext(occupant, place, samus, this));

    /// <summary>What <see cref="ApplyWalkingSpacePirateCue"/> does on each <see cref="WalkingSpacePirateCue"/>, built once (#2553).</summary>
    private static WalkingSpacePirateCueCases<ApplyWalkingSpacePirateCueContext, WalkingSpacePirateSegment?> ApplyWalkingSpacePirateCueCases { get; } = new(
            FireLaser: static (fireLaser, context) =>
            {
                _ = context.Owner.SpawnSpacePirateLaser(context.Occupant, fireLaser.Direction, fireLaser.LaserHeight,
                    context.Occupant.SpawnSettings<WalkingSpacePirateSpawnSettings>().LaserSpeed);
                return null;
            },
            Begin: static (begin, context) =>
            {
                // The pirate animation does not jump to the stand or walk it begins: main AI runs it from the next
                // frame, as the animation runs on.
                RequireWalkingSpacePirateState(context.Place).Action = begin.Action;
                return null;
            },
            BeginHeading: static (begin, context) =>
            {
                // As a plain begin, main AI runs the walk from the next frame, the cue's way (#2165).
                WalkingSpacePirateEnemyState walker = RequireWalkingSpacePirateState(context.Place);
                walker.Action = begin.Action;
                walker.Heading = begin.Heading;
                return null;
            },
            // The walking pirates' one signal chooses their movement.
            Signal: static (_, context) => SelectWalkingSpacePirateMovement(context.Place, context.Samus));

    /// <summary>What <see cref="ApplyWalkingSpacePirateCue"/>'s cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Place">The <c>place</c> <see cref="ApplyWalkingSpacePirateCue"/> was handed.</param>
    /// <param name="Samus">The <c>samus</c> <c>ApplyWalkingSpacePirateCue</c> was handed.</param>
    /// <param name="Owner">The <see cref="WalkingSpacePirateBehavior"/> reacting.</param>
    private readonly record struct ApplyWalkingSpacePirateCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Place, SamusState Samus, WalkingSpacePirateBehavior Owner);

    /// <summary>
    /// Queues <paramref name="sound"/> as a space pirate makes it, after the update's earlier requests: walking, wall and
    /// ninja pirates and Mother Brain's pirate laser all request their sounds through this.
    /// </summary>
    internal void RequestSpacePirateSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.Normal);

    /// <summary>Initializes the walking Space Pirate.</summary>
    internal static void InitializeWalkingSpacePirate(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new WalkingSpacePirateEnemyState(WalkingSpacePirateAction.StandingStill);
        place.SetSpeciesState(state);

        // The placement's facing alone selects the first walk.
        WalkingSpacePirateSpawnSettings settings = occupant.SpawnSettings<WalkingSpacePirateSpawnSettings>();
        state.Playback = new(WalkingSpacePirateAnimations.StartingWith(settings.Facing == HorizontalDirection.Right
            ? WalkingSpacePirateSegment.WalkingRight
            : WalkingSpacePirateSegment.WalkingLeft), AnimationPlace.StartOfAnimation);

        // The posts wrap across the room's coordinate seam, as the game's additions do, so a modded patrol
        // reach can straddle it; the patrol compares positions by their signed difference, which still
        // sees each post on its own side of the pirate.
        state.RightPostX = OffRoomPlaces.Place(place.Position.X.Whole + settings.PatrolReach);
        state.LeftPostX = OffRoomPlaces.Place(place.Position.X.Whole - settings.PatrolReach);
    }

    /// <summary>What <see cref="RunWalkingSpacePirateMain"/> does for each <see cref="WalkingSpacePirateAction"/> member, each member's own arm.</summary>
    private IdentityTable<WalkingSpacePirateAction, Action<RoomEnemyPlace, WalkingSpacePirateEnemyState, SamusState, RoomLevelData>> RunWalkingSpacePirateMainForWalkingSpacePirateAction => field ??= new(new Dictionary<WalkingSpacePirateAction, Action<RoomEnemyPlace, WalkingSpacePirateEnemyState, SamusState, RoomLevelData>>
    {
        [WalkingSpacePirateAction.StandingStill] = (place, state, samus, level) =>
        {
        },
        [WalkingSpacePirateAction.Walking] = (place, state, samus, level) =>
        {
            if (state.Heading == HorizontalDirection.Left)
            {
                RunWalkingSpacePirateLeft(place, state, samus, level);
            }
            else
            {
                RunWalkingSpacePirateRight(place, state, samus, level);
            }
        },
    });

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    private void RunWalkingSpacePirateMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus,
        RoomLevelData level,
        SamusProjectileSystem samusProjectiles)
    {
        RunWalkingSpacePirateMainForWalkingSpacePirateAction[state.Action](place, state, samus, level);

        // A pirate placed to flinch looks for Samus's shots after its normal AI, and a flinch may replace an animation
        // selected just above.
        if (occupant.SpawnSettings<WalkingSpacePirateSpawnSettings>().ShotWatch == WalkingSpacePirateShotWatch.FlinchesFromShots)
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.PirateMotherBrainLaser, Laser, graphics: SpriteGraphics.SharedStandard, new SpacePirateProjectileAnimation(Laser, direction == HorizontalDirection.Right
            ? SpacePirateProjectileSegment.LaserRight
            : SpacePirateProjectileSegment.LaserLeft));
        if (projectile is null)
            return null;


        projectile.Position = FixedPosition<RoomPixels>.FromWhole(muzzle);

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
        RoomEnemyPlace place) =>
        place.SpeciesState<WalkingSpacePirateEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy place {place.Ordinal} has no initialized walking Space Pirate state.");

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
        /// <remarks>
        /// The cue starts the laser's flight the way its firer chose, the one place the way is held (#2553); a firing cue, the
        /// laser's, also runs the flight's first update at once, so it moves on the update it fires and may leave the screen
        /// then, as <see cref="SpacePirateProjectileCueKind.FireLaser"/> says.
        /// </remarks>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            projectile.StartMotion(this, SingleMotion.Moving);
            if (cue.Kind == SpacePirateProjectileCueKind.FireLaser)
                FlyPirateLaser(this, projectile, context.Camera);
        }
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    internal void RunWalkingSpacePirateMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWalkingSpacePirateMain(
            occupant, place,
            RequireWalkingSpacePirateState(place),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances the walking Space Pirate's typed animation.</summary>
    internal void AdvanceWalkingSpacePirateTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = RequireWalkingSpacePirateState(place);
        AnimationPlayback<WalkingSpacePirateSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, WalkingSpacePirateAnimations.Segments, ref playback,
            cue => ApplyWalkingSpacePirateCue(occupant, place, cue, context.Samus) is { } continuesAt
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
        Ensure.NotNull(pirateAction);
        if (!pirateAction.RunsNormalReaction)
        {
            // The collision marks a non-plasma projectile as having entered an extended hitbox before the
            // selected reaction runs. Normal hits get the equivalent lifecycle transition from TryStartEnemyImpact;
            // rejected and reflected hits return before that shared path. An enemy that stops the Plasma Beam also
            // forces the mark.
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        }
        if (pirateAction.Reflected is { } reflectedFamily)
        {
            c.Enemy.Invincibility.Restart(SpacePirateCollisionDefinitions.ReflectInvincibility);
            // A shot aimed straight left or right reflects up and away; every other aim reflects down.
            SamusProjectileDirection reflected = Reflections[c.Projectile.Direction];
            c.Projectile.Direction = reflected;
            c.Projectile.FlyOn();
            c.Projectiles.EnemyImpacts.ReflectFromEnemy(c.Samus,
                c.Shot, reflectedFamily);
            RequestSpacePirateSound(EnemySoundEffectDefinitions.WalkingPirateReflectSound);
            return true;
        }
        // The ninja's body ignores a just-fired Super Missile that has no link yet, and what else it neither reflects nor
        // takes; the ignored shot is consumed and touches no further hitbox.
        return !pirateAction.RunsNormalReaction;
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
        // included.
        AfterSuperMissileQuake = (in EnemyShotContext c) => false,
        ShowsHitBurst = c => PirateHitboxShotActions.ShowsHitBurst(c.HitboxShot),
        ReplacesGenericDeath = c =>
        {
            if (c.HitboxShot.IsOrdinary)
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
            if (c.HitboxShot.IsOrdinary)
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

    /// <summary>
    /// Runs the touch a Pirate's hitbox or definition names: the shared Pirate touch, which skips a frozen Pirate before
    /// normal touch damage, or, at the shared point-at-origin frame's hitbox, the ordinary touch, which runs the common
    /// touch damage as the hitbox's own touch does.
    /// </summary>
    internal void ReactToSpacePirateTouch(EnemyTouchContext context)
    {
        if (context.HitboxTouch == EnemyReaction.OrdinaryTouch)
        {
            _collision.ResolveOrdinaryTouch(context);
            return;
        }
        // While the Pirate is frozen, touching it deals neither Samus nor the Pirate contact damage.
        if (context.Place.Freeze.IsRunning)
            return;
        _collision.ResolveOrdinaryTouch(context);
    }
}
