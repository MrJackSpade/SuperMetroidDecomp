using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WorkRobots.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The work robot species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WorkRobotBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. WorkRobotPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WorkRobotEntity(this, slot, common);

    /// <summary>Checks Phantoon's defeat flag and samples the room random word for laser timing.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves the robot along floors and probes its next foothold.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates lasers from the common enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy slots, whose powered robots all show the shared glow.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Whether Phantoon's area-boss bit is set, which powers the work robots.</summary>
    internal bool IsAreaBossDefeated => _room.Services.Bosses.IsDefeated(BossBits.AreaBoss);

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds robot locomotion, power state, lasers, and the glow to room services.</summary>
    internal WorkRobotBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
    }

    /// <summary>The leftward laser velocity, two pixels per update; only its X is meaningful.</summary>
    private static readonly EightEightVector<RoomPixels> WorkRobotFacingLeftVelocity = new(-SignedEightEight.FromParts(2, 0), SignedEightEight.Zero);
    /// <summary>The rightward laser velocity, two pixels per update; only its X is meaningful.</summary>
    private static readonly EightEightVector<RoomPixels> WorkRobotFacingRightVelocity = new(SignedEightEight.FromParts(2, 0), SignedEightEight.Zero);
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
    /// <summary>Pixels advanced by one robot walk instruction.</summary>
    private const int WorkRobotStepPixels = 4;

    /// <summary>Updates left before the powered robots' glow steps; zero while the glow is stopped.</summary>
    internal int GlowStepCountdown { get; private set; }
    /// <summary>
    /// The step of the glow every powered robot shows, 0 through <see cref="WorkRobotGlowDefinitions.StepCount"/> minus
    /// one; after the last step the cycle starts again at the first.
    /// </summary>
    internal int GlowStep { get; private set; } = WorkRobotGlowDefinitions.StepCount - 1;
    /// <summary>The glow every powered robot in the room shows now (#1512).</summary>
    internal PictureShading Glow => PictureShading.Shaded(WorkRobotShades.Glow(GlowStep));

    /// <summary>Matches powered and unpowered Wrecked Ship robot population definitions.</summary>
    internal static bool IsWorkRobotDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.Robot or EnemyDefinitionId.RobotNoPower;

    /// <summary>Initializes the Robot.</summary>
    internal void InitializeWorkRobot(RoomEnemySlot slot)
    {
        var state = new WorkRobotEnemyState();
        slot.SetSpeciesState(state);

        bool areaBossDefeated = _room.Services.Bosses.IsDefeated(BossBits.AreaBoss);
        if (slot.DefinitionId != EnemyDefinitionId.Robot || !areaBossDefeated)
        {
            InitializeWorkRobotNoPower(slot, state);
            return;
        }

        state.Powered = true;

        // The initializer sets the animation-script bit and the unknown-but-observable high
        // bit. Population records already carry process-off-screen in retail rooms.
        slot.Contact = slot.Contact.With(EnemyContact.SolidToSamus);
        slot.Processing = slot.Processing.With(EnemyProcessing.AnimationScript);
        slot.FrameHold = WrappingCountdown.Start(4);
        slot.Timer = WrappingCountdown.Start(0);
        state.Playback = new(WorkRobotAnimations.StartingWith(WorkRobotProgram.Initial), AnimationPlace.StartOfAnimation);
        state.LaserVelocity = WorkRobotFacingLeftVelocity;
        state.LaserCooldown = WrappingCountdown.Start(0);

        // The room has one glow, not one per robot: every powered robot's initialization restarts it. Every powered
        // robot, and only a powered robot, shows it (#1512).
        RestartGlow();
        slot.Shading = Glow;
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
    internal void InitializeWorkRobotNoPower(RoomEnemySlot slot, WorkRobotEnemyState state)
    {
        state.Powered = false;

        // The placement names the unpowered pose the robot holds.
        state.Playback = new(WorkRobotAnimations.StartingWith(slot.SpawnSettings<WorkRobotSpawnSettings>().Pose), AnimationPlace.StartOfAnimation);
        slot.Contact = slot.Contact.With(EnemyContact.SolidToSamus);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        // An unpowered robot stops the room's glow, an earlier powered robot's too: in a room placing both, the order
        // they are placed in decides whether the glow runs.
        GlowStepCountdown = 0;
        state.FallVelocity = WorkRobotInitialFallVelocity;
    }

    /// <summary>Runs the robot's gravity-only main behaviour.</summary>
    private void RunWorkRobotMain(
        RoomEnemySlot slot,
        WorkRobotEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Work Robot gravity requires room collision data.");

        if (_terrain.MoveEnemyVertically(level, slot, state.FallVelocity))
            return;

        // Enemy instruction processing will decrement the timer later in this same frame.
        // Incrementing here cancels that tick, freezing the current art while airborne.
        slot.FrameHold = slot.FrameHold.AddFrames(1);
        // The fraction carries into the whole pixels, which wrap, as the game's paired additions do.
        state.FallVelocity += WorkRobotGravity;
    }

    /// <summary>
    /// Carries out one cue of the Work Robot's animations. Steps that meet a wall or ledge, the two walk redirections, and
    /// a laser that fires continue the animation elsewhere, as the routines replace the program.
    /// </summary>
    private WorkRobotProgram? ApplyWorkRobotCue(RoomEnemySlot slot, WorkRobotEnemyState state, EnemyInstructionContext context, WorkRobotCue cue)
    {
        SamusState? samus = context.Samus;
        RoomLevelData? level = context.Level;
        Position16<RoomPixels> camera = context.Camera;
        switch (cue.Kind)
        {
            case WorkRobotCueKind.LeftForwardWatchingLedges:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallProgram = WorkRobotProgram.FacingLeftHitWallMovingForwards, CheckLedge = true, LedgeProbeDirection = -1, FallProgram = WorkRobotProgram.ApproachingFallLeft, FallLaserVelocity = WorkRobotFacingRightVelocity });
            case WorkRobotCueKind.LeftForward:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallProgram = WorkRobotProgram.FacingLeftHitWallMovingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallProgram = null, FallLaserVelocity = default });
            case WorkRobotCueKind.LeftBackwardWatchingLedges:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallProgram = WorkRobotProgram.FacingLeftWalkingForwards, CheckLedge = true, LedgeProbeDirection = 1, FallProgram = WorkRobotProgram.ApproachingFallRight, FallLaserVelocity = WorkRobotFacingLeftVelocity });
            case WorkRobotCueKind.LeftBackward:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingLeftVelocity, WallProgram = WorkRobotProgram.FacingLeftWalkingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallProgram = null, FallLaserVelocity = default });
            case WorkRobotCueKind.WalkRight:
                return WorkRobotProgram.FacingRightWalkingForwards;
            case WorkRobotCueKind.RightForwardWatchingLedges:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallProgram = WorkRobotProgram.FacingRightHitWallMovingForwards, CheckLedge = true, LedgeProbeDirection = 1, FallProgram = WorkRobotProgram.ApproachingFallRight, FallLaserVelocity = WorkRobotFacingLeftVelocity });
            case WorkRobotCueKind.RightForward:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallProgram = WorkRobotProgram.FacingRightHitWallMovingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallProgram = null, FallLaserVelocity = default });
            case WorkRobotCueKind.RightBackwardWatchingLedges:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallProgram = WorkRobotProgram.FacingLeftWalkingForwards, CheckLedge = true, LedgeProbeDirection = -1, FallProgram = WorkRobotProgram.ApproachingFallLeft, FallLaserVelocity = WorkRobotFacingRightVelocity });
            case WorkRobotCueKind.RightBackward:
                return MoveWorkRobot(slot, state, samus, level, new WorkRobotMoveStep { HorizontalPixels = -WorkRobotStepPixels, FacingLaserVelocity = WorkRobotFacingRightVelocity, WallProgram = WorkRobotProgram.FacingLeftWalkingForwards, CheckLedge = false, LedgeProbeDirection = 0, FallProgram = null, FallLaserVelocity = default });
            case WorkRobotCueKind.Footstep:
                if (IsWorkRobotOriginStrictlyOnScreen(slot, camera))
                    _audio.QueueEnemySound(WorkRobotFootstepSound, priority: SoundRequestPriority.Normal);
                return null;
            case WorkRobotCueKind.WalkLeft:
                return WorkRobotProgram.FacingLeftWalkingForwards;
            case WorkRobotCueKind.TryLaserUpRight:
                return TryFireWorkRobotLaser(
                    slot, state, WorkRobotLaser.UpRight,
                    WorkRobotProgram.FacingRightShotLaserUpRight,
                    camera);
            case WorkRobotCueKind.TryLaserUpLeft:
                return TryFireWorkRobotLaser(
                    slot, state, WorkRobotLaser.UpLeft,
                    WorkRobotProgram.FacingLeftShotLaserUpLeft,
                    camera);
            case WorkRobotCueKind.TryLaserRight:
                return TryFireWorkRobotLaser(
                    slot, state, WorkRobotLaser.Horizontal,
                    WorkRobotProgram.FacingRightShotLaserRight,
                    camera);
            case WorkRobotCueKind.TryLaserLeft:
                return TryFireWorkRobotLaser(
                    slot, state, WorkRobotLaser.Horizontal,
                    WorkRobotProgram.FacingLeftShotLaserLeft,
                    camera);
            case WorkRobotCueKind.TryLaserDownRight:
                return TryFireWorkRobotLaser(
                    slot, state, WorkRobotLaser.DownRight,
                    WorkRobotProgram.FacingRightShotLaserDownRight,
                    camera);
            case WorkRobotCueKind.TryLaserDownLeft:
                return TryFireWorkRobotLaser(
                    slot, state, WorkRobotLaser.DownLeft,
                    WorkRobotProgram.FacingLeftShotLaserDownLeft,
                    camera);
            case WorkRobotCueKind.CountDownLaserCooldown:
                DecrementWorkRobotLaserCooldown(state);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(WorkRobotCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Applies one walk step, carries Samus on contact, and branches to wall or fall lists when support ends.</summary>
    private WorkRobotProgram? MoveWorkRobot(RoomEnemySlot slot, WorkRobotEnemyState state, SamusState? samus, RoomLevelData? level, WorkRobotMoveStep step)
    {
        if (level is null)
            throw new InvalidOperationException("Work Robot movement instruction requires room collision data.");

        DecrementWorkRobotLaserCooldown(state);
        state.LaserVelocity = step.FacingLaserVelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                SignedSixteenSixteen.FromWholePixels(step.HorizontalPixels)))
        {
            state.LaserCooldown = state.LaserCooldown.AddFrames(8);
            return step.WallProgram;
        }

        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(slot, samus))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(step.HorizontalPixels), 0));
        }

        if (!step.CheckLedge)
            return null;

        // The game moves only whole X/Y pixels for this temporary foot probe. In particular,
        // the downward move's collision-produced Y subposition survives after Y is restored.
        state.BackupPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithWrappedWholeX(
            slot.Position.X.Whole + step.LedgeProbeDirection * slot.Radius.X * 2);
        bool floorFound = _terrain.MoveEnemyVertically(level, slot, SignedSixteenSixteen.FromWholePixels(1));
        slot.Position = slot.Position.WithWhole(state.BackupPosition);
        if (floorFound)
            return null;

        state.LaserCooldown = state.LaserCooldown.AddFrames(8);
        state.LaserVelocity = step.FallLaserVelocity;
        return step.FallProgram ?? throw new InvalidOperationException("A ledge-watching Work Robot step names no fall animation.");
    }

    /// <summary>Checks the robot origin against the screen rectangle with exclusive top and left edges.</summary>
    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemySlot robot,
        Position16<RoomPixels> camera) =>
        CoordinateAxis.Difference(robot.Position.X.Whole, camera.X) > 0 &&
        CoordinateAxis.AsSigned(camera.X + 256 - robot.Position.X.Whole) >= 0 &&
        CoordinateAxis.Difference(robot.Position.Y.Whole, camera.Y) > 0 &&
        CoordinateAxis.AsSigned(camera.Y + 224 - robot.Position.Y.Whole) >= 0;

    /// <summary>Uses the existing random word to reset cooldown and enters the firing list when ready.</summary>
    private WorkRobotProgram? TryFireWorkRobotLaser(
        RoomEnemySlot robot,
        WorkRobotEnemyState state,
        WorkRobotLaser projectileDefinition,
        WorkRobotProgram firingProgram,
        Position16<RoomPixels> camera)
    {
        if (!state.LaserCooldown.IsZero)
        {
            DecrementWorkRobotLaserCooldown(state);
            return null;
        }

        state.LaserCooldown = WrappingCountdown.Start(WorkRobotLaserDefinitions.CooldownJitter.Of(_room.Random.Read()) + 16);
        SpawnWorkRobotLaser(robot, state, projectileDefinition, camera);
        return firingProgram;
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
    private static void ResolveWorkRobotTouch(RoomEnemySlot robot, SamusState samus)
    {
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromParts(
            robot.Position.X.Whole < samus.Position.X.Whole ? 4 : -4, 0));
    }

    /// <summary>
    /// Runs the private tail of powered shot AI after common shot handling. The
    /// indestructible vulnerability table leaves health unchanged, but the accepted impact
    /// still chooses an ahead/behind recoil list and delays the next laser by 64 frames.
    /// </summary>
    private void ResolveWorkRobotShotAfterCommon(RoomEnemySlot robot, SamusState? samus)
    {
        if (robot.DefinitionId != EnemyDefinitionId.Robot || robot.Health == 0)
            return;
        if (!_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
            return;
        if (samus is null)
            throw new InvalidOperationException("Work Robot shot recoil requires the active Samus actor.");

        WorkRobotEnemyState state = robot.RequireSpeciesState<WorkRobotEnemyState>();
        bool facingLeft = state.LaserVelocity.X.IsNegative;
        if (facingLeft)
        {
            state.Playback = new(WorkRobotAnimations.StartingWith(samus.Position.X.Whole < robot.Position.X.Whole
                ? WorkRobotProgram.FacingLeftShotSamusAhead
                : WorkRobotProgram.FacingLeftShotSamusBehind), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Playback = new(WorkRobotAnimations.StartingWith(samus.Position.X.Whole >= robot.Position.X.Whole
                ? WorkRobotProgram.FacingRightShotSamusAhead
                : WorkRobotProgram.FacingRightShotSamusBehind), AnimationPlace.StartOfAnimation);
        }
        robot.FrameHold = WrappingCountdown.Start(1);
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
        foreach (RoomEnemySlot robot in _slotTable.Slots)
        {
            if (robot.SpeciesState<WorkRobotEnemyState>() is { Powered: true })
                robot.Shading = glow;
        }
        GlowStepCountdown = WorkRobotGlowDefinitions.Durations[GlowStep];
    }

    /// <summary>Rejects powered-robot bomb damage before Phantoon's defeat and applies recoil after admitted hits.</summary>
    internal void AddWorkRobotBombReactions(EnemySharedReactions table)
    {
        // Powered Work Robot returns until Phantoon is dead. The bomb pass has already marked the bomb, so callback
        // rejection belongs here rather than in collision admission.
        table.AddBombCommon(new EnemyBombReaction(c => IsWorkRobotDefinition(c.Enemy.DefinitionId) &&
                                                       c.SelectedShotAi is EnemyReaction.WorkRobotShot or EnemyReaction.WorkRobotNoPowerShot)
        {
            Rejects = c => c.Enemy.DefinitionId == EnemyDefinitionId.Robot &&
                           c.SelectedShotAi == EnemyReaction.WorkRobotShot &&
                           !_room.Services.Bosses.IsDefeated(BossBits.AreaBoss),
            AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
        });
    }

    /// <summary>Connects robot lasers to collision-first movement in their projectile frame behaviour.</summary>
    internal static void AddWorkRobotProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunWorkRobotLaserFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.WorkRobotLaserFlight);
    }

    /// <summary>Ports WorkRobot's main AI.</summary>
    internal void RunWorkRobotMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWorkRobotMain(slot, slot.RequireSpeciesState<WorkRobotEnemyState>(), frame.Level);
    }

    /// <summary>Advances WorkRobot's typed animation.</summary>
    internal void AdvanceWorkRobotTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, WorkRobotAnimations.Segments, WorkRobotFrameDefinitions.ShownFrames.Of,
            static (WorkRobotEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyWorkRobotCue(slot, state, context, cue) is { } continuesAt
                ? new AnimationContinuation<WorkRobotProgram>(WorkRobotAnimations.StartingWith(continuesAt), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>Applies the powered robot's accepted-impact recoil tail after common projectile shots.</summary>
    internal void AddWorkRobotShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsWorkRobotDefinition(enemy.DefinitionId) &&
                                                     EnemyCollision.ShotAi(enemy) is EnemyReaction.WorkRobotShot or EnemyReaction.WorkRobotNoPowerShot)
        {
            AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
        });
    }

    /// <summary>Routes robot contact to its four-pixel external push without Samus damage.</summary>
    internal static void AddWorkRobotTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => IsWorkRobotDefinition(slot.DefinitionId) &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.WorkRobotTouch,
            context => ResolveWorkRobotTouch(context.Slot, context.Samus));
    }

    /// <summary>
    /// Allocates one of the five Work Robot laser definitions. Horizontal left/right
    /// share one definition; the sign in the owner's laser velocity supplies the sixth direction.
    /// </summary>
    internal void SpawnWorkRobotLaser(
        RoomEnemySlot robot,
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
            SpriteGraphics.SharedStandard);
        projectile.Velocity = projectile.Velocity.WithX(state.LaserVelocity.X);
        projectile.Velocity = projectile.Velocity.WithY(definition switch
        {
            WorkRobotLaser.UpLeft or WorkRobotLaser.UpRight =>
                -SignedEightEight.FromParts(0, 128),
            WorkRobotLaser.DownLeft or WorkRobotLaser.DownRight => SignedEightEight.FromParts(0, 128),
            WorkRobotLaser.Horizontal => SignedEightEight.Zero,
            _ => throw new InvalidOperationException(
                $"Undefined Work Robot laser definition {definition}."),
        });

        // All five initializers place the muzzle four pixels in the signed facing direction
        // and sixteen pixels above the robot.
        projectile.Position = new(new(CoordinateAxis.Wrap(robot.Position.X.Whole +
            (state.LaserVelocity.X.IsNegative ? -4 : 4)), 0), new(CoordinateAxis.Wrap(robot.Position.Y.Whole - 16), 0));

        // Preserve the original laser viewport bug. Its final Y comparison omits
        // the camera's Y, so lasers only request sound while their owner's absolute
        // top edge is less than 224 pixels from the room's top, even when a lower room row is on screen.
        bool horizontallyVisible =
            CoordinateAxis.AsSigned(robot.Position.X.Whole + robot.Radius.X - camera.X) >= 0 &&
            CoordinateAxis.Difference(robot.Position.X.Whole - robot.Radius.X - 257, camera.X) < 0;
        bool verticallyVisible =
            CoordinateAxis.AsSigned(robot.Position.Y.Whole + robot.Radius.Y - camera.Y) >= 0 &&
            CoordinateAxis.Difference(robot.Position.Y.Whole - robot.Radius.Y, 224) < 0;
        if (horizontallyVisible && verticallyVisible)
            _audio.QueueEnemySound(WorkRobotLaserSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs the laser's per-frame behaviour: X collision, then Y collision, else move.</summary>
    internal static void RunWorkRobotLaserFrameBehavior(
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
