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
    /// <summary>Spawns the enemies the fake death releases into the room.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    private readonly MotherBrainEncounterAccess _access;
    /// <summary>Mother Brain's projectiles, whose ceiling tubes the collapse drops.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainFakeDeath(MotherBrainFakeDeathDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;

        _access = dependencies.Access;
        _projectileAttacks = dependencies.ProjectileAttacks;
    }

    /// <summary>
    /// Collision radii and landing floor selected by piece role. The two
    /// outer and two inner pieces share their values; the main tube has its own floor.
    /// </summary>
    internal static MotherBrainFallingTubeDefinition MotherBrainFallingTubeBody(MotherBrainTubePiece piece) =>
        MotherBrainFallingTubePopulationDefinitions.Bodies[piece];

    /// <summary>Steps Mother Brain's fake-death gray fade and advances when its colour cycle completes.</summary>
    internal void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        if (state.PhaseTimer.StepRunsOut())
        {
            state.PhaseTimer.Count = 8;
            int step = state.FakeDeathGreying.TakeStep();
            if (step == MotherBrainFakeDeathFadeDefinitions.FrameCount)
            {
                state.DescentPhase = MotherBrainDescentPhase.CollapseTubes;
            }
            else
            {
                // Fake-death fade greys her brain's three colours only. Copying a convenient
                // whole colour set would visibly alter unrelated room art and is not what
                // the fake-death fade does.
                state.Colours.ShowFakeDeathToGrey(step);
            }
        }

        // Every update of the grey fade also runs the tube collapse and the explosion, so tube sequencing and dust
        // begin during the grey fade rather than waiting for the eighth fade step.
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
        if (state.FakeDeathExplosionTimer.StepKeepsRunning())
            return;

        state.FakeDeathExplosionTimer.Count = 8;
        // The index steps backward through the eight positions, from the first back to the last.
        state.FakeDeathExplosionIndex = (state.FakeDeathExplosionIndex + MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions.Length - 1) %
            MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions.Length;
        Position16<RoomPixels> explosion = MotherBrainBehavior.MotherBrainFakeDeathExplosionPositions[state.FakeDeathExplosionIndex];

        // The tube dust reads the current random number and never draws a new one. Drawing a new
        // number here would desynchronize every later Rinka and phase-two attack decision.
        MiscDustAnimation animation = MotherBrainRandomDefinitions.FakeDeathDustSmokes.IsTakenBy(_room.Random.Read())
            ? MiscDustAnimation.Smoke
            : MiscDustAnimation.SmallExplosion;
        _projectiles.SpawnDustOrExplosion(explosion, animation);
        _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);
    }

    /// <summary>Schedules glass and tube collapse phases with their authored waits.</summary>
    /// <remarks>Every spawn after the first waits until its timer runs out, so each waits out the
    /// 32 (or 2) count its preceding clear step set.</remarks>
    internal void RunMotherBrainTubeCollapse(MotherBrainEnemyState state)
    {
        switch (state.TubeCollapseStep)
        {
            case MotherBrainTubeCollapseStep.WaitForFourFreeProjectileSlots:
                if (_projectiles.EnemyProjectiles.Count(projectile => !projectile.IsActive) < 4)
                    return;
                SpawnMotherBrainFallingTube(state, MotherBrainTubePiece.BottomLeft);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomLeftTube;
                return;

            case MotherBrainTubeCollapseStep.ClearBottomLeftTube:
                RequestMotherBrainTubeObject(state, new(5, 9),
                    MotherBrainRoomMutation.ClearBottomLeftTube,
                    MotherBrainTubeCollapseStep.SpawnTopRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopRightTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, MotherBrainTopTubeSegment.TopRight, new(152, 47));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn9;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn9:
                RequestMotherBrainTubeObject(state, new(9, 2),
                    MotherBrainRoomMutation.ClearCeilingBlock,
                    MotherBrainTubeCollapseStep.SpawnTopLeftTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopLeftTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopLeftTube, MotherBrainTopTubeSegment.TopLeft, new(104, 47));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn6;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn6:
                RequestMotherBrainTubeObject(state, new(6, 2),
                    MotherBrainRoomMutation.ClearCeilingBlock,
                    MotherBrainTubeCollapseStep.SpawnBottomRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnBottomRightTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainFallingTube(state, MotherBrainTubePiece.BottomRight);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomRightTube;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomRightTube:
                RequestMotherBrainTubeObject(state, new(10, 9),
                    MotherBrainRoomMutation.ClearBottomRightTube,
                    MotherBrainTubeCollapseStep.SpawnBottomMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnBottomMiddleLeftTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainFallingTube(state, MotherBrainTubePiece.BottomMiddleLeft);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomMiddleLeftTube;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomMiddleLeftTube:
                RequestMotherBrainTubeObject(state, new(6, 10),
                    MotherBrainRoomMutation.ClearBottomMiddleSideTube,
                    MotherBrainTubeCollapseStep.SpawnTopMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopMiddleLeftTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube, MotherBrainTopTubeSegment.TopMiddleLeft, new(120, 59));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn7;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn7:
                RequestMotherBrainTubeObject(state, new(7, 2),
                    MotherBrainRoomMutation.ClearCeilingTube,
                    MotherBrainTubeCollapseStep.SpawnTopMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnTopMiddleRightTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube, MotherBrainTopTubeSegment.TopMiddleRight, new(136, 59));
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearCeilingColumn8;
                return;
            case MotherBrainTubeCollapseStep.ClearCeilingColumn8:
                RequestMotherBrainTubeObject(state, new(8, 2),
                    MotherBrainRoomMutation.ClearCeilingTube,
                    MotherBrainTubeCollapseStep.SpawnBottomMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseStep.SpawnBottomMiddleRightTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainFallingTube(state, MotherBrainTubePiece.BottomMiddleRight);
                state.TubeCollapseStep = MotherBrainTubeCollapseStep.ClearBottomMiddleRightTube;
                return;
            case MotherBrainTubeCollapseStep.ClearBottomMiddleRightTube:
                RequestMotherBrainTubeObject(state, new(9, 10),
                    MotherBrainRoomMutation.ClearBottomMiddleSideTube,
                    MotherBrainTubeCollapseStep.SpawnMainTube, 2);
                return;
            case MotherBrainTubeCollapseStep.SpawnMainTube:
                if (!state.TubeCollapseTimer.StepRunsOut())
                    return;
                SpawnMotherBrainFallingTube(state, MotherBrainTubePiece.Main);
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

    /// <summary>Queues the room block change at the tube's location.</summary>
    private void RequestMotherBrainTubeObject(
        MotherBrainEnemyState state,
        Position16<RoomBlocks> block,
        MotherBrainRoomMutation mutation,
        MotherBrainTubeCollapseStep next,
        int timer)
    {
        _access.RequestMotherBrainObject(block, mutation);
        state.TubeCollapseStep = next;
        state.TubeCollapseTimer.Count = timer;
    }

    /// <summary>Spawns the top tube projectile of <paramref name="kind"/>, falling as <paramref name="tube"/> shows, for the scripted collapse.</summary>
    internal void SpawnMotherBrainTopTube(
        RoomEnemyProjectileKind kind,
        MotherBrainTopTubeSegment tube,
        Position16<RoomPixels> position)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, _projectileAttacks.Projectile,
            graphics: SpriteGraphics.SharedIn(MotherBrainColours.GlassAndTubes));
        projectile.StartAnimation(new MotherBrainTopTubeAnimation(tube));
        projectile.Position = projectile.Position.WithWhole(position);
        projectile.Velocity = EightEightVector<RoomPixels>.Zero;
        projectile.State = new MotherBrainTopTubeState();
    }

    /// <summary>Creates the falling tube enemy of <paramref name="piece"/>.</summary>
    internal void SpawnMotherBrainFallingTube(MotherBrainEnemyState state, MotherBrainTubePiece piece)
    {
        // The bound source supplies the fixed cutscene placement; retail rooms resolve it from the loaded definitions.
        RoomEnemyPlacement population = _room.Definitions.ReadFallingTubePopulation(piece);
        if (population.DefinitionId != EnemyDefinitionId.MotherBrainTubes)
        {
            throw new InvalidDataException(
                $"Mother Brain tube population entry for {piece} names enemy " +
                $"{population.DefinitionId}, not the falling tube.");
        }

        // A spawn quietly does nothing when all 32 enemy places are occupied.
        // Each tube holds the encounter that spawned it from its setup (#2218).
        _ = _lifecycle.SpawnEnemyJoining(population, state);
    }

    /// <summary>Initializes a falling tube.</summary>
    internal static void InitializeMotherBrainFallingTube(RoomEnemyPlace tube)
    {
        MotherBrainTubeSpawnSettings settings = tube.Spawn.Population.SpawnSettingsOf<MotherBrainTubeSpawnSettings>();
        MotherBrainTubePiece piece = settings.Piece;
        MotherBrainFallingTubeDefinition tubeBody = MotherBrainFallingTubeBody(piece);
        tube.Radius = tubeBody.Radius;
        var state = new MotherBrainFallingTubeState(piece == MotherBrainTubePiece.Main
            ? MotherBrainTubeAction.WaitingToFall
            : MotherBrainTubeAction.NonMainTube)
        {
            Playback = MotherBrainTubeAnimations.Starting(piece),
            FloorY = tubeBody.Floor,
            FallDelay = { Count = settings.FallDelay },
        };
        tube.SetSpeciesState(state);
    }

    /// <summary>Runs the actions of the physical tube enemies.</summary>
    internal void RunMotherBrainFallingTubeMain(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        switch (tubeState.Action)
        {
            case MotherBrainTubeAction.NonMainTube:
                FallMotherBrainSideTube(tube);
                return;
            case MotherBrainTubeAction.WaitingToFall:
                // The main tube falls as its fall delay runs out.
                if (tubeState.FallDelay.StepKeepsRunning())
                    return;
                tubeState.Action = MotherBrainTubeAction.Falling;
                FallMotherBrainMainTube(tube);
                return;
            case MotherBrainTubeAction.Falling:
                FallMotherBrainMainTube(tube);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain falling tube has no behaviour for action {tubeState.Action}.");
        }
    }

    /// <summary>Drops a side tube until it reaches the floor and explodes, trailing smoke as it falls.</summary>
    private void FallMotherBrainSideTube(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = AccelerateMotherBrainTube(tube);
        if (tube.Position.Y.Whole >= tubeState.FloorY)
            ExplodeMotherBrainTube(tube);
        else
            SpawnMotherBrainTubeSmoke(tube);
    }

    /// <summary>Accelerates a falling tube by six sub-pixels per update and moves it down by its fall speed.</summary>
    private static MotherBrainFallingTubeState AccelerateMotherBrainTube(RoomEnemyPlace tube)
    {
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        // A tube falls to its floor and explodes there, so its speed stays a few pixels an update.
        tubeState.FallSpeed += SignedEightEight.FromTwoHundredFiftySixthsOfPixel(6);
        tube.Position = tube.Position.AdvanceY(tubeState.FallSpeed.ToSixteenSixteenDelta());
        return tubeState;
    }

    /// <summary>Drops the main tube, carrying Mother Brain's head with it and landing her body on the floor.</summary>
    private void FallMotherBrainMainTube(RoomEnemyPlace tube)
    {
        AccelerateMotherBrainTube(tube);

        // Hiding the tube and moving the brain are independent branches.
        // The brain follows the tube throughout its descent, including before hiding.
        if (tube.Position.Y.Whole >= 244)
            tube.Presence = tube.Presence.With(EnemyPresence.Hidden);
        // The tube holds the encounter that spawned it (#2218).
        MotherBrainEnemyState state = tube.RequireSpeciesState<MotherBrainEnemyState>();
        state.Head.Position = state.Head.Position.WithWrappedWholeY(tube.Position.Y.Whole - 56);
        if (state.Head.Position.Y.Whole < 196)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        StopMotherBrainRoomFlash(state);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundBackgroundAndEnemies), 32);
        // The fake death removes the room's liquid effects, and any X-ray cone, for the rest of the encounter.
        _frameResults.RemoveLiquidEffects = true;
        state.Head.Position = state.Head.Position.WithWholeY(196);
        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));

        // The main tube's landing initializes the neck, not phase-two setup. These
        // lengths and starting angles must exist before the ascent gets installed several
        // frames later or the first visible neck frame collapses every joint onto zero.
        MotherBrainNeckJointDefinitions.InitialDistances.CopyTo(state.NeckJointDistances);
        state.LowerNeckAngle = RotationAngle.FromSteps(72);
        state.UpperNeckAngle = RotationAngle.FromSteps(80);
        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Steady;
        state.AscentPhase = MotherBrainAscentPhase.DrawMural;
        state.MuralRowsDrawn = 0;
        ExplodeMotherBrainTube(tube);
    }

    /// <summary>Emits smoke at the falling tube's current position.</summary>
    private void SpawnMotherBrainTubeSmoke(RoomEnemyPlace tube)
    {
        // The smoke cadence counts down and emits as it runs out.
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        if (tubeState.SmokeDelay.StepKeepsRunning())
            return;

        tubeState.SmokeDelay.Count = 8;
        tubeState.SmokePositionIndex = (tubeState.SmokePositionIndex + 1) % MotherBrainBehavior.MotherBrainFallingTubeSmokeXOffsets.Length;
        _projectiles.SpawnDustOrExplosion(
            new(CoordinateAxis.Wrap(tube.Position.X.Whole + MotherBrainBehavior.MotherBrainFallingTubeSmokeXOffsets[tubeState.SmokePositionIndex]), 208),
            MiscDustAnimation.SmallDustCloud);
    }

    /// <summary>Finishes a falling tube with its explosion and block update.</summary>
    private void ExplodeMotherBrainTube(RoomEnemyPlace tube)
    {
        Position16<RoomPixels> position = tube.Position.Whole;
        tube.Presence = tube.Presence.With(EnemyPresence.Deleted);
        _projectiles.SpawnDustOrExplosion(position, MiscDustAnimation.SmallExplosion);
        _motherBrainEncounter.RequestBodySound(EnemySound.SmallExplosion);
    }

    /// <summary>
    /// Draws the next rows of the mural the tube collapse revealed, two an update from the top (#2211), and hands off to
    /// the phase-two graphics setup once every row is drawn.
    /// </summary>
    internal void DrawNextMuralRows(MotherBrainEnemyState state)
    {
        int drawn = state.MuralRowsDrawn;
        for (int row = drawn; row < drawn + MotherBrainMuralDefinitions.RowsEachUpdate; row++)
        {
            _access.RequestMotherBrainObject(
                new(MotherBrainMuralDefinitions.Column, CoordinateAxis.Wrap(MotherBrainMuralDefinitions.TopBlockRow + row)),
                MotherBrainMuralDefinitions.Rows[row]);
        }
        state.MuralRowsDrawn = drawn + MotherBrainMuralDefinitions.RowsEachUpdate;
        if (state.MuralRowsDrawn == MotherBrainMuralDefinitions.Rows.Count)
            state.AscentPhase = MotherBrainAscentPhase.SetupPhase2Graphics;
    }

    /// <summary>Restores the combat colours after the gray fake-death phase.</summary>
    internal static void TransitionMotherBrainFromGray(MotherBrainEnemyState state)
    {
        if (!state.PhaseTimer.StepRunsOut())
            return;

        state.PhaseTimer.Count = 4;
        int fadeStep = state.FromFakeDeathGrey.TakeStep();
        if (fadeStep < MotherBrainFakeDeathFadeDefinitions.FrameCount)
        {
            // The restoration from grey recolours only three of the brain's colours.
            state.Colours.ShowFakeDeathFromGrey(fadeStep);
            return;
        }

        state.Form = MotherBrainForm.SecondPhase;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovement = MotherBrainNeckMovement.Lower;
        state.UpperNeckMovement = MotherBrainNeckMovement.Lower;
        state.NeckTurnPerUpdate = MotherBrainNeckTurnDefinitions.Fast;
        state.Phase2FightPhase = MotherBrainPhase2FightPhase.StretchingShakeHead;
        state.PhaseTimer.Count = 23;
    }
}
