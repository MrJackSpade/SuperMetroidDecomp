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
    /// <summary>Writes the shared animated robot palette into its OBJ color row.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Whether Phantoon's area-boss bit is set, which powers the work robots.</summary>
    internal bool IsAreaBossDefeated => _room.Services.Bosses.IsDefeated(BossBits.AreaBoss);

    /// <summary>Binds robot locomotion, power state, lasers, and palette animation to room services.</summary>
    internal WorkRobotBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyPresentation presentation)
    {
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>The leftward laser velocity, two pixels per update; only its X is meaningful.</summary>
    private static readonly EightEightVector<RoomPixels> WorkRobotFacingLeftVelocity = new(-512, 0);
    /// <summary>The rightward laser velocity, two pixels per update; only its X is meaningful.</summary>
    private static readonly EightEightVector<RoomPixels> WorkRobotFacingRightVelocity = new(512, 0);
    /// <summary>What gravity adds to a falling unpowered robot's speed each airborne update: half a pixel.</summary>
    private static readonly SnesSignedSixteenSixteen WorkRobotGravity = SnesSignedSixteenSixteen.FromParts(0, 32768);
    /// <summary>The speed an unpowered robot starts falling at: one pixel per update.</summary>
    private static readonly SnesSignedSixteenSixteen WorkRobotInitialFallVelocity = SnesSignedSixteenSixteen.FromParts(1, 0);

    /// <summary>Library-two sound queued by a robot footstep.</summary>
    private static readonly SoundEffectId WorkRobotFootstepSound =
        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.WorkRobotFootstepSound);
    /// <summary>Library-two sound queued by a robot laser.</summary>
    private static readonly SoundEffectId WorkRobotLaserSound =
        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.WorkRobotLaserSound);
    /// <summary>Pixels advanced by one robot walk instruction.</summary>
    private const int WorkRobotStepPixels = 4;

    /// <summary>Shared countdown to the next robot palette animation record.</summary>
    internal WrappingCountdown WorkRobotPaletteAnimationTimer { get; set; }
    /// <summary>
    /// The next palette record the robot palette hook loads, 0 through
    /// <see cref="WorkRobotPaletteTimingDefinitions.RecordCount"/>; the count itself is the table's terminator, which the
    /// hook resolves back to record zero before loading.
    /// </summary>
    internal int WorkRobotPaletteAnimationRecord { get; set; }
    /// <summary>The OBJ palette row, 0 through 7, the robot graphics-drawn hook animates.</summary>
    internal int WorkRobotPaletteAnimationPaletteRow { get; set; }

    /// <summary>
    /// Most recent library-two sound requested by robot feet ($68) or a robot laser ($67).
    /// It is cleared at the beginning of every enemy frame, like the other audio seams.
    /// </summary>
    public SoundEffectId? LastWorkRobotSoundEffect { get; private set; }

    /// <summary>Matches powered and unpowered Wrecked Ship robot population definitions.</summary>
    internal static bool IsWorkRobotDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.Robot or EnemyDefinitionId.RobotNoPower;

    /// <summary>Ports <c>InitAI_Robot</c> at $A8:CB77.</summary>
    internal void InitializeWorkRobot(RoomEnemySlot slot)
    {
        var state = new WorkRobotEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        bool areaBossDefeated = _room.Services.Bosses.IsDefeated(BossBits.AreaBoss);
        if (slot.DefinitionId != EnemyDefinitionId.Robot || !areaBossDefeated)
        {
            InitializeWorkRobotNoPower(slot, state);
            return;
        }

        state.Powered = true;

        // ORA #$A000 installs the common instruction bit and unknown-but-observable bit
        // $8000. Population records already carry process-off-screen $0800 in retail rooms.
        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = WrappingCountdown.Start(4);
        slot.Timer = WrappingCountdown.Start(0);
        state.Program = AnimationProgramCursor<WorkRobotProgram>.Start(WorkRobotProgram.Initial);
        state.LaserVelocity = WorkRobotFacingLeftVelocity;
        state.LaserCooldown = WrappingCountdown.Start(0);

        // The graphics-drawn hook is global. Every powered initialization resets its shared
        // clock/table and remembers this actor's OBJ palette exactly as $A8:CB83-$CBCB does.
        WorkRobotPaletteAnimationTimer = WrappingCountdown.Start(1);
        WorkRobotPaletteAnimationRecord = 0;
        WorkRobotPaletteAnimationPaletteRow = (slot.PaletteIndex >> 9) & 7;
    }

    /// <summary>Ports <c>InitAI_RobotNoPower</c> at $A8:CBCC.</summary>
    internal void InitializeWorkRobotNoPower(RoomEnemySlot slot, WorkRobotEnemyState state)
    {
        state.Powered = false;

        // The native range check accidentally accepts parameter three even though the
        // pointer table has only three entries; retail populations use zero/one, and the
        // overrun into code at $CC36 fails loudly in the animation selection.
        int animation = slot.Spawn.Population.Parameter1;
        if (unchecked((short)animation) < 0 || animation >= 4)
            animation = 0;
        state.Program = AnimationProgramCursor<WorkRobotProgram>.Start(
            WorkRobotInitializationDefinitions.InitialProgram(animation));
        slot.Properties = slot.Properties.With(EnemyProperties.SolidToSamus);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        // This global write disables an earlier robot's palette hook too. Mixed powered and
        // unpowered sets are unusual, but retaining the shared timer makes order observable.
        WorkRobotPaletteAnimationTimer = WrappingCountdown.Start(0);
        state.FallVelocity = WorkRobotInitialFallVelocity;

        // $A8:CC1C writes four colors to active palette RAM through a stale global palette
        // index. Room fade immediately overwrites them, and no retail deactivated robot can
        // select its own palette there. The deliberately ineffective writes are documented
        // rather than synthesized into CGRAM, matching the visible cartridge result.
    }

    /// <summary>Ports the gravity-only <c>MainAI_Robot</c> at $A8:CC36.</summary>
    private void RunWorkRobotMain(
        RoomEnemySlot slot,
        WorkRobotEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Work Robot gravity requires room collision data.");

        if (_terrain.MoveEnemyVertically(level, slot, state.FallVelocity.RawValue))
            return;

        // Enemy instruction processing will decrement the timer later in this same frame.
        // Incrementing here cancels that tick, freezing the current art while airborne.
        slot.InstructionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(slot.InstructionTimer.RawValue + 1)));
        // The fraction carries into the whole pixels, which wrap, as the cartridge's paired additions do.
        state.FallVelocity += WorkRobotGravity;
    }

    /// <summary>
    /// Carries out one cue of the Work Robot's animations. Steps that meet a wall or ledge, the two walk redirections, and
    /// a laser that fires continue the animation elsewhere, as the native routines replace the instruction pointer.
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
                    LastWorkRobotSoundEffect = WorkRobotFootstepSound;
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
                SnesSixteenSixteen.FromWhole(step.HorizontalPixels)))
        {
            state.LaserCooldown = WrappingCountdown.FromRaw(unchecked((ushort)(state.LaserCooldown.RawValue + 8)));
            return step.WallProgram;
        }

        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(slot, samus))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SnesSignedSixteenSixteen.FromParts(unchecked((short)step.HorizontalPixels), 0));
        }

        if (!step.CheckLedge)
            return null;

        // The ROM moves only whole X/Y words for this temporary foot probe. In particular,
        // MoveEnemyDown's collision-produced Y subposition survives after Y is restored.
        state.BackupPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(
            slot.Position.X.Whole + step.LedgeProbeDirection * slot.Radius.X * 2)));
        bool floorFound = _terrain.MoveEnemyVertically(level, slot, 1 << 16);
        slot.Position = slot.Position.WithWhole(state.BackupPosition);
        if (floorFound)
            return null;

        state.LaserCooldown = WrappingCountdown.FromRaw(unchecked((ushort)(state.LaserCooldown.RawValue + 8)));
        state.LaserVelocity = step.FallLaserVelocity;
        return step.FallProgram ?? throw new InvalidOperationException("A ledge-watching Work Robot step names no fall animation.");
    }

    /// <summary>Checks the robot origin against the native screen rectangle with exclusive top and left edges.</summary>
    private static bool IsWorkRobotOriginStrictlyOnScreen(
        RoomEnemySlot robot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(robot.Position.X.Whole - camera.X)) > 0 &&
        unchecked((short)(camera.X + 256 - robot.Position.X.Whole)) >= 0 &&
        unchecked((short)(robot.Position.Y.Whole - camera.Y)) > 0 &&
        unchecked((short)(camera.Y + 224 - robot.Position.Y.Whole)) >= 0;

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

        Func<int> readRandomNumber = _room.Random.Read;
        state.LaserCooldown = WrappingCountdown.FromRaw(unchecked((ushort)((readRandomNumber() & 31) + 16)));
        SpawnWorkRobotLaser(robot, state, projectileDefinition, camera);
        return firingProgram;
    }

    /// <summary>Counts a nonzero laser cooldown down without unsigned wraparound.</summary>
    private static void DecrementWorkRobotLaserCooldown(WorkRobotEnemyState state)
    {
        state.LaserCooldown = state.LaserCooldown.DecrementToZero();
    }

    /// <summary>
    /// Ports custom touch $A8:D174. Work Robots are moving solids: body contact publishes a
    /// four-pixel external push away from their center and deliberately deals no touch damage.
    /// </summary>
    private static void ResolveWorkRobotTouch(RoomEnemySlot robot, SamusState samus)
    {
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(SnesSignedSixteenSixteen.FromParts(
            robot.Position.X.Whole < samus.Position.X.Whole ? (short)4 : (short)-4, 0));
    }

    /// <summary>
    /// Ports the private tail of powered shot AI $A8:D192 after common shot handling. The
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
        bool facingLeft = state.LaserVelocity.X.SignedRawValue < 0;
        if (facingLeft)
        {
            state.Program = AnimationProgramCursor<WorkRobotProgram>.Start(samus.Position.X.Whole < robot.Position.X.Whole
                ? WorkRobotProgram.FacingLeftShotSamusAhead
                : WorkRobotProgram.FacingLeftShotSamusBehind);
        }
        else
        {
            state.Program = AnimationProgramCursor<WorkRobotProgram>.Start(samus.Position.X.Whole >= robot.Position.X.Whole
                ? WorkRobotProgram.FacingRightShotSamusAhead
                : WorkRobotProgram.FacingRightShotSamusBehind);
        }
        robot.InstructionTimer = WrappingCountdown.Start(1);
        state.LaserCooldown = WrappingCountdown.FromRaw(unchecked((ushort)(state.LaserCooldown.RawValue + 64)));
    }

    /// <summary>Ports the global graphics-drawn palette hook at $A8:CC67.</summary>
    internal void StepWorkRobotPaletteAnimation()
    {
        if (WorkRobotPaletteAnimationTimer.IsZero)
            return;
        WorkRobotPaletteAnimationTimer = WorkRobotPaletteAnimationTimer.Decrement().Remaining;
        if (!WorkRobotPaletteAnimationTimer.IsZero)
            return;

        // The terminator after the last record resolves back to the first.
        if (WorkRobotPaletteAnimationRecord == WorkRobotPaletteTimingDefinitions.RecordCount)
            WorkRobotPaletteAnimationRecord = 0;
        int record = WorkRobotPaletteAnimationRecord;
        ushort recordByteOffset = checked((ushort)(record * WorkRobotPaletteTimingDefinitions.RecordByteCount));

        ColourRun animated = _presentation.ColourSet(WorkRobotPaletteAnimationPaletteRow)
            .Slice(WorkRobotPaletteRomData.FirstAnimatedColor, WorkRobotPaletteRomData.ColorCount);
        (_room.Artwork?.WorkRobotPaletteCycle ?? throw new InvalidDataException(
            "Work Robot palette animation requires installed artwork."))
            .ApplyFrame(animated, record);
        WorkRobotPaletteAnimationTimer =
            WrappingCountdown.FromRaw(WorkRobotPaletteTimingDefinitions.DurationForByteOffset(recordByteOffset));
        WorkRobotPaletteAnimationRecord = record + 1;
    }

    /// <summary>Rejects powered-robot bomb damage before Phantoon's defeat and applies recoil after admitted hits.</summary>
    internal void RegisterWorkRobotBombReactions(EnemySharedReactions table)
    {
        // Powered Work Robot returns until Phantoon is dead. Bank $A0 has already marked the bomb, so callback
        // rejection belongs here rather than in collision admission.
        table.AddBombCommon(new EnemyBombReaction(c => IsWorkRobotDefinition(c.Enemy.DefinitionId) &&
                                                       c.SelectedShotAi is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot)
        {
            Rejects = c => c.Enemy.DefinitionId == EnemyDefinitionId.Robot &&
                           c.SelectedShotAi == EnemyInteractionCallback.WorkRobotShot &&
                           !_room.Services.Bosses.IsDefeated(BossBits.AreaBoss),
            AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
        });
    }

    /// <summary>Connects robot lasers to collision-first movement in their projectile pre-instruction.</summary>
    internal static void RegisterWorkRobotProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunWorkRobotLaserPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.WreckedShipRobotLaser);
    }

    /// <summary>Ports WorkRobot's main AI.</summary>
    internal void RunWorkRobotMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWorkRobotMain(slot, slot.RequireSpeciesState<WorkRobotEnemyState>(), frame.Level);
    }

    /// <summary>Advances WorkRobot's typed animation.</summary>
    internal void AdvanceWorkRobotTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, WorkRobotAnimations.Programs, WorkRobotFrameDefinitions.ShownFrames.Of,
            static (WorkRobotEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyWorkRobotCue(slot, state, context, cue));

    /// <summary>Applies the powered robot's accepted-impact recoil tail after common projectile shots.</summary>
    internal void RegisterWorkRobotShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => IsWorkRobotDefinition(enemy.DefinitionId) &&
                                                     EnemyCollision.ShotAi(enemy) is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot)
        {
            AfterCommon = c => ResolveWorkRobotShotAfterCommon(c.Enemy, c.Samus),
        });
    }

    /// <summary>Routes robot contact to its four-pixel external push without Samus damage.</summary>
    internal static void RegisterWorkRobotTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => IsWorkRobotDefinition(slot.DefinitionId) &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.WorkRobotTouch,
            context => ResolveWorkRobotTouch(context.Slot, context.Samus));
    }

    /// <summary>
    /// Allocates one of bank $86's five Work Robot laser definitions. Horizontal left/right
    /// share $D2B4; the sign in the owner's laser velocity supplies the sixth direction.
    /// </summary>
    internal void SpawnWorkRobotLaser(
        RoomEnemySlot robot,
        WorkRobotEnemyState state,
        WorkRobotLaser definition,
        Position16<RoomPixels> camera)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        // SpawnEnemyProjectileY_ParameterA_XGraphics installs the complete seven-word
        // definition before entering the family initializer. Keeping that division here is
        // important: the five records do not share radii or damage, and the common helper
        // also supplies the native $8000 pre-draw map sentinel and all three property bits.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            (RoomEnemyProjectileKind)definition,
            unchecked((ushort)(robot.VramTilesIndex | robot.PaletteIndex)));
        projectile.Velocity = projectile.Velocity.WithX(state.LaserVelocity.X);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(definition switch
        {
            WorkRobotLaser.UpLeft or WorkRobotLaser.UpRight =>
                unchecked((ushort)-128),
            WorkRobotLaser.DownLeft or WorkRobotLaser.DownRight => 128,
            WorkRobotLaser.Horizontal => 0,
            _ => throw new InvalidOperationException(
                $"Undefined Work Robot laser definition {definition}."),
        }));

        // All five initializers place the muzzle four pixels in the signed facing direction
        // and sixteen pixels above the robot. The temporary graphics word comes from the
        // owner; pre-instruction $D3BF clears it before the first draw, matching the ROM's
        // use of palette/tile bits embedded directly in the bank-$8D laser spritemaps.
        projectile.Position = new(new(unchecked((ushort)(robot.Position.X.Whole +
            (state.LaserVelocity.X.SignedRawValue < 0 ? -4 : 4))), 0), new(unchecked((ushort)(robot.Position.Y.Whole - 16)), 0));
        // $86:D326 clears the graphics word in the down-left/right initializer itself.
        // Upward and horizontal variants retain the owner's word until common
        // pre-instruction $D3BF clears it on their first projectile pass.
        if (definition is
            WorkRobotLaser.DownLeft or WorkRobotLaser.DownRight)
            projectile.GraphicsIndex = 0;

        // Preserve the original $86:D35B viewport bug. Its final Y comparison omits
        // `CMP Layer1YPosition`, so lasers only request sound while their owner's absolute
        // top edge is above scanline 224, even when a lower room row is on screen.
        bool horizontallyVisible =
            unchecked((short)(robot.Position.X.Whole + robot.Radius.X - camera.X)) >= 0 &&
            unchecked((short)(robot.Position.X.Whole - robot.Radius.X - 257 - camera.X)) < 0;
        bool verticallyVisible =
            unchecked((short)(robot.Position.Y.Whole + robot.Radius.Y - camera.Y)) >= 0 &&
            unchecked((short)(robot.Position.Y.Whole - robot.Radius.Y - 224)) < 0;
        if (horizontallyVisible && verticallyVisible)
            LastWorkRobotSoundEffect = WorkRobotLaserSound;
    }

    /// <summary>Ports pre-instruction $86:D3BF: X collision, then Y collision, else move.</summary>
    internal static void RunWorkRobotLaserPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        projectile.GraphicsIndex = 0;
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Clear();
        }
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastWorkRobotSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastWorkRobotSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }
}
