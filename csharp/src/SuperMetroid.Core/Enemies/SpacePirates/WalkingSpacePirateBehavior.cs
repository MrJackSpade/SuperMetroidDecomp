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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateRedWalking, EnemyDefinitionId.PirateSilverWalking];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new WalkingSpacePirateEntity(this, place, common);

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
    private WalkingSpacePirateSegment? ApplyWalkingSpacePirateCue(RoomEnemyPlace place, WalkingSpacePirateCue cue, SamusState samus)
    {
        switch (cue)
        {
            case WalkingSpacePirateCue.FireLaser fireLaser:
                SpawnWalkingSpacePirateLaser(place, direction: fireLaser.Direction, fireLaser.LaserHeight);
                return null;
            case WalkingSpacePirateCue.Signal { Kind: WalkingSpacePirateCueKind.ChooseMovement }:
                return SelectWalkingSpacePirateMovement(place, samus);
            case WalkingSpacePirateCue.Begin begin:
                // The pirate animation does not jump to the stand or walk it begins: main AI runs it from the next
                // frame, as the animation runs on.
                RequireWalkingSpacePirateState(place).Action = begin.Action;
                return null;
            case WalkingSpacePirateCue.BeginHeading begin:
                // As a plain begin, main AI runs the walk from the next frame, the cue's way (#2165).
                WalkingSpacePirateEnemyState walker = RequireWalkingSpacePirateState(place);
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
    internal static void InitializeWalkingSpacePirate(RoomEnemyPlace place)
    {
        var state = new WalkingSpacePirateEnemyState(WalkingSpacePirateAction.StandingStill);
        place.SetSpeciesState(state);

        // The facing option alone selects the initial facing. The slow-laser option is an
        // independent projectile-flinch option and must not accidentally select right art.
        WalkingSpacePirateSpawnSettings settings = place.SpawnSettings<WalkingSpacePirateSpawnSettings>();
        WalkingSpacePirateOptions options = settings.Options;
        state.Playback = new(WalkingSpacePirateAnimations.StartingWith(
            (options & WalkingSpacePirateOptions.StartsFacingRight) != WalkingSpacePirateOptions.None
            ? WalkingSpacePirateSegment.WalkingRight
            : WalkingSpacePirateSegment.WalkingLeft), AnimationPlace.StartOfAnimation);

        // The posts wrap across the room's coordinate seam, as the game's additions do, so a modded patrol
        // reach can straddle it; the patrol compares positions by their signed difference, which still
        // sees each post on its own side of the pirate.
        state.RightPostX = CoordinateAxis.Wrap(place.Position.X.Whole + settings.PatrolReach);
        state.LeftPostX = CoordinateAxis.Wrap(place.Position.X.Whole - settings.PatrolReach);
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    private void RunWalkingSpacePirateMain(
        RoomEnemyPlace place,
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

        // The slow-laser option enables the retail flinch detector after normal AI.
        // The detector is intentionally allowed to replace an animation selected just above.
        if (((place.SpawnSettings<WalkingSpacePirateSpawnSettings>().Options &
                WalkingSpacePirateOptions.SlowLaserAndProjectileFlinch) != WalkingSpacePirateOptions.None))
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

        // Left and right are not mirrors: walking left probes for floor 17 pixels ahead and moves 14337/65536 of a pixel;
        // walking right probes 16 pixels ahead and moves 14336/65536.
        int originalX = place.Position.X.Whole;
        place.Position = place.Position.WithWrappedWholeX(
            place.Position.X.Whole - SpacePirateMotionDefinitions.WalkingLeftLedgeProbe);
        bool floorAhead = _terrain.MoveEnemyVertically(level, place, SpacePirateMotionDefinitions.WalkingGroundProbeStep);
        place.Position = place.Position.WithWholeX(originalX);

        if (floorAhead)
        {
            // The ignored horizontal probe has one mutable result, a displacement pair the following move
            // immediately overwrites, so it leaves no persistent actor-side effect.
            bool hitWall = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                displacement: SpacePirateMotionDefinitions.WalkingLeftStep);
            if (!hitWall &&
                CoordinateAxis.Difference(place.Position.X.Whole, state.LeftPostX) >= 0)
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
        place.Position = place.Position.WithWrappedWholeX(
            place.Position.X.Whole + SpacePirateMotionDefinitions.WalkingRightLedgeProbe);
        bool floorAhead = _terrain.MoveEnemyVertically(level, place, SpacePirateMotionDefinitions.WalkingGroundProbeStep);
        place.Position = place.Position.WithWholeX(originalX);

        if (floorAhead &&
            !_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                displacement: SpacePirateMotionDefinitions.WalkingRightStep) &&
            CoordinateAxis.Precedes(place.Position.X.Whole, state.RightPostX))
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
        SamusProjectile? shot = PirateFlinchFromHighestShot.Shot(projectiles.Shots, projectile => !projectile.Type.IsNone);
        if (shot is null ||
            CoordinateAxis.Distance(shot.Position.X.Whole, place.Position.X.Whole) >=
                SpacePirateMotionDefinitions.WalkingFlinchReach ||
            CoordinateAxis.Distance(shot.Position.Y.Whole, place.Position.Y.Whole) >=
                SpacePirateMotionDefinitions.WalkingFlinchReach)
        {
            return;
        }

        WalkingSpacePirateSegment flinch = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0
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
            CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) >= 0;
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

    /// <summary>Tests signed vertical separation against the sixteen-pixel firing band.</summary>
    private static bool WalkingSpacePirateIsVerticallyClose(
        RoomEnemyPlace place,
        SamusState samus) =>
        CoordinateAxis.Distance(samus.Position.Y.Whole, place.Position.Y.Whole) <
            SpacePirateMotionDefinitions.WalkingFiringBand;

    /// <summary>Chooses a left or right laser animation from Samus's current side.</summary>
    private static void InstallWalkingSpacePirateAttack(
        RoomEnemyPlace place,
        WalkingSpacePirateEnemyState state,
        SamusState samus) =>
        StartWalkingSpacePirateAnimation(
            place,
            state,
            CoordinateAxis.Difference(samus.Position.X.Whole, place.Position.X.Whole) >= 0
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

    /// <summary>Runs the firing cues and the laser initializer.</summary>
    private void SpawnWalkingSpacePirateLaser(
        RoomEnemyPlace source,
        HorizontalDirection direction,
        int laserHeight)
    {
        _ = SpawnSpacePirateLaser(source, direction, laserHeight);
    }

    /// <summary>
    /// Runs the common laser initializer. Wall and walking Pirates
    /// use different actor animation steps and muzzle offsets, but the allocated projectile is the
    /// same retail definition and therefore belongs in one shared implementation.
    /// </summary>
    internal bool SpawnSpacePirateLaser(
        RoomEnemyPlace source,
        HorizontalDirection direction,
        int laserHeight)
    {
        EnemyProjectile? projectile = SpawnPirateMotherBrainLaser(
            source,
            new(
                CoordinateAxis.Wrap(
                    source.Position.X.Whole +
                    (direction == HorizontalDirection.Right ? SpacePirateMotionDefinitions.WalkingLaserMuzzleAhead : -SpacePirateMotionDefinitions.WalkingLaserMuzzleAhead)),
                CoordinateAxis.Wrap(source.Position.Y.Whole - laserHeight)),
            direction,
            LaserSpeedOf(source));
        if (projectile is null)
            return false;

        RequestSpacePirateSound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound);
        return true;
    }

    /// <summary>
    /// The speed of a pirate's laser: the initializer reads the firing pirate's placement, where walking and wall
    /// pirates name the slow laser.
    /// </summary>
    private static PirateLaserSpeed LaserSpeedOf(RoomEnemyPlace pirate)
    {
        bool slow = pirate.Spawn.Population.SpawnSettings switch
        {
            WalkingSpacePirateSpawnSettings walking =>
                (walking.Options & WalkingSpacePirateOptions.SlowLaserAndProjectileFlinch) != WalkingSpacePirateOptions.None,
            WallSpacePirateSpawnSettings wall =>
                (wall.Options & WallSpacePirateOptions.SlowJumpAndLaser) != WallSpacePirateOptions.None,
            _ => throw new InvalidOperationException($"{pirate.DefinitionId} is not a pirate that fires lasers."),
        };
        return slow ? PirateLaserSpeed.Slow : PirateLaserSpeed.Fast;
    }

    /// <summary>
    /// Runs the common laser initializer once the firing enemy has chosen
    /// its muzzle coordinate. Walking Pirates, wall Pirates, and Mother Brain enter through
    /// different actor animation steps, but the game deliberately funnels all of them through
    /// this definition so animation, speed flags, collision damage, and disposal agree.
    /// </summary>
    /// <param name="source">The enemy firing the laser, whose damage the laser takes.</param>
    /// <param name="muzzle">Where the laser starts.</param>
    /// <param name="direction">The way the laser travels.</param>
    /// <param name="speed">The laser's speed, which the firing enemy's placement names.</param>
    internal EnemyProjectile? SpawnPirateMotherBrainLaser(
        RoomEnemyPlace source,
        Position16<RoomPixels> muzzle,
        HorizontalDirection direction,
        PirateLaserSpeed speed)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.PirateMotherBrainLaser,
            Laser,
            graphics: default);

        projectile.Position = FixedPosition<RoomPixels>.FromWhole(muzzle);
        projectile.StartAnimation(new SpacePirateProjectileAnimation(direction == HorizontalDirection.Right
            ? SpacePirateProjectileSegment.LaserRight
            : SpacePirateProjectileSegment.LaserLeft));

        // The laser holds still through its three two-frame muzzle frames until its animation fires it, running its flight
        // at once.
        projectile.State = new PirateLaserState(speed);

        // The initializer replaces the projectile definition's properties with the enemy's damage and the
        // laser's own property. The laser does not take the Pirate's pictures or colours: it draws from the room's
        // shared sheet in its standard colours.
        projectile.Damage = source.Definition.Damage;
        projectile.CanDamageSamus = true;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return projectile;
    }

    /// <summary>Flies a laser one step in <paramref name="direction"/>, deleting it once it leaves the 256x256 camera square.</summary>
    private static void FlyPirateLaser(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        int pixels = projectile.StateOf<PirateLaserState>().Speed == PirateLaserSpeed.Slow
            ? SpacePirateMotionDefinitions.WalkingSlowLaserStep
            : SpacePirateMotionDefinitions.WalkingFastLaserStep;
        projectile.Position = projectile.Position.WithWholeX(direction == HorizontalDirection.Right
            ? CoordinateAxis.Wrap(projectile.Position.X.Whole + pixels)
            : CoordinateAxis.Wrap(projectile.Position.X.Whole - pixels));

        if (!EnemyView.ContainsBeforeFarEdges(projectile.Position.Whole, camera))
            projectile.Delete();
    }

    /// <summary>Requires the enemy to have been initialized as a walking pirate.</summary>
    internal static WalkingSpacePirateEnemyState RequireWalkingSpacePirateState(
        RoomEnemyPlace place) =>
        place.SpeciesState<WalkingSpacePirateEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy place {place.Ordinal} has no initialized walking Space Pirate state.");

    /// <summary>
    /// The Pirates' and Mother Brain's laser (#1760): its spawner starts its animation, which installs its flight left or
    /// right.
    /// </summary>
    internal static readonly EnemyProjectileBehavior Laser = new LaserBehavior();

    /// <summary>A laser: it flies left or right until it leaves the camera, once its animation fires it.</summary>
    private sealed class LaserBehavior : EnemyProjectileMotionBehavior<PirateLaserMotion>, IEnemyProjectileCues<SpacePirateProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, PirateLaserMotion motion)
        {
            switch (motion)
            {
                case PirateLaserMotion.FlyingLeft:
                    FlyPirateLaser(context.Projectile, context.Camera, HorizontalDirection.Left);
                    break;
                case PirateLaserMotion.FlyingRight:
                    FlyPirateLaser(context.Projectile, context.Camera, HorizontalDirection.Right);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PirateLaserMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, SpacePirateProjectileCue cue)
        {
            if (cue.Kind != SpacePirateProjectileCueKind.FireLaser)
                throw new InvalidOperationException($"A laser's animation published {cue}.");
            Fire(context.Projectile, context.Camera, cue.Direction);
        }

        /// <summary>
        /// Fires the laser <paramref name="direction"/>: it flies that way from now on, and its first flight runs at once,
        /// so the laser moves on the update it fires and may leave the screen then.
        /// </summary>
        private static void Fire(EnemyProjectile projectile, Position16<RoomPixels> camera, HorizontalDirection direction)
        {
            projectile.StartMotion(direction == HorizontalDirection.Right ? PirateLaserMotion.FlyingRight : PirateLaserMotion.FlyingLeft);
            FlyPirateLaser(projectile, camera, direction);
        }
    }

    /// <summary>Runs the walking Space Pirate's main AI.</summary>
    internal void RunWalkingSpacePirateMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWalkingSpacePirateMain(
            place,
            RequireWalkingSpacePirateState(place),
            frame.Samus,
            frame.Level,
            frame.SamusProjectiles);
    }

    /// <summary>Advances the walking Space Pirate's typed animation.</summary>
    internal void AdvanceWalkingSpacePirateTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = RequireWalkingSpacePirateState(place);
        AnimationPlayback<WalkingSpacePirateSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, WalkingSpacePirateAnimations.Segments, ref playback,
            cue => ApplyWalkingSpacePirateCue(place, cue, context.Samus) is { } continuesAt
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
            // rejected and reflected hits return before that shared path. The plasma-blocking enemy property also
            // forces the mark.
            c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Ignore)
        {
            // The Ninja body reaction deliberately returns for a just-fired Super Missile that has no link yet.
            // The frame-hitbox collision walk nevertheless exits after running that reaction.
            return true;
        }
        if (pirateAction == PirateHitboxShotActions.PirateHitboxShotAction.Reflect)
        {
            c.Enemy.InvincibilityTimer = SpacePirateCollisionDefinitions.ReflectInvincibility;
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
            c.Projectile.IsSpent = false;
            c.Projectiles.EnemyImpacts.ReflectFromEnemy(c.Samus,
                c.Projectile);
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
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
            c.UsesFrameHitboxes && ResolveSpacePirateHitboxShot(c, PirateHitboxShotActions.SelectPirateHitboxShotAction(c.HitboxShot)),
        ShowsHitBurst = c => PirateHitboxShotActions.ShowsHitBurst(c.HitboxShot),
        ReplacesGenericDeath = c =>
        {
            if (c.HitboxShot == EnemyReaction.NormalEnemyShot)
                return false;
            StartPirateDeath(c);
            return true;
        },
    };

    /// <summary>The gold Ninja's shot reaction: its armour's reflections, the Pirate shot logic and the drops its death scatters.</summary>
    internal EnemyShotReaction GoldNinjaShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
            c.UsesFrameHitboxes && ResolveSpacePirateHitboxShot(c, PirateHitboxShotActions.SelectGoldNinjaHitboxShotAction(c.Enemy, c.Projectile, c.HitboxShot)),
        ShowsHitBurst = c => PirateHitboxShotActions.GoldNinjaShowsHitBurst(c.HitboxShot),
        ReplacesGenericDeath = c =>
        {
            if (c.HitboxShot == EnemyReaction.NormalEnemyShot)
                return false;
            // The Gold Ninja's death records its position as the drop origin before the enemy's death clears the
            // place; it then spawns the death explosion before the metal ninja Pirate's item drop spawns the drops.
            Position16<RoomPixels> dropOrigin = c.Enemy.Position.Whole;
            StartPirateDeath(c);
            _deathsAndDrops.SpawnEnemyDropScatterAround(EnemyDefinitionId.PirateGoldNinja, count: 5, dropOrigin, c.Samus);
            return true;
        },
    };

    /// <summary>Starts a Pirate's death by a shot: the big explosion its shot's death animation rule chooses.</summary>
    private void StartPirateDeath(EnemyShotContext c) =>
        _deathsAndDrops.StartGenericEnemyDeath(
            c.Enemy,
            EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(c.Enemy, c.ProjectileType, rule: ShotDeathAnimationRule.PirateBigExplosion),
            c.Samus);

    /// <summary>Skips frozen contact and rejects unsupported extended hitbox reactions before normal touch damage.</summary>
    internal void ReactToSpacePirateTouch(EnemyTouchContext context)
    {
        // Every Space Pirate frame's hitboxes use the shared Pirate touch
        // reaction in every retail hitbox. Keep the reaction check explicit: silently
        // treating a later family-specific attack box as ordinary body contact would
        // recreate the exact radius-flattening bug this path is intended to remove.
        if (context.UsesFrameHitboxes && context.HitboxTouch != SpacePirateReactions.Touch)
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }
        // The shared Pirate touch returns before common touch AI while frozen. The overlap was
        // still found, but neither Samus nor the Pirate receives contact damage.
        if (context.Place.FrozenTimer != 0)
            return;
        _collision.ResolveNormalEnemyTouch(context);
    }
}
