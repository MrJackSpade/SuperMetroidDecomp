using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The work robot species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WorkRobotBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.WorkRobot, EnemyDefinitionId.UnpoweredWorkRobot];

    /// <summary>A powered or an unpowered work robot (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no work robot definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.WorkRobot)
            return new PoweredWorkRobotEntity(this, place, common);
        if (definition == EnemyDefinitionId.UnpoweredWorkRobot)
            return new UnpoweredWorkRobotEntity(this, place, common);
        throw new InvalidOperationException($"The work robot species creates no {place.DefinitionId}.");
    }

    /// <summary>Checks Phantoon's defeat flag and samples the room random number for laser timing.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves the robot along floors and probes its next foothold.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates lasers from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, whose powered robots all show the shared glow.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Whether Phantoon is defeated, which powers the work robots.</summary>
    internal bool IsAreaBossDefeated => _room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss);

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds robot locomotion, power state, lasers, and the glow to room services.</summary>
    internal WorkRobotBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _places = places;
    }

    /// <summary>How fast a robot's laser flies the way the robot faces: two pixels per update.</summary>
    private static readonly SignedEightEight WorkRobotLaserSpeed = SignedEightEight.FromPixels(2);
    /// <summary>What gravity adds to a falling unpowered robot's speed each airborne update: half a pixel.</summary>
    private static readonly SignedSixteenSixteen WorkRobotGravity = SignedSixteenSixteen.FromParts(0, 32768);
    /// <summary>The speed an unpowered robot starts falling at: one pixel per update.</summary>
    private static readonly SignedSixteenSixteen WorkRobotInitialFallVelocity = SignedSixteenSixteen.FromParts(1, 0);

    /// <summary>Enemy-library sound queued by a robot footstep.</summary>
    private static readonly SoundEffectId WorkRobotFootstepSound =
        EnemySoundEffectDefinitions.WorkRobotFootstepSound;
    /// <summary>Enemy-library sound queued by a robot laser.</summary>
    private static readonly SoundEffectId WorkRobotLaserSound =
        EnemySoundEffectDefinitions.WorkRobotLaserSound;

    /// <summary>Updates left before the powered robots' glow steps; zero while the glow is stopped.</summary>
    internal int GlowStepCountdown { get; private set; }
    /// <summary>
    /// The step of the glow every powered robot shows, 0 through <see cref="WorkRobotGlowDefinitions.StepCount"/> minus
    /// one; after the last step the cycle starts again at the first.
    /// </summary>
    internal int GlowStep { get; private set; } = WorkRobotGlowDefinitions.StepCount - 1;
    /// <summary>The glow every powered robot in the room shows now (#1512).</summary>
    internal PictureShading Glow => PictureShading.Shaded(WorkRobotShades.Glow(GlowStep));

    /// <summary>Initializes the Robot.</summary>
    internal void InitializeWorkRobot(RoomEnemyPlace place)
    {
        var state = new WorkRobotEnemyState();
        place.SetSpeciesState(state);

        bool areaBossDefeated = _room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss);
        if (place.DefinitionId != EnemyDefinitionId.WorkRobot || !areaBossDefeated)
        {
            InitializeWorkUnpoweredWorkRobot(place, state);
            return;
        }

        state.Powered = true;

        // A powered robot is solid to Samus and animates; its placements already process it off screen in every retail
        // room.
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.RestartAfter(4);
        state.Playback = new(WorkRobotAnimations.StartingWith(WorkRobotSegment.Initial), AnimationPlace.StartOfAnimation);
        state.Facing = HorizontalDirection.Left;
        state.LaserCooldown = WrappingCountdown.Start(0);

        // The room has one glow, not one per robot: every powered robot's initialization restarts it. Every powered
        // robot, and only a powered robot, shows it (#1512).
        RestartGlow();
        place.Shading = Glow;
    }

    /// <summary>
    /// Restarts the powered robots' glow so that it steps to its first step on the next update: it rests on its last
    /// step until then.
    /// </summary>
    internal void RestartGlow()
    {
        GlowStepCountdown = 1;
        GlowStep = WorkRobotGlowDefinitions.StepCount - 1;
    }

    /// <summary>Initializes the unpowered Work Robot.</summary>
    internal void InitializeWorkUnpoweredWorkRobot(RoomEnemyPlace place, WorkRobotEnemyState state)
    {
        state.Powered = false;

        // The placement names the unpowered pose the robot holds.
        state.Playback = new(WorkRobotAnimations.StartingWith(place.SpawnSettings<WorkRobotSpawnSettings>().Pose), AnimationPlace.StartOfAnimation);
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();

        // An unpowered robot stops the room's glow, an earlier powered robot's too: in a room placing both, the order
        // they are placed in decides whether the glow runs.
        GlowStepCountdown = 0;
        state.FallVelocity = WorkRobotInitialFallVelocity;
    }

    /// <summary>Runs the robot's gravity-only main behaviour.</summary>
    private void RunWorkRobotMain(
        RoomEnemyPlace place,
        WorkRobotEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, place, state.FallVelocity))
            return;

        // Enemy animation processing will decrement the timer later in this same frame.
        // Incrementing here cancels that tick, freezing the current art while airborne.
        place.AnimationClock.Extend(1);
        // The fraction carries into the whole pixels, which wrap, as the game's paired additions do.
        state.FallVelocity += WorkRobotGravity;
    }

    /// <summary>
    /// Carries out one cue of the Work Robot's animations. Steps that meet a wall or ledge and a laser that fires continue
    /// the animation elsewhere, as the cues replace the segment.
    /// </summary>
    private WorkRobotSegment? ApplyWorkRobotCue(RoomEnemyPlace place, WorkRobotEnemyState state, EnemyAnimationCueContext context, WorkRobotCue cue)
    {
        SamusState samus = context.Samus;
        RoomLevelData level = context.Level;
        Position16<RoomPixels> camera = context.Camera;
        if (cue is WorkRobotCue.Step step)
            return MoveWorkRobot(place, state, samus, level, step.Move);
        if (cue is WorkRobotCue.TryLaser laser)
            return TryFireWorkRobotLaser(place, state, laser.Laser, laser.Firing, camera);
        WorkRobotCueKind kind = ((WorkRobotCue.Signal)cue).Kind;
        switch (kind)
        {
            case WorkRobotCueKind.Footstep:
                if (IsWorkRobotOriginStrictlyOnScreen(place, camera))
                    _audio.QueueEnemySound(WorkRobotFootstepSound, priority: SoundRequestPriority.Normal);
                return null;
            case WorkRobotCueKind.CountDownLaserCooldown:
                DecrementWorkRobotLaserCooldown(state);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(WorkRobotCueKind)} {kind}.");
        }
    }

    /// <summary>Applies one walk step, carries Samus on contact, and branches to wall or fall animations when support ends.</summary>
    private WorkRobotSegment? MoveWorkRobot(RoomEnemyPlace place, WorkRobotEnemyState state, SamusState samus, RoomLevelData level, WorkRobotMoveStep step)
    {
        DecrementWorkRobotLaserCooldown(state);
        state.Facing = step.Facing;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                SignedSixteenSixteen.FromWholePixels(step.HorizontalPixels)))
        {
            state.LaserCooldown = state.LaserCooldown.AddFrames(8);
            return step.WallAnimation;
        }

        if (EnemyCollision.IsEnemyTouchingSamusFromBelow(place, samus))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(step.HorizontalPixels), 0));
        }

        if (!step.CheckLedge)
            return null;

        // The game moves only whole X/Y pixels for this temporary foot probe. In particular,
        // the downward move's collision-produced Y subposition survives after Y is restored.
        Position16<RoomPixels> walkingPosition = place.Position.Whole;
        place.Position = place.Position.WithWrappedWholeX(
            place.Position.X.Whole + step.LedgeProbeDirection * place.Radius.X * 2);
        bool floorFound = _terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(1));
        place.Position = place.Position.WithWhole(walkingPosition);
        if (floorFound)
            return null;

        state.LaserCooldown = state.LaserCooldown.AddFrames(8);
        state.Facing = step.FallFacing ?? throw new InvalidOperationException("A ledge-watching Work Robot step names no fall facing.");
        return step.FallAnimation ?? throw new InvalidOperationException("A ledge-watching Work Robot step names no fall animation.");
    }

    /// <summary>Checks the robot origin against the screen rectangle with exclusive top and left edges.</summary>
    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemyPlace robot,
        Position16<RoomPixels> camera) =>
        CoordinateAxis.Difference(robot.Position.X.Whole, camera.X) > 0 &&
        CoordinateAxis.AsSigned(camera.X + EnemyView.Extent - robot.Position.X.Whole) >= 0 &&
        CoordinateAxis.Difference(robot.Position.Y.Whole, camera.Y) > 0 &&
        CoordinateAxis.AsSigned(camera.Y + EnemyView.PictureHeight - robot.Position.Y.Whole) >= 0;

    /// <summary>Uses the existing random number to reset cooldown and starts the firing animation when ready.</summary>
    private WorkRobotSegment? TryFireWorkRobotLaser(
        RoomEnemyPlace robot,
        WorkRobotEnemyState state,
        WorkRobotLaser projectileDefinition,
        WorkRobotSegment firingSegment,
        Position16<RoomPixels> camera)
    {
        if (!state.LaserCooldown.IsZero)
        {
            DecrementWorkRobotLaserCooldown(state);
            return null;
        }

        state.LaserCooldown = WrappingCountdown.Start(WorkRobotLaserDefinitions.CooldownJitter.Of(_room.Random.Read()) + WorkRobotLaserDefinitions.CooldownBase);
        SpawnWorkRobotLaser(robot, state, projectileDefinition, camera);
        return firingSegment;
    }

    /// <summary>Counts a nonzero laser cooldown down without unsigned wraparound.</summary>
    private static void DecrementWorkRobotLaserCooldown(WorkRobotEnemyState state)
    {
        state.LaserCooldown = state.LaserCooldown.DecrementToZero();
    }

    /// <summary>
    /// Runs the robot's custom touch. Work Robots are moving solids: body contact publishes a
    /// four-pixel external push away from their center and deliberately deals no touch damage.
    /// </summary>
    private static void ResolveWorkRobotTouch(RoomEnemyPlace robot, SamusState samus)
    {
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromParts(
            robot.Position.X.Whole < samus.Position.X.Whole ? 4 : -4, 0));
    }

    /// <summary>
    /// Runs the private tail of powered shot AI after common shot handling. The
    /// indestructible vulnerability table leaves health unchanged, but the accepted impact
    /// still chooses an ahead/behind recoil animation and delays the next laser by 64 frames.
    /// </summary>
    private void ResolveWorkRobotShotAfterCommon(RoomEnemyPlace robot, SamusState samus)
    {
        if (robot.DefinitionId != EnemyDefinitionId.WorkRobot || robot.Health == 0)
            return;
        if (!_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
            return;

        WorkRobotEnemyState state = robot.RequireSpeciesState<WorkRobotEnemyState>();
        bool facingLeft = state.Facing == HorizontalDirection.Left;
        if (facingLeft)
        {
            state.Playback = new(WorkRobotAnimations.StartingWith(samus.Position.X.Whole < robot.Position.X.Whole
                ? WorkRobotSegment.FacingLeftShotSamusAhead
                : WorkRobotSegment.FacingLeftShotSamusBehind), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Playback = new(WorkRobotAnimations.StartingWith(samus.Position.X.Whole >= robot.Position.X.Whole
                ? WorkRobotSegment.FacingRightShotSamusAhead
                : WorkRobotSegment.FacingRightShotSamusBehind), AnimationPlace.StartOfAnimation);
        }
        robot.AnimationClock.StepOnNextUpdate();
        state.LaserCooldown = state.LaserCooldown.AddFrames(64);
    }

    /// <summary>
    /// Counts down the powered robots' glow after the enemies' update unless it is stopped; when the step's time is up
    /// the glow shows its next step on every powered robot and holds it for that step's duration.
    /// </summary>
    internal void StepGlow()
    {
        if (GlowStepCountdown == 0)
            return;
        if (--GlowStepCountdown != 0)
            return;

        GlowStep = (GlowStep + 1) % WorkRobotGlowDefinitions.StepCount;
        PictureShading glow = Glow;
        foreach (RoomEnemyPlace robot in _places.InUpdateOrder)
        {
            if (robot.SpeciesState<WorkRobotEnemyState>() is { Powered: true })
                robot.Shading = glow;
        }
        GlowStepCountdown = WorkRobotGlowDefinitions.Durations[GlowStep];
    }

    /// <summary>Rejects powered-robot bomb damage before Phantoon's defeat and applies recoil after admitted hits.</summary>
    internal EnemyBombReaction WorkRobotBombReaction => field ??= new()
    {
        // The powered Work Robot's own shot reaction returns until Phantoon is dead; the bomb selects it where the
        // powered robot's definition leaves shots to it. The bomb pass has already marked the bomb, so reaction
        // rejection belongs here rather than in collision admission.
        Rejects = c => c.Enemy.DefinitionId == EnemyDefinitionId.WorkRobot &&
                       c.SelectedShot is null &&
                       !_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss),
        AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
    };

    /// <summary>A Work Robot laser, in any of its five directions (#1760): it flies with room collision.</summary>
    internal static readonly EnemyProjectileBehavior Laser = new SingleMotionProjectileBehavior(
        context => FlyWorkRobotLaser(context.Projectile, context.Level));

    /// <summary>Runs WorkRobot's main AI.</summary>
    internal void RunWorkRobotMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWorkRobotMain(place, place.RequireSpeciesState<WorkRobotEnemyState>(), frame.Level);
    }

    /// <summary>Advances WorkRobot's typed animation.</summary>
    internal void AdvanceWorkRobotTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, WorkRobotAnimations.Segments,
            static (WorkRobotEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWorkRobotCue(place, state, context, cue) is { } continuesAt
                ? new AnimationContinuation<WorkRobotSegment>(WorkRobotAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Applies the powered robot's accepted-impact recoil tail after common projectile shots.</summary>
    internal EnemyShotReaction WorkRobotShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
    };

    /// <summary>Routes robot contact to its four-pixel external push without Samus damage.</summary>
    internal static void ReactToWorkRobotTouch(EnemyTouchContext context) =>
        ResolveWorkRobotTouch(context.Place, context.Samus);

    /// <summary>
    /// Allocates one of the five Work Robot laser definitions. Horizontal left/right
    /// share one definition; the sign in the owner's laser velocity supplies the sixth direction.
    /// </summary>
    internal void SpawnWorkRobotLaser(
        RoomEnemyPlace robot,
        WorkRobotEnemyState state,
        WorkRobotLaser definition,
        Position16<RoomPixels> camera)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // The enemy-projectile spawn installs the complete definition before the family's own setup. Keeping that
        // division here is important: the five lasers do not share radii or damage, and the common helper also
        // supplies the blank first frame and all three contact properties. The laser's pictures carry their own
        // colours, so it draws from the room's shared sheet in its standard colours: no picture of it shows before its
        // first pass, so the robot's graphics its spawn briefly held never draw.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            definition.ProjectileKind(),
            Laser,
            SpriteGraphics.SharedStandard);
        projectile.StartAnimation(new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial));
        projectile.Velocity = projectile.Velocity.WithX(state.Facing == HorizontalDirection.Left ? -WorkRobotLaserSpeed : WorkRobotLaserSpeed);
        projectile.Velocity = projectile.Velocity.WithY(definition switch
        {
            WorkRobotLaser.UpLeft or WorkRobotLaser.UpRight =>
                -SignedEightEight.FromParts(0, 128),
            WorkRobotLaser.DownLeft or WorkRobotLaser.DownRight => SignedEightEight.FromParts(0, 128),
            WorkRobotLaser.Horizontal => SignedEightEight.Zero,
            _ => throw new InvalidOperationException(
                $"Undefined Work Robot laser definition {definition}."),
        });

        // All five initializers place the muzzle ahead of the robot in the way it faces and above it.
        int ahead = WorkRobotLaserDefinitions.MuzzleAhead;
        projectile.Position = new(new(CoordinateAxis.Wrap(robot.Position.X.Whole +
            (state.Facing == HorizontalDirection.Left ? -ahead : ahead)), 0),
            new(CoordinateAxis.Wrap(robot.Position.Y.Whole - WorkRobotLaserDefinitions.MuzzleAbove), 0));

        // Preserve the laser viewport bug. Its final Y comparison omits
        // the camera's Y, so lasers only request sound while their owner's absolute
        // top edge is less than the picture's height from the room's top, even when a lower room row is on screen.
        bool horizontallyVisible =
            CoordinateAxis.AsSigned(robot.Position.X.Whole + robot.Radius.X - camera.X) >= 0 &&
            CoordinateAxis.Difference(robot.Position.X.Whole - robot.Radius.X - (EnemyView.Extent + 1), camera.X) < 0;
        bool verticallyVisible =
            CoordinateAxis.AsSigned(robot.Position.Y.Whole + robot.Radius.Y - camera.Y) >= 0 &&
            CoordinateAxis.Difference(robot.Position.Y.Whole - robot.Radius.Y, EnemyView.PictureHeight) < 0;
        if (horizontallyVisible && verticallyVisible)
            _audio.QueueEnemySound(WorkRobotLaserSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the laser's per-frame behaviour: X collision, then Y collision, else move.</summary>
    internal static void FlyWorkRobotLaser(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Delete();
        }
    }
}
