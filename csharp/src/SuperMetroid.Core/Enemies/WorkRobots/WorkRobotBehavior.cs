using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The work robot species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WorkRobotBehavior : IEnemySpecies, IEnemyEntityFactory, IRoomGlow
{
    /// <summary>
    /// A powered or an unpowered work robot (#2146, #2179). The rooms place the powered robot's definition only in their
    /// states once Phantoon is defeated, so it always has power (#2455).
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<WorkRobotSpawnSettings>([EnemyDefinitionId.WorkRobot], (occupant, _, place, common) =>
            EnemyOccupant.Running(new PoweredWorkRobotEntity(this, occupant, place, common)));
        registrar.Spawns<WorkRobotSpawnSettings>([EnemyDefinitionId.UnpoweredWorkRobot], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new UnpoweredWorkRobotEntity(this, occupant, place, common, settings)));
    }

    /// <summary>Samples the room random number for laser timing.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves the robot along floors and probes its next foothold.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates lasers from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>
    /// The places of the powered robots the room has set up and that still hold them, in the order they were placed; all
    /// show the shared glow (#2306).
    /// </summary>
    private readonly List<RoomEnemyPlace> _poweredRobots = [];

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds robot locomotion, lasers and the glow to room services.</summary>
    internal WorkRobotBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// The powered robots' glow (#2380): the place its step shows and the updates that step has left. A room starts it on
    /// its last step, stopped.
    /// </summary>
    private readonly ShowingAnimationPlayer<WorkRobotGlowSegment, WorkRobotGlowPlace, NoAnimationCue> glow =
        ShowingAnimationPlayer<WorkRobotGlowSegment, WorkRobotGlowPlace, NoAnimationCue>.StartShowing(
            WorkRobotGlowAnimations.Segments, WorkRobotGlowAnimations.Restarted);

    /// <summary>Whether the glow runs: a powered robot's setup starts it, and an unpowered robot's stops it.</summary>
    internal bool GlowRuns { get; private set; }

    /// <summary>
    /// The place of the glow every powered robot shows (#2393); after the last step the cycle starts again at the first.
    /// </summary>
    private WorkRobotGlowPlace GlowPlace => glow.Showing;

    /// <summary>The glow every powered robot in the room shows now (#1512).</summary>
    private PictureShading Glow => PictureShading.Shaded(WorkRobotShades.Glow(GlowPlace));

    /// <summary>Initializes a powered robot.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal WorkRobotEnemyState InitializePoweredWorkRobot(RoomEnemyPlace place)
    {
        var state = new WorkRobotEnemyState();
        _poweredRobots.Add(place);

        // A powered robot is solid to Samus and animates; its placements already process it off screen in every
        // room.
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.AnimationClock.RestartAfter(WorkRobotMotionDefinitions.FirstStepDelay);
        state.Playback = new(WorkRobotAnimations.StartingWith(WorkRobotSegment.StandingFacingLeft), AnimationPlace.StartOfAnimation);
        state.Facing = HorizontalDirection.Left;
        state.LaserCooldown = WrappingCountdown.Start(0);

        // The room has one glow, not one per robot: every powered robot's initialization restarts it. Every powered
        // robot, and only a powered robot, shows it (#1512).
        RestartGlow();
        place.Shading = Glow;
        return state;
    }

    /// <summary>
    /// Restarts the powered robots' glow so that it steps to its first step on the next update: it rests on its last
    /// step until then.
    /// </summary>
    private void RestartGlow()
    {
        glow.Show(WorkRobotGlowAnimations.Restarted);
        GlowRuns = true;
    }

    /// <summary>Initializes an unpowered robot.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal UnpoweredWorkRobotEnemyState InitializeUnpoweredWorkRobot(WorkRobotSpawnSettings workRobotSpawnSettings, RoomEnemyPlace place)
    {
        // The placement names the unpowered pose the robot holds.
        var state = new UnpoweredWorkRobotEnemyState(
            new(WorkRobotAnimations.StartingWith(workRobotSpawnSettings.Pose), AnimationPlace.StartOfAnimation));
        place.Contact = place.Contact.With(EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();

        // An unpowered robot stops the room's glow, an earlier powered robot's too: in a room placing both, the order
        // they are placed in decides whether the glow runs.
        GlowRuns = false;
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
    private WorkRobotSegment? ApplyWorkRobotCue(RoomEnemyPlace place, WorkRobotEnemyState state, EnemyAnimationCueContext context, WorkRobotCue cue) =>
        cue.Match(ApplyWorkRobotCueCases, new ApplyWorkRobotCueContext(place, state, context, this));

    /// <summary>What <see cref="ApplyWorkRobotCue"/> does on each <see cref="WorkRobotCue"/>, built once (#2553).</summary>
    private static WorkRobotCueCases<ApplyWorkRobotCueContext, WorkRobotSegment?> ApplyWorkRobotCueCases { get; } = new(
            Step: static (step, reads) => reads.Owner.MoveWorkRobot(reads.Place, reads.State, reads.Context.Samus, reads.Context.Level, step.Move),
            TryLaser: static (laser, reads) => reads.Owner.TryFireWorkRobotLaser(reads.Place, reads.State, laser.Laser, laser.Firing, reads.Context.Camera),
            Signal: static (signal, reads) =>
            {
                reads.Owner.WorkRobotSignals[signal.Kind](reads.Place, reads.State, reads.Context.Camera);
                return null;
            });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Context">The context the cases read.</param>
    /// <param name="Owner">The <see cref="WorkRobotBehavior"/> reacting.</param>
    private readonly record struct ApplyWorkRobotCueContext(RoomEnemyPlace Place, WorkRobotEnemyState State, EnemyAnimationCueContext Context, WorkRobotBehavior Owner);

    /// <summary>What each of the Work Robot's signals does; each runs on.</summary>
    private IdentityTable<WorkRobotCueKind, Action<RoomEnemyPlace, WorkRobotEnemyState, Position16<RoomPixels>>> WorkRobotSignals => field ??=
        new(new Dictionary<WorkRobotCueKind, Action<RoomEnemyPlace, WorkRobotEnemyState, Position16<RoomPixels>>>
        {
            [WorkRobotCueKind.Footstep] = (place, _, camera) =>
            {
                if (IsWorkRobotOriginStrictlyOnScreen(place, camera))
                    _audio.QueueEnemySound(EnemySoundEffectDefinitions.WorkRobotFootstepSound, priority: SoundRequestPriority.Normal);
            },
            [WorkRobotCueKind.CountDownLaserCooldown] = (_, state, _) => DecrementWorkRobotLaserCooldown(state),
        });

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

        // The foot probe stands beside the robot and falls; the robot keeps the fractions its fall left
        // (ProbeMovementKept).
        int probeX = (WorkRobotMotionDefinitions.LedgeProbeRadii.Toward(ledge.ProbeSide)) * place.Radius.X;
        var probe = new EnemyTerrainProbe(place, new Offset16<RoomPixels>(probeX, 0));
        bool floorFound = _terrain.MoveEnemyVertically(level, probe, WorkRobotMotionDefinitions.LedgeProbeDepth);
        place.Position = ProbeMovementKept.AfterLedgeProbe(place.Position, probe.Position);
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
        WorkRobotLaser laser,
        WorkRobotSegment firingSegment,
        Position16<RoomPixels> camera)
    {
        if (!state.LaserCooldown.IsZero)
        {
            DecrementWorkRobotLaserCooldown(state);
            return null;
        }

        state.LaserCooldown = WrappingCountdown.Start(WorkRobotLaserDefinitions.CooldownJitter.Of(_room.Random.Read()) + WorkRobotLaserDefinitions.CooldownBase);
        SpawnWorkRobotLaser(robot, state, laser, camera);
        return firingSegment;
    }

    /// <summary>Counts the laser cooldown down, stopping at zero.</summary>
    private static void DecrementWorkRobotLaserCooldown(WorkRobotEnemyState state)
    {
        state.LaserCooldown = state.LaserCooldown.DecrementToZero();
    }

    /// <summary>
    /// Runs the robot's custom touch. Work Robots are moving solids: body contact publishes a
    /// external push away from their center (<see cref="WorkRobotMotionDefinitions.TouchPush"/>) and deliberately deals no touch damage.
    /// </summary>
    private static void ResolveWorkRobotTouch(RoomEnemyPlace robot, SamusState samus)
    {
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromWholePixels(
            WorkRobotMotionDefinitions.TouchPush.Toward(robot.Position.X.Whole < samus.Position.X.Whole ? HorizontalDirection.Right : HorizontalDirection.Left)));
    }

    /// <summary>
    /// Finishes a powered robot's reaction to a shot: the robot takes no damage, but the shot's impact still turns it to
    /// recoil ahead or behind and holds off its next laser for its recoil delay
    /// (<see cref="WorkRobotLaserDefinitions.RecoilDelay"/>).
    /// </summary>
    private static void ResolveWorkRobotShotAfterCommon(RoomEnemyPlace robot, WorkRobotEnemyState state, SamusState samus)
    {
        if (robot.Health == 0)
            return;
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
    /// Plays the powered robots' glow on after the enemies' update unless it is stopped; when the step's time is up the
    /// glow shows its next step on every powered robot still in the place it set up and holds it for that step's hold.
    /// </summary>
    public void StepGlow()
    {
        if (!GlowRuns)
            return;
        WorkRobotGlowPlace shown = GlowPlace;
        this.glow.Step();
        // No step of the glow shows the place the step before it showed, so a place that differs from the one shown is the
        // next step's, shown now.
        if (GlowPlace == shown)
            return;

        PictureShading glow = Glow;
        // A robot that leaves its place removes the place from the list as it leaves, so no place it left,
        // nor any enemy that took the place since, shows the glow.
        foreach (RoomEnemyPlace robot in _poweredRobots)
        {
            // A place whose robot has left it, and any enemy that took the place since, shows no glow.
            if (robot.Entity is PoweredWorkRobotEntity)
                robot.Shading = glow;
        }
    }

    /// <summary>
    /// Removes the place a powered robot has left, deleted, killed or replaced, from the robots the glow shows on: the
    /// place shows no glow from then on unless a powered robot sets up there again.
    /// </summary>
    internal void RemovePoweredRobot(RoomEnemyPlace place) => _poweredRobots.Remove(Ensure.NotNull(place));

    /// <summary>Applies a powered robot's recoil after the bomb damage it takes, which it never rejects once powered.</summary>
    /// <param name="state">The state of the robot the reaction is built for.</param>
    internal static EnemyBombReaction PoweredWorkRobotBombReaction(WorkRobotEnemyState state) => new()
    {
        AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, state, c.Samus),
    };

    /// <summary>An unpowered robot takes the bomb damage, which it never rejects, and nothing more: it has no recoil.</summary>
    internal EnemyBombReaction UnpoweredWorkRobotBombReaction { get; } = new();

    /// <summary>A robot without power runs nothing after the common shot reaction: it has no recoil.</summary>
    internal EnemyShotReaction UnpoweredWorkRobotShotReaction { get; } = new();

    /// <summary>A Work Robot laser, whichever way it flies (#1760): it flies with room collision.</summary>
    private static readonly EnemyProjectileBehavior Laser = new SingleMotionProjectileBehavior(
        context => FlyWorkRobotLaser(context.Projectile, context.Level));

    /// <summary>Runs a powered robot's main AI.</summary>
    internal void RunWorkRobotMainAi(RoomEnemyPlace place, WorkRobotEnemyState state, IEnemyAiFrame frame)
    {
        state.FallVelocity = RunWorkRobotFall(place, state.FallVelocity, frame.Level);
    }

    /// <summary>Advances an unpowered robot's pose, which holds and raises no cue.</summary>
    internal static void AdvanceUnpoweredWorkRobotTypedAnimation(RoomEnemyPlace place, UnpoweredWorkRobotEnemyState unpoweredWorkRobotEnemyState) =>
        EnemyAnimationPlayback.Advance(place, unpoweredWorkRobotEnemyState, WorkRobotAnimations.Segments,
            static (UnpoweredWorkRobotEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"An unpowered Work Robot's pose raised {cue}."));

    /// <summary>Advances a powered robot's typed animation.</summary>
    internal void AdvanceWorkRobotTypedAnimation(RoomEnemyPlace place, WorkRobotEnemyState workRobotEnemyState, EnemyAnimationCueContext context) =>
        EnemyAnimationPlayback.Advance(place, workRobotEnemyState, WorkRobotAnimations.Segments,
            static (WorkRobotEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWorkRobotCue(place, state, context, cue) is { } continuesAt
                ? new AnimationContinuation<WorkRobotSegment>(WorkRobotAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>The powered robot's shot reaction: the common damage, then its recoil.</summary>
    /// <param name="state">The state of the robot the reaction is built for.</param>
    internal static EnemyShotReaction WorkRobotShotReaction(WorkRobotEnemyState state) => new()
    {
        AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, state, c.Samus),
    };

    /// <summary>Routes robot contact to its external push without Samus damage.</summary>
    internal static void ReactToWorkRobotTouch(EnemyTouchContext context) =>
        ResolveWorkRobotTouch(context.Place, context.Samus);

    /// <summary>
    /// Allocates a Work Robot laser: it flies ahead of the robot, the way it faces, and up, straight or down as
    /// <paramref name="laser"/> names, its velocity holding its way (#2278).
    /// </summary>
    private void SpawnWorkRobotLaser(
        RoomEnemyPlace robot,
        WorkRobotEnemyState state,
        WorkRobotLaser laser,
        Position16<RoomPixels> camera)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(laser.ProjectileKind(), Laser, SpriteGraphics.SharedStandard, new WorkRobotLaserAnimation(WorkRobotLaserSegment.Flare));
        if (projectile is null)
            return;

        // The laser takes its kind's radii, damage and contact (the diagonal and horizontal lasers differ), and draws
        // from the room's shared sheet in its standard colours.
        projectile.Velocity = projectile.Velocity.WithX(WorkRobotLaserDefinitions.Speed.Toward(state.Facing));
        projectile.Velocity = projectile.Velocity.WithY(laser switch
        {
            WorkRobotLaser.Upward => -WorkRobotLaserDefinitions.DiagonalVerticalSpeed,
            WorkRobotLaser.Downward => WorkRobotLaserDefinitions.DiagonalVerticalSpeed,
            WorkRobotLaser.Horizontal => SignedEightEight.Zero,
            _ => throw new InvalidOperationException(
                $"Undefined Work Robot laser {laser}."),
        });

        // Every laser starts at the muzzle ahead of the robot, the way it faces, and above it.
        int ahead = WorkRobotLaserDefinitions.MuzzleAhead;
        projectile.Position = new(FixedCoordinate.FromWhole(robot.Position.X.Whole +
            (ahead.Toward(state.Facing))),
            FixedCoordinate.FromWhole(robot.Position.Y.Whole - WorkRobotLaserDefinitions.MuzzleAbove));

        if (WorkRobotLaserSoundRange.RequestsSound(robot.Position.Whole, robot.Radius, camera))
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.WorkRobotLaserSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Flies the laser for one update: X collision, then Y collision, else move.</summary>
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
