using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's false death: the fade to gray, the tubes collapsing and falling, the room rows and the fade back.</summary>
[RoomLifetime]
internal sealed class MotherBrainFakeDeath
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Routes Mother Brain's AI and projectile handlers through the room dispatch tables.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    private readonly MotherBrainEncounterAccess _access;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFakeDeath(MotherBrainFakeDeathDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;

        _access = dependencies.Access;
    }

    /// <summary>
    /// Collision radii and landing floor selected by piece role. The two
    /// outer and two inner pieces share their values; the main tube has its own floor.
    /// </summary>
    internal static MotherBrainFallingTubeDefinition MotherBrainFallingTubeBody(int tubeIndex) =>
        tubeIndex switch
        {
            0 or 1 => new(new(16, 32), 248),
            2 or 3 => new(new(8, 24), 240),
            4 => new(new(16, 32), 246),
            _ => throw new InvalidDataException($"Mother Brain falling tube index {tubeIndex} is outside its five pieces."),
        };

    /// <summary>Steps Mother Brain's fake-death gray fade and advances when its palette cycle completes.</summary>
    internal void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        if (--state.PhaseTimer < 0)
        {
            state.PhaseTimer = 8;
            int step = state.GrayFadeIndex++;
            if (step == MotherBrainFakeDeathPaletteDefinitions.FrameCount)
            {
                state.Phase = MotherBrainBodyPhase.FakeDeathDescentCollapseTubes;
            }
            else
            {
                // Fake-death fade greys her brain's three colours only. Copying a convenient
                // whole palette would visibly alter unrelated room art and is not what
                // the fake-death fade does.
                state.Colours.ShowFakeDeathToGrey(step);
            }
        }

        // The grey fade's last step is an unconditional tail-call. Tube sequencing and dust therefore
        // begin during the grey fade rather than waiting for the eighth palette entry.
        RunMotherBrainTubeCollapse(state);
        RunMotherBrainFakeDeathExplosion(state);
    }

    /// <summary>Stops her room's flash and shows its steady grey.</summary>
    internal static void StopMotherBrainRoomFlash(MotherBrainEnemyState state)
    {
        state.RoomFlashFrame = null;
        state.RoomFlashTimer = 0;
        state.Colours.ShowRoomGrey();
    }

    /// <summary>Emits timed explosions while the first-phase body appears to die.</summary>
    internal void RunMotherBrainFakeDeathExplosion(MotherBrainEnemyState state)
    {
        if (--state.FakeDeathExplosionTimer >= 0)
            return;

        state.FakeDeathExplosionTimer = 8;
        // The index steps backward through the eight positions, from the first back to the last.
        state.FakeDeathExplosionIndex = (state.FakeDeathExplosionIndex + MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions.Length - 1) %
            MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions.Length;
        Position16<RoomPixels> explosion = MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions[state.FakeDeathExplosionIndex];

        // The tube dust reads the current random number and never draws a new one. Drawing a new
        // number here would desynchronize every later Rinka and phase-two attack decision.
        MiscDustAnimation animation = MotherBrainRandomDefinitions.FakeDeathDustSmokes.IsTakenBy(_room.Random.Read())
            ? MiscDustAnimation.Smoke
            : MiscDustAnimation.SmallExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(explosion, animation);
        _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);
    }

    /// <summary>Schedules glass and tube collapse phases with their authored waits.</summary>
    /// <remarks>Every spawn after the first waits until its timer goes negative, so each waits out the
    /// 32 (or 2) its preceding clear step set.</remarks>
    internal void RunMotherBrainTubeCollapse(MotherBrainEnemyState state)
    {
        switch (state.TubeCollapseStep)
        {
            case MotherBrainTubeCollapseStep.WaitForFourFreeProjectileSlots:
                if (_projectiles.EnemyProjectiles.Count(projectile => !projectile.IsActive) < 4)
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomLeft);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomLeftTube;
                return;

            case MotherBrainTubeCollapseStep.ClearBottomLeftTube:
                RequestMotherBrainTubeObject(state, new(5, 9),
                    MotherBrainRoomMutation.ClearBottomLeftTube,
                    MotherBrainTubeCollapseStep.SpawnTopRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, new(152, 47));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn9;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn9:
                RequestMotherBrainTubeObject(state, new(9, 2),
                    MotherBrainRoomMutation.ClearCeilingBlock,
                    MotherBrainTubeCollapseStep.SpawnTopLeftTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopLeftTube, new(104, 47));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn6;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn6:
                RequestMotherBrainTubeObject(state, new(6, 2),
                    MotherBrainRoomMutation.ClearCeilingBlock,
                    MotherBrainTubeCollapseStep.SpawnBottomRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnBottomRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomRight);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomRightTube;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomRightTube:
                RequestMotherBrainTubeObject(state, new(10, 9),
                    MotherBrainRoomMutation.ClearBottomRightTube,
                    MotherBrainTubeCollapseStep.SpawnBottomMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnBottomMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomMiddleLeft);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomMiddleLeftTube;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomMiddleLeftTube:
                RequestMotherBrainTubeObject(state, new(6, 10),
                    MotherBrainRoomMutation.ClearBottomMiddleSideTube,
                    MotherBrainTubeCollapseStep.SpawnTopMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube, new(120, 59));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn7;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn7:
                RequestMotherBrainTubeObject(state, new(7, 2),
                    MotherBrainRoomMutation.ClearCeilingTube,
                    MotherBrainTubeCollapseStep.SpawnTopMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube, new(136, 59));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn8;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn8:
                RequestMotherBrainTubeObject(state, new(8, 2),
                    MotherBrainRoomMutation.ClearCeilingTube,
                    MotherBrainTubeCollapseStep.SpawnBottomMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnBottomMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.BottomMiddleRight);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomMiddleRightTube;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomMiddleRightTube:
                RequestMotherBrainTubeObject(state, new(9, 10),
                    MotherBrainRoomMutation.ClearBottomMiddleSideTube,
                    MotherBrainTubeCollapseStep.SpawnMainTube, 2);
                return;
            case MotherBrainTubeCollapseStep.SpawnMainTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainTubePiece.Main);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomMiddleTubes;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomMiddleTubes:
                RequestMotherBrainTubeObject(state, new(7, 7),
                    MotherBrainRoomMutation.ClearBottomMiddleTubes,
                    MotherBrainTubeCollapseStep.Finished, 0);
                return;
            case MotherBrainTubeCollapseStep.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain tube has no behaviour for tube collapse step {state.TubeCollapseStep}.");
        }
    }

    /// <summary>Counts the tube phase's timer down and reports whether it ran past zero.</summary>
    private static bool DecrementMotherBrainTubeTimerPastZero(MotherBrainEnemyState state) => --state.TubeCollapseTimer < 0;

    /// <summary>Queues the room tile change at the tube's location.</summary>
    private void RequestMotherBrainTubeObject(
        MotherBrainEnemyState state,
        Position16<RoomBlocks> block,
        MotherBrainRoomMutation mutation,
        MotherBrainTubeCollapseStep next,
        int timer)
    {
        _access.RequestMotherBrainObject(block, mutation);
        state.TubeCollapseStep = next;
        state.TubeCollapseTimer = timer;
    }

    /// <summary>Spawns the top tube projectile for the scripted collapse.</summary>
    internal void SpawnMotherBrainTopTube(
        RoomEnemyProjectileKind kind,
        Position16<RoomPixels> position)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        MotherBrainProjectileStarts.Initialize(projectile, kind, graphics: SpriteGraphics.SharedIn(MotherBrainColours.GlassAndTubes));
        projectile.Position = projectile.Position.WithWhole(position);
        projectile.Velocity = EightEightVector<RoomPixels>.Zero;
        projectile.State = new MotherBrainTopTubeState();
    }

    /// <summary>Creates the falling tube enemy of <paramref name="piece"/>.</summary>
    internal void SpawnMotherBrainFallingTube(MotherBrainTubePiece piece)
    {
        // The bound source supplies the fixed cutscene placement; retail rooms resolve it from the loaded definitions.
        RoomEnemyPopulationRecord population = _room.LoadedDefinitions.ReadFallingTubePopulation(piece);
        if (population.DefinitionId != EnemyDefinitionId.MotherBrainTubes)
        {
            throw new InvalidDataException(
                $"Mother Brain tube record for {piece} names enemy " +
                $"{population.DefinitionId}, not the falling tube.");
        }

        // SpawnEnemy silently fails when all 32 records are occupied.
        _ = _dispatch.SpawnEnemy(population);
    }

    /// <summary>Initializes a falling tube.</summary>
    internal static void InitializeMotherBrainFallingTube(RoomEnemySlot tube)
    {
        MotherBrainTubeSpawnSettings settings = tube.Spawn.Population.SpawnSettingsOf<MotherBrainTubeSpawnSettings>();
        MotherBrainTubePiece piece = settings.Piece;
        MotherBrainFallingTubeDefinition tubeBody = MotherBrainFallingTubeBody(MotherBrainFallingTubePopulationDefinitions.Pieces.PositionOf(piece));
        tube.Radius = tubeBody.Radius;
        var state = new MotherBrainFallingTubeState
        {
            Playback = MotherBrainTubeAnimations.Starting(piece),
            Action = piece == MotherBrainTubePiece.Main
                ? MotherBrainTubeAction.MainTube_WaitingToFall
                : MotherBrainTubeAction.NonMainTube,
            FloorY = tubeBody.Floor,
            FallDelay = settings.FallDelay,
        };
        tube.SetSpeciesState(state);
    }

    /// <summary>Runs the actions of the physical tube enemies.</summary>
    internal void RunMotherBrainFallingTubeMain(RoomEnemySlot tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        switch (tubeState.Action ?? throw new InvalidDataException("A falling tube has no function."))
        {
            case MotherBrainTubeAction.NonMainTube:
                FallMotherBrainSideTube(tube);
                return;
            case MotherBrainTubeAction.MainTube_WaitingToFall:
                // The main tube's fall delay expires when it passes zero.
                if (--tubeState.FallDelay >= 0)
                    return;
                tubeState.Action = MotherBrainTubeAction.MainTube_Falling;
                FallMotherBrainMainTube(tube);
                return;
            case MotherBrainTubeAction.MainTube_Falling:
                FallMotherBrainMainTube(tube);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain falling tube has no behaviour for action {tubeState.Action}.");
        }
    }

    /// <summary>Drops a side tube until it reaches the floor and explodes, trailing smoke as it falls.</summary>
    private void FallMotherBrainSideTube(RoomEnemySlot tube)
    {
        MotherBrainFallingTubeState tubeState = AccelerateMotherBrainTube(tube);
        if (tube.Position.Y.Whole >= tubeState.FloorY)
            ExplodeMotherBrainTube(tube);
        else
            SpawnMotherBrainTubeSmoke(tube);
    }

    /// <summary>Accelerates a falling tube by six sub-pixels per update and moves it down by its fall speed.</summary>
    private static MotherBrainFallingTubeState AccelerateMotherBrainTube(RoomEnemySlot tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        // A tube falls to its floor and explodes there, so its speed stays a few pixels an update.
        tubeState.FallSpeed += SignedEightEight.FromTwoHundredFiftySixthsOfPixel(6);
        tube.Position = tube.Position.AdvanceY(tubeState.FallSpeed.ToSixteenSixteenDelta());
        return tubeState;
    }

    /// <summary>Drops the main tube, carrying Mother Brain's head with it and landing her body on the floor.</summary>
    private void FallMotherBrainMainTube(RoomEnemySlot tube)
    {
        AccelerateMotherBrainTube(tube);

        // Hiding the tube and moving the brain are independent branches.
        // The brain follows the tube throughout its descent, including before hiding.
        if (tube.Position.Y.Whole >= 244)
            tube.Presence = tube.Presence.With(EnemyPresence.Hidden);
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidOperationException("A falling main tube has no Mother Brain encounter state.");
        state.Head.Position = state.Head.Position.WithWrappedWholeY(tube.Position.Y.Whole - 56);
        if (state.Head.Position.Y.Whole < 196)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        StopMotherBrainRoomFlash(state);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundBackgroundAndEnemies), 32);
        // The fake death ends the first four per-line display effects, removing the room's
        // liquid scroll effects (and any X-ray cone) for the rest of the encounter.
        _frameResults.RemoveLiquidEffects = true;
        state.Head.Position = state.Head.Position.WithWholeY(196);
        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));

        // The main tube's landing initializes the neck, not phase-two setup. These
        // lengths and starting angles must exist before the ascent gets installed several
        // frames later or the first visible neck frame collapses every joint onto zero.
        MotherBrainNeckJointDefinitions.InitialDistances.CopyTo(state.NeckJointDistances);
        state.LowerNeckAngle = RotationAngle.FromSteps(72);
        state.UpperNeckAngle = RotationAngle.FromSteps(80);
        state.NeckAngleDelta = 256;
        state.Phase = MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2;
        ExplodeMotherBrainTube(tube);
    }

    /// <summary>Emits smoke at the falling tube's current position.</summary>
    private void SpawnMotherBrainTubeSmoke(RoomEnemySlot tube)
    {
        // The smoke cadence counts down and emits when it passes zero.
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        if (--tubeState.SmokeDelay >= 0)
            return;

        tubeState.SmokeDelay = 8;
        tubeState.SmokePositionIndex = (tubeState.SmokePositionIndex + 1) % MotherBrainBehavior.MotherBrainFallingTubeSmokeXOffsets.Length;
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(CoordinateAxis.Wrap(tube.Position.X.Whole + MotherBrainBehavior.MotherBrainFallingTubeSmokeXOffsets[tubeState.SmokePositionIndex]), 208),
            MiscDustAnimation.SmallDustCloud);
    }

    /// <summary>Finishes a falling tube with its explosion and tile update.</summary>
    private void ExplodeMotherBrainTube(RoomEnemySlot tube)
    {
        Position16<RoomPixels> position = tube.Position.Whole;
        tube.Presence = tube.Presence.With(EnemyPresence.Deleted);
        _projectiles.SpawnRoomGraphicsDustExplosion(position, MiscDustAnimation.SmallExplosion);
        if (_motherBrainEncounter.MotherBrain is not null)
            _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);
    }

    /// <summary>Queues the background room rows revealed by the tube collapse.</summary>
    internal void RequestMotherBrainRoomRows(
        MotherBrainEnemyState state,
        MotherBrainRoomMutation firstRowMutation,
        MotherBrainRoomMutation secondRowMutation,
        MotherBrainBodyPhase next)
    {
        int firstRow = state.Phase switch
        {
            MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows1And2 => 2,
            MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows3And4 => 4,
            MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows5And6 => 6,
            MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows7And8 => 8,
            MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows9And10 => 10,
            MotherBrainBodyPhase.FakeDeathAscentDrawMuralRows11And12 => 12,
            MotherBrainBodyPhase.FirstPhase or
                MotherBrainBodyPhase.FakeDeathDescentInitialPause or
                MotherBrainBodyPhase.FakeDeathDescentPauseBeforeLock or
                MotherBrainBodyPhase.FakeDeathDescentPauseBeforeMusic or
                MotherBrainBodyPhase.FakeDeathDescentPauseBeforeUnlock or
                MotherBrainBodyPhase.FakeDeathDescentPauseBeforeFlash or
                MotherBrainBodyPhase.FakeDeathDescentFadeToGray or
                MotherBrainBodyPhase.FakeDeathDescentCollapseTubes or
                MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Graphics or
                MotherBrainBodyPhase.FakeDeathAscentSetupPhase2Brain or
                MotherBrainBodyPhase.FakeDeathAscentPauseForSuspense or
                MotherBrainBodyPhase.FakeDeathAscentPrepareForRising or
                MotherBrainBodyPhase.FakeDeathAscentReadyStandingBody or
                MotherBrainBodyPhase.FakeDeathAscentContinuePausing or
                MotherBrainBodyPhase.FakeDeathAscentStartMusicAndEarthquake or
                MotherBrainBodyPhase.FakeDeathAscentRaiseMotherBrain or
                MotherBrainBodyPhase.FakeDeathAscentWaitUntilUncrouched or
                MotherBrainBodyPhase.FakeDeathAscentTransitionFromGray or
                MotherBrainBodyPhase.SecondPhaseStretchingShakeHead or
                MotherBrainBodyPhase.SecondPhaseStretchingBringHeadUp or
                MotherBrainBodyPhase.SecondPhaseStretchingFinish or
                MotherBrainBodyPhase.SecondPhaseThinking or
                MotherBrainBodyPhase.SecondPhaseTryAttack or
                MotherBrainBodyPhase.SecondPhaseBombDecideWalking or
                MotherBrainBodyPhase.SecondPhaseBombWalkingBackwards or
                MotherBrainBodyPhase.SecondPhaseBombCrouch or
                MotherBrainBodyPhase.SecondPhaseBombFired or
                MotherBrainBodyPhase.SecondPhaseBombStandUp or
                MotherBrainBodyPhase.SecondPhaseLaserPositionHeadQuickly or
                MotherBrainBodyPhase.SecondPhaseLaserPositionHeadSlowlyAndFire or
                MotherBrainBodyPhase.SecondPhaseLaserFinishAttack or
                MotherBrainBodyPhase.SecondPhaseHandBeam or
                MotherBrainBodyPhase.SecondPhaseRainbowExtendNeck or
                MotherBrainBodyPhase.SecondPhaseRainbowStartCharging or
                MotherBrainBodyPhase.SecondPhaseRainbowRetractNeck or
                MotherBrainBodyPhase.SecondPhaseRainbowWaitForCharge or
                MotherBrainBodyPhase.SecondPhaseRainbowExtendNeckDown or
                MotherBrainBodyPhase.SecondPhaseRainbowStartFiring or
                MotherBrainBodyPhase.SecondPhaseRainbowMoveSamusTowardWall or
                MotherBrainBodyPhase.SecondPhaseRainbowOneFrameDelay or
                MotherBrainBodyPhase.SecondPhaseRainbowStartDrainingSamus or
                MotherBrainBodyPhase.SecondPhaseRainbowDrainingSamus or
                MotherBrainBodyPhase.SecondPhaseRainbowFinishFiring or
                MotherBrainBodyPhase.SecondPhaseRainbowLetSamusFall or
                MotherBrainBodyPhase.SecondPhaseRainbowWaitForSamusToLand or
                MotherBrainBodyPhase.SecondPhaseRainbowLowerHead or
                MotherBrainBodyPhase.SecondPhaseRainbowDecideNextAction or
                MotherBrainBodyPhase.SecondPhaseFinishSamusOff or
                MotherBrainBodyPhase.SecondPhaseFinishSamusOffStandUp or
                MotherBrainBodyPhase.SecondPhaseFinishSamusOffAdmire or
                MotherBrainBodyPhase.SecondPhaseFinishSamusOffChargeFinalBeam or
                MotherBrainBodyPhase.SecondPhaseFinishSamusOffReadyBaby or
                MotherBrainBodyPhase.SecondPhaseFinishSamusOffFireFinalBeam or
                MotherBrainBodyPhase.SecondPhaseFinalRainbowBeamHolding or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyTakenAback or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyRegainBalance or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyFiringRainbowBeam or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyRainbowBeamRunOut or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyMoveToBackOfRoom or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyGoIntoLowPowerMode or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyPrepareTransitionToGrey or
                MotherBrainBodyPhase.SecondPhaseDrainedByBabyTransitionToGrey or
                MotherBrainBodyPhase.SecondPhaseReviveInanimateGrey or
                MotherBrainBodyPhase.SecondPhaseReviveShowSignsOfLife or
                MotherBrainBodyPhase.SecondPhaseReviveTransitionFromGrey or
                MotherBrainBodyPhase.SecondPhaseReviveWakeUp or
                MotherBrainBodyPhase.SecondPhaseReviveWakeUpStretch or
                MotherBrainBodyPhase.SecondPhaseReviveWalkUpToBaby or
                MotherBrainBodyPhase.SecondPhaseRevivePrepareNeckForBabyDeath or
                MotherBrainBodyPhase.SecondPhaseReviveFinishPreparingForBabyDeath or
                MotherBrainBodyPhase.SecondPhaseMurderBabyAttack or
                MotherBrainBodyPhase.SecondPhaseMurderBabyAttackCooldown or
                MotherBrainBodyPhase.SecondPhasePrepareForFinalBabyAttack or
                MotherBrainBodyPhase.SecondPhaseExecuteFinalBabyAttack or
                MotherBrainBodyPhase.SecondPhaseFinalBabyAttackHolding or
                MotherBrainBodyPhase.ThirdPhaseRecoverMakeSomeDistance or
                MotherBrainBodyPhase.ThirdPhaseRecoverSetupForFighting or
                MotherBrainBodyPhase.ThirdPhaseFightingMain or
                MotherBrainBodyPhase.ThirdPhaseFightingAttackCooldown or
                MotherBrainBodyPhase.ThirdPhaseDeathMoveToBackOfRoom or
                MotherBrainBodyPhase.ThirdPhaseDeathIdleWhilstExploding or
                MotherBrainBodyPhase.ThirdPhaseDeathStumbleToMiddleOfRoom or
                MotherBrainBodyPhase.ThirdPhaseDeathDisableBrainEffects or
                MotherBrainBodyPhase.ThirdPhaseDeathSetupBodyFadeOut or
                MotherBrainBodyPhase.ThirdPhaseDeathFadeOutBody or
                MotherBrainBodyPhase.ThirdPhaseDeathFinalFewExplosions or
                MotherBrainBodyPhase.ThirdPhaseDeathRealizeDecapitation or
                MotherBrainBodyPhase.ThirdPhaseDeathBrainFallsToGround or
                MotherBrainBodyPhase.ThirdPhaseDeathReadyCorpse or
                MotherBrainBodyPhase.ThirdPhaseDeathSetupFadeToGrey or
                MotherBrainBodyPhase.ThirdPhaseDeathFadeToGrey or
                MotherBrainBodyPhase.ThirdPhaseDeathCorpseTipsOver or
                MotherBrainBodyPhase.ThirdPhaseDeathCorpseRotsAway or
                MotherBrainBodyPhase.ThirdPhaseDeath20FrameDelay or
                MotherBrainBodyPhase.ThirdPhaseDeathReadyEscapeWarning or
                MotherBrainBodyPhase.ThirdPhaseDeathStartEscape or
                MotherBrainBodyPhase.ThirdPhaseDeathSpawnTimeBombSetSubtitle or
                MotherBrainBodyPhase.ThirdPhaseDeathTypeOutZebesEscapeText or
                MotherBrainBodyPhase.ThirdPhaseDeathDoorExplodingStartTimer or
                MotherBrainBodyPhase.ThirdPhaseDeathBlowUpEscapeDoor or
                MotherBrainBodyPhase.ThirdPhaseDeathKeepEarthquakeGoing => throw new InvalidOperationException("Mother Brain row draw called outside fake ascent."),
            _ => throw new InvalidOperationException("Mother Brain row draw called outside fake ascent."),
        };
        _access.RequestMotherBrainObject(new(2, CoordinateAxis.Wrap(firstRow)), firstRowMutation);
        _access.RequestMotherBrainObject(new(2, CoordinateAxis.Wrap(firstRow + 1)), secondRowMutation);
        state.Phase = next;
    }

    /// <summary>Restores the combat palette after the gray fake-death phase.</summary>
    internal static void TransitionMotherBrainFromGray(MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.PhaseTimer = 4;
        int paletteStep = state.GrayTransitionCounter++;
        if (paletteStep < MotherBrainFakeDeathPaletteDefinitions.FrameCount)
        {
            // Fake-death restoration changes only brain sprite colors one through three.
            state.Colours.ShowFakeDeathFromGrey(paletteStep);
            return;
        }

        state.Form = MotherBrainForm.SecondPhase;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        state.NeckAngleDelta = 1280;
        state.Phase = MotherBrainBodyPhase.SecondPhaseStretchingShakeHead;
        state.PhaseTimer = 23;
    }
}
