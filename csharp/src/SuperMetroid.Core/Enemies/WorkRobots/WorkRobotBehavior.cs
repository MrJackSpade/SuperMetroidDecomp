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

    /// <summary>
    /// A powered, a powerless or an unpowered work robot (#2146, #2179). The powered robot's definition has power only
    /// once Phantoon is defeated, which no update within the room changes, so its power is decided once, here.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place holds no work robot definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.WorkRobot)
            return IsAreaBossDefeated
                ? new PoweredWorkRobotEntity(this, place, common)
                : new PowerlessWorkRobotEntity(this, place, common);
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

    /// <summary>Updates left before the powered robots' glow steps; zero while the glow is stopped.</summary>
    internal int GlowStepCountdown { get; private set; }
    /// <summary>
    /// The step of the glow every powered robot shows, 0 through <see cref="WorkRobotGlowDefinitions.StepCount"/> minus
    /// one; after the last step the cycle starts again at the first.
    /// </summary>
    internal int GlowStep { get; private set; } = WorkRobotGlowDefinitions.StepCount - 1;
    /// <summary>The glow every powered robot in the room shows now (#1512).</summary>
    internal PictureShading Glow => PictureShading.Shaded(WorkRobotShades.Glow(GlowStep));

    /// <summary>Initializes a powered robot.</summary>
    internal void InitializePoweredWorkRobot(RoomEnemyPlace place)
    {
        var state = new WorkRobotEnemyState();
        place.SetSpeciesState(state);

        // A powered robot is solid to Samus and animates; its placements already process it off screen in every retail
        // room.
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.RestartAfter(WorkRobotMotionDefinitions.FirstStepDelay);
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

    /// <summary>
    /// Initializes an unpowered robot: the unpowered robot's definition, or the powered robot's while the ship has no power.
    /// </summary>
    /// <returns>The robot's state, holding the pose its placement names.</returns>
    internal UnpoweredWorkRobotEnemyState InitializeUnpoweredWorkRobot(RoomEnemyPlace place)
    {
        // The placement names the unpowered pose the robot holds.
        var state = new UnpoweredWorkRobotEnemyState(
            new(WorkRobotAnimations.StartingWith(place.SpawnSettings<WorkRobotSpawnSettings>().Pose), AnimationPlace.StartOfAnimation));
        place.SetSpeciesState(state);
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();

        // An unpowered robot stops the room's glow, an earlier powered robot's too: in a room placing both, the order
        // they are placed in decides whether the glow runs.
        GlowStepCountdown = 0;
        state.FallVelocity = WorkRobotMotionDefinitions.FallStartSpeed;
        return state;
    }

    /// <summary>
    /// Runs the robot's gravity-only main behaviour: it falls by <paramref name="fallVelocity"/> unless the floor holds it.
    /// </summary>
    /// <returns>The falling speed for the next update: unchanged on the floor, or with a further update's gravity.</returns>
    private SignedSixteenSixteen RunWorkRobotFall(
        RoomEnemyPlace place,
        SignedSixteenSixteen fallVelocity,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, place, fallVelocity))
            return fallVelocity;

        // The animation's own update counts this frame down later in the update, so extending it by one holds the
        // robot's current frame while it falls.
        place.AnimationClock.Extend(1);
        return fallVelocity + WorkRobotMotionDefinitions.Gravity;
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
                    _audio.QueueEnemySound(EnemySoundEffectDefinitions.WorkRobotFootstepSound, priority: SoundRequestPriority.Normal);
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
            state.LaserCooldown = state.LaserCooldown.AddFrames(WorkRobotLaserDefinitions.TurnDelay);
            return step.WallAnimation;
        }

        if (EnemyCollision.IsSamusStandingOn(place, samus))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromWholePixels(step.HorizontalPixels));
        }

        if (step.Ledge is not { } ledge)
            return null;

        // The game moves only whole X/Y pixels for this temporary foot probe. In particular,
        // the downward move's collision-produced Y subposition survives after Y is restored.
        Position16<RoomPixels> walkingPosition = place.Position.Whole;
        place.Position = place.Position.WithWrappedWholeX(
            place.Position.X.Whole + (ledge.ProbeSide == HorizontalDirection.Left ? -WorkRobotMotionDefinitions.LedgeProbeRadii : WorkRobotMotionDefinitions.LedgeProbeRadii) * place.Radius.X);
        bool floorFound = _terrain.MoveEnemyVertically(level, place, WorkRobotMotionDefinitions.LedgeProbeDepth);
        place.Position = place.Position.WithWhole(walkingPosition);
        if (floorFound)
            return null;

        state.LaserCooldown = state.LaserCooldown.AddFrames(WorkRobotLaserDefinitions.TurnDelay);
        state.Facing = ledge.FallFacing;
        return ledge.FallAnimation;
    }

    /// <summary>Checks the robot origin against the screen rectangle with exclusive top and left edges.</summary>
    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemyPlace robot,
        Position16<RoomPixels> camera) =>
        robot.Position.X.Whole > camera.X &&
        robot.Position.X.Whole <= camera.X + ScreenGeometry.ScreenWidthPixels &&
        robot.Position.Y.Whole > camera.Y &&
        robot.Position.Y.Whole <= camera.Y + ScreenGeometry.ScreenHeightPixels;

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
            robot.Position.X.Whole < samus.Position.X.Whole ? WorkRobotMotionDefinitions.TouchPush : -WorkRobotMotionDefinitions.TouchPush, 0));
    }

    /// <summary>
    /// Runs the private tail of a powered robot's shot AI after common shot handling. The
    /// indestructible vulnerability table leaves health unchanged, but the accepted impact
    /// still chooses an ahead/behind recoil animation and delays the next laser by 64 frames.
    /// </summary>
    private static void ResolveWorkRobotShotAfterCommon(RoomEnemyPlace robot, SamusState samus)
    {
        if (robot.Health == 0)
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
        state.LaserCooldown = state.LaserCooldown.AddFrames(WorkRobotLaserDefinitions.RecoilDelay);
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
            if (robot.SpeciesState<WorkRobotEnemyState>() is not null)
                robot.Shading = glow;
        }
        GlowStepCountdown = WorkRobotGlowDefinitions.Durations[GlowStep];
    }

    /// <summary>Applies a powered robot's recoil after the bomb damage it takes, which it never rejects once powered.</summary>
    internal EnemyBombReaction PoweredWorkRobotBombReaction { get; } = new()
    {
        AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
    };

    /// <summary>
    /// Rejects a bomb's damage on the powered robot's definition while the ship has no power, and recoils nothing: its own
    /// shot reaction returns until Phantoon is dead, and the bomb selects it where the definition leaves shots to it. The
    /// bomb pass has already marked the bomb, so the rejection belongs here rather than in collision admission.
    /// </summary>
    internal EnemyBombReaction PowerlessWorkRobotBombReaction { get; } = new()
    {
        Rejects = c => c.SelectedShot is null,
    };

    /// <summary>An unpowered robot takes the bomb damage, which it never rejects, and nothing more: it has no recoil.</summary>
    internal EnemyBombReaction UnpoweredWorkRobotBombReaction { get; } = new();

    /// <summary>A robot without power runs nothing after the common shot AI: it has no recoil.</summary>
    internal EnemyShotReaction UnpoweredWorkRobotShotReaction { get; } = new();

    /// <summary>A Work Robot laser, whichever way it flies (#1760): it flies with room collision.</summary>
    internal static readonly EnemyProjectileBehavior Laser = new SingleMotionProjectileBehavior(
        context => FlyWorkRobotLaser(context.Projectile, context.Level));

    /// <summary>Runs a powered robot's main AI.</summary>
    internal void RunWorkRobotMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        WorkRobotEnemyState state = place.RequireSpeciesState<WorkRobotEnemyState>();
        state.FallVelocity = RunWorkRobotFall(place, state.FallVelocity, frame.Level);
    }

    /// <summary>Runs the main AI of the powered robot's definition while the ship has no power.</summary>
    internal void RunPowerlessWorkRobotMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        UnpoweredWorkRobotEnemyState state = place.RequireSpeciesState<UnpoweredWorkRobotEnemyState>();
        state.FallVelocity = RunWorkRobotFall(place, state.FallVelocity, frame.Level);
    }

    /// <summary>Advances an unpowered robot's pose, which holds and raises no cue.</summary>
    internal static void AdvanceUnpoweredWorkRobotTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, WorkRobotAnimations.Segments,
            static (UnpoweredWorkRobotEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"An unpowered Work Robot's pose raised {cue}."));

    /// <summary>Advances a powered robot's typed animation.</summary>
    internal void AdvanceWorkRobotTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, WorkRobotAnimations.Segments,
            static (WorkRobotEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWorkRobotCue(place, state, context, cue) is { } continuesAt
                ? new AnimationContinuation<WorkRobotSegment>(WorkRobotAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Applies the powered robot's accepted-impact recoil tail after common projectile shots.</summary>
    internal EnemyShotReaction WorkRobotShotReaction { get; } = new()
    {
        AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
    };

    /// <summary>Routes robot contact to its four-pixel external push without Samus damage.</summary>
    internal static void ReactToWorkRobotTouch(EnemyTouchContext context) =>
        ResolveWorkRobotTouch(context.Place, context.Samus);

    /// <summary>
    /// Allocates a Work Robot laser: it flies ahead of the robot, the way it faces, and up, straight or down as
    /// <paramref name="definition"/> names, its velocity holding its way (#2278).
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

        // The enemy-projectile spawn copies the complete definition before the family's own setup: the diagonal and
        // horizontal lasers do not share radii or damage, and the definition also supplies the blank first frame and all three contact
        // properties. The laser's pictures carry their own
        // colours, so it draws from the room's shared sheet in its standard colours: no picture of it shows before its
        // first pass, so the robot's graphics its spawn briefly held never draw.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            definition.ProjectileKind(),
            Laser,
            SpriteGraphics.SharedStandard);
        projectile.StartAnimation(new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial));
        projectile.Velocity = projectile.Velocity.WithX(state.Facing == HorizontalDirection.Left ? -WorkRobotLaserDefinitions.Speed : WorkRobotLaserDefinitions.Speed);
        projectile.Velocity = projectile.Velocity.WithY(definition switch
        {
            WorkRobotLaser.Upward => -WorkRobotLaserDefinitions.DiagonalVerticalSpeed,
            WorkRobotLaser.Downward => WorkRobotLaserDefinitions.DiagonalVerticalSpeed,
            WorkRobotLaser.Horizontal => SignedEightEight.Zero,
            _ => throw new InvalidOperationException(
                $"Undefined Work Robot laser definition {definition}."),
        });

        // Every laser starts at the muzzle ahead of the robot, the way it faces, and above it.
        int ahead = WorkRobotLaserDefinitions.MuzzleAhead;
        projectile.Position = new(new(CoordinateAxis.Wrap(robot.Position.X.Whole +
            (state.Facing == HorizontalDirection.Left ? -ahead : ahead)), 0),
            new(CoordinateAxis.Wrap(robot.Position.Y.Whole - WorkRobotLaserDefinitions.MuzzleAbove), 0));

        if (WorkRobotLaserSoundRange.RequestsSound(robot.Position.Whole, robot.Radius, camera))
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.WorkRobotLaserSound, priority: SoundRequestPriority.Normal);
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
